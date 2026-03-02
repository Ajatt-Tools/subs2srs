using System;
using System.Collections.Generic;
using Gtk;

namespace subs2srs
{
    public class DialogPref : Dialog
    {
        private PropertyTable propTable;
        private TreeStore _store;
        private TreeView _treeView;
        private TextView _descView;

        private const int COL_NAME = 0;
        private const int COL_STR_VAL = 1;
        private const int COL_BOOL_VAL = 2;
        private const int COL_IS_BOOL = 3;
        private const int COL_IS_CATEGORY = 4;
        private const int COL_DESCRIPTION = 5;
        private const int COL_PROP_KEY = 6;
        private const int COL_WEIGHT = 7;
        private const int COL_IS_INT = 8;

        public DialogPref(Window parent) : base(
            "Preferences", parent, DialogFlags.Modal,
            "Cancel", ResponseType.Cancel,
            "OK", ResponseType.Ok)
        {
            SetDefaultSize(750, 550);

            PrefIO.read();
            BuildPropTable();
            BuildUI();
            PopulateTree();
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
            propTable["Join Lines That End With One of the Following Characters Subs1"] = convertToTokens(ConstantSettings.DefaultJoinSentencesCharListSubs1);

            // default_join_sentences_char_list_subs2
            propTable.Properties.Add(new PropertySpec("Join Lines That End With One of the Following Characters Subs2", typeof(string),
                "User Interface Defaults",
                "Specify the list of characters in the 'Join Lines That End With One of the Following Characters' Subs2 option.",
                PrefDefaults.DefaultJoinSentencesCharListSubs2));
            propTable["Join Lines That End With One of the Following Characters Subs2"] = convertToTokens(ConstantSettings.DefaultJoinSentencesCharListSubs2);

            // srs_filename_format
            propTable.Properties.Add(new PropertySpec("SRS Filename Format", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for SRS (ex. Anki) import filename.\n\n"
                + "Supported Tokens: All except ${episode_num}, ${sequence_num}, ${subs1_line}, ${subs2_line}, or any of the time tokens.",
                convertToTokens(PrefDefaults.SrsFilenameFormat)));
            propTable["SRS Filename Format"] = convertToTokens(ConstantSettings.SrsFilenameFormat);

            // srs_delimiter
            propTable.Properties.Add(new PropertySpec("Delimiter", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The delimiter to use for the SRS (ex. Anki) import file.\n\nSupported Tokens: Only ${tab}.",
                convertToTokens(PrefDefaults.SrsDelimiter)));
            propTable["Delimiter"] = convertToTokens(ConstantSettings.SrsDelimiter);

            // srs_tag_format
            propTable.Properties.Add(new PropertySpec("Tag Format", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the tag in the SRS import file. Leave blank if you do not want to include it.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.SrsTagFormat)));
            propTable["Tag Format"] = convertToTokens(ConstantSettings.SrsTagFormat);

            // srs_sequence_marker_format
            propTable.Properties.Add(new PropertySpec("Sequence Marker Format", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the sequence marker in the SRS import file. Leave blank if you do not want to include it.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.SrsSequenceMarkerFormat)));
            propTable["Sequence Marker Format"] = convertToTokens(ConstantSettings.SrsSequenceMarkerFormat);

            // srs_audio_filename_prefix
            propTable.Properties.Add(new PropertySpec("Audio Clip Prefix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the prefix of the audio entry in the SRS import file.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.SrsAudioFilenamePrefix)));
            propTable["Audio Clip Prefix"] = convertToTokens(ConstantSettings.SrsAudioFilenamePrefix);

            // audio_filename_format
            propTable.Properties.Add(new PropertySpec("Audio Clip Filename Format", typeof(string),
                "Audio Clip Formatting (Uses Tokens)",
                "The format to use for audio clip filenames. You must ensure that each filename will be unique.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.AudioFilenameFormat)));
            propTable["Audio Clip Filename Format"] = convertToTokens(ConstantSettings.AudioFilenameFormat);

            // audio_id3_artist
            propTable.Properties.Add(new PropertySpec("ID3 Tag Artist", typeof(string),
                "Audio Clip Formatting (Uses Tokens)",
                "The format to use for the audio file's ID3 Artist tag.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.AudioId3Artist)));
            propTable["ID3 Tag Artist"] = convertToTokens(ConstantSettings.AudioId3Artist);

            // audio_id3_album
            propTable.Properties.Add(new PropertySpec("ID3 Tag Album", typeof(string),
                "Audio Clip Formatting (Uses Tokens)",
                "The format to use for the audio file's ID3 Album tag.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.AudioId3Album)));
            propTable["ID3 Tag Album"] = convertToTokens(ConstantSettings.AudioId3Album);

            // audio_id3_title
            propTable.Properties.Add(new PropertySpec("ID3 Tag Title", typeof(string),
                "Audio Clip Formatting (Uses Tokens)",
                "The format to use for the audio file's ID3 Title tag.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.AudioId3Title)));
            propTable["ID3 Tag Title"] = convertToTokens(ConstantSettings.AudioId3Title);

            // audio_id3_genre
            propTable.Properties.Add(new PropertySpec("ID3 Tag Genre", typeof(string),
                "Audio Clip Formatting (Uses Tokens)",
                "The format to use for the audio file's ID3 Genre tag.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.AudioId3Genre)));
            propTable["ID3 Tag Genre"] = convertToTokens(ConstantSettings.AudioId3Genre);

            // audio_id3_lyrics
            propTable.Properties.Add(new PropertySpec("ID3 Tag Lyrics", typeof(string),
                "Audio Clip Formatting (Uses Tokens)",
                "The format to use for the audio file's ID3 Lyrics tag.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.AudioId3Lyrics)));
            propTable["ID3 Tag Lyrics"] = convertToTokens(ConstantSettings.AudioId3Lyrics);

            // srs_audio_filename_suffix
            propTable.Properties.Add(new PropertySpec("Audio Clip Suffix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the suffix of the audio entry in the SRS import file.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.SrsAudioFilenameSuffix)));
            propTable["Audio Clip Suffix"] = convertToTokens(ConstantSettings.SrsAudioFilenameSuffix);

            // srs_snapshot_filename_prefix
            propTable.Properties.Add(new PropertySpec("Snapshot Prefix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the prefix of the snapshot entry in the SRS import file.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.SrsSnapshotFilenamePrefix)));
            propTable["Snapshot Prefix"] = convertToTokens(ConstantSettings.SrsSnapshotFilenamePrefix);

            // snapshot_filename_format
            propTable.Properties.Add(new PropertySpec("Snapshot Filename Format", typeof(string),
                "Snapshot Formatting (Uses Tokens)",
                "The format to use for snapshot filenames. You must ensure that each filename will be unique.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.SnapshotFilenameFormat)));
            propTable["Snapshot Filename Format"] = convertToTokens(ConstantSettings.SnapshotFilenameFormat);

            // srs_snapshot_filename_suffix
            propTable.Properties.Add(new PropertySpec("Snapshot Suffix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the suffix of the snapshot entry in the SRS import file.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.SrsSnapshotFilenameSuffix)));
            propTable["Snapshot Suffix"] = convertToTokens(ConstantSettings.SrsSnapshotFilenameSuffix);

            // srs_video_filename_prefix
            propTable.Properties.Add(new PropertySpec("Video Clip Prefix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the prefix of the video entry in the SRS import file.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.SrsVideoFilenamePrefix)));
            propTable["Video Clip Prefix"] = convertToTokens(ConstantSettings.SrsVideoFilenamePrefix);

            // video_filename_format
            propTable.Properties.Add(new PropertySpec("Video Clip Filename Format", typeof(string),
                "Video Formatting (Uses Tokens)",
                "The format to use for video clip filenames. You must ensure that each filename will be unique.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.VideoFilenameFormat)));
            propTable["Video Clip Filename Format"] = convertToTokens(ConstantSettings.VideoFilenameFormat);

            // srs_video_filename_suffix
            propTable.Properties.Add(new PropertySpec("Video Clip Suffix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the suffix of the video entry in the SRS import file.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.SrsVideoFilenameSuffix)));
            propTable["Video Clip Suffix"] = convertToTokens(ConstantSettings.SrsVideoFilenameSuffix);

            // srs_vobsub_filename_prefix
            propTable.Properties.Add(new PropertySpec("Vobsub Prefix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the prefix of the vobsub entry in the SRS import file.\n\nSupported Tokens: Only ${deck_name}.",
                convertToTokens(PrefDefaults.SrsVobsubFilenamePrefix)));
            propTable["Vobsub Prefix"] = convertToTokens(ConstantSettings.SrsVobsubFilenamePrefix);

            // srs_vobsub_filename_suffix
            propTable.Properties.Add(new PropertySpec("Vobsub Suffix", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use for the suffix of the vobsub entry in the SRS import file.\n\nSupported Tokens: Only ${deck_name}.",
                convertToTokens(PrefDefaults.SrsVobsubFilenameSuffix)));
            propTable["Vobsub Suffix"] = convertToTokens(ConstantSettings.SrsVobsubFilenameSuffix);

            // srs_subs1_format
            propTable.Properties.Add(new PropertySpec("Subs1 Format", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use when adding Subs1 to the SRS import file.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.SrsSubs1Format)));
            propTable["Subs1 Format"] = convertToTokens(ConstantSettings.SrsSubs1Format);

            // srs_subs2_format
            propTable.Properties.Add(new PropertySpec("Subs2 Format", typeof(string),
                "SRS File Formatting (Uses Tokens)",
                "The format to use when adding Subs2 to the SRS import file.\n\nSupported Tokens: All.",
                convertToTokens(PrefDefaults.SrsSubs2Format)));
            propTable["Subs2 Format"] = convertToTokens(ConstantSettings.SrsSubs2Format);

            // extract_media_audio_filename_format
            propTable.Properties.Add(new PropertySpec("Extract Audio From Media Filename Format", typeof(string),
                "Extract Audio from Media Formats (Uses Tokens)",
                "The format to use for audio filenames in the Extract Audio from Media dialog.\n\nSupported Tokens: All except ${subs1_line}, ${subs2_line}, ${width} and ${height}.",
                convertToTokens(PrefDefaults.ExtractMediaAudioFilenameFormat)));
            propTable["Extract Audio From Media Filename Format"] = convertToTokens(ConstantSettings.ExtractMediaAudioFilenameFormat);

            // extract_media_lyrics_subs1_format
            propTable.Properties.Add(new PropertySpec("Lyrics Subs1 Format", typeof(string),
                "Extract Audio from Media Formats (Uses Tokens)",
                "The format to use when adding Subs1 to the audio file's ID3 Lyrics tag.\n\nSupported Tokens: All except ${width} and ${height}.",
                convertToTokens(PrefDefaults.ExtractMediaLyricsSubs1Format)));
            propTable["Lyrics Subs1 Format"] = convertToTokens(ConstantSettings.ExtractMediaLyricsSubs1Format);

            // extract_media_lyrics_subs2_format
            propTable.Properties.Add(new PropertySpec("Lyrics Subs2 Format", typeof(string),
                "Extract Audio from Media Formats (Uses Tokens)",
                "The format to use when adding Subs2 to the audio file's ID3 Lyrics tag.\n\nSupported Tokens: All except ${width} and ${height}.",
                convertToTokens(PrefDefaults.ExtractMediaLyricsSubs2Format)));
            propTable["Lyrics Subs2 Format"] = convertToTokens(ConstantSettings.ExtractMediaLyricsSubs2Format);

            // dueling_subtitle_filename_format
            propTable.Properties.Add(new PropertySpec("Dueling Subtitles Filename Format", typeof(string),
                "Dueling Subtitles Formats (Uses Tokens)",
                "The format to use for dueling subtitle filenames.\n\nSupported Tokens: All except ${sequence_num}, ${subs1_line}, ${subs2_line}, ${width}, and ${height}.",
                convertToTokens(PrefDefaults.DuelingSubtitleFilenameFormat)));
            propTable["Dueling Subtitles Filename Format"] = convertToTokens(ConstantSettings.DuelingSubtitleFilenameFormat);

            // dueling_quick_ref_filename_format
            propTable.Properties.Add(new PropertySpec("Quick Reference Filename Format", typeof(string),
                "Dueling Subtitles Formats (Uses Tokens)",
                "The format to use for dueling subtitle quick reference filenames.\n\nSupported Tokens: All except ${sequence_num}, ${subs1_line}, ${subs2_line}, ${width}, and ${height}.",
                convertToTokens(PrefDefaults.DuelingQuickRefFilenameFormat)));
            propTable["Quick Reference Filename Format"] = convertToTokens(ConstantSettings.DuelingQuickRefFilenameFormat);

            // dueling_quick_ref_subs1_format
            propTable.Properties.Add(new PropertySpec("Quick Reference Subs1 Format", typeof(string),
                "Dueling Subtitles Formats (Uses Tokens)",
                "The format to use when adding Subs1 to the quick reference file.\n\nSupported Tokens: All except ${width} and ${height}.",
                convertToTokens(PrefDefaults.DuelingQuickRefSubs1Format)));
            propTable["Quick Reference Subs1 Format"] = convertToTokens(ConstantSettings.DuelingQuickRefSubs1Format);

            // dueling_quick_ref_subs2_format
            propTable.Properties.Add(new PropertySpec("Quick Reference Subs2 Format", typeof(string),
                "Dueling Subtitles Formats (Uses Tokens)",
                "The format to use when adding Subs2 to the quick reference file.\n\nSupported Tokens: All except ${width} and ${height}.",
                convertToTokens(PrefDefaults.DuelingQuickRefSubs2Format)));
            propTable["Quick Reference Subs2 Format"] = convertToTokens(ConstantSettings.DuelingQuickRefSubs2Format);

            // video_player
            propTable.Properties.Add(new PropertySpec("Video Player Path", typeof(string),
                "Video Player (Uses Tokens)",
                "The video player to use in the Preview dialog.\n\nSupported Tokens: None.",
                PrefDefaults.VideoPlayer));
            propTable["Video Player Path"] = convertToTokens(ConstantSettings.VideoPlayer);

            // video_player_args
            propTable.Properties.Add(new PropertySpec("Video Player Arguments", typeof(string),
                "Video Player (Uses Tokens)",
                "The video player arguments to pass to the video player in the Preview dialog.\n\nSupported Tokens: All except ${subs1_line}, ${subs2_line}, ${total_line_num} and ${sequence_num}.",
                convertToTokens(PrefDefaults.VideoPlayerArgs)));
            propTable["Video Player Arguments"] = convertToTokens(ConstantSettings.VideoPlayerArgs);

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
        }

        // ── GTK UI ──────────────────────────────────────────────────────────

        private void BuildUI()
        {
            _store = new TreeStore(
                typeof(string), typeof(string), typeof(bool),
                typeof(bool), typeof(bool), typeof(string),
                typeof(string), typeof(int), typeof(bool));

            _treeView = new TreeView(_store) { HeadersVisible = true };
            _treeView.Selection.Changed += OnSelectionChanged;

            // Name column
            var nameR = new CellRendererText();
            var nameCol = new TreeViewColumn();
            nameCol.Title = "Property";
            nameCol.PackStart(nameR, true);
            nameCol.AddAttribute(nameR, "text", COL_NAME);
            nameCol.AddAttribute(nameR, "weight", COL_WEIGHT);
            nameCol.Resizable = true;
            nameCol.MinWidth = 300;
            _treeView.AppendColumn(nameCol);

            // Value column — dual renderer: toggle for bools, text for the rest
            var valCol = new TreeViewColumn { Title = "Value", Resizable = true, MinWidth = 200 };

            var toggleR = new CellRendererToggle { Activatable = true };
            toggleR.Toggled += OnBoolToggled;
            valCol.PackStart(toggleR, false);
            valCol.SetCellDataFunc(toggleR, (TreeViewColumn c, CellRenderer cell, ITreeModel m, TreeIter it) =>
            {
                bool isBool = (bool)m.GetValue(it, COL_IS_BOOL);
                bool isCat = (bool)m.GetValue(it, COL_IS_CATEGORY);
                var t = (CellRendererToggle)cell;
                t.Visible = isBool && !isCat;
                if (isBool && !isCat) t.Active = (bool)m.GetValue(it, COL_BOOL_VAL);
            });

            var textR = new CellRendererText();
            textR.Edited += OnValueEdited;
            valCol.PackStart(textR, true);
            valCol.SetCellDataFunc(textR, (TreeViewColumn c, CellRenderer cell, ITreeModel m, TreeIter it) =>
            {
                bool isBool = (bool)m.GetValue(it, COL_IS_BOOL);
                bool isCat = (bool)m.GetValue(it, COL_IS_CATEGORY);
                var t = (CellRendererText)cell;
                t.Visible = !isBool && !isCat;
                t.Editable = !isBool && !isCat;
                t.Text = (!isBool && !isCat) ? (string)m.GetValue(it, COL_STR_VAL) : "";
            });

            _treeView.AppendColumn(valCol);

            var sw = new ScrolledWindow { ShadowType = ShadowType.In };
            sw.Add(_treeView);

            // Description area
            _descView = new TextView { Editable = false, WrapMode = WrapMode.Word, HeightRequest = 80 };
            var descSw = new ScrolledWindow { ShadowType = ShadowType.In };
            descSw.Add(_descView);

            // Tool buttons
            var btnBox = new Box(Orientation.Horizontal, 6);

            var btnResetAll = new Button("Reset All");
            btnResetAll.Clicked += OnResetAllClicked;
            btnBox.PackStart(btnResetAll, false, false, 0);

            var btnResetSel = new Button("Reset Selected");
            btnResetSel.Clicked += OnResetSelectedClicked;
            btnBox.PackStart(btnResetSel, false, false, 0);

            var btnTokens = new Button("Token List...");
            btnTokens.Clicked += OnTokenListClicked;
            btnBox.PackStart(btnTokens, false, false, 0);

            var vbox = new Box(Orientation.Vertical, 6) { BorderWidth = 8 };
            vbox.PackStart(sw, true, true, 0);
            vbox.PackStart(new Label("Description:") { Halign = Align.Start }, false, false, 0);
            vbox.PackStart(descSw, false, false, 0);
            vbox.PackStart(btnBox, false, false, 0);

            ContentArea.PackStart(vbox, true, true, 0);
            ContentArea.ShowAll();

            this.Response += OnResponse;
        }

        // ── TREE POPULATION ─────────────────────────────────────────────────

        private void PopulateTree()
        {
            _store.Clear();

            var categories = new List<string>();
            for (int i = 0; i < propTable.Properties.Count; i++)
            {
                string cat = propTable.Properties[i].Category;
                if (!categories.Contains(cat))
                    categories.Add(cat);
            }

            var catIters = new Dictionary<string, TreeIter>();
            foreach (string cat in categories)
            {
                catIters[cat] = _store.AppendValues(
                    cat, "", false, false, true, "", "", (int)Pango.Weight.Bold, false);
            }

            for (int i = 0; i < propTable.Properties.Count; i++)
            {
                var prop = propTable.Properties[i];
                object val = propTable[prop.Name];
                bool isBool = val is bool;
                bool isInt = val is int;

                _store.AppendValues(catIters[prop.Category],
                    prop.Name,
                    isBool ? "" : (val?.ToString() ?? ""),
                    isBool ? (bool)val : false,
                    isBool,
                    false,
                    prop.Description ?? "",
                    prop.Name,
                    (int)Pango.Weight.Normal,
                    isInt);
            }

            _treeView.ExpandAll();
        }

        // ── TREEVIEW EVENT HANDLERS ─────────────────────────────────────────

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (_treeView.Selection.GetSelected(out var model, out var iter))
                _descView.Buffer.Text = (string)model.GetValue(iter, COL_DESCRIPTION) ?? "";
            else
                _descView.Buffer.Text = "";
        }

        private void OnBoolToggled(object sender, ToggledArgs args)
        {
            if (!_store.GetIter(out var iter, new TreePath(args.Path))) return;
            if ((bool)_store.GetValue(iter, COL_IS_CATEGORY)) return;

            string key = (string)_store.GetValue(iter, COL_PROP_KEY);
            bool newVal = !(bool)_store.GetValue(iter, COL_BOOL_VAL);
            _store.SetValue(iter, COL_BOOL_VAL, newVal);
            propTable[key] = newVal;
        }

        private void OnValueEdited(object sender, EditedArgs args)
        {
            if (!_store.GetIter(out var iter, new TreePath(args.Path))) return;
            if ((bool)_store.GetValue(iter, COL_IS_CATEGORY)) return;

            string key = (string)_store.GetValue(iter, COL_PROP_KEY);
            bool isInt = (bool)_store.GetValue(iter, COL_IS_INT);

            if (isInt)
            {
                if (int.TryParse(args.NewText, out int val))
                {
                    propTable[key] = val;
                    _store.SetValue(iter, COL_STR_VAL, val.ToString());
                }
            }
            else
            {
                propTable[key] = args.NewText;
                _store.SetValue(iter, COL_STR_VAL, args.NewText);
            }
        }

        // ── BUTTON HANDLERS ─────────────────────────────────────────────────

        private void OnResetAllClicked(object sender, EventArgs e)
        {
            if (!UtilsMsg.showConfirm("Are you sure that you want to reset all preferences to default values?"))
                return;

            for (int i = 0; i < propTable.Properties.Count; i++)
                propTable[propTable.Properties[i].Name] = propTable.Properties[i].DefaultValue;

            PopulateTree();
        }

        private void OnResetSelectedClicked(object sender, EventArgs e)
        {
            if (!_treeView.Selection.GetSelected(out var model, out var iter)) return;
            if ((bool)model.GetValue(iter, COL_IS_CATEGORY)) return;

            string key = (string)model.GetValue(iter, COL_PROP_KEY);

            for (int i = 0; i < propTable.Properties.Count; i++)
            {
                if (propTable.Properties[i].Name == key)
                {
                    object def = propTable.Properties[i].DefaultValue;
                    propTable[key] = def;

                    if (def is bool bv)
                        _store.SetValue(iter, COL_BOOL_VAL, bv);
                    else
                        _store.SetValue(iter, COL_STR_VAL, def?.ToString() ?? "");

                    break;
                }
            }
        }

        private void OnTokenListClicked(object sender, EventArgs e)
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

        // ── RESPONSE / SAVE ─────────────────────────────────────────────────

        private void OnResponse(object sender, ResponseArgs args)
        {
            if (args.ResponseId == ResponseType.Ok)
                SavePreferences();
        }

        private void SavePreferences()
        {
            var pairList = new List<PrefIO.SettingsPair>();

            pairList.Add(new PrefIO.SettingsPair("main_window_width",
                UtilsCommon.checkRange((int)propTable["Main Window Width"], 0, 9999, PrefDefaults.MainWindowWidth).ToString()));
            pairList.Add(new PrefIO.SettingsPair("main_window_height",
                UtilsCommon.checkRange((int)propTable["Main Window Height"], 0, 9999, PrefDefaults.MainWindowHeight).ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_enable_audio_clip_generation", propTable["Enable Audio Clip Generation"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_enable_snapshots_generation", propTable["Enable Snapshots Generation"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_enable_video_clips_generation", propTable["Enable Video Clips Generation"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_audio_clip_bitrate",
                UtilsCommon.checkRangeInSet((int)propTable["Audio Clip Bitrate"],
                new List<int>(new[] { 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 192, 224, 256, 320 }),
                PrefDefaults.DefaultAudioClipBitrate).ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_audio_normalize", propTable["Normalize Audio"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_video_clip_video_bitrate",
                UtilsCommon.checkRange((int)propTable["Video Clip Video Bitrate"], 100, 3000, PrefDefaults.DefaultVideoClipVideoBitrate).ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_video_clip_audio_bitrate",
                UtilsCommon.checkRangeInSet((int)propTable["Video Clip Audio Bitrate"],
                new List<int>(new[] { 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 192, 224, 256, 320 }),
                PrefDefaults.DefaultVideoClipAudioBitrate).ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_ipod_support", propTable["iPhone Support"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_encoding_subs1",
                checkValidEncoding((string)propTable["Encoding Subs1"], PrefDefaults.DefaultEncodingSubs1)));
            pairList.Add(new PrefIO.SettingsPair("default_encoding_subs2",
                checkValidEncoding((string)propTable["Encoding Subs2"], PrefDefaults.DefaultEncodingSubs2)));
            pairList.Add(new PrefIO.SettingsPair("default_context_num_leading",
                UtilsCommon.checkRange((int)propTable["Context Number Leading"], 0, 9, PrefDefaults.DefaultContextNumLeading).ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_context_num_trailing",
                UtilsCommon.checkRange((int)propTable["Context Number Trailing"], 0, 9, PrefDefaults.DefaultContextNumTrailing).ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_context_leading_range",
                UtilsCommon.checkRange((int)propTable["Context Nearby Line Range Leading"], 0, 99999, PrefDefaults.DefaultContextLeadingRange).ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_context_trailing_range",
                UtilsCommon.checkRange((int)propTable["Context Nearby Line Range Trailing"], 0, 99999, PrefDefaults.DefaultContextTrailingRange).ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_file_browser_start_dir", convertOut("File Browser Start Folder")));
            pairList.Add(new PrefIO.SettingsPair("default_remove_styled_lines_subs1", propTable["Remove Subs1 Styled Lines"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_remove_styled_lines_subs2", propTable["Remove Subs2 Styled Lines"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_remove_no_counterpart_subs1", propTable["Remove Subs1 Lines With No Obvious Counterpart"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_remove_no_counterpart_subs2", propTable["Remove Subs2 Lines With No Obvious Counterpart"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_included_text_subs1", convertOut("Included Text Subs1")));
            pairList.Add(new PrefIO.SettingsPair("default_included_text_subs2", convertOut("Included Text Subs2")));
            pairList.Add(new PrefIO.SettingsPair("default_excluded_text_subs1", convertOut("Excluded Text Subs1")));
            pairList.Add(new PrefIO.SettingsPair("default_excluded_text_subs2", convertOut("Excluded Text Subs2")));
            pairList.Add(new PrefIO.SettingsPair("default_exclude_duplicate_lines_subs1", propTable["Exclude Duplicate Lines Subs1"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_exclude_duplicate_lines_subs2", propTable["Exclude Duplicate Lines Subs2"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_exclude_lines_with_fewer_than_n_chars_subs1", propTable["Exclude Lines With Fewer Than n Characters Enable Subs1"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_exclude_lines_with_fewer_than_n_chars_subs2", propTable["Exclude Lines With Fewer Than n Characters Enable Subs2"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_exclude_lines_with_fewer_than_n_chars_num_subs1",
                UtilsCommon.checkRange((int)propTable["Exclude Lines With Fewer Than n Characters Number Subs1"], 2, 99999, PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs1).ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_exclude_lines_with_fewer_than_n_chars_num_subs2",
                UtilsCommon.checkRange((int)propTable["Exclude Lines With Fewer Than n Characters Number Subs2"], 2, 99999, PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs2).ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_exclude_lines_shorter_than_n_ms_subs1", propTable["Exclude Lines Shorter Than n Milliseconds Enable Subs1"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_exclude_lines_shorter_than_n_ms_subs2", propTable["Exclude Lines Shorter Than n Milliseconds Enable Subs2"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_exclude_lines_shorter_than_n_ms_num_subs1",
                UtilsCommon.checkRange((int)propTable["Exclude Lines Shorter Than n Milliseconds Number Subs1"], 100, 99999, PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs1).ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_exclude_lines_shorter_than_n_ms_num_subs2",
                UtilsCommon.checkRange((int)propTable["Exclude Lines Shorter Than n Milliseconds Number Subs2"], 100, 99999, PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs2).ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_exclude_lines_longer_than_n_ms_subs1", propTable["Exclude Lines Longer Than n Milliseconds Enable Subs1"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_exclude_lines_longer_than_n_ms_subs2", propTable["Exclude Lines Longer Than n Milliseconds Enable Subs2"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_exclude_lines_longer_than_n_ms_num_subs1",
                UtilsCommon.checkRange((int)propTable["Exclude Lines Longer Than n Milliseconds Number Subs1"], 100, 99999, PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs1).ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_exclude_lines_longer_than_n_ms_num_subs2",
                UtilsCommon.checkRange((int)propTable["Exclude Lines Longer Than n Milliseconds Number Subs2"], 100, 99999, PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs2).ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_join_sentences_subs1", propTable["Join Lines That End With One of the Following Characters Enable Subs1"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_join_sentences_subs2", propTable["Join Lines That End With One of the Following Characters Enable Subs2"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("default_join_sentences_char_list_subs1", preventBlank("Join Lines That End With One of the Following Characters Subs1")));
            pairList.Add(new PrefIO.SettingsPair("default_join_sentences_char_list_subs2", preventBlank("Join Lines That End With One of the Following Characters Subs2")));
            pairList.Add(new PrefIO.SettingsPair("srs_filename_format", preventBlank("SRS Filename Format")));
            pairList.Add(new PrefIO.SettingsPair("srs_delimiter", preventBlank("Delimiter")));
            pairList.Add(new PrefIO.SettingsPair("srs_tag_format", convertOut("Tag Format")));
            pairList.Add(new PrefIO.SettingsPair("srs_sequence_marker_format", convertOut("Sequence Marker Format")));
            pairList.Add(new PrefIO.SettingsPair("srs_audio_filename_prefix", convertOut("Audio Clip Prefix")));
            pairList.Add(new PrefIO.SettingsPair("audio_filename_format", preventBlank("Audio Clip Filename Format")));
            pairList.Add(new PrefIO.SettingsPair("audio_id3_artist", convertOut("ID3 Tag Artist")));
            pairList.Add(new PrefIO.SettingsPair("audio_id3_album", convertOut("ID3 Tag Album")));
            pairList.Add(new PrefIO.SettingsPair("audio_id3_title", convertOut("ID3 Tag Title")));
            pairList.Add(new PrefIO.SettingsPair("audio_id3_genre", convertOut("ID3 Tag Genre")));
            pairList.Add(new PrefIO.SettingsPair("audio_id3_lyrics", convertOut("ID3 Tag Lyrics")));
            pairList.Add(new PrefIO.SettingsPair("srs_audio_filename_suffix", convertOut("Audio Clip Suffix")));
            pairList.Add(new PrefIO.SettingsPair("srs_snapshot_filename_prefix", convertOut("Snapshot Prefix")));
            pairList.Add(new PrefIO.SettingsPair("snapshot_filename_format", preventBlank("Snapshot Filename Format")));
            pairList.Add(new PrefIO.SettingsPair("srs_snapshot_filename_suffix", convertOut("Snapshot Suffix")));
            pairList.Add(new PrefIO.SettingsPair("srs_video_filename_prefix", convertOut("Video Clip Prefix")));
            pairList.Add(new PrefIO.SettingsPair("video_filename_format", preventBlank("Video Clip Filename Format")));
            pairList.Add(new PrefIO.SettingsPair("srs_video_filename_suffix", convertOut("Video Clip Suffix")));
            pairList.Add(new PrefIO.SettingsPair("srs_vobsub_filename_prefix", convertOut("Vobsub Prefix")));
            pairList.Add(new PrefIO.SettingsPair("srs_vobsub_filename_suffix", convertOut("Vobsub Suffix")));
            pairList.Add(new PrefIO.SettingsPair("srs_subs1_format", preventBlank("Subs1 Format")));
            pairList.Add(new PrefIO.SettingsPair("srs_subs2_format", preventBlank("Subs2 Format")));
            pairList.Add(new PrefIO.SettingsPair("extract_media_audio_filename_format", preventBlank("Extract Audio From Media Filename Format")));
            pairList.Add(new PrefIO.SettingsPair("extract_media_lyrics_subs1_format", preventBlank("Lyrics Subs1 Format")));
            pairList.Add(new PrefIO.SettingsPair("extract_media_lyrics_subs2_format", convertOut("Lyrics Subs2 Format")));
            pairList.Add(new PrefIO.SettingsPair("dueling_subtitle_filename_format", preventBlank("Dueling Subtitles Filename Format")));
            pairList.Add(new PrefIO.SettingsPair("dueling_quick_ref_filename_format", preventBlank("Quick Reference Filename Format")));
            pairList.Add(new PrefIO.SettingsPair("dueling_quick_ref_subs1_format", preventBlank("Quick Reference Subs1 Format")));
            pairList.Add(new PrefIO.SettingsPair("dueling_quick_ref_subs2_format", convertOut("Quick Reference Subs2 Format")));
            pairList.Add(new PrefIO.SettingsPair("video_player", convertOut("Video Player Path")));
            pairList.Add(new PrefIO.SettingsPair("video_player_args", convertOut("Video Player Arguments")));
            pairList.Add(new PrefIO.SettingsPair("reencode_before_splitting_audio", propTable["Re-encode Before Splitting Audio"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("enable_logging", propTable["Enable Logging"].ToString()));
            pairList.Add(new PrefIO.SettingsPair("audio_normalize_args", convertOut("Normalize Audio Arguments")));
            pairList.Add(new PrefIO.SettingsPair("long_clip_warning_seconds",
                UtilsCommon.checkRange((int)propTable["Long Clip Warning"], 0, 99999, PrefDefaults.LongClipWarningSeconds).ToString()));

            PrefIO.writeString(pairList);
            PrefIO.read();
        }

        // ── HELPERS ─────────────────────────────────────────────────────────

        private string convertToTokens(string format)
        {
            return format.Replace("\t", "${tab}").Replace("\r", "${cr}").Replace("\n", "${lf}");
        }

        private string convertOut(string pref)
        {
            string value = (string)propTable[pref];
            return (value ?? "").Trim() == "" ? "none" : value;
        }

        private string preventBlank(string pref)
        {
            string value = (string)propTable[pref];
            if ((value ?? "").Trim() == "")
            {
                for (int i = 0; i < propTable.Properties.Count; i++)
                {
                    if (propTable.Properties[i].Name == pref)
                        return propTable.Properties[i].DefaultValue.ToString();
                }
            }
            return value;
        }

        private string checkValidEncoding(string encoding, string def)
        {
            return InfoEncoding.isValidShortEncoding(encoding) ? encoding : def;
        }
    }
}
