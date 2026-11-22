# subs2srs

## What changed

| Area | erjiang port | This port |
|---|---|---|
| UI toolkit | WinForms on Mono | **GTK4** via GirCore |
| Runtime | Mono | **.NET 10+** |
| System.Drawing | Required everywhere | **Removed** — `SrsColor`, `FontInfo` used instead |
| Serialization | `BinaryFormatter` | **System.Text.Json** (`ObjectCloner`) |
| Preferences format | Custom `key = value` text with regex updates | **JSON** (`preferences.json`) |
| Progress dialogs | `BackgroundWorker` + modal `DialogProgress` | **`async/await`** + `IProgressReporter` |
| PropertyGrid (Preferences) | WinForms `PropertyGrid` | **`ColumnView`** with editable cells |
| Preview dialog | `BackgroundWorker` (deadlocked on Wayland) | **`Task.Run` + `async`** |
| Font/Color pickers | WinForms dialogs | **`FontDialogButton` / `ColorDialogButton`** (native GTK4) |
| VobSub support | Built-in | **Optional** (compile with `EnableVobSub=true`) |
| MS fonts | Required fontconfig workaround | **Not needed** |
| Build system | mcs / xbuild | **`dotnet publish`** via Makefile |

### Removed components

- **SubsReTimer** — separate tool, not part of this port
- **DialogAbout** — removed (was WinForms bitmap-based)
- **DialogPreviewSnapshot** — merged into `DialogPreview`
- **DialogVideoDimensionsChooser** — removed (size set directly in settings)
- **GroupBoxCheck** — WinForms custom control, not needed in GTK

## Adding a new preference

1. Add default constant to `PrefDefaults`
2. Add property to `PreferencesData.cs` with default from `PrefDefaults`
3. Add delegating property to `ConstantSettings`
4. Add to `DialogPref.BuildPropTable()` + `DialogPref.SavePreferences()`
5. Add to `Logger.writeSettingsToLog()`
6. If the preference maps to `Settings.Instance`, add to `Settings.Reset()`

## Building with VobSub support

VobSub (`.sub`/`.idx`) parsing requires `System.Drawing.Common` and is
disabled by default. To enable:

```sh
dotnet publish subs2srs/subs2srs.csproj -c Release -p:EnableVobSub=true
```
