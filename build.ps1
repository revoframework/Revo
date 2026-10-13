# Builds Revo (see build/Build.cs) - the same as build.sh. Requires the .NET SDK from global.json; the PostgreSQL
# tests also need Docker (without it, they are skipped locally).
#
#   .\build.ps1                 tests and packs the solution -> artifacts/test-results, artifacts/packages
#   .\build.ps1 Pack            packs the solution without running the tests
#   .\build.ps1 --help          lists all targets and parameters

dotnet run --project "$PSScriptRoot/build/_build.csproj" -- @args --root "$PSScriptRoot"
exit $LASTEXITCODE
