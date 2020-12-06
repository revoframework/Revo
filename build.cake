#r "System.Xml"
#tool nuget:?package=Codecov&version=1.12.3
#addin nuget:?package=Cake.Codecov&version=0.9.1

using System.Xml;

const string SolutionName = "Revo";

const string RelaseNuGetSourceUrl = "https://api.nuget.org/v3/index.json";
const string PreRelaseNuGetSourceUrl = "https://nuget.pkg.github.com/revoframework/index.json";

readonly string SolutionDir = Context.Environment.WorkingDirectory.FullPath;
readonly string SolutionFile = System.IO.Path.Combine(SolutionDir, SolutionName + ".sln");

readonly string PackagesDir = System.IO.Path.Combine(SolutionDir, "build", "packages");
readonly string ReportsDir = System.IO.Path.Combine(SolutionDir, "build", "reports");
readonly string CoverageHtmlReportDir = System.IO.Path.Combine(ReportsDir, "coverage_html");

readonly bool IsAzurePipelines = AzurePipelines.IsRunningOnAzurePipelines || AzurePipelines.IsRunningOnAzurePipelinesHosted;
readonly bool IsCiBuild = IsAzurePipelines;

string Target = Argument<string>("Target", "Default");

bool IsCleanEnabled = Argument<bool>("DoClean", true);
bool IsRestoreEnabled = Argument<bool>("DoRestore", true);
bool IsBuildEnabled = Argument<bool>("DoBuild", true);
bool IsTestEnabled = Argument<bool>("DoTest", true);
bool IsPackEnabled = Argument<bool>("DoPack", true);
bool IsPushEnabled = Argument<bool>("DoPush", true);

string Configuration = HasArgument("Configuration")
    ? Argument<string>("Configuration")
    : EnvironmentVariable("Configuration") ?? "Release";

string ReleaseNuGetApiKey = HasArgument("ReleaseNuGetApiKey")
    ? Argument<string>("ReleaseNuGetApiKey")
    : EnvironmentVariable("RELEASE_NUGET_API_KEY");

string PreReleaseNuGetApiKey = HasArgument("PreReleaseNuGetApiKey")
    ? Argument<string>("PreReleaseNuGetApiKey")
    : EnvironmentVariable("PRE_RELEASE_NUGET_API_KEY");

string Version = HasArgument("BuildVersion") ?
  Argument<string>("BuildVersion", null) :
  EnvironmentVariable("BUILD_VERSION");

int? BuildNumber =
  HasArgument("BuildNumber") ? (int?) Argument<int>("BuildNumber") :
  IsAzurePipelines ? AzurePipelines.Environment.Build.Id :
  EnvironmentVariable("BuildNumber") != null ? (int?) int.Parse(EnvironmentVariable("BuildNumber")) : null;

bool IsReleaseBuild = false;

if (Version == null)
{
  var xmlDocument = new XmlDocument();
  xmlDocument.Load(System.IO.Path.Combine(SolutionDir, "Common.props"));

  string versionPrefix = ((XmlElement) xmlDocument.SelectSingleNode("Project/PropertyGroup/VersionPrefix")).InnerText;
  string versionSuffix = "";

  if (IsCiBuild)
  {
    var branchName = AzurePipelines.Environment.Repository.SourceBranchName;

    var shortCommitHash = AzurePipelines.Environment.Repository.SourceVersion;
    shortCommitHash = shortCommitHash.Substring(0, 7);

    if (AzurePipelines.Environment.PullRequest.IsPullRequest)
    {
      versionSuffix += $"-pr{AzurePipelines.Environment.PullRequest.Id}-{shortCommitHash}";
      IsPushEnabled = false;
    }
    else if (branchName == "master")
    {
      IsReleaseBuild = true;
    }
    else
    {
      versionSuffix += $"-{branchName.Replace('_', '-').ToLowerInvariant()}";

      if (branchName != "develop")
      {
        versionSuffix += $"-{shortCommitHash}";
        IsPushEnabled = false;
      }
      else if (BuildNumber != null)
      {
        versionSuffix += $"{BuildNumber.Value:00000}";
      }
    }
  }
  else
  {
    versionSuffix += $"-local{DateTime.Now.ToString("yyMMdd-HHmmss")}";
  }

  Version = $"{versionPrefix}{versionSuffix}";
}

Information($"{SolutionName} cake build script start");
Information($"Build target: {Target}, version: {Version}, is CI build: {IsCiBuild}, "
+ $"release NuGet API key: {(string.IsNullOrWhiteSpace(ReleaseNuGetApiKey) ? "null" : "****")}, "
+ $"pre-release NuGet API key: {(string.IsNullOrWhiteSpace(PreReleaseNuGetApiKey) ? "null" : "****")}");

if (IsCiBuild)
{
  AzurePipelines.Commands.UpdateBuildNumber(Version);
}

Task("Default")
    .IsDependentOn("Pack");

Task("CI")
    .IsDependentOn("PublishTestResults")
    .IsDependentOn("PublishCodeCoverage")
    .IsDependentOn("PushNuGet");

Task("Clean")
  .WithCriteria(IsCleanEnabled)
  .Does(() =>
  {
    CleanDirectories(new [] { PackagesDir, ReportsDir });

    DotNetCoreClean(SolutionFile,
      new DotNetCoreCleanSettings
      {
        Verbosity = DotNetCoreVerbosity.Minimal,
        Configuration = Configuration
      });
  });

Task("Restore")
  .WithCriteria(IsRestoreEnabled)
  .IsDependentOn("Clean")
  .Does(() =>
  {
    DotNetCoreRestore(SolutionFile,
      new DotNetCoreRestoreSettings
      {
        Verbosity = DotNetCoreVerbosity.Minimal,
        Interactive = !IsCiBuild
      });
  });

Task("Build")
  .WithCriteria(IsBuildEnabled)
  .IsDependentOn("Restore")
  .Does(() =>
  {
    DotNetCoreBuild(SolutionFile,
      new DotNetCoreBuildSettings
      {
        ArgumentCustomization = args => args
          .Append($"/p:Version={Version}")
          .Append("/p:ci=true")
          .AppendSwitch("/p:DebugType", "=", Configuration == "Release" ? "portable" : "full")
          .AppendSwitch("/p:ContinuousIntegrationBuild", "=", IsCiBuild ? "true" : "false")
          .AppendSwitch("/p:DeterministicSourcePaths", "=", "false"), // Temporary workaround for https://github.com/dotnet/sourcelink/issues/91,
        Configuration = Configuration,
        NoRestore = true,
        Verbosity = DotNetCoreVerbosity.Minimal
      });
  });

Task("Test")
  .WithCriteria(IsTestEnabled)
  .IsDependentOn("Build")
  .Does(() =>
  {
    DotNetCoreTest(SolutionFile,
      new DotNetCoreTestSettings
      {
        ArgumentCustomization = args => args
          .Append("--collect:\"XPlat Code Coverage\"")
          .Append("--settings coverlet.runsettings"),
        Configuration = Configuration,
        Logger = "trx",
        NoBuild = true,
        NoRestore = true,
        Verbosity = DotNetCoreVerbosity.Minimal,
        ResultsDirectory = ReportsDir
      });
  });

Task("PublishTestResults")
  .WithCriteria(IsAzurePipelines)
  .IsDependentOn("Test")
  .Does(() =>
  {
    var testResultFiles = GetFiles(System.IO.Path.Combine(ReportsDir, "**/*.trx")).ToArray();
    if (testResultFiles.Length > 0)
    {
      // there is a bug when publishing test result files that have brackets in their filename
      foreach (var testResultFile in testResultFiles)
      {
        if (testResultFile.GetFilename().ToString().Contains("[")
          || testResultFile.GetFilename().ToString().Contains("]"))
        {
          var fileName = testResultFile.GetFilename().ToString()
            .Replace("[", "_lb_")
            .Replace("]", "_rb_");
          var newPath = testResultFile.GetDirectory().CombineWithFilePath(fileName);
          MoveFile(testResultFile, newPath);
        }
      }

      testResultFiles = GetFiles(System.IO.Path.Combine(ReportsDir, "**/*.trx")).ToArray();
      AzurePipelines.Commands.PublishTestResults(
        new AzurePipelinesPublishTestResultsData()
        {
          Configuration = Configuration,
          TestResultsFiles = testResultFiles,
          TestRunner = AzurePipelinesTestRunnerType.VSTest
        });
    }
  });

Task("PublishCodeCoverage")
  .WithCriteria(IsAzurePipelines)
  .IsDependentOn("Test")
  .Does(() =>
  {
    var coverageFiles = GetFiles(System.IO.Path.Combine(ReportsDir, "*/coverage.cobertura.xml"));

    Codecov(
      new CodecovSettings
      {
        Files = coverageFiles.Select(x => x.ToString()),
        Build = Version
      });
  });

Task("Pack")
  .WithCriteria(IsPackEnabled)
  .IsDependentOn("Test")
  .Does(() =>
  {
    DotNetCorePack(SolutionFile,
      new DotNetCorePackSettings
      {
        ArgumentCustomization = args => args
          .Append($"/p:Version={Version}"),
        Configuration = Configuration,
        OutputDirectory = PackagesDir,
        NoBuild = true,
        NoRestore = true,
        Verbosity = DotNetCoreVerbosity.Minimal
      });
  });

Task("PushNuGet")
  .WithCriteria(IsPushEnabled)
  .IsDependentOn("Pack")
  .Does(() =>
  {
    if (string.IsNullOrWhiteSpace(PreReleaseNuGetApiKey))
    {
      throw new Exception("Error: PreReleaseNuGetApiKey is required to push pre-release NuGet packages");
    }

    DotNetCoreNuGetPush(System.IO.Path.Combine(PackagesDir, "*.nupkg"),
      new DotNetCoreNuGetPushSettings
      {
        ApiKey = PreReleaseNuGetApiKey,
        Interactive = !IsCiBuild,
        Source = PreRelaseNuGetSourceUrl
      });

    if (IsReleaseBuild)
    {
      if (string.IsNullOrWhiteSpace(RelaseNuGetSourceUrl))
      {
        throw new Exception("Error: RelaseNuGetSourceUrl is required to push release NuGet packages");
      }

      DotNetCoreNuGetPush(System.IO.Path.Combine(PackagesDir, "*.nupkg"),
        new DotNetCoreNuGetPushSettings
        {
          ApiKey = ReleaseNuGetApiKey,
          Interactive = !IsCiBuild,
          Source = RelaseNuGetSourceUrl
        });
    }
  });

RunTarget(Target);