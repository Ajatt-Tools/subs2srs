# TODO

## Medium priority

- [x] **Unit tests**
  Added xUnit project with tests for `UtilsSubs` (time formatting, padding,
  overlap, roundtrip), `UtilsName.createName()`, `PrefIO.convertFromTokens()`.

- [x] **`UtilsCommon` — deduplicate process launching**
  Done. `getExePaths()` centralizes "try rel → try abs → try PATH".

- [ ] **`PrefIO` — migrate to JSON**
  Current regex-based replacement is fragile. Read both formats, write JSON.
  Would break manual editing of `preferences.txt` — needs migration path.

## Low priority

- [ ] **Remove `SaveSettings` class**
  Serialize `Settings.Instance` directly with `[JsonIgnore]` on transient
  fields. `SaveSettings.gatherData()` and `Settings.loadSettings()` become
  unnecessary. Requires analysis of all `.s2s` consumers.

- [x] **`Process` → async with `CancellationToken`**
  Done. `runProcessWithProgress()` uses `WaitForExitAsync(token)`.

- [ ] **GTK4 migration**
  GtkSharp for GTK4 is not yet stable. Revisit when upstream is ready.
