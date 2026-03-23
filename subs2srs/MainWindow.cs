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
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using Action = System.Action;

namespace subs2srs
{
    public class MainWindow : Window
    {
        // Main tab
        private Entry _txtSubs1;
        private Entry _txtSubs2;
        private Entry _txtVideo;
        private Entry _txtOutputDir;
        private Entry _txtDeckName;
        private SpinButton _spinEpisodeStart;
        private ComboBoxText _comboEncodingSubs1;
        private ComboBoxText _comboEncodingSubs2;
        private ComboBoxText _comboAudioStream;
        private RadioButton _radioTimingSubs1;
        private RadioButton _radioTimingSubs2;
        private CheckButton _chkTimeShift;
        private SpinButton _spinTimeShiftSubs1;
        private TreeView _tvShiftRules;
        private ListStore _shiftRulesStore;
        private SpinButton _spinTimeShiftSubs2;
        private CheckButton _chkSpan;
        private Entry _txtSpanStart;
        private Entry _txtSpanEnd;

        // Audio tab
        private CheckButton _chkGenerateAudio;
        private RadioButton _radioAudioFromVideo;
        private RadioButton _radioAudioExisting;
        private Entry _txtAudioFile;
        private ComboBoxText _comboAudioBitrate;
        private CheckButton _chkAudioPad;
        private SpinButton _spinAudioPadStart;
        private SpinButton _spinAudioPadEnd;
        private CheckButton _chkNormalize;

        // Snapshot tab
        private CheckButton _chkGenerateSnapshots;
        private SpinButton _spinSnapshotWidth;
        private SpinButton _spinSnapshotHeight;
        private SpinButton _spinSnapshotCropBottom;
        private SpinButton _spinSnapshotQuality;

        // Video tab
        private CheckButton _chkGenerateVideo;
        private SpinButton _spinVideoWidth;
        private SpinButton _spinVideoHeight;
        private SpinButton _spinVideoCropBottom;
        private SpinButton _spinVideoBitrateVideo;
        private ComboBoxText _comboVideoBitrateAudio;
        private CheckButton _chkVideoPad;
        private SpinButton _spinVideoPadStart;
        private SpinButton _spinVideoPadEnd;
        private CheckButton _chkIPod;

        private Button _btnGo;
        private Button _btnCancel;
        private ProgressBar _progressBar;
        private GtkProgressReporter? _reporter;
        private List<InfoStream> _audioStreams = new List<InfoStream>();
        private DialogPreview _preview;

        public MainWindow() : base(WindowType.Toplevel)
        {
            Title = "subs2srs";
            SetDefaultSize(700, 650);
            WindowPosition = WindowPosition.Center;
            DeleteEvent += (o, args) =>
            {
                if (_preview != null)
                {
                    _preview.CleanupAndDestroy();
                    _preview = null;
                }
                Logger.Instance.flush();
                Application.Quit();
            };

            BuildUI();
            LoadSettings();
        }

        private void BuildUI()
        {
            var mainVBox = new Box(Orientation.Vertical, 5) { BorderWidth = 8 };

            var notebook = new Notebook();
            notebook.AppendPage(BuildMainTab(), new Label("Main"));
            notebook.AppendPage(BuildAudioTab(), new Label("Audio"));
            notebook.AppendPage(BuildSnapshotTab(), new Label("Snapshots"));
            notebook.AppendPage(BuildVideoTab(), new Label("Video Clips"));
            notebook.AppendPage(BuildToolsTab(), new Label("Tools"));
            mainVBox.PackStart(notebook, true, true, 0);

            _progressBar = new ProgressBar { Fraction = 0.0, ShowText = true, Text = "Ready" };
            mainVBox.PackStart(_progressBar, false, false, 0);

            var bottomHBox = new Box(Orientation.Horizontal, 5);
            var btnPreview = new Button("Preview...");
            btnPreview.Clicked += (s, e) =>
            {
                SaveSettings();
                if (_preview == null || _preview.IsDestroyed)
                {
                    _preview = new DialogPreview();
                    _preview.RefreshSettings += (s2, e2) => SaveSettings();
                    _preview.GoRequested += (s2, e2) => OnGoClicked(null, null);
                }
                _preview.StartPreview();
            };

            var btnAbout = new Button("About");
            btnAbout.Clicked += (s, e) =>
            {
                var dlg = new AboutDialog
                {
                    ProgramName = UtilsAssembly.Title,
                    Version = UtilsAssembly.Version,
                    Comments = UtilsAssembly.Product,
                    Authors = new[] { "Original author: Christopher Brochtrup" },
                    Website = "https://gitlab.com/fkzys/subs2srs-gtk3",
                    WebsiteLabel = "Source Code (GTK3 port)",
                    Copyright = UtilsAssembly.Copyright,
                    License = "GNU General Public License v3",
                    TransientFor = this
                };
                dlg.Run();
                dlg.Destroy();
            };

            var btnPref = new Button("Preferences...");
            btnPref.Clicked += (s, e) =>
            {
                var dlg = new DialogPref(this);
                dlg.Run();
                dlg.Destroy();
                PrefIO.read();
                SetDefaultSize(ConstantSettings.MainWindowWidth, ConstantSettings.MainWindowHeight);
            };
            bottomHBox.PackStart(btnPref, false, false, 0);

            bottomHBox.PackStart(btnAbout, false, false, 0);

            _btnGo = new Button("Go!") { WidthRequest = 100 };
            _btnGo.Clicked += OnGoClicked;

            _btnCancel = new Button("Cancel") { WidthRequest = 100, Sensitive = false };
            _btnCancel.Clicked += (s, e) => { if (_reporter != null) _reporter.Cancel = true; };

            bottomHBox.PackStart(btnPreview, false, false, 0);
            bottomHBox.PackEnd(_btnGo, false, false, 0);
            bottomHBox.PackEnd(_btnCancel, false, false, 0);

            mainVBox.PackStart(bottomHBox, false, false, 0);
            Add(mainVBox);
        }

        // ── TOOLS TAB ─────────────────────────────────────────────────────────

        private Widget BuildToolsTab()
        {
            var vbox = new Box(Orientation.Vertical, 12) { BorderWidth = 10 };

            // Extract Audio
            var extractFrame = new Frame("Extract Audio from Media");
            var extractBox = new Box(Orientation.Vertical, 6) { BorderWidth = 8 };
            extractBox.PackStart(new Label("Extract audio clips from media files using subtitle timings.")
                { Halign = Align.Start }, false, false, 0);
            var btnExtractAudio = new Button("Extract Audio...") { Halign = Align.Start };
            btnExtractAudio.Clicked += (s, e) =>
            {
                SaveSettings();
                var dlg = new DialogExtractAudioFromMedia(this)
                {
                    MediaFilePattern = _txtVideo.Text,
                    OutputDir = _txtOutputDir.Text,
                    DeckName = _txtDeckName.Text,
                    EpisodeStartNumber = (int)_spinEpisodeStart.Value,
                    Bitrate = Settings.Instance.AudioClips.Bitrate
                };
                dlg.Run();
                dlg.Destroy();
            };
            extractBox.PackStart(btnExtractAudio, false, false, 0);
            extractFrame.Add(extractBox);
            vbox.PackStart(extractFrame, false, false, 0);

            // Dueling Subtitles
            var duelingFrame = new Frame("Dueling Subtitles");
            var duelingBox = new Box(Orientation.Vertical, 6) { BorderWidth = 8 };
            duelingBox.PackStart(new Label("Create dual-language subtitle files.")
                { Halign = Align.Start }, false, false, 0);
            var btnDueling = new Button("Dueling Subs...") { Halign = Align.Start };
            btnDueling.Clicked += (s, e) =>
            {
                SaveSettings();
                var dlg = new DialogDuelingSubtitles(this);
                dlg.Run();
                dlg.Destroy();
            };
            duelingBox.PackStart(btnDueling, false, false, 0);
            duelingFrame.Add(duelingBox);
            vbox.PackStart(duelingFrame, false, false, 0);

            // Advanced Subtitle Options
            var advFrame = new Frame("Advanced Subtitle Options");
            var advBox = new Box(Orientation.Vertical, 6) { BorderWidth = 8 };
            advBox.PackStart(new Label("Configure filtering, joining, context lines and other subtitle options.")
                { Halign = Align.Start }, false, false, 0);
            var btnAdvanced = new Button("Advanced...") { Halign = Align.Start };
            btnAdvanced.Clicked += (s, e) =>
            {
                SaveSettings();
                var dlg = new DialogAdvancedSubtitleOptions(this)
                {
                    Subs1FilePattern = _txtSubs1.Text,
                    Subs2FilePattern = _txtSubs2.Text,
                    Subs1Encoding = _comboEncodingSubs1.ActiveText ?? "",
                    Subs2Encoding = _comboEncodingSubs2.ActiveText ?? ""
                };
                if (dlg.Run() == (int)ResponseType.Ok)
                    dlg.SaveToSettings();
                dlg.Destroy();
            };
            advBox.PackStart(btnAdvanced, false, false, 0);
            advFrame.Add(advBox);
            vbox.PackStart(advFrame, false, false, 0);

            return vbox;
        }

        // ── MAIN TAB ─────────────────────────────────────────────────────────

        private Widget BuildMainTab()
        {
            var vbox = new Box(Orientation.Vertical, 8) { BorderWidth = 10 };

            // Files grid
            var grid = new Grid { RowSpacing = 6, ColumnSpacing = 6 };
            int row = 0;

            // Subs1
            grid.Attach(new Label("Subtitle 1:") { Halign = Align.End }, 0, row, 1, 1);
            _txtSubs1 = new Entry { Hexpand = true };
            _txtSubs1.Changed += OnSubs1Changed;
            grid.Attach(_txtSubs1, 1, row, 1, 1);
            var btnSubs1 = new Button("Browse...");
            btnSubs1.Clicked += (s, e) => { var f = SelectFile("Select Subtitle 1"); if (f != "") _txtSubs1.Text = f; };
            grid.Attach(btnSubs1, 2, row, 1, 1);
            row++;

            // Encoding Subs1
            grid.Attach(new Label("Subs1 Encoding:") { Halign = Align.End }, 0, row, 1, 1);
            _comboEncodingSubs1 = BuildEncodingCombo();
            grid.Attach(_comboEncodingSubs1, 1, row, 2, 1);
            row++;

            // Subs2
            grid.Attach(new Label("Subtitle 2:") { Halign = Align.End }, 0, row, 1, 1);
            _txtSubs2 = new Entry { Hexpand = true };
            grid.Attach(_txtSubs2, 1, row, 1, 1);
            var btnSubs2 = new Button("Browse...");
            btnSubs2.Clicked += (s, e) => { var f = SelectFile("Select Subtitle 2"); if (f != "") _txtSubs2.Text = f; };
            grid.Attach(btnSubs2, 2, row, 1, 1);
            row++;

            // Encoding Subs2
            grid.Attach(new Label("Subs2 Encoding:") { Halign = Align.End }, 0, row, 1, 1);
            _comboEncodingSubs2 = BuildEncodingCombo();
            grid.Attach(_comboEncodingSubs2, 1, row, 2, 1);
            row++;

            // Video
            grid.Attach(new Label("Video:") { Halign = Align.End }, 0, row, 1, 1);
            _txtVideo = new Entry { Hexpand = true };
            _txtVideo.Changed += OnVideoChanged;
            grid.Attach(_txtVideo, 1, row, 1, 1);
            var btnVideo = new Button("Browse...");
            btnVideo.Clicked += (s, e) => { var f = SelectFile("Select Video"); if (f != "") _txtVideo.Text = f; };
            grid.Attach(btnVideo, 2, row, 1, 1);
            row++;

            // Audio stream
            grid.Attach(new Label("Audio Stream:") { Halign = Align.End }, 0, row, 1, 1);
            _comboAudioStream = new ComboBoxText();
            _comboAudioStream.AppendText("0 - (Default)");
            _comboAudioStream.Active = 0;
            grid.Attach(_comboAudioStream, 1, row, 2, 1);
            row++;

            // Output dir
            grid.Attach(new Label("Output Dir:") { Halign = Align.End }, 0, row, 1, 1);
            _txtOutputDir = new Entry { Hexpand = true };
            grid.Attach(_txtOutputDir, 1, row, 1, 1);
            var btnOut = new Button("Browse...");
            btnOut.Clicked += (s, e) => { var f = SelectFolder("Select Output Directory"); if (f != "") _txtOutputDir.Text = f; };
            grid.Attach(btnOut, 2, row, 1, 1);
            row++;

            // Deck name
            grid.Attach(new Label("Deck Name:") { Halign = Align.End }, 0, row, 1, 1);
            _txtDeckName = new Entry { Hexpand = true };
            grid.Attach(_txtDeckName, 1, row, 2, 1);
            row++;

            // Episode start
            grid.Attach(new Label("Episode Start #:") { Halign = Align.End }, 0, row, 1, 1);
            _spinEpisodeStart = new SpinButton(1, 9999, 1) { Value = 1 };
            grid.Attach(_spinEpisodeStart, 1, row, 1, 1);
            row++;

            vbox.PackStart(grid, false, false, 0);
            vbox.PackStart(new Separator(Orientation.Horizontal), false, false, 4);

            // Timing
            var timingFrame = new Frame("Use Timings From");
            var timingBox = new Box(Orientation.Horizontal, 10) { BorderWidth = 6 };
            _radioTimingSubs1 = new RadioButton("Subs 1");
            _radioTimingSubs2 = new RadioButton(_radioTimingSubs1, "Subs 2");
            timingBox.PackStart(_radioTimingSubs1, false, false, 0);
            timingBox.PackStart(_radioTimingSubs2, false, false, 0);
            timingFrame.Add(timingBox);
            vbox.PackStart(timingFrame, false, false, 0);

            // Time shift
            _chkTimeShift = new CheckButton("Time Shift");
            var timeShiftBox = new Box(Orientation.Vertical, 6);

            var globalShiftRow = new Box(Orientation.Horizontal, 6);
            globalShiftRow.PackStart(_chkTimeShift, false, false, 0);
            globalShiftRow.PackStart(new Label("Subs1 (ms):"), false, false, 0);
            _spinTimeShiftSubs1 = new SpinButton(-99999, 99999, 1) { Value = 0 };
            globalShiftRow.PackStart(_spinTimeShiftSubs1, false, false, 0);
            globalShiftRow.PackStart(new Label("Subs2 (ms):"), false, false, 0);
            _spinTimeShiftSubs2 = new SpinButton(-99999, 99999, 1) { Value = 0 };
            globalShiftRow.PackStart(_spinTimeShiftSubs2, false, false, 0);
            timeShiftBox.PackStart(globalShiftRow, false, false, 0);

            // Per-episode cascading shift rules
            var rulesFrame = new Frame("Per-Episode Shift Rules (cascading)");
            var rulesVBox = new Box(Orientation.Vertical, 4) { BorderWidth = 4 };

            // Store columns: FromEpisode (int), Subs1Shift (int), Subs2Shift (int)
            _shiftRulesStore = new ListStore(typeof(int), typeof(int), typeof(int));
            _tvShiftRules = new TreeView(_shiftRulesStore) { HeadersVisible = true };

            // Column 0: From Episode
            var epRenderer = new CellRendererText { Editable = true };
            epRenderer.Edited += OnShiftRuleCellEdited;
            var epCol = new TreeViewColumn("From Episode", epRenderer);
            epCol.SetCellDataFunc(epRenderer, (TreeViewColumn col, CellRenderer cell, ITreeModel model, TreeIter iter) =>
            {
                ((CellRendererText)cell).Text = ((int)model.GetValue(iter, 0)).ToString();
            });
            epCol.MinWidth = 120;
            epCol.Expand = true;
            _tvShiftRules.AppendColumn(epCol);

            // Column 1: Subs1 Shift (ms)
            var s1Renderer = new CellRendererText { Editable = true };
            s1Renderer.Edited += OnShiftRuleCellEdited;
            var s1Col = new TreeViewColumn("Subs1 Shift (ms)", s1Renderer);
            s1Col.SetCellDataFunc(s1Renderer, (TreeViewColumn col, CellRenderer cell, ITreeModel model, TreeIter iter) =>
            {
                ((CellRendererText)cell).Text = ((int)model.GetValue(iter, 1)).ToString();
            });
            s1Col.MinWidth = 140;
            s1Col.Expand = true;
            _tvShiftRules.AppendColumn(s1Col);

            // Column 2: Subs2 Shift (ms)
            var s2Renderer = new CellRendererText { Editable = true };
            s2Renderer.Edited += OnShiftRuleCellEdited;
            var s2Col = new TreeViewColumn("Subs2 Shift (ms)", s2Renderer);
            s2Col.SetCellDataFunc(s2Renderer, (TreeViewColumn col, CellRenderer cell, ITreeModel model, TreeIter iter) =>
            {
                ((CellRendererText)cell).Text = ((int)model.GetValue(iter, 2)).ToString();
            });
            s2Col.MinWidth = 140;
            s2Col.Expand = true;
            _tvShiftRules.AppendColumn(s2Col);

            var rulesSw = new ScrolledWindow { ShadowType = ShadowType.In, HeightRequest = 120 };
            rulesSw.Add(_tvShiftRules);
            rulesVBox.PackStart(rulesSw, true, true, 0);

            var rulesBtnBox = new Box(Orientation.Horizontal, 4);
            var btnAddRule = new Button("Add Rule");
            btnAddRule.Clicked += (s, e) =>
            {
                int nextEp = 1;
                if (_shiftRulesStore.IterNChildren() > 0)
                {
                    _shiftRulesStore.GetIter(out var last,
                        new TreePath(new[] { _shiftRulesStore.IterNChildren() - 1 }));
                    nextEp = (int)_shiftRulesStore.GetValue(last, 0) + 1;
                }
                var newIter = _shiftRulesStore.AppendValues(nextEp, 0, 0);
                // Select and scroll to the new row
                _tvShiftRules.Selection.SelectIter(newIter);
                _tvShiftRules.ScrollToCell(
                    _shiftRulesStore.GetPath(newIter), null, false, 0, 0);
            };
            rulesBtnBox.PackStart(btnAddRule, false, false, 0);

            var btnRemoveRule = new Button("Remove Selected");
            btnRemoveRule.Clicked += (s, e) =>
            {
                if (_tvShiftRules.Selection.GetSelected(out var iter))
                    _shiftRulesStore.Remove(ref iter);
            };
            rulesBtnBox.PackStart(btnRemoveRule, false, false, 0);
            rulesVBox.PackStart(rulesBtnBox, false, false, 0);

            rulesFrame.Add(rulesVBox);
            timeShiftBox.PackStart(rulesFrame, false, false, 0);
            vbox.PackStart(timeShiftBox, false, false, 0);

            // Span
            _chkSpan = new CheckButton("Span (h:mm:ss)");
            var spanBox = new Box(Orientation.Horizontal, 6);
            spanBox.PackStart(_chkSpan, false, false, 0);
            spanBox.PackStart(new Label("Start:"), false, false, 0);
            _txtSpanStart = new Entry { Text = "0:01:30", WidthChars = 8 };
            spanBox.PackStart(_txtSpanStart, false, false, 0);
            spanBox.PackStart(new Label("End:"), false, false, 0);
            _txtSpanEnd = new Entry { Text = "0:22:30", WidthChars = 8 };
            spanBox.PackStart(_txtSpanEnd, false, false, 0);
            vbox.PackStart(spanBox, false, false, 0);

            return vbox;
        }

        private ComboBoxText BuildEncodingCombo()
        {
            var combo = new ComboBoxText();
            var encodings = InfoEncoding.getEncodings();
            foreach (InfoEncoding enc in encodings)
                combo.AppendText(enc.LongName);
            int idx = 0;
            foreach (InfoEncoding enc in encodings)
            {
                if (enc.ShortName == "utf-8") { combo.Active = idx; break; }
                idx++;
            }
            return combo;
        }

        // ── AUDIO TAB ────────────────────────────────────────────────────────

        private Widget BuildAudioTab()
        {
            var vbox = new Box(Orientation.Vertical, 8) { BorderWidth = 10 };

            _chkGenerateAudio = new CheckButton("Generate Audio Clips") { Active = true };
            vbox.PackStart(_chkGenerateAudio, false, false, 0);
            vbox.PackStart(new Separator(Orientation.Horizontal), false, false, 2);

            // Source
            var sourceFrame = new Frame("Source");
            var sourceBox = new Box(Orientation.Vertical, 4) { BorderWidth = 6 };
            _radioAudioFromVideo = new RadioButton("Extract from video, bitrate:");
            _radioAudioExisting = new RadioButton(_radioAudioFromVideo, "Use existing audio file:");
            var bitrateBox = new Box(Orientation.Horizontal, 6);
            bitrateBox.PackStart(_radioAudioFromVideo, false, false, 0);
            _comboAudioBitrate = new ComboBoxText();
            foreach (var b in new[] { "64", "96", "112", "128", "160", "192", "256", "320" })
                _comboAudioBitrate.AppendText(b);
            _comboAudioBitrate.Active = 3; // 128
            bitrateBox.PackStart(_comboAudioBitrate, false, false, 0);
            bitrateBox.PackStart(new Label("kbps"), false, false, 0);
            sourceBox.PackStart(bitrateBox, false, false, 0);

            var existingBox = new Box(Orientation.Horizontal, 6);
            existingBox.PackStart(_radioAudioExisting, false, false, 0);
            _txtAudioFile = new Entry { Hexpand = true, Sensitive = false };
            existingBox.PackStart(_txtAudioFile, true, true, 0);
            var btnAudio = new Button("Browse...");
            btnAudio.Sensitive = false;
            btnAudio.Clicked += (s, e) => { var f = SelectFile("Select Audio File"); if (f != "") _txtAudioFile.Text = f; };
            existingBox.PackStart(btnAudio, false, false, 0);
            sourceBox.PackStart(existingBox, false, false, 0);

            _radioAudioFromVideo.Toggled += (s, e) => {
                _comboAudioBitrate.Sensitive = _radioAudioFromVideo.Active;
                _txtAudioFile.Sensitive = !_radioAudioFromVideo.Active;
                btnAudio.Sensitive = !_radioAudioFromVideo.Active;
            };

            sourceFrame.Add(sourceBox);
            vbox.PackStart(sourceFrame, false, false, 0);

            // Pad timings
            _chkAudioPad = new CheckButton("Pad Timings");
            var padBox = new Box(Orientation.Horizontal, 6);
            padBox.PackStart(_chkAudioPad, false, false, 0);
            padBox.PackStart(new Label("Start (ms):"), false, false, 0);
            _spinAudioPadStart = new SpinButton(0, 9999, 50) { Value = 250 };
            padBox.PackStart(_spinAudioPadStart, false, false, 0);
            padBox.PackStart(new Label("End (ms):"), false, false, 0);
            _spinAudioPadEnd = new SpinButton(0, 9999, 50) { Value = 250 };
            padBox.PackStart(_spinAudioPadEnd, false, false, 0);
            vbox.PackStart(padBox, false, false, 0);

            _chkNormalize = new CheckButton("Normalize audio (requires mp3gain)");
            vbox.PackStart(_chkNormalize, false, false, 0);

            return vbox;
        }

        // ── SNAPSHOT TAB ─────────────────────────────────────────────────────

        private Widget BuildSnapshotTab()
        {
            var vbox = new Box(Orientation.Vertical, 8) { BorderWidth = 10 };

            _chkGenerateSnapshots = new CheckButton("Generate Snapshots") { Active = true };
            vbox.PackStart(_chkGenerateSnapshots, false, false, 0);
            vbox.PackStart(new Separator(Orientation.Horizontal), false, false, 2);

            var grid = new Grid { RowSpacing = 6, ColumnSpacing = 6 };
            grid.Attach(new Label("Width:") { Halign = Align.End }, 0, 0, 1, 1);
            _spinSnapshotWidth = new SpinButton(16, 3840, 16) { Value = 240 };
            grid.Attach(_spinSnapshotWidth, 1, 0, 1, 1);
            grid.Attach(new Label("px"), 2, 0, 1, 1);

            grid.Attach(new Label("Height:") { Halign = Align.End }, 0, 1, 1, 1);
            _spinSnapshotHeight = new SpinButton(16, 2160, 2) { Value = 160 };
            grid.Attach(_spinSnapshotHeight, 1, 1, 1, 1);
            grid.Attach(new Label("px"), 2, 1, 1, 1);

            grid.Attach(new Label("Crop Bottom:") { Halign = Align.End }, 0, 2, 1, 1);
            _spinSnapshotCropBottom = new SpinButton(0, 2160, 2) { Value = 0 };
            grid.Attach(_spinSnapshotCropBottom, 1, 2, 1, 1);
            grid.Attach(new Label("px"), 2, 2, 1, 1);

            grid.Attach(new Label("JPEG Quality:") { Halign = Align.End }, 0, 3, 1, 1);
            _spinSnapshotQuality = new SpinButton(1, 31, 1) { Value = 3 };
            grid.Attach(_spinSnapshotQuality, 1, 3, 1, 1);
            grid.Attach(new Label("(1 = best, 5 = good, 15 = low)") { Halign = Align.Start }, 2, 3, 1, 1);

            vbox.PackStart(grid, false, false, 0);
            return vbox;
        }

        // ── VIDEO CLIP TAB ───────────────────────────────────────────────────

        private Widget BuildVideoTab()
        {
            var vbox = new Box(Orientation.Vertical, 8) { BorderWidth = 10 };

            _chkGenerateVideo = new CheckButton("Generate Video Clips") { Active = false };
            vbox.PackStart(_chkGenerateVideo, false, false, 0);
            vbox.PackStart(new Separator(Orientation.Horizontal), false, false, 2);

            var grid = new Grid { RowSpacing = 6, ColumnSpacing = 6 };
            int row = 0;

            grid.Attach(new Label("Width:") { Halign = Align.End }, 0, row, 1, 1);
            _spinVideoWidth = new SpinButton(16, 3840, 16) { Value = 240 };
            grid.Attach(_spinVideoWidth, 1, row, 1, 1);
            grid.Attach(new Label("px"), 2, row, 1, 1);
            row++;

            grid.Attach(new Label("Height:") { Halign = Align.End }, 0, row, 1, 1);
            _spinVideoHeight = new SpinButton(16, 2160, 2) { Value = 160 };
            grid.Attach(_spinVideoHeight, 1, row, 1, 1);
            grid.Attach(new Label("px"), 2, row, 1, 1);
            row++;

            grid.Attach(new Label("Crop Bottom:") { Halign = Align.End }, 0, row, 1, 1);
            _spinVideoCropBottom = new SpinButton(0, 2160, 2) { Value = 0 };
            grid.Attach(_spinVideoCropBottom, 1, row, 1, 1);
            grid.Attach(new Label("px"), 2, row, 1, 1);
            row++;

            grid.Attach(new Label("Video Bitrate:") { Halign = Align.End }, 0, row, 1, 1);
            _spinVideoBitrateVideo = new SpinButton(100, 10000, 100) { Value = 800 };
            grid.Attach(_spinVideoBitrateVideo, 1, row, 1, 1);
            grid.Attach(new Label("kb/s"), 2, row, 1, 1);
            row++;

            grid.Attach(new Label("Audio Bitrate:") { Halign = Align.End }, 0, row, 1, 1);
            _comboVideoBitrateAudio = new ComboBoxText();
            foreach (var b in new[] { "64", "96", "112", "128", "160", "192", "256", "320" })
                _comboVideoBitrateAudio.AppendText(b);
            _comboVideoBitrateAudio.Active = 3; // 128
            grid.Attach(_comboVideoBitrateAudio, 1, row, 1, 1);
            grid.Attach(new Label("kb/s"), 2, row, 1, 1);
            row++;

            vbox.PackStart(grid, false, false, 0);

            // Pad
            _chkVideoPad = new CheckButton("Pad Timings");
            var padBox = new Box(Orientation.Horizontal, 6);
            padBox.PackStart(_chkVideoPad, false, false, 0);
            padBox.PackStart(new Label("Start (ms):"), false, false, 0);
            _spinVideoPadStart = new SpinButton(0, 9999, 50) { Value = 250 };
            padBox.PackStart(_spinVideoPadStart, false, false, 0);
            padBox.PackStart(new Label("End (ms):"), false, false, 0);
            _spinVideoPadEnd = new SpinButton(0, 9999, 50) { Value = 250 };
            padBox.PackStart(_spinVideoPadEnd, false, false, 0);
            vbox.PackStart(padBox, false, false, 0);

            _chkIPod = new CheckButton("iPod/iPhone support");
            vbox.PackStart(_chkIPod, false, false, 0);

            return vbox;
        }

        // ── SETTINGS ─────────────────────────────────────────────────────────

        private void LoadSettings()
        {
            PrefIO.read();
            Settings.Instance.reset();

            _txtDeckName.Text = Settings.Instance.DeckName != "" ? Settings.Instance.DeckName : "MyDeck";
            _txtOutputDir.Text = Settings.Instance.OutputDir != ""
                ? Settings.Instance.OutputDir
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _spinEpisodeStart.Value = Settings.Instance.EpisodeStartNumber;

            _chkGenerateAudio.Active = Settings.Instance.AudioClips.Enabled;
            // Sync audio bitrate combo with loaded settings
            SetBitrateCombo(_comboAudioBitrate, Settings.Instance.AudioClips.Bitrate);
            SetBitrateCombo(_comboVideoBitrateAudio, Settings.Instance.VideoClips.BitrateAudio);
            _chkGenerateSnapshots.Active = Settings.Instance.Snapshots.Enabled;
            _chkGenerateVideo.Active = Settings.Instance.VideoClips.Enabled;

            _spinSnapshotWidth.Value = Settings.Instance.Snapshots.Size.Width > 0 ? Settings.Instance.Snapshots.Size.Width : 240;
            _spinSnapshotHeight.Value = Settings.Instance.Snapshots.Size.Height > 0 ? Settings.Instance.Snapshots.Size.Height : 160;
            _spinSnapshotCropBottom.Value = Settings.Instance.Snapshots.Crop.Bottom;
            _spinSnapshotQuality.Value = Settings.Instance.Snapshots.Quality > 0
                ? Settings.Instance.Snapshots.Quality : 3;

            _spinVideoWidth.Value = Settings.Instance.VideoClips.Size.Width > 0 ? Settings.Instance.VideoClips.Size.Width : 240;
            _spinVideoHeight.Value = Settings.Instance.VideoClips.Size.Height > 0 ? Settings.Instance.VideoClips.Size.Height : 160;
            _spinVideoCropBottom.Value = Settings.Instance.VideoClips.Crop.Bottom;
            _spinVideoBitrateVideo.Value = Settings.Instance.VideoClips.BitrateVideo > 0 ? Settings.Instance.VideoClips.BitrateVideo : 800;

            SetEncodingCombo(_comboEncodingSubs1, Settings.Instance.Subs[0].Encoding);
            SetEncodingCombo(_comboEncodingSubs2, Settings.Instance.Subs[1].Encoding);

            _radioTimingSubs1.Active = true;

            // Load per-episode shift rules
            _shiftRulesStore.Clear();
            if (Settings.Instance.Subs[0].TimeShiftRules?.Count > 0)
            {
                for (int i = 0; i < Settings.Instance.Subs[0].TimeShiftRules.Count; i++)
                {
                    int fromEp = Settings.Instance.Subs[0].TimeShiftRules[i].FromEpisode;
                    int s1 = Settings.Instance.Subs[0].TimeShiftRules[i].ShiftMs;
                    int s2 = (Settings.Instance.Subs[1].TimeShiftRules?.Count > i)
                        ? Settings.Instance.Subs[1].TimeShiftRules[i].ShiftMs : 0;
                    _shiftRulesStore.AppendValues(fromEp, s1, s2);
                }
            }

            SetDefaultSize(ConstantSettings.MainWindowWidth, ConstantSettings.MainWindowHeight);
        }

        private void SaveSettings()
        {
            try
            {
                Settings.Instance.DeckName = _txtDeckName.Text.Trim();
                Settings.Instance.OutputDir = _txtOutputDir.Text.Trim();
                Settings.Instance.EpisodeStartNumber = (int)_spinEpisodeStart.Value;

                // Subs
                Settings.Instance.Subs[0].FilePattern = _txtSubs1.Text.Trim();
                Settings.Instance.Subs[0].Encoding = GetSelectedEncoding(_comboEncodingSubs1);
                Settings.Instance.Subs[0].TimingsEnabled = _radioTimingSubs1.Active;
                Settings.Instance.Subs[0].TimeShift = (int)_spinTimeShiftSubs1.Value;
                Settings.Instance.Subs[0].Files = UtilsSubs.getSubsFiles(
                    Settings.Instance.Subs[0].FilePattern).ToArray();

                Settings.Instance.Subs[1].FilePattern = _txtSubs2.Text.Trim();
                Settings.Instance.Subs[1].Encoding = GetSelectedEncoding(_comboEncodingSubs2);
                Settings.Instance.Subs[1].TimingsEnabled = _radioTimingSubs2.Active;
                Settings.Instance.Subs[1].TimeShift = (int)_spinTimeShiftSubs2.Value;
                if (Settings.Instance.Subs[1].FilePattern.Length > 0)
                    Settings.Instance.Subs[1].Files = UtilsSubs.getSubsFiles(
                        Settings.Instance.Subs[1].FilePattern).ToArray();
                else
                    Settings.Instance.Subs[1].Files = Array.Empty<string>();

                Settings.Instance.TimeShiftEnabled = _chkTimeShift.Active;

                // Per-episode shift rules
                Settings.Instance.Subs[0].TimeShiftRules.Clear();
                Settings.Instance.Subs[1].TimeShiftRules.Clear();
                if (_shiftRulesStore.GetIterFirst(out var ruleIter))
                {
                    do
                    {
                        int fromEp = (int)_shiftRulesStore.GetValue(ruleIter, 0);
                        int s1Shift = (int)_shiftRulesStore.GetValue(ruleIter, 1);
                        int s2Shift = (int)_shiftRulesStore.GetValue(ruleIter, 2);
                        Settings.Instance.Subs[0].TimeShiftRules.Add(new TimeShiftRule(fromEp, s1Shift));
                        Settings.Instance.Subs[1].TimeShiftRules.Add(new TimeShiftRule(fromEp, s2Shift));
                    } while (_shiftRulesStore.IterNext(ref ruleIter));

                    // Ensure sorted by FromEpisode
                    Settings.Instance.Subs[0].TimeShiftRules.Sort((a, b) => a.FromEpisode.CompareTo(b.FromEpisode));
                    Settings.Instance.Subs[1].TimeShiftRules.Sort((a, b) => a.FromEpisode.CompareTo(b.FromEpisode));
                }

                Settings.Instance.SpanEnabled = _chkSpan.Active;
                if (_chkSpan.Active)
                {
                    Settings.Instance.SpanStart = UtilsSubs.stringToTime(_txtSpanStart.Text.Trim());
                    Settings.Instance.SpanEnd = UtilsSubs.stringToTime(_txtSpanEnd.Text.Trim());
                }

                // Video
                Settings.Instance.VideoClips.FilePattern = _txtVideo.Text.Trim();
                Settings.Instance.VideoClips.Files = UtilsCommon.getNonHiddenFiles(
                    Settings.Instance.VideoClips.FilePattern);

                // Audio stream — store actual ffmpeg stream number, not combo index
                if (_comboAudioStream.Active >= 0 && _comboAudioStream.Active < _audioStreams.Count)
                {
                    Settings.Instance.VideoClips.AudioStream = _audioStreams[_comboAudioStream.Active];
                }
                else
                {
                    Settings.Instance.VideoClips.AudioStream = new InfoStream("0:a:0", "0", "", "");
                }

                // Audio
                Settings.Instance.AudioClips.Enabled = _chkGenerateAudio.Active;
                Settings.Instance.AudioClips.UseAudioFromVideo = _radioAudioFromVideo.Active;
                Settings.Instance.AudioClips.UseExistingAudio = _radioAudioExisting.Active;
                Settings.Instance.AudioClips.Bitrate = GetSelectedBitrate(_comboAudioBitrate, 128);
                Settings.Instance.AudioClips.PadEnabled = _chkAudioPad.Active;
                Settings.Instance.AudioClips.PadStart = (int)_spinAudioPadStart.Value;
                Settings.Instance.AudioClips.PadEnd = (int)_spinAudioPadEnd.Value;
                Settings.Instance.AudioClips.Normalize = _chkNormalize.Active;

                Settings.Instance.AudioClips.FilePattern = _txtAudioFile.Text.Trim();
                Settings.Instance.AudioClips.Files = UtilsCommon.getNonHiddenFiles(
                    Settings.Instance.AudioClips.FilePattern);

                // Snapshots
                Settings.Instance.Snapshots.Enabled = _chkGenerateSnapshots.Active;
                Settings.Instance.Snapshots.Size.Width = (int)_spinSnapshotWidth.Value;
                Settings.Instance.Snapshots.Size.Height = (int)_spinSnapshotHeight.Value;
                Settings.Instance.Snapshots.Crop.Bottom = (int)_spinSnapshotCropBottom.Value;
                Settings.Instance.Snapshots.Quality = (int)_spinSnapshotQuality.Value;

                // Video clips
                Settings.Instance.VideoClips.Enabled = _chkGenerateVideo.Active;
                Settings.Instance.VideoClips.Size.Width = (int)_spinVideoWidth.Value;
                Settings.Instance.VideoClips.Size.Height = (int)_spinVideoHeight.Value;
                Settings.Instance.VideoClips.Crop.Bottom = (int)_spinVideoCropBottom.Value;
                Settings.Instance.VideoClips.BitrateVideo = (int)_spinVideoBitrateVideo.Value;
                Settings.Instance.VideoClips.BitrateAudio = GetSelectedBitrate(_comboVideoBitrateAudio, 128);
                Settings.Instance.VideoClips.PadEnabled = _chkVideoPad.Active;
                Settings.Instance.VideoClips.PadStart = (int)_spinVideoPadStart.Value;
                Settings.Instance.VideoClips.PadEnd = (int)_spinVideoPadEnd.Value;
                Settings.Instance.VideoClips.IPodSupport = _chkIPod.Active;
            }
            catch (Exception e1)
            {
                UtilsMsg.showErrMsg("Something went wrong while gathering interface data:\n" + e1);
            }
        }

        private string GetSelectedEncoding(ComboBoxText combo)
        {
            var encodings = InfoEncoding.getEncodings();
            int idx = combo.Active;
            int i = 0;
            foreach (InfoEncoding enc in encodings)
            {
                if (i == idx) return enc.ShortName;
                i++;
            }
            return "utf-8";
        }

        private void SetEncodingCombo(ComboBoxText combo, string shortName)
        {
            int i = 0;
            foreach (InfoEncoding enc in InfoEncoding.getEncodings())
            {
                if (enc.ShortName == shortName) { combo.Active = i; return; }
                i++;
            }
            combo.Active = 0;
        }

        private int GetSelectedBitrate(ComboBoxText combo, int defaultVal)
        {
            if (int.TryParse(combo.ActiveText, out int val)) return val;
            return defaultVal;
        }

        private void SetBitrateCombo(ComboBoxText combo, int bitrate)
        {
            string target = bitrate.ToString();
            for (int i = 0; i < 8; i++) // 8 items in combo
            {
                combo.Active = i;
                if (combo.ActiveText == target) return;
            }
            combo.Active = 3; // fallback: 128
        }

        // ── EVENT HANDLERS ───────────────────────────────────────────────────

        private void OnSubs1Changed(object? sender, EventArgs e)
        {
        }

        private async void OnVideoChanged(object? sender, EventArgs e)
        {
            string pattern = _txtVideo.Text.Trim();
            if (string.IsNullOrEmpty(pattern)) return;

            // Run file lookup and ffprobe off the UI thread
            var (streams, fallback) = await Task.Run(() =>
            {
                string file = null;
                if (System.IO.File.Exists(pattern))
                    file = pattern;
                else
                {
                    var files = UtilsCommon.getNonHiddenFiles(pattern);
                    if (files.Length > 0) file = files[0];
                }

                if (file == null)
                    return (new List<InfoStream>(), true);

                var s = UtilsVideo.getAvailableAudioStreams(file);
                return (s, false);
            });

            if (fallback) return;

            _audioStreams = streams;
            _comboAudioStream.RemoveAll();

            if (_audioStreams.Count > 0)
            {
                foreach (var s in _audioStreams)
                    _comboAudioStream.AppendText(s.ToString());
            }
            else
            {
                _audioStreams.Add(new InfoStream("0:a:0", "0", "", "Default"));
                _comboAudioStream.AppendText("0 - (Default)");
            }
            _comboAudioStream.Active = 0;
        }

        private async void OnGoClicked(object? sender, EventArgs e)
        {
            if (!_btnGo.Sensitive) return;

            if (string.IsNullOrWhiteSpace(_txtSubs1.Text))
            {
                UtilsMsg.showErrMsg("Please provide at least Subtitle 1.");
                return;
            }
            if (string.IsNullOrWhiteSpace(_txtOutputDir.Text))
            {
                UtilsMsg.showErrMsg("Please provide Output Directory.");
                return;
            }
            if (string.IsNullOrWhiteSpace(_txtDeckName.Text))
            {
                UtilsMsg.showErrMsg("Please provide Deck Name.");
                return;
            }

            SaveSettings();

            // Validate audio stream consistency across episodes
            bool needsAudioFromVideo =
              (Settings.Instance.AudioClips.Enabled && Settings.Instance.AudioClips.UseAudioFromVideo)
              || Settings.Instance.VideoClips.Enabled;

            if (needsAudioFromVideo
                && Settings.Instance.VideoClips.Files?.Length > 1)
            {
                int streamIdx = _comboAudioStream.Active >= 0 ? _comboAudioStream.Active : 0;
                var files = Settings.Instance.VideoClips.Files;

                // Probe off the UI thread to avoid blocking
                string warning = await Task.Run(() =>
                    UtilsVideo.validateAudioStreamConsistency(files, streamIdx));

                if (warning != null)
                {
                    if (!UtilsMsg.showConfirm(warning))
                        return;
                }
            }

            _btnGo.Sensitive = false;
            _btnCancel.Sensitive = true;

            _reporter = new GtkProgressReporter(_progressBar);
            var processor = new SubsProcessor();

            try
            {
                await processor.StartAsync(_reporter);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                UtilsMsg.showErrMsg($"Error: {ex.Message}\n\n{ex.StackTrace}");
            }

            _reporter.Stop();

            _btnGo.Sensitive = true;
            _btnCancel.Sensitive = false;
            if (_reporter != null)
            {
                _progressBar.Text = _reporter.Cancel ? "Cancelled" : "Finished!";
                _progressBar.Fraction = _reporter.Cancel ? 0.0 : 1.0;
            }
        }

        private void OnShiftRuleCellEdited(object sender, EditedArgs args)
        {
            if (!_shiftRulesStore.GetIter(out var iter, new TreePath(args.Path)))
                return;
            if (!int.TryParse(args.NewText.Trim(), out int val))
                return;

            // Find which column this renderer belongs to
            int col = -1;
            for (int c = 0; c < _tvShiftRules.Columns.Length; c++)
            {
                if (_tvShiftRules.Columns[c].Cells[0] == sender)
                { col = c; break; }
            }

            if (col < 0) return;

            // Episode number must be >= 1
            if (col == 0 && val < 1) val = 1;

            _shiftRulesStore.SetValue(iter, col, val);
        }

        // ── FILE DIALOGS ─────────────────────────────────────────────────────

        private string SelectFile(string title)
        {
            var dlg = new FileChooserDialog(title, this, FileChooserAction.Open,
                "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
            string result = string.Empty;
            if (dlg.Run() == (int)ResponseType.Accept)
                result = dlg.Filename;
            dlg.Destroy();
            return result;
        }

        private string SelectFolder(string title)
        {
            var dlg = new FileChooserDialog(title, this, FileChooserAction.SelectFolder,
                "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
            string result = string.Empty;
            if (dlg.Run() == (int)ResponseType.Accept)
                result = dlg.Filename;
            dlg.Destroy();
            return result;
        }

        // ── PROGRESS REPORTER ────────────────────────────────────────────────

        private class GtkProgressReporter : IProgressReporter
        {
            private readonly ProgressBar _bar;
            private readonly CancellationTokenSource _cts = new();
            private bool _cancel;
            public int StepsTotal { get; set; }

            private string _text;
            private double _fraction = -1;
            private bool _dirty;
            private readonly object _sync = new object();
            private bool _active = true;

            public bool Cancel
            {
                get => _cancel;
                set { _cancel = value; if (value) _cts.Cancel(); }
            }

            public CancellationToken Token => _cts.Token;

            public GtkProgressReporter(ProgressBar bar)
            {
                _bar = bar;
                GLib.Timeout.Add(50, OnPoll);
            }

            public void Stop()
            {
                _active = false;
                OnPoll();
            }

            private bool OnPoll()
            {
                string text;
                double frac;
                bool dirty;
                lock (_sync)
                {
                    text = _text;
                    frac = _fraction;
                    dirty = _dirty;
                    _dirty = false;
                }
                if (dirty)
                {
                    if (text != null) _bar.Text = text;
                    if (frac >= 0) _bar.Fraction = frac;
                }
                return _active;
            }

            public void NextStep(int step, string description)
            {
                lock (_sync) { _text = $"[{step}/{StepsTotal}] {description}"; _fraction = 0.0; _dirty = true; }
            }

            public void UpdateProgress(int percent, string text)
            {
                lock (_sync) { _text = text; _fraction = Math.Max(0, Math.Min(1, percent / 100.0)); _dirty = true; }
            }

            public void UpdateProgress(string text)
            {
                lock (_sync) { _text = text; _dirty = true; }
            }

            public void EnableDetail(bool enable) { }
            public void SetDuration(TimeSpan duration) { }
            public void OnFFmpegOutput(object sender, System.Diagnostics.DataReceivedEventArgs e) { }
        }
    }
}
