# Release Plan Status Evaluation

_Last updated: 2025-10-01_

## Current Stage Summary
- **Stage**: Preparing Milestone M2 – Overworld Gen + Sim (not started).
- **Rationale**: Code scaffold goals are satisfied: deterministic service container/installer flows are live, baseline world data containers/normalizers/validators are in place, and serialization plus EditMode coverage have landed. Simulation features outlined for M2 are still ahead.

## Milestone Breakdown

| Milestone | Deliverable Snapshot | Status | Evidence |
| --- | --- | --- | --- |
| **M0 – Docs & Scaffolding** | Documentation set (design, architecture, tests), project scaffolding, Unity-focused `.gitignore`, placeholder assets | ✅ Complete | `docs/` design docs, refreshed `README.md`, Unity folder layout in `Assets/` and `ProjectSettings/` |
| **M1 – Code Scaffold** | Unity project settings, assembly definitions, deterministic services, TickManager, baseline data containers with tests | ✅ Complete | `ProjectSettings/*.asset`, asmdefs in `Assets/_Project/Scripts/**`, deterministic services & tick manager (`Assets/_Project/Scripts/Core/**`), data containers + normalizer (`Assets/_Project/Scripts/Core/Data/*.cs`), persistence & installer coverage in `Tests/EditMode/*.cs` |
| **M2 – Overworld Gen + Sim** | World generation pipeline, overworld simulation loop, legends logging, persistence seams with tests | ⏳ Not started | No overworld generation or simulation implementations present |
| **M3 – Base Mode MVP** | Base scene systems (zones, jobs, raids, social mandates), Oracle integration, comprehensive save/load & tests | ⏳ Not started | Base mode gameplay systems not yet implemented |

## Key Gating Items to Kick Off M2
1. Stand up the initial overworld generation pipeline per `docs/WORLDGEN.md`, using the new data containers as output targets.
2. Implement the deterministic overworld simulation loop skeleton within `Assets/_Project/Scripts/Simulation/` and validate via EditMode tests.
3. Define persistence seams for overworld snapshots (load/save entry points) that exercise `WorldDataSerializer` against representative scenarios.

## Recommended Next Steps
- Stand up CI or editor automation to run the deterministic EditMode suite on every change.
- Spike the world generation prototype using `WorldData` outputs to validate data shaping ahead of full implementation.
- Outline the overworld simulation tick phases (per `docs/WORLDGEN.md` and `docs/BASE_MODE.md`) to de-risk cross-mode integration work.
