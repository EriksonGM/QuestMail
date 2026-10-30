"""Generate GitHub release notes from the complete validated Git commit range."""

import argparse
import html
from pathlib import Path
import re
import subprocess
import sys


STABLE_TAG = re.compile(r"v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)")


def git(*args: str, cwd: Path | None = None) -> str:
    return subprocess.run(
        ["git", "-c", "i18n.logOutputEncoding=utf-8", *args],
        cwd=cwd, check=True, capture_output=True, encoding="utf-8",
    ).stdout


def markdown_text(value: str) -> str:
    # Commit subjects are text, not Markdown/HTML supplied by the release author.
    return re.sub(r"([\\`*_{}\[\]()#+.!|])", r"\\\1", html.escape(value, quote=False))


def generate_notes(*, previous_tag: str, source_ref: str, release_tag: str,
                   repository: str, cwd: Path | None = None) -> str:
    if not STABLE_TAG.fullmatch(release_tag) or (previous_tag and not STABLE_TAG.fullmatch(previous_tag)):
        raise ValueError("Release tags must use vMAJOR.MINOR.PATCH.")
    if not re.fullmatch(r"[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+", repository):
        raise ValueError("Repository must use owner/name.")

    source_sha = git("rev-parse", "--verify", "--end-of-options", f"{source_ref}^{{commit}}", cwd=cwd).strip()
    repo_url = f"https://github.com/{repository}"
    lines = ["## Changes", ""]
    if previous_tag:
        previous_sha = git("rev-parse", "--verify", "--end-of-options", f"refs/tags/{previous_tag}^{{commit}}", cwd=cwd).strip()
        ancestor = subprocess.run(["git", "merge-base", "--is-ancestor", previous_sha, source_sha],
                                  cwd=cwd, capture_output=True, encoding="utf-8")
        if ancestor.returncode == 1:
            raise ValueError(f"Previous release tag {previous_tag} is not an ancestor of the validated commit.")
        ancestor.check_returncode()
        commit_range = f"{previous_sha}..{source_sha}"
        lines.extend([f"All commits since [{previous_tag}]({repo_url}/releases/tag/{previous_tag}).", ""])
    else:
        commit_range = source_sha
        lines.extend(["Initial release: all commits up to the validated source commit.", ""])

    # NUL delimiters preserve Unicode subjects and avoid delimiter collisions in commit messages.
    # No --first-parent or --no-merges: include direct commits, merged branch commits and merge commits.
    history = git("log", "--reverse", "--topo-order", "-z", "--format=%H%x00%s", commit_range, cwd=cwd)
    fields = history.split("\0")
    if fields[-1] == "":
        fields.pop()
    for index in range(0, len(fields), 2):
        commit_sha, subject = fields[index:index + 2]
        lines.append(f"- {markdown_text(subject)} ([{commit_sha[:7]}]({repo_url}/commit/{commit_sha}))")
    if not fields:
        lines.append("No new commits since the previous version.")

    if previous_tag:
        lines.extend(["", f"**Full changelog:** [{previous_tag}...{release_tag}]({repo_url}/compare/{previous_tag}...{release_tag})"])
    else:
        lines.extend(["", f"**Full history:** [View commits]({repo_url}/commits/{source_sha})"])
    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--previous-tag", default="")
    parser.add_argument("--source-ref", required=True)
    parser.add_argument("--release-tag", required=True)
    parser.add_argument("--repository", required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    try:
        notes = generate_notes(previous_tag=args.previous_tag, source_ref=args.source_ref,
                               release_tag=args.release_tag, repository=args.repository)
        args.output.write_text(notes, encoding="utf-8")
    except (ValueError, subprocess.CalledProcessError, OSError) as error:
        detail = error.stderr.strip() if isinstance(error, subprocess.CalledProcessError) and error.stderr else str(error)
        print(f"::error::{detail}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
