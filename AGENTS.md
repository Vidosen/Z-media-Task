# AGENTS

## Project Context
- Product: **Z-media Army Clash** prototype.
- Primary spec/backlog: `TDD.md`.
- Unity version: `6000.3.6f1` (`ProjectSettings/ProjectVersion.txt`).
- Architecture is layered under `Assets/ZMediaTask`: `Domain -> Application -> Infrastructure -> Presentation`.
- Runtime rules from spec: no ECS, DI via Reflex, UI via UI Toolkit, input via New Input System, reactive flow via R3.

## Project Structure
- Core code: `Assets/ZMediaTask/Domain`, `Assets/ZMediaTask/Application`, `Assets/ZMediaTask/Infrastructure`, `Assets/ZMediaTask/Presentation`.
- Tests: `Assets/ZMediaTask/Tests/EditMode`, `Assets/ZMediaTask/Tests/PlayMode`.
- Scenes: `Assets/Scenes`.
- Config and assets: `Assets/Resources`, `Assets/Settings`, `Packages/manifest.json`.
- Treat `Library`, `Logs`, `Temp`, and `obj` as generated output.

## New Workflows

### 1) Feature Workflow (TDD-first)
1. Pick the current iteration in `TDD.md` and define acceptance tests first.
2. Implement in order: `Domain` -> `Application` -> `Infrastructure` -> `Presentation`.
3. Add/adjust EditMode tests for logic changes and PlayMode tests for scene/integration behavior.
4. Run EditMode tests, then targeted PlayMode tests.
5. Validate Unity compile status and console before commit.

### 2) Balancing Workflow (stats/combat tuning)
1. Update trait/config data or combat constants in `Domain`/`Infrastructure` configs.
2. Re-run domain-level EditMode suites (`Traits`, `Combat`, `Battle`).
3. Verify no behavioral regression in battle loop integration tests.
4. Record balancing rationale in commit message or PR description.

### 3) Dependency Workflow (Unity packages/NuGet)
1. Pin package versions in `Packages/manifest.json` (never use floating `latest`).
2. Let Unity refresh package lock state (`Packages/packages-lock.json`).
3. Re-run EditMode tests after dependency changes.
4. Commit manifest and lock changes together.

## New Commands

### Unity Editor
- Open in Unity Hub:
  `open -a "Unity Hub" .`

### Unity Test Runs (preferred)
- Preferred path: run EditMode/PlayMode tests through Unity MCP when available.
- If Unity MCP is not available, ask the user to connect/enable Unity MCP first and proceed after connection.

### Logs and Diagnostics
- Find compile/runtime errors in logs:
  `rg -n "error CS|Exception|FAILED|Assertion" Logs`
- Tail latest EditMode log:
  `tail -n 200 Logs/editmode.log`

## Coding Conventions
- C# naming: `PascalCase` for types/methods/properties, `_camelCase` for private fields.
- Keep `Domain` free of Unity API usage.
- Prefer explicit interfaces at layer boundaries (`Application`/`Infrastructure`).
- Avoid mixing formatting-only changes with functional changes.

## Commit Guidelines
- Use short imperative commit subjects (`Add ...`, `Fix ...`, `Refactor ...`).
- Keep each commit scoped to one workflow unit (feature, balance pass, or dependency update).
- Include test evidence (EditMode/PlayMode commands or results) in PR description.
