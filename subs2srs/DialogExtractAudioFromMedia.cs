//  Copyright (C) 2026 fkzys and contributors
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
using IOPath = System.IO.Path;

namespace subs2srs
{
    /// <summary>
    /// Extract Audio from Media tool dialog — GTK4/Gir.Core port.
    /// Replaces GTK3 Dialog with a modal Gtk.Window + nested GLib.MainLoop
    /// to preserve the synchronous Run() semantics expected by callers.
    /// </summary>
    public class DialogExtractAudioFromMedia : Gtk.Window
    {
        private Settings _snapshot;

        // Media
        private Gtk.Entry _txtMediaFile;
        private Gtk.DropDown _comboAudioStream;
        private Gtk.StringList _audioStreamModel;

        // Output
        private Gtk.Entry _txtOutputDir;

        // Span
        private Gtk.CheckButton _chkSpan;
        private Gtk.Entry _txtSpanStart, _txtSpanEnd;

        // Bitrate
        private Gtk.DropDown _comboBitrate;
        private Gtk.StringList _bitrateModel;

        // Format
        private Gtk.DropDown _comboFormat;
        private Gtk.StringList _formatModel;

        // Format
        private Gtk.CheckButton _radioSingle, _radioMultiple;
        private Gtk.Entry _txtClipLength;

        // Lyrics
        private Gtk.CheckButton _chkLyrics;
        private Gtk.Box _lyricsContent;
        private Gtk.Entry _txtSubs1File, _txtSubs2File;
        private Gtk.DropDown _comboEncSubs1, _comboEncSubs2;
        private Gtk.StringList _encModel1, _encModel2;
        private Gtk.CheckButton _radioTimingSubs1, _radioTimingSubs2;
        private Gtk.CheckButton _chkTimeShift;
        private Gtk.SpinButton _spinTimeShiftSubs1, _spinTimeShiftSubs2;
        private Gtk.CheckButton _chkRemoveNoCounterS1, _chkRemoveNoCounterS2;
        private Gtk.CheckButton _chkRemoveStyledS1, _chkRemoveStyledS2;

        // Naming
        private Gtk.Entry _txtName;
        private Gtk.SpinButton _spinEpisodeStart;

        // Progress
        private Gtk.ProgressBar _progressBar;
        private Gtk.Button _btnExtract;
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

        // Nested main loop for synchronous Run()
        private GLib.MainLoop _loop;
        private int _responseId;

        // Bitrate values for the dropdown
        private static readonly string[] BitrateValues =
            { "32", "40", "48", "56", "64", "80", "96", "112", "128", "144", "160", "192", "224", "256", "320" };

        public string MediaFilePattern { set => _txtMediaFile.SetText(value); }
        public string Subs1FilePattern { set => _txtSubs1File.SetText(value); }
        public string Subs2FilePattern { set => _txtSubs2File.SetText(value); }
        public string OutputDir { set => _txtOutputDir.SetText(value); }
        public string DeckName { set => _txtName.SetText(value); }
        public int EpisodeStartNumber { set => _spinEpisodeStart.Value = value; }

        public bool UseSubs1Timings
        {
            set
            {
                _radioTimingSubs1.SetActive(value);
                _radioTimingSubs2.SetActive(!value);
            }
        }

        public bool UseTimeShift { set => _chkTimeShift.SetActive(value); }
        public int TimeShiftSubs1 { set => _spinTimeShiftSubs1.Value = value; }
        public int TimeShiftSubs2 { set => _spinTimeShiftSubs2.Value = value; }

        public int Bitrate
        {
            set
            {
                string v = value.ToString();
                for (int i = 0; i < BitrateValues.Length; i++)
                {
                    if (BitrateValues[i] == v) { _comboBitrate.SetSelected((uint)i); return; }
                }
                _comboBitrate.SetSelected(8); // default: 128
            }
        }

        public bool SpanEnabled { set => _chkSpan.SetActive(value); }
        public string SpanStart { set => _txtSpanStart.SetText(value); }
        public string SpanEnd { set => _txtSpanEnd.SetText(value); }

        public string EncodingSubs1
        {
            set => SetEncodingCombo(_comboEncSubs1, _encModel1, value);
        }

        public string EncodingSubs2
        {
            set => SetEncodingCombo(_comboEncSubs2, _encModel2, value);
        }

        public int AudioStreamIndex
        {
            set { if (value >= 0) _comboAudioStream.SetSelected((uint)value); }
        }

        public string FileBrowserStartDir
        {
            get => Directory.Exists(lastDirPath) ? lastDirPath : "";
            set => lastDirPath = value;
        }

        public DialogExtractAudioFromMedia(Gtk.Window parent) : base()
        {
            SetTitle("Extract Audio from Media Tool");
            SetDefaultSize(700, 620);
            SetModal(true);
            if (parent != null)
                SetTransientFor(parent);

            BuildUI();
            LoadInitialState();
        }

        /// <summary>
        /// Show the dialog modally using a nested GLib main loop.
        /// Returns a response ID (0 = closed/cancelled).
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
            _cancelRequested = true;
            _responseId = 0;
            if (_loop != null && _loop.IsRunning())
                _loop.Quit();
            return false; // allow default close
        }

        private void BuildUI()
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            vbox.SetMarginTop(8);
            vbox.SetMarginBottom(8);
            vbox.SetMarginStart(8);
            vbox.SetMarginEnd(8);

            // Help text
            var helpLabel = Gtk.Label.New(
                "Use this tool to extract, convert and split the audio track from a media file.");
            helpLabel.SetHalign(Gtk.Align.Center);
            helpLabel.SetMarginBottom(6);
            vbox.Append(helpLabel);

            // ── MEDIA FILE ──────────────────────────────────────────────────
            var mediaGrid = Gtk.Grid.New();
            mediaGrid.SetRowSpacing(6);
            mediaGrid.SetColumnSpacing(6);
            int r = 0;

            var lblMedia = Gtk.Label.New("Media file:");
            lblMedia.SetHalign(Gtk.Align.End);
            mediaGrid.Attach(lblMedia, 0, r, 1, 1);
            _txtMediaFile = Gtk.Entry.New();
            _txtMediaFile.SetHexpand(true);
            _txtMediaFile.OnChanged += OnMediaFileChanged;
            mediaGrid.Attach(_txtMediaFile, 1, r, 1, 1);
            var btnMedia = Gtk.Button.NewWithLabel("Browse...");
            btnMedia.OnClicked += (s, e) =>
                SelectFileAsync("Select Media File", f => _txtMediaFile.SetText(f));
            mediaGrid.Attach(btnMedia, 2, r, 1, 1);
            r++;

            var lblStream = Gtk.Label.New("Audio Stream:");
            lblStream.SetHalign(Gtk.Align.End);
            mediaGrid.Attach(lblStream, 0, r, 1, 1);
            _audioStreamModel = Gtk.StringList.New(new[] { "0 - (Default)" });
            _comboAudioStream = Gtk.DropDown.New(_audioStreamModel, null);
            _comboAudioStream.SetSelected(0);
            mediaGrid.Attach(_comboAudioStream, 1, r, 2, 1);
            r++;

            // ── OUTPUT DIR ──────────────────────────────────────────────────
            var lblOut = Gtk.Label.New("Output Dir:");
            lblOut.SetHalign(Gtk.Align.End);
            mediaGrid.Attach(lblOut, 0, r, 1, 1);
            _txtOutputDir = Gtk.Entry.New();
            _txtOutputDir.SetHexpand(true);
            mediaGrid.Attach(_txtOutputDir, 1, r, 1, 1);
            var btnOut = Gtk.Button.NewWithLabel("Browse...");
            btnOut.OnClicked += (s, e) =>
                SelectFolderAsync("Select Output Directory", f => _txtOutputDir.SetText(f));
            mediaGrid.Attach(btnOut, 2, r, 1, 1);

            vbox.Append(mediaGrid);
            vbox.Append(Gtk.Separator.New(Gtk.Orientation.Horizontal));

            // ── OPTIONS ─────────────────────────────────────────────────────
            var optFrame = Gtk.Frame.New("Options");
            var optBox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            optBox.SetMarginTop(6);
            optBox.SetMarginBottom(6);
            optBox.SetMarginStart(6);
            optBox.SetMarginEnd(6);

            var topRow = Gtk.Box.New(Gtk.Orientation.Horizontal, 8);

            // Span
            var spanFrame = Gtk.Frame.New("Span (h:mm:ss)");
            var spanBox = Gtk.Box.New(Gtk.Orientation.Vertical, 4);
            spanBox.SetMarginTop(4);
            spanBox.SetMarginBottom(4);
            spanBox.SetMarginStart(4);
            spanBox.SetMarginEnd(4);
            _chkSpan = Gtk.CheckButton.NewWithLabel("Enable");
            _chkSpan.OnToggled += (s, e) =>
            {
                _txtSpanStart.SetSensitive(_chkSpan.GetActive());
                _txtSpanEnd.SetSensitive(_chkSpan.GetActive());
            };
            spanBox.Append(_chkSpan);
            var spanGrid = Gtk.Grid.New();
            spanGrid.SetColumnSpacing(4);
            spanGrid.SetRowSpacing(4);
            spanGrid.Attach(Gtk.Label.New("Start:"), 0, 0, 1, 1);
            _txtSpanStart = Gtk.Entry.New();
            _txtSpanStart.SetText("0:01:30");
            _txtSpanStart.SetWidthChars(8);
            _txtSpanStart.SetSensitive(false);
            spanGrid.Attach(_txtSpanStart, 1, 0, 1, 1);
            spanGrid.Attach(Gtk.Label.New("End:"), 0, 1, 1, 1);
            _txtSpanEnd = Gtk.Entry.New();
            _txtSpanEnd.SetText("0:05:00");
            _txtSpanEnd.SetWidthChars(8);
            _txtSpanEnd.SetSensitive(false);
            spanGrid.Attach(_txtSpanEnd, 1, 1, 1, 1);
            spanBox.Append(spanGrid);
            spanFrame.SetChild(spanBox);
            topRow.Append(spanFrame);

            // Bitrate
            var bitrateFrame = Gtk.Frame.New("Bitrate");
            var bitrateBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
            bitrateBox.SetMarginTop(6);
            bitrateBox.SetMarginBottom(6);
            bitrateBox.SetMarginStart(6);
            bitrateBox.SetMarginEnd(6);
            _bitrateModel = Gtk.StringList.New(BitrateValues);
            _comboBitrate = Gtk.DropDown.New(_bitrateModel, null);
            _comboBitrate.SetSelected(8); // 128
            bitrateBox.Append(_comboBitrate);
            bitrateBox.Append(Gtk.Label.New("kb/s"));
            bitrateBox.Append(Gtk.Label.New("format:"));
            _formatModel = Gtk.StringList.New(PrefDefaults.AudioFormats);
            _comboFormat = Gtk.DropDown.New(_formatModel, null);
            _comboFormat.SetSelected(0); // Opus
            bitrateBox.Append(_comboFormat);
            bitrateFrame.SetChild(bitrateBox);
            topRow.Append(bitrateFrame);

            // Format — use CheckButton with SetGroup for radio behavior
            var formatFrame = Gtk.Frame.New("Format");
            var formatBox = Gtk.Box.New(Gtk.Orientation.Vertical, 4);
            formatBox.SetMarginTop(6);
            formatBox.SetMarginBottom(6);
            formatBox.SetMarginStart(6);
            formatBox.SetMarginEnd(6);
            _radioSingle = Gtk.CheckButton.NewWithLabel("Extract entire audio track as single clip");
            _radioMultiple = Gtk.CheckButton.NewWithLabel("Break into clips of length (h:mm:ss):");
            _radioMultiple.SetGroup(_radioSingle);
            _radioMultiple.SetActive(true);
            _txtClipLength = Gtk.Entry.New();
            _txtClipLength.SetText("0:05:00");
            _txtClipLength.SetWidthChars(8);
            _radioMultiple.OnToggled += (s, e) =>
                _txtClipLength.SetSensitive(_radioMultiple.GetActive());
            formatBox.Append(_radioSingle);
            var multiBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
            multiBox.Append(_radioMultiple);
            multiBox.Append(_txtClipLength);
            formatBox.Append(multiBox);
            formatFrame.SetChild(formatBox);
            // Let format frame expand horizontally
            formatFrame.SetHexpand(true);
            topRow.Append(formatFrame);

            optBox.Append(topRow);

            // ── LYRICS ──────────────────────────────────────────────────────
            var lyricsFrame = Gtk.Frame.New("Lyrics");
            var lyricsOuterBox = Gtk.Box.New(Gtk.Orientation.Vertical, 4);
            lyricsOuterBox.SetMarginTop(4);
            lyricsOuterBox.SetMarginBottom(4);
            lyricsOuterBox.SetMarginStart(4);
            lyricsOuterBox.SetMarginEnd(4);
            _chkLyrics = Gtk.CheckButton.NewWithLabel("Enable lyrics (add to ID3 tag)");
            _chkLyrics.OnToggled += (s, e) =>
                _lyricsContent.SetSensitive(_chkLyrics.GetActive());
            lyricsOuterBox.Append(_chkLyrics);

            _lyricsContent = Gtk.Box.New(Gtk.Orientation.Vertical, 4);
            _lyricsContent.SetSensitive(false);

            var lyrGrid = Gtk.Grid.New();
            lyrGrid.SetRowSpacing(4);
            lyrGrid.SetColumnSpacing(4);
            int lr = 0;

            // Subs1
            var lblS1 = Gtk.Label.New("Subs1:");
            lblS1.SetHalign(Gtk.Align.End);
            lyrGrid.Attach(lblS1, 0, lr, 1, 1);
            _txtSubs1File = Gtk.Entry.New();
            _txtSubs1File.SetHexpand(true);
            lyrGrid.Attach(_txtSubs1File, 1, lr, 1, 1);
            var btnS1 = Gtk.Button.NewWithLabel("Browse...");
            btnS1.OnClicked += (s, e) =>
                SelectSubFileAsync("Select Subtitle 1", f => _txtSubs1File.SetText(f));
            lyrGrid.Attach(btnS1, 2, lr, 1, 1);
            var lblEnc1 = Gtk.Label.New("Encoding:");
            lblEnc1.SetHalign(Gtk.Align.End);
            lyrGrid.Attach(lblEnc1, 3, lr, 1, 1);
            (_comboEncSubs1, _encModel1) = BuildEncodingDropDown();
            lyrGrid.Attach(_comboEncSubs1, 4, lr, 1, 1);
            lr++;

            // Subs2
            var lblS2 = Gtk.Label.New("Subs2 (opt):");
            lblS2.SetHalign(Gtk.Align.End);
            lyrGrid.Attach(lblS2, 0, lr, 1, 1);
            _txtSubs2File = Gtk.Entry.New();
            _txtSubs2File.SetHexpand(true);
            lyrGrid.Attach(_txtSubs2File, 1, lr, 1, 1);
            var btnS2 = Gtk.Button.NewWithLabel("Browse...");
            btnS2.OnClicked += (s, e) =>
                SelectSubFileAsync("Select Subtitle 2", f => _txtSubs2File.SetText(f));
            lyrGrid.Attach(btnS2, 2, lr, 1, 1);
            var lblEnc2 = Gtk.Label.New("Encoding:");
            lblEnc2.SetHalign(Gtk.Align.End);
            lyrGrid.Attach(lblEnc2, 3, lr, 1, 1);
            (_comboEncSubs2, _encModel2) = BuildEncodingDropDown();
            lyrGrid.Attach(_comboEncSubs2, 4, lr, 1, 1);

            _lyricsContent.Append(lyrGrid);

            // Timing + Time Shift + Remove options
            var lyricsOptRow = Gtk.Box.New(Gtk.Orientation.Horizontal, 8);

            // Use Timings From — radio group via SetGroup
            var timingFrame = Gtk.Frame.New("Timings From");
            var timingBox = Gtk.Box.New(Gtk.Orientation.Vertical, 2);
            timingBox.SetMarginTop(4);
            timingBox.SetMarginBottom(4);
            timingBox.SetMarginStart(4);
            timingBox.SetMarginEnd(4);
            _radioTimingSubs1 = Gtk.CheckButton.NewWithLabel("Subs1");
            _radioTimingSubs1.SetActive(true);
            _radioTimingSubs2 = Gtk.CheckButton.NewWithLabel("Subs2");
            _radioTimingSubs2.SetGroup(_radioTimingSubs1);
            timingBox.Append(_radioTimingSubs1);
            timingBox.Append(_radioTimingSubs2);
            timingFrame.SetChild(timingBox);
            lyricsOptRow.Append(timingFrame);

            // Time Shift
            var tsFrame = Gtk.Frame.New("Time Shift");
            var tsBox = Gtk.Box.New(Gtk.Orientation.Vertical, 2);
            tsBox.SetMarginTop(4);
            tsBox.SetMarginBottom(4);
            tsBox.SetMarginStart(4);
            tsBox.SetMarginEnd(4);
            _chkTimeShift = Gtk.CheckButton.NewWithLabel("Enable");
            _chkTimeShift.OnToggled += (s, e) =>
            {
                _spinTimeShiftSubs1.SetSensitive(_chkTimeShift.GetActive());
                _spinTimeShiftSubs2.SetSensitive(_chkTimeShift.GetActive());
            };
            tsBox.Append(_chkTimeShift);
            var tsGrid = Gtk.Grid.New();
            tsGrid.SetColumnSpacing(4);
            tsGrid.SetRowSpacing(2);
            tsGrid.Attach(Gtk.Label.New("S1:"), 0, 0, 1, 1);
            _spinTimeShiftSubs1 = Gtk.SpinButton.NewWithRange(-99999, 99999, 10);
            _spinTimeShiftSubs1.Value = 0;
            _spinTimeShiftSubs1.SetSensitive(false);
            tsGrid.Attach(_spinTimeShiftSubs1, 1, 0, 1, 1);
            tsGrid.Attach(Gtk.Label.New("ms"), 2, 0, 1, 1);
            tsGrid.Attach(Gtk.Label.New("S2:"), 0, 1, 1, 1);
            _spinTimeShiftSubs2 = Gtk.SpinButton.NewWithRange(-99999, 99999, 10);
            _spinTimeShiftSubs2.Value = 0;
            _spinTimeShiftSubs2.SetSensitive(false);
            tsGrid.Attach(_spinTimeShiftSubs2, 1, 1, 1, 1);
            tsGrid.Attach(Gtk.Label.New("ms"), 2, 1, 1, 1);
            tsBox.Append(tsGrid);
            tsFrame.SetChild(tsBox);
            lyricsOptRow.Append(tsFrame);

            // Remove w/o counterpart
            var remFrame = Gtk.Frame.New("Remove w/o Counterpart");
            var remBox = Gtk.Box.New(Gtk.Orientation.Vertical, 2);
            remBox.SetMarginTop(4);
            remBox.SetMarginBottom(4);
            remBox.SetMarginStart(4);
            remBox.SetMarginEnd(4);
            _chkRemoveNoCounterS1 = Gtk.CheckButton.NewWithLabel("Subs1");
            _chkRemoveNoCounterS1.SetActive(true);
            _chkRemoveNoCounterS2 = Gtk.CheckButton.NewWithLabel("Subs2");
            _chkRemoveNoCounterS2.SetActive(true);
            remBox.Append(_chkRemoveNoCounterS1);
            remBox.Append(_chkRemoveNoCounterS2);
            remFrame.SetChild(remBox);
            lyricsOptRow.Append(remFrame);

            // Remove styled lines
            var styledFrame = Gtk.Frame.New("Remove Styled Lines");
            var styledBox = Gtk.Box.New(Gtk.Orientation.Vertical, 2);
            styledBox.SetMarginTop(4);
            styledBox.SetMarginBottom(4);
            styledBox.SetMarginStart(4);
            styledBox.SetMarginEnd(4);
            _chkRemoveStyledS1 = Gtk.CheckButton.NewWithLabel("Subs1");
            _chkRemoveStyledS1.SetActive(true);
            _chkRemoveStyledS2 = Gtk.CheckButton.NewWithLabel("Subs2");
            _chkRemoveStyledS2.SetActive(true);
            styledBox.Append(_chkRemoveStyledS1);
            styledBox.Append(_chkRemoveStyledS2);
            styledFrame.SetChild(styledBox);
            lyricsOptRow.Append(styledFrame);

            _lyricsContent.Append(lyricsOptRow);
            lyricsOuterBox.Append(_lyricsContent);
            lyricsFrame.SetChild(lyricsOuterBox);
            optBox.Append(lyricsFrame);

            optFrame.SetChild(optBox);
            vbox.Append(optFrame);

            // ── NAMING ──────────────────────────────────────────────────────
            var nameFrame = Gtk.Frame.New("Naming");
            var nameGrid = Gtk.Grid.New();
            nameGrid.SetRowSpacing(4);
            nameGrid.SetColumnSpacing(6);
            nameGrid.SetMarginTop(6);
            nameGrid.SetMarginBottom(6);
            nameGrid.SetMarginStart(6);
            nameGrid.SetMarginEnd(6);
            var lblName = Gtk.Label.New("Name:");
            lblName.SetHalign(Gtk.Align.End);
            nameGrid.Attach(lblName, 0, 0, 1, 1);
            _txtName = Gtk.Entry.New();
            _txtName.SetHexpand(true);
            nameGrid.Attach(_txtName, 1, 0, 1, 1);
            var lblFirstEp = Gtk.Label.New("First Episode:");
            lblFirstEp.SetHalign(Gtk.Align.End);
            nameGrid.Attach(lblFirstEp, 2, 0, 1, 1);
            _spinEpisodeStart = Gtk.SpinButton.NewWithRange(0, 999, 1);
            _spinEpisodeStart.Value = 1;
            nameGrid.Attach(_spinEpisodeStart, 3, 0, 1, 1);
            nameFrame.SetChild(nameGrid);
            vbox.Append(nameFrame);

            // ── PROGRESS ────────────────────────────────────────────────────
            _progressBar = Gtk.ProgressBar.New();
            _progressBar.SetShowText(true);
            _progressBar.SetText("Ready");
            vbox.Append(_progressBar);

            // ── BUTTONS ─────────────────────────────────────────────────────
            var btnRow = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
            btnRow.SetHalign(Gtk.Align.End);

            _btnExtract = Gtk.Button.NewWithLabel("Extract Audio");
            _btnExtract.SetSizeRequest(130, -1);
            _btnExtract.OnClicked += OnExtractClicked;
            btnRow.Append(_btnExtract);

            var cancelBtn = Gtk.Button.NewWithLabel("Cancel");
            cancelBtn.OnClicked += (s, e) =>
            {
                _cancelRequested = true;
                Close();
            };
            btnRow.Append(cancelBtn);

            vbox.Append(btnRow);

            SetChild(vbox);
        }

        // ── INITIAL STATE ───────────────────────────────────────────────────

        private void LoadInitialState()
        {
            // Snapshot global settings for restore on close
            _snapshot = Settings.Instance.Snapshot();

            _chkRemoveNoCounterS1.SetActive(Settings.Instance.Subs[0].RemoveNoCounterpart);
            _chkRemoveNoCounterS2.SetActive(Settings.Instance.Subs[1].RemoveNoCounterpart);
            _chkRemoveStyledS1.SetActive(Settings.Instance.Subs[0].RemoveStyledLines);
            _chkRemoveStyledS2.SetActive(Settings.Instance.Subs[1].RemoveStyledLines);

            _txtOutputDir.SetText(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            // Restore settings snapshot when window is closed
            OnCloseRequest += (s, e) =>
            {
                Settings.Instance.RestoreFrom(_snapshot);
                return false;
            };
        }

        // ── EVENTS ──────────────────────────────────────────────────────────

        private void OnMediaFileChanged(Gtk.Editable sender, EventArgs e)
        {
            string filePattern = _txtMediaFile.GetText().Trim();
            string[] files = UtilsCommon.getNonHiddenFiles(filePattern);

            // Rebuild audio stream model
            var items = new List<string>();

            if (files.Length > 0)
            {
                var streams = UtilsVideo.getAvailableAudioStreams(files[0]);
                if (streams.Count > 0)
                    foreach (var s in streams) items.Add(s.ToString());
                else
                    items.Add(new InfoStream().ToString());
            }
            else
            {
                items.Add("0 - (Default)");
            }

            _audioStreamModel = Gtk.StringList.New(items.ToArray());
            _comboAudioStream.SetModel(_audioStreamModel);
            _comboAudioStream.SetSelected(0);
            _comboAudioStream.SetSensitive(files.Length > 0);
        }

        private async void OnExtractClicked(Gtk.Button sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            UpdateSettings();

            Logger.Instance.info("Extract Audio From Media: GO!");

            _btnExtract.SetSensitive(false);
            _cancelRequested = false;
            _progressBar.SetText("Starting...");
            _progressBar.SetFraction(0.0);

            DateTime startTime = DateTime.Now;

            try
            {
                bool lyricsEnabled = _chkLyrics.GetActive();
                string subs2Pattern = _txtSubs2File.GetText().Trim();

                bool success = await Task.Run(() => SplitAudio(lyricsEnabled, subs2Pattern));

                if (_cancelRequested)
                {
                    _progressBar.SetText("Cancelled");
                    _progressBar.SetFraction(0.0);
                }
                else if (success)
                {
                    TimeSpan elapsed = DateTime.Now - startTime;
                    string msg = $"Audio extraction completed in {elapsed.TotalMinutes:0.00} minutes.";
                    _progressBar.SetText("Done!");
                    _progressBar.SetFraction(1.0);
                    UtilsMsg.showInfoMsg(msg);
                }
            }
            catch (Exception ex)
            {
                UtilsMsg.showErrMsg($"Error during extraction:\n{ex.Message}");
                _progressBar.SetText("Error");
            }

            _btnExtract.SetSensitive(true);
        }

        // ── VALIDATION ──────────────────────────────────────────────────────

        private bool ValidateForm()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(_txtName.GetText()))
                errors.Add("Name is required.");

            string name = _txtName.GetText().Trim();
            if (name.IndexOfAny(new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' }) >= 0)
                errors.Add("Name must not contain \\ / : * ? \" < > |");

            if (!Directory.Exists(_txtOutputDir.GetText().Trim()))
                errors.Add("Output directory does not exist.");

            if (_chkLyrics.GetActive())
            {
                string s1 = _txtSubs1File.GetText().Trim();
                if (UtilsSubs.getNumSubsFiles(s1) == 0)
                    errors.Add("Lyrics enabled but Subs1 file is invalid.");

                string s2 = _txtSubs2File.GetText().Trim();
                if (_radioTimingSubs2.GetActive() && string.IsNullOrEmpty(s2))
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
            mediaFiles = UtilsCommon.getNonHiddenFiles(_txtMediaFile.GetText().Trim());

            // Parse audio stream from dropdown
            int streamIdx = (int)_comboAudioStream.GetSelected();
            audioStream = new InfoStream(streamIdx.ToString(), streamIdx.ToString(), "", "");

            outputDir = _txtOutputDir.GetText().Trim();
            deckName = _txtName.GetText().Trim().Replace(" ", "_");
            episodeStartNumber = (int)_spinEpisodeStart.Value;
            isSingleFile = _radioSingle.GetActive();

            if (_chkLyrics.GetActive())
            {
                Settings.Instance.Subs[0].FilePattern = _txtSubs1File.GetText().Trim();
                Settings.Instance.Subs[0].TimingsEnabled = _radioTimingSubs1.GetActive();
                Settings.Instance.Subs[0].TimeShift = (int)_spinTimeShiftSubs1.Value;
                Settings.Instance.Subs[0].Files = UtilsSubs.getSubsFiles(
                    Settings.Instance.Subs[0].FilePattern).ToArray();
                Settings.Instance.Subs[0].Encoding = GetEncodingShortName(_comboEncSubs1, _encModel1);
                Settings.Instance.Subs[0].RemoveNoCounterpart = _chkRemoveNoCounterS1.GetActive();
                Settings.Instance.Subs[0].RemoveStyledLines = _chkRemoveStyledS1.GetActive();

                Settings.Instance.Subs[1].FilePattern = _txtSubs2File.GetText().Trim();
                Settings.Instance.Subs[1].TimingsEnabled = _radioTimingSubs2.GetActive();
                Settings.Instance.Subs[1].TimeShift = (int)_spinTimeShiftSubs2.Value;
                Settings.Instance.Subs[1].Encoding = GetEncodingShortName(_comboEncSubs2, _encModel2);
                Settings.Instance.Subs[1].RemoveNoCounterpart = _chkRemoveNoCounterS2.GetActive();
                Settings.Instance.Subs[1].RemoveStyledLines = _chkRemoveStyledS2.GetActive();

                if (Settings.Instance.Subs[1].FilePattern.Length > 0)
                    Settings.Instance.Subs[1].Files = UtilsSubs.getSubsFiles(
                        Settings.Instance.Subs[1].FilePattern).ToArray();
                else
                    Settings.Instance.Subs[1].Files = Array.Empty<string>();

                Settings.Instance.TimeShiftEnabled = _chkTimeShift.GetActive();
            }

            if (!isSingleFile)
                clipLength = UtilsSubs.stringToTime(_txtClipLength.GetText().Trim());

            useSpan = _chkSpan.GetActive();
            if (useSpan)
            {
                spanStart = UtilsSubs.stringToTime(_txtSpanStart.GetText().Trim());
                spanEnd = UtilsSubs.stringToTime(_txtSpanEnd.GetText().Trim());
            }

            // Get bitrate from dropdown
            uint brIdx = _comboBitrate.GetSelected();
            bitrate = (brIdx < BitrateValues.Length && int.TryParse(BitrateValues[brIdx], out int br))
                ? br : 128;
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

                string tempMp3 = IOPath.Combine(IOPath.GetTempPath(),
                    ConstantSettings.TempAudioFilename);

                var audioFormat = PrefDefaults.AudioFormats[_comboFormat.GetSelected()];
                var audioCodec = audioFormat.ToUpper() switch
                {
                    "OPUS" => UtilsVideo.AudioCodec.Opus,
                    "MP3" => UtilsVideo.AudioCodec.MP3,
                    _ => UtilsVideo.AudioCodec.MP3
                };

                UtilsAudio.ripAudioFromVideo(file, audioStream.Num,
                    mediaStartTime, mediaEndTime, bitrate, tempMp3, null, audioCodec);

                if (_cancelRequested) { TryDelete(tempMp3); return false; }

                int numClips = 1;
                if (!isSingleFile)
                    numClips = (int)Math.Ceiling(
                        mediaDuration.TotalMilliseconds / (clipLength.TotalSeconds * 1000.0));

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
                        startTime = startTime + TimeSpan.FromSeconds(
                            clipLength.TotalSeconds * clipIdx);
                        endTime = endTime + TimeSpan.FromSeconds(
                            clipLength.TotalSeconds * (clipIdx + 1));
                        if (endTime.TotalMilliseconds >= mediaDuration.TotalMilliseconds)
                            endTime = mediaDuration;
                    }

                    TimeSpan startTimeName = startTime + mediaStartTime;
                    TimeSpan endTimeName = endTime + mediaStartTime;

                    name.TotalNumLines = numClips;

                    string nameStr = name.createName(
                        ConstantSettings.ExtractMediaAudioFilenameFormatWithExt,
                        episode + episodeStartNumber - 1, clipIdx + 1,
                        startTimeName, endTimeName, "", "");

                    string outName = IOPath.Combine(outputDir, nameStr);

                    UtilsAudio.cutAudio(tempMp3, startTime, endTime, outName);

                    // ID3 Tags
                    string tagArtist = name.createName(ConstantSettings.AudioId3Artist,
                        episode + episodeStartNumber - 1, clipIdx + 1,
                        startTimeName, endTimeName, "", "");
                    string tagAlbum = name.createName(ConstantSettings.AudioId3Album,
                        episode + episodeStartNumber - 1, clipIdx + 1,
                        startTimeName, endTimeName, "", "");
                    string tagTitle = name.createName(ConstantSettings.AudioId3Title,
                        episode + episodeStartNumber - 1, clipIdx + 1,
                        startTimeName, endTimeName, "", "");
                    string tagGenre = name.createName(ConstantSettings.AudioId3Genre,
                        episode + episodeStartNumber - 1, clipIdx + 1,
                        startTimeName, endTimeName, "", "");

                    string tagLyrics = "";
                    if (lyricsEnabled && combinedAll.Count >= episode)
                    {
                        int curLyricsNum = 1;
                        foreach (InfoCombined comb in combinedAll[episode - 1])
                        {
                            if (comb.Subs1.StartTime.TotalMilliseconds >=
                                    startTimeName.TotalMilliseconds
                                && comb.Subs1.StartTime.TotalMilliseconds <=
                                    endTimeName.TotalMilliseconds)
                            {
                                tagLyrics += FormatLyricsPair(comb, name, startTimeName,
                                    episode + episodeStartNumber - 1, curLyricsNum,
                                    subs2Pattern) + "\n";
                                curLyricsNum++;
                            }
                        }
                    }

                    UtilsAudio.tagAudio(outName, tagArtist, tagAlbum, tagTitle,
                        tagGenre, tagLyrics, clipIdx + 1, numClips);
                }
            }

            return true;
        }

        private string FormatLyricsPair(InfoCombined comb, UtilsName name,
            TimeSpan clipStartTime, int episode, int sequenceNum, string subs2Pattern)
        {
            string subs1Text = comb.Subs1.Text;
            string subs2Text = comb.Subs2.Text;

            TimeSpan lyricTime = TimeSpan.FromMilliseconds(
                comb.Subs1.StartTime.TotalMilliseconds - clipStartTime.TotalMilliseconds);

            string pair = name.createName(ConstantSettings.ExtractMediaLyricsSubs1Format,
                episode, sequenceNum, lyricTime, lyricTime, subs1Text, subs2Text);

            if (subs2Pattern.Length > 0
                && ConstantSettings.ExtractMediaLyricsSubs2Format != "")
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
            GLib.Functions.IdleAdd(0, () =>
            {
                _progressBar.SetText(text);
                _progressBar.SetFraction(frac);
                return false;
            });
        }

        private void TryDelete(string path)
        {
            try { File.Delete(path); } catch { }
        }

        /// <summary>
        /// Build an encoding DropDown backed by a StringList.
        /// Returns both so callers can read selections by index.
        /// </summary>
        private (Gtk.DropDown, Gtk.StringList) BuildEncodingDropDown()
        {
            var encodings = InfoEncoding.getEncodings();
            var names = new List<string>();
            int selIdx = 0, idx = 0;
            foreach (var enc in encodings)
            {
                names.Add(enc.LongName);
                if (enc.ShortName == "utf-8") selIdx = idx;
                idx++;
            }
            var model = Gtk.StringList.New(names.ToArray());
            var dd = Gtk.DropDown.New(model, null);
            dd.SetSelected((uint)selIdx);
            return (dd, model);
        }

        private void SetEncodingCombo(Gtk.DropDown dd, Gtk.StringList model, string longName)
        {
            for (uint i = 0; i < model.GetNItems(); i++)
            {
                if (model.GetString(i) == longName) { dd.SetSelected(i); return; }
            }
        }

        /// <summary>
        /// Get the short encoding name for the currently selected item.
        /// </summary>
        private string GetEncodingShortName(Gtk.DropDown dd, Gtk.StringList model)
        {
            uint sel = dd.GetSelected();
            if (sel < model.GetNItems())
            {
                string longName = model.GetString(sel);
                return InfoEncoding.longToShort(longName ?? "Unicode (UTF-8)");
            }
            return "utf-8";
        }

       // ── FILE DIALOGS (GTK4 async FileDialog) ────────────────────────────

        private async void SelectFileAsync(string title, System.Action<string> onSelected)
        {
            var dlg = Gtk.FileDialog.New();
            dlg.SetTitle(title);

            if (lastDirPath != "" && System.IO.Directory.Exists(lastDirPath))
                dlg.SetInitialFolder(Gio.FileHelper.NewForPath(lastDirPath));

            try
            {
                var file = await dlg.OpenAsync(this);
                if (file != null)
                {
                    string path = file.GetPath() ?? "";
                    if (path != "")
                    {
                        lastDirPath = IOPath.GetDirectoryName(path) ?? "";
                        onSelected(path);
                    }
                }
            }
            catch { /* user cancelled */ }
        }

        private async void SelectSubFileAsync(string title, System.Action<string> onSelected)
        {
            var dlg = Gtk.FileDialog.New();
            dlg.SetTitle(title);

            var filter = Gtk.FileFilter.New();
            filter.SetName("Subtitle Files");
            filter.AddPattern("*.ass"); filter.AddPattern("*.ssa");
            filter.AddPattern("*.srt"); filter.AddPattern("*.lrc");
            filter.AddPattern("*.trs");

            var allFilter = Gtk.FileFilter.New();
            allFilter.SetName("All Files");
            allFilter.AddPattern("*");

            var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
            filters.Append(filter);
            filters.Append(allFilter);
            dlg.SetFilters(filters);

            if (lastDirPath != "" && System.IO.Directory.Exists(lastDirPath))
                dlg.SetInitialFolder(Gio.FileHelper.NewForPath(lastDirPath));

            try
            {
                var file = await dlg.OpenAsync(this);
                if (file != null)
                {
                    string path = file.GetPath() ?? "";
                    if (path != "")
                    {
                        lastDirPath = IOPath.GetDirectoryName(path) ?? "";
                        onSelected(path);
                    }
                }
            }
            catch { /* user cancelled */ }
        }

        private async void SelectFolderAsync(string title, System.Action<string> onSelected)
        {
            var dlg = Gtk.FileDialog.New();
            dlg.SetTitle(title);

            if (lastDirPath != "" && System.IO.Directory.Exists(lastDirPath))
                dlg.SetInitialFolder(Gio.FileHelper.NewForPath(lastDirPath));

            try
            {
                var file = await dlg.SelectFolderAsync(this);
                if (file != null)
                {
                    string path = file.GetPath() ?? "";
                    if (path != "")
                    {
                        lastDirPath = path;
                        onSelected(path);
                    }
                }
            }
            catch { /* user cancelled */ }
        }
      }
    }
