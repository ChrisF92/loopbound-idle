# Technical Architecture

## Engine choice

Use Unity for the Android client. Keep the simulation independent from scenes
and MonoBehaviours so most work can happen in Cursor without opening Unity.

## Layering

### Core simulation

Location: `Assets/LoopboundIdle/Scripts/Core`

Responsibilities:

- Resource storage.
- Building costs and production.
- Upgrade effects.
- Challenge modifiers and completion tracking.
- Collapse/prestige math.
- Offline progress simulation.

The core should avoid direct UI dependencies. Unity UI should read from and call
into this layer.

### Presentation

Location: `Assets/LoopboundIdle/Scripts/Presentation`

Responsibilities:

- Screen navigation.
- Button binding.
- Text formatting.
- Resource display.
- Save/load timing.
- Ad and purchase entry points.

Presentation code may depend on UnityEngine and TMPro. Core balance code should
not.

### Persistence

Location: `Assets/LoopboundIdle/Scripts/Persistence`

Responsibilities:

- Local save/load.
- Export/import save text.
- Save migrations.
- Offline progress timestamp handling.

Keep a `saveVersion` field on save data and migrate old saves explicitly.

## Balance data

The first catalog is hard-coded in `KingdomCatalog.CreateDefault()` to move
quickly. Once the loop is fun, migrate definitions to ScriptableObjects or JSON
if editing speed becomes a bottleneck.

## Testing

Unity edit-mode tests live under:

`Assets/LoopboundIdle/Scripts/Tests/EditMode`

Use these tests for deterministic economy rules: production, purchases,
collapse rewards, challenge completion, save migration, and offline progress.

## Agent workflow

Good Cursor tasks while away from Unity:

- Add or tune building definitions.
- Add challenge definitions and rewards.
- Implement save migration classes.
- Expand unit tests.
- Build UI presenter/view-model classes.
- Write Google Play store listing drafts.
- Review monetization flows for non-P2W compliance.

Tasks that need Unity editor access:

- Scene layout.
- Canvas anchoring and portrait-device testing.
- TextMeshPro asset setup.
- Android signing/build validation.
- In-app purchase/ad SDK setup.

