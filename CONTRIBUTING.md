# Contributing

## Pull requests

Create a branch from `main` for each change and open a pull request targeting `main`. Link an issue in the pull request description with `Fixes #123` when the pull request resolves it. GitHub will close the linked issue when the pull request is merged.

CI runs tests in Debug and Release and validates the Velopack installer for pull requests. Wait for the checks to pass before merging.

Use [Conventional Commits](https://www.conventionalcommits.org/) for meaningful commit messages, such as `fix(map): correct terrain rendering` or `feat(search): add filtering by unit type`.

## Releases

Merging to `main` does not publish a release. When ready to release, choose a new stable semantic version, create an annotated tag on the desired `main` commit, and push it:

```powershell
git switch main
git pull --ff-only origin main
git tag -a v0.12.1 -m "Release v0.12.1"
git push origin v0.12.1
```

Replace `v0.12.1` with the intended new version. Pushing a `vMAJOR.MINOR.PATCH` tag starts the release workflow, which tests and publishes the application, builds the Velopack installer and packages, and creates a GitHub Release. The tag must be new; never move an existing release tag. The application version comes from the tag. `Directory.Build.props` supplies the default version for non-release builds.
