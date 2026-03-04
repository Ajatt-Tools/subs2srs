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


  // Procedure for creating a new Constant Settings that can be set in the Preferences dialog.
  // 1) Create a new entry in PrefIO.writeDefaultPreferences.
  // 2) Create a default for the setting in PrefDefaults (above).
  // 3) Create the setting in ConstantSettings (property with PrefDefaults default).
  // 4) Add setting to PrefIO.read.
  // 5) Add setting to DialogPref.BuildPropTable.
  // 6) Add setting to DialogPref.SavePreferences.
  // 7) Add setting to Logger.writeSettingsToLog.
  // 8) For GUI settings (ones that map to Settings.Instance), add to SaveSettings constructor.

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

    // ── Mutable (loaded from preferences.txt via PrefIO.read) ──────────

    public static int MaxParallelTasks { get; set; } = PrefDefaults.MaxParallelTasks;

    public static int EffectiveParallelism => MaxParallelTasks > 0
        ? MaxParallelTasks
        : Environment.ProcessorCount;

    public static int MainWindowWidth { get; set; } = PrefDefaults.MainWindowWidth;
    public static int MainWindowHeight { get; set; } = PrefDefaults.MainWindowHeight;

    public static bool DefaultEnableAudioClipGeneration { get; set; } = PrefDefaults.DefaultEnableAudioClipGeneration;
    public static bool DefaultEnableSnapshotsGeneration { get; set; } = PrefDefaults.DefaultEnableSnapshotsGeneration;
    public static bool DefaultEnableVideoClipsGeneration { get; set; } = PrefDefaults.DefaultEnableVideoClipsGeneration;

    public static string VideoPlayer { get; set; } = PrefDefaults.VideoPlayer;
    public static string VideoPlayerArgs { get; set; } = PrefDefaults.VideoPlayerArgs;

    public static bool ReencodeBeforeSplittingAudio { get; set; } = PrefDefaults.ReencodeBeforeSplittingAudio;
    public static bool EnableLogging { get; set; } = PrefDefaults.EnableLogging;
    public static string AudioNormalizeArgs { get; set; } = PrefDefaults.AudioNormalizeArgs;
    public static int LongClipWarningSeconds { get; set; } = PrefDefaults.LongClipWarningSeconds;

    public static int DefaultAudioClipBitrate { get; set; } = PrefDefaults.DefaultAudioClipBitrate;
    public static bool DefaultAudioNormalize { get; set; } = PrefDefaults.DefaultAudioNormalize;

    public static int DefaultVideoClipVideoBitrate { get; set; } = PrefDefaults.DefaultVideoClipVideoBitrate;
    public static int DefaultVideoClipAudioBitrate { get; set; } = PrefDefaults.DefaultVideoClipAudioBitrate;
    public static bool DefaultIphoneSupport { get; set; } = PrefDefaults.DefaultIphoneSupport;

    public static string DefaultEncodingSubs1 { get; set; } = PrefDefaults.DefaultEncodingSubs1;
    public static string DefaultEncodingSubs2 { get; set; } = PrefDefaults.DefaultEncodingSubs2;

    public static int DefaultContextNumLeading { get; set; } = PrefDefaults.DefaultContextNumLeading;
    public static int DefaultContextNumTrailing { get; set; } = PrefDefaults.DefaultContextNumTrailing;
    public static int DefaultContextLeadingRange { get; set; } = PrefDefaults.DefaultContextLeadingRange;
    public static int DefaultContextTrailingRange { get; set; } = PrefDefaults.DefaultContextTrailingRange;

    public static string DefaultFileBrowserStartDir { get; set; } = PrefDefaults.DefaultFileBrowserStartDir;

    public static bool DefaultRemoveStyledLinesSubs1 { get; set; } = PrefDefaults.DefaultRemoveStyledLinesSubs1;
    public static bool DefaultRemoveStyledLinesSubs2 { get; set; } = PrefDefaults.DefaultRemoveStyledLinesSubs2;
    public static bool DefaultRemoveNoCounterpartSubs1 { get; set; } = PrefDefaults.DefaultRemoveNoCounterpartSubs1;
    public static bool DefaultRemoveNoCounterpartSubs2 { get; set; } = PrefDefaults.DefaultRemoveNoCounterpartSubs2;

    public static string DefaultIncludeTextSubs1 { get; set; } = PrefDefaults.DefaultIncludeTextSubs1;
    public static string DefaultIncludeTextSubs2 { get; set; } = PrefDefaults.DefaultIncludeTextSubs2;
    public static string DefaultExcludeTextSubs1 { get; set; } = PrefDefaults.DefaultExcludeTextSubs1;
    public static string DefaultExcludeTextSubs2 { get; set; } = PrefDefaults.DefaultExcludeTextSubs2;

    public static bool DefaultExcludeDuplicateLinesSubs1 { get; set; } = PrefDefaults.DefaultExcludeDuplicateLinesSubs1;
    public static bool DefaultExcludeDuplicateLinesSubs2 { get; set; } = PrefDefaults.DefaultExcludeDuplicateLinesSubs2;

    public static bool DefaultExcludeLinesFewerThanCharsSubs1 { get; set; } = PrefDefaults.DefaultExcludeLinesFewerThanCharsSubs1;
    public static bool DefaultExcludeLinesFewerThanCharsSubs2 { get; set; } = PrefDefaults.DefaultExcludeLinesFewerThanCharsSubs2;
    public static int DefaultExcludeLinesFewerThanCharsNumSubs1 { get; set; } = PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs1;
    public static int DefaultExcludeLinesFewerThanCharsNumSubs2 { get; set; } = PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs2;

    public static bool DefaultExcludeLinesShorterThanMsSubs1 { get; set; } = PrefDefaults.DefaultExcludeLinesShorterThanMsSubs1;
    public static bool DefaultExcludeLinesShorterThanMsSubs2 { get; set; } = PrefDefaults.DefaultExcludeLinesShorterThanMsSubs2;
    public static int DefaultExcludeLinesShorterThanMsNumSubs1 { get; set; } = PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs1;
    public static int DefaultExcludeLinesShorterThanMsNumSubs2 { get; set; } = PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs2;

    public static bool DefaultExcludeLinesLongerThanMsSubs1 { get; set; } = PrefDefaults.DefaultExcludeLinesLongerThanMsSubs1;
    public static bool DefaultExcludeLinesLongerThanMsSubs2 { get; set; } = PrefDefaults.DefaultExcludeLinesLongerThanMsSubs2;
    public static int DefaultExcludeLinesLongerThanMsNumSubs1 { get; set; } = PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs1;
    public static int DefaultExcludeLinesLongerThanMsNumSubs2 { get; set; } = PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs2;

    public static bool DefaultJoinSentencesSubs1 { get; set; } = PrefDefaults.DefaultJoinSentencesSubs1;
    public static bool DefaultJoinSentencesSubs2 { get; set; } = PrefDefaults.DefaultJoinSentencesSubs2;
    public static string DefaultJoinSentencesCharListSubs1 { get; set; } = PrefDefaults.DefaultJoinSentencesCharListSubs1;
    public static string DefaultJoinSentencesCharListSubs2 { get; set; } = PrefDefaults.DefaultJoinSentencesCharListSubs2;

    public static string SrsFilenameFormat { get; set; } = PrefDefaults.SrsFilenameFormat;
    public static string SrsDelimiter { get; set; } = PrefDefaults.SrsDelimiter;
    public static string SrsTagFormat { get; set; } = PrefDefaults.SrsTagFormat;
    public static string SrsSequenceMarkerFormat { get; set; } = PrefDefaults.SrsSequenceMarkerFormat;

    public static string SrsAudioFilenamePrefix { get; set; } = PrefDefaults.SrsAudioFilenamePrefix;
    public static string SrsAudioFilenameSuffix { get; set; } = PrefDefaults.SrsAudioFilenameSuffix;
    public static string SrsSnapshotFilenamePrefix { get; set; } = PrefDefaults.SrsSnapshotFilenamePrefix;
    public static string SrsSnapshotFilenameSuffix { get; set; } = PrefDefaults.SrsSnapshotFilenameSuffix;
    public static string SrsVideoFilenamePrefix { get; set; } = PrefDefaults.SrsVideoFilenamePrefix;
    public static string SrsVideoFilenameSuffix { get; set; } = PrefDefaults.SrsVideoFilenameSuffix;
    public static string SrsSubs1Format { get; set; } = PrefDefaults.SrsSubs1Format;
    public static string SrsSubs2Format { get; set; } = PrefDefaults.SrsSubs2Format;
    public static string SrsVobsubFilenamePrefix { get; set; } = PrefDefaults.SrsVobsubFilenamePrefix;
    public static string SrsVobsubFilenameSuffix { get; set; } = PrefDefaults.SrsVobsubFilenameSuffix;

    public static string AudioFilenameFormat { get; set; } = PrefDefaults.AudioFilenameFormat;
    public static string SnapshotFilenameFormat { get; set; } = PrefDefaults.SnapshotFilenameFormat;
    public static string VideoFilenameFormat { get; set; } = PrefDefaults.VideoFilenameFormat;
    public static string VobsubFilenameFormat { get; set; } = PrefDefaults.VobsubFilenameFormat;

    public static string AudioId3Artist { get; set; } = PrefDefaults.AudioId3Artist;
    public static string AudioId3Album { get; set; } = PrefDefaults.AudioId3Album;
    public static string AudioId3Title { get; set; } = PrefDefaults.AudioId3Title;
    public static string AudioId3Genre { get; set; } = PrefDefaults.AudioId3Genre;
    public static string AudioId3Lyrics { get; set; } = PrefDefaults.AudioId3Lyrics;

    public static string ExtractMediaAudioFilenameFormat { get; set; } = PrefDefaults.ExtractMediaAudioFilenameFormat;
    public static string ExtractMediaLyricsSubs1Format { get; set; } = PrefDefaults.ExtractMediaLyricsSubs1Format;
    public static string ExtractMediaLyricsSubs2Format { get; set; } = PrefDefaults.ExtractMediaLyricsSubs2Format;

    public static string DuelingSubtitleFilenameFormat { get; set; } = PrefDefaults.DuelingSubtitleFilenameFormat;
    public static string DuelingQuickRefFilenameFormat { get; set; } = PrefDefaults.DuelingQuickRefFilenameFormat;
    public static string DuelingQuickRefSubs1Format { get; set; } = PrefDefaults.DuelingQuickRefSubs1Format;
    public static string DuelingQuickRefSubs2Format { get; set; } = PrefDefaults.DuelingQuickRefSubs2Format;
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
    public DateTime SpanStart { get; set; }
    public DateTime SpanEnd { get; set; }

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
    public DateTime SpanStart { get; set; }

    [JsonPropertyName("spanEnd")]
    public DateTime SpanEnd { get; set; }

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
      SpanStart = new DateTime().AddMilliseconds(90_000);
      SpanEnd = new DateTime().AddMilliseconds(1_350_000);
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
