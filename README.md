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
This repository currently contains design documents, folder scaffolding, and planning artifacts only. No Unity project files or binary assets exist yet. Use the documents in `docs/` to understand the intended architecture, systems, and testing strategy. Future milestones will add Unity project settings, assembly definitions, and source code following the outlined plans.

## Contribution Rules
- Read `docs/ARCHITECTURE.md`, `docs/TEST_PLAN.md`, and `docs/NAMING.md` before contributing.
- All contributions must include updated documentation and tests according to `docs/CHECKLISTS.md`.
- Follow the branching and PR process described in `docs/CONTRIBUTING.md`.
- Preserve determinism and testability seams (RNG, Time, IO) in all systems.

## What Exists Now
- Documentation of design pillars, world generation, AI, social systems, mechanic context, architecture, and test plan.
- Folder structure matching the planned Unity project layout with placeholder markers.
- `.gitignore` tuned for Unity and this documentation-focused phase.

## What Comes Next
1. **M0 (current)**: Complete documentation and scaffolding.
2. **M1**: Introduce Unity project settings, assembly definition files, and code scaffolding (interfaces, deterministic RNG, tick manager, data containers).
3. **M2**: Implement overworld generation and simulation loop with legends logging and persistence seams.
4. **M3**: Deliver base mode MVP with zones, jobs, raids, and comprehensive test coverage.

See `docs/ROADMAP.md` for detailed milestone descriptions and risk mitigation strategies.
