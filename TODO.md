# TODO

## Medium priority

- [x] **Unit tests**
  Added xUnit project with tests for `UtilsSubs` (time formatting, padding,
  overlap, roundtrip), `UtilsName.createName()`, `PrefIO` round-trip.

- [x] **`UtilsCommon` — deduplicate process launching**
  Done. `getExePaths()` centralizes "try rel → try abs → try PATH".

- [x] **`PrefIO` — migrate to JSON**
  Done. `PreferencesData` POCO serialized via `System.Text.Json`.
  Auto-migrates from old `preferences.txt` on first launch.
  Adding a new preference: 2 places instead of 7.

## Low priority

- [x] **Remove `SaveSettings` class**
  Done. `Settings.Snapshot()` / `Settings.RestoreFrom()` replace
  `SaveSettings.gatherData()` and `Settings.loadSettings()`.
  `Settings` now has `[JsonPropertyName]` attributes for `.s2s` compatibility.

- [x] **`Process` → async with `CancellationToken`**
  Done. `runProcessWithProgress()` uses `WaitForExitAsync(token)`.

- [x] **GTK4 migration**
  Done. Migrated from GtkSharp (GTK3) to GirCore.Gtk-4.0 0.7.0 (GTK4).
  ComboBoxText → DropDown, TreeView/ListStore → ListView/ListStore,
  FontButton/ColorButton → FontDialogButton/ColorDialogButton,
  FileChooserDialog → FileDialog (async), Image+Pixbuf → Picture,
  PackStart/PackEnd → Append, Application.Invoke → GLib.Functions.IdleAdd.
