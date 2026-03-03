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
using System.Text.RegularExpressions;
using System.Text;

namespace subs2srs
{
  /// <summary>
  /// Used for reading/writing to the settings file.
  /// </summary>
  public class PrefIO
  {
    /// <summary>
    /// Represents a key/value pair of settings
    /// </summary>
    public class SettingsPair
    {
      public string Key { get; set; }
      public string Value { get; set; }

      public SettingsPair(string key, string value)
      {
        this.Key = key;
        this.Value = value;
      }
    }

    /// <summary>
    /// Write a list of settings to the settings file.
    /// </summary>
    public static void writeString(List<SettingsPair> settingsList)
    {
      try
      {
        string contents = File.ReadAllText(ConstantSettings.SettingsFilename, Encoding.UTF8);

        // Replace each setting in list
        foreach (SettingsPair pair in settingsList)
        {
          string regex = "^" + pair.Key + @"\s*?=.*$";
          string replacement = pair.Key + " = " + pair.Value;
          contents = Regex.Replace(contents, regex, replacement, RegexOptions.Multiline);
        }

        File.WriteAllText(ConstantSettings.SettingsFilename, contents, Encoding.UTF8);
      }
      catch
      {
        UtilsMsg.showErrMsg("Unable to find the preferences file: '" + ConstantSettings.SettingsFilename + "'\r\nPreferences will not be saved.");
      }
    }

    /// <summary>
    /// Convert a string value to token representation for storage.
    /// Empty strings become "none".
    /// </summary>
    private static string Tok(string val)
    {
      if (string.IsNullOrEmpty(val))
        return "none";
      return val.Replace("\t", "${tab}").Replace("\r", "${cr}").Replace("\n", "${lf}");
    }

    /// <summary>
    /// Create a default preferences file with all settings at their default values.
    /// Must contain every key that read() expects, otherwise writeString() regex won't match.
    /// </summary>
    public static void writeDefaultPreferences()
    {
      try
      {
        var dir = Path.GetDirectoryName(ConstantSettings.SettingsFilename);
        if (!string.IsNullOrEmpty(dir))
          Directory.CreateDirectory(dir);

        var sb = new StringBuilder();
        sb.AppendLine("# subs2srs preferences file");
        sb.AppendLine("# Auto-generated with default values");
        sb.AppendLine();

        sb.AppendLine($"main_window_width = {PrefDefaults.MainWindowWidth}");
        sb.AppendLine($"main_window_height = {PrefDefaults.MainWindowHeight}");
        sb.AppendLine();

        sb.AppendLine($"default_enable_audio_clip_generation = {PrefDefaults.DefaultEnableAudioClipGeneration}");
        sb.AppendLine($"default_enable_snapshots_generation = {PrefDefaults.DefaultEnableSnapshotsGeneration}");
        sb.AppendLine($"default_enable_video_clips_generation = {PrefDefaults.DefaultEnableVideoClipsGeneration}");
        sb.AppendLine();

        sb.AppendLine($"video_player = {Tok(PrefDefaults.VideoPlayer)}");
        sb.AppendLine($"video_player_args = {Tok(PrefDefaults.VideoPlayerArgs)}");
        sb.AppendLine();

        sb.AppendLine($"reencode_before_splitting_audio = {PrefDefaults.ReencodeBeforeSplittingAudio}");
        sb.AppendLine($"enable_logging = {PrefDefaults.EnableLogging}");
        sb.AppendLine($"audio_normalize_args = {Tok(PrefDefaults.AudioNormalizeArgs)}");
        sb.AppendLine($"long_clip_warning_seconds = {PrefDefaults.LongClipWarningSeconds}");
        sb.AppendLine();

        sb.AppendLine($"default_audio_clip_bitrate = {PrefDefaults.DefaultAudioClipBitrate}");
        sb.AppendLine($"default_audio_normalize = {PrefDefaults.DefaultAudioNormalize}");
        sb.AppendLine();

        sb.AppendLine($"default_video_clip_video_bitrate = {PrefDefaults.DefaultVideoClipVideoBitrate}");
        sb.AppendLine($"default_video_clip_audio_bitrate = {PrefDefaults.DefaultVideoClipAudioBitrate}");
        sb.AppendLine($"default_ipod_support = {PrefDefaults.DefaultIphoneSupport}");
        sb.AppendLine();

        sb.AppendLine($"default_encoding_subs1 = {Tok(PrefDefaults.DefaultEncodingSubs1)}");
        sb.AppendLine($"default_encoding_subs2 = {Tok(PrefDefaults.DefaultEncodingSubs2)}");
        sb.AppendLine();

        sb.AppendLine($"default_context_num_leading = {PrefDefaults.DefaultContextNumLeading}");
        sb.AppendLine($"default_context_num_trailing = {PrefDefaults.DefaultContextNumTrailing}");
        sb.AppendLine($"default_context_leading_range = {PrefDefaults.DefaultContextLeadingRange}");
        sb.AppendLine($"default_context_trailing_range = {PrefDefaults.DefaultContextTrailingRange}");
        sb.AppendLine();

        sb.AppendLine($"default_remove_styled_lines_subs1 = {PrefDefaults.DefaultRemoveStyledLinesSubs1}");
        sb.AppendLine($"default_remove_styled_lines_subs2 = {PrefDefaults.DefaultRemoveStyledLinesSubs2}");
        sb.AppendLine($"default_remove_no_counterpart_subs1 = {PrefDefaults.DefaultRemoveNoCounterpartSubs1}");
        sb.AppendLine($"default_remove_no_counterpart_subs2 = {PrefDefaults.DefaultRemoveNoCounterpartSubs2}");
        sb.AppendLine();

        sb.AppendLine($"default_included_text_subs1 = {Tok(PrefDefaults.DefaultIncludeTextSubs1)}");
        sb.AppendLine($"default_included_text_subs2 = {Tok(PrefDefaults.DefaultIncludeTextSubs2)}");
        sb.AppendLine($"default_excluded_text_subs1 = {Tok(PrefDefaults.DefaultExcludeTextSubs1)}");
        sb.AppendLine($"default_excluded_text_subs2 = {Tok(PrefDefaults.DefaultExcludeTextSubs2)}");
        sb.AppendLine();

        sb.AppendLine($"default_exclude_duplicate_lines_subs1 = {PrefDefaults.DefaultExcludeDuplicateLinesSubs1}");
        sb.AppendLine($"default_exclude_duplicate_lines_subs2 = {PrefDefaults.DefaultExcludeDuplicateLinesSubs2}");
        sb.AppendLine();

        sb.AppendLine($"default_exclude_lines_with_fewer_than_n_chars_subs1 = {PrefDefaults.DefaultExcludeLinesFewerThanCharsSubs1}");
        sb.AppendLine($"default_exclude_lines_with_fewer_than_n_chars_subs2 = {PrefDefaults.DefaultExcludeLinesFewerThanCharsSubs2}");
        sb.AppendLine($"default_exclude_lines_with_fewer_than_n_chars_num_subs1 = {PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs1}");
        sb.AppendLine($"default_exclude_lines_with_fewer_than_n_chars_num_subs2 = {PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs2}");
        sb.AppendLine();

        sb.AppendLine($"default_exclude_lines_shorter_than_n_ms_subs1 = {PrefDefaults.DefaultExcludeLinesShorterThanMsSubs1}");
        sb.AppendLine($"default_exclude_lines_shorter_than_n_ms_subs2 = {PrefDefaults.DefaultExcludeLinesShorterThanMsSubs2}");
        sb.AppendLine($"default_exclude_lines_shorter_than_n_ms_num_subs1 = {PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs1}");
        sb.AppendLine($"default_exclude_lines_shorter_than_n_ms_num_subs2 = {PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs2}");
        sb.AppendLine();

        sb.AppendLine($"default_exclude_lines_longer_than_n_ms_subs1 = {PrefDefaults.DefaultExcludeLinesLongerThanMsSubs1}");
        sb.AppendLine($"default_exclude_lines_longer_than_n_ms_subs2 = {PrefDefaults.DefaultExcludeLinesLongerThanMsSubs2}");
        sb.AppendLine($"default_exclude_lines_longer_than_n_ms_num_subs1 = {PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs1}");
        sb.AppendLine($"default_exclude_lines_longer_than_n_ms_num_subs2 = {PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs2}");
        sb.AppendLine();

        sb.AppendLine($"default_join_sentences_subs1 = {PrefDefaults.DefaultJoinSentencesSubs1}");
        sb.AppendLine($"default_join_sentences_subs2 = {PrefDefaults.DefaultJoinSentencesSubs2}");
        sb.AppendLine($"default_join_sentences_char_list_subs1 = {Tok(PrefDefaults.DefaultJoinSentencesCharListSubs1)}");
        sb.AppendLine($"default_join_sentences_char_list_subs2 = {Tok(PrefDefaults.DefaultJoinSentencesCharListSubs2)}");
        sb.AppendLine();

        sb.AppendLine($"default_file_browser_start_dir = {Tok(PrefDefaults.DefaultFileBrowserStartDir)}");
        sb.AppendLine();

        sb.AppendLine($"srs_filename_format = {Tok(PrefDefaults.SrsFilenameFormat)}");
        sb.AppendLine($"srs_delimiter = {Tok(PrefDefaults.SrsDelimiter)}");
        sb.AppendLine($"srs_tag_format = {Tok(PrefDefaults.SrsTagFormat)}");
        sb.AppendLine($"srs_sequence_marker_format = {Tok(PrefDefaults.SrsSequenceMarkerFormat)}");
        sb.AppendLine();

        sb.AppendLine($"srs_audio_filename_prefix = {Tok(PrefDefaults.SrsAudioFilenamePrefix)}");
        sb.AppendLine($"audio_filename_format = {Tok(PrefDefaults.AudioFilenameFormat)}");
        sb.AppendLine($"audio_id3_artist = {Tok(PrefDefaults.AudioId3Artist)}");
        sb.AppendLine($"audio_id3_album = {Tok(PrefDefaults.AudioId3Album)}");
        sb.AppendLine($"audio_id3_title = {Tok(PrefDefaults.AudioId3Title)}");
        sb.AppendLine($"audio_id3_genre = {Tok(PrefDefaults.AudioId3Genre)}");
        sb.AppendLine($"audio_id3_lyrics = {Tok(PrefDefaults.AudioId3Lyrics)}");
        sb.AppendLine($"srs_audio_filename_suffix = {Tok(PrefDefaults.SrsAudioFilenameSuffix)}");
        sb.AppendLine();

        sb.AppendLine($"srs_snapshot_filename_prefix = {Tok(PrefDefaults.SrsSnapshotFilenamePrefix)}");
        sb.AppendLine($"snapshot_filename_format = {Tok(PrefDefaults.SnapshotFilenameFormat)}");
        sb.AppendLine($"srs_snapshot_filename_suffix = {Tok(PrefDefaults.SrsSnapshotFilenameSuffix)}");
        sb.AppendLine();

        sb.AppendLine($"srs_video_filename_prefix = {Tok(PrefDefaults.SrsVideoFilenamePrefix)}");
        sb.AppendLine($"video_filename_format = {Tok(PrefDefaults.VideoFilenameFormat)}");
        sb.AppendLine($"srs_video_filename_suffix = {Tok(PrefDefaults.SrsVideoFilenameSuffix)}");
        sb.AppendLine();

        sb.AppendLine($"srs_subs1_format = {Tok(PrefDefaults.SrsSubs1Format)}");
        sb.AppendLine($"srs_subs2_format = {Tok(PrefDefaults.SrsSubs2Format)}");
        sb.AppendLine();

        sb.AppendLine($"extract_media_audio_filename_format = {Tok(PrefDefaults.ExtractMediaAudioFilenameFormat)}");
        sb.AppendLine($"extract_media_lyrics_subs1_format = {Tok(PrefDefaults.ExtractMediaLyricsSubs1Format)}");
        sb.AppendLine($"extract_media_lyrics_subs2_format = {Tok(PrefDefaults.ExtractMediaLyricsSubs2Format)}");
        sb.AppendLine();

        sb.AppendLine($"dueling_subtitle_filename_format = {Tok(PrefDefaults.DuelingSubtitleFilenameFormat)}");
        sb.AppendLine($"dueling_quick_ref_filename_format = {Tok(PrefDefaults.DuelingQuickRefFilenameFormat)}");
        sb.AppendLine($"dueling_quick_ref_subs1_format = {Tok(PrefDefaults.DuelingQuickRefSubs1Format)}");
        sb.AppendLine($"dueling_quick_ref_subs2_format = {Tok(PrefDefaults.DuelingQuickRefSubs2Format)}");
        sb.AppendLine();

        sb.AppendLine($"srs_vobsub_filename_prefix = {Tok(PrefDefaults.SrsVobsubFilenamePrefix)}");
        sb.AppendLine($"vobsub_filename_format = {Tok(PrefDefaults.VobsubFilenameFormat)}");
        sb.AppendLine($"srs_vobsub_filename_suffix = {Tok(PrefDefaults.SrsVobsubFilenameSuffix)}");

        File.WriteAllText(ConstantSettings.SettingsFilename, sb.ToString(), Encoding.UTF8);
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine($"Warning: could not create default preferences file: {ex.Message}");
      }
    }


    /// <summary>
    /// Read a string setting.
    /// </summary>
    public static string getString(string key, string def)
    {
      string value = "";

      try
      {
        using var settingsFile = new StreamReader(ConstantSettings.SettingsFilename, Encoding.UTF8);
        string fileLine;

        // Read each line of the settings file, try to find the key and its value
        while ((fileLine = settingsFile.ReadLine()) != null)
        {
          Match lineMatch = Regex.Match(fileLine,
            @"^(?<Key>" + key + @")\s*=\s*(?<Value>.*)",
            RegexOptions.IgnoreCase);

          if (lineMatch.Success)
          {
            string settingsKey = lineMatch.Groups["Key"].ToString().Trim();

            // Does this line contain the search key?
            if (settingsKey.ToLower() == key.ToLower())
            {
              // Extract the value from the line
              value = convertFromTokens(lineMatch.Groups["Value"].ToString().Trim());
              break;
            }
          }
        }

        // If the value is set to "none", blank it
        if (value.ToLower() == "none")
        {
          value = "";
        }
        else if (value == "") // else if the key is not found, use the default
        {
          value = def;
        }
      }
      catch
      {
        value = def;
      }

      return value;
    }


    /// <summary>
    /// Read a boolean setting.
    /// </summary>
    public static bool getBool(string key, bool def)
    {
      string defString = def.ToString();
      string valString = PrefIO.getString(key, defString);

      return valString.Equals("true", StringComparison.OrdinalIgnoreCase);
    }


    /// <summary>
    /// Read an integer setting.
    /// </summary>
    public static int getInt(string key, int def)
    {
      string valString = PrefIO.getString(key, def.ToString());

      return int.TryParse(valString, out int result) ? result : def;
    }


    /// <summary>
    /// Read a float setting.
    /// </summary>
    public static float getFloat(string key, float def)
    {
      string valString = PrefIO.getString(key, def.ToString());

      try
      {
        return (float)UtilsLang.toDouble(valString);
      }
      catch
      {
        return def;
      }
    }


    /// <summary>
    /// Replace tabs and newline tokens with their with real tabs and newlines.
    /// </summary>
    public static string convertFromTokens(string format)
    {
      string newFormat = format;

      newFormat = newFormat.Replace("${tab}", "\t");
      newFormat = newFormat.Replace("${cr}", "\r");
      newFormat = newFormat.Replace("${lf}", "\n");
    
      return newFormat;
    }


    /// <summary>
    /// Read all the settings in the settings file and store globally.
    /// One pass: file is read once into a dictionary, then all keys are looked up in memory.
    /// </summary>
    public static void read()
    {
      // Create default preferences file on first launch
      if (!File.Exists(ConstantSettings.SettingsFilename))
      {
        writeDefaultPreferences();
      }

      // Read entire file once into a dictionary
      var pairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      try
      {
        foreach (var line in File.ReadAllLines(ConstantSettings.SettingsFilename, Encoding.UTF8))
        {
          if (line.TrimStart().StartsWith('#')) continue;
          int eqIdx = line.IndexOf('=');
          if (eqIdx < 0) continue;
          string key = line[..eqIdx].Trim();
          if (key.Length > 0)
            pairs[key] = line[(eqIdx + 1)..].Trim();
        }
      }
      catch
      {
        // If file can't be read, all settings will use defaults
      }

      // Local lookup helpers — no file I/O
      string getStr(string key, string def)
      {
        if (!pairs.TryGetValue(key, out string raw))
          return def;
        string val = convertFromTokens(raw);
        if (val.Equals("none", StringComparison.OrdinalIgnoreCase))
          return "";
        return val.Length == 0 ? def : val;
      }

      bool getBl(string key, bool def) =>
        getStr(key, def.ToString()).Equals("true", StringComparison.OrdinalIgnoreCase);

      int getI(string key, int def) =>
        int.TryParse(getStr(key, def.ToString()), out int v) ? v : def;

      // --- Apply all settings ---

      ConstantSettings.MainWindowWidth = getI("main_window_width", PrefDefaults.MainWindowWidth);
      ConstantSettings.MainWindowHeight = getI("main_window_height", PrefDefaults.MainWindowHeight);

      ConstantSettings.DefaultEnableAudioClipGeneration = getBl("default_enable_audio_clip_generation", PrefDefaults.DefaultEnableAudioClipGeneration);
      ConstantSettings.DefaultEnableSnapshotsGeneration = getBl("default_enable_snapshots_generation", PrefDefaults.DefaultEnableSnapshotsGeneration);
      ConstantSettings.DefaultEnableVideoClipsGeneration = getBl("default_enable_video_clips_generation", PrefDefaults.DefaultEnableVideoClipsGeneration);

      ConstantSettings.VideoPlayer = getStr("video_player", PrefDefaults.VideoPlayer);
      ConstantSettings.VideoPlayerArgs = getStr("video_player_args", PrefDefaults.VideoPlayerArgs);

      ConstantSettings.ReencodeBeforeSplittingAudio = getBl("reencode_before_splitting_audio", PrefDefaults.ReencodeBeforeSplittingAudio);
      ConstantSettings.EnableLogging = getBl("enable_logging", PrefDefaults.EnableLogging);
      ConstantSettings.AudioNormalizeArgs = getStr("audio_normalize_args", PrefDefaults.AudioNormalizeArgs);
      ConstantSettings.LongClipWarningSeconds = getI("long_clip_warning_seconds", PrefDefaults.LongClipWarningSeconds);

      ConstantSettings.DefaultAudioClipBitrate = getI("default_audio_clip_bitrate", PrefDefaults.DefaultAudioClipBitrate);
      ConstantSettings.DefaultAudioNormalize = getBl("default_audio_normalize", PrefDefaults.DefaultAudioNormalize);

      ConstantSettings.DefaultVideoClipVideoBitrate = getI("default_video_clip_video_bitrate", PrefDefaults.DefaultVideoClipVideoBitrate);
      ConstantSettings.DefaultVideoClipAudioBitrate = getI("default_video_clip_audio_bitrate", PrefDefaults.DefaultVideoClipAudioBitrate);
      ConstantSettings.DefaultIphoneSupport = getBl("default_ipod_support", PrefDefaults.DefaultIphoneSupport);

      ConstantSettings.DefaultEncodingSubs1 = getStr("default_encoding_subs1", PrefDefaults.DefaultEncodingSubs1);
      ConstantSettings.DefaultEncodingSubs2 = getStr("default_encoding_subs2", PrefDefaults.DefaultEncodingSubs2);

      ConstantSettings.DefaultContextNumLeading = getI("default_context_num_leading", PrefDefaults.DefaultContextNumLeading);
      ConstantSettings.DefaultContextNumTrailing = getI("default_context_num_trailing", PrefDefaults.DefaultContextNumTrailing);

      ConstantSettings.DefaultContextLeadingRange = getI("default_context_leading_range", PrefDefaults.DefaultContextLeadingRange);
      ConstantSettings.DefaultContextTrailingRange = getI("default_context_trailing_range", PrefDefaults.DefaultContextTrailingRange);

      ConstantSettings.DefaultRemoveStyledLinesSubs1 = getBl("default_remove_styled_lines_subs1", PrefDefaults.DefaultRemoveStyledLinesSubs1);
      ConstantSettings.DefaultRemoveStyledLinesSubs2 = getBl("default_remove_styled_lines_subs2", PrefDefaults.DefaultRemoveStyledLinesSubs2); // FIX: was Subs1

      ConstantSettings.DefaultRemoveNoCounterpartSubs1 = getBl("default_remove_no_counterpart_subs1", PrefDefaults.DefaultRemoveNoCounterpartSubs1);
      ConstantSettings.DefaultRemoveNoCounterpartSubs2 = getBl("default_remove_no_counterpart_subs2", PrefDefaults.DefaultRemoveNoCounterpartSubs2);

      ConstantSettings.DefaultIncludeTextSubs1 = getStr("default_included_text_subs1", PrefDefaults.DefaultIncludeTextSubs1);
      ConstantSettings.DefaultIncludeTextSubs2 = getStr("default_included_text_subs2", PrefDefaults.DefaultIncludeTextSubs2);

      ConstantSettings.DefaultExcludeTextSubs1 = getStr("default_excluded_text_subs1", PrefDefaults.DefaultExcludeTextSubs1);
      ConstantSettings.DefaultExcludeTextSubs2 = getStr("default_excluded_text_subs2", PrefDefaults.DefaultExcludeTextSubs2);

      ConstantSettings.DefaultExcludeDuplicateLinesSubs1 = getBl("default_exclude_duplicate_lines_subs1", PrefDefaults.DefaultExcludeDuplicateLinesSubs1);
      ConstantSettings.DefaultExcludeDuplicateLinesSubs2 = getBl("default_exclude_duplicate_lines_subs2", PrefDefaults.DefaultExcludeDuplicateLinesSubs2);

      ConstantSettings.DefaultExcludeLinesFewerThanCharsSubs1 = getBl("default_exclude_lines_with_fewer_than_n_chars_subs1", PrefDefaults.DefaultExcludeLinesFewerThanCharsSubs1);
      ConstantSettings.DefaultExcludeLinesFewerThanCharsSubs2 = getBl("default_exclude_lines_with_fewer_than_n_chars_subs2", PrefDefaults.DefaultExcludeLinesFewerThanCharsSubs2);
      ConstantSettings.DefaultExcludeLinesFewerThanCharsNumSubs1 = getI("default_exclude_lines_with_fewer_than_n_chars_num_subs1", PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs1);
      ConstantSettings.DefaultExcludeLinesFewerThanCharsNumSubs2 = getI("default_exclude_lines_with_fewer_than_n_chars_num_subs2", PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs2);

      ConstantSettings.DefaultExcludeLinesShorterThanMsSubs1 = getBl("default_exclude_lines_shorter_than_n_ms_subs1", PrefDefaults.DefaultExcludeLinesShorterThanMsSubs1);
      ConstantSettings.DefaultExcludeLinesShorterThanMsSubs2 = getBl("default_exclude_lines_shorter_than_n_ms_subs2", PrefDefaults.DefaultExcludeLinesShorterThanMsSubs2);
      ConstantSettings.DefaultExcludeLinesShorterThanMsNumSubs1 = getI("default_exclude_lines_shorter_than_n_ms_num_subs1", PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs1);
      ConstantSettings.DefaultExcludeLinesShorterThanMsNumSubs2 = getI("default_exclude_lines_shorter_than_n_ms_num_subs2", PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs2);

      ConstantSettings.DefaultExcludeLinesLongerThanMsSubs1 = getBl("default_exclude_lines_longer_than_n_ms_subs1", PrefDefaults.DefaultExcludeLinesLongerThanMsSubs1);
      ConstantSettings.DefaultExcludeLinesLongerThanMsSubs2 = getBl("default_exclude_lines_longer_than_n_ms_subs2", PrefDefaults.DefaultExcludeLinesLongerThanMsSubs2);
      ConstantSettings.DefaultExcludeLinesLongerThanMsNumSubs1 = getI("default_exclude_lines_longer_than_n_ms_num_subs1", PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs1);
      ConstantSettings.DefaultExcludeLinesLongerThanMsNumSubs2 = getI("default_exclude_lines_longer_than_n_ms_num_subs2", PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs2);

      ConstantSettings.DefaultJoinSentencesSubs1 = getBl("default_join_sentences_subs1", PrefDefaults.DefaultJoinSentencesSubs1);
      ConstantSettings.DefaultJoinSentencesSubs2 = getBl("default_join_sentences_subs2", PrefDefaults.DefaultJoinSentencesSubs2);
      ConstantSettings.DefaultJoinSentencesCharListSubs1 = getStr("default_join_sentences_char_list_subs1", PrefDefaults.DefaultJoinSentencesCharListSubs1);
      ConstantSettings.DefaultJoinSentencesCharListSubs2 = getStr("default_join_sentences_char_list_subs2", PrefDefaults.DefaultJoinSentencesCharListSubs2);

      ConstantSettings.DefaultFileBrowserStartDir = getStr("default_file_browser_start_dir", PrefDefaults.DefaultFileBrowserStartDir);

      ConstantSettings.SrsFilenameFormat = getStr("srs_filename_format", PrefDefaults.SrsFilenameFormat);

      ConstantSettings.SrsDelimiter = getStr("srs_delimiter", PrefDefaults.SrsDelimiter);

      ConstantSettings.SrsTagFormat = getStr("srs_tag_format", PrefDefaults.SrsTagFormat);
      ConstantSettings.SrsSequenceMarkerFormat = getStr("srs_sequence_marker_format", PrefDefaults.SrsSequenceMarkerFormat);

      ConstantSettings.SrsAudioFilenamePrefix = getStr("srs_audio_filename_prefix", PrefDefaults.SrsAudioFilenamePrefix);
      ConstantSettings.AudioFilenameFormat = getStr("audio_filename_format", PrefDefaults.AudioFilenameFormat);
      ConstantSettings.AudioId3Artist = getStr("audio_id3_artist", PrefDefaults.AudioId3Artist);
      ConstantSettings.AudioId3Album = getStr("audio_id3_album", PrefDefaults.AudioId3Album);
      ConstantSettings.AudioId3Title = getStr("audio_id3_title", PrefDefaults.AudioId3Title);
      ConstantSettings.AudioId3Genre = getStr("audio_id3_genre", PrefDefaults.AudioId3Genre);
      ConstantSettings.AudioId3Lyrics = getStr("audio_id3_lyrics", PrefDefaults.AudioId3Lyrics);
      ConstantSettings.SrsAudioFilenameSuffix = getStr("srs_audio_filename_suffix", PrefDefaults.SrsAudioFilenameSuffix);

      ConstantSettings.SrsSnapshotFilenamePrefix = getStr("srs_snapshot_filename_prefix", PrefDefaults.SrsSnapshotFilenamePrefix);
      ConstantSettings.SnapshotFilenameFormat = getStr("snapshot_filename_format", PrefDefaults.SnapshotFilenameFormat);
      ConstantSettings.SrsSnapshotFilenameSuffix = getStr("srs_snapshot_filename_suffix", PrefDefaults.SrsSnapshotFilenameSuffix);

      ConstantSettings.SrsVideoFilenamePrefix = getStr("srs_video_filename_prefix", PrefDefaults.SrsVideoFilenamePrefix);
      ConstantSettings.VideoFilenameFormat = getStr("video_filename_format", PrefDefaults.VideoFilenameFormat);
      ConstantSettings.SrsVideoFilenameSuffix = getStr("srs_video_filename_suffix", PrefDefaults.SrsVideoFilenameSuffix);

      ConstantSettings.SrsSubs1Format = getStr("srs_subs1_format", PrefDefaults.SrsSubs1Format);
      ConstantSettings.SrsSubs2Format = getStr("srs_subs2_format", PrefDefaults.SrsSubs2Format);

      ConstantSettings.ExtractMediaAudioFilenameFormat = getStr("extract_media_audio_filename_format", PrefDefaults.ExtractMediaAudioFilenameFormat);
      ConstantSettings.ExtractMediaLyricsSubs1Format = getStr("extract_media_lyrics_subs1_format", PrefDefaults.ExtractMediaLyricsSubs1Format);
      ConstantSettings.ExtractMediaLyricsSubs2Format = getStr("extract_media_lyrics_subs2_format", PrefDefaults.ExtractMediaLyricsSubs2Format);

      ConstantSettings.DuelingSubtitleFilenameFormat = getStr("dueling_subtitle_filename_format", PrefDefaults.DuelingSubtitleFilenameFormat);
      ConstantSettings.DuelingQuickRefFilenameFormat = getStr("dueling_quick_ref_filename_format", PrefDefaults.DuelingQuickRefFilenameFormat);
      ConstantSettings.DuelingQuickRefSubs1Format = getStr("dueling_quick_ref_subs1_format", PrefDefaults.DuelingQuickRefSubs1Format);
      ConstantSettings.DuelingQuickRefSubs2Format = getStr("dueling_quick_ref_subs2_format", PrefDefaults.DuelingQuickRefSubs2Format);

      ConstantSettings.SrsVobsubFilenamePrefix = getStr("srs_vobsub_filename_prefix", PrefDefaults.SrsVobsubFilenamePrefix);
      ConstantSettings.VobsubFilenameFormat = getStr("vobsub_filename_format", PrefDefaults.VobsubFilenameFormat); // FIX: was VideoFilenameFormat
      ConstantSettings.SrsVobsubFilenameSuffix = getStr("srs_vobsub_filename_suffix", PrefDefaults.SrsVobsubFilenameSuffix);
    }
  }
}
