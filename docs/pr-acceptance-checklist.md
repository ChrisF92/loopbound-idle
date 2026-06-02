# PR Acceptance Checklist

Use this checklist before merging the first-playable prototype support branch.

## Unity compile and tests

- Open the project in Unity 2022.3.
- Confirm all asmdef files compile:
  - `LoopboundIdle.Kingdom.Core`
  - `LoopboundIdle.Kingdom.Persistence`
  - `LoopboundIdle.Kingdom.Presentation`
  - `LoopboundIdle.Kingdom.Tests.EditMode`
- Run all Edit Mode tests.
- Expected:
  - No compiler errors.
  - All Edit Mode tests pass.
  - No missing assembly references.

## Prototype scene wiring

- Create a scene with one `KingdomGameController`.
- Wire the minimum panels from `prototype-ui-wiring-checklist.md`.
- Confirm the UI refreshes from `viewModelChanged`.
- Confirm buttons call controller methods rather than mutating state directly.

## Debug utilities

- Add `KingdomDebugController` only to development/prototype scenes.
- Wire debug buttons separately from player-facing controls.
- Confirm debug actions are not reachable from production settings UI.
- Use debug grants to exercise:
  - resource displays
  - collapse reward flow
  - challenge completion flow
  - balance report generation

## Manual QA

- Run `manual-qa-checklist.md`.
- Record any failures with:
  - scene or device used
  - reproduction steps
  - expected behavior
  - actual behavior

## Save compatibility

- Start from no save.
- Save and load a local save.
- Export, start a new game, import the export.
- Attempt invalid import text.
- Expected:
  - Valid saves restore correctly.
  - Invalid saves leave current state unchanged.
  - Error text is visible for invalid imports.

## Balance sanity

- Generate a Markdown balance report through `KingdomDebugTools` or
  `KingdomDebugController`.
- Review 1, 5, and 15 minute snapshots.
- Confirm the early loop can buy at least Farms and Lumber Camps without manual
  resource grants.

## Merge readiness

- Unity compile succeeds.
- Edit Mode tests pass in Unity.
- Manual QA has no blocker failures.
- Debug utilities are either absent from production scenes or hidden behind a
  development-only entry point.
- PR description reflects any final scope changes.
