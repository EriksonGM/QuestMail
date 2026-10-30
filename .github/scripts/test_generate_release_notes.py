"""Local Git fixtures; tests never contact GitHub, push tags, or publish packages."""

from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

from generate_release_notes import generate_notes, git


class ReleaseNotesTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory(prefix="questmail-release-notes-")
        self.addCleanup(self.directory.cleanup)
        self.repo = Path(self.directory.name)
        self.git("init", "--initial-branch=master")
        self.git("config", "user.name", "Release Notes Tests")
        self.git("config", "user.email", "release-tests@example.test")
        self.initial = self.commit("chore: initial content")
        self.git("tag", "v0.1.0")

    def git(self, *args):
        return git(*args, cwd=self.repo)

    def commit(self, subject):
        self.git("commit", "--allow-empty", "-m", subject)
        return self.git("rev-parse", "HEAD").strip()

    def notes(self, previous_tag="v0.1.0", source_ref="HEAD"):
        return generate_notes(previous_tag=previous_tag, source_ref=source_ref,
                              release_tag="v0.1.1", repository="EriksonGM/QuestMail", cwd=self.repo)

    def test_all_direct_commits_are_included_with_links_in_history_order(self):
        subjects = ["feat: add custom CSS", "fix: preserve body settings", "docs: update guide", "test: cover tracking", "ci: update workflow"]
        commits = [self.commit(subject) for subject in subjects]
        notes = self.notes()

        self.assertNotIn("chore: initial content", notes)
        self.assertNotIn(self.initial, notes)
        self.assertEqual(5, sum(line.startswith("- ") for line in notes.splitlines()))
        for subject, sha in zip(subjects, commits):
            self.assertIn(f"- {subject} ([{sha[:7]}](https://github.com/EriksonGM/QuestMail/commit/{sha}))", notes)
        self.assertEqual(sorted(notes.index(subject) for subject in subjects), [notes.index(subject) for subject in subjects])
        self.assertIn("/compare/v0.1.0...v0.1.1", notes)

    def test_first_release_includes_history_up_to_pinned_commit(self):
        target = self.commit("feat: first release feature")
        later = self.commit("fix: later work")
        notes = self.notes(previous_tag="", source_ref=target)

        self.assertIn("Initial release", notes)
        self.assertIn("chore: initial content", notes)
        self.assertIn(target, notes)
        self.assertNotIn("later work", notes)
        self.assertNotIn(later, notes)
        self.assertIn(f"/commits/{target}", notes)
        self.assertNotIn("/compare/", notes)

    def test_existing_release_range_uses_validated_sha_not_current_head(self):
        target = self.commit("feat: validated feature")
        self.commit("feat: not part of this release")

        notes = self.notes(source_ref=target)

        self.assertIn("validated feature", notes)
        self.assertNotIn("not part of this release", notes)

    def test_annotated_previous_tag_is_resolved_to_its_commit(self):
        self.git("tag", "-a", "v0.0.9", self.initial, "-m", "Annotated release")
        self.commit("fix: after annotated tag")

        notes = self.notes(previous_tag="v0.0.9")

        self.assertIn("after annotated tag", notes)
        self.assertNotIn("chore: initial content", notes)

    def test_branch_commits_and_merge_commit_are_all_included(self):
        self.git("switch", "-c", "feature")
        feature = self.commit("feat: merged branch work")
        self.git("switch", "master")
        direct = self.commit("docs: direct mainline work")
        self.git("merge", "--no-ff", "feature", "-m", "Merge feature branch")
        notes = self.notes()

        self.assertIn(feature, notes)
        self.assertIn(direct, notes)
        self.assertIn("Merge feature branch", notes)
        self.assertEqual(3, sum(line.startswith("- ") for line in notes.splitlines()))

    def test_empty_range_is_explicit(self):
        notes = self.notes()

        self.assertIn("No new commits since the previous version.", notes)
        self.assertIn("/compare/v0.1.0...v0.1.1", notes)
        self.assertFalse(any(line.startswith("- ") for line in notes.splitlines()))

    def test_non_ancestor_previous_tag_is_rejected(self):
        self.git("switch", "-c", "other-release")
        self.commit("feat: unrelated release branch")
        self.git("tag", "v9.0.0")
        self.git("switch", "master")

        with self.assertRaisesRegex(ValueError, "not an ancestor"):
            self.notes(previous_tag="v9.0.0")

    def test_missing_previous_tag_fails_instead_of_falling_back_to_all_history(self):
        with self.assertRaises(subprocess.CalledProcessError):
            self.notes(previous_tag="v8.0.0")

    def test_unicode_and_markdown_subjects_are_rendered_as_text(self):
        self.commit("docs: olá [guide](https://example.com) <script> `code` *bold*")

        notes = self.notes()

        self.assertIn("olá", notes)
        self.assertIn(r"\[guide\]\(https://example\.com\)", notes)
        self.assertIn("&lt;script&gt;", notes)
        self.assertIn(r"\`code\` \*bold\*", notes)
        self.assertNotIn("<script>", notes)

    def test_invalid_tags_and_repository_are_rejected(self):
        for tag in ["v1.2", "v01.2.3", "v1.2.3-beta", "--all"]:
            with self.subTest(tag=tag), self.assertRaises(ValueError):
                self.notes(previous_tag=tag)
        with self.assertRaises(ValueError):
            generate_notes(previous_tag="", source_ref="HEAD", release_tag="v0.1.1",
                           repository="invalid repository", cwd=self.repo)

    def test_cli_writes_utf8_notes_without_modifying_git_history(self):
        target = self.commit("feat: função nova")
        output = self.repo / "notes.md"
        result = subprocess.run(
            [sys.executable, str(Path(__file__).with_name("generate_release_notes.py")),
             "--previous-tag", "v0.1.0", "--source-ref", target, "--release-tag", "v0.1.1",
             "--repository", "EriksonGM/QuestMail", "--output", str(output)],
            cwd=self.repo, capture_output=True, encoding="utf-8",
        )

        self.assertEqual(0, result.returncode, result.stderr)
        self.assertIn("função nova", output.read_text(encoding="utf-8"))
        self.assertEqual(target, self.git("rev-parse", "HEAD").strip())
        self.assertEqual("v0.1.0", self.git("tag", "--list").strip())


if __name__ == "__main__":
    unittest.main()
