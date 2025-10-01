# Continuous Integration Pipeline

This project targets Unity 2022/2023 LTS, so the recommended way to run the automated
EditMode and PlayMode suites in CI is to invoke the Unity Test Runner in batch mode.
The commands below work on Windows, macOS, or Linux runners as long as the matching
Unity editor is installed and activated with a valid license (see Unity's licensing
documentation for headless runners). You **must** supply licensing credentials to your
CI environment before attempting headless execution; GitHub Actions runners require one
of the following secrets:

- `UNITY_LICENSE`: the base64-encoded contents of an activated `.ulf` license file.
- `UNITY_SERIAL`: a Unity Plus/Pro/Enterprise serial number.
- `UNITY_EMAIL` and `UNITY_PASSWORD`: a personal license login pair that can claim and
  activate a license at runtime.

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
action and runs both suites on every pull request and pushes to the `develop` branch. The
workflow validates that license secrets are configured before invoking Unity; when secrets
are absent it emits a job summary explaining how to configure them and skips execution
instead of failing outright. You can adapt the same structure for GitLab CI, Azure Pipelines,
or Jenkins by substituting the installation step with your runner's preferred Unity provisioning
strategy.

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
      - name: Validate Unity licensing secrets
        id: license-secrets
        shell: bash
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
          UNITY_SERIAL: ${{ secrets.UNITY_SERIAL }}
          UNITY_EMAIL: ${{ secrets.UNITY_EMAIL }}
          UNITY_PASSWORD: ${{ secrets.UNITY_PASSWORD }}
        run: |
          if [[ -n "$UNITY_LICENSE" || -n "$UNITY_SERIAL" || ( -n "$UNITY_EMAIL" && -n "$UNITY_PASSWORD" ) ]]; then
            echo "license_present=true" >> "$GITHUB_OUTPUT"
          else
            echo "license_present=false" >> "$GITHUB_OUTPUT"
            echo "::warning::Unity license secrets are not configured; skipping test execution."
          fi
      - name: Run Unity Test Runner
        if: ${{ steps.license-secrets.outputs.license_present == 'true' }}
        uses: game-ci/unity-test-runner@v4
        with:
          projectPath: .
          unityVersion: 2022.3.0f1
          testMode: all
      - name: Upload test results
        if: ${{ steps.license-secrets.outputs.license_present == 'true' && always() }}
        uses: actions/upload-artifact@v4
        with:
          name: unity-test-results
          path: artifacts
      - name: Summarize skipped tests
        if: ${{ steps.license-secrets.outputs.license_present != 'true' }}
        run: |
          cat <<'MSG' >> "$GITHUB_STEP_SUMMARY"
          ## Unity tests skipped
          The Unity licensing secrets (`UNITY_LICENSE`, `UNITY_SERIAL`, or email/password pair) are not configured for this repository. Configure one of these secrets to enable automated EditMode and PlayMode test execution.
          MSG
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
