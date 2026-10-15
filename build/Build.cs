using System;
using System.Linq;
using System.Text.RegularExpressions;
using Fallout.Common;
using Fallout.Common.CI;
using Fallout.Common.CI.GitHubActions;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Serilog;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

/// <summary>
/// Build of Revo - run it with build.sh or build.ps1, the same way locally and in CI. Without any targets, it tests and
/// packs the solution.
/// </summary>
/// <remarks>
/// The versions are calculated by MinVer from the Revo-X.Y.Z tags (configured in Common.props): a tagged commit gets
/// the version of its tag, the commits after it X.Y.(Z+1)-alpha.0.N (N being the number of commits since the tag).
/// Local builds get X.Y.Z-localYYMMDD-HHMMSS instead, so that every one of them has a version of its own.
/// </remarks>
[ShutdownDotNetAfterServerBuild]
class Build : FalloutBuild
{
    const string ReleaseTagPrefix = "Revo-";

    public static int Main() => Execute<Build>(x => x.Test, x => x.Pack);

    [Parameter("Configuration to build - default is 'Release'")]
    readonly Configuration Configuration = Configuration.Release;

    [Parameter("Package version - calculated from the git tags by default")]
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

    string Version => version ??= GetVersion();

    /// <summary>
    /// The version without the pre-release part, e.g. 2.0.0 for 2.0.0-rc.1.
    /// </summary>
    string ReleaseVersion => Version.Split('-', '+')[0];

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
                .SetProperty("MinVerVersionOverride", Version)
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
                .SetProperty("MinVerVersionOverride", Version)
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
        .DependsOn(Restore)
        .Executes(() =>
        {
            var notes = (RootDirectory / "RELEASE_NOTES.md").ReadAllLines()
                .SkipWhile(x => !x.StartsWith($"## [{ReleaseVersion}]"))
                .Skip(1)
                .TakeWhile(x => !x.StartsWith("## "))
                .Select(x => Regex.Replace(x, @"^-(?=\S)", "- ")) // "-fixed ..." -> list item
                .ToArray();

            var text = string.Join("\n", notes).Trim();
            Assert.True(text.Length > 0, $"There are no notes for {ReleaseVersion} in RELEASE_NOTES.md (expected a '## [{ReleaseVersion}] - date' section)");

            ReleaseNotesFile.WriteAllText(text + "\n");
            Log.Information("Release notes of {Version}:\n{Notes}", ReleaseVersion, text);
        });

    string GetVersion()
    {
        if (!string.IsNullOrEmpty(BuildVersion))
        {
            return BuildVersion;
        }

        var minVerVersion = GetMinVerVersion();

        var gitHubActions = GitHubActions.Instance;
        if (gitHubActions == null)
        {
            return $"{minVerVersion.Split('-', '+')[0]}-local{DateTime.Now:yyMMdd-HHmmss}";
        }

        if (gitHubActions.RefType == "tag")
        {
            // e.g. with a shallow clone, MinVer would not see the tag
            var tag = gitHubActions.RefName;
            Assert.True(tag == ReleaseTagPrefix + minVerVersion,
                $"The version calculated from the tags ({minVerVersion}) does not match the release tag {tag}");
        }

        return minVerVersion;
    }

    /// <summary>
    /// Gets the version MinVer calculates for the current commit (requires the restored solution).
    /// </summary>
    string GetMinVerVersion()
    {
        var project = RootDirectory / "Revo.Core" / "Revo.Core.csproj";
        return DotNet($"msbuild {project} -nologo -t:MinVer -getProperty:MinVerVersion", logOutput: false)
            .StdToText()
            .Trim();
    }
}
