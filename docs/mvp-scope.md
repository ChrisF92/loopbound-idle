# MVP Scope

This document defines the smallest useful version of Loopbound Idle: Kingdom.
The goal is to reach a playable Android prototype with enough replay value to
test retention before expanding content.

## MVP gameplay systems

### Resources

- **Population** - enables labor and scales early production.
- **Food** - sustains population and buys early growth.
- **Wood** - first construction material.
- **Stone** - mid-run construction material.
- **Knowledge** - research and law unlocks.
- **Authority** - advanced governance and challenge push resource.
- **Legacy** - prestige currency earned from collapse.

### Buildings

Initial building set:

1. Farms - produce Food.
2. Lumber Camps - produce Wood.
3. Quarries - produce Stone.
4. Schools - produce Knowledge.
5. Councils - produce Authority.
6. Archives - improve collapse rewards and later preserve progress.

Each building should have:

- Level/count.
- Scaling cost.
- Per-second production or multiplier effect.
- Milestone bonuses.

### Upgrades

Early upgrade categories:

- Production multipliers.
- Cost reductions.
- Collapse reward improvements.
- Automation unlocks.
- Challenge-specific rewards.

### Prestige

The first reset layer is **Collapse**.

Collapse should:

- Reset current-loop resources and buildings.
- Award Legacy based on total kingdom value and elapsed loop pressure.
- Preserve challenge completions and permanent upgrades.
- Start the next loop faster through Legacy purchases.

### Challenges

Challenge reigns are restricted loops with permanent rewards.

Initial challenge examples:

- **Famine Age** - Food production is reduced.
- **Dark Age** - Knowledge production is reduced.
- **Stonebound** - Wood production is reduced, Stone goals matter more.
- **Short Reign** - Collapse pressure rises faster.
- **Silent Council** - Authority production is reduced.

Challenge rewards should unlock mechanics or new build options where possible,
not only flat percentages.

## MVP UX

Required screens:

- Kingdom overview.
- Resources and production.
- Buildings.
- Research/upgrades.
- Collapse/prestige.
- Challenges.
- Settings/save/export.

Nice-to-have screens:

- Statistics.
- Build presets.
- Offline progress report.
- Run history.

## Save requirements

MVP saves should support:

- Local save.
- Manual export/import text.
- Version field for migrations.
- Last saved timestamp for offline progress.

Cloud save can wait until after the local model is stable.

## Release guardrails

Do not add these before the core loop is fun:

- Multiplayer.
- Complex character rosters.
- 3D scenes.
- Procedural maps.
- Server-authoritative economy.
- Paid-exclusive upgrades.

