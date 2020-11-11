#r "System.Xml"

using System.Xml;

const string SolutionName = "Revo";

string Target = Argument<string>("Target", "Default");

bool IsCleanEnabled = Argument<bool>("DoClean", true);
bool IsRestoreEnabled = Argument<bool>("DoRestore", true);
bool IsBuildEnabled = Argument<bool>("DoBuild", true);
bool IsTestEnabled = Argument<bool>("DoTest", true);
bool IsPackEnabled = Argument<bool>("DoPack", true);

string Configuration = HasArgument("Configuration")
    ? Argument<string>("Configuration")
    : EnvironmentVariable("Configuration") ?? "Release";

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
    }
    else if (branchName != "develop" && branchName != "master")
    {
      versionSuffix += $"-{branchName.Replace('_', '-').ToLowerInvariant()}";
    }

    if (branchName != "master" && BuildNumber != null)
    {
      versionSuffix += $"-build{BuildNumber.Value:00000}";
    }
  }
  else
  {
    versionSuffix += $"-local{DateTime.Now.ToString("yyMMdd")}";
  }

  Version = $"{versionPrefix}{versionSuffix}";
}

string GetXunitXmlReportFilePath(FilePath projectFile)
{
  return new DirectoryPath(ReportsDir).CombineWithFilePath(projectFile.GetFilenameWithoutExtension()).FullPath + ".xml";
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
    .IsDependentOn("Pack");

Task("Clean")
  .WithCriteria(IsCleanEnabled)
  .Does(() =>
  {
    CleanDirectories(new [] { PackagesDir, ReportsDir });

    DotNetCoreClean(SolutionFile,
      new DotNetCoreCleanSettings()
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
        Verbosity = DotNetCoreVerbosity.Normal,
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
    foreach (var projectFile in GetFiles("./**/Revo.*.csproj")) // without the "Revo.*" prefix, it also matches stuff from ./tools
    {
      if (!projectFile.GetFilename().FullPath.StartsWith("Revo.")
      || projectFile.GetFilename().FullPath.EndsWith(".Tests.csproj")
      || projectFile.GetFilename().FullPath.StartsWith("Revo.Examples."))
      {
        continue;
      }

      DotNetCorePack(
        projectFile.FullPath,
        new DotNetCorePackSettings
        {
        ArgumentCustomization = args => args.Append($"/p:Version={Version}"),
          Configuration = Configuration,
          OutputDirectory = PackagesDir,
          NoBuild = true,
          NoRestore = true,
          IncludeSymbols = true,
          Verbosity = DotNetCoreVerbosity.Minimal
        });
    }
  });

RunTarget(Target);