# Local Environment Guide

This guide explains how to set up the Desert Wastes Unity project on a local workstation, run the
available gameplay loops, execute automated tests, and integrate new graphical assets.

## 1. Required Hardware & Operating Systems
- Windows 10/11, macOS 12+, or a modern Linux distribution with OpenGL 4.5/Vulkan drivers.
- Quad-core CPU, 16 GB RAM, and an SSD recommended for Unity import speed.
- GPU capable of Unity 2D URP; most integrated graphics from the last 5 years work for editor play mode.

## 2. Install the Core Software Stack
1. **Unity Hub 3.x** – install the Unity 2022.3.17f1 LTS editor to match the repository's project
   version.【F:ProjectSettings/ProjectVersion.txt†L1-L2】
2. **Unity modules** – add Windows/Mac/Linux build support modules matching your OS if you intend to
   produce standalone builds.
3. **Git 2.40+** – required for version control and syncing with the `develop` branch workflow.【F:docs/CONTRIBUTING.md†L4-L18】
4. **Git LFS** – ensures large binary assets (future art/audio) sync correctly with collaborators.【F:docs/CI_PIPELINE.md†L20-L23】
5. **Script IDE** – Visual Studio, JetBrains Rider, or VS Code with the C# extension; configure Unity to
   open scripts with your preferred editor via *Edit → Preferences → External Tools*.

## 3. Clone and Prepare the Repository
1. Fork the repository if needed, then clone locally:
   ```bash
   git clone git@github.com:<your-account>/Desert-Wastes.git
   cd Desert-Wastes
   ```
2. If the repo adds Git LFS-tracked assets later, run `git lfs install` once per machine.
3. Review the contributor expectations before you begin coding:
   - Branch from `develop` and follow the issue-first workflow.【F:docs/CONTRIBUTING.md†L4-L18】
   - Adhere to the naming conventions for scripts, assets, and branches.【F:docs/NAMING.md†L1-L35】【F:docs/NAMING.md†L37-L52】
   - Consult the architecture and test strategy docs for subsystem design intent.【F:docs/ARCHITECTURE.md†L1-L8】【F:docs/TEST_PLAN.md†L1-L34】

## 4. Open the Project in Unity
1. Launch Unity Hub and click **Add** → select the `Desert-Wastes` folder.
2. Open the project with Unity 2022.3.17f1. Hub will import packages defined in `Packages/` and rebuild the
   Library cache on first open.
3. The Scenes folder currently contains Boot/World/Base entry points described in the design docs; use
   *File → Open Scene* and load `Assets/_Project/Scenes` as you iterate.【F:docs/SCENES_AND_FLOW.md†L1-L22】【F:ea6d17†L1-L9】
4. Configure play mode resolution and URP rendering via *Project Settings → Graphics* if you need to profile
   performance, keeping deterministic services intact as outlined in the architecture doc.【F:docs/ARCHITECTURE.md†L1-L8】

## 5. Run the Game Locally
1. **Boot Flow** – enter Play Mode from the Boot scene to initialize services and transition into the World/Base
   flow documented in `docs/SCENES_AND_FLOW.md`.【F:docs/SCENES_AND_FLOW.md†L1-L22】
2. **World Simulation** – use the World scene to validate overworld generation and yearly tick progression via
   the deterministic simulation loop.【F:Assets/_Project/Scripts/Simulation/OverworldSimulationLoop.cs†L1-L126】
3. **Base Simulation** – switch to the Base scene to observe daily tick processing, mandates, and runtime state
   scaffolding.【F:Assets/_Project/Scripts/BaseMode/BaseSceneBootstrapper.cs†L9-L63】【F:Assets/_Project/Scripts/BaseMode/BaseRuntimeState.cs†L9-L205】

## 6. Execute Automated Tests
1. Inside the Unity Editor, open **Window → General → Test Runner** and run both EditMode and PlayMode suites
   to confirm determinism and regression safety.【F:docs/TEST_PLAN.md†L1-L34】
2. For command-line or CI parity, run Unity in batch mode using the documented commands:
   ```bash
   "${UNITY_EDITOR_PATH}" \
     -batchmode -quit \
     -projectPath "$(pwd)" \
     -runTests -testPlatform editmode \
     -testResults "$(pwd)/artifacts/editmode-results.xml"

   "${UNITY_EDITOR_PATH}" \
     -batchmode -quit \
     -projectPath "$(pwd)" \
     -runTests -testPlatform playmode \
     -testResults "$(pwd)/artifacts/playmode-results.xml"
   ```
   These mirror the CI pipeline, ensuring local runs catch issues before pushing.【F:docs/CI_PIPELINE.md†L1-L43】
3. Follow the test plan's deterministic-first guidance when authoring new coverage; prefer EditMode tests for
   pure logic and PlayMode tests for scene interactions.【F:docs/TEST_PLAN.md†L1-L34】

## 7. Add Graphical Resources
1. Place new sprites, textures, or VFX assets under `Assets/_Project/Art` to keep runtime content organized
   with other project assets.【F:ea6d17†L1-L9】
2. Apply the naming conventions (`spr_<context>_<descriptor>` for sprites, etc.) so pipelines and tooling can
   locate assets predictably.【F:docs/NAMING.md†L37-L52】
3. Configure import settings in the Inspector (Sprite Mode, Pixels Per Unit, Compression) to match the pixel-art
   direction described in the design overview.【F:docs/DESIGN.md†L1-L9】
4. For UI or runtime loading, reference assets via the Resources folder or ScriptableObject databases as defined
   in the architecture and base mode scaffolding so deterministic services remain intact.【F:Assets/_Project/Scripts/Core/Management/DeterministicServiceContainer.cs†L1-L77】【F:Assets/_Project/Scripts/BaseMode/BaseRuntimeState.cs†L9-L205】
5. Commit both the asset files and any metadata (`.meta`) Unity generates; verify GUID stability to avoid broken
   references when collaborating.

## 8. Validate Changes Before Committing
1. Re-run EditMode and PlayMode tests after significant logic or asset updates.【F:docs/TEST_PLAN.md†L1-L34】
2. Manually exercise the Boot → World → Base flow to ensure assets load correctly and deterministic services remain
   stable.【F:docs/SCENES_AND_FLOW.md†L1-L22】
3. Follow the pull request checklist (tests, docs, naming, accessibility) before opening a PR.【F:docs/CONTRIBUTING.md†L20-L34】

By following this workflow you maintain local parity with the shared CI pipeline, preserve determinism, and keep
art additions consistent with the project's architecture and naming standards.
