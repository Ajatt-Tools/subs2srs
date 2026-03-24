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

namespace subs2srs
{
    /// <summary>
    /// POCO that holds every user preference.
    /// Serialized as JSON by PrefIO.  All defaults come from PrefDefaults.
    /// To add a new preference: add a property here, then wire it in
    /// ConstantSettings, DialogPref.BuildPropTable, DialogPref.SavePreferences,
    /// and Logger.writeSettingsToLog.
    /// </summary>
    public class PreferencesData
    {
        // ── Window ───────────────────────────────────────────────────────────
        public int MainWindowWidth { get; set; } = PrefDefaults.MainWindowWidth;
        public int MainWindowHeight { get; set; } = PrefDefaults.MainWindowHeight;

        // ── Generation toggles ───────────────────────────────────────────────
        public bool DefaultEnableAudioClipGeneration { get; set; } = PrefDefaults.DefaultEnableAudioClipGeneration;
        public bool DefaultEnableSnapshotsGeneration { get; set; } = PrefDefaults.DefaultEnableSnapshotsGeneration;
        public bool DefaultEnableVideoClipsGeneration { get; set; } = PrefDefaults.DefaultEnableVideoClipsGeneration;

        // ── Video player ─────────────────────────────────────────────────────
        public string VideoPlayer { get; set; } = PrefDefaults.VideoPlayer;
        public string VideoPlayerArgs { get; set; } = PrefDefaults.VideoPlayerArgs;

        // ── Misc ─────────────────────────────────────────────────────────────
        public bool ReencodeBeforeSplittingAudio { get; set; } = PrefDefaults.ReencodeBeforeSplittingAudio;
        public bool EnableLogging { get; set; } = PrefDefaults.EnableLogging;
        public string AudioNormalizeArgs { get; set; } = PrefDefaults.AudioNormalizeArgs;
        public int LongClipWarningSeconds { get; set; } = PrefDefaults.LongClipWarningSeconds;
        public int MaxParallelTasks { get; set; } = PrefDefaults.MaxParallelTasks;

        // ── Audio clips ──────────────────────────────────────────────────────
        public int DefaultAudioClipBitrate { get; set; } = PrefDefaults.DefaultAudioClipBitrate;
        public bool DefaultAudioNormalize { get; set; } = PrefDefaults.DefaultAudioNormalize;

        // ── Video clips ──────────────────────────────────────────────────────
        public int DefaultVideoClipVideoBitrate { get; set; } = PrefDefaults.DefaultVideoClipVideoBitrate;
        public int DefaultVideoClipAudioBitrate { get; set; } = PrefDefaults.DefaultVideoClipAudioBitrate;
        public int DefaultSnapshotJpegQuality { get; set; } = PrefDefaults.DefaultSnapshotJpegQuality;
        public bool DefaultIphoneSupport { get; set; } = PrefDefaults.DefaultIphoneSupport;

        // ── Encoding ─────────────────────────────────────────────────────────
        public string DefaultEncodingSubs1 { get; set; } = PrefDefaults.DefaultEncodingSubs1;
        public string DefaultEncodingSubs2 { get; set; } = PrefDefaults.DefaultEncodingSubs2;

        // ── Context ──────────────────────────────────────────────────────────
        public int DefaultContextNumLeading { get; set; } = PrefDefaults.DefaultContextNumLeading;
        public int DefaultContextNumTrailing { get; set; } = PrefDefaults.DefaultContextNumTrailing;
        public int DefaultContextLeadingRange { get; set; } = PrefDefaults.DefaultContextLeadingRange;
        public int DefaultContextTrailingRange { get; set; } = PrefDefaults.DefaultContextTrailingRange;

        // ── File browser ─────────────────────────────────────────────────────
        public string DefaultFileBrowserStartDir { get; set; } = PrefDefaults.DefaultFileBrowserStartDir;

        // ── Output directory ────────────────────────────────────────────────
        public string DefaultOutputDir { get; set; } = PrefDefaults.DefaultOutputDir;

        // ── Line filtering ───────────────────────────────────────────────────
        public bool DefaultRemoveStyledLinesSubs1 { get; set; } = PrefDefaults.DefaultRemoveStyledLinesSubs1;
        public bool DefaultRemoveStyledLinesSubs2 { get; set; } = PrefDefaults.DefaultRemoveStyledLinesSubs2;
        public bool DefaultRemoveNoCounterpartSubs1 { get; set; } = PrefDefaults.DefaultRemoveNoCounterpartSubs1;
        public bool DefaultRemoveNoCounterpartSubs2 { get; set; } = PrefDefaults.DefaultRemoveNoCounterpartSubs2;

        public string DefaultIncludeTextSubs1 { get; set; } = PrefDefaults.DefaultIncludeTextSubs1;
        public string DefaultIncludeTextSubs2 { get; set; } = PrefDefaults.DefaultIncludeTextSubs2;
        public string DefaultExcludeTextSubs1 { get; set; } = PrefDefaults.DefaultExcludeTextSubs1;
        public string DefaultExcludeTextSubs2 { get; set; } = PrefDefaults.DefaultExcludeTextSubs2;

        public bool DefaultExcludeDuplicateLinesSubs1 { get; set; } = PrefDefaults.DefaultExcludeDuplicateLinesSubs1;
        public bool DefaultExcludeDuplicateLinesSubs2 { get; set; } = PrefDefaults.DefaultExcludeDuplicateLinesSubs2;

        public bool DefaultExcludeLinesFewerThanCharsSubs1 { get; set; } = PrefDefaults.DefaultExcludeLinesFewerThanCharsSubs1;
        public bool DefaultExcludeLinesFewerThanCharsSubs2 { get; set; } = PrefDefaults.DefaultExcludeLinesFewerThanCharsSubs2;
        public int DefaultExcludeLinesFewerThanCharsNumSubs1 { get; set; } = PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs1;
        public int DefaultExcludeLinesFewerThanCharsNumSubs2 { get; set; } = PrefDefaults.DefaultExcludeLinesFewerThanCharsNumSubs2;

        public bool DefaultExcludeLinesShorterThanMsSubs1 { get; set; } = PrefDefaults.DefaultExcludeLinesShorterThanMsSubs1;
        public bool DefaultExcludeLinesShorterThanMsSubs2 { get; set; } = PrefDefaults.DefaultExcludeLinesShorterThanMsSubs2;
        public int DefaultExcludeLinesShorterThanMsNumSubs1 { get; set; } = PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs1;
        public int DefaultExcludeLinesShorterThanMsNumSubs2 { get; set; } = PrefDefaults.DefaultExcludeLinesShorterThanMsNumSubs2;

        public bool DefaultExcludeLinesLongerThanMsSubs1 { get; set; } = PrefDefaults.DefaultExcludeLinesLongerThanMsSubs1;
        public bool DefaultExcludeLinesLongerThanMsSubs2 { get; set; } = PrefDefaults.DefaultExcludeLinesLongerThanMsSubs2;
        public int DefaultExcludeLinesLongerThanMsNumSubs1 { get; set; } = PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs1;
        public int DefaultExcludeLinesLongerThanMsNumSubs2 { get; set; } = PrefDefaults.DefaultExcludeLinesLongerThanMsNumSubs2;

        // ── Join sentences ───────────────────────────────────────────────────
        public bool DefaultJoinSentencesSubs1 { get; set; } = PrefDefaults.DefaultJoinSentencesSubs1;
        public bool DefaultJoinSentencesSubs2 { get; set; } = PrefDefaults.DefaultJoinSentencesSubs2;
        public string DefaultJoinSentencesCharListSubs1 { get; set; } = PrefDefaults.DefaultJoinSentencesCharListSubs1;
        public string DefaultJoinSentencesCharListSubs2 { get; set; } = PrefDefaults.DefaultJoinSentencesCharListSubs2;

        // ── SRS file formatting ──────────────────────────────────────────────
        public string SrsFilenameFormat { get; set; } = PrefDefaults.SrsFilenameFormat;
        public string SrsDelimiter { get; set; } = PrefDefaults.SrsDelimiter;
        public string SrsTagFormat { get; set; } = PrefDefaults.SrsTagFormat;
        public string SrsSequenceMarkerFormat { get; set; } = PrefDefaults.SrsSequenceMarkerFormat;

        // ── Audio clip formatting ────────────────────────────────────────────
        public string SrsAudioFilenamePrefix { get; set; } = PrefDefaults.SrsAudioFilenamePrefix;
        public string SrsAudioFilenameSuffix { get; set; } = PrefDefaults.SrsAudioFilenameSuffix;
        public string AudioFilenameFormat { get; set; } = PrefDefaults.AudioFilenameFormat;
        public string AudioId3Artist { get; set; } = PrefDefaults.AudioId3Artist;
        public string AudioId3Album { get; set; } = PrefDefaults.AudioId3Album;
        public string AudioId3Title { get; set; } = PrefDefaults.AudioId3Title;
        public string AudioId3Genre { get; set; } = PrefDefaults.AudioId3Genre;
        public string AudioId3Lyrics { get; set; } = PrefDefaults.AudioId3Lyrics;

        // ── Snapshot formatting ──────────────────────────────────────────────
        public string SrsSnapshotFilenamePrefix { get; set; } = PrefDefaults.SrsSnapshotFilenamePrefix;
        public string SrsSnapshotFilenameSuffix { get; set; } = PrefDefaults.SrsSnapshotFilenameSuffix;
        public string SnapshotFilenameFormat { get; set; } = PrefDefaults.SnapshotFilenameFormat;

        // ── Video formatting ─────────────────────────────────────────────────
        public string SrsVideoFilenamePrefix { get; set; } = PrefDefaults.SrsVideoFilenamePrefix;
        public string SrsVideoFilenameSuffix { get; set; } = PrefDefaults.SrsVideoFilenameSuffix;
        public string VideoFilenameFormat { get; set; } = PrefDefaults.VideoFilenameFormat;

        // ── Subs formatting ──────────────────────────────────────────────────
        public string SrsSubs1Format { get; set; } = PrefDefaults.SrsSubs1Format;
        public string SrsSubs2Format { get; set; } = PrefDefaults.SrsSubs2Format;

        // ── Vobsub formatting ────────────────────────────────────────────────
        public string SrsVobsubFilenamePrefix { get; set; } = PrefDefaults.SrsVobsubFilenamePrefix;
        public string SrsVobsubFilenameSuffix { get; set; } = PrefDefaults.SrsVobsubFilenameSuffix;
        public string VobsubFilenameFormat { get; set; } = PrefDefaults.VobsubFilenameFormat;

        // ── Extract media ────────────────────────────────────────────────────
        public string ExtractMediaAudioFilenameFormat { get; set; } = PrefDefaults.ExtractMediaAudioFilenameFormat;
        public string ExtractMediaLyricsSubs1Format { get; set; } = PrefDefaults.ExtractMediaLyricsSubs1Format;
        public string ExtractMediaLyricsSubs2Format { get; set; } = PrefDefaults.ExtractMediaLyricsSubs2Format;

        // ── Dueling subtitles ────────────────────────────────────────────────
        public string DuelingSubtitleFilenameFormat { get; set; } = PrefDefaults.DuelingSubtitleFilenameFormat;
        public string DuelingQuickRefFilenameFormat { get; set; } = PrefDefaults.DuelingQuickRefFilenameFormat;
        public string DuelingQuickRefSubs1Format { get; set; } = PrefDefaults.DuelingQuickRefSubs1Format;
        public string DuelingQuickRefSubs2Format { get; set; } = PrefDefaults.DuelingQuickRefSubs2Format;
    }
}
