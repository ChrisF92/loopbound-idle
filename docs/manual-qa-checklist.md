# Manual QA Checklist

Use this checklist when the prototype can be opened in Unity or run on device.
Each section is scoped to first-playable behavior rather than final polish.

## Fresh start

- Delete the local save or install/run with no existing save.
- Launch the scene with `KingdomGameController`.
- Expected:
  - No missing-save error is shown on first launch.
  - Loop label shows loop 1.
  - Food, Wood, and Population have non-zero starting amounts.
  - Stone, Knowledge, Authority, and Legacy start at zero.
  - Building, upgrade, collapse, challenge, and settings panels can be opened.

## Resource production

- Buy one Farm.
- Wait a few seconds.
- Expected:
  - Farm level increases to 1.
  - Wood is reduced by the Farm cost.
  - Food amount increases over time.
  - Food rate label is greater than zero.

- Buy one Lumber Camp after enough Food is available.
- Wait a few seconds.
- Expected:
  - Lumber Camp level increases to 1.
  - Wood amount increases over time.
  - Wood rate label is greater than zero.

## Affordability and buttons

- Spend below a building or upgrade cost.
- Expected:
  - The related buy button becomes non-interactable or visibly disabled.
  - Pressing the action directly does not create negative resources.

- Earn enough resources for the same item.
- Expected:
  - The buy button becomes interactable.
  - Buying updates cost, level/status, and resource labels on the next refresh.

## Save and load

- Buy at least one building.
- Press Save.
- Restart the scene or press Load after changing state.
- Expected:
  - Building levels and resources return to the saved values.
  - No error is shown for a valid save.

## Offline progress

- Buy at least one producing building.
- Save and close the app or stop the scene.
- Wait a short real-world interval.
- Reopen/load.
- Expected:
  - Offline progress label shows the elapsed interval.
  - Produced resources increase.
  - Offline simulation never exceeds the configured cap.

## Export and import

- Press Export.
- Expected:
  - Export text starts with `LOOPBOUND-KINGDOM-SAVE:1:`.
  - Export text is copied/displayed without truncation.

- Start a new game, paste the export text, and import.
- Expected:
  - Imported resources/buildings/challenges match the exported state.
  - Offline progress is applied from the export timestamp.

- Attempt to import invalid text.
- Expected:
  - Current state remains unchanged.
  - A clear error is shown.

## Collapse

- Grant or earn enough resources to make projected Legacy greater than zero.
- Press Collapse.
- Expected:
  - Loop index increments.
  - Current-loop resources and building levels reset.
  - Legacy increases by the projected reward.
  - Active challenge resets to none.

## Challenges

- Start Famine Age.
- Expected:
  - Active challenge label shows Famine Age.
  - Challenge row is marked active.
  - Completion button is disabled until goals are met.

- Grant or earn completion resources, then complete the active challenge.
- Expected:
  - Famine Age completion count increments.
  - Loop resets after completion.
  - Active challenge returns to none.

## Settings and destructive actions

- Press New Game.
- Expected:
  - State resets to initial loop values.
  - Save file is not deleted unless a separate delete/reset-save control exists.

- If a delete-save control is added later:
  - Confirm it requires deliberate user action.
  - Confirm relaunch starts fresh with no load error.

## Device sanity pass

- Test portrait layout on a small Android aspect ratio.
- Confirm all core buttons are reachable with one hand.
- Confirm long numbers do not overlap buttons.
- Confirm import/export fields can scroll/select/copy long text.
- Background and resume the app.
- Expected:
  - Pause save runs.
  - Resume does not duplicate timers or create double production.
