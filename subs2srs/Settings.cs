//  Copyright (C) 2009-2016 Christopher Brochtrup
//  Copyright (C) 2026 fkzys (GTK3/.NET 10 port)
//
//  This file is part of subs2srs.
//
//  subs2srs is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  subs2srs is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with subs2srs.  If not, see <http://www.gnu.org/licenses/>.
//
//////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace subs2srs
{
  public static class PrefDefaults
  {
    public const int MaxParallelTasks = 0; // 0 = auto (Environment.ProcessorCount)
    public const int MainWindowWidth = 614;
    public const int MainWindowHeight = 630;
    public const bool DefaultEnableAudioClipGeneration = true;
    public const bool DefaultEnableSnapshotsGeneration = true;
    public const bool DefaultEnableVideoClipsGeneration = false;
    public const string VideoPlayer = "";
    public const string VideoPlayerArgs = "";
    public const bool ReencodeBeforeSplittingAudio = false;
    public const bool EnableLogging = true;
    public const string AudioNormalizeArgs = "/f /q /r /k";
    public const int LongClipWarningSeconds = 10;
    public const int DefaultAudioClipBitrate = 128;
    public const bool DefaultAudioNormalize = false;
    public const int DefaultVideoClipVideoBitrate = 800;
    public const int DefaultVideoClipAudioBitrate = 128;
    public const int DefaultSnapshotJpegQuality = 3;
    public const bool DefaultIphoneSupport = false;
    public const string DefaultEncodingSubs1 = "utf-8";
    public const string DefaultEncodingSubs2 = "utf-8";
    public const int DefaultContextNumLeading = 0;
    public const int DefaultContextNumTrailing = 0;
    public const int DefaultContextLeadingRange = 15;
    public const int DefaultContextTrailingRange = 15;
    public const string DefaultFileBrowserStartDir = "";
    public const bool DefaultRemoveStyledLinesSubs1 = true;
    public const bool DefaultRemoveStyledLinesSubs2 = true;
    public const bool DefaultRemoveNoCounterpartSubs1 = true;
    public const bool DefaultRemoveNoCounterpartSubs2 = true;
    public const string DefaultIncludeTextSubs1 = "";
    public const string DefaultIncludeTextSubs2 = "";
    public const string DefaultExcludeTextSubs1 = "";
    public const string DefaultExcludeTextSubs2 = "";
    public const bool DefaultExcludeDuplicateLinesSubs1 = false;
    public const bool DefaultExcludeDuplicateLinesSubs2 = false;
    public const bool DefaultExcludeLinesFewerThanCharsSubs1 = false;
    public const bool DefaultExcludeLinesFewerThanCharsSubs2 = false;
    public const int DefaultExcludeLinesFewerThanCharsNumSubs1 = 8;
    public const int DefaultExcludeLinesFewerThanCharsNumSubs2 = 8;
    public const bool DefaultExcludeLinesShorterThanMsSubs1 = false;
    public const bool DefaultExcludeLinesShorterThanMsSubs2 = false;
    public const int DefaultExcludeLinesShorterThanMsNumSubs1 = 800;
    public const int DefaultExcludeLinesShorterThanMsNumSubs2 = 800;
    public const bool DefaultExcludeLinesLongerThanMsSubs1 = false;
    public const bool DefaultExcludeLinesLongerThanMsSubs2 = false;
    public const int DefaultExcludeLinesLongerThanMsNumSubs1 = 5000;
    public const int DefaultExcludeLinesLongerThanMsNumSubs2 = 5000;
    public const bool DefaultJoinSentencesSubs1 = true;
    public const bool DefaultJoinSentencesSubs2 = true;
    public const string DefaultJoinSentencesCharListSubs1 = ",、→";
    public const string DefaultJoinSentencesCharListSubs2 = ",、→";
    public const string SrsFilenameFormat = "${deck_name}.tsv";
    public const string SrsDelimiter = "\t";
    public const string SrsTagFormat = "${deck_name}_${0:episode_num}";
    public const string SrsSequenceMarkerFormat = "${0:episode_num}_${0:sequence_num}_${0:s_total_hour}.${0:s_min}.${0:s_sec}.${0:s_msec}";
    public const string SrsAudioFilenamePrefix = "[sound:";
    public const string SrsAudioFilenameSuffix = "]";
    public const string SrsSnapshotFilenamePrefix = "<img src=\"";
    public const string SrsSnapshotFilenameSuffix = "\">";
    public const string SrsVideoFilenamePrefix = "[sound:";
    public const string SrsVideoFilenameSuffix = "]";
    public const string SrsSubs1Format = "${subs1_line}";
    public const string SrsSubs2Format = "${subs2_line}";
    public const string SrsVobsubFilenamePrefix = "<img src=\"";
    public const string SrsVobsubFilenameFormat = "${deck_name}_${0:episode_num}_Stream_${0:stream_num}_${0:s_total_hour}.${0:s_min}.${0:s_sec}.${0:s_msec}-${0:e_total_hour}.${0:e_min}.${0:e_sec}.${0:e_msec}.png";
    public const string SrsVobsubFilenameSuffix = "\">";
    public const string AudioFilenameFormat = "${deck_name}_${0:episode_num}_${0:s_total_hour}.${0:s_min}.${0:s_sec}.${0:s_msec}-${0:e_total_hour}.${0:e_min}.${0:e_sec}.${0:e_msec}.mp3";
    public const string SnapshotFilenameFormat = "${deck_name}_${0:episode_num}_${0:m_total_hour}.${0:m_min}.${0:m_sec}.${0:m_msec}.jpg";
    public const string VideoFilenameFormat = "${deck_name}_${0:episode_num}_${0:s_total_hour}.${0:s_min}.${0:s_sec}.${0:s_msec}-${0:e_total_hour}.${0:e_min}.${0:e_sec}.${0:e_msec}";
    public const string VobsubFilenameFormat = "${deck_name}_${0:episode_num}_Stream_${0:stream_num}_${0:s_total_hour}.${0:s_min}.${0:s_sec}.${0:s_msec}-${0:e_total_hour}.${0:e_min}.${0:e_sec}.${0:e_msec}.png";
    public const string AudioId3Artist = "${deck_name}";
    public const string AudioId3Album = "${deck_name}_${0:episode_num}";
    public const string AudioId3Title = "${deck_name}_${0:episode_num}_${0:s_total_hour}.${0:s_min}.${0:s_sec}.${0:s_msec}-${0:e_total_hour}.${0:e_min}.${0:e_sec}.${0:e_msec}";
    public const string AudioId3Genre = "Speech";
    public const string AudioId3Lyrics = "${subs1_line} ${subs2_line}";
    public const string ExtractMediaAudioFilenameFormat = "${deck_name}_${0:episode_num}_${0:s_total_hour}.${0:s_min}.${0:s_sec}.${0:s_msec}-${0:e_total_hour}.${0:e_min}.${0:e_sec}.${0:e_msec}.mp3";
    public const string ExtractMediaLyricsSubs1Format = "[${2:s_total_min}:${2:s_sec}.${2:s_hsec}] ${subs1_line}";
    public const string ExtractMediaLyricsSubs2Format = "[${2:s_total_min}:${2:s_sec}.${2:s_hsec}] ${subs2_line}";
    public const string DuelingSubtitleFilenameFormat = "${deck_name}_${0:episode_num}.ass";
    public const string DuelingQuickRefFilenameFormat = "${deck_name}_${0:episode_num}.txt";
    public const string DuelingQuickRefSubs1Format = "[${0:s_total_hour}:${0:s_min}:${0:s_sec}.${0:s_hsec}]  ${subs1_line}";
    public const string DuelingQuickRefSubs2Format = "[${0:s_total_hour}:${0:s_min}:${0:s_sec}.${0:s_hsec}]  ${subs2_line}\n";
  }


  // Procedure for creating a new preference that can be set in the Preferences dialog:
  // 1) Create a default for the setting in PrefDefaults (above).
  // 2) Add property to PreferencesData.cs with default from PrefDefaults.
  // 3) Add delegating property to ConstantSettings (below).
  // 4) Add to DialogPref.BuildPropTable + DialogPref.SavePreferences.
  // 5) Add to Logger.writeSettingsToLog.
  // 6) For GUI settings (ones that map to Settings.Instance), add to SaveSettings constructor.

  public static class ConstantSettings
  {
    private static string FindInPath(string name)
    {
      foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
      {
        var full = Path.Combine(dir, name);
        if (File.Exists(full)) return full;
      }
      return name;
    }

    // ── Immutable (no setter) ──────────────────────────────────────────

    public static string SaveExt { get; } = "s2s";
    public static string HelpPage { get; } = "http://subs2srs.sourceforge.net/";
    public static string LogDir { get; } = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
      "subs2srs", "Logs") + Path.DirectorySeparatorChar;
    public static int MaxLogFiles { get; } = 10;

    public static string ExeFFmpeg { get; } = "ffmpeg";
    public static string PathFFmpegFullExe { get; } = FindInPath("ffmpeg");
    public static string PathFFmpegExe { get; } = FindInPath("ffmpeg");
    public static string PathFFmpegPresetsFull { get; } = Path.Combine(
      UtilsCommon.getAppDir(true), "presets");

    public static string TempImageFilename { get; } = $"subs2srs_temp_{Guid.NewGuid()}.jpg";
    public static string TempVideoFilename { get; } = $"subs2srs_temp_{Guid.NewGuid()}";
    public static string TempAudioFilename { get; } = $"subs2srs_temp_{Guid.NewGuid()}.mp3";
    public static string TempAudioPreviewFilename { get; } = $"subs2srs_temp_{Guid.NewGuid()}.wav";
    public static string TempPreviewDirName { get; } = $"subs2srs_preview_{Guid.NewGuid()}";
    public static string TempMkvExtractSubs1Filename { get; } = $"subs2srs_mkv_extract_subs1_{Guid.NewGuid()}";
    public static string TempMkvExtractSubs2Filename { get; } = $"subs2srs_mkv_extract_subs2_{Guid.NewGuid()}";

    public static string NormalizeAudioExe { get; } = "mp3gain";
    public static string PathNormalizeAudioExeRel { get; } = "mp3gain";
    public static string PathNormalizeAudioExeFull { get; } = FindInPath("mp3gain");
    public static string PathSubsReTimerFull { get; } = FindInPath("SubsReTimer");

    public static string ExeMkvInfo { get; } = "mkvinfo";
    public static string PathMkvDirRel { get; } = "";
    public static string PathMkvDirFull { get; } = "";
    public static string PathMkvInfoExeRel { get; } = "mkvinfo";
    public static string PathMkvInfoExeFull { get; } = FindInPath("mkvinfo");

    public static string ExeMkvExtract { get; } = "mkvextract";
    public static string PathMkvExtractExeRel { get; } = "mkvextract";
    public static string PathMkvExtractExeFull { get; } = FindInPath("mkvextract");

    public static string SettingsFilename { get; } = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
      "subs2srs", "preferences.txt");

    // ── Backing store for preferences (populated by PrefIO.read) ─────
    internal static PreferencesData Prefs { get; set; } = new();

    // ── Mutable — delegate to Prefs ──────────────────────────────────

    public static int MaxParallelTasks
    {
        get => Prefs.MaxParallelTasks;
        set => Prefs.MaxParallelTasks = value;
    }

    public static int EffectiveParallelism => MaxParallelTasks > 0
        ? MaxParallelTasks
        : Environment.ProcessorCount;

    public static int MainWindowWidth
    {
        get => Prefs.MainWindowWidth;
        set => Prefs.MainWindowWidth = value;
    }

    public static int MainWindowHeight
    {
        get => Prefs.MainWindowHeight;
        set => Prefs.MainWindowHeight = value;
    }

    public static bool DefaultEnableAudioClipGeneration
    {
        get => Prefs.DefaultEnableAudioClipGeneration;
        set => Prefs.DefaultEnableAudioClipGeneration = value;
    }

    public static bool DefaultEnableSnapshotsGeneration
    {
        get => Prefs.DefaultEnableSnapshotsGeneration;
        set => Prefs.DefaultEnableSnapshotsGeneration = value;
    }

    public static bool DefaultEnableVideoClipsGeneration
    {
        get => Prefs.DefaultEnableVideoClipsGeneration;
        set => Prefs.DefaultEnableVideoClipsGeneration = value;
    }

    public static string VideoPlayer
    {
        get => Prefs.VideoPlayer;
        set => Prefs.VideoPlayer = value;
    }

    public static string VideoPlayerArgs
    {
        get => Prefs.VideoPlayerArgs;
        set => Prefs.VideoPlayerArgs = value;
    }

    public static bool ReencodeBeforeSplittingAudio
    {
        get => Prefs.ReencodeBeforeSplittingAudio;
        set => Prefs.ReencodeBeforeSplittingAudio = value;
    }

    public static bool EnableLogging
    {
        get => Prefs.EnableLogging;
        set => Prefs.EnableLogging = value;
    }

    public static string AudioNormalizeArgs
    {
        get => Prefs.AudioNormalizeArgs;
        set => Prefs.AudioNormalizeArgs = value;
    }

    public static int LongClipWarningSeconds
    {
        get => Prefs.LongClipWarningSeconds;
        set => Prefs.LongClipWarningSeconds = value;
    }

    public static int DefaultAudioClipBitrate
    {
        get => Prefs.DefaultAudioClipBitrate;
        set => Prefs.DefaultAudioClipBitrate = value;
    }

    public static bool DefaultAudioNormalize
    {
        get => Prefs.DefaultAudioNormalize;
        set => Prefs.DefaultAudioNormalize = value;
    }

    public static int DefaultVideoClipVideoBitrate
    {
        get => Prefs.DefaultVideoClipVideoBitrate;
        set => Prefs.DefaultVideoClipVideoBitrate = value;
    }

    public static int DefaultVideoClipAudioBitrate
    {
        get => Prefs.DefaultVideoClipAudioBitrate;
        set => Prefs.DefaultVideoClipAudioBitrate = value;
    }

    public static int DefaultSnapshotJpegQuality
    {
        get => Prefs.DefaultSnapshotJpegQuality;
        set => Prefs.DefaultSnapshotJpegQuality = value;
    }

    public static bool DefaultIphoneSupport
    {
        get => Prefs.DefaultIphoneSupport;
        set => Prefs.DefaultIphoneSupport = value;
    }

    public static string DefaultEncodingSubs1
    {
        get => Prefs.DefaultEncodingSubs1;
        set => Prefs.DefaultEncodingSubs1 = value;
    }

    public static string DefaultEncodingSubs2
    {
        get => Prefs.DefaultEncodingSubs2;
        set => Prefs.DefaultEncodingSubs2 = value;
    }

    public static int DefaultContextNumLeading
    {
        get => Prefs.DefaultContextNumLeading;
        set => Prefs.DefaultContextNumLeading = value;
    }

    public static int DefaultContextNumTrailing
    {
        get => Prefs.DefaultContextNumTrailing;
        set => Prefs.DefaultContextNumTrailing = value;
    }

    public static int DefaultContextLeadingRange
    {
        get => Prefs.DefaultContextLeadingRange;
        set => Prefs.DefaultContextLeadingRange = value;
    }

    public static int DefaultContextTrailingRange
    {
        get => Prefs.DefaultContextTrailingRange;
        set => Prefs.DefaultContextTrailingRange = value;
    }

    public static string DefaultFileBrowserStartDir
    {
        get => Prefs.DefaultFileBrowserStartDir;
        set => Prefs.DefaultFileBrowserStartDir = value;
    }

    public static bool DefaultRemoveStyledLinesSubs1
    {
        get => Prefs.DefaultRemoveStyledLinesSubs1;
        set => Prefs.DefaultRemoveStyledLinesSubs1 = value;
    }

    public static bool DefaultRemoveStyledLinesSubs2
    {
        get => Prefs.DefaultRemoveStyledLinesSubs2;
        set => Prefs.DefaultRemoveStyledLinesSubs2 = value;
    }

    public static bool DefaultRemoveNoCounterpartSubs1
    {
        get => Prefs.DefaultRemoveNoCounterpartSubs1;
        set => Prefs.DefaultRemoveNoCounterpartSubs1 = value;
    }

    public static bool DefaultRemoveNoCounterpartSubs2
    {
        get => Prefs.DefaultRemoveNoCounterpartSubs2;
        set => Prefs.DefaultRemoveNoCounterpartSubs2 = value;
    }

    public static string DefaultIncludeTextSubs1
    {
        get => Prefs.DefaultIncludeTextSubs1;
        set => Prefs.DefaultIncludeTextSubs1 = value;
    }

    public static string DefaultIncludeTextSubs2
    {
        get => Prefs.DefaultIncludeTextSubs2;
        set => Prefs.DefaultIncludeTextSubs2 = value;
    }

    public static string DefaultExcludeTextSubs1
    {
        get => Prefs.DefaultExcludeTextSubs1;
        set => Prefs.DefaultExcludeTextSubs1 = value;
    }

    public static string DefaultExcludeTextSubs2
    {
        get => Prefs.DefaultExcludeTextSubs2;
        set => Prefs.DefaultExcludeTextSubs2 = value;
    }

    public static bool DefaultExcludeDuplicateLinesSubs1
    {
        get => Prefs.DefaultExcludeDuplicateLinesSubs1;
        set => Prefs.DefaultExcludeDuplicateLinesSubs1 = value;
    }

    public static bool DefaultExcludeDuplicateLinesSubs2
    {
        get => Prefs.DefaultExcludeDuplicateLinesSubs2;
        set => Prefs.DefaultExcludeDuplicateLinesSubs2 = value;
    }

    public static bool DefaultExcludeLinesFewerThanCharsSubs1
    {
        get => Prefs.DefaultExcludeLinesFewerThanCharsSubs1;
        set => Prefs.DefaultExcludeLinesFewerThanCharsSubs1 = value;
    }

    public static bool DefaultExcludeLinesFewerThanCharsSubs2
    {
        get => Prefs.DefaultExcludeLinesFewerThanCharsSubs2;
        set => Prefs.DefaultExcludeLinesFewerThanCharsSubs2 = value;
    }

    public static int DefaultExcludeLinesFewerThanCharsNumSubs1
    {
        get => Prefs.DefaultExcludeLinesFewerThanCharsNumSubs1;
        set => Prefs.DefaultExcludeLinesFewerThanCharsNumSubs1 = value;
    }

    public static int DefaultExcludeLinesFewerThanCharsNumSubs2
    {
        get => Prefs.DefaultExcludeLinesFewerThanCharsNumSubs2;
        set => Prefs.DefaultExcludeLinesFewerThanCharsNumSubs2 = value;
    }

    public static bool DefaultExcludeLinesShorterThanMsSubs1
    {
        get => Prefs.DefaultExcludeLinesShorterThanMsSubs1;
        set => Prefs.DefaultExcludeLinesShorterThanMsSubs1 = value;
    }

    public static bool DefaultExcludeLinesShorterThanMsSubs2
    {
        get => Prefs.DefaultExcludeLinesShorterThanMsSubs2;
        set => Prefs.DefaultExcludeLinesShorterThanMsSubs2 = value;
    }

    public static int DefaultExcludeLinesShorterThanMsNumSubs1
    {
        get => Prefs.DefaultExcludeLinesShorterThanMsNumSubs1;
        set => Prefs.DefaultExcludeLinesShorterThanMsNumSubs1 = value;
    }

    public static int DefaultExcludeLinesShorterThanMsNumSubs2
    {
        get => Prefs.DefaultExcludeLinesShorterThanMsNumSubs2;
        set => Prefs.DefaultExcludeLinesShorterThanMsNumSubs2 = value;
    }

    public static bool DefaultExcludeLinesLongerThanMsSubs1
    {
        get => Prefs.DefaultExcludeLinesLongerThanMsSubs1;
        set => Prefs.DefaultExcludeLinesLongerThanMsSubs1 = value;
    }

    public static bool DefaultExcludeLinesLongerThanMsSubs2
    {
        get => Prefs.DefaultExcludeLinesLongerThanMsSubs2;
        set => Prefs.DefaultExcludeLinesLongerThanMsSubs2 = value;
    }

    public static int DefaultExcludeLinesLongerThanMsNumSubs1
    {
        get => Prefs.DefaultExcludeLinesLongerThanMsNumSubs1;
        set => Prefs.DefaultExcludeLinesLongerThanMsNumSubs1 = value;
    }

    public static int DefaultExcludeLinesLongerThanMsNumSubs2
    {
        get => Prefs.DefaultExcludeLinesLongerThanMsNumSubs2;
        set => Prefs.DefaultExcludeLinesLongerThanMsNumSubs2 = value;
    }

    public static bool DefaultJoinSentencesSubs1
    {
        get => Prefs.DefaultJoinSentencesSubs1;
        set => Prefs.DefaultJoinSentencesSubs1 = value;
    }

    public static bool DefaultJoinSentencesSubs2
    {
        get => Prefs.DefaultJoinSentencesSubs2;
        set => Prefs.DefaultJoinSentencesSubs2 = value;
    }

    public static string DefaultJoinSentencesCharListSubs1
    {
        get => Prefs.DefaultJoinSentencesCharListSubs1;
        set => Prefs.DefaultJoinSentencesCharListSubs1 = value;
    }

    public static string DefaultJoinSentencesCharListSubs2
    {
        get => Prefs.DefaultJoinSentencesCharListSubs2;
        set => Prefs.DefaultJoinSentencesCharListSubs2 = value;
    }

    public static string SrsFilenameFormat
    {
        get => Prefs.SrsFilenameFormat;
        set => Prefs.SrsFilenameFormat = value;
    }

    public static string SrsDelimiter
    {
        get => Prefs.SrsDelimiter;
        set => Prefs.SrsDelimiter = value;
    }

    public static string SrsTagFormat
    {
        get => Prefs.SrsTagFormat;
        set => Prefs.SrsTagFormat = value;
    }

    public static string SrsSequenceMarkerFormat
    {
        get => Prefs.SrsSequenceMarkerFormat;
        set => Prefs.SrsSequenceMarkerFormat = value;
    }

    public static string SrsAudioFilenamePrefix
    {
        get => Prefs.SrsAudioFilenamePrefix;
        set => Prefs.SrsAudioFilenamePrefix = value;
    }

    public static string SrsAudioFilenameSuffix
    {
        get => Prefs.SrsAudioFilenameSuffix;
        set => Prefs.SrsAudioFilenameSuffix = value;
    }

    public static string AudioFilenameFormat
    {
        get => Prefs.AudioFilenameFormat;
        set => Prefs.AudioFilenameFormat = value;
    }

    public static string AudioId3Artist
    {
        get => Prefs.AudioId3Artist;
        set => Prefs.AudioId3Artist = value;
    }

    public static string AudioId3Album
    {
        get => Prefs.AudioId3Album;
        set => Prefs.AudioId3Album = value;
    }

    public static string AudioId3Title
    {
        get => Prefs.AudioId3Title;
        set => Prefs.AudioId3Title = value;
    }

    public static string AudioId3Genre
    {
        get => Prefs.AudioId3Genre;
        set => Prefs.AudioId3Genre = value;
    }

    public static string AudioId3Lyrics
    {
        get => Prefs.AudioId3Lyrics;
        set => Prefs.AudioId3Lyrics = value;
    }

    public static string SrsSnapshotFilenamePrefix
    {
        get => Prefs.SrsSnapshotFilenamePrefix;
        set => Prefs.SrsSnapshotFilenamePrefix = value;
    }

    public static string SrsSnapshotFilenameSuffix
    {
        get => Prefs.SrsSnapshotFilenameSuffix;
        set => Prefs.SrsSnapshotFilenameSuffix = value;
    }

    public static string SnapshotFilenameFormat
    {
        get => Prefs.SnapshotFilenameFormat;
        set => Prefs.SnapshotFilenameFormat = value;
    }

    public static string SrsVideoFilenamePrefix
    {
        get => Prefs.SrsVideoFilenamePrefix;
        set => Prefs.SrsVideoFilenamePrefix = value;
    }

    public static string SrsVideoFilenameSuffix
    {
        get => Prefs.SrsVideoFilenameSuffix;
        set => Prefs.SrsVideoFilenameSuffix = value;
    }

    public static string VideoFilenameFormat
    {
        get => Prefs.VideoFilenameFormat;
        set => Prefs.VideoFilenameFormat = value;
    }

    public static string SrsSubs1Format
    {
        get => Prefs.SrsSubs1Format;
        set => Prefs.SrsSubs1Format = value;
    }

    public static string SrsSubs2Format
    {
        get => Prefs.SrsSubs2Format;
        set => Prefs.SrsSubs2Format = value;
    }

    public static string SrsVobsubFilenamePrefix
    {
        get => Prefs.SrsVobsubFilenamePrefix;
        set => Prefs.SrsVobsubFilenamePrefix = value;
    }

    public static string SrsVobsubFilenameSuffix
    {
        get => Prefs.SrsVobsubFilenameSuffix;
        set => Prefs.SrsVobsubFilenameSuffix = value;
    }

    public static string VobsubFilenameFormat
    {
        get => Prefs.VobsubFilenameFormat;
        set => Prefs.VobsubFilenameFormat = value;
    }

    public static string ExtractMediaAudioFilenameFormat
    {
        get => Prefs.ExtractMediaAudioFilenameFormat;
        set => Prefs.ExtractMediaAudioFilenameFormat = value;
    }

    public static string ExtractMediaLyricsSubs1Format
    {
        get => Prefs.ExtractMediaLyricsSubs1Format;
        set => Prefs.ExtractMediaLyricsSubs1Format = value;
    }

    public static string ExtractMediaLyricsSubs2Format
    {
        get => Prefs.ExtractMediaLyricsSubs2Format;
        set => Prefs.ExtractMediaLyricsSubs2Format = value;
    }

    public static string DuelingSubtitleFilenameFormat
    {
        get => Prefs.DuelingSubtitleFilenameFormat;
        set => Prefs.DuelingSubtitleFilenameFormat = value;
    }

    public static string DuelingQuickRefFilenameFormat
    {
        get => Prefs.DuelingQuickRefFilenameFormat;
        set => Prefs.DuelingQuickRefFilenameFormat = value;
    }

    public static string DuelingQuickRefSubs1Format
    {
        get => Prefs.DuelingQuickRefSubs1Format;
        set => Prefs.DuelingQuickRefSubs1Format = value;
    }

    public static string DuelingQuickRefSubs2Format
    {
        get => Prefs.DuelingQuickRefSubs2Format;
        set => Prefs.DuelingQuickRefSubs2Format = value;
    }
  }

  /// <summary>
  /// Cascading time shift rule: from a given episode number onward,
  /// apply the specified shift in milliseconds.
  /// Rules are sorted by FromEpisode; lookup finds the last rule where FromEpisode ≤ episode.
  /// </summary>
  public class TimeShiftRule
  {
    public int FromEpisode { get; set; }
    public int ShiftMs { get; set; }

    public TimeShiftRule() { }
    public TimeShiftRule(int from, int shift) { FromEpisode = from; ShiftMs = shift; }
  }

  public class SubSettings
  {
    public string FilePattern { get; set; } = "";

    [JsonIgnore]
    public string[] Files { get; set; } = Array.Empty<string>();

    public InfoStream VobsubStream { get; set; }
    public bool TimingsEnabled { get; set; }
    public int TimeShift { get; set; }
    public string[] IncludedWords { get; set; } = Array.Empty<string>();
    public string[] ExcludedWords { get; set; } = Array.Empty<string>();
    public bool RemoveNoCounterpart { get; set; } = true;
    public bool RemoveStyledLines { get; set; } = true;
    public bool ExcludeDuplicateLinesEnabled { get; set; }
    public bool ExcludeFewerEnabled { get; set; }
    public int ExcludeFewerCount { get; set; } = 8;
    public bool ExcludeShorterThanTimeEnabled { get; set; }
    public int ExcludeShorterThanTime { get; set; } = 800;
    public bool ExcludeLongerThanTimeEnabled { get; set; }
    public int ExcludeLongerThanTime { get; set; } = 5000;
    public bool JoinSentencesEnabled { get; set; } = true;
    public string JoinSentencesCharList { get; set; } = ",、→";
    public bool ActorsEnabled { get; set; }
    public string Encoding { get; set; } = "utf-8";
    /// <summary>
    /// Per-episode time shift overrides (cascading rules).
    /// When non-empty, overrides the global TimeShift for matched episodes.
    /// Sorted by FromEpisode ascending.
    /// </summary>
    public List<TimeShiftRule> TimeShiftRules { get; set; } = new();

    /// <summary>
    /// Get the effective time shift for a given episode number.
    /// Cascading: last rule where FromEpisode ≤ episode wins.
    /// Falls back to global TimeShift when no rules are defined.
    /// </summary>
    public int GetEffectiveTimeShift(int episodeNumber)
    {
      if (TimeShiftRules == null || TimeShiftRules.Count == 0)
        return TimeShift;

      int shift = TimeShift;
      foreach (var rule in TimeShiftRules)
      {
        if (rule.FromEpisode <= episodeNumber)
          shift = rule.ShiftMs;
        else
          break;
      }
      return shift;
    }
  }


  public class ImageSize
  {
    public int Width { get; set; }
    public int Height { get; set; }

    public ImageSize() { }

    public ImageSize(int width, int height)
    {
      Width = width;
      Height = height;
    }
  }


  public class ImageCrop
  {
    public int Top { get; set; }
    public int Bottom { get; set; }
    public int Left { get; set; }
    public int Right { get; set; }

    public ImageCrop() { }

    public ImageCrop(int top, int bottom, int left, int right)
    {
      Top = top;
      Bottom = bottom;
      Left = left;
      Right = right;
    }
  }


  public class VideoClips
  {
    public bool Enabled { get; set; }
    public string FilePattern { get; set; } = "";
    public InfoStream AudioStream { get; set; }

    [JsonIgnore]
    public string[] Files { get; set; } = Array.Empty<string>();

    public ImageSize Size { get; set; } = new ImageSize(240, 160);
    public int BitrateVideo { get; set; } = 700;
    public int BitrateAudio { get; set; } = 128;
    public bool PadEnabled { get; set; }
    public int PadStart { get; set; } = 250;
    public int PadEnd { get; set; } = 250;
    public ImageCrop Crop { get; set; } = new ImageCrop();
    public bool IPodSupport { get; set; }
  }


  public class AudioClips
  {
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("filePattern")]
    public string FilePattern { get; set; } = "";

    [JsonIgnore]
    public string[] Files { get; set; } = Array.Empty<string>();

    public bool PadEnabled { get; set; }
    public int PadStart { get; set; } = 250;
    public int PadEnd { get; set; } = 250;
    public int Bitrate { get; set; } = 128;
    public bool UseAudioFromVideo { get; set; } = true;
    public bool UseExistingAudio { get; set; }
    public bool Normalize { get; set; }
  }


  public class Snapshots
  {
    public bool Enabled { get; set; } = true;
    public ImageSize Size { get; set; } = new ImageSize(240, 160);
    public ImageCrop Crop { get; set; } = new ImageCrop();

    /// <summary>
    /// JPEG quality for ffmpeg -q:v flag (1=best, 31=worst).
    /// Ignored when output format is PNG.
    /// </summary>
    public int Quality { get; set; } = 3;
  }


#if ENABLE_VOBSUB
  public class VobSubColors
  {
    public bool Enabled { get; set; }
    public System.Drawing.Color[] Colors { get; set; }
    public bool[] TransparencyEnabled { get; set; }

    public VobSubColors()
    {
      Colors = new System.Drawing.Color[4];
      Colors[0] = System.Drawing.Color.FromArgb(253, 253, 253);
      Colors[1] = System.Drawing.Color.FromArgb(189, 189, 189);
      Colors[2] = System.Drawing.Color.FromArgb(126, 126, 126);
      Colors[3] = System.Drawing.Color.FromArgb(29, 29, 29);
      TransparencyEnabled = new bool[4];
      TransparencyEnabled[0] = true;
    }
  }
#else
  public class VobSubColors
  {
    public bool Enabled { get; set; }
    public bool[] TransparencyEnabled { get; set; } = new bool[4];
  }
#endif


  public class LanguageSpecific
  {
    public bool KanjiLinesOnly { get; set; }
  }


  public sealed class Settings
  {
    private static readonly Settings instance = new Settings();

    public SubSettings[] Subs { get; set; }
    public VideoClips VideoClips { get; set; }
    public AudioClips AudioClips { get; set; }
    public Snapshots Snapshots { get; set; }
    public VobSubColors VobSubColors { get; set; }
    public LanguageSpecific LanguageSpecific { get; set; }
    public string OutputDir { get; set; }
    public bool TimeShiftEnabled { get; set; }
    public bool SpanEnabled { get; set; }
    public TimeSpan SpanStart { get; set; }
    public TimeSpan SpanEnd { get; set; }

    private string _deckName = "";
    public string DeckName
    {
      get => _deckName;
      set => _deckName = value.Trim().Replace(" ", "_");
    }

    public int EpisodeStartNumber { get; set; }
    public List<string> ActorList { get; set; }

    public int ContextLeadingCount { get; set; }
    public int ContextTrailingCount { get; set; }
    public bool ContextLeadingIncludeSnapshots { get; set; }
    public bool ContextLeadingIncludeAudioClips { get; set; }
    public bool ContextLeadingIncludeVideoClips { get; set; }
    public int ContextLeadingRange { get; set; }
    public bool ContextTrailingIncludeSnapshots { get; set; }
    public bool ContextTrailingIncludeAudioClips { get; set; }
    public bool ContextTrailingIncludeVideoClips { get; set; }
    public int ContextTrailingRange { get; set; }

    public static Settings Instance => instance;

    static Settings() { }
    private Settings() { reset(); }

    public void loadSettings(SaveSettings settings)
    {
      Subs = settings.Subs;
      Subs[0].Files = Array.Empty<string>();
      Subs[1].Files = Array.Empty<string>();
      VideoClips = settings.VideoClips;
      VideoClips.Files = Array.Empty<string>();
      AudioClips = settings.AudioClips;
      AudioClips.Files = Array.Empty<string>();
      Snapshots = settings.Snapshots;
      VobSubColors = settings.VobSubColors;
      LanguageSpecific = settings.LanguageSpecific;

      OutputDir = settings.OutputDir;

      TimeShiftEnabled = settings.TimeShiftEnabled;

      SpanEnabled = settings.SpanEnabled;
      SpanStart = settings.SpanStart;
      SpanEnd = settings.SpanEnd;

      DeckName = settings.DeckName;
      EpisodeStartNumber = settings.EpisodeStartNumber;

      ActorList = settings.ActorList;

      ContextLeadingCount = settings.ContextLeadingCount;
      ContextLeadingIncludeSnapshots = settings.ContextLeadingIncludeSnapshots;
      ContextLeadingIncludeAudioClips = settings.ContextLeadingIncludeAudioClips;
      ContextLeadingIncludeVideoClips = settings.ContextLeadingIncludeVideoClips;
      ContextLeadingRange = settings.ContextLeadingRange;

      ContextTrailingCount = settings.ContextTrailingCount;
      ContextTrailingIncludeSnapshots = settings.ContextTrailingIncludeSnapshots;
      ContextTrailingIncludeAudioClips = settings.ContextTrailingIncludeAudioClips;
      ContextTrailingIncludeVideoClips = settings.ContextTrailingIncludeVideoClips;
      ContextTrailingRange = settings.ContextTrailingRange;
    }

    public void reset()
    {
      loadSettings(new SaveSettings());
    }
  }


  public class SaveSettings
  {
    [JsonPropertyName("subs")]
    public SubSettings[] Subs { get; set; }

    [JsonPropertyName("videoClips")]
    public VideoClips VideoClips { get; set; }

    [JsonPropertyName("audioClips")]
    public AudioClips AudioClips { get; set; }

    [JsonPropertyName("snapshots")]
    public Snapshots Snapshots { get; set; }

    [JsonPropertyName("vobSubColors")]
    public VobSubColors VobSubColors { get; set; }

    [JsonPropertyName("langaugeSpecific")]
    public LanguageSpecific LanguageSpecific { get; set; }

    [JsonPropertyName("outputDir")]
    public string OutputDir { get; set; }

    [JsonPropertyName("timeShiftEnabled")]
    public bool TimeShiftEnabled { get; set; }

    [JsonPropertyName("spanEnabled")]
    public bool SpanEnabled { get; set; }

    [JsonPropertyName("spanStart")]
    public TimeSpan SpanStart { get; set; }

    [JsonPropertyName("spanEnd")]
    public TimeSpan SpanEnd { get; set; }

    [JsonPropertyName("deckName")]
    public string DeckName { get; set; }

    [JsonPropertyName("episodeStartNumber")]
    public int EpisodeStartNumber { get; set; }

    [JsonPropertyName("actorList")]
    public List<string> ActorList { get; set; }

    [JsonPropertyName("contextLeadingCount")]
    public int ContextLeadingCount { get; set; }

    [JsonPropertyName("contextLeadingIncludeSnapshots")]
    public bool ContextLeadingIncludeSnapshots { get; set; }

    [JsonPropertyName("contextLeadingIncludeAudioClips")]
    public bool ContextLeadingIncludeAudioClips { get; set; }

    [JsonPropertyName("contextLeadingIncludeVideoClips")]
    public bool ContextLeadingIncludeVideoClips { get; set; }

    [JsonPropertyName("contextLeadingRange")]
    public int ContextLeadingRange { get; set; }

    [JsonPropertyName("contextTrailingCount")]
    public int ContextTrailingCount { get; set; }

    [JsonPropertyName("contextTrailingIncludeSnapshots")]
    public bool ContextTrailingIncludeSnapshots { get; set; }

    [JsonPropertyName("contextTrailingIncludeAudioClips")]
    public bool ContextTrailingIncludeAudioClips { get; set; }

    [JsonPropertyName("contextTrailingIncludeVideoClips")]
    public bool ContextTrailingIncludeVideoClips { get; set; }

    [JsonPropertyName("contextTrailingRange")]
    public int ContextTrailingRange { get; set; }

    public SaveSettings()
    {
      Subs = new SubSettings[2];
      Subs[0] = new SubSettings();
      Subs[1] = new SubSettings();
      Subs[0].ActorsEnabled = true;
      Subs[0].TimingsEnabled = true;

      // Sync preference defaults → SubSettings
      Subs[0].Encoding = ConstantSettings.DefaultEncodingSubs1;
      Subs[1].Encoding = ConstantSettings.DefaultEncodingSubs2;
      Subs[0].RemoveStyledLines = ConstantSettings.DefaultRemoveStyledLinesSubs1;
      Subs[1].RemoveStyledLines = ConstantSettings.DefaultRemoveStyledLinesSubs2;
      Subs[0].RemoveNoCounterpart = ConstantSettings.DefaultRemoveNoCounterpartSubs1;
      Subs[1].RemoveNoCounterpart = ConstantSettings.DefaultRemoveNoCounterpartSubs2;
      Subs[0].IncludedWords = UtilsCommon.removeExtraSpaces(
        ConstantSettings.DefaultIncludeTextSubs1.Split(';', StringSplitOptions.RemoveEmptyEntries));
      Subs[1].IncludedWords = UtilsCommon.removeExtraSpaces(
        ConstantSettings.DefaultIncludeTextSubs2.Split(';', StringSplitOptions.RemoveEmptyEntries));
      Subs[0].ExcludedWords = UtilsCommon.removeExtraSpaces(
        ConstantSettings.DefaultExcludeTextSubs1.Split(';', StringSplitOptions.RemoveEmptyEntries));
      Subs[1].ExcludedWords = UtilsCommon.removeExtraSpaces(
        ConstantSettings.DefaultExcludeTextSubs2.Split(';', StringSplitOptions.RemoveEmptyEntries));
      Subs[0].ExcludeDuplicateLinesEnabled = ConstantSettings.DefaultExcludeDuplicateLinesSubs1;
      Subs[1].ExcludeDuplicateLinesEnabled = ConstantSettings.DefaultExcludeDuplicateLinesSubs2;
      Subs[0].ExcludeFewerEnabled = ConstantSettings.DefaultExcludeLinesFewerThanCharsSubs1;
      Subs[1].ExcludeFewerEnabled = ConstantSettings.DefaultExcludeLinesFewerThanCharsSubs2;
      Subs[0].ExcludeFewerCount = ConstantSettings.DefaultExcludeLinesFewerThanCharsNumSubs1;
      Subs[1].ExcludeFewerCount = ConstantSettings.DefaultExcludeLinesFewerThanCharsNumSubs2;
      Subs[0].ExcludeShorterThanTimeEnabled = ConstantSettings.DefaultExcludeLinesShorterThanMsSubs1;
      Subs[1].ExcludeShorterThanTimeEnabled = ConstantSettings.DefaultExcludeLinesShorterThanMsSubs2;
      Subs[0].ExcludeShorterThanTime = ConstantSettings.DefaultExcludeLinesShorterThanMsNumSubs1;
      Subs[1].ExcludeShorterThanTime = ConstantSettings.DefaultExcludeLinesShorterThanMsNumSubs2;
      Subs[0].ExcludeLongerThanTimeEnabled = ConstantSettings.DefaultExcludeLinesLongerThanMsSubs1;
      Subs[1].ExcludeLongerThanTimeEnabled = ConstantSettings.DefaultExcludeLinesLongerThanMsSubs2;
      Subs[0].ExcludeLongerThanTime = ConstantSettings.DefaultExcludeLinesLongerThanMsNumSubs1;
      Subs[1].ExcludeLongerThanTime = ConstantSettings.DefaultExcludeLinesLongerThanMsNumSubs2;
      Subs[0].JoinSentencesEnabled = ConstantSettings.DefaultJoinSentencesSubs1;
      Subs[1].JoinSentencesEnabled = ConstantSettings.DefaultJoinSentencesSubs2;
      Subs[0].JoinSentencesCharList = ConstantSettings.DefaultJoinSentencesCharListSubs1;
      Subs[1].JoinSentencesCharList = ConstantSettings.DefaultJoinSentencesCharListSubs2;

      // Sync preference defaults → media settings
      AudioClips = new AudioClips();
      AudioClips.Enabled = ConstantSettings.DefaultEnableAudioClipGeneration;
      AudioClips.Bitrate = ConstantSettings.DefaultAudioClipBitrate;
      AudioClips.Normalize = ConstantSettings.DefaultAudioNormalize;

      Snapshots = new Snapshots();
      Snapshots.Enabled = ConstantSettings.DefaultEnableSnapshotsGeneration;
      Snapshots.Quality = ConstantSettings.DefaultSnapshotJpegQuality;

      VideoClips = new VideoClips();
      VideoClips.Enabled = ConstantSettings.DefaultEnableVideoClipsGeneration;
      VideoClips.BitrateVideo = ConstantSettings.DefaultVideoClipVideoBitrate;
      VideoClips.BitrateAudio = ConstantSettings.DefaultVideoClipAudioBitrate;
      VideoClips.IPodSupport = ConstantSettings.DefaultIphoneSupport;

      VobSubColors = new VobSubColors();
      LanguageSpecific = new LanguageSpecific();
      OutputDir = "";
      TimeShiftEnabled = false;
      SpanEnabled = false;
      SpanStart = TimeSpan.FromMilliseconds(90_000);
      SpanEnd = TimeSpan.FromMilliseconds(1_350_000);
      DeckName = "";
      EpisodeStartNumber = 1;

      ActorList = new List<string>();

      ContextLeadingCount = ConstantSettings.DefaultContextNumLeading;
      ContextLeadingIncludeSnapshots = false;
      ContextLeadingIncludeAudioClips = false;
      ContextLeadingIncludeVideoClips = false;
      ContextLeadingRange = ConstantSettings.DefaultContextLeadingRange;

      ContextTrailingCount = ConstantSettings.DefaultContextNumTrailing;
      ContextTrailingIncludeSnapshots = false;
      ContextTrailingIncludeAudioClips = false;
      ContextTrailingIncludeVideoClips = false;
      ContextTrailingRange = ConstantSettings.DefaultContextTrailingRange;
    }

    public void gatherData()
    {
      Subs = Settings.Instance.Subs;
      VideoClips = Settings.Instance.VideoClips;
      AudioClips = Settings.Instance.AudioClips;
      Snapshots = Settings.Instance.Snapshots;
      VobSubColors = Settings.Instance.VobSubColors;
      LanguageSpecific = Settings.Instance.LanguageSpecific;

      OutputDir = Settings.Instance.OutputDir;

      TimeShiftEnabled = Settings.Instance.TimeShiftEnabled;

      SpanEnabled = Settings.Instance.SpanEnabled;
      SpanStart = Settings.Instance.SpanStart;
      SpanEnd = Settings.Instance.SpanEnd;

      DeckName = Settings.Instance.DeckName;
      EpisodeStartNumber = Settings.Instance.EpisodeStartNumber;

      ActorList = Settings.Instance.ActorList;

      ContextLeadingCount = Settings.Instance.ContextLeadingCount;
      ContextLeadingIncludeSnapshots = Settings.Instance.ContextLeadingIncludeSnapshots;
      ContextLeadingIncludeAudioClips = Settings.Instance.ContextLeadingIncludeAudioClips;
      ContextLeadingIncludeVideoClips = Settings.Instance.ContextLeadingIncludeVideoClips;
      ContextLeadingRange = Settings.Instance.ContextLeadingRange;

      ContextTrailingCount = Settings.Instance.ContextTrailingCount;
      ContextTrailingIncludeSnapshots = Settings.Instance.ContextTrailingIncludeSnapshots;
      ContextTrailingIncludeAudioClips = Settings.Instance.ContextTrailingIncludeAudioClips;
      ContextTrailingIncludeVideoClips = Settings.Instance.ContextTrailingIncludeVideoClips;
      ContextTrailingRange = Settings.Instance.ContextTrailingRange;
    }
  }
}
