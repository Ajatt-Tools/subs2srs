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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtk;
using IOPath = System.IO.Path;

namespace subs2srs
{
    public class DialogExtractAudioFromMedia : Dialog
    {
        private SaveSettings oldSettings = new SaveSettings();

        // Media
        private Entry _txtMediaFile;
        private ComboBoxText _comboAudioStream;

        // Output
        private Entry _txtOutputDir;

        // Span
        private CheckButton _chkSpan;
        private Entry _txtSpanStart, _txtSpanEnd;

        // Bitrate
        private ComboBoxText _comboBitrate;

        // Format
        private RadioButton _radioSingle, _radioMultiple;
        private Entry _txtClipLength;

        // Lyrics
        private CheckButton _chkLyrics;
        private Box _lyricsContent;
        private Entry _txtSubs1File, _txtSubs2File;
        private ComboBoxText _comboEncSubs1, _comboEncSubs2;
        private RadioButton _radioTimingSubs1, _radioTimingSubs2;
        private CheckButton _chkTimeShift;
        private SpinButton _spinTimeShiftSubs1, _spinTimeShiftSubs2;
        private CheckButton _chkRemoveNoCounterS1, _chkRemoveNoCounterS2;
        private CheckButton _chkRemoveStyledS1, _chkRemoveStyledS2;

        // Naming
        private Entry _txtName;
        private SpinButton _spinEpisodeStart;

        // Progress
        private ProgressBar _progressBar;
        private Button _btnExtract, _btnCancel;
        private bool _cancelRequested;
        private string lastDirPath = "";

        // Worker data
        private string[] mediaFiles;
        private string deckName;
        private string outputDir;
        private int episodeStartNumber;
        private bool isSingleFile;
        private TimeSpan clipLength;
        private int bitrate;
        private bool useSpan;
        private TimeSpan spanStart, spanEnd;
        private InfoStream audioStream;

        public string MediaFilePattern { set => _txtMediaFile.Text = value; }
        public string Subs1FilePattern { set => _txtSubs1File.Text = value; }
        public string Subs2FilePattern { set => _txtSubs2File.Text = value; }
        public string OutputDir { set => _txtOutputDir.Text = value; }
        public string DeckName { set => _txtName.Text = value; }
        public int EpisodeStartNumber { set => _spinEpisodeStart.Value = value; }

        public bool UseSubs1Timings
        {
            set { _radioTimingSubs1.Active = value; _radioTimingSubs2.Active = !value; }
        }

        public bool UseTimeShift { set => _chkTimeShift.Active = value; }
        public int TimeShiftSubs1 { set => _spinTimeShiftSubs1.Value = value; }
        public int TimeShiftSubs2 { set => _spinTimeShiftSubs2.Value = value; }

        public int Bitrate
        {
            set
            {
                string v = value.ToString();
                for (int i = 0; ; i++)
                {
                    _comboBitrate.Active = i;
                    if (_comboBitrate.ActiveText == null) { _comboBitrate.Active = 8; break; }
                    if (_comboBitrate.ActiveText == v) break;
                }
            }
        }

        public bool SpanEnabled { set => _chkSpan.Active = value; }
        public string SpanStart { set => _txtSpanStart.Text = value; }
        public string SpanEnd { set => _txtSpanEnd.Text = value; }

        public string EncodingSubs1
        {
            set => SetEncodingCombo(_comboEncSubs1, value);
        }

        public string EncodingSubs2
        {
            set => SetEncodingCombo(_comboEncSubs2, value);
        }

        public int AudioStreamIndex
        {
            set { if (value >= 0) _comboAudioStream.Active = value; }
        }

        public string FileBrowserStartDir
        {
            get => Directory.Exists(lastDirPath) ? lastDirPath : "";
            set => lastDirPath = value;
        }

        public DialogExtractAudioFromMedia(Window parent) : base(
            "Extract Audio from Media Tool", parent, DialogFlags.Modal)
        {
            SetDefaultSize(700, 620);
            BuildUI();
            LoadInitialState();
        }

        private void BuildUI()
        {
            var vbox = new Box(Orientation.Vertical, 6) { BorderWidth = 8 };

            // Help text
            var helpLabel = new Label("Use this tool to extract, convert and split the audio track from a media file.")
            {
                Halign = Align.Center,
                MarginBottom = 6
            };
            vbox.PackStart(helpLabel, false, false, 0);

            // ── MEDIA FILE ──────────────────────────────────────────────────
            var mediaGrid = new Grid { RowSpacing = 6, ColumnSpacing = 6 };
            int r = 0;

            mediaGrid.Attach(new Label("Media file:") { Halign = Align.End }, 0, r, 1, 1);
            _txtMediaFile = new Entry { Hexpand = true };
            _txtMediaFile.Changed += OnMediaFileChanged;
            mediaGrid.Attach(_txtMediaFile, 1, r, 1, 1);
            var btnMedia = new Button("Browse...");
            btnMedia.Clicked += (s, e) => { var f = SelectFile("Select Media File"); if (f != "") _txtMediaFile.Text = f; };
            mediaGrid.Attach(btnMedia, 2, r, 1, 1);
            r++;

            mediaGrid.Attach(new Label("Audio Stream:") { Halign = Align.End }, 0, r, 1, 1);
            _comboAudioStream = new ComboBoxText();
            _comboAudioStream.AppendText("0 - (Default)");
            _comboAudioStream.Active = 0;
            mediaGrid.Attach(_comboAudioStream, 1, r, 2, 1);
            r++;

            // ── OUTPUT DIR ──────────────────────────────────────────────────
            mediaGrid.Attach(new Label("Output Dir:") { Halign = Align.End }, 0, r, 1, 1);
            _txtOutputDir = new Entry { Hexpand = true };
            mediaGrid.Attach(_txtOutputDir, 1, r, 1, 1);
            var btnOut = new Button("Browse...");
            btnOut.Clicked += (s, e) => { var f = SelectFolder("Select Output Directory"); if (f != "") _txtOutputDir.Text = f; };
            mediaGrid.Attach(btnOut, 2, r, 1, 1);

            vbox.PackStart(mediaGrid, false, false, 0);
            vbox.PackStart(new Separator(Orientation.Horizontal), false, false, 4);

            // ── OPTIONS ─────────────────────────────────────────────────────
            var optFrame = new Frame("Options");
            var optBox = new Box(Orientation.Vertical, 6) { BorderWidth = 6 };

            // Row: Span + Bitrate + Format
            var topRow = new Box(Orientation.Horizontal, 8);

            // Span
            var spanFrame = new Frame("Span (h:mm:ss)");
            var spanBox = new Box(Orientation.Vertical, 4) { BorderWidth = 4 };
            _chkSpan = new CheckButton("Enable");
            _chkSpan.Toggled += (s, e) => { _txtSpanStart.Sensitive = _chkSpan.Active; _txtSpanEnd.Sensitive = _chkSpan.Active; };
            spanBox.PackStart(_chkSpan, false, false, 0);
            var spanGrid = new Grid { ColumnSpacing = 4, RowSpacing = 4 };
            spanGrid.Attach(new Label("Start:") { Halign = Align.End }, 0, 0, 1, 1);
            _txtSpanStart = new Entry { Text = "0:01:30", WidthChars = 8, Sensitive = false };
            spanGrid.Attach(_txtSpanStart, 1, 0, 1, 1);
            spanGrid.Attach(new Label("End:") { Halign = Align.End }, 0, 1, 1, 1);
            _txtSpanEnd = new Entry { Text = "0:05:00", WidthChars = 8, Sensitive = false };
            spanGrid.Attach(_txtSpanEnd, 1, 1, 1, 1);
            spanBox.PackStart(spanGrid, false, false, 0);
            spanFrame.Add(spanBox);
            topRow.PackStart(spanFrame, false, false, 0);

            // Bitrate
            var bitrateFrame = new Frame("Bitrate");
            var bitrateBox = new Box(Orientation.Horizontal, 4) { BorderWidth = 6 };
            _comboBitrate = new ComboBoxText();
            foreach (var b in new[] { "32", "40", "48", "56", "64", "80", "96", "112", "128", "144", "160", "192", "224", "256", "320" })
                _comboBitrate.AppendText(b);
            _comboBitrate.Active = 8; // 128
            bitrateBox.PackStart(_comboBitrate, false, false, 0);
            bitrateBox.PackStart(new Label("kb/s"), false, false, 0);
            bitrateFrame.Add(bitrateBox);
            topRow.PackStart(bitrateFrame, false, false, 0);

            // Format
            var formatFrame = new Frame("Format");
            var formatBox = new Box(Orientation.Vertical, 4) { BorderWidth = 6 };
            _radioSingle = new RadioButton("Extract entire audio track as single clip");
            _radioMultiple = new RadioButton(_radioSingle, "Break into clips of length (h:mm:ss):");
            _radioMultiple.Active = true;
            _txtClipLength = new Entry { Text = "0:05:00", WidthChars = 8 };
            _radioMultiple.Toggled += (s, e) => _txtClipLength.Sensitive = _radioMultiple.Active;
            formatBox.PackStart(_radioSingle, false, false, 0);
            var multiBox = new Box(Orientation.Horizontal, 4);
            multiBox.PackStart(_radioMultiple, false, false, 0);
            multiBox.PackStart(_txtClipLength, false, false, 0);
            formatBox.PackStart(multiBox, false, false, 0);
            formatFrame.Add(formatBox);
            topRow.PackStart(formatFrame, true, true, 0);

            optBox.PackStart(topRow, false, false, 0);

            // ── LYRICS ──────────────────────────────────────────────────────
            var lyricsFrame = new Frame("Lyrics");
            var lyricsOuterBox = new Box(Orientation.Vertical, 4) { BorderWidth = 4 };
            _chkLyrics = new CheckButton("Enable lyrics (add to ID3 tag)");
            _chkLyrics.Toggled += (s, e) => _lyricsContent.Sensitive = _chkLyrics.Active;
            lyricsOuterBox.PackStart(_chkLyrics, false, false, 0);

            _lyricsContent = new Box(Orientation.Vertical, 4) { Sensitive = false };

            var lyrGrid = new Grid { RowSpacing = 4, ColumnSpacing = 4 };
            int lr = 0;

            // Subs1
            lyrGrid.Attach(new Label("Subs1:") { Halign = Align.End }, 0, lr, 1, 1);
            _txtSubs1File = new Entry { Hexpand = true };
            lyrGrid.Attach(_txtSubs1File, 1, lr, 1, 1);
            var btnS1 = new Button("Browse...");
            btnS1.Clicked += (s, e) => { var f = SelectSubFile("Select Subtitle 1"); if (f != "") _txtSubs1File.Text = f; };
            lyrGrid.Attach(btnS1, 2, lr, 1, 1);
            lyrGrid.Attach(new Label("Encoding:") { Halign = Align.End }, 3, lr, 1, 1);
            _comboEncSubs1 = BuildEncodingCombo();
            lyrGrid.Attach(_comboEncSubs1, 4, lr, 1, 1);
            lr++;

            // Subs2
            lyrGrid.Attach(new Label("Subs2 (opt):") { Halign = Align.End }, 0, lr, 1, 1);
            _txtSubs2File = new Entry { Hexpand = true };
            lyrGrid.Attach(_txtSubs2File, 1, lr, 1, 1);
            var btnS2 = new Button("Browse...");
            btnS2.Clicked += (s, e) => { var f = SelectSubFile("Select Subtitle 2"); if (f != "") _txtSubs2File.Text = f; };
            lyrGrid.Attach(btnS2, 2, lr, 1, 1);
            lyrGrid.Attach(new Label("Encoding:") { Halign = Align.End }, 3, lr, 1, 1);
            _comboEncSubs2 = BuildEncodingCombo();
            lyrGrid.Attach(_comboEncSubs2, 4, lr, 1, 1);

            _lyricsContent.PackStart(lyrGrid, false, false, 0);

            // Timing + Time Shift + Remove options
            var lyricsOptRow = new Box(Orientation.Horizontal, 8);

            // Use Timings From
            var timingFrame = new Frame("Timings From");
            var timingBox = new Box(Orientation.Vertical, 2) { BorderWidth = 4 };
            _radioTimingSubs1 = new RadioButton("Subs1") { Active = true };
            _radioTimingSubs2 = new RadioButton(_radioTimingSubs1, "Subs2");
            timingBox.PackStart(_radioTimingSubs1, false, false, 0);
            timingBox.PackStart(_radioTimingSubs2, false, false, 0);
            timingFrame.Add(timingBox);
            lyricsOptRow.PackStart(timingFrame, false, false, 0);

            // Time Shift
            var tsFrame = new Frame("Time Shift");
            var tsBox = new Box(Orientation.Vertical, 2) { BorderWidth = 4 };
            _chkTimeShift = new CheckButton("Enable");
            _chkTimeShift.Toggled += (s, e) =>
            {
                _spinTimeShiftSubs1.Sensitive = _chkTimeShift.Active;
                _spinTimeShiftSubs2.Sensitive = _chkTimeShift.Active;
            };
            tsBox.PackStart(_chkTimeShift, false, false, 0);
            var tsGrid = new Grid { ColumnSpacing = 4, RowSpacing = 2 };
            tsGrid.Attach(new Label("S1:"), 0, 0, 1, 1);
            _spinTimeShiftSubs1 = new SpinButton(-99999, 99999, 10) { Value = 0, Sensitive = false };
            tsGrid.Attach(_spinTimeShiftSubs1, 1, 0, 1, 1);
            tsGrid.Attach(new Label("ms"), 2, 0, 1, 1);
            tsGrid.Attach(new Label("S2:"), 0, 1, 1, 1);
            _spinTimeShiftSubs2 = new SpinButton(-99999, 99999, 10) { Value = 0, Sensitive = false };
            tsGrid.Attach(_spinTimeShiftSubs2, 1, 1, 1, 1);
            tsGrid.Attach(new Label("ms"), 2, 1, 1, 1);
            tsBox.PackStart(tsGrid, false, false, 0);
            tsFrame.Add(tsBox);
            lyricsOptRow.PackStart(tsFrame, false, false, 0);

            // Remove w/o counterpart
            var remFrame = new Frame("Remove w/o Counterpart");
            var remBox = new Box(Orientation.Vertical, 2) { BorderWidth = 4 };
            _chkRemoveNoCounterS1 = new CheckButton("Subs1") { Active = true };
            _chkRemoveNoCounterS2 = new CheckButton("Subs2") { Active = true };
            remBox.PackStart(_chkRemoveNoCounterS1, false, false, 0);
            remBox.PackStart(_chkRemoveNoCounterS2, false, false, 0);
            remFrame.Add(remBox);
            lyricsOptRow.PackStart(remFrame, false, false, 0);

            // Remove styled lines
            var styledFrame = new Frame("Remove Styled Lines");
            var styledBox = new Box(Orientation.Vertical, 2) { BorderWidth = 4 };
            _chkRemoveStyledS1 = new CheckButton("Subs1") { Active = true };
            _chkRemoveStyledS2 = new CheckButton("Subs2") { Active = true };
            styledBox.PackStart(_chkRemoveStyledS1, false, false, 0);
            styledBox.PackStart(_chkRemoveStyledS2, false, false, 0);
            styledFrame.Add(styledBox);
            lyricsOptRow.PackStart(styledFrame, false, false, 0);

            _lyricsContent.PackStart(lyricsOptRow, false, false, 0);
            lyricsOuterBox.PackStart(_lyricsContent, false, false, 0);
            lyricsFrame.Add(lyricsOuterBox);
            optBox.PackStart(lyricsFrame, false, false, 0);

            optFrame.Add(optBox);
            vbox.PackStart(optFrame, false, false, 0);

            // ── NAMING ──────────────────────────────────────────────────────
            var nameFrame = new Frame("Naming");
            var nameGrid = new Grid { RowSpacing = 4, ColumnSpacing = 6, BorderWidth = 6 };
            nameGrid.Attach(new Label("Name:") { Halign = Align.End }, 0, 0, 1, 1);
            _txtName = new Entry { Hexpand = true };
            nameGrid.Attach(_txtName, 1, 0, 1, 1);
            nameGrid.Attach(new Label("First Episode:") { Halign = Align.End }, 2, 0, 1, 1);
            _spinEpisodeStart = new SpinButton(0, 999, 1) { Value = 1 };
            nameGrid.Attach(_spinEpisodeStart, 3, 0, 1, 1);
            nameFrame.Add(nameGrid);
            vbox.PackStart(nameFrame, false, false, 0);

            // ── PROGRESS ────────────────────────────────────────────────────
            _progressBar = new ProgressBar { ShowText = true, Text = "Ready" };
            vbox.PackStart(_progressBar, false, false, 0);

            // ── BUTTONS ─────────────────────────────────────────────────────
            var btnRow = new Box(Orientation.Horizontal, 6);

            var cancelBtn = new Button("Cancel");
            cancelBtn.Clicked += (s, e) => { _cancelRequested = true; Respond(ResponseType.Cancel); };
            btnRow.PackEnd(cancelBtn, false, false, 0);

            _btnExtract = new Button("Extract Audio") { WidthRequest = 130 };
            _btnExtract.Clicked += OnExtractClicked;
            btnRow.PackEnd(_btnExtract, false, false, 0);

            vbox.PackStart(btnRow, false, false, 4);

            ContentArea.PackStart(vbox, true, true, 0);
            ContentArea.ShowAll();
        }

        // ── INITIAL STATE ───────────────────────────────────────────────────

        private void LoadInitialState()
        {
            SaveSettings curSettings = new SaveSettings();
            curSettings.gatherData();
            oldSettings = ObjectCopier.Clone<SaveSettings>(curSettings); // save for restore on close

            _chkRemoveNoCounterS1.Active = Settings.Instance.Subs[0].RemoveNoCounterpart;
            _chkRemoveNoCounterS2.Active = Settings.Instance.Subs[1].RemoveNoCounterpart;
            _chkRemoveStyledS1.Active = Settings.Instance.Subs[0].RemoveStyledLines;
            _chkRemoveStyledS2.Active = Settings.Instance.Subs[1].RemoveStyledLines;

            _txtOutputDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            Destroyed += (s, e) => Settings.Instance.loadSettings(oldSettings);
        }

        // ── EVENTS ──────────────────────────────────────────────────────────

        private void OnMediaFileChanged(object sender, EventArgs e)
        {
            string filePattern = _txtMediaFile.Text.Trim();
            string[] files = UtilsCommon.getNonHiddenFiles(filePattern);

            _comboAudioStream.RemoveAll();

            if (files.Length > 0)
            {
                var streams = UtilsVideo.getAvailableAudioStreams(files[0]);
                if (streams.Count > 0)
                    foreach (var s in streams) _comboAudioStream.AppendText(s.ToString());
                else
                    _comboAudioStream.AppendText(new InfoStream().ToString());
                _comboAudioStream.Active = 0;
                _comboAudioStream.Sensitive = true;
            }
            else
            {
                _comboAudioStream.AppendText("0 - (Default)");
                _comboAudioStream.Active = 0;
                _comboAudioStream.Sensitive = false;
            }
        }

        private async void OnExtractClicked(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            UpdateSettings();

            Logger.Instance.info("Extract Audio From Media: GO!");

            _btnExtract.Sensitive = false;
            _cancelRequested = false;
            _progressBar.Text = "Starting...";
            _progressBar.Fraction = 0.0;

            DateTime startTime = DateTime.Now;

            try
            {
                bool lyricsEnabled = _chkLyrics.Active;
                string subs2Pattern = _txtSubs2File.Text.Trim();

                bool success = await Task.Run(() => SplitAudio(lyricsEnabled, subs2Pattern));

                if (_cancelRequested)
                {
                    _progressBar.Text = "Cancelled";
                    _progressBar.Fraction = 0.0;
                }
                else if (success)
                {
                    TimeSpan elapsed = DateTime.Now - startTime;
                    string msg = $"Audio extraction completed in {elapsed.TotalMinutes:0.00} minutes.";
                    _progressBar.Text = "Done!";
                    _progressBar.Fraction = 1.0;
                    UtilsMsg.showInfoMsg(msg);
                }
            }
            catch (Exception ex)
            {
                UtilsMsg.showErrMsg($"Error during extraction:\n{ex.Message}");
                _progressBar.Text = "Error";
            }

            _btnExtract.Sensitive = true;
        }

        // ── VALIDATION ──────────────────────────────────────────────────────

        private bool ValidateForm()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(_txtName.Text))
                errors.Add("Name is required.");

            string name = _txtName.Text.Trim();
            if (name.IndexOfAny(new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' }) >= 0)
                errors.Add("Name must not contain \\ / : * ? \" < > |");

            if (!Directory.Exists(_txtOutputDir.Text.Trim()))
                errors.Add("Output directory does not exist.");

            if (_chkLyrics.Active)
            {
                string s1 = _txtSubs1File.Text.Trim();
                if (UtilsSubs.getNumSubsFiles(s1) == 0)
                    errors.Add("Lyrics enabled but Subs1 file is invalid.");

                string s2 = _txtSubs2File.Text.Trim();
                if (_radioTimingSubs2.Active && string.IsNullOrEmpty(s2))
                    errors.Add("Subs2 timings selected but no Subs2 file provided.");

                if (s2.Length > 0 && UtilsSubs.getNumSubsFiles(s2) == 0)
                    errors.Add("Subs2 file is invalid.");
            }

            if (errors.Count > 0)
            {
                UtilsMsg.showErrMsg(string.Join("\n", errors));
                return false;
            }
            return true;
        }

        // ── UPDATE SETTINGS ─────────────────────────────────────────────────

        private void UpdateSettings()
        {
            mediaFiles = UtilsCommon.getNonHiddenFiles(_txtMediaFile.Text.Trim());

            // Parse audio stream from combo
            int streamIdx = _comboAudioStream.Active;
            audioStream = new InfoStream(streamIdx.ToString(), streamIdx.ToString(), "", "");

            outputDir = _txtOutputDir.Text.Trim();
            deckName = _txtName.Text.Trim().Replace(" ", "_");
            episodeStartNumber = (int)_spinEpisodeStart.Value;
            isSingleFile = _radioSingle.Active;

            if (_chkLyrics.Active)
            {
                Settings.Instance.Subs[0].FilePattern = _txtSubs1File.Text.Trim();
                Settings.Instance.Subs[0].TimingsEnabled = _radioTimingSubs1.Active;
                Settings.Instance.Subs[0].TimeShift = (int)_spinTimeShiftSubs1.Value;
                Settings.Instance.Subs[0].Files = UtilsSubs.getSubsFiles(Settings.Instance.Subs[0].FilePattern).ToArray();
                Settings.Instance.Subs[0].Encoding = InfoEncoding.longToShort(_comboEncSubs1.ActiveText ?? "Unicode (UTF-8)");
                Settings.Instance.Subs[0].RemoveNoCounterpart = _chkRemoveNoCounterS1.Active;
                Settings.Instance.Subs[0].RemoveStyledLines = _chkRemoveStyledS1.Active;

                Settings.Instance.Subs[1].FilePattern = _txtSubs2File.Text.Trim();
                Settings.Instance.Subs[1].TimingsEnabled = _radioTimingSubs2.Active;
                Settings.Instance.Subs[1].TimeShift = (int)_spinTimeShiftSubs2.Value;
                Settings.Instance.Subs[1].Encoding = InfoEncoding.longToShort(_comboEncSubs2.ActiveText ?? "Unicode (UTF-8)");
                Settings.Instance.Subs[1].RemoveNoCounterpart = _chkRemoveNoCounterS2.Active;
                Settings.Instance.Subs[1].RemoveStyledLines = _chkRemoveStyledS2.Active;

                if (Settings.Instance.Subs[1].FilePattern.Length > 0)
                    Settings.Instance.Subs[1].Files = UtilsSubs.getSubsFiles(Settings.Instance.Subs[1].FilePattern).ToArray();
                else
                    Settings.Instance.Subs[1].Files = new string[0];

                Settings.Instance.TimeShiftEnabled = _chkTimeShift.Active;
            }

            if (!isSingleFile)
                clipLength = UtilsSubs.stringToTime(_txtClipLength.Text.Trim());

            useSpan = _chkSpan.Active;
            if (useSpan)
            {
                spanStart = UtilsSubs.stringToTime(_txtSpanStart.Text.Trim());
                spanEnd = UtilsSubs.stringToTime(_txtSpanEnd.Text.Trim());
            }

            bitrate = int.TryParse(_comboBitrate.ActiveText, out int br) ? br : 128;
        }

        // ── AUDIO SPLITTING (runs on background thread) ─────────────────────

        private bool SplitAudio(bool lyricsEnabled, string subs2Pattern)
        {
            List<List<InfoCombined>> combinedAll = new List<List<InfoCombined>>();

            if (lyricsEnabled)
            {
                WorkerVars wv = new WorkerVars(null, outputDir, WorkerVars.SubsProcessingType.Normal);
                WorkerSubs subsWorker = new WorkerSubs();
                try
                {
                    combinedAll = subsWorker.combineAllSubs(wv, null);
                    if (combinedAll == null) return false;
                }
                catch (Exception ex)
                {
                    UtilsMsg.showErrMsg("Error combining subtitles:\n" + ex);
                    return false;
                }
            }

            int episode = 0;

            foreach (string file in mediaFiles)
            {
                episode++;
                if (_cancelRequested) return false;

                TimeSpan mediaStartTime = TimeSpan.Zero;
                TimeSpan mediaEndTime;

                try { mediaEndTime = UtilsVideo.getVideoLength(file); }
                catch (Exception ex)
                {
                    UtilsMsg.showErrMsg("Error determining media duration:\n" + ex);
                    return false;
                }

                if (useSpan)
                {
                    mediaStartTime = spanStart;
                    if (spanEnd < mediaEndTime) mediaEndTime = spanEnd;
                }

                UtilsName name = new UtilsName(deckName, mediaFiles.Length, 1, mediaEndTime, 0, 0);
                TimeSpan mediaDuration = UtilsSubs.getDurationTime(mediaStartTime, mediaEndTime);

                UpdateProgress(episode, mediaFiles.Length, "Processing audio...");

                string tempMp3 = IOPath.Combine(IOPath.GetTempPath(), ConstantSettings.TempAudioFilename);

                UtilsAudio.ripAudioFromVideo(file, audioStream.Num,
                    mediaStartTime, mediaEndTime, bitrate, tempMp3, null);

                if (_cancelRequested) { TryDelete(tempMp3); return false; }

                int numClips = 1;
                if (!isSingleFile)
                    numClips = (int)Math.Ceiling(mediaDuration.TotalMilliseconds / (clipLength.TotalSeconds * 1000.0));

                for (int clipIdx = 0; clipIdx < numClips; clipIdx++)
                {
                    if (_cancelRequested) { TryDelete(tempMp3); return false; }

                    UpdateProgress(episode, mediaFiles.Length,
                        $"Splitting {clipIdx + 1}/{numClips} from file {episode}/{mediaFiles.Length}");

                    TimeSpan startTime = TimeSpan.Zero;
                    TimeSpan endTime = TimeSpan.Zero;

                    if (isSingleFile)
                    {
                        endTime = mediaDuration;
                    }
                    else
                    {
                        startTime = startTime + TimeSpan.FromSeconds(clipLength.TotalSeconds * clipIdx);
                        endTime = endTime + TimeSpan.FromSeconds(clipLength.TotalSeconds * (clipIdx + 1));
                        if (endTime.TotalMilliseconds >= mediaDuration.TotalMilliseconds)
                            endTime = mediaDuration;
                    }

                    TimeSpan startTimeName = startTime + mediaStartTime;
                    TimeSpan endTimeName = endTime + mediaStartTime;

                    name.TotalNumLines = numClips;

                    string nameStr = name.createName(ConstantSettings.ExtractMediaAudioFilenameFormat,
                        episode + episodeStartNumber - 1, clipIdx + 1, startTimeName, endTimeName, "", "");

                    string outName = IOPath.Combine(outputDir, nameStr);

                    UtilsAudio.cutAudio(tempMp3, startTime, endTime, outName);

                    // ID3 Tags
                    string tagArtist = name.createName(ConstantSettings.AudioId3Artist, episode + episodeStartNumber - 1, clipIdx + 1, startTimeName, endTimeName, "", "");
                    string tagAlbum = name.createName(ConstantSettings.AudioId3Album, episode + episodeStartNumber - 1, clipIdx + 1, startTimeName, endTimeName, "", "");
                    string tagTitle = name.createName(ConstantSettings.AudioId3Title, episode + episodeStartNumber - 1, clipIdx + 1, startTimeName, endTimeName, "", "");
                    string tagGenre = name.createName(ConstantSettings.AudioId3Genre, episode + episodeStartNumber - 1, clipIdx + 1, startTimeName, endTimeName, "", "");

                    string tagLyrics = "";
                    if (lyricsEnabled && combinedAll.Count >= episode)
                    {
                        int curLyricsNum = 1;
                        foreach (InfoCombined comb in combinedAll[episode - 1])
                        {
                            if (comb.Subs1.StartTime.TotalMilliseconds >= startTimeName.TotalMilliseconds
                                && comb.Subs1.StartTime.TotalMilliseconds <= endTimeName.TotalMilliseconds)
                            {
                                tagLyrics += FormatLyricsPair(comb, name, startTimeName, 
                                    episode + episodeStartNumber - 1, curLyricsNum, subs2Pattern) + "\n";
                                curLyricsNum++;
                            }
                        }
                    }

                    UtilsAudio.tagAudio(outName, tagArtist, tagAlbum, tagTitle, tagGenre, tagLyrics, clipIdx + 1, numClips);
                }
            }

            return true;
        }

        private string FormatLyricsPair(InfoCombined comb, UtilsName name, 
            TimeSpan clipStartTime, int episode, int sequenceNum, string subs2Pattern)
        {
            string subs1Text = comb.Subs1.Text;
            string subs2Text = comb.Subs2.Text;

            TimeSpan lyricTime = TimeSpan.FromMilliseconds(comb.Subs1.StartTime.TotalMilliseconds - clipStartTime.TotalMilliseconds);

            string pair = name.createName(ConstantSettings.ExtractMediaLyricsSubs1Format,
                episode, sequenceNum, lyricTime, lyricTime, subs1Text, subs2Text);

            if (subs2Pattern.Length > 0 && ConstantSettings.ExtractMediaLyricsSubs2Format != "")
            {
                pair += "\n";
                pair += name.createName(ConstantSettings.ExtractMediaLyricsSubs2Format,
                    episode, sequenceNum, lyricTime, lyricTime, subs1Text, subs2Text);
            }

            return pair;
        }

        // ── HELPERS ─────────────────────────────────────────────────────────

        private void UpdateProgress(int episode, int total, string text)
        {
            double frac = Math.Max(0, Math.Min(1, (episode - 1.0) / total));
            Application.Invoke((s, e) =>
            {
                _progressBar.Text = text;
                _progressBar.Fraction = frac;
            });
        }

        private void TryDelete(string path)
        {
            try { File.Delete(path); } catch { }
        }

        private ComboBoxText BuildEncodingCombo()
        {
            var combo = new ComboBoxText();
            var encodings = InfoEncoding.getEncodings();
            int idx = 0, selIdx = 0;
            foreach (var enc in encodings)
            {
                combo.AppendText(enc.LongName);
                if (enc.ShortName == "utf-8") selIdx = idx;
                idx++;
            }
            combo.Active = selIdx;
            return combo;
        }

        private void SetEncodingCombo(ComboBoxText combo, string longName)
        {
            var encodings = InfoEncoding.getEncodings();
            int i = 0;
            foreach (var enc in encodings)
            {
                if (enc.LongName == longName) { combo.Active = i; return; }
                i++;
            }
        }

        private string SelectFile(string title)
        {
            var dlg = new FileChooserDialog(title, this, FileChooserAction.Open,
                "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
            if (lastDirPath != "" && Directory.Exists(lastDirPath))
                dlg.SetCurrentFolder(lastDirPath);
            string result = "";
            if (dlg.Run() == (int)ResponseType.Accept)
            {
                result = dlg.Filename;
                lastDirPath = IOPath.GetDirectoryName(result);
            }
            dlg.Destroy();
            return result;
        }

        private string SelectSubFile(string title)
        {
            var dlg = new FileChooserDialog(title, this, FileChooserAction.Open,
                "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
            var filter = new FileFilter { Name = "Subtitle Files" };
            filter.AddPattern("*.ass"); filter.AddPattern("*.ssa");
            filter.AddPattern("*.srt"); filter.AddPattern("*.lrc");
            filter.AddPattern("*.trs");
            dlg.AddFilter(filter);
            var allFilter = new FileFilter { Name = "All Files" };
            allFilter.AddPattern("*");
            dlg.AddFilter(allFilter);
            if (lastDirPath != "" && Directory.Exists(lastDirPath))
                dlg.SetCurrentFolder(lastDirPath);
            string result = "";
            if (dlg.Run() == (int)ResponseType.Accept)
            {
                result = dlg.Filename;
                lastDirPath = IOPath.GetDirectoryName(result);
            }
            dlg.Destroy();
            return result;
        }

        private string SelectFolder(string title)
        {
            var dlg = new FileChooserDialog(title, this, FileChooserAction.SelectFolder,
                "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
            if (lastDirPath != "" && Directory.Exists(lastDirPath))
                dlg.SetCurrentFolder(lastDirPath);
            string result = "";
            if (dlg.Run() == (int)ResponseType.Accept)
            {
                result = dlg.Filename;
                lastDirPath = result;
            }
            dlg.Destroy();
            return result;
        }
    }
}
