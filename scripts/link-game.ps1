param(
    # Name of the install-path variable, e.g. VINTAGE_STORY_121.
    [Parameter(Mandatory = $true)][string]$Key,
    # Workspace-relative directory junction to (re)create, e.g. bin/game-1.21.
    [Parameter(Mandatory = $true)][string]$Link
)

$ErrorActionPreference = 'Stop'

# VS Code can't read .env for ${env:} substitution in a launch "program" path, so the launch
# configs point at a stable workspace junction instead. This resolves the real install (env var
# first, else the gitignored repo-root .env) and (re)creates that junction before launch.

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

$path = [Environment]::GetEnvironmentVariable($Key)
if (-not $path) {
    $envFile = Join-Path $repoRoot '.env'
    if (Test-Path $envFile) {
        foreach ($line in Get-Content $envFile) {
            if ($line -match "^\s*$([regex]::Escape($Key))\s*=\s*(.+?)\s*$") {
                $path = $matches[1]
                break
            }
        }
    }
}

if (-not $path) {
    throw "$Key is not set in the environment or in the repo-root .env file (see .env.example)."
}
if (-not (Test-Path (Join-Path $path 'Vintagestory.dll'))) {
    throw "No Vintagestory.dll found under '$path' (from $Key). Check the path."
}

$linkFull = Join-Path $repoRoot $Link
$parent = Split-Path $linkFull -Parent
New-Item -ItemType Directory -Force -Path $parent | Out-Null

# Remove any existing junction first (rmdir removes the reparse point without touching the target).
if (Test-Path $linkFull) {
    & cmd /c rmdir "$linkFull"
}
New-Item -ItemType Junction -Path $linkFull -Target $path | Out-Null
Write-Host "Linked $Link -> $path"
