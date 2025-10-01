# Architecture Overview

## Layered Structure
| Layer | Responsibilities | Assembly (planned) |
| --- | --- | --- |
| Core | Deterministic tick management, DI container seams, event bus, RNG/time abstractions. | `Wastelands.Core` |
| Generation | World creation algorithms, apocalypse synthesis, faction seeding. | `Wastelands.Generation` |
| Simulation (Overworld) | Yearly tick processing, diplomacy, trade, raids, legends updates. | `Wastelands.Simulation` |
| BaseMode | Daily base simulation, jobs, hazards, raids. | `Wastelands.BaseMode` |
| UI | Presentation logic, overlays, HUD, indirect order interfaces. | `Wastelands.UI` |
| Persistence | Save/load, snapshot management, JSON serialization, schema versioning. | `Wastelands.Persistence` |
| Utils | Shared math helpers, data transforms, extension methods. | `Wastelands.Utils` (implicit within Core if needed) |

Unity-facing components reside in UI, BaseMode, and Simulation assemblies. Core, Generation, Persistence stay pure C# to maximize testability.

## Dependency Diagram
```
Wastelands.UI
   ↑
Wastelands.BaseMode   Wastelands.Simulation
        ↑                ↑
     Wastelands.Core ← Wastelands.Generation
           ↑
  Wastelands.Persistence
```
- Arrows indicate "depends on" (references assembly below).
- Tests assemblies reference relevant runtime assemblies but are not depended upon.
- `Wastelands.Utils` (if split) may be referenced by all non-UI assemblies but must remain Unity-agnostic.

## Dependency Rules
- Core has no dependencies on UnityEngine types; other assemblies interact with Unity via adapter interfaces defined in Core.
- Generation depends only on Core and Utils.
- Simulation depends on Core, Generation, and Persistence for data access.
- BaseMode depends on Core and Persistence; may consume Simulation DTOs but not behaviors.
- UI depends on Core, Simulation, and BaseMode for view models only.
- Persistence depends on Core for data models; no upward dependencies.
- Tests assemblies reference runtime assemblies but cannot be referenced back.

## Service Boundaries & Interfaces
- **IRngService**: Deterministic stream provider with named channels (terrain, factions, combat). Seed injected at Boot.
- **ITimeProvider**: Offers current tick, converts between overworld/yearly and base/daily scales (mockable).
- **IEventBus**: Publish/subscribe system for cross-layer communication without static singletons.
- **IPathfinder**: Abstract pathfinding logic enabling mockable tests and future job system offloading.
- **IPersistenceGateway**: Handles read/write of JSON snapshots; implemented in Persistence, mocked in tests.
- **IOracleInterventionService**: Determines when the overseer AI draws from Minor/Major/Epic decks and emits deterministic intervention commands.
- **IAudio/IVfx Interfaces**: Unity adapters living in UI assembly; core logic only raises intents.

## Update & Tick Model
- **Overworld**: Fixed yearly tick triggered by Simulation layer, pulling data from Generation and Core state.
- **Base Mode**: Fixed daily tick subdivided into hourly microticks for job scheduling.
- **Event Processing**: Commanded through event bus to ensure deterministic order; queue processed each tick.
- **Oracle Evaluation**: `IOracleInterventionService` samples tension metrics after simulation ticks, selects eligible deck/cards, and pushes effects back through the event bus.
- **Performance**: Use object pools and struct-based DTOs to reduce GC pressure; no static mutable state.

## Assembly Definition Plan
- `Wastelands.Core.asmdef`
- `Wastelands.Generation.asmdef`
- `Wastelands.Simulation.asmdef`
- `Wastelands.BaseMode.asmdef`
- `Wastelands.UI.asmdef`
- `Wastelands.Persistence.asmdef`
- `Wastelands.Tests.EditMode.asmdef`
- `Wastelands.Tests.PlayMode.asmdef`

Dependencies follow the diagram above; tests reference their required runtime assemblies only.

## Composition Root & DI
- Boot scene hosts composition root that wires services using scriptable configuration assets (later stored under `Assets/_Project/Settings`).
- Core exposes interfaces; runtime implementations registered at Boot based on build target.
- Avoid static singletons; prefer constructor injection or factory methods.

## Testability Constraints
- Deterministic RNG interface, time provider, and IO gateways must be injectable.
- Pure data models defined in Core, Generation, and Persistence without UnityEngine references (`docs/DATA_MODEL.md`).
- Simulation ticks must be side-effect free given identical inputs; use command objects for state mutations.
- Tests rely on JSON fixtures and mock services as outlined in `docs/TEST_PLAN.md`.

## Cross-References
- Data models: `docs/DATA_MODEL.md`
- Testing details: `docs/TEST_PLAN.md`
- Scene flow: `docs/SCENES_AND_FLOW.md`
- Social and AI systems: `docs/SOCIAL_NOBLES.md`, `docs/AI_COMBAT_INDIRECT.md`, `docs/BASE_MODE.md`
