$currentSuffixDate = (Get-Date).ToString("yyMMdd")
$sufixFilePath = "$PSScriptRoot/last-build-suffix.txt"

if ([System.IO.File]::Exists($sufixFilePath)) {
  $lastSuffix = Get-Content $sufixFilePath -Raw
  $lastSuffix = $lastSuffix.Trim()
  $lastSuffixParts = $lastSuffix.Split("-")
  $lastSuffixDate = if ($lastSuffixParts.Length -gt 1) {$lastSuffixParts[0]} else {$lastSuffix};
  
  $suffixCounter = if ($lastSuffixParts.Length -gt 1) {$lastSuffixParts[1]} else {0};
  $suffixCounter = if ($lastSuffixDate -ne $currentSuffixDate) {1} else {[int]$suffixCounter + 1}
}
else {
  $suffixCounter = 1
}

$newSuffix = $currentSuffixDate + "-" + $suffixCounter

Invoke-Expression ("$PSScriptRoot/build.ps1 -Target=Pack -VersionSuffix=local-" + $newSuffix + " -DoClean=false -DoTest=false -Configuration=Debug")

Set-Content -Path $sufixFilePath -Value $newSuffix

$commonProps = Get-Content "$PSScriptRoot/Common.props" -Raw
$versionPrefix = [regex]::Match($commonProps,'<VersionPrefix>(.+?)</VersionPrefix>').Groups[1].Value

$buildTargetsPaths = 
    "D:/Dev/Olify/Olify.Monorepo/Directory.Build.targets";
    
Foreach ($buildTargetsPath in $buildTargetsPaths)
{
  $buildTargets = Get-Content $buildTargetsPath -Raw
  $buildTargets = $buildTargets -replace '<PackageReference Update="Revo\.(.+?)" Version="(.+?)" />', ('<PackageReference Update="Revo.$1" Version="' + $versionPrefix + '-local-' + $newSuffix + '" />')
  Set-Content -Path $buildTargetsPath  -Value $buildTargets
}