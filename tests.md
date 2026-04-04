# Tests

## Overview

| Project | File | Framework | What it tests |
|---------|------|-----------|---------------|
| `subs2srs.Tests` | `UtilsSubsTests.cs` | xUnit | Time formatting, padding, overlap, roundtrip |
| `subs2srs.Tests` | `UtilsNameTests.cs` | xUnit | Token replacement, zero-padding, escape chars |
| `subs2srs.Tests` | `PrefIOTests.cs` | xUnit | JSON round-trip, defaults, migration, special chars |
| `subs2srs.Tests` | `ProjectIOTests.cs` | xUnit | `.s2s.json` save/load, corruption handling, transient fields |
| `subs2srs.Tests` | `SettingsSnapshotTests.cs` | xUnit | Deep copy, isolation, restore, defaults |
| `subs2srs.Tests` | `TimeShiftRuleTests.cs` | xUnit | Cascading lookup, edge cases, negative shifts |
| `subs2srs.Tests` | `SubsParserTests.cs` | xUnit | SRT/ASS/LRC parsing, error handling, resource disposal |

## Running

```bash
# All tests
make test

# Individual suite (via dotnet)
dotnet test subs2srs.Tests/subs2srs.Tests.csproj --filter "FullyQualifiedName~UtilsSubsTests"
```

## How they work

### xUnit suites
All tests use the standard `xunit` package. No external test frameworks.
- **Parallelization disabled**: `[assembly: CollectionBehavior(DisableTestParallelization = true)]` prevents race conditions on the mutable `Settings.Instance` singleton.
- **Singleton reset**: `Settings.Instance.reset()` called in constructor and `Dispose()` to isolate test state.
- **Temp directories**: `Path.GetTempPath()` + `Guid` creates isolated dirs. Cleaned up via `IDisposable.Dispose()`.
- **Mocking**: External CLI tools (`ffmpeg`, `ffprobe`) are not invoked. File I/O tests use real temp files.

## Test environment
- All tests create temporary directories via `Path.Combine(Path.GetTempPath(), ...)` and clean up in `Dispose()`
- No root privileges required
- No real media files, subtitles, or system paths are touched
- Tests run sequentially to avoid singleton pollution
