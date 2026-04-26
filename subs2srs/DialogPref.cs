//  Copyright (C) 2026 fkzys
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

namespace subs2srs
{
    /// <summary>
    /// Preferences dialog — GTK4/Gir.Core port.
    ///
    /// GTK4 removed TreeView/TreeStore. This port uses Gtk.ListView
    /// backed by a Gio.ListStore of PrefItem GObject wrappers.
    /// Categories are flat bold rows; properties are editable rows beneath them.
    ///
    /// Replaces GTK3 Dialog with Gtk.Window + nested GLib.MainLoop for Run().
    /// </summary>
    public class DialogPref : Gtk.Window
    {
        private PropertyTable propTable;
        private Gio.ListStore _store;
        private List<PrefItem> _items = new();
        private Gtk.ListView _listView;
        private Gtk.SingleSelection _selection;
        private Gtk.Label _descLabel;

        // Search / filter
        private Gtk.SearchEntry _searchEntry;
        private Gtk.FilterListModel _filterModel;
        private Gtk.CustomFilter _customFilter;
        private string _filterText = "";

        // Nested main loop for synchronous Run()
        private GLib.MainLoop _loop;
        private int _responseId;

        public DialogPref(Gtk.Window parent) : base()
        {
            SetTitle("Preferences");
            SetDefaultSize(750, 550);
            SetModal(true);
            if (parent != null)
                SetTransientFor(parent);

            PrefIO.read();
            BuildPropTable();
            BuildUI();
            PopulateStore();
        }

        /// <summary>
        /// Show the dialog modally. Returns 1 for OK, 0 for Cancel/close.
        /// </summary>
        public int Run()
        {
            _loop = GLib.MainLoop.New(null, false);
            _responseId = 0;

            OnCloseRequest += OnDialogCloseRequest;
            Show();
            _loop.Run();

            return _responseId;
        }

        private bool OnDialogCloseRequest(Gtk.Window sender, EventArgs args)
        {
            if (_loop != null && _loop.IsRunning())
                _loop.Quit();
            return false;
        }

        // ── PROPERTY TABLE (same data as original) ──────────────────────────

        private void BuildPropTable()
        {
            propTable = new PropertyTable();

            // main_window_width
            propTable.Properties.Add(new PropertySpec("Main Window Width", typeof(int),
                "User Interface Defaults",
                "The width in pixels of the main interface.\n\nRange: 0-9999.",
                PrefDefaults.MainWindowWidth));
            propTable["Main Window Width"] = ConstantSettings.MainWindowWidth;

            // main_window_height
            propTable.Properties.Add(new PropertySpec("Main Window Height", typeof(int),
                "User Interface Defaults",
                "The height in pixels of the main interface.\n\nRange: 0-9999.",
                PrefDefaults.MainWindowHeight));
            propTable["Main Window Height"] = ConstantSettings.MainWindowHeight;

            // default_enable_audio_clip_generation
            propTable.Properties.Add(new PropertySpec("Enable Audio Clip Generation", typeof(bool),
                "User Interface Defaults",
                "Enable the Generate Audio Clips option when subs2srs starts up.",
                PrefDefaults.DefaultEnableAudioClipGeneration));
            propTable["Enable Audio Clip Generation"] = ConstantSettings.DefaultEnableAudioClipGeneration;

            // default_enable_snapshots_generation
            propTable.Properties.Add(new PropertySpec("Enable Snapshots Generation", typeof(bool),
                "User Interface Defaults",
                "Enable the Generate Snapshots option when subs2srs starts up.",
                PrefDefaults.DefaultEnableSnapshotsGeneration));
            propTable["Enable Snapshots Generation"] = ConstantSettings.DefaultEnableSnapshotsGeneration;

            // default_enable_video_clips_generation
            propTable.Properties.Add(new PropertySpec("Enable Video Clips Generation", typeof(bool),
                "User Interface Defaults",
                "Enable the Generate Video Clips option when subs2srs starts up.",
                PrefDefaults.DefaultEnableVideoClipsGeneration));
            propTable["Enable Video Clips Generation"] = ConstantSettings.DefaultEnableVideoClipsGeneration;

            // default_audio_clip_bitrate
            propTable.Properties.Add(new PropertySpec("Audio Clip Bitrate", typeof(int),
                "User Interface Defaults",
                "The default audio clip bitrate to use when subs2srs starts up.\n\n"
                + "You may use these values: 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 192, 224, 256, 320.",
                PrefDefaults.DefaultAudioClipBitrate));
            propTable["Audio Clip Bitrate"] = ConstantSettings.DefaultAudioClipBitrate;

            // default_audio_format
            propTable.Properties.Add(new PropertySpec("Audio Clip Format", typeof(string),
                "User Interface Defaults",
                "The default audio clip format.\n\nSupported: Opus, MP3.",
                PrefDefaults.DefaultAudioFormat));
            propTable["Audio Clip Format"] = ConstantSettings.AudioFormat;

            // default_audio_normalize
            propTable.Properties.Add(new PropertySpec("Normalize Audio", typeof(bool),
                "User Interface Defaults",
                "Enable the 'Normalize Audio' option when subs2srs starts up.",
                PrefDefaults.DefaultAudioNormalize));
            propTable["Normalize Audio"] = ConstantSettings.DefaultAudioNormalize;

            // default_video_clip_video_bitrate
            propTable.Properties.Add(new PropertySpec("Video Clip Video Bitrate", typeof(int),
                "User Interface Defaults",
                "The default video clip video bitrate to use when subs2srs starts up.\n\nRange: 100-3000.",
                PrefDefaults.DefaultVideoClipVideoBitrate));
            propTable["Video Clip Video Bitrate"] = ConstantSettings.DefaultVideoClipVideoBitrate;

            // default_video_clip_audio_bitrate
            propTable.Properties.Add(new PropertySpec("Video Clip Audio Bitrate", typeof(int),
                "User Interface Defaults",
                "The default video clip audio bitrate to use when subs2srs starts up.\n\n"
                + "You may use these values: 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 192, 224, 256, 320.",
                PrefDefaults.DefaultVideoClipAudioBitrate));
            propTable["Video Clip Audio Bitrate"] = ConstantSettings.DefaultVideoClipAudioBitrate;

            // default_ipod_support
            propTable.Properties.Add(new PropertySpec("iPhone Support", typeof(bool),
                "User Interface Defaults",
                "Enable the iPhone Support option when subs2srs starts up.",
                PrefDefaults.DefaultIphoneSupport));
            propTable["iPhone Support"] = ConstantSettings.DefaultIphoneSupport;

            // default_snapshot_jpeg_quality
            propTable.Properties.Add(new PropertySpec("Snapshot JPEG Quality", typeof(int),
                "User Interface Defaults",
                "The default JPEG quality for snapshots (ffmpeg -q:v).\n\n"
                + "1 = best quality (largest file), 31 = worst quality (smallest file).\n"
                + "Recommended: 2-5 for Anki cards.\n\nRange: 1-31.",
                PrefDefaults.DefaultSnapshotJpegQuality));
            propTable["Snapshot JPEG Quality"] = ConstantSettings.DefaultSnapshotJpegQuality;

            string encodingList =
                "You may use these values:\n"
                + "ASMO-708, big5, cp1025, cp866, cp875, csISO2022JP, DOS-720, DOS-862, "
                + "EUC-CN, euc-jp, EUC-JP, euc-kr, GB18030, gb2312, hz-gb-2312, IBM00858, "
                + "IBM00924, IBM01047, IBM01140, IBM01141, IBM01142, IBM01143, IBM01144, "
                + "IBM01145, IBM01146, IBM01147, IBM01148, IBM01149, IBM037, IBM1026, "
                + "IBM273, IBM277, IBM278, IBM280, IBM284, IBM285, IBM290, IBM297, IBM420, "
                + "IBM423, IBM424, IBM437, IBM500, ibm737, ibm775, ibm850, ibm852, IBM855, "
                + "ibm857, IBM860, ibm861, IBM863, IBM864, IBM865, ibm869, IBM870, IBM871, "
                + "IBM880, IBM905, IBM-Thai, iso-2022-jp, iso-2022-kr, iso-8859-1, "
                + "iso-8859-13, iso-8859-15, iso-8859-2, iso-8859-3, iso-8859-4, iso-8859-5, "
                + "iso-8859-6, iso-8859-7, iso-8859-8, iso-8859-8-i, iso-8859-9, Johab, koi8-r, "
                + "koi8-u, ks_c_5601-1987, macintosh, shift_jis, unicodeFFFE, us-ascii, utf-16, "
                + "utf-32, utf-32BE, utf-7, utf-8, windows-1250, windows-1251, Windows-1252, "
                + "windows-1253, windows-1254, windows-1255, windows-1256, windows-1257, "
                + "windows-1258, windows-874, x-Chinese-CNS, x-Chinese-Eten, x-cp20001, "
                + "x-cp20003, x-cp20004, x-cp20005, x-cp20261, x-cp20269, x-cp20936, "
                + "x-cp20949, x-cp50227, x-EBCDIC-KoreanExtended, x-Europa, x-IA5, "
                + "x-IA5-German, x-IA5-Norwegian, x-IA5-Swedish, x-iscii-as, "
                + "x-iscii-be, x-iscii-de, x-iscii-gu, x-iscii-ka, x-iscii-ma, x-iscii-or, "
                + "x-iscii-pa, x-iscii-ta, x-iscii-te, x-mac-arabic, x-mac-ce, "
                + "x-mac-chinesesimp, x-mac-chinesetrad, x-mac-croatian, x-mac-cyrillic, "
                + "x-mac-greek, x-mac-hebrew, x-mac-icelandic, x-mac-japanese, x-mac-korean, "
                + "x-mac-romanian, x-mac-thai, x-mac-turkish, x-mac-ukrainian.";

            // default_encoding_subs1
            propTable.Properties.Add(new PropertySpec("Encoding Subs1", typeof(string),
                "User Interface Defaults",
                "The default text encoding to use for subs1.\n\n" + encodingList,
                PrefDefaults.DefaultEncodingSubs1));
            propTable["Encoding Subs1"] = ConstantSettings.DefaultEncodingSubs1;

            // default_encoding_subs2
            propTable.Properties.Add(new PropertySpec("Encoding Subs2", typeof(string),
                "User Interface Defaults",
                "The default text encoding to use for subs2.\n\n" + encodingList,
                PrefDefaults.DefaultEncodingSubs2));
            propTable["Encoding Subs2"] = ConstantSettings.DefaultEncodingSubs2;

            // default_context_num_leading
            propTable.Properties.Add(new PropertySpec("Context Number Leading", typeof(int),
                "User Interface Defaults",
                "The default number of leading context lines to use when subs2srs starts up.\n\nRange: 0-9.",
                PrefDefaults.DefaultContextNumLeading));
            propTable["Context Number Leading"] = ConstantSettings.DefaultContextNumLeading;

            // default_context_num_trailing
            propTable.Properties.Add(new PropertySpec("Context Number Trailing", typeof(int),
                "User Interface Defaults",
                "The default number of trailing context lines to use when subs2srs starts up.\n\nRange: 0-9.",
                PrefDefaults.DefaultContextNumTrailing));
            propTable["Context Number Trailing"] = ConstantSettings.DefaultContextNumTrailing;

            // default_context_leading_range
            propTable.Properties.Add(new PropertySpec("Context Nearby Line Range Leading", typeof(int),
                "User Interface Defaults",
                "The default leading nearby line range to use when subs2srs starts up.\n\nRange: 0-99999. To disable this feature, set to 0.",
                PrefDefaults.DefaultContextLeadingRange));
            propTable["Context Nearby Line Range Leading"] = ConstantSettings.DefaultContextLeadingRange;

            // default_context_trailing_range
            propTable.Properties.Add(new PropertySpec("Context Nearby Line Range Trailing", typeof(int),
                "User Interface Defaults",
                "The default trailing nearby line range to use when subs2srs starts up.\n\nRange: 0-99999. To disable this feature, set to 0.",
                PrefDefaults.DefaultContextTrailingRange));
            propTable["Context Nearby Line Range Trailing"] = ConstantSettings.DefaultContextTrailingRange;

            // default_file_browser_start_dir
            propTable.Properties.Add(new PropertySpec("File Browser Start Folder", typeof(string),
                "User Interface Defaults",
                "The directory that the file/directory browser will start in by default.",
                PrefDefaults.DefaultFileBrowserStartDir));
            propTable["File Browser Start Folder"] = ConstantSettings.DefaultFileBrowserStartDir;

            // default_output_dir
            propTable.Properties.Add(new PropertySpec("Default Output Directory", typeof(string),
                "User Interface Defaults",
                "The default output directory for generated files.\n\n"
                + "When set, this directory will be used instead of ~/Documents on startup.\n"
                + "Also auto-updated when you run Go with a different output directory.",
                PrefDefaults.DefaultOutputDir));
            propTable["Default Output Directory"] = ConstantSettings.DefaultOutputDir;

            // default_remove_styled_lines_subs1
            propTable.Properties.Add(new PropertySpec("Remove Subs1 Styled Lines", typeof(bool),
                "User Interface Defaults",
                "Remove styled lines when parsing .ass subtitles for Subs1. A styled line "
                + "is one that starts with a '{' character.",
                PrefDefaults.DefaultRemoveStyledLinesSubs1));
            propTable["Remove Subs1 Styled Lines"] = ConstantSettings.DefaultRemoveStyledLinesSubs1;

            // default_remove_styled_lines_subs2
            propTable.Properties.Add(new PropertySpec("Remove Subs2 Styled Lines", typeof(bool),
                "User Interface Defaults",
                "Remove styled lines when parsing .ass subtitles for Subs2. A styled line "
                + "is one that starts with a '{' character.",
                PrefDefaults.DefaultRemoveStyledLinesSubs2));
            propTable["Remove Subs2 Styled Lines"] = ConstantSettings.DefaultRemoveStyledLinesSubs2;

            // default_remove_no_counterpart_subs1
            propTable.Properties.Add(new PropertySpec("Remove Subs1 Lines With No Obvious Counterpart", typeof(bool),
                "User Interface Defaults",
                "Remove a line from Subs1 if there exists no obvious Subs1 counterpart.",
                PrefDefaults.DefaultRemoveNoCounterpartSubs1));
            propTable["Remove Subs1 Lines With No Obvious Counterpart"] = ConstantSettings.DefaultRemoveNoCounterpartSubs1;

            // default_remove_no_counterpart_subs2
            propTable.Properties.Add(new PropertySpec("Remove Subs2 Lines With No Obvious Counterpart", typeof(bool),
                "User Interface Defaults",
                "Remove a line from Subs2 if there exists no obvious Subs1 counterpart.",
                PrefDefaults.DefaultRemoveNoCounterpartSubs2));
            propTable["Remove Subs2 Lines With No Obvious Counterpart"] = ConstantSettings.DefaultRemoveNoCounterpartSubs2;

            // default_included_text_subs1
            propTable.Properties.Add(new PropertySpec("Included Text Subs1", typeof(string),
                "User Interface Defaults",
                "The list of semicolon-separated word/phrases to use for the Subs1 'Included Text' option.",
                PrefDefaults.DefaultIncludeTextSubs1));
            propTable["Included Text Subs1"] = ConstantSettings.DefaultIncludeTextSubs1;

            // default_included_text_subs2
            propTable.Properties.Add(new PropertySpec("Included Text Subs2", typeof(string),
                "User Interface Defaults",
                "The list of semicolon-separated word/phrases to use for the Subs2 'Included Text' option.",
                PrefDefaults.DefaultIncludeTextSubs2));
            propTable["Included Text Subs2"] = ConstantSettings.DefaultIncludeTextSubs2;

            // default_excluded_text_subs1
            propTable.Properties.Add(new PropertySpec("Excluded Text Subs1", typeof(string),
                "User Interface Defaults",
                "The list of semicolon-separated word/phrases to use for the Subs1 'Excluded Text' option.",
                PrefDefaults.DefaultExcludeTextSubs1));
            propTable["Excluded Text Subs1"] = ConstantSettings.DefaultExcludeTextSubs1;

            // default_excluded_text_subs2
            propTable.Properties.Add(new PropertySpec("Excluded Text Subs2", typeof(string),
                "User Interface Defaults",
                "The list of semicolon-separated word/phrases to use for the Subs2 'Excluded Text' option.",
                PrefDefaults.DefaultExcludeTextSubs2));
            propTable["Excluded Text Subs2"] = ConstantSettings.DefaultExcludeTextSubs2;

            // default_exclude_duplicate_lines_subs1
            propTable.Properties.Add(new PropertySpec("Exclude Duplicate Lines Subs1", typeof(bool),
                "User Interface Defaults",
                "Enable/Disable the 'Exclude Duplicate Lines' Subs1 option.",
                PrefDefaults.DefaultExcludeDuplicateLinesSubs1));
            propTable["Exclude Duplicate Lines Subs1"] = ConstantSettings.DefaultExcludeDuplicateLinesSubs1;

            // default_exclude_duplicate_lines_subs2
            propTable.Properties.Add(new PropertySpec("Exclude Duplicate Lines Subs2", typeof(bool),
                "User Interface Defaults",
                "Enable/Disable the 'Exclude Duplicate Lines' Subs2 option.",
                PrefDefaults.DefaultExcludeDuplicateLinesSubs2));
            propTable["Exclude Duplicate Lines Subs2"] = ConstantSettings.DefaultExcludeDuplicateLinesSubs2;

            // default_exclude_lines_with_fewer_than_n_chars_subs1
            propTable.Properties.Add(new PropertySpec("Exclude Lines With Fewer Than n Characters Enable Subs1", typeof(bool),
                "User Interface Defaults",
                "Enable/Disable the 'Exclude Lines With Fewer Than n Characters' Subs1 option.",
                PrefDefaults.DefaultExcludeLinesFewerThanCharsSubs1));
            propTable["Exclude Lines With Fewer Than n Characters Enable Subs1"] = ConstantSettings.DefaultExcludeLinesFewerThanCharsSubs1;

            // default_exclude_lines_with_fewer_than_n_chars_subs2
            propTable.Properties.Add(new PropertySpec("Exclude Lines With Fewer Than n Characters Enable Subs2", typeof(bool),
                "User Interface Defaults",
                "Enable/Disable the 'Exclude Lines With Fewer Than n Characters' Subs2 option.",
                PrefDefaults.DefaultExcludeLinesFewerThanCharsSubs2));
            propTable["Exclude Lines With Fewer Than n Characters Enable Subs2"] = ConstantSettings.DefaultExcludeLinesFewerThanCharsSubs2;

            // default_exclude_lines_with_fewer_than_n_chars_num_subs1
            propTable.Properties.Add(new PropertySpec("Exclude Lines With Fewer Than n Characters Number Subs1", typeof(int),
                "User Interface Defaults",
                "Specify the 'n' in the 'Exclude Lines With Fewer Than n Characters' Subs1 option.\n\nRange: 2-99999.",
                PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs1));
            propTable["Exclude Lines With Fewer Than n Characters Number Subs1"] = ConstantSettings.DefaultExcludeLinesFewerThanCharsNumSubs1;

            // default_exclude_lines_with_fewer_than_n_chars_num_subs2
            propTable.Properties.Add(new PropertySpec("Exclude Lines With Fewer Than n Characters Number Subs2", typeof(int),
                "User Interface Defaults",
                "Specify the 'n' in the 'Exclude Lines With Fewer Than n Characters' Subs2 option.\n\nRange: 2-99999.",
                PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs2));
            propTable["Exclude Lines With Fewer Than n Characters Number Subs2"] = ConstantSettings.DefaultExcludeLinesFewerThanCharsNumSubs2;

            // default_exclude_lines_shorter_than_n_ms_subs1
            propTable.Properties.Add(new PropertySpec("Exclude Lines Shorter Than n Milliseconds Enable Subs1", typeof(bool),
                "User Interface Defaults",
                "Enable/Disable the 'Exclude Lines Shorter Than n Milliseconds' Subs1 option.",
                PrefDefaults.DefaultExcludeLinesShorterThanMsSubs1));
            propTable["Exclude Lines Shorter Than n Milliseconds Enable Subs1"] = ConstantSettings.DefaultExcludeLinesShorterThanMsSubs1;

            // default_exclude_lines_shorter_than_n_ms_subs2
            propTable.Properties.Add(new PropertySpec("Exclude Lines Shorter Than n Milliseconds Enable Subs2", typeof(bool),
                "User Interface Defaults",
                "Enable/Disable the 'Exclude Lines Shorter Than n Milliseconds' Subs2 option.",
                PrefDefaults.DefaultExcludeLinesShorterThanMsSubs2));
            propTable["Exclude Lines Shorter Than n Milliseconds Enable Subs2"] = ConstantSettings.DefaultExcludeLinesShorterThanMsSubs2;

            // default_exclude_lines_shorter_than_n_ms_num_subs1
            propTable.Properties.Add(new PropertySpec("Exclude Lines Shorter Than n Milliseconds Number Subs1", typeof(int),
                "User Interface Defaults",
                "Specify the 'n' in the 'Exclude Lines Shorter Than n Milliseconds' Subs1 option.\n\nRange: 100-99999.",
                PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs1));
            propTable["Exclude Lines Shorter Than n Milliseconds Number Subs1"] = ConstantSettings.DefaultExcludeLinesShorterThanMsNumSubs1;

            // default_exclude_lines_shorter_than_n_ms_num_subs2
            propTable.Properties.Add(new PropertySpec("Exclude Lines Shorter Than n Milliseconds Number Subs2", typeof(int),
                "User Interface Defaults",
                "Specify the 'n' in the 'Exclude Lines Shorter Than n Milliseconds' Subs2 option.\n\nRange: 100-99999.",
                PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs2));
            propTable["Exclude Lines Shorter Than n Milliseconds Number Subs2"] = ConstantSettings.DefaultExcludeLinesShorterThanMsNumSubs2;

            // default_exclude_lines_longer_than_n_ms_subs1
            propTable.Properties.Add(new PropertySpec("Exclude Lines Longer Than n Milliseconds Enable Subs1", typeof(bool),
                "User Interface Defaults",
                "Enable/Disable the 'Exclude Lines Longer Than n Milliseconds' Subs1 option.",
                PrefDefaults.DefaultExcludeLinesLongerThanMsSubs1));
            propTable["Exclude Lines Longer Than n Milliseconds Enable Subs1"] = ConstantSettings.DefaultExcludeLinesLongerThanMsSubs1;

            // default_exclude_lines_longer_than_n_ms_subs2
            propTable.Properties.Add(new PropertySpec("Exclude Lines Longer Than n Milliseconds Enable Subs2", typeof(bool),
                "User Interface Defaults",
                "Enable/Disable the 'Exclude Lines Longer Than n Milliseconds' Subs2 option.",
                PrefDefaults.DefaultExcludeLinesLongerThanMsSubs2));
            propTable["Exclude Lines Longer Than n Milliseconds Enable Subs2"] = ConstantSettings.DefaultExcludeLinesLongerThanMsSubs2;

            // default_exclude_lines_longer_than_n_ms_num_subs1
            propTable.Properties.Add(new PropertySpec("Exclude Lines Longer Than n Milliseconds Number Subs1", typeof(int),
                "User Interface Defaults",
                "Specify the 'n' in the 'Exclude Lines Longer Than n Milliseconds' Subs1 option.\n\nRange: 100-99999.",
                PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs1));
            propTable["Exclude Lines Longer Than n Milliseconds Number Subs1"] = ConstantSettings.DefaultExcludeLinesLongerThanMsNumSubs1;

            // default_exclude_lines_longer_than_n_ms_num_subs2
            propTable.Properties.Add(new PropertySpec("Exclude Lines Longer Than n Milliseconds Number Subs2", typeof(int),
                "User Interface Defaults",
                "Specify the 'n' in the 'Exclude Lines Longer Than n Milliseconds' Subs2 option.\n\nRange: 100-99999.",
                PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs2));
            propTable["Exclude Lines Longer Than n Milliseconds Number Subs2"] = ConstantSettings.DefaultExcludeLinesLongerThanMsNumSubs2;

            // default_join_sentences_subs1
            propTable.Properties.Add(new PropertySpec("Join Lines That End With One of the Following Characters Enable Subs1", typeof(bool),
                "User Interface Defaults",
                "Enable/Disable the 'Join Lines That End With One of the Following Characters' Subs1 option.",
                PrefDefaults.DefaultJoinSentencesSubs1));
            propTable["Join Lines That End With One of the Following Characters Enable Subs1"] = ConstantSettings.DefaultJoinSentencesSubs1;

            // default_join_sentences_subs2
            propTable.Properties.Add(new PropertySpec("Join Lines That End With One of the Following Characters Enable Subs2", typeof(bool),
                "User Interface Defaults",
                "Enable/Disable the 'Join Lines That End With One of the Following Characters' Subs2 option.",
                PrefDefaults.DefaultJoinSentencesSubs2));
            propTable["Join Lines That End With One of the Following Characters Enable Subs2"] = ConstantSettings.DefaultJoinSentencesSubs2;

            // default_join_sentences_char_list_subs1
            propTable.Properties.Add(new PropertySpec("Join Lines That End With One of the Following Characters Subs1", typeof(string),
                "User Interface Defaults",
                "Specify the list of characters in the 'Join Lines That End With One of the Following Characters' Subs1 option.",
                PrefDefaults.DefaultJoinSentencesCharListSubs1));
            propTable["Join Lines That End With One of the Following Characters Subs1"] = ConstantSettings.DefaultJoinSentencesCharListSubs1;

            // default_join_sentences_char_list_subs2
            propTable.Properties.Add(new PropertySpec("Join Lines That End With One of the Following Characters Subs2", typeof(string),
                "User Interface Defaults",
                "Specify the list of characters in the 'Join Lines That End With One of the Following Characters' Subs2 option.",
                PrefDefaults.DefaultJoinSentencesCharListSubs2));
            propTable["Join Lines That End With One of the Following Characters Subs2"] = ConstantSettings.DefaultJoinSentencesCharListSubs2;

            // srs_filename_format
            propTable.Properties.Add(new PropertySpec("SRS Filename Format", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for SRS (ex. Anki) import filename.\n\n"
                + "Supported Tokens: All except ${episode_num}, ${sequence_num}, ${subs1_line}, ${subs2_line}, or any of the time tokens.",
                PrefDefaults.SrsFilenameFormat));
            propTable["SRS Filename Format"] = ConstantSettings.SrsFilenameFormat;

            // srs_delimiter
            propTable.Properties.Add(new PropertySpec("Delimiter", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The delimiter to use for the SRS (ex. Anki) import file.\n\nSupported Tokens: Only ${tab}.",
                PrefDefaults.SrsDelimiter));
            propTable["Delimiter"] = ConstantSettings.SrsDelimiter;

            // srs_tag_format
            propTable.Properties.Add(new PropertySpec("Tag Format", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the tag in the SRS import file. Leave blank if you do not want to include it.\n\nSupported Tokens: All.",
                PrefDefaults.SrsTagFormat));
            propTable["Tag Format"] = ConstantSettings.SrsTagFormat;

            // srs_sequence_marker_format
            propTable.Properties.Add(new PropertySpec("Sequence Marker Format", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the sequence marker in the SRS import file. Leave blank if you do not want to include it.\n\nSupported Tokens: All.",
                PrefDefaults.SrsSequenceMarkerFormat));
            propTable["Sequence Marker Format"] = ConstantSettings.SrsSequenceMarkerFormat;

            // srs_audio_filename_prefix
            propTable.Properties.Add(new PropertySpec("Audio Clip Prefix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the prefix of the audio entry in the SRS import file.\n\nSupported Tokens: All.",
                PrefDefaults.SrsAudioFilenamePrefix));
            propTable["Audio Clip Prefix"] = ConstantSettings.SrsAudioFilenamePrefix;

            // audio_filename_format
            propTable.Properties.Add(new PropertySpec("Audio Clip Filename Format", typeof(string),
                "Audio Clip Formatting (Uses Tokens)",
                "The format to use for audio clip filenames. You must ensure that each filename will be unique.\n\nSupported Tokens: All.",
                PrefDefaults.AudioFilenameFormat));
            propTable["Audio Clip Filename Format"] = ConstantSettings.AudioFilenameFormat;

            // audio_id3_artist
            propTable.Properties.Add(new PropertySpec("ID3 Tag Artist", typeof(string),
                "Audio Clip Formatting (Uses Tokens)",
                "The format to use for the audio file's ID3 Artist tag.\n\nSupported Tokens: All.",
                PrefDefaults.AudioId3Artist));
            propTable["ID3 Tag Artist"] = ConstantSettings.AudioId3Artist;

            // audio_id3_album
            propTable.Properties.Add(new PropertySpec("ID3 Tag Album", typeof(string),
                "Audio Clip Formatting (Uses Tokens)",
                "The format to use for the audio file's ID3 Album tag.\n\nSupported Tokens: All.",
                PrefDefaults.AudioId3Album));
            propTable["ID3 Tag Album"] = ConstantSettings.AudioId3Album;

            // audio_id3_title
            propTable.Properties.Add(new PropertySpec("ID3 Tag Title", typeof(string),
                "Audio Clip Formatting (Uses Tokens)",
                "The format to use for the audio file's ID3 Title tag.\n\nSupported Tokens: All.",
                PrefDefaults.AudioId3Title));
            propTable["ID3 Tag Title"] = ConstantSettings.AudioId3Title;

            // audio_id3_genre
            propTable.Properties.Add(new PropertySpec("ID3 Tag Genre", typeof(string),
                "Audio Clip Formatting (Uses Tokens)",
                "The format to use for the audio file's ID3 Genre tag.\n\nSupported Tokens: All.",
                PrefDefaults.AudioId3Genre));
            propTable["ID3 Tag Genre"] = ConstantSettings.AudioId3Genre;

            // audio_id3_lyrics
            propTable.Properties.Add(new PropertySpec("ID3 Tag Lyrics", typeof(string),
                "Audio Clip Formatting (Uses Tokens)",
                "The format to use for the audio file's ID3 Lyrics tag.\n\nSupported Tokens: All.",
                PrefDefaults.AudioId3Lyrics));
            propTable["ID3 Tag Lyrics"] = ConstantSettings.AudioId3Lyrics;

            // srs_audio_filename_suffix
            propTable.Properties.Add(new PropertySpec("Audio Clip Suffix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the suffix of the audio entry in the SRS import file.\n\nSupported Tokens: All.",
                PrefDefaults.SrsAudioFilenameSuffix));
            propTable["Audio Clip Suffix"] = ConstantSettings.SrsAudioFilenameSuffix;

            // srs_snapshot_filename_prefix
            propTable.Properties.Add(new PropertySpec("Snapshot Prefix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the prefix of the snapshot entry in the SRS import file.\n\nSupported Tokens: All.",
                PrefDefaults.SrsSnapshotFilenamePrefix));
            propTable["Snapshot Prefix"] = ConstantSettings.SrsSnapshotFilenamePrefix;

            // snapshot_filename_format
            propTable.Properties.Add(new PropertySpec("Snapshot Filename Format", typeof(string),
                "Snapshot Formatting (Uses Tokens)",
                "The format to use for snapshot filenames. You must ensure that each filename will be unique.\n\nSupported Tokens: All.",
                PrefDefaults.SnapshotFilenameFormat));
            propTable["Snapshot Filename Format"] = ConstantSettings.SnapshotFilenameFormat;

            // srs_snapshot_filename_suffix
            propTable.Properties.Add(new PropertySpec("Snapshot Suffix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the suffix of the snapshot entry in the SRS import file.\n\nSupported Tokens: All.",
                PrefDefaults.SrsSnapshotFilenameSuffix));
            propTable["Snapshot Suffix"] = ConstantSettings.SrsSnapshotFilenameSuffix;

            // srs_video_filename_prefix
            propTable.Properties.Add(new PropertySpec("Video Clip Prefix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the prefix of the video entry in the SRS import file.\n\nSupported Tokens: All.",
                PrefDefaults.SrsVideoFilenamePrefix));
            propTable["Video Clip Prefix"] = ConstantSettings.SrsVideoFilenamePrefix;

            // video_filename_format
            propTable.Properties.Add(new PropertySpec("Video Clip Filename Format", typeof(string),
                "Video Formatting (Uses Tokens)",
                "The format to use for video clip filenames. You must ensure that each filename will be unique.\n\nSupported Tokens: All.",
                PrefDefaults.VideoFilenameFormat));
            propTable["Video Clip Filename Format"] = ConstantSettings.VideoFilenameFormat;

            // srs_video_filename_suffix
            propTable.Properties.Add(new PropertySpec("Video Clip Suffix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the suffix of the video entry in the SRS import file.\n\nSupported Tokens: All.",
                PrefDefaults.SrsVideoFilenameSuffix));
            propTable["Video Clip Suffix"] = ConstantSettings.SrsVideoFilenameSuffix;

            // srs_vobsub_filename_prefix
            propTable.Properties.Add(new PropertySpec("Vobsub Prefix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the prefix of the vobsub entry in the SRS import file.\n\nSupported Tokens: Only ${deck_name}.",
                PrefDefaults.SrsVobsubFilenamePrefix));
            propTable["Vobsub Prefix"] = ConstantSettings.SrsVobsubFilenamePrefix;

            // srs_vobsub_filename_suffix
            propTable.Properties.Add(new PropertySpec("Vobsub Suffix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the suffix of the vobsub entry in the SRS import file.\n\nSupported Tokens: Only ${deck_name}.",
                PrefDefaults.SrsVobsubFilenameSuffix));
            propTable["Vobsub Suffix"] = ConstantSettings.SrsVobsubFilenameSuffix;

            // srs_subs1_format
            propTable.Properties.Add(new PropertySpec("Subs1 Format", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use when adding Subs1 to the SRS import file.\n\nSupported Tokens: All.",
                PrefDefaults.SrsSubs1Format));
            propTable["Subs1 Format"] = ConstantSettings.SrsSubs1Format;

            // srs_subs2_format
            propTable.Properties.Add(new PropertySpec("Subs2 Format", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use when adding Subs2 to the SRS import file.\n\nSupported Tokens: All.",
                PrefDefaults.SrsSubs2Format));
            propTable["Subs2 Format"] = ConstantSettings.SrsSubs2Format;

            // extract_media_audio_filename_format
            propTable.Properties.Add(new PropertySpec("Extract Audio From Media Filename Format", typeof(string),
                "Extract Audio from Media Formats (Uses Tokens)",
                "The format to use for audio filenames in the Extract Audio from Media dialog.\n\nSupported Tokens: All except ${subs1_line}, ${subs2_line}, ${width} and ${height}.",
                PrefDefaults.ExtractMediaAudioFilenameFormat));
            propTable["Extract Audio From Media Filename Format"] = ConstantSettings.ExtractMediaAudioFilenameFormat;

            // extract_media_lyrics_subs1_format
            propTable.Properties.Add(new PropertySpec("Lyrics Subs1 Format", typeof(string),
                "Extract Audio from Media Formats (Uses Tokens)",
                "The format to use when adding Subs1 to the audio file's ID3 Lyrics tag.\n\nSupported Tokens: All except ${width} and ${height}.",
                PrefDefaults.ExtractMediaLyricsSubs1Format));
            propTable["Lyrics Subs1 Format"] = ConstantSettings.ExtractMediaLyricsSubs1Format;

            // extract_media_lyrics_subs2_format
            propTable.Properties.Add(new PropertySpec("Lyrics Subs2 Format", typeof(string),
                "Extract Audio from Media Formats (Uses Tokens)",
                "The format to use when adding Subs2 to the audio file's ID3 Lyrics tag.\n\nSupported Tokens: All except ${width} and ${height}.",
                PrefDefaults.ExtractMediaLyricsSubs2Format));
            propTable["Lyrics Subs2 Format"] = ConstantSettings.ExtractMediaLyricsSubs2Format;

            // dueling_subtitle_filename_format
            propTable.Properties.Add(new PropertySpec("Dueling Subtitles Filename Format", typeof(string),
                "Dueling Subtitles Formats (Uses Tokens)",
                "The format to use for dueling subtitle filenames.\n\nSupported Tokens: All except ${sequence_num}, ${subs1_line}, ${subs2_line}, ${width}, and ${height}.",
                PrefDefaults.DuelingSubtitleFilenameFormat));
            propTable["Dueling Subtitles Filename Format"] = ConstantSettings.DuelingSubtitleFilenameFormat;

            // dueling_quick_ref_filename_format
            propTable.Properties.Add(new PropertySpec("Quick Reference Filename Format", typeof(string),
                "Dueling Subtitles Formats (Uses Tokens)",
                "The format to use for dueling subtitle quick reference filenames.\n\nSupported Tokens: All except ${sequence_num}, ${subs1_line}, ${subs2_line}, ${width}, and ${height}.",
                PrefDefaults.DuelingQuickRefFilenameFormat));
            propTable["Quick Reference Filename Format"] = ConstantSettings.DuelingQuickRefFilenameFormat;

            // dueling_quick_ref_subs1_format
            propTable.Properties.Add(new PropertySpec("Quick Reference Subs1 Format", typeof(string),
                "Dueling Subtitles Formats (Uses Tokens)",
                "The format to use when adding Subs1 to the quick reference file.\n\nSupported Tokens: All except ${width} and ${height}.",
                PrefDefaults.DuelingQuickRefSubs1Format));
            propTable["Quick Reference Subs1 Format"] = ConstantSettings.DuelingQuickRefSubs1Format;

            // dueling_quick_ref_subs2_format
            propTable.Properties.Add(new PropertySpec("Quick Reference Subs2 Format", typeof(string),
                "Dueling Subtitles Formats (Uses Tokens)",
                "The format to use when adding Subs2 to the quick reference file.\n\nSupported Tokens: All except ${width} and ${height}.",
                PrefDefaults.DuelingQuickRefSubs2Format));
            propTable["Quick Reference Subs2 Format"] = ConstantSettings.DuelingQuickRefSubs2Format;

            // video_player
            propTable.Properties.Add(new PropertySpec("Video Player Path", typeof(string),
                "Video Player (Uses Tokens)",
                "The video player to use in the Preview dialog.\n\nSupported Tokens: None.",
                PrefDefaults.VideoPlayer));
            propTable["Video Player Path"] = ConstantSettings.VideoPlayer;

            // video_player_args
            propTable.Properties.Add(new PropertySpec("Video Player Arguments", typeof(string),
                "Video Player (Uses Tokens)",
                "The video player arguments to pass to the video player in the Preview dialog.\n\nSupported Tokens: All except ${subs1_line}, ${subs2_line}, ${total_line_num} and ${sequence_num}.",
                PrefDefaults.VideoPlayerArgs));
            propTable["Video Player Arguments"] = ConstantSettings.VideoPlayerArgs;

            // reencode_before_splitting_audio
            propTable.Properties.Add(new PropertySpec("Re-encode Before Splitting Audio", typeof(bool),
                "Misc",
                "When set, subs2srs will re-encode the mp3 before splitting it. "
                + "Useful for certain malformed .mp3 files.",
                PrefDefaults.ReencodeBeforeSplittingAudio));
            propTable["Re-encode Before Splitting Audio"] = ConstantSettings.ReencodeBeforeSplittingAudio;

            // enable_logging
            propTable.Properties.Add(new PropertySpec("Enable Logging", typeof(bool),
                "Misc",
                "Enable logging. Logs will be placed in the Logs directory. Up to " + ConstantSettings.MaxLogFiles
                + " logs will be stored. Takes effect on restart.",
                PrefDefaults.EnableLogging));
            propTable["Enable Logging"] = ConstantSettings.EnableLogging;

            // audio_normalize_args
            propTable.Properties.Add(new PropertySpec("Normalize Audio Arguments", typeof(string),
                "Misc",
                "The arguments to pass to mp3gain (the tool used to normalize the audio).",
                PrefDefaults.AudioNormalizeArgs));
            propTable["Normalize Audio Arguments"] = ConstantSettings.AudioNormalizeArgs;

            // long_clip_warning_seconds
            propTable.Properties.Add(new PropertySpec("Long Clip Warning", typeof(int),
                "Misc",
                "If a line of dialog's duration exceeds the specified number of seconds, display a warning.\n\nRange: 0-99999. To disable, set to 0.",
                PrefDefaults.LongClipWarningSeconds));
            propTable["Long Clip Warning"] = ConstantSettings.LongClipWarningSeconds;

            // max_parallel_tasks
            propTable.Properties.Add(new PropertySpec("Max Parallel Tasks", typeof(int),
                "Misc",
                "Maximum number of parallel threads for media generation.\n\n"
                + "0 = auto (number of CPU cores).\n"
                + "1 = sequential (no parallelism).\n\n"
                + "Range: 0-128.",
                PrefDefaults.MaxParallelTasks));
            propTable["Max Parallel Tasks"] = ConstantSettings.MaxParallelTasks;
        }

        // ── GTK UI ──────────────────────────────────────────────────────────

        private void BuildUI()
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            vbox.SetMarginTop(8);
            vbox.SetMarginBottom(8);
            vbox.SetMarginStart(8);
            vbox.SetMarginEnd(8);

            // Search bar for filtering preferences
            _searchEntry = Gtk.SearchEntry.New();
            _searchEntry.SetPlaceholderText("Filter preferences…");
            _searchEntry.OnSearchChanged += OnSearchChanged;
            vbox.Append(_searchEntry);

            // ListView backed by Gio.ListStore of StringObject, wrapped in FilterListModel
            _store = Gio.ListStore.New(Gtk.StringObject.GetGType());

            // Custom filter that resolves item index from StringObject content
            _customFilter = Gtk.CustomFilter.New((item) =>
            {
                // item is a GObject.Object; cast to StringObject to read the index tag
                if (item is Gtk.StringObject strObj)
                {
                    string? tag = strObj.GetString();
                    if (tag != null && int.TryParse(tag, out int idx))
                        return IsItemVisible(idx);
                }
                return true;
            });

            _filterModel = Gtk.FilterListModel.New(_store, _customFilter);
            _selection = Gtk.SingleSelection.New(_filterModel);
            _selection.OnNotify += (s, e) =>
            {
                if (e.Pspec.GetName() == "selected") OnSelectionChanged();
            };

            var factory = BuildFactory();
            _listView = Gtk.ListView.New(_selection, factory);
            _listView.SetVexpand(true);

            var sw = Gtk.ScrolledWindow.New();
            sw.SetChild(_listView);
            sw.SetVexpand(true);
            vbox.Append(sw);

            // Description area
            var descLbl = Gtk.Label.New("Description:");
            descLbl.SetHalign(Gtk.Align.Start);
            vbox.Append(descLbl);
            _descLabel = Gtk.Label.New("");
            _descLabel.SetWrap(true);
            _descLabel.SetHalign(Gtk.Align.Start);
            _descLabel.SetSizeRequest(-1, 80);
            var descSw = Gtk.ScrolledWindow.New();
            descSw.SetChild(_descLabel);
            descSw.SetSizeRequest(-1, 80);
            vbox.Append(descSw);

            // Tool buttons
            var btnBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);

            var btnResetAll = Gtk.Button.NewWithLabel("Reset All");
            btnResetAll.OnClicked += OnResetAllClicked;
            btnBox.Append(btnResetAll);

            var btnResetSel = Gtk.Button.NewWithLabel("Reset Selected");
            btnResetSel.OnClicked += OnResetSelectedClicked;
            btnBox.Append(btnResetSel);

            var btnTokens = Gtk.Button.NewWithLabel("Token List...");
            btnTokens.OnClicked += OnTokenListClicked;
            btnBox.Append(btnTokens);

            vbox.Append(btnBox);

            // OK / Cancel buttons
            var actionBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
            actionBox.SetHalign(Gtk.Align.End);

            var btnCancel = Gtk.Button.NewWithLabel("Cancel");
            btnCancel.OnClicked += (s, e) =>
            {
                _responseId = 0;
                Close();
            };
            actionBox.Append(btnCancel);

            var btnOk = Gtk.Button.NewWithLabel("OK");
            btnOk.OnClicked += (s, e) =>
            {
                SavePreferences();
                _responseId = 1;
                Close();
            };
            actionBox.Append(btnOk);

            vbox.Append(actionBox);

            SetChild(vbox);
        }

        /// <summary>
        /// Build a SignalListItemFactory that renders each PrefItem row.
        /// Category rows display bold name only.
        /// Bool property rows display name + CheckButton.
        /// String/int property rows display name + editable Entry.
        /// </summary>
        private Gtk.SignalListItemFactory BuildFactory()
        {
            var factory = Gtk.SignalListItemFactory.New();

            factory.OnSetup += (f, args) =>
            {
                var listItem = (Gtk.ListItem)args.Object;

                var box = Gtk.Box.New(Gtk.Orientation.Horizontal, 8);

                // Name label (left side)
                var lblName = Gtk.Label.New("");
                lblName.SetHalign(Gtk.Align.Start);
                lblName.SetHexpand(true);
                lblName.SetEllipsize(Pango.EllipsizeMode.End);
                lblName.SetWidthChars(40);
                box.Append(lblName);

                // Toggle for bool values
                var chk = Gtk.CheckButton.New();
                chk.SetVisible(false);
                box.Append(chk);

                // Entry for string/int values
                var entry = Gtk.Entry.New();
                entry.SetVisible(false);
                entry.SetHexpand(true);
                entry.SetWidthChars(30);
                box.Append(entry);

                listItem.SetChild(box);
            };

            factory.OnBind += (f, args) =>
            {
                var listItem = (Gtk.ListItem)args.Object;

                // Resolve actual item index from StringObject tag
                int pos = ResolveItemIndex(listItem);
                if (pos < 0 || pos >= _items.Count) return;

                var item = _items[pos];
                var box = (Gtk.Box)listItem.GetChild();
                if (box == null) return;

                var lblName = (Gtk.Label)box.GetFirstChild();
                var chk = (Gtk.CheckButton)lblName.GetNextSibling();
                var entry = (Gtk.Entry)chk.GetNextSibling();

                lblName.SetText(item.Name);

                if (item.IsCategory)
                {
                    lblName.SetMarkup($"<b>{GLib.Functions.MarkupEscapeText(item.Name, -1)}</b>");
                    chk.SetVisible(false);
                    entry.SetVisible(false);
                }
                else if (item.IsBool)
                {
                    chk.SetVisible(true);
                    entry.SetVisible(false);
                    chk.SetActive(item.BoolValue);
                    chk.OnToggled += (s, e) =>
                    {
                        item.BoolValue = chk.GetActive();
                        propTable[item.PropKey] = item.BoolValue;
                    };
                }
                else
                {
                    chk.SetVisible(false);
                    entry.SetVisible(true);                    entry.SetText(item.StrValue);
                    entry.OnChanged += (s, ev) =>
                    {
                        item.StrValue = entry.GetText();
                    };
                }
            };

            factory.OnUnbind += (f, args) =>
            {
                var listItem = (Gtk.ListItem)args.Object;

                int pos = ResolveItemIndex(listItem);
                if (pos < 0 || pos >= _items.Count) return;

                var item = _items[pos];
                if (item.IsCategory || item.IsBool) return;

                var box = (Gtk.Box)listItem.GetChild();
                if (box == null) return;
                var lblName = (Gtk.Label)box.GetFirstChild();
                var chk = (Gtk.CheckButton)lblName.GetNextSibling();
                var entry = (Gtk.Entry)chk.GetNextSibling();
                CommitEntryValue(item, entry);
            };
            return factory;
        }

        /// <summary>
        /// Extract the _items index stored as string inside the ListItem's StringObject.
        /// </summary>
        private int ResolveItemIndex(Gtk.ListItem listItem)
        {
            var obj = listItem.GetItem();
            if (obj is Gtk.StringObject strObj)
            {
                string? tag = strObj.GetString();
                if (tag != null && int.TryParse(tag, out int idx))
                    return idx;
            }
            return -1;
        }

        private void CommitEntryValue(PrefItem item, Gtk.Entry entry)
        {
            string text = entry.GetText();
            item.StrValue = text;

            if (item.IsInt)
            {
                if (int.TryParse(text, out int val))
                    propTable[item.PropKey] = val;
            }
            else
            {
                propTable[item.PropKey] = text;
            }
        }

        // ── STORE POPULATION ────────────────────────────────────────────────

        private void PopulateStore()
        {
            _store.RemoveAll();
            _items.Clear();

            var categories = new List<string>();
            for (int i = 0; i < propTable.Properties.Count; i++)
            {
                string cat = propTable.Properties[i].Category;
                if (!categories.Contains(cat))
                    categories.Add(cat);
            }

            foreach (string cat in categories)
            {
                int idx = _items.Count;
                _items.Add(PrefItem.CreateCategory(cat));
                // Store item index as string tag inside StringObject
                _store.Append(Gtk.StringObject.New(idx.ToString()));

                for (int i = 0; i < propTable.Properties.Count; i++)
                {
                    var prop = propTable.Properties[i];
                    if (prop.Category != cat) continue;

                    object val = propTable[prop.Name];
                    bool isBool = val is bool;
                    bool isInt = val is int;

                    idx = _items.Count;
                    _items.Add(PrefItem.CreateProperty(
                        prop.Name,
                        isBool ? "" : (val?.ToString() ?? ""),
                        isBool && (bool)val,
                        isBool, isInt,
                        prop.Description ?? "",
                        prop.Name));
                    _store.Append(Gtk.StringObject.New(idx.ToString()));
                }
            }
        }

        // ── SELECTION HANDLER ───────────────────────────────────────────────

        private void OnSelectionChanged()
        {
            uint sel = _selection.GetSelected();
            if (sel == Gtk.Constants.INVALID_LIST_POSITION)
            {
                _descLabel.SetText("");
                return;
            }

            // Get the StringObject from the filtered model at the selected position
            var obj = _filterModel.GetObject(sel);
            if (obj is not Gtk.StringObject strObj)
            {
                _descLabel.SetText("");
                return;
            }

            string? tag = strObj.GetString();
            if (tag == null || !int.TryParse(tag, out int itemIdx))
            {
                _descLabel.SetText("");
                return;
            }

            if (itemIdx < 0 || itemIdx >= _items.Count)
            {
                _descLabel.SetText("");
                return;
            }
            _descLabel.SetText(_items[itemIdx].Description ?? "");
        }

        // ── BUTTON HANDLERS ────────────────────────────────────────────────

        private void OnResetAllClicked(Gtk.Button sender, EventArgs e)
        {
            if (!UtilsMsg.showConfirm(
                "Are you sure that you want to reset all preferences to default values?"))
                return;

            for (int i = 0; i < propTable.Properties.Count; i++)
                propTable[propTable.Properties[i].Name] = propTable.Properties[i].DefaultValue;

            PopulateStore();
        }

        private void OnResetSelectedClicked(Gtk.Button sender, EventArgs e)
        {
            uint sel = _selection.GetSelected();
            if (sel == Gtk.Constants.INVALID_LIST_POSITION || sel >= _items.Count) return;

            var item = _items[(int)sel];
            if (item.IsCategory) return;

            for (int i = 0; i < propTable.Properties.Count; i++)
            {
                if (propTable.Properties[i].Name == item.PropKey)
                {
                    object def = propTable.Properties[i].DefaultValue;
                    propTable[item.PropKey] = def;
                    if (def is bool bv) item.BoolValue = bv;
                    else item.StrValue = def?.ToString() ?? "";
                    PopulateStore();
                    break;
                }
            }
        }

        private void OnTokenListClicked(Gtk.Button sender, EventArgs e)
        {
            try
            {
                string target = $"{ConstantSettings.HelpPage}#prefs_tokens";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = target,
                    UseShellExecute = true
                });
            }
            catch
            {
                UtilsMsg.showErrMsg("Help page not found.");
            }
        }

        // ── SEARCH / FILTER ───────────────────────────────────────────────────

        private void OnSearchChanged(Gtk.SearchEntry sender, EventArgs e)
        {
            _filterText = _searchEntry.GetText().Trim().ToLower();
            // Re-evaluate the filter for all items
            _customFilter.Changed(Gtk.FilterChange.Different);
        }

        /// <summary>
        /// Determine whether item at the given position in _items should be visible.
        /// A category row is visible if any of its child properties match.
        /// A property row is visible if its name or description contains the filter text.
        /// When filter is empty, everything is visible.
        /// </summary>
        private bool IsItemVisible(int pos)
        {
            if (string.IsNullOrEmpty(_filterText)) return true;
            if (pos < 0 || pos >= _items.Count) return true;

            var item = _items[pos];

            if (item.IsCategory)
            {
                // Show category if any child property matches
                for (int i = pos + 1; i < _items.Count; i++)
                {
                    if (_items[i].IsCategory) break; // reached next category
                    if (PropertyMatches(_items[i])) return true;
                }
                return false;
            }

            return PropertyMatches(item);
        }

        private bool PropertyMatches(PrefItem item)
        {
            if (item.IsCategory) return false;
            return item.Name.ToLower().Contains(_filterText)
                || (item.Description ?? "").ToLower().Contains(_filterText);
        }

        // ── SAVE PREFERENCES ───────────────────────────────────────────────

        private void SavePreferences()
        {
            // Commit all pending entry values (user may have typed but not pressed Enter)
            foreach (var item in _items)
            {
                if (item.IsCategory || item.IsBool) continue;
                if (item.IsInt)
                {
                    if (int.TryParse(item.StrValue, out int val))
                        propTable[item.PropKey] = val;
                }
                else
                {
                    propTable[item.PropKey] = item.StrValue;
                }
            }

            ConstantSettings.MainWindowWidth =
                UtilsCommon.checkRange((int)propTable["Main Window Width"], 0, 9999, PrefDefaults.MainWindowWidth);
            ConstantSettings.MainWindowHeight =
                UtilsCommon.checkRange((int)propTable["Main Window Height"], 0, 9999, PrefDefaults.MainWindowHeight);

            ConstantSettings.DefaultEnableAudioClipGeneration = (bool)propTable["Enable Audio Clip Generation"];
            ConstantSettings.DefaultEnableSnapshotsGeneration = (bool)propTable["Enable Snapshots Generation"];
            ConstantSettings.DefaultEnableVideoClipsGeneration = (bool)propTable["Enable Video Clips Generation"];

            ConstantSettings.DefaultAudioClipBitrate =
                UtilsCommon.checkRangeInSet((int)propTable["Audio Clip Bitrate"],
                    new List<int>(new[] { 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 192, 224, 256, 320 }),
                    PrefDefaults.DefaultAudioClipBitrate);

            string audioFormat = (string)propTable["Audio Clip Format"];
            if (audioFormat == "Opus" || audioFormat == "MP3")
                ConstantSettings.AudioFormat = audioFormat;

            ConstantSettings.DefaultAudioNormalize = (bool)propTable["Normalize Audio"];

            ConstantSettings.DefaultVideoClipVideoBitrate =
                UtilsCommon.checkRange((int)propTable["Video Clip Video Bitrate"], 100, 3000, PrefDefaults.DefaultVideoClipVideoBitrate);
            ConstantSettings.DefaultVideoClipAudioBitrate =
                UtilsCommon.checkRangeInSet((int)propTable["Video Clip Audio Bitrate"],
                    new List<int>(new[] { 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 192, 224, 256, 320 }),
                    PrefDefaults.DefaultVideoClipAudioBitrate);
            ConstantSettings.DefaultIphoneSupport = (bool)propTable["iPhone Support"];

            ConstantSettings.DefaultSnapshotJpegQuality =
                UtilsCommon.checkRange((int)propTable["Snapshot JPEG Quality"], 1, 31, PrefDefaults.DefaultSnapshotJpegQuality);

            ConstantSettings.DefaultEncodingSubs1 =
                checkValidEncoding((string)propTable["Encoding Subs1"], PrefDefaults.DefaultEncodingSubs1);
            ConstantSettings.DefaultEncodingSubs2 =
                checkValidEncoding((string)propTable["Encoding Subs2"], PrefDefaults.DefaultEncodingSubs2);

            ConstantSettings.DefaultContextNumLeading =
                UtilsCommon.checkRange((int)propTable["Context Number Leading"], 0, 9, PrefDefaults.DefaultContextNumLeading);
            ConstantSettings.DefaultContextNumTrailing =
                UtilsCommon.checkRange((int)propTable["Context Number Trailing"], 0, 9, PrefDefaults.DefaultContextNumTrailing);
            ConstantSettings.DefaultContextLeadingRange =
                UtilsCommon.checkRange((int)propTable["Context Nearby Line Range Leading"], 0, 99999, PrefDefaults.DefaultContextLeadingRange);
            ConstantSettings.DefaultContextTrailingRange =
                UtilsCommon.checkRange((int)propTable["Context Nearby Line Range Trailing"], 0, 99999, PrefDefaults.DefaultContextTrailingRange);

            ConstantSettings.DefaultFileBrowserStartDir = getStr("File Browser Start Folder");
            ConstantSettings.DefaultOutputDir = getStr("Default Output Directory");

            ConstantSettings.DefaultRemoveStyledLinesSubs1 = (bool)propTable["Remove Subs1 Styled Lines"];
            ConstantSettings.DefaultRemoveStyledLinesSubs2 = (bool)propTable["Remove Subs2 Styled Lines"];
            ConstantSettings.DefaultRemoveNoCounterpartSubs1 = (bool)propTable["Remove Subs1 Lines With No Obvious Counterpart"];
            ConstantSettings.DefaultRemoveNoCounterpartSubs2 = (bool)propTable["Remove Subs2 Lines With No Obvious Counterpart"];

            ConstantSettings.DefaultIncludeTextSubs1 = getStr("Included Text Subs1");
            ConstantSettings.DefaultIncludeTextSubs2 = getStr("Included Text Subs2");
            ConstantSettings.DefaultExcludeTextSubs1 = getStr("Excluded Text Subs1");
            ConstantSettings.DefaultExcludeTextSubs2 = getStr("Excluded Text Subs2");

            ConstantSettings.DefaultExcludeDuplicateLinesSubs1 = (bool)propTable["Exclude Duplicate Lines Subs1"];
            ConstantSettings.DefaultExcludeDuplicateLinesSubs2 = (bool)propTable["Exclude Duplicate Lines Subs2"];

            ConstantSettings.DefaultExcludeLinesFewerThanCharsSubs1 = (bool)propTable["Exclude Lines With Fewer Than n Characters Enable Subs1"];
            ConstantSettings.DefaultExcludeLinesFewerThanCharsSubs2 = (bool)propTable["Exclude Lines With Fewer Than n Characters Enable Subs2"];
            ConstantSettings.DefaultExcludeLinesFewerThanCharsNumSubs1 =
                UtilsCommon.checkRange((int)propTable["Exclude Lines With Fewer Than n Characters Number Subs1"], 2, 99999, PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs1);
            ConstantSettings.DefaultExcludeLinesFewerThanCharsNumSubs2 =
                UtilsCommon.checkRange((int)propTable["Exclude Lines With Fewer Than n Characters Number Subs2"], 2, 99999, PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs2);

            ConstantSettings.DefaultExcludeLinesShorterThanMsSubs1 = (bool)propTable["Exclude Lines Shorter Than n Milliseconds Enable Subs1"];
            ConstantSettings.DefaultExcludeLinesShorterThanMsSubs2 = (bool)propTable["Exclude Lines Shorter Than n Milliseconds Enable Subs2"];
            ConstantSettings.DefaultExcludeLinesShorterThanMsNumSubs1 =
                UtilsCommon.checkRange((int)propTable["Exclude Lines Shorter Than n Milliseconds Number Subs1"], 100, 99999, PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs1);
            ConstantSettings.DefaultExcludeLinesShorterThanMsNumSubs2 =
                UtilsCommon.checkRange((int)propTable["Exclude Lines Shorter Than n Milliseconds Number Subs2"], 100, 99999, PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs2);

            ConstantSettings.DefaultExcludeLinesLongerThanMsSubs1 = (bool)propTable["Exclude Lines Longer Than n Milliseconds Enable Subs1"];
            ConstantSettings.DefaultExcludeLinesLongerThanMsSubs2 = (bool)propTable["Exclude Lines Longer Than n Milliseconds Enable Subs2"];
            ConstantSettings.DefaultExcludeLinesLongerThanMsNumSubs1 =
                UtilsCommon.checkRange((int)propTable["Exclude Lines Longer Than n Milliseconds Number Subs1"], 100, 99999, PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs1);
            ConstantSettings.DefaultExcludeLinesLongerThanMsNumSubs2 =
                UtilsCommon.checkRange((int)propTable["Exclude Lines Longer Than n Milliseconds Number Subs2"], 100, 99999, PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs2);

            ConstantSettings.DefaultJoinSentencesSubs1 = (bool)propTable["Join Lines That End With One of the Following Characters Enable Subs1"];
            ConstantSettings.DefaultJoinSentencesSubs2 = (bool)propTable["Join Lines That End With One of the Following Characters Enable Subs2"];
            ConstantSettings.DefaultJoinSentencesCharListSubs1 = getStrRequired("Join Lines That End With One of the Following Characters Subs1");
            ConstantSettings.DefaultJoinSentencesCharListSubs2 = getStrRequired("Join Lines That End With One of the Following Characters Subs2");

            ConstantSettings.SrsFilenameFormat = getStrRequired("SRS Filename Format");
            ConstantSettings.SrsDelimiter = getStrRequired("Delimiter");
            ConstantSettings.SrsTagFormat = getStr("Tag Format");
            ConstantSettings.SrsSequenceMarkerFormat = getStr("Sequence Marker Format");

            ConstantSettings.SrsAudioFilenamePrefix = getStr("Audio Clip Prefix");
            ConstantSettings.AudioFilenameFormat = getStrRequired("Audio Clip Filename Format");
            ConstantSettings.AudioId3Artist = getStr("ID3 Tag Artist");
            ConstantSettings.AudioId3Album = getStr("ID3 Tag Album");
            ConstantSettings.AudioId3Title = getStr("ID3 Tag Title");
            ConstantSettings.AudioId3Genre = getStr("ID3 Tag Genre");
            ConstantSettings.AudioId3Lyrics = getStr("ID3 Tag Lyrics");
            ConstantSettings.SrsAudioFilenameSuffix = getStr("Audio Clip Suffix");

            ConstantSettings.SrsSnapshotFilenamePrefix = getStr("Snapshot Prefix");
            ConstantSettings.SnapshotFilenameFormat = getStrRequired("Snapshot Filename Format");
            ConstantSettings.SrsSnapshotFilenameSuffix = getStr("Snapshot Suffix");

            ConstantSettings.SrsVideoFilenamePrefix = getStr("Video Clip Prefix");
            ConstantSettings.VideoFilenameFormat = getStrRequired("Video Clip Filename Format");
            ConstantSettings.SrsVideoFilenameSuffix = getStr("Video Clip Suffix");

            ConstantSettings.SrsVobsubFilenamePrefix = getStr("Vobsub Prefix");
            ConstantSettings.SrsVobsubFilenameSuffix = getStr("Vobsub Suffix");

            ConstantSettings.SrsSubs1Format = getStrRequired("Subs1 Format");
            ConstantSettings.SrsSubs2Format = getStrRequired("Subs2 Format");

            ConstantSettings.ExtractMediaAudioFilenameFormat = getStrRequired("Extract Audio From Media Filename Format");
            ConstantSettings.ExtractMediaLyricsSubs1Format = getStrRequired("Lyrics Subs1 Format");
            ConstantSettings.ExtractMediaLyricsSubs2Format = getStr("Lyrics Subs2 Format");

            ConstantSettings.DuelingSubtitleFilenameFormat = getStrRequired("Dueling Subtitles Filename Format");
            ConstantSettings.DuelingQuickRefFilenameFormat = getStrRequired("Quick Reference Filename Format");
            ConstantSettings.DuelingQuickRefSubs1Format = getStrRequired("Quick Reference Subs1 Format");
            ConstantSettings.DuelingQuickRefSubs2Format = getStr("Quick Reference Subs2 Format");

            ConstantSettings.VideoPlayer = getStr("Video Player Path");
            ConstantSettings.VideoPlayerArgs = getStr("Video Player Arguments");

            ConstantSettings.ReencodeBeforeSplittingAudio = (bool)propTable["Re-encode Before Splitting Audio"];
            ConstantSettings.EnableLogging = (bool)propTable["Enable Logging"];
            ConstantSettings.AudioNormalizeArgs = getStr("Normalize Audio Arguments");

            ConstantSettings.LongClipWarningSeconds =
                UtilsCommon.checkRange((int)propTable["Long Clip Warning"], 0, 99999, PrefDefaults.LongClipWarningSeconds);
            ConstantSettings.MaxParallelTasks =
                UtilsCommon.checkRange((int)propTable["Max Parallel Tasks"], 0, 128, PrefDefaults.MaxParallelTasks);

            PrefIO.Write();
        }

        // ── HELPERS ─────────────────────────────────────────────────────────

        private string getStr(string key)
        {
            string val = (string)propTable[key];
            return (val ?? "").Trim();
        }

        private string getStrRequired(string key)
        {
            string val = getStr(key);
            if (val.Length == 0)
            {
                for (int i = 0; i < propTable.Properties.Count; i++)
                    if (propTable.Properties[i].Name == key)
                        return propTable.Properties[i].DefaultValue?.ToString() ?? "";
            }
            return val;
        }

        private string checkValidEncoding(string encoding, string def)
        {
            return InfoEncoding.isValidShortEncoding(encoding) ? encoding : def;
        }
    }

    // ── PrefItem GObject wrapper ────────────────────────────────────────────

    /// <summary>
    /// GObject wrapper for a single preferences list row.
    /// Required because Gio.ListStore can only store GObject instances.
    /// </summary>
    // ── PrefItem: plain C# data class ────────────────────────────────────────

    public class PrefItem
    {
        public string Name { get; set; } = "";
        public string StrValue { get; set; } = "";
        public bool BoolValue { get; set; }
        public bool IsBool { get; set; }
        public bool IsInt { get; set; }
        public bool IsCategory { get; set; }
        public string Description { get; set; } = "";
        public string PropKey { get; set; } = "";

        public static PrefItem CreateCategory(string name) =>
            new PrefItem { Name = name, IsCategory = true };

        public static PrefItem CreateProperty(string name, string strValue,
            bool boolValue, bool isBool, bool isInt, string description, string propKey) =>
            new PrefItem
            {
                Name = name, StrValue = strValue, BoolValue = boolValue,
                IsBool = isBool, IsInt = isInt, Description = description, PropKey = propKey
            };
    }
}
