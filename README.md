# Desert Wastes: World Unification Simulator

## Project Vision
"Desert Wastes" is a post-apocalyptic, Dwarf Fortress-inspired world and history simulator built in Unity 2D URP. Players guide a fledgling faction to rebuild civilization by uniting the fractured world, balancing overworld strategy with detailed base management. Leadership matters: indirect orders cascade through command hierarchies, the permadeath of the expedition leader ends the campaign, and a semi-autonomous "divine" AI occasionally intervenes through curated event decks that reshape the world. Presentation favors square-tile pixel art for tactical clarity, while combat, labor, social, and intellectual skill tracks echo the depth of classic fortress sims.

## One-Pager
- **Genre**: Simulation / Strategy / Base Builder
- **Engine**: Unity 2022/2023 LTS (2D URP, Input System)
- **Platform Goals**: PC (mouse + keyboard, accessibility in mind)
- **Core Loop**: Generate world → Survey history → Embark → Manage base with indirect orders → Respond to raids/disasters → Oracle AI draws from event decks → Return to overworld → Expand influence → Unite world.
- **Fail State**: Leader permadeath or total faction collapse.

## Repository Usage
This repository now includes the Unity project scaffold alongside the design documentation. Project settings, assembly definitions, and core deterministic services are in place to support forthcoming overworld and base mode systems. Use the documents in `docs/` to understand the intended architecture, systems, and testing strategy while the early runtime foundation comes together.

## Contribution Rules
- Read `docs/ARCHITECTURE.md`, `docs/TEST_PLAN.md`, and `docs/NAMING.md` before contributing.
- All contributions must include updated documentation and tests according to `docs/CHECKLISTS.md`.
- Follow the branching and PR process described in `docs/CONTRIBUTING.md`.
- Preserve determinism and testability seams (RNG, Time, IO) in all systems.

## What Exists Now
- Documentation of design pillars, world generation, AI, social systems, mechanic context, architecture, and test plan.
- Unity project settings under `ProjectSettings/` and scoped assembly definition files inside `Assets/_Project/Scripts/`.
- Deterministic core services (time provider, RNG, event bus) and tick manager scaffolding with corresponding EditMode unit tests.
- Folder structure and placeholder assets that align with the planned Unity layout, plus a Unity-focused `.gitignore`.

## What Comes Next
1. **M1 (current)**: Harden the code scaffold by expanding data containers and integration seams for overworld/base systems.
2. **M2**: Implement overworld generation and simulation loop with legends logging and persistence seams.
3. **M3**: Deliver base mode MVP with zones, jobs, raids, and comprehensive test coverage.

See `docs/ROADMAP.md` for detailed milestone descriptions and risk mitigation strategies.
