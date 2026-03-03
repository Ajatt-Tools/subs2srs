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
- `Logger.flush()` — mutex never released if write throws (deadlock)
- `Logger` constructor / `writeFileToLog()` — `StreamWriter`/`StreamReader` without `using`
- `PrefIO.read()` — `DefaultRemoveStyledLinesSubs2` default was `Subs1`; `VobsubFilenameFormat` default was `VideoFilenameFormat`
- `PrefIO.writeString()` — regex replacement broke on keys containing regex metacharacters
- `UtilsName.createName()` — `${width}` and `${height}` tokens replaced with `subs2Text` instead of actual dimensions
- Audio stream number stored as combo box index instead of ffmpeg stream identifier — multi-stream MKV files produced empty audio clips
- Episode change in Preview dialog triggered infinite re-entrant loop (missing guard), causing 100% CPU
- Audio stream combo not populated when video path uses a glob pattern (`*.mkv`)

**Performance:**
- `PrefIO.read()` — read preferences file ~70 times → single pass into dictionary
- Workers skip existing output files — interrupted runs resume without re-extracting
- `WorkerVideo` — skip expensive video conversion when all clips for an episode already exist
- Audio/snapshot/video clip generation parallelised with `Parallel.ForEach` (configurable via `max_parallel_tasks`)

**Reliability:**
- Workers write to `.tmp` file then rename — incomplete files from crashes cannot be mistaken for finished output
- `UtilsMsg` — errors and info messages always echo to `stderr` for terminal visibility
- Unhandled exceptions and unobserved task exceptions logged to both `stderr` and log file

**Refactoring:**
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
- `Logger` — `Mutex` → `lock` (single-process, cannot leak)
- `PrefIO` — legacy per-key read methods marked `[Obsolete]`
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
