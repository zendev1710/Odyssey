# Contributing

## Commit messages and releases

Use [Conventional Commits](https://www.conventionalcommits.org/) for commits that may be merged into `main`:

```text
<type>[optional scope][!]: <description>
```

Examples:

- `feat(search): add filtering by unit type` — increments the minor version.
- `fix(map): correct terrain rendering` — increments the patch version.
- `feat!: replace the saved-layout format` or a `BREAKING CHANGE:` footer — increments the major version.
- `docs: clarify the release process` — does not create a release.

On pushes to `main`, the release workflow examines the non-merge commits introduced by that push. `feat` increments the minor version; `fix`, `perf`, and `revert` increment the patch version; `!` or a `BREAKING CHANGE:` footer takes precedence and increments the major version. Other commit types do not create a release.

The workflow creates an annotated `vMAJOR.MINOR.PATCH` tag and a GitHub Release containing the Windows ZIP and MSI. The application and MSI use the same version. The starting version is held in `Directory.Build.props` until the first release tag exists; later releases use the latest stable version tag. Pushing a stable `vMAJOR.MINOR.PATCH` tag manually also runs the release workflow.
