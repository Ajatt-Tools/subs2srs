//  Copyright (C) 2009-2016 Christopher Brochtrup
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
  // 1) Create a new entry in preferences.txt
  // 2) Create a default for the setting in PrefDefaults (above).
  // 3) Create the setting in ConstantSettings (var and property).
  // 4) Add setting to PrefIO.read.
  // 5) Add setting to DialogPref constructor.
  // 6) Add setting to DialogPref buttonOK_Click.
  // 7) Add setting to Logger.writeSettingsToLog.
  // 8) For GUI settings, add to FormMain.readPreferencesFile.


  public static class ConstantSettings
  {
    private static string FindInPath(string name)
    {
        foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "")
            .Split(Path.PathSeparator))
        {
            var full = Path.Combine(dir, name);
            if (File.Exists(full)) return full;
        }
        return name;
    }

    private static string saveExt = "s2s";
    private static string helpPage = "http://subs2srs.sourceforge.net/";
    private static string logDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "subs2srs", "Logs") + Path.DirectorySeparatorChar;
    private static int maxLogFiles = 10;

    private static string exeFFmpeg = "ffmpeg";
    private static string pathFFmpegFullExe = FindInPath("ffmpeg");
    private static string pathFFmpegExe = pathFFmpegFullExe;
    private static string pathFFmpegPresetsFull = Path.Combine(
        UtilsCommon.getAppDir(true), "presets");

    private static string tempImageFilename = $"subs2srs_temp_{Guid.NewGuid()}.jpg";
    private static string tempVideoFilename = $"subs2srs_temp_{Guid.NewGuid()}";
    private static string tempAudioFilename = $"subs2srs_temp_{Guid.NewGuid()}.mp3";
    private static string tempAudioPreviewFilename = $"subs2srs_temp_{Guid.NewGuid()}.wav";
    private static string tempPreviewDirName = $"subs2srs_preview_{Guid.NewGuid()}";
    private static string tempMkvExtractSubs1Filename = $"subs2srs_mkv_extract_subs1_{Guid.NewGuid()}";
    private static string tempMkvExtractSubs2Filename = $"subs2srs_mkv_extract_subs2_{Guid.NewGuid()}";

    private static string normalizeAudioExe = "mp3gain";
    private static string pathNormalizeAudioExeRel = "mp3gain";
    private static string pathNormalizeAudioExeFull = FindInPath("mp3gain");

    private static string pathSubsReTimerFull = FindInPath("SubsReTimer");

    private static string exeMkvInfo = "mkvinfo";
    private static string pathMkvDirRel = "";
    private static string pathMkvDirFull = "";
    private static string pathMkvInfoExeRel = "mkvinfo";
    private static string pathMkvInfoExeFull = FindInPath("mkvinfo");

    private static string exeMkvExtract = "mkvextract";
    private static string pathMkvExtractExeRel = "mkvextract";
    private static string pathMkvExtractExeFull = FindInPath("mkvextract");

    private static string settingsFilename = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "subs2srs", "preferences.txt");
    private static int mainWindowWidth = PrefDefaults.MainWindowWidth;

    // Preferences file stuff
    private static int mainWindowHeight = PrefDefaults.MainWindowHeight;
    private static bool defaultEnableAudioClipGeneration = PrefDefaults.DefaultEnableAudioClipGeneration;
    private static bool defaultEnableSnapshotsGeneration = PrefDefaults.DefaultEnableSnapshotsGeneration;
    private static bool defaultEnableVideoClipsGeneration = PrefDefaults.DefaultEnableVideoClipsGeneration;
    private static string videoPlayer = PrefDefaults.VideoPlayer;
    private static string videoPlayerArgs = PrefDefaults.VideoPlayerArgs;
    private static bool reencodeBeforeSplittingAudio = PrefDefaults.ReencodeBeforeSplittingAudio;
    private static bool enableLogging = PrefDefaults.EnableLogging;
    private static string audioNormalizeArgs = PrefDefaults.AudioNormalizeArgs;
    private static int longClipWarningSeconds = PrefDefaults.LongClipWarningSeconds;
    private static int defaultAudioClipBitrate = PrefDefaults.DefaultAudioClipBitrate;
    private static bool defaultAudioNormalize = PrefDefaults.DefaultAudioNormalize;
    private static int defaultVideoClipVideoBitrate = PrefDefaults.DefaultVideoClipVideoBitrate;
    private static int defaultVideoClipAudioBitrate = PrefDefaults.DefaultVideoClipAudioBitrate;
    private static bool defaultIpodSupport = PrefDefaults.DefaultIphoneSupport;
    private static string defaultEncodingSubs1 = PrefDefaults.DefaultEncodingSubs1;
    private static string defaultEncodingSubs2 = PrefDefaults.DefaultEncodingSubs2;
    private static int defaultContextNumLeading = PrefDefaults.DefaultContextNumLeading;
    private static int defaultContextNumTrailing = PrefDefaults.DefaultContextNumTrailing;
    private static int defaultContextLeadingRange = PrefDefaults.DefaultContextLeadingRange;
    private static int defaultContextTrailingRange = PrefDefaults.DefaultContextTrailingRange;
    private static string defaultFileBrowserStartDir = PrefDefaults.DefaultFileBrowserStartDir;
    private static bool defaultRemoveStyledLinesSubs1 = PrefDefaults.DefaultRemoveStyledLinesSubs1;
    private static bool defaultRemoveStyledLinesSubs2 = PrefDefaults.DefaultRemoveStyledLinesSubs2;
    private static bool defaultRemoveNoCounterpartSubs1 = PrefDefaults.DefaultRemoveNoCounterpartSubs1;
    private static bool defaultRemoveNoCounterpartSubs2 = PrefDefaults.DefaultRemoveNoCounterpartSubs2;
    private static string defaultIncludeTextSubs1 = PrefDefaults.DefaultIncludeTextSubs1;
    private static string defaultIncludeTextSubs2 = PrefDefaults.DefaultIncludeTextSubs2;
    private static string defaultExcludeTextSubs1 = PrefDefaults.DefaultExcludeTextSubs1;
    private static string defaultExcludeTextSubs2 = PrefDefaults.DefaultExcludeTextSubs2;
    private static bool defaultExcludeDuplicateLinesSubs1 = PrefDefaults.DefaultExcludeDuplicateLinesSubs1;
    private static bool defaultExcludeDuplicateLinesSubs2 = PrefDefaults.DefaultExcludeDuplicateLinesSubs2;
    private static bool defaultExcludeLinesFewerThanCharsSubs1 = PrefDefaults.DefaultExcludeLinesFewerThanCharsSubs1;
    private static bool defaultExcludeLinesFewerThanCharsSubs2 = PrefDefaults.DefaultExcludeLinesFewerThanCharsSubs2;
    private static int defaultExcludeLinesFewerThanCharsNumSubs1 = PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs1;
    private static int defaultExcludeLinesFewerThanCharsNumSubs2 = PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs2;
    private static bool defaultExcludeLinesShorterThanMsSubs1 = PrefDefaults.DefaultExcludeLinesShorterThanMsSubs1;
    private static bool defaultExcludeLinesShorterThanMsSubs2 = PrefDefaults.DefaultExcludeLinesShorterThanMsSubs2;
    private static int defaultExcludeLinesShorterThanMsNumSubs1 = PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs1;
    private static int defaultExcludeLinesShorterThanMsNumSubs2 = PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs2;
    private static bool defaultExcludeLinesLongerThanMsSubs1 = PrefDefaults.DefaultExcludeLinesLongerThanMsSubs1;
    private static bool defaultExcludeLinesLongerThanMsSubs2 = PrefDefaults.DefaultExcludeLinesLongerThanMsSubs2;
    private static int defaultExcludeLinesLongerThanMsNumSubs1 = PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs1;
    private static int defaultExcludeLinesLongerThanMsNumSubs2 = PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs2;
    private static bool defaultJoinSentencesSubs1 = PrefDefaults.DefaultJoinSentencesSubs1;
    private static bool defaultJoinSentencesSubs2 = PrefDefaults.DefaultJoinSentencesSubs2;
    private static string defaultJoinSentencesCharListSubs1 = PrefDefaults.DefaultJoinSentencesCharListSubs1;
    private static string defaultJoinSentencesCharListSubs2 = PrefDefaults.DefaultJoinSentencesCharListSubs2;
    private static string srsFilenameFormat = PrefDefaults.SrsFilenameFormat;
    private static string srsDelimiter = PrefDefaults.SrsDelimiter;
    private static string srsTagFormat = PrefDefaults.SrsTagFormat;
    private static string srsSequenceMarkerFormat = PrefDefaults.SrsSequenceMarkerFormat;
    private static string srsAudioFilenamePrefix = PrefDefaults.SrsAudioFilenamePrefix;
    private static string srsAudioFilenameSuffix = PrefDefaults.SrsAudioFilenameSuffix;
    private static string srsSnapshotFilenamePrefix = PrefDefaults.SrsSnapshotFilenamePrefix;
    private static string srsSnapshotFilenameSuffix = PrefDefaults.SrsSnapshotFilenameSuffix;
    private static string srsVideoFilenamePrefix = PrefDefaults.SrsVideoFilenamePrefix;
    private static string srsVideoFilenameSuffix = PrefDefaults.SrsVideoFilenameSuffix;
    private static string srsSubs1Format = PrefDefaults.SrsSubs1Format;
    private static string srsSubs2Format = PrefDefaults.SrsSubs2Format;
    private static string srsVobsubFilenamePrefix = PrefDefaults.SrsVobsubFilenamePrefix;
    private static string srsVobsubFilenameSuffix = PrefDefaults.SrsVobsubFilenameSuffix;
    private static string audioFilenameFormat = PrefDefaults.AudioFilenameFormat;
    private static string snapshotFilenameFormat = PrefDefaults.SnapshotFilenameFormat;
    private static string videoFilenameFormat = PrefDefaults.VideoFilenameFormat;
    private static string vobsubFilenameFormat = PrefDefaults.VobsubFilenameFormat;
    private static string audioId3Artist = PrefDefaults.AudioId3Artist;
    private static string audioId3Album = PrefDefaults.AudioId3Album;
    private static string audioId3Title = PrefDefaults.AudioId3Title;
    private static string audioId3Genre = PrefDefaults.AudioId3Genre;
    private static string audioId3Lyrics = PrefDefaults.AudioId3Lyrics;
    private static string extractMediaAudioFilenameFormat = PrefDefaults.ExtractMediaAudioFilenameFormat;
    private static string extractMediaLyricsSubs1Format = PrefDefaults.ExtractMediaLyricsSubs1Format;
    private static string extractMediaLyricsSubs2Format = PrefDefaults.ExtractMediaLyricsSubs2Format;
    private static string duelingSubtitleFilenameFormat = PrefDefaults.DuelingSubtitleFilenameFormat;
    private static string duelingQuickRefFilenameFormat = PrefDefaults.DuelingQuickRefFilenameFormat;
    private static string duelingQuickRefSubs1Format = PrefDefaults.DuelingQuickRefSubs1Format;
    private static string duelingQuickRefSubs2Format = PrefDefaults.DuelingQuickRefSubs2Format;

    public static string SaveExt
    {
      get { return saveExt; }
    }

    public static string HelpPage
    {
      get { return helpPage; }
    }

    public static string LogDir
    {
      get { return logDir; }
    }

    public static int MaxLogFiles
    {
      get { return maxLogFiles; }
    }

    public static string ExeFFmpeg
    {
      get 
      {
        return exeFFmpeg;
      }
    }

    public static string PathFFmpegExe
    {
      get 
      {
        return pathFFmpegExe;
      }
    }

    public static string PathFFmpegFullExe
    {
      get
      {
        return pathFFmpegFullExe;
      }
    }

    public static string PathFFmpegPresetsFull
    {
      get { return pathFFmpegPresetsFull; }
    }

    public static string TempImageFilename
    {
      get { return tempImageFilename; }
    }

    public static string TempVideoFilename
    {
      get { return tempVideoFilename; }
    }
  
    public static string TempAudioFilename
    {
      get { return tempAudioFilename; }
    }

    public static string TempAudioPreviewFilename
    {
      get { return tempAudioPreviewFilename; }
    }

    public static string TempPreviewDirName
    {
      get { return tempPreviewDirName; }
    }

    public static string TempMkvExtractSubs1Filename
    {
      get { return tempMkvExtractSubs1Filename; }
    }

    public static string TempMkvExtractSubs2Filename
    {
      get { return tempMkvExtractSubs2Filename; }
    }

    public static string NormalizeAudioExe
    {
      get { return normalizeAudioExe; }
    }

    public static string PathNormalizeAudioExeRel
    {
      get { return pathNormalizeAudioExeRel; }
    }

    public static string PathNormalizeAudioExeFull
    {
      get { return pathNormalizeAudioExeFull; }
    }

    public static string PathSubsReTimerFull
    {
      get { return pathSubsReTimerFull; }
    }

    public static string PathMkvDirRel
    {
      get { return pathMkvDirRel; }
    }

    public static string PathMkvDirFull
    {
      get { return pathMkvDirFull; }
    }

    public static string ExeMkvInfo
    {
      get { return exeMkvInfo; }
    }

    public static string PathMkvInfoExeRel
    {
      get { return pathMkvInfoExeRel; }
    }

    public static string PathMkvInfoExeFull
    {
      get { return pathMkvInfoExeFull; }
    }

    public static string ExeMkvExtract
    {
      get { return exeMkvExtract; }
    }

    public static string PathMkvExtractExeRel
    {
      get { return pathMkvExtractExeRel; }
    }

    public static string PathMkvExtractExeFull
    {
      get { return pathMkvExtractExeFull; }
    }

    public static string SettingsFilename
    {
      get { return settingsFilename; }
    }

    public static int MainWindowWidth
    {
      get { return mainWindowWidth; }
      set { mainWindowWidth = value; }
    }

    public static int MainWindowHeight
    {
      get { return mainWindowHeight; }
      set { mainWindowHeight = value; }
    }

    public static bool DefaultEnableAudioClipGeneration
    {
      get { return defaultEnableAudioClipGeneration; }
      set { defaultEnableAudioClipGeneration = value; }
    }

    public static bool DefaultEnableSnapshotsGeneration
    {
      get { return defaultEnableSnapshotsGeneration; }
      set { defaultEnableSnapshotsGeneration = value; }
    }

    public static bool DefaultEnableVideoClipsGeneration
    {
      get { return defaultEnableVideoClipsGeneration; }
      set { defaultEnableVideoClipsGeneration = value; }
    }

    public static string VideoPlayer
    {
      get { return videoPlayer; }
      set { videoPlayer = value; }
    }

    public static string VideoPlayerArgs
    {
      get { return videoPlayerArgs; }
      set { videoPlayerArgs = value; }
    }

    public static bool ReencodeBeforeSplittingAudio
    {
      get { return reencodeBeforeSplittingAudio; }
      set { reencodeBeforeSplittingAudio = value; }
    }

    public static bool EnableLogging
    {
      get { return enableLogging; }
      set { enableLogging = value; }
    }

    public static string AudioNormalizeArgs
    {
      get { return audioNormalizeArgs; }
      set { audioNormalizeArgs = value; }
    }

    public static int LongClipWarningSeconds
    {
      get { return longClipWarningSeconds; }
      set { longClipWarningSeconds = value; }
    }

    public static int DefaultAudioClipBitrate
    {
      get { return defaultAudioClipBitrate; }
      set { defaultAudioClipBitrate = value; }
    }

    public static bool DefaultAudioNormalize
    {
      get { return defaultAudioNormalize; }
      set { defaultAudioNormalize = value; }
    }

    public static int DefaultVideoClipVideoBitrate
    {
      get { return defaultVideoClipVideoBitrate; }
      set { defaultVideoClipVideoBitrate = value; }
    }

    public static int DefaultVideoClipAudioBitrate
    {
      get { return defaultVideoClipAudioBitrate; }
      set { defaultVideoClipAudioBitrate = value; }
    }

    public static bool DefaultIphoneSupport
    {
      get { return defaultIpodSupport; }
      set { defaultIpodSupport = value; }
    }

    public static string DefaultEncodingSubs1
    {
      get { return defaultEncodingSubs1; }
      set { defaultEncodingSubs1 = value; }
    }

    public static string DefaultEncodingSubs2
    {
      get { return defaultEncodingSubs2; }
      set { defaultEncodingSubs2 = value; }
    }

    public static int DefaultContextNumLeading
    {
      get { return defaultContextNumLeading; }
      set { defaultContextNumLeading = value; }
    }

    public static int DefaultContextNumTrailing
    {
      get { return defaultContextNumTrailing; }
      set { defaultContextNumTrailing = value; }
    }

    public static int DefaultContextLeadingRange
    {
      get { return defaultContextLeadingRange; }
      set { defaultContextLeadingRange = value; }
    }

    public static int DefaultContextTrailingRange
    {
      get { return defaultContextTrailingRange; }
      set { defaultContextTrailingRange = value; }
    }

    public static string DefaultFileBrowserStartDir
    {
      get { return defaultFileBrowserStartDir; }
      set { defaultFileBrowserStartDir = value; }
    }

    public static bool DefaultRemoveStyledLinesSubs1
    {
      get { return defaultRemoveStyledLinesSubs1; }
      set { defaultRemoveStyledLinesSubs1 = value; }
    }

    public static bool DefaultRemoveStyledLinesSubs2
    {
      get { return defaultRemoveStyledLinesSubs2; }
      set {defaultRemoveStyledLinesSubs2 = value; }
    }

    public static bool DefaultRemoveNoCounterpartSubs1
    {
      get { return defaultRemoveNoCounterpartSubs1; }
      set { defaultRemoveNoCounterpartSubs1 = value; }
    }

    public static bool DefaultRemoveNoCounterpartSubs2
    {
      get { return defaultRemoveNoCounterpartSubs2; }
      set { defaultRemoveNoCounterpartSubs2 = value; }
    }

    public static string DefaultIncludeTextSubs1
    {
      get { return defaultIncludeTextSubs1; }
      set { defaultIncludeTextSubs1 = value; }
    }

    public static string DefaultIncludeTextSubs2
    {
      get { return defaultIncludeTextSubs2; }
      set { defaultIncludeTextSubs2 = value; }
    }

    public static string DefaultExcludeTextSubs1
    {
      get { return defaultExcludeTextSubs1; }
      set { defaultExcludeTextSubs1 = value; }
    }

    public static string DefaultExcludeTextSubs2
    {
      get { return defaultExcludeTextSubs2; }
      set { defaultExcludeTextSubs2 = value; }
    }

    public static bool DefaultExcludeLinesFewerThanCharsSubs1
    {
      get { return defaultExcludeLinesFewerThanCharsSubs1; }
      set { defaultExcludeLinesFewerThanCharsSubs1 = value; }
    }

    public static bool DefaultExcludeLinesFewerThanCharsSubs2
    {
      get { return defaultExcludeLinesFewerThanCharsSubs2; }
      set { defaultExcludeLinesFewerThanCharsSubs2 = value; }
    }

    public static int DefaultExcludeLinesFewerThanCharsNumSubs1
    {
      get { return defaultExcludeLinesFewerThanCharsNumSubs1; }
      set { defaultExcludeLinesFewerThanCharsNumSubs1 = value; }
    }

    public static int DefaultExcludeLinesFewerThanCharsNumSubs2
    {
      get { return defaultExcludeLinesFewerThanCharsNumSubs2; }
      set { defaultExcludeLinesFewerThanCharsNumSubs2 = value; }
    }

    public static bool DefaultExcludeLinesShorterThanMsSubs1
    {
      get { return defaultExcludeLinesShorterThanMsSubs1; }
      set { defaultExcludeLinesShorterThanMsSubs1 = value; }
    }

    public static bool DefaultExcludeLinesShorterThanMsSubs2
    {
      get { return defaultExcludeLinesShorterThanMsSubs2; }
      set { defaultExcludeLinesShorterThanMsSubs2 = value; }
    }

    public static int DefaultExcludeLinesShorterThanMsNumSubs1
    {
      get { return defaultExcludeLinesShorterThanMsNumSubs1; }
      set { defaultExcludeLinesShorterThanMsNumSubs1 = value; }
    }

    public static int DefaultExcludeLinesShorterThanMsNumSubs2
    {
      get { return defaultExcludeLinesShorterThanMsNumSubs2; }
      set { defaultExcludeLinesShorterThanMsNumSubs2 = value; }
    }

    public static bool DefaultExcludeLinesLongerThanMsSubs1
    {
      get { return defaultExcludeLinesLongerThanMsSubs1; }
      set { defaultExcludeLinesLongerThanMsSubs1 = value; }
    }

    public static bool DefaultExcludeLinesLongerThanMsSubs2
    {
      get { return defaultExcludeLinesLongerThanMsSubs2; }
      set { defaultExcludeLinesLongerThanMsSubs2 = value; }
    }

    public static int DefaultExcludeLinesLongerThanMsNumSubs1
    {
      get { return defaultExcludeLinesLongerThanMsNumSubs1; }
      set { defaultExcludeLinesLongerThanMsNumSubs1 = value; }
    }

    public static int DefaultExcludeLinesLongerThanMsNumSubs2
    {
      get { return defaultExcludeLinesLongerThanMsNumSubs2; }
      set { defaultExcludeLinesLongerThanMsNumSubs2 = value; }
    }

    public static bool DefaultJoinSentencesSubs1
    {
      get { return defaultJoinSentencesSubs1; }
      set { defaultJoinSentencesSubs1 = value; }
    }

    public static bool DefaultJoinSentencesSubs2
    {
      get { return defaultJoinSentencesSubs2; }
      set { defaultJoinSentencesSubs2 = value; }
    }

    public static string DefaultJoinSentencesCharListSubs1
    {
      get { return defaultJoinSentencesCharListSubs1; }
      set { defaultJoinSentencesCharListSubs1 = value; }
    }

    public static string DefaultJoinSentencesCharListSubs2
    {
      get { return defaultJoinSentencesCharListSubs2; }
      set { defaultJoinSentencesCharListSubs2 = value; }
    }

    public static bool DefaultExcludeDuplicateLinesSubs1
    {
      get { return defaultExcludeDuplicateLinesSubs1; }
      set { defaultExcludeDuplicateLinesSubs1 = value; }
    }

    public static bool DefaultExcludeDuplicateLinesSubs2
    {
      get { return defaultExcludeDuplicateLinesSubs2; }
      set { defaultExcludeDuplicateLinesSubs2 = value; }
    }  

    public static string SrsFilenameFormat
    {
      get { return srsFilenameFormat; }
      set { srsFilenameFormat = value; }
    } 

    public static string SrsDelimiter
    {
      get { return srsDelimiter; }
      set { srsDelimiter = value; }
    }   

    public static string SrsTagFormat
    {
      get { return srsTagFormat; }
      set { srsTagFormat = value; }
    }

    public static string SrsSequenceMarkerFormat
    {
      get { return srsSequenceMarkerFormat; }
      set { srsSequenceMarkerFormat = value; }
    }

    public static string SrsAudioFilenamePrefix
    {
      get { return srsAudioFilenamePrefix; }
      set { srsAudioFilenamePrefix = value; }
    }

    public static string SrsAudioFilenameSuffix
    {
      get { return srsAudioFilenameSuffix; }
      set { srsAudioFilenameSuffix = value; }
    }

    public static string SrsSnapshotFilenamePrefix
    {
      get { return srsSnapshotFilenamePrefix; }
      set { srsSnapshotFilenamePrefix = value; }
    }

    public static string SrsSnapshotFilenameSuffix
    {
      get { return srsSnapshotFilenameSuffix; }
      set { srsSnapshotFilenameSuffix = value; }
    }

    public static string SrsVideoFilenamePrefix
    {
      get { return srsVideoFilenamePrefix; }
      set { srsVideoFilenamePrefix = value; }
    }

    public static string SrsVideoFilenameSuffix
    {
      get { return srsVideoFilenameSuffix; }
      set { srsVideoFilenameSuffix = value; }
    }

    public static string SrsSubs1Format
    {
      get { return srsSubs1Format; }
      set { srsSubs1Format = value; }
    }

    public static string SrsSubs2Format
    {
      get { return srsSubs2Format; }
      set { srsSubs2Format = value; }
    }

    public static string SrsVobsubFilenamePrefix
    {
      get { return srsVobsubFilenamePrefix; }
      set { srsVobsubFilenamePrefix = value; }
    }

    public static string SrsVobsubFilenameSuffix
    {
      get { return srsVobsubFilenameSuffix; }
      set { srsVobsubFilenameSuffix = value; }
    }

    public static string AudioFilenameFormat
    {
      get { return audioFilenameFormat; }
      set { audioFilenameFormat = value; }
    }

    public static string SnapshotFilenameFormat
    {
      get { return snapshotFilenameFormat; }
      set { snapshotFilenameFormat = value; }
    }

    public static string VideoFilenameFormat
    {
      get { return videoFilenameFormat; }
      set { videoFilenameFormat = value; }
    }

    public static string VobsubFilenameFormat
    {
      get { return vobsubFilenameFormat; }
      set { vobsubFilenameFormat = value; }
    }

    public static string AudioId3Artist
    {
      get { return audioId3Artist; }
      set { audioId3Artist = value; }
    }

    public static string AudioId3Album
    {
      get { return audioId3Album; }
      set { audioId3Album = value; }
    }

    public static string AudioId3Title
    {
      get { return audioId3Title; }
      set { audioId3Title = value; }
    }

    public static string AudioId3Genre
    {
      get { return audioId3Genre; }
      set { audioId3Genre = value; }
    }

    public static string AudioId3Lyrics
    {
      get { return audioId3Lyrics; }
      set { audioId3Lyrics = value; }
    }

    public static string ExtractMediaAudioFilenameFormat
    {
      get { return extractMediaAudioFilenameFormat; }
      set { extractMediaAudioFilenameFormat = value; }
    }

    public static string ExtractMediaLyricsSubs1Format
    {
      get { return extractMediaLyricsSubs1Format; }
      set { extractMediaLyricsSubs1Format = value; }
    }

    public static string ExtractMediaLyricsSubs2Format
    {
      get { return extractMediaLyricsSubs2Format; }
      set { extractMediaLyricsSubs2Format = value; }
    }

    public static string DuelingSubtitleFilenameFormat
    {
      get { return duelingSubtitleFilenameFormat; }
      set { duelingSubtitleFilenameFormat = value; }
    }

    public static string DuelingQuickRefFilenameFormat
    {
      get { return duelingQuickRefFilenameFormat; }
      set { duelingQuickRefFilenameFormat = value; }
    }

    public static string DuelingQuickRefSubs1Format
    {
      get { return duelingQuickRefSubs1Format; }
      set { duelingQuickRefSubs1Format = value; }
    }

    public static string DuelingQuickRefSubs2Format
    {
      get { return duelingQuickRefSubs2Format; }
      set { duelingQuickRefSubs2Format = value; }
    }
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
    public string filePattern { get; set; } = "";

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
      Subs = settings.subs;
      Subs[0].Files = Array.Empty<string>();
      Subs[1].Files = Array.Empty<string>();
      VideoClips = settings.videoClips;
      VideoClips.Files = Array.Empty<string>();
      AudioClips = settings.audioClips;
      AudioClips.Files = Array.Empty<string>();
      Snapshots = settings.snapshots;
      VobSubColors = settings.vobSubColors;
      LanguageSpecific = settings.languageSpecific;

      OutputDir = settings.outputDir;

      TimeShiftEnabled = settings.timeShiftEnabled;

      SpanEnabled = settings.spanEnabled;
      SpanStart = settings.spanStart;
      SpanEnd = settings.spanEnd;

      DeckName = settings.deckName;
      EpisodeStartNumber = settings.episodeStartNumber;

      ActorList = settings.actorList;

      ContextLeadingCount = settings.contextLeadingCount;
      ContextLeadingIncludeSnapshots = settings.contextLeadingIncludeSnapshots;
      ContextLeadingIncludeAudioClips = settings.contextLeadingIncludeAudioClips;
      ContextLeadingIncludeVideoClips = settings.contextLeadingIncludeVideoClips;
      ContextLeadingRange = settings.contextLeadingRange;

      ContextTrailingCount = settings.contextTrailingCount;
      ContextTrailingIncludeSnapshots = settings.contextTrailingIncludeSnapshots;
      ContextTrailingIncludeAudioClips = settings.contextTrailingIncludeAudioClips;
      ContextTrailingIncludeVideoClips = settings.contextTrailingIncludeVideoClips;
      ContextTrailingRange = settings.contextTrailingRange;
    }

    public void reset()
    {
      loadSettings(new SaveSettings());
    }
  }


  public class SaveSettings
  {
    public SubSettings[] subs;
    public VideoClips videoClips;
    public AudioClips audioClips;
    public Snapshots snapshots;
    public VobSubColors vobSubColors;

    [JsonPropertyName("langaugeSpecific")]
    public LanguageSpecific languageSpecific;

    public string outputDir;
    public bool timeShiftEnabled;
    public bool spanEnabled;
    public DateTime spanStart;
    public DateTime spanEnd;
    public string deckName;
    public int episodeStartNumber;
    public List<string> actorList;

    public int contextLeadingCount;
    public bool contextLeadingIncludeSnapshots;
    public bool contextLeadingIncludeAudioClips;
    public bool contextLeadingIncludeVideoClips;
    public int contextLeadingRange;

    public int contextTrailingCount;
    public bool contextTrailingIncludeSnapshots;
    public bool contextTrailingIncludeAudioClips;
    public bool contextTrailingIncludeVideoClips;
    public int contextTrailingRange;

    public SaveSettings()
    {
      subs = new SubSettings[2];
      subs[0] = new SubSettings();
      subs[1] = new SubSettings();
      subs[0].ActorsEnabled = true;
      subs[0].TimingsEnabled = true;

      videoClips = new VideoClips();
      audioClips = new AudioClips();
      snapshots = new Snapshots();
      vobSubColors = new VobSubColors();
      languageSpecific = new LanguageSpecific();
      outputDir = "";
      timeShiftEnabled = false;
      spanEnabled = false;
      spanStart = new DateTime().AddMilliseconds(90_000);
      spanEnd = new DateTime().AddMilliseconds(1_350_000);
      deckName = "";
      episodeStartNumber = 1;

      actorList = new List<string>();

      contextLeadingCount = 0;
      contextLeadingIncludeSnapshots = false;
      contextLeadingIncludeAudioClips = false;
      contextLeadingIncludeVideoClips = false;
      contextLeadingRange = 15;

      contextTrailingCount = 0;
      contextTrailingIncludeSnapshots = false;
      contextTrailingIncludeAudioClips = false;
      contextTrailingIncludeVideoClips = false;
      contextTrailingRange = 15;
    }

    public void gatherData()
    {
      subs = Settings.Instance.Subs;
      videoClips = Settings.Instance.VideoClips;
      audioClips = Settings.Instance.AudioClips;
      snapshots = Settings.Instance.Snapshots;
      vobSubColors = Settings.Instance.VobSubColors;
      languageSpecific = Settings.Instance.LanguageSpecific;

      outputDir = Settings.Instance.OutputDir;

      timeShiftEnabled = Settings.Instance.TimeShiftEnabled;

      spanEnabled = Settings.Instance.SpanEnabled;
      spanStart = Settings.Instance.SpanStart;
      spanEnd = Settings.Instance.SpanEnd;

      deckName = Settings.Instance.DeckName;
      episodeStartNumber = Settings.Instance.EpisodeStartNumber;

      actorList = Settings.Instance.ActorList;

      contextLeadingCount = Settings.Instance.ContextLeadingCount;
      contextLeadingIncludeSnapshots = Settings.Instance.ContextLeadingIncludeSnapshots;
      contextLeadingIncludeAudioClips = Settings.Instance.ContextLeadingIncludeAudioClips;
      contextLeadingIncludeVideoClips = Settings.Instance.ContextLeadingIncludeVideoClips;
      contextLeadingRange = Settings.Instance.ContextLeadingRange;

      contextTrailingCount = Settings.Instance.ContextTrailingCount;
      contextTrailingIncludeSnapshots = Settings.Instance.ContextTrailingIncludeSnapshots;
      contextTrailingIncludeAudioClips = Settings.Instance.ContextTrailingIncludeAudioClips;
      contextTrailingIncludeVideoClips = Settings.Instance.ContextTrailingIncludeVideoClips;
      contextTrailingRange = Settings.Instance.ContextTrailingRange;
    }
  }
}
