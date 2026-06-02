# Prototype UI Wiring Checklist

Use this checklist when Unity access is available again. The first scene can be
plain uGUI/TextMeshPro panels driven by `KingdomGameController`.

## Scene setup

- Create one persistent game object named `KingdomGame`.
- Add `KingdomGameController`.
- Keep defaults for the first pass:
  - `saveFileName`: `kingdom-save.json`
  - `loadOnStart`: enabled
  - `tickAutomatically`: enabled
  - `autosaveIntervalSeconds`: `30`
  - `saveOnPause`: enabled
  - `saveOnQuit`: enabled
- Subscribe UI refresh code to `viewModelChanged`.
- Subscribe export text UI to `exportTextCreated`.
- Subscribe an error label/toast to `errorChanged`.

## Header panel

Bind labels from `KingdomGameViewModel`:

- Loop label: `loopLabel`
- Elapsed label: `elapsedLabel`
- Collapse pressure: `collapsePressureLabel`
- Collapse pressure rate: `collapsePressureRateLabel`
- Projected Legacy: `projectedLegacyRewardLabel`
- Active challenge: `activeChallengeLabel`
- Last offline progress: `lastOfflineLabel`

## Resource panel

Create one row per `ResourceViewModel` in `resources`.

- Name label: `displayName`
- Amount label: `amountLabel`
- Rate label: `rateLabel`

Suggested first ordering is the array order from the view model.

## Buildings panel

Create one card or row per `BuildingViewModel` in `buildings`.

- Title: `displayName`
- Description: `description`
- Level: `levelLabel`
- Cost: `costLabel`
- Production: `productionLabel`
- Buy button interactable: `canBuy`
- Buy button action: `KingdomGameController.BuyBuilding((int)buildingId)`

For an Inspector-only first pass, wire fixed buttons by enum index:

- Farms: `0`
- Lumber Camps: `1`
- Quarries: `2`
- Schools: `3`
- Councils: `4`
- Archives: `5`

## Upgrades panel

Create one row per `UpgradeViewModel` in `upgrades`.

- Title: `displayName`
- Description: `description`
- Cost: `costLabel`
- Effect: `effectLabel`
- Status: `statusLabel`
- Buy button interactable: `canBuy`
- Buy button action: `KingdomGameController.BuyUpgrade((int)upgradeId)`

Hide or disable purchased upgrades when `purchased` is true.

## Collapse panel

- Projected reward label: `projectedLegacyRewardLabel`
- Collapse button action: `KingdomGameController.CollapseAge()`
- After collapse, refresh comes through `viewModelChanged` automatically.

## Challenges panel

Create one row per `ChallengeViewModel` in `challenges`.

- Title: `displayName`
- Description: `description`
- Goals: `completionGoalLabel`
- Modifier: `modifierLabel`
- Reward: `rewardLabel`
- Completions: `completionLabel`
- Active marker: `active`
- Start button interactable: `canStart`
- Start button action: `KingdomGameController.StartChallenge((int)challengeId)`
- Complete button interactable: `canComplete`
- Complete button action: `KingdomGameController.CompleteActiveChallenge()`

## Settings/save panel

- Save button: `KingdomGameController.Save()`
- Load button: `KingdomGameController.Load()`
- New game button: `KingdomGameController.NewGame()`
- Delete local save debug/settings button: `KingdomGameController.DeleteSave()`
- Export button: `KingdomGameController.ExportSaveText()`
- Export text field: latest `exportTextCreated` payload or `LastExportText`
- Import input field: player-provided save text
- Import button: `KingdomGameController.ImportSaveText(inputField.text)`
- Error label: latest `errorChanged` payload

## Optional debug panel

Use `KingdomDebugController` only in prototype/development scenes.

- Grant resource button: `KingdomDebugController.GrantResource(resourceId)`
- Grant all resources button: `KingdomDebugController.GrantAllResources()`
- Collapse setup button: `KingdomDebugController.GrantCollapseTestResources()`
- Challenge setup button: `KingdomDebugController.GrantActiveChallengeGoals()`
- Delete local save button: `KingdomDebugController.DeleteLocalSave()`
- Generate balance report button: `KingdomDebugController.GenerateBalanceReport()`
- Balance report text field: latest `balanceReportGenerated` payload

## First playable acceptance pass

Use `manual-qa-checklist.md` for the full pass. The minimum smoke test is:

- Start with no save and see initial resources.
- Buy Farms and Lumber Camps from the Buildings panel.
- Watch Food and Wood amounts/rates update.
- Save, reload the scene, and confirm progress remains.
- Close/reopen after elapsed real time and confirm offline progress appears.
- Export a save, start a new game, import the save, and confirm progress returns.
- Collapse once and confirm loop resources/buildings reset while Legacy remains.
- Start a challenge and confirm active challenge labels/buttons update.
