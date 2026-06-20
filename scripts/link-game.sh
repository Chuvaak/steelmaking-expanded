#!/usr/bin/env bash
# Linux/macOS counterpart of link-game.ps1. VS Code can't read .env for ${env:} substitution
# in a launch "program" path, so the launch configs point at a stable workspace symlink instead.
# This resolves the real install (env var first, else the gitignored repo-root .env) and
# (re)creates that symlink before launch. Accepts the same flags as the PowerShell version.
#
#   link-game.sh -Key <ENV_VAR_NAME> -Link <workspace-relative-path>
set -euo pipefail

key=""
link=""
while [[ $# -gt 0 ]]; do
  case "$1" in
    -Key)  key="${2:?-Key needs a value}"; shift 2 ;;
    -Link) link="${2:?-Link needs a value}"; shift 2 ;;
    *)     echo "Unknown argument: $1" >&2; exit 1 ;;
  esac
done

[[ -n "$key" && -n "$link" ]] || { echo "Usage: link-game.sh -Key <VAR> -Link <path>" >&2; exit 1; }

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/.." && pwd)"

# Env var wins; otherwise read the matching line from the repo-root .env (see .env.example).
path="${!key:-}"
if [[ -z "$path" && -f "$repo_root/.env" ]]; then
  path="$(sed -nE "s/^[[:space:]]*${key}[[:space:]]*=[[:space:]]*(.+[^[:space:]])[[:space:]]*$/\1/p" "$repo_root/.env" | head -n1)"
fi

[[ -n "$path" ]] || { echo "$key is not set in the environment or in the repo-root .env file (see .env.example)." >&2; exit 1; }
[[ -f "$path/Vintagestory.dll" ]] || { echo "No Vintagestory.dll found under '$path' (from $key). Check the path." >&2; exit 1; }

link_full="$repo_root/$link"
mkdir -p "$(dirname "$link_full")"

# Remove any existing symlink/dir at the link path first (without touching the target).
if [[ -L "$link_full" ]]; then
  rm -f "$link_full"
elif [[ -e "$link_full" ]]; then
  rm -rf "$link_full"
fi

ln -s "$path" "$link_full"
echo "Linked $link -> $path"
