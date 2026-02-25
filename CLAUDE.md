# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Z-media Army Clash — a mobile battle simulator prototype built with Unity 6 (6000.3.6f1). Two armies of 20 units auto-battle until one side is destroyed. Units have stats (HP, ATK, SPEED, ATKSPD) computed from base values plus Shape/Size/Color trait modifiers. Special feature: "God's Wrath" — a player-activated AoE skill charged by kills.

Primary spec and backlog: `TDD.md`. Development follows TDD (Red → Green → Refactor).

## Build & Test Commands

```bash
UNITY_BIN="/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity"

# Headless compile check
"$UNITY_BIN" -batchmode -projectPath "$PWD" -quit -logFile Logs/compile-check.log

# Run EditMode tests
"$UNITY_BIN" -batchmode -projectPath "$PWD" -runTests -testPlatform EditMode \
  -testResults Logs/editmode-results.xml -logFile Logs/editmode.log -quit

# Run PlayMode tests
"$UNITY_BIN" -batchmode -projectPath "$PWD" -runTests -testPlatform PlayMode \
  -testResults Logs/playmode-results.xml -logFile Logs/playmode.log -quit

# Run specific test suite
"$UNITY_BIN" -batchmode -projectPath "$PWD" -runTests -testPlatform EditMode \
  -testFilter "ZMediaTask.Tests.EditMode" -testResults Logs/editmode-filtered.xml \
  -logFile Logs/editmode-filtered.log -quit

# Optional dotnet checks (may fail in sandboxed environments)
dotnet build "Z-media Task.sln"
dotnet test ZMediaTask.Tests.EditMode.csproj
dotnet test ZMediaTask.Tests.PlayMode.csproj
```

Check logs for errors: `rg -n "error CS|Exception|FAILED|Assertion" Logs`

## Architecture

Four-layer architecture under `Assets/ZMediaTask/`:

```
Domain (pure C#, no Unity API)  ←  Application  →  Infrastructure (Unity adapters)
                                        ↑
                                   Presentation (UI, input, VFX)
```

**Dependency flow:** `Presentation → Application → Domain` and `Infrastructure → Application/Domain`

### Layer Rules

| Layer | Assembly | Unity API | Key References |
|---|---|---|---|
| **Domain** | `ZMediaTask.Domain` | Forbidden (`noEngineReferences: true`) | None |
| **Application** | `ZMediaTask.Application` | Forbidden (`noEngineReferences: true`) | Domain |
| **Infrastructure** | `ZMediaTask.Infrastructure` | Allowed | Domain, Application, Reflex |
| **Presentation** | `ZMediaTask.Presentation` | Allowed | Application, Reflex, R3.Unity, DOTween |

### Key Technology Constraints

- **No ECS** — traditional OOP with services
- **DI:** Reflex (v14.2.0) — `ProjectScope` for global services, `SceneScope` for battle session
- **UI:** UI Toolkit (not uGUI)
- **Input:** New Input System (v1.18.0)
- **Reactive:** R3 (v1.3.0) — `ReactiveProperty`, `Subject`, `Observable`. No C# events in runtime logic
- **Animation:** DOTween — Presentation layer only, never in Domain/Application

### Domain Concepts

- **Stats:** `StatBlock`, `StatModifier`, `StatsCalculator` — trait modifiers are table-driven via `UnitTraitCatalog` (ScriptableObject config), not switch statements
- **Combat:** `AttackService`, `MovementService`, `HealthService`, `CooldownTracker`
- **Navigation:** 2-layer (`IPathfinder` for routes + `ISteeringService` for local movement + `ISlotAllocator` for melee positioning)
- **Battle loop:** `BattleLoopService` with `BattleStateMachine` (Preparation → Running → Finished)
- **God's Wrath:** `WrathMeter`, `WrathService`, `TryCastWrathUseCase` — AoE with friendly fire, player-only

### ATKSPD Formula

Higher ATKSPD = slower attacks: `cooldownSeconds = baseAttackDelay * ATKSPD`

## Coding Conventions

- `PascalCase` for types/methods/properties, `_camelCase` for private fields
- Interfaces: `IServiceName`
- Use cases: `VerbNounUseCase` (e.g., `TryCastWrathUseCase`)
- Interfaces at layer boundaries, concrete implementations in Infrastructure
- Trait modifiers via lookup tables, not switch/if chains
- All reactive communication via R3 streams, disposable lifecycle per View/ViewModel

## Testing

- Heavy EditMode unit tests (Domain + Application pure logic)
- PlayMode integration tests (scene bootstrap, DI, R3 subscriptions)
- Deterministic: seeded `IRandomProvider`, no FPS-dependent tests, no real timers
- Tests: `Assets/ZMediaTask/Tests/EditMode/` and `Assets/ZMediaTask/Tests/PlayMode/`

## Workflows

1. **Feature:** Pick iteration from `TDD.md` → write failing test → implement Domain → Application → Infrastructure → Presentation → run tests
2. **Balancing:** Update trait/config data → re-run Domain EditMode suites → verify no regressions
3. **Dependencies:** Pin versions in `Packages/manifest.json` (never floating `latest`) → re-run tests → commit manifest + lock together

## Commits

Short imperative subjects (`Add ...`, `Fix ...`, `Refactor ...`). One workflow unit per commit. Don't mix formatting with functional changes.

## Generated Directories

Treat `Library/`, `Logs/`, `Temp/`, and `obj/` as generated output — never commit these.
