#!/usr/bin/env bash
# Builds Revo (see build/Build.cs) - the same way as CI. Requires the .NET SDK from global.json; the PostgreSQL tests
# also need Docker (without it, they are skipped locally).
#
#   ./build.sh                  tests and packs the solution -> artifacts/test-results, artifacts/packages
#   ./build.sh Pack             packs the solution without running the tests
#   ./build.sh Push             pushes artifacts/packages to NUGET_SOURCE (API key in NUGET_API_KEY)
#   ./build.sh --help           lists all targets and parameters
set -euo pipefail

repo_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec dotnet run --project "$repo_dir/build/_build.csproj" -- "$@" --root "$repo_dir"
