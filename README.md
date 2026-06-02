# Loopbound Idle: Kingdom

Loopbound Idle: Kingdom is a text/UI-first idle kingdom builder about ruling the
same doomed age over and over. Each loop lets the player grow a small kingdom,
complete constrained challenge reigns, collapse the timeline, and reinvest the
kingdom's remembered legacy into stronger future runs.

The project is intentionally designed for a small Android release scope:

- Unity target with a mostly UI-driven presentation.
- Pure C# simulation code that can be edited and reviewed in Cursor without
  needing the Unity editor open.
- Challenge-heavy replay inspired by games like Idling to Rule the Gods.
- Freemium monetization focused on convenience, cosmetics, and optional ads,
  never paid-exclusive progression power.

## Current repository layout

```text
Assets/LoopboundIdle/Scripts/Core/   Unity-compatible game simulation code
docs/                               Product, design, and implementation notes
Packages/                           Unity package manifest
ProjectSettings/                    Minimal Unity project version settings
```

## Recommended engine direction

Use Unity for the shipped Android client. The main reason is practical: Android
build support, Google Play integration, Unity Ads/mediation options, local
notifications, and your existing Unity experience all reduce release risk.

To make Cursor useful while you are away from Unity, keep the core game logic in
plain C# classes under `Assets/LoopboundIdle/Scripts/Core`. Unity scenes and UI
controllers should call into that core instead of owning balance formulas
directly.

## MVP pillars

1. **Short repeatable loops** - build the kingdom, push production, collapse,
   and return stronger.
2. **Challenge reigns** - restricted loops with permanent mechanical rewards.
3. **Convenience automation** - earned automation is core progression; paid
   convenience adds presets and UX, not stronger numbers.
4. **Data-first balancing** - buildings, upgrades, and challenges should be easy
   to tune without rewriting UI.

## First milestone

The first playable prototype should support:

- Food, Wood, Stone, Knowledge, Authority, and Legacy resources.
- A small chain of buildings that produce or convert resources.
- Basic upgrades.
- Collapse/prestige rewards.
- A few challenge modifiers.
- Offline progress simulation.
- Save/export/import hooks.

