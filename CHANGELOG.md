# Changelog

## 0.2.3

**Preview dialog — ColumnView + multi-selection:**
- `Gtk.ListView` replaced with `Gtk.ColumnView` — five resizable columns (Subs1, Subs2, Start, End, Duration) with native drag-resize handles
- `Gtk.SingleSelection` replaced with `Gtk.MultiSelection` — activate/deactivate now applies to all selected rows
- "Select All", "Select None", "Invert" buttons added to the action bar
- Find Next now searches from the currently selected row instead of a separate `_findIdx` counter; wraps around at end of list
- Column resize uses P/Invoke (`gtk_column_view_column_set_resizable`, `set_fixed_width`, `set_expand`) because gir.core 0.7.0 does not expose these APIs
- Layout rule: fixed_width columns are resizable, last column uses expand — never both on the same column (prevents GTK4 layout fight during drag)
- CSS selectors updated from `listview > row` to `columnview listview > row` for correct selection styling; zero-gap cell padding added

**Shift rules — ColumnView with editable cells:**
- Per-episode shift rules table migrated from `Gtk.ListView` with manual header labels to `Gtk.ColumnView` with three columns ("From Episode", "Subs1 Shift (ms)", "Subs2 Shift (ms)")
- Editable `Gtk.Entry` cells use `ShiftRuleRef` mutable reference wrapper — `OnChanged` handler installed once in `OnSetup`, target swapped on each `OnBind`, nulled on `OnUnbind` to prevent stale writes

**Episode End # limit:**
- New `Episode End #` spin button on the Subs tab (0 = process all episodes)
- `Settings.EpisodeEndNumber` property added, persisted in `.s2s.json` and preferences
- File arrays (`Subs[0].Files`, `Subs[1].Files`, `VideoClips.Files`, `AudioClips.Files`) truncated to `endNum - startNum + 1` entries when Episode End # is set
- Preview episode combo uses `Settings.Instance.Subs[0].Files.Length` (already truncated) instead of re-globbing via `getNumSubsFiles()`

**New file:**
- `GtkColumnViewHelper.cs` — static P/Invoke helpers for `gtk_column_view_column_set_resizable`, `set_fixed_width`, `set_expand`

**Bug fixes:**
- Preview `_running` flag reset on `StartPreview()` — prevents stale lock when previous run was interrupted by window hide
- Indentation fix: `OnSnapshotClicked` XML-doc had one-space indent instead of standard eight-space

---

**GTK4 migration**
- UI toolkit migrated from GTK3 (GtkSharp) to **GTK4** (GirCore.Gtk-4.0 0.7.0)
- `ComboBoxText` replaced with `Gtk.DropDown` + `Gtk.StringList` throughout
- `TreeView` / `ListStore` replaced with `Gtk.ListView` + `Gio.ListStore` (Preferences, Preview, shift rules, actors)
- `FontButton` / `ColorButton` replaced with `FontDialogButton` / `ColorDialogButton` backed by `FontDialog` / `ColorDialog`
- `FileChooserDialog` replaced with `Gtk.FileDialog` async API (`OpenAsync`, `SaveAsync`, `SelectFolderAsync`)
- `Gio.FileHelper.NewForPath()` used for `FileDialog.InitialFolder` / `InitialFile` setup
- `Gtk.Image` + `Pixbuf` replaced with `Gtk.Picture` for snapshot preview
- `Box.PackStart` / `PackEnd` replaced with `Box.Append`
- `ShowAll()` removed — GTK4 widgets are visible by default
- `Destroyed` signal replaced with `OnCloseRequest` for window lifecycle
- `Application.Invoke()` / `GLib.Idle.Add()` replaced with `GLib.Functions.IdleAdd()`
- `GLib.Timeout.Add()` replaced with `GLib.Functions.TimeoutAdd()` for poll-based progress
- `GtkSynchronizationContext` updated to use `GLib.Functions.IdleAdd()` for `async/await` continuations
- `GLibLogFilter` (`g_log_set_writer_func`) removed — not needed with GirCore (GtkSharp `toggle_ref` warnings no longer occur)
- Dialog setup uses `SetModal(true)` + `SetTransientFor(parent)` methods (GirCore binding style)

**Preferences: JSON migration**
- `PrefIO` rewritten — preferences stored as `preferences.json` instead of custom `key = value` text format
- `PreferencesData` POCO added as single serializable source of truth for all preferences
- `ConstantSettings` mutable properties now delegate to `PreferencesData` backing store
- `DialogPref.SavePreferences()` writes directly to `ConstantSettings` then calls `PrefIO.Write()`
- Auto-migration from `preferences.txt` on first launch (old file left intact)
- Removed: `SettingsPair`, `writeString()`, `writeDefaultPreferences()`, `Tok()`, `convertFromTokens()`, `convertToTokens()`, `convertOut()`, `preventBlank()`
- Adding a new preference now requires 2 places instead of 7

**Features:**
- Per-episode cascading time shift rules — override global time shift per episode range; last rule where `FromEpisode ≤ episode` wins
- Snapshot JPEG quality control — configurable `ffmpeg -q:v` (1–31, default 3), persisted in Preferences and `.s2s` files; ignored for PNG output
- Audio track title in stream picker — shows container metadata title (e.g. "Commentary", "Original Soundtrack") via ffprobe JSON; falls back to ffmpeg regex parsing
- Audio stream consistency validation — warns before processing when selected audio stream has mismatched language or commentary track across episodes
- Output directory remembered between sessions — persisted as `default_output_dir` preference, auto-updated on Go
- Save/Load Project in `.s2s.json` format (File → Save / File → Load)

**Bug fixes:**
- `Settings.Reset()` (was `SaveSettings.gatherData()`) — `ContextLeadingIncludeSnapshots` was copied from `AudioClips` instead of `Snapshots`
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
- `UtilsSubs.timeToString/formatAssTime` — same trailing-dot format specifier bug (`{0:00.}`) produced `"00.:01.:30."` / `"0:00.:36..16."` instead of `"0:01:30"` / `"0:00:36.16"`
- Audio bitrate combos reset to default instead of showing loaded value on startup
- Audio stream consistency false positives reduced (relaxed matching logic)
- `demuxAudioCopy` — input seeking (`-ss` before `-i`) caused keyframe drift; demuxed audio started earlier than `entireClipStartTime`, making `WorkerAudio` shift calculations wrong and clipping the end of every audio clip; fixed by moving `-ss` after `-i` (output seeking)

**Architecture:**
- `ConstantSettings` → `Settings.Instance` synchronization moved from `MainWindow.LoadSettings()` into `Settings.Reset()` — adding a new preference no longer requires manual sync in 6 places
- Legacy per-key `PrefIO.getString/getBool/getInt/getFloat` methods removed (were `[Obsolete]`, unused)
- `GtkSynchronizationContext` — routes `async/await` continuations to the GTK main loop; without it, code after `await Task.Run(...)` ran on thread-pool threads, causing GTK threading violations
- `GLibLogFilter` — writer-level GLib log filter (`g_log_set_writer_func`) suppresses harmless `toggle_ref` warnings from GtkSharp GC finalizer
- Preview dialog Go button delegates to `MainWindow.OnGoClicked` via event — single processing path for both main window and preview
- Preview window reused (hide/show) instead of destroyed on close — avoids widget recreation and pixbuf leaks
- `IProgressReporter` implementations use poll-based `GLib.Timeout` instead of `Application.Invoke` — thread-safe, no cross-thread GTK calls
- `IProgressReporter.Token` — exposes `CancellationToken` for cooperative cancellation; `Cancel` setter triggers `CancellationTokenSource`

**Performance:**
- `PrefIO.read()` — read preferences file ~70 times → single pass into dictionary
- Workers skip existing output files — interrupted runs resume without re-extracting
- `WorkerVideo` — skip expensive video conversion when all clips for an episode already exist
- Audio/snapshot/video clip generation parallelised with `Parallel.ForEach` (configurable via `max_parallel_tasks`)
- `runProcessWithProgress()` — `Thread.Sleep(100)` polling loop replaced with `WaitForExitAsync(token)` for proper async cancellation
- Audio extraction pipeline reworked: demux (stream copy) → decode to WAV (PCM) → parallel per-clip encode; sample-accurate cuts (±0.02ms vs ±13ms mp3 frame boundary); output seeking in demux phase for correct timing baseline
- Snapshot ffmpeg flags — added `-sn -dn -noaccurate_seek -threads 1` for faster single-frame extraction

**Reliability:**
- Workers write to `.tmp` file then rename — incomplete files from crashes cannot be mistaken for finished output
- `UtilsMsg` — errors and info messages always echo to `stderr` for terminal visibility
- Unhandled exceptions and unobserved task exceptions logged to both `stderr` and log file

**UX:**
- `OnVideoChanged` — ffprobe runs off UI thread, no longer freezes interface on large files or network paths
- `max_parallel_tasks` preference exposed in Preferences dialog

**Refactoring:**
- `DialogProgress` static wrapper class removed — `IProgressReporter` used directly
- `AudioClips.filePattern` renamed to `FilePattern` with `[JsonPropertyName]` for `.s2s` compatibility
- `PrefIO` — `StreamReader`/`StreamWriter` → `File.ReadAllText`/`WriteAllText`; create `preferences.txt` on first launch
- `Settings.cs` — all model classes (`SubSettings`, `AudioClips`, `VideoClips`, `Snapshots`, etc.) converted to auto-properties; `SaveSettings` class removed in favour of `Settings.Snapshot()`/`RestoreFrom()`
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
