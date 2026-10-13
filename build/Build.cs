using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Fallout.Common;
using Fallout.Common.CI;
using Fallout.Common.CI.GitHubActions;
using Fallout.Common.IO;
using Fallout.Common.Tools.DotNet;
using Serilog;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

/// <summary>
/// Build of Revo - run it with build.sh or build.ps1, the same way locally and in CI. Without any targets, it tests and
/// packs the solution.
/// </summary>
/// <remarks>
/// The package version is the VersionPrefix from Common.props with a suffix depending on what is being built:
/// <list type="bullet">
/// <item>release tag Revo-X.Y.Z[-suffix] -> X.Y.Z[-suffix] (X.Y.Z must match the VersionPrefix)</item>
/// <item>main or release/* branch -> X.Y.Z-ci.N</item>
/// <item>pull request -> X.Y.Z-prN-commit</item>
/// <item>other branch -> X.Y.Z-branch-commit</item>
/// <item>local build -> X.Y.Z-localYYMMDD-HHMMSS</item>
/// </list>
/// </remarks>
[ShutdownDotNetAfterServerBuild]
class Build : FalloutBuild
{
    const string ReleaseTagPrefix = "Revo-";

    public static int Main() => Execute<Build>(x => x.Test, x => x.Pack);

    [Parameter("Configuration to build - default is 'Release'")]
    readonly Configuration Configuration = Configuration.Release;

    [Parameter("Package version - computed from Common.props and the build context by default")]
    readonly string BuildVersion;

    [Parameter("Filter of the tests to run (see dotnet test --filter)")]
    readonly string TestFilter;

    [Parameter("NuGet feed to push the packages to")]
    readonly string NuGetSource;

    [Parameter("API key for pushing the packages"), Secret]
    readonly string NuGetApiKey;

    string version;

    AbsolutePath SolutionFile => RootDirectory / "Revo.slnx";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath PackagesDirectory => ArtifactsDirectory / "packages";
    AbsolutePath TestResultsDirectory => ArtifactsDirectory / "test-results";
    AbsolutePath ReleaseNotesFile => ArtifactsDirectory / "release-notes.md";

    string VersionPrefix => XDocument.Load(RootDirectory / "Common.props")
        .Descendants("VersionPrefix")
        .Single().Value.Trim();

    string Version => version ??= GetVersion();

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(SolutionFile));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            Log.Information("Building Revo {Version} ({Configuration})", Version, (string) Configuration);

            DotNetBuild(s => s
                .SetProjectFile(SolutionFile)
                .SetConfiguration(Configuration)
                .SetVersion(Version)
                .SetContinuousIntegrationBuild(IsServerBuild)
                // keeps the actual source paths in the PDBs for the code coverage (dotnet/sourcelink#91)
                .SetProperty("DeterministicSourcePaths", false)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            TestResultsDirectory.CreateOrCleanDirectory();

            DotNetTest(s => s
                .SetProjectFile(SolutionFile)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetFilter(TestFilter)
                .SetDataCollector("XPlat Code Coverage")
                .SetSettingsFile(RootDirectory / "coverlet.runsettings")
                .SetLoggers("trx")
                .SetResultsDirectory(TestResultsDirectory));
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .After(Test)
        .Executes(() =>
        {
            PackagesDirectory.CreateOrCleanDirectory();

            DotNetPack(s => s
                .SetProject(SolutionFile)
                .SetConfiguration(Configuration)
                .SetVersion(Version)
                .EnableNoBuild()
                .SetOutputDirectory(PackagesDirectory));
        });

    /// <summary>
    /// Pushes the packages already in artifacts/packages - together with Pack, it pushes the packages it has just built.
    /// </summary>
    Target Push => _ => _
        .After(Pack)
        .Requires(() => NuGetSource)
        .Requires(() => NuGetApiKey)
        .Executes(() =>
        {
            Assert.NotEmpty(PackagesDirectory.GlobFiles("*.nupkg"), $"There are no packages to push in {PackagesDirectory}");

            DotNetNuGetPush(s => s
                .SetTargetPath(PackagesDirectory / "*.nupkg")
                .SetSource(NuGetSource)
                .SetApiKey(NuGetApiKey)
                .EnableSkipDuplicate());
        });

    /// <summary>
    /// Extracts the notes of the version being built from RELEASE_NOTES.md (for its GitHub Release).
    /// </summary>
    Target ReleaseNotes => _ => _
        .Executes(() =>
        {
            var notes = (RootDirectory / "RELEASE_NOTES.md").ReadAllLines()
                .SkipWhile(x => !x.StartsWith($"## [{VersionPrefix}]"))
                .Skip(1)
                .TakeWhile(x => !x.StartsWith("## "))
                .Select(x => Regex.Replace(x, @"^-(?=\S)", "- ")) // "-fixed ..." -> list item
                .ToArray();

            var text = string.Join("\n", notes).Trim();
            Assert.True(text.Length > 0, $"There are no notes for {VersionPrefix} in RELEASE_NOTES.md (expected a '## [{VersionPrefix}] - date' section)");

            ReleaseNotesFile.WriteAllText(text + "\n");
            Log.Information("Release notes of {Version}:\n{Notes}", VersionPrefix, text);
        });

    string GetVersion()
    {
        if (!string.IsNullOrEmpty(BuildVersion))
        {
            return BuildVersion;
        }

        var gitHubActions = GitHubActions.Instance;
        if (gitHubActions == null)
        {
            return $"{VersionPrefix}-local{DateTime.Now:yyMMdd-HHmmss}";
        }

        if (gitHubActions.RefType == "tag")
        {
            var tag = gitHubActions.RefName;
            Assert.True(tag.StartsWith(ReleaseTagPrefix), $"{tag} is not a release tag ({ReleaseTagPrefix}X.Y.Z)");

            var tagVersion = tag[ReleaseTagPrefix.Length..];
            Assert.True(tagVersion.Split('-')[0] == VersionPrefix,
                $"Release tag {tag} does not match the VersionPrefix in Common.props ({VersionPrefix})");
            return tagVersion;
        }

        var commit = gitHubActions.Sha[..7];
        if (gitHubActions.IsPullRequest)
        {
            var pullRequestNumber = gitHubActions.Ref.Split('/')[2]; // refs/pull/N/merge
            return $"{VersionPrefix}-pr{pullRequestNumber}-{commit}";
        }

        var branch = gitHubActions.RefName;
        if (branch == "main" || branch.StartsWith("release/"))
        {
            return $"{VersionPrefix}-ci.{gitHubActions.RunNumber}";
        }

        return $"{VersionPrefix}-{Regex.Replace(branch.ToLowerInvariant(), "[^0-9a-z-]", "-")}-{commit}";
    }
}
