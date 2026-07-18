#!/usr/bin/env bash
set -euo pipefail

WIKI_REPO="${1:-https://github.com/RobertAgterhuis/Blazor.Radzen.Themes.wiki.git}"
BRANCH="${WIKI_BRANCH:-master}"
DRY_RUN="${DRY_RUN:-false}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
SOURCE_DIR="$REPO_ROOT/eng/wiki"
TEMP_DIR="$(mktemp -d)"

cleanup() {
  rm -rf "$TEMP_DIR"
}
trap cleanup EXIT

echo "Preparing wiki publish from: $SOURCE_DIR"
echo "Target wiki repo: $WIKI_REPO"

if ! git clone --branch "$BRANCH" "$WIKI_REPO" "$TEMP_DIR"; then
  echo "Failed to clone wiki repository. If this is the first publish, initialize the GitHub wiki once via the UI by clicking 'Create the first page', then rerun this script." >&2
  echo "Local source pages prepared for sync:"
  ls -1 "$SOURCE_DIR"/*.md | xargs -n1 basename
  exit 1
fi

find "$TEMP_DIR" -maxdepth 1 -name '*.md' -type f -delete
cp "$SOURCE_DIR"/*.md "$TEMP_DIR"/

cd "$TEMP_DIR"

if [[ "$DRY_RUN" == "true" ]]; then
  echo "Dry run mode. Pending wiki changes:"
  git status --short
  exit 0
fi

if [[ -z "$(git status --porcelain)" ]]; then
  echo "No wiki changes to publish."
  exit 0
fi

git add *.md
git commit -m "docs(wiki): sync from eng/wiki"
git push origin "$BRANCH"

echo "Wiki publish complete."
