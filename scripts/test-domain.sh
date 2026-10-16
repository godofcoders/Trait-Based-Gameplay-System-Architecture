#!/usr/bin/env bash
set -euo pipefail

# Set UNITY_EDITOR to the Unity executable, not the .app directory on macOS.
: "${UNITY_EDITOR:?Set UNITY_EDITOR to your installed Unity editor executable. See README.md.}"
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
editor_dir="$(cd "$(dirname "$UNITY_EDITOR")" && pwd)"
if [[ "$editor_dir" == */Contents/MacOS ]]; then
    mono_root="$editor_dir/../MonoBleedingEdge"
else
    mono_root="$editor_dir/Data/MonoBleedingEdge"
fi
nunit_path=""
for candidate in "$repo_root"/Library/PackageCache/com.unity.ext.nunit@*/net40/unity-custom/nunit.framework.dll; do
    if [[ -f "$candidate" ]]; then nunit_path="$candidate"; break; fi
done
if [[ -z "$nunit_path" ]]; then
    echo "NUnit not cached. Open this project in Unity once to resolve its packages." >&2
    exit 1
fi
build_dir="$(mktemp -d "${TMPDIR:-/tmp}/trait-domain.XXXXXX")"
trap 'rm -rf "$build_dir"' EXIT
mono="$mono_root/bin/mono"
csc="$mono_root/lib/mono/4.5/csc.exe"
# Separate assemblies enforce the same direction of dependencies as the asmdefs.
"$mono" "$csc" -nologo -warnaserror+ -target:library -out:"$build_dir/GameplayInfrastructure.Core.dll" \
    "$repo_root"/Assets/GameplayInfrastructure/Core/*.cs
"$mono" "$csc" -nologo -warnaserror+ -target:library -out:"$build_dir/GameplayInfrastructure.Traits.dll" \
    -r:"$build_dir/GameplayInfrastructure.Core.dll" "$repo_root"/Assets/GameplayInfrastructure/Trait/*.cs
cp "$nunit_path" "$build_dir/nunit.framework.dll"
"$mono" "$csc" -nologo -warnaserror+ -out:"$build_dir/DomainTests.exe" \
    -r:"$build_dir/nunit.framework.dll" \
    -r:"$build_dir/GameplayInfrastructure.Core.dll" -r:"$build_dir/GameplayInfrastructure.Traits.dll" \
    "$repo_root/Tools/DomainTestRunner.cs" \
    "$repo_root/Assets/Tests/EditMode/AttributeMapTests.cs" \
    "$repo_root/Assets/Tests/EditMode/GameplayEntityTests.cs"
mkdir -p "$repo_root/TestResults"
"$mono" "$build_dir/DomainTests.exe" "$repo_root/TestResults/domain.xml"
