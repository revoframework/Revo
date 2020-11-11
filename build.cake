#r "System.Xml"

using System.Xml;

const string SolutionName = "Revo";

const string ProductionNuGetSourceUrl = "https://api.nuget.org/v3/index.json";
const string DevelopNuGetSourceUrl = "https://www.myget.org/F/revoframework/api/v2/package";

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

string ProductionNuGetApiKey = HasArgument("ProductionNuGetApiKey")
    ? Argument<string>("ProductionNuGetApiKey")
    : EnvironmentVariable("PRODUCTION_NUGET_API_KEY");

string DevelopmentNuGetApiKey = HasArgument("DevelopmentNuGetApiKey")
    ? Argument<string>("DevelopmentNuGetApiKey")
    : EnvironmentVariable("DEVELOPMENT_NUGET_API_KEY");

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
    versionSuffix += $"-local{DateTime.Now.ToString("yyMMdd")}";
  }

  Version = $"{versionPrefix}{versionSuffix}";
}

Information($"{SolutionName} cake build script start");
Information($"Build target: {Target}, version: {Version}, is CI build: {IsCiBuild}, "
+ $"prod NuGet API key: {(string.IsNullOrWhiteSpace(ProductionNuGetApiKey) ? "null" : "****")}, "
+ $"dev NuGet API key: {(string.IsNullOrWhiteSpace(DevelopmentNuGetApiKey) ? "null" : "****")}");

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
    DotNetCoreNuGetPushSettings pushSettings;

    if (IsReleaseBuild)
    {
      if (string.IsNullOrWhiteSpace(ProductionNuGetSourceUrl))
      {
        throw new Exception("Error: ProductionNuGetSourceUrl is required to push NuGet packages");
      }

      pushSettings = new DotNetCoreNuGetPushSettings
      {
        ApiKey = ProductionNuGetSourceUrl,
        Interactive = !IsCiBuild,
        Source = ProductionNuGetSourceUrl
      };
    }
    else
    {
      if (string.IsNullOrWhiteSpace(DevelopmentNuGetApiKey))
      {
        throw new Exception("Error: DevelopmentNuGetApiKey is required to push NuGet packages");
      }

      pushSettings = new DotNetCoreNuGetPushSettings
      {
        ApiKey = DevelopmentNuGetApiKey,
        Interactive = !IsCiBuild,
        Source = DevelopNuGetSourceUrl
      };
    }

    DotNetCoreNuGetPush(System.IO.Path.Combine(PackagesDir, "*.nupkg"),
      pushSettings);
  });

RunTarget(Target);