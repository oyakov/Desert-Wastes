# Roadmap & Risk Register

## Milestones
| Milestone | Focus | Key Deliverables |
| --- | --- | --- |
| M0 – Docs & Scaffolding (Now) | Establish concept, architecture, test strategy, folder structure. | Completed documentation, `.gitignore`, placeholder assets, planning artifacts. |
| M1 – Code Scaffold | Create Unity project settings, assembly definitions, deterministic RNG, TickManager, core interfaces, basic data containers. | Unity project setup, asmdefs, interface implementations with unit tests. |
| M2 – Overworld Gen + Sim | Implement world generation, overworld simulation loop, legends logging, persistence seams. | Playable overworld loop, JSON saves, Oracle tension metrics, EditMode & PlayMode coverage for worldgen. |
| M3 – Base Mode MVP | Deliver base scene features: zones, jobs, raids, social mandates; integrate with overworld. | Functional base mode, Oracle event deck interventions, raids, research, full save/load, expanded tests. |

## Risk Register
| Risk | Impact | Mitigation |
| --- | --- | --- |
| Determinism Breaks (floating point drift) | High | Use integer/fixed-point where possible, centralize RNG/time, add regression tests (`docs/TEST_PLAN.md`). |
| Scope Creep in Social Systems | Medium | Lock MVP features per `docs/SOCIAL_NOBLES.md`, schedule future enhancements post-M3. |
| Oracle Intervention Balance | Medium | Define deterministic deck rules in `docs/AI_COMBAT_INDIRECT.md`, validate via tests (`docs/TEST_PLAN.md`). |
| Performance Bottlenecks in Simulation | Medium | Profile early, use data-oriented structures per `docs/ARCHITECTURE.md`, limit per-tick work. |
| Unity Version Upgrades | Medium | Pin 2022/2023 LTS, document upgrade path in `.tools/` scripts, test via CI. |
| Content Creation Load | Low | Rely on procedural generation, placeholders during MVP, defer art/audio to post-M3. |

## Dependencies & Sequencing
- M1 unlocks deterministic services required by worldgen tests (see `docs/TEST_PLAN.md`).
- M2 builds on data models defined in `docs/DATA_MODEL.md` and Generation plan (`docs/WORLDGEN.md`).
- M3 leverages social/noble rules (`docs/SOCIAL_NOBLES.md`) and indirect combat design (`docs/AI_COMBAT_INDIRECT.md`).

## Post-MVP Considerations
- Expanded diplomacy UI, mod support, advanced accessibility features.
- Live-ops style events and narrative arcs.
- Performance optimization for large-scale simulations.
