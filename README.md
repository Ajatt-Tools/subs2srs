# subs2srs (GTK3 port)

A tool that creates [Anki](https://apps.ankiweb.net/) flashcards from movies
and TV shows with subtitles, for language learning.

This is a **GTK3 / .NET 10** rewrite of the UI layer.
The processing core (subtitle parsing, ffmpeg calls, SRS generation)
is carried over from the original with minimal changes.

## Credits

- [Christopher Brochtrup](https://sourceforge.net/projects/subs2srs/) — original author
- [erjiang](https://github.com/erjiang/subs2srs) — Linux/Mono port
- [nihil-admirari](https://github.com/nihil-admirari/subs2srs-net48-builds) — updated dependencies

## What changed from the Mono/WinForms version

| Area | Old (erjiang fork) | This port |
|---|---|---|
| UI toolkit | WinForms on Mono | **GTK3** via GtkSharp |
| Runtime | Mono | **.NET 10+** |
| System.Drawing | Required everywhere | **Removed** — `SrsColor`, `FontInfo` used instead |
| Serialization | `BinaryFormatter` | **System.Text.Json** (`ObjectCloner`) |
| Progress dialogs | `BackgroundWorker` + modal `DialogProgress` | **`async/await`** + `IProgressReporter` |
| PropertyGrid (Preferences) | WinForms `PropertyGrid` | **`TreeView`** with editable cells |
| Preview dialog | `BackgroundWorker` (deadlocked on Wayland) | **`Task.Run` + `async`** |
| Font/Color pickers | WinForms dialogs | **`FontButton` / `ColorButton`** (native GTK) |
| VobSub support | Built-in | **Optional** (compile with `EnableVobSub=true`) |
| MS fonts | Required fontconfig workaround | **Not needed** |
| Build system | mcs / xbuild | **`dotnet publish`** via Makefile |

### Removed components

- **SubsReTimer** — separate tool, not part of this port
- **DialogAbout** — removed (was WinForms bitmap-based)
- **DialogPreviewSnapshot** — merged into `DialogPreview`
- **DialogVideoDimensionsChooser** — removed (size set directly in settings)
- **GroupBoxCheck** — WinForms custom control, not needed in GTK

### Post-port cleanup

**Bug fixes:**
- `SaveSettings.gatherData()` — `ContextLeadingIncludeSnapshots` was copied from `AudioClips` instead of `Snapshots`
- `WorkerSrs.genSrs()` — `TextWriter` without `using`, file descriptor leak on exception
- `SubsProcessor.DoWork()` — empty `catch {}` silently swallowed VobSub copy errors
- `Logger.flush()` — `StringBuilder` reset outside `lock`, race condition with `append()` under `Parallel.ForEach`
- `Logger.append()` — no synchronization, concurrent calls could corrupt `StringBuilder`
- `Logger` constructor / `writeFileToLog()` — `StreamWriter`/`StreamReader` without `using`
- `MainWindow.LoadSettings()` — called after Preferences dialog, resetting current session widget state to defaults
- `PrefIO.read()` — `DefaultRemoveStyledLinesSubs2` default was `Subs1`; `VobsubFilenameFormat` default was `VideoFilenameFormat`
- `PrefIO.writeString()` — regex replacement broke on keys containing regex metacharacters
- `UtilsName.createName()` — `${width}` and `${height}` tokens replaced with `subs2Text` instead of actual dimensions
- Audio stream number stored as combo box index instead of ffmpeg stream identifier — multi-stream MKV files produced empty audio clips
- Episode change in Preview dialog triggered infinite re-entrant loop (missing guard), causing 100% CPU
- Audio stream combo not populated when video path uses a glob pattern (`*.mkv`)
- `File.Move` in workers without `overwrite: true` — atomic rename could throw on interrupted retry
- `UtilsCommon.getExePaths` — substring `Contains` check could false-match partial path segments; now uses exact `HashSet` match
- `UtilsVideo.formatStartTimeArg/formatDurationArg` — trailing dots in format specifiers (`{0:00.}`) produced malformed ffmpeg `-ss`/`-t` arguments
- `WorkerAudio` — shared temp file path across episodes caused collisions on retry; error dialogs shown after user cancellation; source file deletion race during `Parallel.ForEach` cancellation

**Architecture:**
- `ConstantSettings` → `Settings.Instance` synchronization moved from `MainWindow.LoadSettings()` into `SaveSettings` constructor — adding a new preference no longer requires manual sync in 6 places
- Legacy per-key `PrefIO.getString/getBool/getInt/getFloat` methods removed (were `[Obsolete]`, unused)
- `GtkSynchronizationContext` — routes `async/await` continuations to the GTK main loop; without it, code after `await Task.Run(...)` ran on thread-pool threads, causing GTK threading violations
- `GLibLogFilter` — writer-level GLib log filter (`g_log_set_writer_func`) suppresses harmless `toggle_ref` warnings from GtkSharp GC finalizer
- Preview dialog Go button delegates to `MainWindow.OnGoClicked` via event — single processing path for both main window and preview
- Preview window reused (hide/show) instead of destroyed on close — avoids widget recreation and pixbuf leaks
- `IProgressReporter` implementations use poll-based `GLib.Timeout` instead of `Application.Invoke` — thread-safe, no cross-thread GTK calls

**Performance:**
- `PrefIO.read()` — read preferences file ~70 times → single pass into dictionary
- Workers skip existing output files — interrupted runs resume without re-extracting
- `WorkerVideo` — skip expensive video conversion when all clips for an episode already exist
- Audio/snapshot/video clip generation parallelised with `Parallel.ForEach` (configurable via `max_parallel_tasks`)

**Reliability:**
- Workers write to `.tmp` file then rename — incomplete files from crashes cannot be mistaken for finished output
- `UtilsMsg` — errors and info messages always echo to `stderr` for terminal visibility
- Unhandled exceptions and unobserved task exceptions logged to both `stderr` and log file

**UX:**
- `OnVideoChanged` — ffprobe runs off UI thread, no longer freezes interface on large files or network paths

**Refactoring:**
- `DialogProgress` static wrapper class removed — `IProgressReporter` used directly
- `AudioClips.filePattern` renamed to `FilePattern` with `[JsonPropertyName]` for `.s2s` compatibility
- `PrefIO` — `StreamReader`/`StreamWriter` → `File.ReadAllText`/`WriteAllText`; create `preferences.txt` on first launch
- `Settings.cs` — all model classes (`SubSettings`, `AudioClips`, `VideoClips`, `Snapshots`, `SaveSettings`, etc.) converted to auto-properties
- `ConstantSettings` — 130 backing field + property pairs → auto-properties (~400 lines removed)
- `InfoCombined`, `InfoLine` — auto-properties, remove `[Serializable]`
- `ObjectCloner` — remove `IncludeFields` (no longer needed with auto-properties)
- `UtilsName` — per-call mutable fields eliminated, state passed via parameters (thread-safe); compiled `Regex` cached as `static readonly`
- `WorkerVars` — backing fields → auto-properties
- `PropertyBag` — removed `ICustomTypeDescriptor` (WinForms `PropertyGrid` leftover), `ArrayList`/`Hashtable` → generics
- `LangaugeSpecific` → `LanguageSpecific` (typo fix across all files, `[JsonPropertyName]` for `.s2s` compat)
- `[Serializable]` / `[NonSerialized]` → removed / `[JsonIgnore]` (unused since `BinaryFormatter` → `System.Text.Json`)
- `DateTime` → `TimeSpan` for all duration/position values (`InfoLine`, `Settings.SpanStart/SpanEnd`, `UtilsSubs`, parsers, workers, dialogs) — semantically correct, supports files >24h
- `Logger` — `Mutex` → `lock` (single-process, cannot leak)
- `PrefIO` — legacy per-key read methods removed
- `new string[0]` → `Array.Empty<string>()` everywhere
- `String.Format` → string interpolation throughout
- Unused `using` directives removed
- Typos: `progessCount` → `progressCount`, `initalized` → `initialized`, `Creeate` → `Create`, `necassary` → `necessary`

## Dependencies

**Runtime:**
- [.NET 10+](https://dotnet.microsoft.com/) runtime
- [GTK 3](https://gtk.org/)
- [ffmpeg](https://ffmpeg.org/)
- [mp3gain](https://mp3gain.sourceforge.net/) *(only if using audio normalization)*
- [mkvtoolnix](https://mkvtoolnix.download/) (`mkvextract`, `mkvinfo`) *(only for MKV track extraction)*

**Build:**
- [.NET 10+ SDK](https://dotnet.microsoft.com/)

**Optional:**
- [noto-fonts-cjk](https://github.com/notofonts/noto-cjk) — for Japanese/Chinese/Korean text

## Build

```sh
make build
```

## Install

### Arch Linux (AUR)

```sh
yay -S subs2srs-gtk3-git
```

### Manual

```sh
git clone https://gitlab.com/fkzys/subs2srs-gtk3.git
cd subs2srs-gtk3
sudo make install
```

Installs to `/usr/lib/subs2srs/`, launcher to `/usr/bin/subs2srs`.

### Uninstall

```sh
sudo make uninstall
```

## Configuration

On first run, `preferences.txt` is created in
`~/.config/subs2srs/` (or `$XDG_CONFIG_HOME/subs2srs/`).

Edit via **Preferences** dialog or manually.

### Adding a new preference

1. Add entry to `PrefIO.writeDefaultPreferences()`
2. Add default constant to `PrefDefaults`
3. Add mutable property to `ConstantSettings`
4. Add read logic to `PrefIO.read()`
5. Add to `DialogPref.BuildPropTable()`
6. Add to `DialogPref.SavePreferences()`
7. Add to `Logger.writeSettingsToLog()`
8. If the preference maps to `Settings.Instance`, add to `SaveSettings` constructor

### Parallelism

Set `max_parallel_tasks` in Preferences → Misc (or in `preferences.txt`):
- `0` — auto (number of CPU cores, default)
- `1` — sequential (no parallelism)
- `N` — use up to N threads for media generation

## Building with VobSub support

VobSub (`.sub`/`.idx`) parsing requires `System.Drawing.Common` and is
disabled by default. To enable:

```sh
dotnet publish subs2srs/subs2srs.csproj -c Release -p:EnableVobSub=true
```

## License

[GPL-3.0-or-later](https://www.gnu.org/licenses/gpl-3.0.html)
