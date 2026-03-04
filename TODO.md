# TODO

## Medium priority

- [ ] **`UtilsCommon` — deduplicate process launching**
  `startFFmpegProgress()` and `getFFmpegText()` repeat the same
  "try rel → try abs → try PATH" pattern three times each (~160 lines of
  copy-paste). Extract into a single `tryRunProcess()` with a callback.

- [ ] **`PrefIO` — migrate to JSON**
  Current regex-based replacement is fragile. Read both formats, write JSON.
  Would break manual editing of `preferences.txt` — needs migration path.

- [ ] **`DateTime` → `TimeSpan` for durations and time offsets**
  `Settings.SpanStart/SpanEnd`, `InfoLine.StartTime/EndTime`,
  `UtilsSubs.getDurationTime`, `IProgressReporter.SetDuration` all use
  `DateTime` for values that represent durations, not points in time.
  Semantically wrong and will break on files longer than 24 hours
  (rare, but possible for audiobooks). Requires coordinated change
  across `UtilsName`, `UtilsSubs`, all parsers, and workers.

## Low priority

- [ ] **Unit tests**
  Zero tests currently. Start with `UtilsName.createName()`, `PrefIO.read()`,
  `UtilsSubs.applyTimePad()`.

- [ ] **Remove `SaveSettings` class**
  Serialize `Settings.Instance` directly with `[JsonIgnore]` on transient
  fields. `SaveSettings.gatherData()` and `Settings.loadSettings()` become
  unnecessary. Requires analysis of all `.s2s` consumers.

- [ ] **`Process` → async with `CancellationToken`**
  Replace `Thread.Sleep(100)` polling in `startFFmpegProgress()` with
  `process.WaitForExitAsync(token)`. Needs .NET 5+ (already satisfied).

- [ ] **GTK4 migration**
  GtkSharp for GTK4 is not yet stable. Revisit when upstream is ready.
