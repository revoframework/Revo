#r "System.IO.Compression.FileSystem"
#r "System.Xml"

//#tool "nuget:?package=xunit.runner.console"
//#tool "nuget:?package=OpenCover"
#addin "Cake.Incubator"

using System.IO.Compression;
using System.Net;
using System.Xml;

string Target = Argument<string>("Target", "Default");

string Configuration = HasArgument("Configuration") 
    ? Argument<string>("Configuration") 
    : EnvironmentVariable("Configuration") ?? "Release";

var SolutionDir = Context.Environment.WorkingDirectory.FullPath;
var SolutionFile = System.IO.Path.Combine(SolutionDir, "Revo.sln");

var PackagesDir = System.IO.Path.Combine(SolutionDir, "build", "packages");
var ReportsDir = System.IO.Path.Combine(SolutionDir, "build", "reports");

bool IsCiBuild = AppVeyor.IsRunningOnAppVeyor;

int BuildNumber =
    HasArgument("BuildNumber") ? Argument<int>("BuildNumber") :
    AppVeyor.IsRunningOnAppVeyor ? AppVeyor.Environment.Build.Number :
    EnvironmentVariable("BuildNumber") != null ? int.Parse(EnvironmentVariable("BuildNumber")) : 0;

string VersionSuffix = null;

// load VersionSuffix (if explicitly specified in Common.props)
var xmlDocument = new XmlDocument();
xmlDocument.Load(System.IO.Path.Combine(SolutionDir, "Common.props"));

var node = xmlDocument.SelectSingleNode("Project/PropertyGroup/VersionSuffix") as XmlElement;
if (node != null)
{
    VersionSuffix = node.InnerText;
}

// append the VersionSuffix for non-release CI builds
string ciTag = AppVeyor.Environment.Repository.Tag.IsTag ? AppVeyor.Environment.Repository.Tag.Name : null;
string ciBranch = AppVeyor.IsRunningOnAppVeyor ? AppVeyor.Environment.Repository.Branch : null;

if (ciTag == null && !string.IsNullOrWhiteSpace(ciBranch) && ciBranch != "master")
{
  VersionSuffix = VersionSuffix != null
    ? $"{VersionSuffix}-build{BuildNumber:00000}"
    : $"build{BuildNumber:00000}";
}

string GetXunitXmlReportFilePath(FilePath projectFile)
{
  return new DirectoryPath(ReportsDir).CombineWithFilePath(projectFile.GetFilenameWithoutExtension()).FullPath + ".xml";
}

Task("Default")
    .IsDependentOn("Pack");

Task("Clean")
  .Does(() =>
  {
    CleanDirectories(new []{ PackagesDir, ReportsDir });

    var msbuildSettings = new MSBuildSettings
      {
        Verbosity = Verbosity.Minimal,
        ToolVersion = MSBuildToolVersion.VS2017,
        Configuration = Configuration,
        PlatformTarget = PlatformTarget.MSIL,
        ArgumentCustomization = args => args
      };

    msbuildSettings.Targets.Add("Clean");

    MSBuild(SolutionFile, msbuildSettings);
  });

Task("Restore")
  .IsDependentOn("Clean")
  .Does(() =>
  {
    NuGetRestore(
      SolutionFile,
      new NuGetRestoreSettings ()
      {
        Verbosity = NuGetVerbosity.Normal
      });
  });

Task("Build")
  .IsDependentOn("Restore")
  .Does(() =>
  {
    MSBuild(SolutionFile,
      new MSBuildSettings
      {
        Verbosity = Verbosity.Minimal,
        ToolVersion = MSBuildToolVersion.VS2017,
        Configuration = Configuration,
        PlatformTarget = PlatformTarget.MSIL,
        ArgumentCustomization = args => args
          .Append($"/p:VersionSuffix={VersionSuffix}")
          .Append("/p:ci=true")
          .AppendSwitch("/p:DebugType", "=", "portable")
          .AppendSwitch("/p:ContinuousIntegrationBuild", "=", IsCiBuild ? "true" : "false")
          .AppendSwitch("/p:DeterministicSourcePaths", "=", "false") // Temporary workaround for https://github.com/dotnet/sourcelink/issues/91
      });
  });

Task("Test")
  .IsDependentOn("Build")
  .Does(() =>
  {
    var projectFiles = GetFiles("./Tests/**/*.csproj");
    foreach (var projectFile in projectFiles)
    {
      var arguments = new ProcessArgumentBuilder()
        .Append("-configuration " + Configuration)
        .Append("-nobuild")
        .Append($"-xml {GetXunitXmlReportFilePath(projectFile)}");

      var parsedProject = ParseProject(projectFile.FullPath, configuration: "Debug");
      if (parsedProject.TargetFrameworkVersions.Contains("netcoreapp2.0"))
      {
        arguments = arguments.Append("-framework netcoreapp2.0");
        arguments = arguments.Append("-fxversion 2.0.7");
      }

      var dotnetTestSettings = new DotNetCoreToolSettings
      {
        WorkingDirectory = projectFile.GetDirectory().FullPath
      };

      DotNetCoreTool(projectFile.FullPath, "xunit", arguments, dotnetTestSettings);
    }
  });

Task("Pack")
  .IsDependentOn("Test")
  .Does(() =>
  {
    foreach (var projectFile in GetFiles("./**/Revo.*.csproj")) // without the "Revo.*" prefix, it also matches stuff from ./tools
    {
      if (projectFile.FullPath.Contains("Revo.Tests.")
      || projectFile.FullPath.Contains("Revo.Tests.")
        || projectFile.FullPath.Contains("Revo.Examples."))
      {
        continue;
      }

      DotNetCorePack(
        projectFile.FullPath,
        new DotNetCorePackSettings
        {
          Configuration = Configuration,
          OutputDirectory = PackagesDir,
          NoBuild = true,
          NoRestore = true,
          IncludeSymbols = true,
          Verbosity = DotNetCoreVerbosity.Minimal,
          VersionSuffix = VersionSuffix
        });
    }
  });

RunTarget(Target);