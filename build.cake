#r "System.Xml"

using System.Xml;

const string SolutionName = "Revo";

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

string NuGetApiKey = HasArgument("NuGetApiKey")
    ? Argument<string>("NuGetApiKey")
    : EnvironmentVariable("NUGET_API_KEY");

var SolutionDir = Context.Environment.WorkingDirectory.FullPath;
string SolutionFile => System.IO.Path.Combine(SolutionDir, SolutionName + ".sln");

var PackagesDir = System.IO.Path.Combine(SolutionDir, "build", "packages");
var ReportsDir = System.IO.Path.Combine(SolutionDir, "build", "reports");

bool IsCiBuild = AzurePipelines.IsRunningOnAzurePipelines || AzurePipelines.IsRunningOnAzurePipelinesHosted;

string Version = HasArgument("BuildVersion") ?
  Argument<string>("BuildVersion", null) :
  EnvironmentVariable("BUILD_VERSION");

int? BuildNumber =
  HasArgument("BuildNumber") ? (int?) Argument<int>("BuildNumber") :
  IsCiBuild ? AzurePipelines.Environment.Build.Id :
  EnvironmentVariable("BuildNumber") != null ? (int?) int.Parse(EnvironmentVariable("BuildNumber")) : null;

if (Version == null)
{
  var xmlDocument = new XmlDocument();
  xmlDocument.Load(System.IO.Path.Combine(SolutionDir, "Common.props"));

  string versionPrefix = ((XmlElement) xmlDocument.SelectSingleNode("Project/PropertyGroup/VersionPrefix")).InnerText;
  string versionSuffix = "";

  if (IsCiBuild)
  {
    var branchName = AzurePipelines.Environment.Repository.SourceBranchName;

    if (AzurePipelines.Environment.PullRequest.IsPullRequest)
    {
      versionSuffix += $"-pr{AzurePipelines.Environment.PullRequest.Id}";
      IsPushEnabled = false;
    }
    else if (branchName != "master")
    {
      versionSuffix += $"-{branchName.Replace('_', '-').ToLowerInvariant()}";

      if (branchName == "develop")
      {
        if (BuildNumber != null)
        {
          versionSuffix += $"{BuildNumber.Value:00000}";
        }
      }
      else
      {
        IsPushEnabled = false;

        var shortCommitHash = AzurePipelines.Environment.Repository.SourceVersion;
        shortCommitHash = shortCommitHash.Substring(0, 7);
        versionSuffix += $"-{shortCommitHash}";
      }
    }
  }
  else
  {
    versionSuffix += $"-local{DateTime.Now.ToString("yyMMdd")}";
  }

  Version = $"{versionPrefix}{versionSuffix}";
}

Information($"{SolutionName} cake build script start");
Information($"Build target: {Target}, version: {Version}, is CI build: {IsCiBuild}");

if (IsCiBuild)
{
  AzurePipelines.Commands.UpdateBuildNumber(Version);
}

Task("Default")
    .IsDependentOn("Pack");

Task("CI")
    .IsDependentOn("Push");

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

Task("Pack")
  .WithCriteria(IsPackEnabled)
  .IsDependentOn("Test")
  .Does(() =>
  {
    DotNetCorePack(SolutionFile,
      new DotNetCorePackSettings
      {
        ArgumentCustomization = args => args.Append($"/p:Version={Version}"),
        Configuration = Configuration,
        OutputDirectory = PackagesDir,
        NoBuild = true,
        NoRestore = true,
        IncludeSource = true,
        IncludeSymbols = true,
        Verbosity = DotNetCoreVerbosity.Minimal
      });
  });

Task("Push")
  .WithCriteria(IsPushEnabled)
  .IsDependentOn("Pack")
  .Does(() =>
  {
    if (string.IsNullOrWhiteSpace(NuGetApiKey))
    {
      throw new Exception("Error: NuGetApiKey is required to push NuGet packages");
    }

    DotNetCoreNuGetPush(System.IO.Path.Combine(PackagesDir, "*.nupkg"),
      new DotNetCoreNuGetPushSettings
      {
        ApiKey = NuGetApiKey,
        Interactive = !IsCiBuild,
        Source = "https://api.nuget.org/v3/index.json",
        Verbosity = DotNetCoreVerbosity.Minimal
      });
  });

RunTarget(Target);