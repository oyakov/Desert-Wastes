# Continuous Integration Pipeline

This project targets Unity 2022/2023 LTS, so the recommended way to run the automated
EditMode and PlayMode suites in CI is to invoke the Unity Test Runner in batch mode.
The commands below work on Windows, macOS, or Linux runners as long as the matching
Unity editor is installed and activated with a valid license (see Unity's licensing
documentation for headless runners).

## Required Packages
- Unity Editor 2022.3 LTS (or the pinned version documented in `ProjectSettings/ProjectVersion.txt`).
- .NET SDK compatible with Unity's test assemblies (installed by Unity Hub on editor install).
- Git LFS for large binary assets (already used by this repository's `.gitattributes`).

## Command Line Test Execution
Use the built-in Unity CLI flags to execute the test suites without opening the editor:

```bash
# EditMode tests
"${UNITY_EDITOR_PATH}" \
  -batchmode -quit \
  -projectPath "${GITHUB_WORKSPACE}" \
  -runTests -testPlatform editmode \
  -testResults "${GITHUB_WORKSPACE}/artifacts/editmode-results.xml"

# PlayMode tests
"${UNITY_EDITOR_PATH}" \
  -batchmode -quit \
  -projectPath "${GITHUB_WORKSPACE}" \
  -runTests -testPlatform playmode \
  -testResults "${GITHUB_WORKSPACE}/artifacts/playmode-results.xml"
```

The `-batchmode` and `-quit` flags ensure the process exits once the tests finish, and the
`-testResults` path emits NUnit-compatible XML that CI systems can parse for reporting.

## Example GitHub Actions Workflow
This repository includes a ready-to-use workflow at `.github/workflows/unity-tests.yml`
that installs Unity via the [`game-ci/unity-test-runner`](https://github.com/game-ci/unity-test-runner)
action and runs both suites on every pull request and pushes to the `develop` branch. You can adapt the
same structure for GitLab CI, Azure Pipelines, or Jenkins by substituting the installation step with your
runner's preferred Unity provisioning strategy.

```yaml
name: Unity Tests

on:
  pull_request:
  push:
    branches:
      - develop

jobs:
  tests:
    name: Run Unity EditMode and PlayMode suites
    runs-on: ubuntu-latest
    permissions:
      contents: read
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
      - name: Cache Unity Library
        uses: actions/cache@v3
        with:
          path: Library
          key: Library-${{ hashFiles('**/Packages/packages-lock.json') }}
      - name: Run Unity Test Runner
        uses: game-ci/unity-test-runner@v4
        with:
          projectPath: .
          unityVersion: 2022.3.0f1
          testMode: all
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: unity-test-results
          path: artifacts
```

## Tips for Reliable CI Runs
- **Determinism**: The systems in this repository are designed to operate deterministically;
  CI should use the same deterministic services and seeds as local runs to avoid flakiness
  (`docs/ARCHITECTURE.md`, `docs/TEST_PLAN.md`).
- **Library Cache**: Caching the Unity `Library/` folder dramatically reduces cold build times,
  but always include a hash of `Packages/packages-lock.json` (or equivalent) in the key to bust
  the cache when dependencies change.
- **Headless Rendering**: PlayMode tests that require rendering can run in the default
  software renderer on Linux runners; for graphics-heavy suites consider enabling xvfb or
  running on Windows/macOS agents.
- **Failure Artifacts**: Export the generated XML result files (and optional logs from
  `Logs/`) as CI artifacts to aid debugging when tests fail.
```
