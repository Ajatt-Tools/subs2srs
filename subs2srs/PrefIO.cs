//  Copyright (C) 2009-2016 Christopher Brochtrup
//  Copyright (C) 2026 fkzys (GTK4/.NET 10 port)
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
using System.IO;
using System.Text;
using System.Text.Json;

namespace subs2srs
{
    /// <summary>
    /// Read/write user preferences as a JSON file.
    /// Replaces the old key=value text format with regex-based updates.
    /// </summary>
    public static class PrefIO
    {
        private static readonly JsonSerializerOptions Opts = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        /// <summary>
        /// Path to the JSON preferences file.
        /// Lives next to where the old preferences.txt was.
        /// </summary>
        private static string JsonPath =>
            Path.Combine(
                Path.GetDirectoryName(ConstantSettings.SettingsFilename) ?? "",
                "preferences.json");

        /// <summary>
        /// Load preferences from disk into ConstantSettings.Prefs.
        /// If file is missing or corrupt, defaults are used and a new file is written.
        /// </summary>
        public static void read()
        {
            // Auto-migrate: if old .txt exists but no .json yet, read old format first
            if (!File.Exists(JsonPath) && File.Exists(ConstantSettings.SettingsFilename))
            {
                MigrateFromText();
                return;
            }

            if (!File.Exists(JsonPath))
            {
                ConstantSettings.Prefs = new PreferencesData();
                Write();
                return;
            }

            try
            {
                var json = File.ReadAllText(JsonPath, Encoding.UTF8);
                ConstantSettings.Prefs =
                    JsonSerializer.Deserialize<PreferencesData>(json, Opts)
                    ?? new PreferencesData();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Warning: failed to read preferences: {ex.Message}");
                ConstantSettings.Prefs = new PreferencesData();
            }
        }

        /// <summary>
        /// Persist current ConstantSettings.Prefs to disk as JSON.
        /// </summary>
        public static void Write()
        {
            try
            {
                var dir = Path.GetDirectoryName(JsonPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(JsonPath,
                    JsonSerializer.Serialize(ConstantSettings.Prefs, Opts),
                    Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Warning: failed to write preferences: {ex.Message}");
            }
        }

        /// <summary>
        /// One-time migration from old preferences.txt to preferences.json.
        /// Reads old format, populates Prefs, writes JSON, leaves old file intact.
        /// </summary>
        private static void MigrateFromText()
        {
            var prefs = new PreferencesData();

            try
            {
                var pairs = new System.Collections.Generic.Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

                foreach (var line in File.ReadAllLines(ConstantSettings.SettingsFilename, Encoding.UTF8))
                {
                    if (line.TrimStart().StartsWith('#')) continue;
                    int eqIdx = line.IndexOf('=');
                    if (eqIdx < 0) continue;
                    string key = line[..eqIdx].Trim();
                    if (key.Length > 0)
                        pairs[key] = line[(eqIdx + 1)..].Trim();
                }

                // Local helpers matching old token format
                string getStr(string key, string def)
                {
                    if (!pairs.TryGetValue(key, out string raw)) return def;
                    string val = raw
                        .Replace("${tab}", "\t")
                        .Replace("${cr}", "\r")
                        .Replace("${lf}", "\n");
                    if (val.Equals("none", StringComparison.OrdinalIgnoreCase)) return "";
                    return val.Length == 0 ? def : val;
                }
                bool getBl(string key, bool def) =>
                    getStr(key, def.ToString()).Equals("true", StringComparison.OrdinalIgnoreCase);
                int getI(string key, int def) =>
                    int.TryParse(getStr(key, def.ToString()), out int v) ? v : def;

                prefs.MainWindowWidth = getI("main_window_width", PrefDefaults.MainWindowWidth);
                prefs.MainWindowHeight = getI("main_window_height", PrefDefaults.MainWindowHeight);
                prefs.DefaultEnableAudioClipGeneration = getBl("default_enable_audio_clip_generation", PrefDefaults.DefaultEnableAudioClipGeneration);
                prefs.DefaultEnableSnapshotsGeneration = getBl("default_enable_snapshots_generation", PrefDefaults.DefaultEnableSnapshotsGeneration);
                prefs.DefaultEnableVideoClipsGeneration = getBl("default_enable_video_clips_generation", PrefDefaults.DefaultEnableVideoClipsGeneration);
                prefs.VideoPlayer = getStr("video_player", PrefDefaults.VideoPlayer);
                prefs.VideoPlayerArgs = getStr("video_player_args", PrefDefaults.VideoPlayerArgs);
                prefs.ReencodeBeforeSplittingAudio = getBl("reencode_before_splitting_audio", PrefDefaults.ReencodeBeforeSplittingAudio);
                prefs.EnableLogging = getBl("enable_logging", PrefDefaults.EnableLogging);
                prefs.AudioNormalizeArgs = getStr("audio_normalize_args", PrefDefaults.AudioNormalizeArgs);
                prefs.LongClipWarningSeconds = getI("long_clip_warning_seconds", PrefDefaults.LongClipWarningSeconds);
                prefs.MaxParallelTasks = getI("max_parallel_tasks", PrefDefaults.MaxParallelTasks);
                prefs.DefaultAudioClipBitrate = getI("default_audio_clip_bitrate", PrefDefaults.DefaultAudioClipBitrate);
                prefs.DefaultAudioNormalize = getBl("default_audio_normalize", PrefDefaults.DefaultAudioNormalize);
                prefs.DefaultVideoClipVideoBitrate = getI("default_video_clip_video_bitrate", PrefDefaults.DefaultVideoClipVideoBitrate);
                prefs.DefaultVideoClipAudioBitrate = getI("default_video_clip_audio_bitrate", PrefDefaults.DefaultVideoClipAudioBitrate);
                prefs.DefaultSnapshotJpegQuality = getI("default_snapshot_jpeg_quality", PrefDefaults.DefaultSnapshotJpegQuality);
                prefs.DefaultIphoneSupport = getBl("default_ipod_support", PrefDefaults.DefaultIphoneSupport);
                prefs.DefaultEncodingSubs1 = getStr("default_encoding_subs1", PrefDefaults.DefaultEncodingSubs1);
                prefs.DefaultEncodingSubs2 = getStr("default_encoding_subs2", PrefDefaults.DefaultEncodingSubs2);
                prefs.DefaultContextNumLeading = getI("default_context_num_leading", PrefDefaults.DefaultContextNumLeading);
                prefs.DefaultContextNumTrailing = getI("default_context_num_trailing", PrefDefaults.DefaultContextNumTrailing);
                prefs.DefaultContextLeadingRange = getI("default_context_leading_range", PrefDefaults.DefaultContextLeadingRange);
                prefs.DefaultContextTrailingRange = getI("default_context_trailing_range", PrefDefaults.DefaultContextTrailingRange);
                prefs.DefaultRemoveStyledLinesSubs1 = getBl("default_remove_styled_lines_subs1", PrefDefaults.DefaultRemoveStyledLinesSubs1);
                prefs.DefaultRemoveStyledLinesSubs2 = getBl("default_remove_styled_lines_subs2", PrefDefaults.DefaultRemoveStyledLinesSubs2);
                prefs.DefaultRemoveNoCounterpartSubs1 = getBl("default_remove_no_counterpart_subs1", PrefDefaults.DefaultRemoveNoCounterpartSubs1);
                prefs.DefaultRemoveNoCounterpartSubs2 = getBl("default_remove_no_counterpart_subs2", PrefDefaults.DefaultRemoveNoCounterpartSubs2);
                prefs.DefaultIncludeTextSubs1 = getStr("default_included_text_subs1", PrefDefaults.DefaultIncludeTextSubs1);
                prefs.DefaultIncludeTextSubs2 = getStr("default_included_text_subs2", PrefDefaults.DefaultIncludeTextSubs2);
                prefs.DefaultExcludeTextSubs1 = getStr("default_excluded_text_subs1", PrefDefaults.DefaultExcludeTextSubs1);
                prefs.DefaultExcludeTextSubs2 = getStr("default_excluded_text_subs2", PrefDefaults.DefaultExcludeTextSubs2);
                prefs.DefaultExcludeDuplicateLinesSubs1 = getBl("default_exclude_duplicate_lines_subs1", PrefDefaults.DefaultExcludeDuplicateLinesSubs1);
                prefs.DefaultExcludeDuplicateLinesSubs2 = getBl("default_exclude_duplicate_lines_subs2", PrefDefaults.DefaultExcludeDuplicateLinesSubs2);
                prefs.DefaultExcludeLinesFewerThanCharsSubs1 = getBl("default_exclude_lines_with_fewer_than_n_chars_subs1", PrefDefaults.DefaultExcludeLinesFewerThanCharsSubs1);
                prefs.DefaultExcludeLinesFewerThanCharsSubs2 = getBl("default_exclude_lines_with_fewer_than_n_chars_subs2", PrefDefaults.DefaultExcludeLinesFewerThanCharsSubs2);
                prefs.DefaultExcludeLinesFewerThanCharsNumSubs1 = getI("default_exclude_lines_with_fewer_than_n_chars_num_subs1", PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs1);
                prefs.DefaultExcludeLinesFewerThanCharsNumSubs2 = getI("default_exclude_lines_with_fewer_than_n_chars_num_subs2", PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs2);
                prefs.DefaultExcludeLinesShorterThanMsSubs1 = getBl("default_exclude_lines_shorter_than_n_ms_subs1", PrefDefaults.DefaultExcludeLinesShorterThanMsSubs1);
                prefs.DefaultExcludeLinesShorterThanMsSubs2 = getBl("default_exclude_lines_shorter_than_n_ms_subs2", PrefDefaults.DefaultExcludeLinesShorterThanMsSubs2);
                prefs.DefaultExcludeLinesShorterThanMsNumSubs1 = getI("default_exclude_lines_shorter_than_n_ms_num_subs1", PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs1);
                prefs.DefaultExcludeLinesShorterThanMsNumSubs2 = getI("default_exclude_lines_shorter_than_n_ms_num_subs2", PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs2);
                prefs.DefaultExcludeLinesLongerThanMsSubs1 = getBl("default_exclude_lines_longer_than_n_ms_subs1", PrefDefaults.DefaultExcludeLinesLongerThanMsSubs1);
                prefs.DefaultExcludeLinesLongerThanMsSubs2 = getBl("default_exclude_lines_longer_than_n_ms_subs2", PrefDefaults.DefaultExcludeLinesLongerThanMsSubs2);
                prefs.DefaultExcludeLinesLongerThanMsNumSubs1 = getI("default_exclude_lines_longer_than_n_ms_num_subs1", PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs1);
                prefs.DefaultExcludeLinesLongerThanMsNumSubs2 = getI("default_exclude_lines_longer_than_n_ms_num_subs2", PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs2);
                prefs.DefaultJoinSentencesSubs1 = getBl("default_join_sentences_subs1", PrefDefaults.DefaultJoinSentencesSubs1);
                prefs.DefaultJoinSentencesSubs2 = getBl("default_join_sentences_subs2", PrefDefaults.DefaultJoinSentencesSubs2);
                prefs.DefaultJoinSentencesCharListSubs1 = getStr("default_join_sentences_char_list_subs1", PrefDefaults.DefaultJoinSentencesCharListSubs1);
                prefs.DefaultJoinSentencesCharListSubs2 = getStr("default_join_sentences_char_list_subs2", PrefDefaults.DefaultJoinSentencesCharListSubs2);
                prefs.DefaultFileBrowserStartDir = getStr("default_file_browser_start_dir", PrefDefaults.DefaultFileBrowserStartDir);
                prefs.SrsFilenameFormat = getStr("srs_filename_format", PrefDefaults.SrsFilenameFormat);
                prefs.SrsDelimiter = getStr("srs_delimiter", PrefDefaults.SrsDelimiter);
                prefs.SrsTagFormat = getStr("srs_tag_format", PrefDefaults.SrsTagFormat);
                prefs.SrsSequenceMarkerFormat = getStr("srs_sequence_marker_format", PrefDefaults.SrsSequenceMarkerFormat);
                prefs.SrsAudioFilenamePrefix = getStr("srs_audio_filename_prefix", PrefDefaults.SrsAudioFilenamePrefix);
                prefs.AudioFilenameFormat = getStr("audio_filename_format", PrefDefaults.AudioFilenameFormat);
                prefs.AudioId3Artist = getStr("audio_id3_artist", PrefDefaults.AudioId3Artist);
                prefs.AudioId3Album = getStr("audio_id3_album", PrefDefaults.AudioId3Album);
                prefs.AudioId3Title = getStr("audio_id3_title", PrefDefaults.AudioId3Title);
                prefs.AudioId3Genre = getStr("audio_id3_genre", PrefDefaults.AudioId3Genre);
                prefs.AudioId3Lyrics = getStr("audio_id3_lyrics", PrefDefaults.AudioId3Lyrics);
                prefs.SrsAudioFilenameSuffix = getStr("srs_audio_filename_suffix", PrefDefaults.SrsAudioFilenameSuffix);
                prefs.SrsSnapshotFilenamePrefix = getStr("srs_snapshot_filename_prefix", PrefDefaults.SrsSnapshotFilenamePrefix);
                prefs.SnapshotFilenameFormat = getStr("snapshot_filename_format", PrefDefaults.SnapshotFilenameFormat);
                prefs.SrsSnapshotFilenameSuffix = getStr("srs_snapshot_filename_suffix", PrefDefaults.SrsSnapshotFilenameSuffix);
                prefs.SrsVideoFilenamePrefix = getStr("srs_video_filename_prefix", PrefDefaults.SrsVideoFilenamePrefix);
                prefs.VideoFilenameFormat = getStr("video_filename_format", PrefDefaults.VideoFilenameFormat);
                prefs.SrsVideoFilenameSuffix = getStr("srs_video_filename_suffix", PrefDefaults.SrsVideoFilenameSuffix);
                prefs.SrsSubs1Format = getStr("srs_subs1_format", PrefDefaults.SrsSubs1Format);
                prefs.SrsSubs2Format = getStr("srs_subs2_format", PrefDefaults.SrsSubs2Format);
                prefs.ExtractMediaAudioFilenameFormat = getStr("extract_media_audio_filename_format", PrefDefaults.ExtractMediaAudioFilenameFormat);
                prefs.ExtractMediaLyricsSubs1Format = getStr("extract_media_lyrics_subs1_format", PrefDefaults.ExtractMediaLyricsSubs1Format);
                prefs.ExtractMediaLyricsSubs2Format = getStr("extract_media_lyrics_subs2_format", PrefDefaults.ExtractMediaLyricsSubs2Format);
                prefs.DuelingSubtitleFilenameFormat = getStr("dueling_subtitle_filename_format", PrefDefaults.DuelingSubtitleFilenameFormat);
                prefs.DuelingQuickRefFilenameFormat = getStr("dueling_quick_ref_filename_format", PrefDefaults.DuelingQuickRefFilenameFormat);
                prefs.DuelingQuickRefSubs1Format = getStr("dueling_quick_ref_subs1_format", PrefDefaults.DuelingQuickRefSubs1Format);
                prefs.DuelingQuickRefSubs2Format = getStr("dueling_quick_ref_subs2_format", PrefDefaults.DuelingQuickRefSubs2Format);
                prefs.SrsVobsubFilenamePrefix = getStr("srs_vobsub_filename_prefix", PrefDefaults.SrsVobsubFilenamePrefix);
                prefs.VobsubFilenameFormat = getStr("vobsub_filename_format", PrefDefaults.VobsubFilenameFormat);
                prefs.SrsVobsubFilenameSuffix = getStr("srs_vobsub_filename_suffix", PrefDefaults.SrsVobsubFilenameSuffix);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Warning: migration from preferences.txt failed: {ex.Message}");
            }

            ConstantSettings.Prefs = prefs;
            Write(); // persist as JSON
        }
    }
}
