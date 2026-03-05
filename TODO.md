# TODO

## Medium priority

- [ ] **Unit tests**
  Zero tests currently. Start with `UtilsName.createName()`, `PrefIO.read()`,
  `UtilsSubs.applyTimePad()`.

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

- [ ] **`Process` → async with `CancellationToken`**
  Replace `Thread.Sleep(100)` polling in `startFFmpegProgress()` with
  `process.WaitForExitAsync(token)`. Needs .NET 5+ (already satisfied).

- [ ] **GTK4 migration**
  GtkSharp for GTK4 is not yet stable. Revisit when upstream is ready.
