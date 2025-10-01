# Documentation Index

This index points to the authoritative design, engineering, and process references that live in this repository. Treat it as the table of contents for the "Desert Wastes" documentation corpus.

## Orientation & Vision
- `README.md` – high-level pitch, current status snapshot, and upcoming priorities for the simulation project.
- `docs/DESIGN.md` – narrative pillars, player fantasy, and systems overview for the world unification simulator.
- `docs/ARCHITECTURE.md` – layered runtime plan, service boundaries, and determinism requirements for Unity assemblies.

## Systems & Simulation References
- `docs/WORLDGEN.md` – procedural world generation flow, including apocalypse synthesis and faction seeding rules.
- `docs/BASE_MODE.md` – day-to-day base simulation loop, zone definitions, job/mandate flows, and raid escalation rules.
- `docs/AI_COMBAT_INDIRECT.md` – indirect combat, Oracle intervention decks, and commander intent propagation.
- `docs/SOCIAL_NOBLES.md` – noble hierarchy, relationship dynamics, and social pressure systems that feed Base Mode decisions.
- `docs/MECHANIC_CONTEXT.md` – cross-cutting mechanics that bridge overworld and base play (resources, stressors, morale).
- `docs/DATA_MODEL.md` – canonical `WorldData` schema and deterministic serialization expectations used by services and tests.

## Engineering & Implementation Guides
- `docs/LOCAL_SETUP.md` – editor requirements, package cache tips, and troubleshooting steps for getting the Unity project running locally.
- `docs/CI_PIPELINE.md` – Unity Test Runner automation guidance, licensing secrets, and NUnit export expectations for CI.
- `docs/TEST_PLAN.md` – deterministic testing strategy, fixture management, and coverage expectations for overworld/base systems.
- `docs/NAMING.md` – code, asset, and data naming conventions shared across assemblies.

## Contribution Process
- `docs/CONTRIBUTING.md` – branching strategy, workflow checklist, and commit message conventions for feature work.
- `docs/CHECKLISTS.md` – Definition of Done checklists for features, tests, documentation, and accessibility reviews.
- `docs/INPUTS_AND_ACCESSIBILITY.md` – input device plans and accessibility considerations to evaluate with every contribution.

## Planning & Status Artifacts
- `docs/ROADMAP.md` – milestone sequencing, risk register, and dependency notes for upcoming work.
- `docs/RELEASE_STATUS.md` – milestone-by-milestone delivery evidence and the current release stage summary.
- `docs/STATUS_REPORT.md` – latest runtime/test coverage evaluation and prioritized risks across systems.
- `docs/MILESTONE_EVALUATION.md` – milestone acceptance criteria and evidence tying runtime features back to goals.

## Scene & Content Resources
- `docs/SCENES_AND_FLOW.md` – planned Unity scenes, bootstrapping order, and navigation flow between overworld and base.
- `docs/BASE_MODE.md` & `docs/SCENES_AND_FLOW.md` – zone layout expectations and transition triggers.
- `docs/STATUS_REPORT.md` & `docs/ROADMAP.md` – references for planning cross-checks when updating content or milestones.

## Updating the Documentation
- When adding systems or pipelines, update the relevant system doc (`docs/BASE_MODE.md`, `docs/WORLDGEN.md`, etc.) and add a short summary to `README.md` and `docs/RELEASE_STATUS.md`.
- Keep deterministic evidence current: link new tests and runtime code in `docs/STATUS_REPORT.md` and `docs/MILESTONE_EVALUATION.md` as features evolve.
- Use this index to verify whether a new document fits an existing category before creating a new file; consolidating updates under existing headings keeps the corpus navigable.
