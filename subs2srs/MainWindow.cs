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

namespace subs2srs
{
    /// <summary>
    /// Main application window — GTK4/Gir.Core port.
    ///
    /// Key changes from GTK3:
    /// - Window(WindowType.Toplevel) → Gtk.ApplicationWindow + SetApplication()
    /// - Notebook tabs unchanged (GTK4 keeps Gtk.Notebook)
    /// - ComboBoxText → Gtk.DropDown + Gtk.StringList
    /// - RadioButton → Gtk.CheckButton + SetGroup()
    /// - TreeView/ListStore (shift rules) → Gtk.ListView + Gio.ListStore + ShiftRuleItem
    /// - FileChooserDialog → Gtk.FileDialog (async)
    /// - PackStart/PackEnd → Append()
    /// - BorderWidth → margins
    /// - Application.Invoke / GLib.Idle.Add → GLib.Functions.IdleAdd
    /// - GLib.Timeout.Add → GLib.Functions.TimeoutAdd
    /// - AboutDialog → simple Gtk.Window (GTK4 Adw.AboutWindow requires libadwaita)
    /// </summary>
    public class MainWindow : Gtk.ApplicationWindow
    {
        // ── Main tab fields ─────────────────────────────────────────────────
        private Gtk.Entry _txtSubs1;
        private Gtk.Entry _txtSubs2;
        private Gtk.Entry _txtVideo;
        private Gtk.Entry _txtOutputDir;
        private Gtk.Entry _txtDeckName;
        private Gtk.SpinButton _spinEpisodeStart;
        private Gtk.SpinButton _spinEpisodeEnd;
        private Gtk.DropDown _comboEncodingSubs1;
        private Gtk.StringList _encModel1;
        private Gtk.DropDown _comboEncodingSubs2;
        private Gtk.StringList _encModel2;
        private Gtk.DropDown _comboAudioStream;
        private Gtk.StringList _audioStreamModel;
        private Gtk.CheckButton _radioTimingSubs1;
        private Gtk.CheckButton _radioTimingSubs2;
        private Gtk.CheckButton _chkTimeShift;
        private Gtk.SpinButton _spinTimeShiftSubs1;
        private Gtk.SpinButton _spinTimeShiftSubs2;
        private Gtk.CheckButton _chkSpan;
        private Gtk.Entry _txtSpanStart;
        private Gtk.Entry _txtSpanEnd;

        // Shift rules — ColumnView with editable Entry cells
        private Gtk.ColumnView _shiftRulesColumnView;
        private Gio.ListStore _shiftRulesStore;
        private Gtk.SingleSelection _shiftRulesSel;
        private List<ShiftRuleItem> _shiftItems = new();
        // Maps Entry widget handle → ShiftRuleRef for OnBind/OnUnbind lookup
        private readonly Dictionary<nint, ShiftRuleRef> _shiftRefMap = new();

        // ── Audio tab fields ────────────────────────────────────────────────
        private Gtk.CheckButton _chkGenerateAudio;
        private Gtk.CheckButton _radioAudioFromVideo;
        private Gtk.CheckButton _radioAudioExisting;
        private Gtk.Entry _txtAudioFile;
        private Gtk.DropDown _comboAudioBitrate;
        private Gtk.StringList _audioBitrateModel;
        private Gtk.CheckButton _chkAudioPad;
        private Gtk.SpinButton _spinAudioPadStart;
        private Gtk.SpinButton _spinAudioPadEnd;
        private Gtk.CheckButton _chkNormalize;
        private Gtk.Button _btnAudioBrowse; // stored for sensitivity toggle

        // ── Snapshot tab fields ─────────────────────────────────────────────
        private Gtk.CheckButton _chkGenerateSnapshots;
        private Gtk.SpinButton _spinSnapshotWidth;
        private Gtk.SpinButton _spinSnapshotHeight;
        private Gtk.SpinButton _spinSnapshotCropBottom;
        private Gtk.SpinButton _spinSnapshotQuality;

        // ── Video tab fields ────────────────────────────────────────────────
        private Gtk.CheckButton _chkGenerateVideo;
        private Gtk.SpinButton _spinVideoWidth;
        private Gtk.SpinButton _spinVideoHeight;
        private Gtk.SpinButton _spinVideoCropBottom;
        private Gtk.SpinButton _spinVideoBitrateVideo;
        private Gtk.DropDown _comboVideoBitrateAudio;
        private Gtk.StringList _videoBitrateModel;
        private Gtk.CheckButton _chkVideoPad;
        private Gtk.SpinButton _spinVideoPadStart;
        private Gtk.SpinButton _spinVideoPadEnd;
        private Gtk.CheckButton _chkIPod;

        // ── Bottom bar ──────────────────────────────────────────────────────
        private Gtk.Button _btnGo;
        private Gtk.Button _btnCancel;
        private Gtk.ProgressBar _progressBar;
        private GtkProgressReporter? _reporter;
        private List<InfoStream> _audioStreams = new List<InfoStream>();
        private DialogPreview? _preview;

        // Bitrate options shared by audio and video tabs
        private static readonly string[] BitrateOptions =
            { "64", "96", "112", "128", "160", "192", "256", "320" };

        public MainWindow(Gtk.Application app) : base()
        {
            SetApplication(app);
            SetTitle("subs2srs");
            SetDefaultSize(700, 650);

            OnCloseRequest += (sender, args) =>
            {
                if (_preview != null)
                {
                    _preview.CleanupAndDestroy();
                    _preview = null;
                }
                Logger.Instance.flush();
                return false; // allow close → app quits when last window closes
            };

            BuildUI();
            LoadSettings();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  BUILD UI
        // ═══════════════════════════════════════════════════════════════════

        private void BuildUI()
        {
            // Global CSS for ColumnView header structure (borders, padding).
            // Text color/opacity is applied per-widget via P/Invoke after
            // the ColumnView is mapped — see deferred call below.
            GtkColumnViewHelper.ApplyGlobalCss(
                "columnview > header > button { "
                + "  background: alpha(currentColor, 0.12); "
                + "  border-bottom: 1px solid alpha(currentColor, 0.2); "
                + "  border-right: 1px solid alpha(currentColor, 0.1); "
                + "  min-height: 28px; "
                + "  padding: 4px 8px; "
                + "} "
                + "columnview > header > button:hover { "
                + "  background: alpha(currentColor, 0.22); "
                + "} "
            );

            var mainVBox = Gtk.Box.New(Gtk.Orientation.Vertical, 5);
            mainVBox.SetMarginTop(8);
            mainVBox.SetMarginBottom(8);
            mainVBox.SetMarginStart(8);
            mainVBox.SetMarginEnd(8);

            var notebook = Gtk.Notebook.New();
            notebook.AppendPage(BuildMainTab(), Gtk.Label.New("Main"));
            notebook.AppendPage(BuildAudioTab(), Gtk.Label.New("Audio"));
            notebook.AppendPage(BuildSnapshotTab(), Gtk.Label.New("Snapshots"));
            notebook.AppendPage(BuildVideoTab(), Gtk.Label.New("Video Clips"));
            notebook.AppendPage(BuildToolsTab(), Gtk.Label.New("Tools"));
            notebook.SetVexpand(true);
            mainVBox.Append(notebook);

            _progressBar = Gtk.ProgressBar.New();
            _progressBar.SetFraction(0.0);
            _progressBar.SetShowText(true);
            _progressBar.SetText("Ready");
            mainVBox.Append(_progressBar);

            // Bottom buttons
            var bottomHBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 5);

            var btnPref = Gtk.Button.NewWithLabel("Preferences...");
            btnPref.OnClicked += (s, e) =>
            {
                var dlg = new DialogPref(this);
                dlg.Run();
                dlg.Close();
                PrefIO.read();
                SetDefaultSize(ConstantSettings.MainWindowWidth,
                    ConstantSettings.MainWindowHeight);
            };
            bottomHBox.Append(btnPref);

            var btnSaveProject = Gtk.Button.NewWithLabel("Save Project");
            btnSaveProject.OnClicked += OnSaveProject;
            bottomHBox.Append(btnSaveProject);

            var btnLoadProject = Gtk.Button.NewWithLabel("Load Project");
            btnLoadProject.OnClicked += OnLoadProject;
            bottomHBox.Append(btnLoadProject);

            var btnPreview = Gtk.Button.NewWithLabel("Preview...");
            btnPreview.OnClicked += (s, e) =>
            {
                SaveSettings();
                if (_preview == null || _preview.IsDestroyed)
                {
                    _preview = new DialogPreview();
                    _preview.RefreshSettings += (s2, e2) => SaveSettings();
                    _preview.GoRequested += (s2, e2) => OnGoClicked(null, EventArgs.Empty);
                }
                _preview.StartPreview();
            };
            bottomHBox.Append(btnPreview);

            var btnAbout = Gtk.Button.NewWithLabel("About");
            btnAbout.OnClicked += OnAboutClicked;
            bottomHBox.Append(btnAbout);

            // Spacer to push Go/Cancel to the right
            var spacer = Gtk.Box.New(Gtk.Orientation.Horizontal, 0);
            spacer.SetHexpand(true);
            bottomHBox.Append(spacer);

            _btnCancel = Gtk.Button.NewWithLabel("Cancel");
            _btnCancel.SetSizeRequest(100, -1);
            _btnCancel.SetSensitive(false);
            _btnCancel.OnClicked += (s, e) =>
            {
                if (_reporter != null) _reporter.Cancel = true;
            };
            bottomHBox.Append(_btnCancel);

            _btnGo = Gtk.Button.NewWithLabel("Go!");
            _btnGo.SetSizeRequest(100, -1);
            _btnGo.OnClicked += OnGoClicked;
            bottomHBox.Append(_btnGo);

            mainVBox.Append(bottomHBox);
            SetChild(mainVBox);
        }

        // ── TOOLS TAB ───────────────────────────────────────────────────────

        private Gtk.Widget BuildToolsTab()
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 12);
            vbox.SetMarginTop(10);
            vbox.SetMarginBottom(10);
            vbox.SetMarginStart(10);
            vbox.SetMarginEnd(10);

            // Extract Audio
            var extractFrame = Gtk.Frame.New("Extract Audio from Media");
            var extractBox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            extractBox.SetMarginTop(8); extractBox.SetMarginBottom(8);
            extractBox.SetMarginStart(8); extractBox.SetMarginEnd(8);
            var lblExtract = Gtk.Label.New(
                "Extract audio clips from media files using subtitle timings.");
            lblExtract.SetHalign(Gtk.Align.Start);
            extractBox.Append(lblExtract);
            var btnExtractAudio = Gtk.Button.NewWithLabel("Extract Audio...");
            btnExtractAudio.SetHalign(Gtk.Align.Start);
            btnExtractAudio.OnClicked += (s, e) =>
            {
                SaveSettings();
                var dlg = new DialogExtractAudioFromMedia(this)
                {
                    MediaFilePattern = _txtVideo.GetText(),
                    OutputDir = _txtOutputDir.GetText(),
                    DeckName = _txtDeckName.GetText(),
                    EpisodeStartNumber = (int)_spinEpisodeStart.Value,
                    Bitrate = Settings.Instance.AudioClips.Bitrate
                };
                dlg.Run();
                dlg.Close();
            };
            extractBox.Append(btnExtractAudio);
            extractFrame.SetChild(extractBox);
            vbox.Append(extractFrame);

            // Dueling Subtitles
            var duelingFrame = Gtk.Frame.New("Dueling Subtitles");
            var duelingBox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            duelingBox.SetMarginTop(8); duelingBox.SetMarginBottom(8);
            duelingBox.SetMarginStart(8); duelingBox.SetMarginEnd(8);
            var lblDueling = Gtk.Label.New("Create dual-language subtitle files.");
            lblDueling.SetHalign(Gtk.Align.Start);
            duelingBox.Append(lblDueling);
            var btnDueling = Gtk.Button.NewWithLabel("Dueling Subs...");
            btnDueling.SetHalign(Gtk.Align.Start);
            btnDueling.OnClicked += (s, e) =>
            {
                SaveSettings();
                var dlg = new DialogDuelingSubtitles(this);
                dlg.Run();
                dlg.Close();
            };
            duelingBox.Append(btnDueling);
            duelingFrame.SetChild(duelingBox);
            vbox.Append(duelingFrame);

            // Advanced Subtitle Options
            var advFrame = Gtk.Frame.New("Advanced Subtitle Options");
            var advBox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            advBox.SetMarginTop(8); advBox.SetMarginBottom(8);
            advBox.SetMarginStart(8); advBox.SetMarginEnd(8);
            var lblAdv = Gtk.Label.New(
                "Configure filtering, joining, context lines and other subtitle options.");
            lblAdv.SetHalign(Gtk.Align.Start);
            advBox.Append(lblAdv);
            var btnAdvanced = Gtk.Button.NewWithLabel("Advanced...");
            btnAdvanced.SetHalign(Gtk.Align.Start);
            btnAdvanced.OnClicked += (s, e) =>
            {
                SaveSettings();
                var dlg = new DialogAdvancedSubtitleOptions(this)
                {
                    Subs1FilePattern = _txtSubs1.GetText(),
                    Subs2FilePattern = _txtSubs2.GetText(),
                    Subs1Encoding = GetSelectedEncodingLong(_comboEncodingSubs1, _encModel1),
                    Subs2Encoding = GetSelectedEncodingLong(_comboEncodingSubs2, _encModel2)
                };
                if (dlg.Run() == 1) // OK
                    dlg.SaveToSettings();
                dlg.Close();
            };
            advBox.Append(btnAdvanced);
            advFrame.SetChild(advBox);
            vbox.Append(advFrame);

            // MKV Extract
            var mkvFrame = Gtk.Frame.New("MKV Extract");
            var mkvBox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            mkvBox.SetMarginTop(8); mkvBox.SetMarginBottom(8);
            mkvBox.SetMarginStart(8); mkvBox.SetMarginEnd(8);
            var lblMkv = Gtk.Label.New(
                "Extract subtitle and audio tracks from MKV files.");
            lblMkv.SetHalign(Gtk.Align.Start);
            mkvBox.Append(lblMkv);
            var btnMkvExtract = Gtk.Button.NewWithLabel("MKV Extract...");
            btnMkvExtract.SetHalign(Gtk.Align.Start);
            btnMkvExtract.OnClicked += (s, e) =>
            {
                var dlg = new DialogMkvExtract(this);
                dlg.Run();
                dlg.Close();
            };
            mkvBox.Append(btnMkvExtract);
            mkvFrame.SetChild(mkvBox);
            vbox.Append(mkvFrame);

            return vbox;
        }

        // ── MAIN TAB ────────────────────────────────────────────────────────

        private Gtk.Widget BuildMainTab()
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 8);
            vbox.SetMarginTop(10); vbox.SetMarginBottom(10);
            vbox.SetMarginStart(10); vbox.SetMarginEnd(10);

            var grid = Gtk.Grid.New();
            grid.SetRowSpacing(6);
            grid.SetColumnSpacing(6);
            int row = 0;

            // Subs1
            AttachLabel(grid, "Subtitle 1:", 0, row);
            _txtSubs1 = Gtk.Entry.New();
            _txtSubs1.SetHexpand(true);
            grid.Attach(_txtSubs1, 1, row, 1, 1);
            var btnSubs1 = Gtk.Button.NewWithLabel("Browse...");
            btnSubs1.OnClicked += (s, e) =>
                SelectFileAsync("Select Subtitle 1", f => _txtSubs1.SetText(f));
            grid.Attach(btnSubs1, 2, row, 1, 1);
            row++;

            // Encoding Subs1
            AttachLabel(grid, "Subs1 Encoding:", 0, row);
            (_comboEncodingSubs1, _encModel1) = BuildEncodingDropDown();
            grid.Attach(_comboEncodingSubs1, 1, row, 2, 1);
            // Scroll wheel changes Subs1 encoding selection
            var encScroll1 = Gtk.EventControllerScroll.New(
                Gtk.EventControllerScrollFlags.Vertical);
            encScroll1.OnScroll += (ctrl, args) =>
            {
                uint count = _encModel1.GetNItems();
                if (count > 1)
                {
                    uint cur = _comboEncodingSubs1.GetSelected();
                    if (args.Dy > 0 && cur + 1 < count)
                        _comboEncodingSubs1.SetSelected(cur + 1);
                    else if (args.Dy < 0 && cur > 0)
                        _comboEncodingSubs1.SetSelected(cur - 1);
                }
                return true;
            };
            _comboEncodingSubs1.AddController(encScroll1);
            row++;

            // Subs2
            AttachLabel(grid, "Subtitle 2:", 0, row);
            _txtSubs2 = Gtk.Entry.New();
            _txtSubs2.SetHexpand(true);
            grid.Attach(_txtSubs2, 1, row, 1, 1);
            var btnSubs2 = Gtk.Button.NewWithLabel("Browse...");
            btnSubs2.OnClicked += (s, e) =>
                SelectFileAsync("Select Subtitle 2", f => _txtSubs2.SetText(f));
            grid.Attach(btnSubs2, 2, row, 1, 1);
            row++;

            // Encoding Subs2
            AttachLabel(grid, "Subs2 Encoding:", 0, row);
            (_comboEncodingSubs2, _encModel2) = BuildEncodingDropDown();
            grid.Attach(_comboEncodingSubs2, 1, row, 2, 1);
            // Scroll wheel changes Subs2 encoding selection
            var encScroll2 = Gtk.EventControllerScroll.New(
                Gtk.EventControllerScrollFlags.Vertical);
            encScroll2.OnScroll += (ctrl, args) =>
            {
                uint count = _encModel2.GetNItems();
                if (count > 1)
                {
                    uint cur = _comboEncodingSubs2.GetSelected();
                    if (args.Dy > 0 && cur + 1 < count)
                        _comboEncodingSubs2.SetSelected(cur + 1);
                    else if (args.Dy < 0 && cur > 0)
                        _comboEncodingSubs2.SetSelected(cur - 1);
                }
                return true;
            };
            _comboEncodingSubs2.AddController(encScroll2);
            row++;

            // Video
            AttachLabel(grid, "Video:", 0, row);
            _txtVideo = Gtk.Entry.New();
            _txtVideo.SetHexpand(true);
            _txtVideo.OnChanged += OnVideoChanged;
            grid.Attach(_txtVideo, 1, row, 1, 1);
            var btnVideo = Gtk.Button.NewWithLabel("Browse...");
            btnVideo.OnClicked += (s, e) =>
                SelectFileAsync("Select Video", f => _txtVideo.SetText(f));
            grid.Attach(btnVideo, 2, row, 1, 1);
            row++;

            // Audio Stream
            AttachLabel(grid, "Audio Stream:", 0, row);
            _audioStreamModel = Gtk.StringList.New(new[] { "0 - (Default)" });
            _comboAudioStream = Gtk.DropDown.New(_audioStreamModel, null);
            _comboAudioStream.SetSelected(0);
            grid.Attach(_comboAudioStream, 1, row, 2, 1);
            // Scroll wheel changes audio stream selection
            var audioStreamScroll = Gtk.EventControllerScroll.New(
                Gtk.EventControllerScrollFlags.Vertical);
            audioStreamScroll.OnScroll += (ctrl, args) =>
            {
                uint count = _audioStreamModel.GetNItems();
                if (count > 1)
                {
                    uint cur = _comboAudioStream.GetSelected();
                    if (args.Dy > 0 && cur + 1 < count)
                        _comboAudioStream.SetSelected(cur + 1);
                    else if (args.Dy < 0 && cur > 0)
                        _comboAudioStream.SetSelected(cur - 1);
                }
                return true; // consume the event
            };
            _comboAudioStream.AddController(audioStreamScroll);
            row++;

            // Output dir
            AttachLabel(grid, "Output Dir:", 0, row);
            _txtOutputDir = Gtk.Entry.New();
            _txtOutputDir.SetHexpand(true);
            grid.Attach(_txtOutputDir, 1, row, 1, 1);
            var btnOut = Gtk.Button.NewWithLabel("Browse...");
            btnOut.OnClicked += (s, e) =>
                SelectFolderAsync("Select Output Directory",
                    f => _txtOutputDir.SetText(f));
            grid.Attach(btnOut, 2, row, 1, 1);
            row++;

            // Deck name
            AttachLabel(grid, "Deck Name:", 0, row);
            _txtDeckName = Gtk.Entry.New();
            _txtDeckName.SetHexpand(true);
            grid.Attach(_txtDeckName, 1, row, 2, 1);
            row++;

            // Episode start
            AttachLabel(grid, "Episode Start #:", 0, row);
            _spinEpisodeStart = Gtk.SpinButton.NewWithRange(1, 9999, 1);
            _spinEpisodeStart.Value = 1;
            grid.Attach(_spinEpisodeStart, 1, row, 1, 1);

            // Episode end on same row — 0 means process all
            var endBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
            endBox.Append(Gtk.Label.New("End #:"));
            _spinEpisodeEnd = Gtk.SpinButton.NewWithRange(0, 9999, 1);
            _spinEpisodeEnd.Value = 0;
            var lblEndHint = Gtk.Label.New("(0 = all)");
            lblEndHint.SetOpacity(0.6);
            endBox.Append(_spinEpisodeEnd);
            endBox.Append(lblEndHint);
            grid.Attach(endBox, 2, row, 1, 1);
            row++;

            vbox.Append(grid);
            vbox.Append(Gtk.Separator.New(Gtk.Orientation.Horizontal));

            // Timing — radio group via SetGroup
            var timingFrame = Gtk.Frame.New("Use Timings From");
            var timingBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 10);
            timingBox.SetMarginTop(6); timingBox.SetMarginBottom(6);
            timingBox.SetMarginStart(6); timingBox.SetMarginEnd(6);
            _radioTimingSubs1 = Gtk.CheckButton.NewWithLabel("Subs 1");
            _radioTimingSubs2 = Gtk.CheckButton.NewWithLabel("Subs 2");
            _radioTimingSubs2.SetGroup(_radioTimingSubs1);
            _radioTimingSubs1.SetActive(true);
            timingBox.Append(_radioTimingSubs1);
            timingBox.Append(_radioTimingSubs2);
            timingFrame.SetChild(timingBox);
            vbox.Append(timingFrame);

            // Time shift
            _chkTimeShift = Gtk.CheckButton.NewWithLabel("Time Shift");
            var timeShiftBox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);

            var globalShiftRow = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
            timeShiftBox.SetVexpand(true);
            globalShiftRow.Append(_chkTimeShift);
            globalShiftRow.Append(Gtk.Label.New("Subs1 (ms):"));
            _spinTimeShiftSubs1 = Gtk.SpinButton.NewWithRange(-99999, 99999, 1);
            _spinTimeShiftSubs1.Value = 0;
            globalShiftRow.Append(_spinTimeShiftSubs1);
            globalShiftRow.Append(Gtk.Label.New("Subs2 (ms):"));
            _spinTimeShiftSubs2 = Gtk.SpinButton.NewWithRange(-99999, 99999, 1);
            _spinTimeShiftSubs2.Value = 0;
            globalShiftRow.Append(_spinTimeShiftSubs2);
            timeShiftBox.Append(globalShiftRow);

            // Per-episode shift rules — ColumnView with editable Entry cells.
            // Each cell Entry syncs to ShiftRuleItem via a mutable reference
            // wrapper that is nulled on unbind, preventing stale handler writes.
            var rulesFrame = Gtk.Frame.New("Per-Episode Shift Rules (cascading)");
            rulesFrame.SetVexpand(true);
            var rulesVBox = Gtk.Box.New(Gtk.Orientation.Vertical, 4);
            rulesVBox.SetMarginTop(4); rulesVBox.SetMarginBottom(4);
            rulesVBox.SetMarginStart(4); rulesVBox.SetMarginEnd(4);

            _shiftRulesStore = Gio.ListStore.New(Gtk.StringObject.GetGType());
            _shiftRulesSel = Gtk.SingleSelection.New(_shiftRulesStore);

            _shiftRulesColumnView = Gtk.ColumnView.New(_shiftRulesSel);
            _shiftRulesColumnView.SetShowColumnSeparators(true);
            _shiftRulesColumnView.SetShowRowSeparators(false);

            // Column: From Episode
            var colEp = CreateEditableShiftColumn("From Episode", 120,
                item => item.FromEpisode.ToString(),
                (item, text) => { if (int.TryParse(text, out int v)) item.FromEpisode = Math.Max(1, v); });
            _shiftRulesColumnView.AppendColumn(colEp);

            // Column: Subs1 Shift (ms)
            var colS1 = CreateEditableShiftColumn("Subs1 Shift (ms)", 140,
                item => item.Subs1Shift.ToString(),
                (item, text) => { if (int.TryParse(text, out int v)) item.Subs1Shift = v; });
            _shiftRulesColumnView.AppendColumn(colS1);

            // Column: Subs2 Shift (ms) — expand to fill remaining space
            var colS2 = CreateEditableShiftColumn("Subs2 Shift (ms)", -1,
                item => item.Subs2Shift.ToString(),
                (item, text) => { if (int.TryParse(text, out int v)) item.Subs2Shift = v; });
            GtkColumnViewHelper.SetExpand(colS2, true);
            _shiftRulesColumnView.AppendColumn(colS2);

            var rulesSw = Gtk.ScrolledWindow.New();
            rulesSw.SetChild(_shiftRulesColumnView);
            rulesSw.SetSizeRequest(-1, 120);
            // Allow the shift rules list to grow when the window is resized
            rulesSw.SetVexpand(true);
            rulesVBox.Append(rulesSw);

            // Defer header styling until widget tree is fully built.
            // IdleAdd runs after the current layout pass, ensuring
            // the ColumnView internal header children exist.
            GLib.Functions.IdleAdd(0, () =>
            {
                GtkColumnViewHelper.StyleColumnViewHeaders(
                    _shiftRulesColumnView,
                    "color: @theme_fg_color; opacity: 1.0; font-weight: 700;");
                return false; // run once
            });

            var rulesBtnBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
            var btnAddRule = Gtk.Button.NewWithLabel("Add Rule");
            btnAddRule.OnClicked += (s, e) =>
            {
                int nextEp = 1;
                if (_shiftItems.Count > 0)
                    nextEp = _shiftItems[_shiftItems.Count - 1].FromEpisode + 1;
                _shiftItems.Add(ShiftRuleItem.Create(nextEp, 0, 0));
                _shiftRulesStore.Append(Gtk.StringObject.New(""));
            };
            rulesBtnBox.Append(btnAddRule);

            var btnRemoveRule = Gtk.Button.NewWithLabel("Remove Selected");
            btnRemoveRule.OnClicked += (s, e) =>
            {
                uint sel = _shiftRulesSel.GetSelected();
                if (sel != Gtk.Constants.INVALID_LIST_POSITION && sel < _shiftItems.Count)
                {
                    _shiftItems.RemoveAt((int)sel);
                    _shiftRulesStore.Remove(sel);
                }
            };
            rulesBtnBox.Append(btnRemoveRule);
            rulesVBox.Append(rulesBtnBox);

            rulesFrame.SetChild(rulesVBox);
            timeShiftBox.Append(rulesFrame);
            vbox.Append(timeShiftBox);

            // Span
            _chkSpan = Gtk.CheckButton.NewWithLabel("Span (h:mm:ss)");
            var spanBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
            spanBox.Append(_chkSpan);
            spanBox.Append(Gtk.Label.New("Start:"));
            _txtSpanStart = Gtk.Entry.New();
            _txtSpanStart.SetText("0:01:30");
            _txtSpanStart.SetWidthChars(8);
            spanBox.Append(_txtSpanStart);
            spanBox.Append(Gtk.Label.New("End:"));
            _txtSpanEnd = Gtk.Entry.New();
            _txtSpanEnd.SetText("0:22:30");
            _txtSpanEnd.SetWidthChars(8);
            spanBox.Append(_txtSpanEnd);
            vbox.Append(spanBox);

            return vbox;
        }

        // ── AUDIO TAB ───────────────────────────────────────────────────────

        private Gtk.Widget BuildAudioTab()
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 8);
            vbox.SetMarginTop(10); vbox.SetMarginBottom(10);
            vbox.SetMarginStart(10); vbox.SetMarginEnd(10);

            _chkGenerateAudio = Gtk.CheckButton.NewWithLabel("Generate Audio Clips");
            _chkGenerateAudio.SetActive(true);
            vbox.Append(_chkGenerateAudio);
            vbox.Append(Gtk.Separator.New(Gtk.Orientation.Horizontal));

            // Source
            var sourceFrame = Gtk.Frame.New("Source");
            var sourceBox = Gtk.Box.New(Gtk.Orientation.Vertical, 4);
            sourceBox.SetMarginTop(6); sourceBox.SetMarginBottom(6);
            sourceBox.SetMarginStart(6); sourceBox.SetMarginEnd(6);

            _radioAudioFromVideo = Gtk.CheckButton.NewWithLabel(
                "Extract from video, bitrate:");
            _radioAudioExisting = Gtk.CheckButton.NewWithLabel(
                "Use existing audio file:");
            _radioAudioExisting.SetGroup(_radioAudioFromVideo);
            _radioAudioFromVideo.SetActive(true);

            var bitrateBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
            bitrateBox.Append(_radioAudioFromVideo);
            _audioBitrateModel = Gtk.StringList.New(BitrateOptions);
            _comboAudioBitrate = Gtk.DropDown.New(_audioBitrateModel, null);
            _comboAudioBitrate.SetSelected(3); // 128
            bitrateBox.Append(_comboAudioBitrate);
            bitrateBox.Append(Gtk.Label.New("kbps"));
            sourceBox.Append(bitrateBox);

            var existingBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
            existingBox.Append(_radioAudioExisting);
            _txtAudioFile = Gtk.Entry.New();
            _txtAudioFile.SetHexpand(true);
            _txtAudioFile.SetSensitive(false);
            existingBox.Append(_txtAudioFile);
            _btnAudioBrowse = Gtk.Button.NewWithLabel("Browse...");
            _btnAudioBrowse.SetSensitive(false);
            _btnAudioBrowse.OnClicked += (s, e) =>
                SelectFileAsync("Select Audio File",
                    f => _txtAudioFile.SetText(f));
            existingBox.Append(_btnAudioBrowse);
            sourceBox.Append(existingBox);

            _radioAudioFromVideo.OnToggled += (s, e) =>
            {
                bool fromVideo = _radioAudioFromVideo.GetActive();
                _comboAudioBitrate.SetSensitive(fromVideo);
                _txtAudioFile.SetSensitive(!fromVideo);
                _btnAudioBrowse.SetSensitive(!fromVideo);
            };

            sourceFrame.SetChild(sourceBox);
            vbox.Append(sourceFrame);

            // Pad timings
            _chkAudioPad = Gtk.CheckButton.NewWithLabel("Pad Timings");
            var padBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
            padBox.Append(_chkAudioPad);
            padBox.Append(Gtk.Label.New("Start (ms):"));
            _spinAudioPadStart = Gtk.SpinButton.NewWithRange(0, 9999, 50);
            _spinAudioPadStart.Value = 250;
            padBox.Append(_spinAudioPadStart);
            padBox.Append(Gtk.Label.New("End (ms):"));
            _spinAudioPadEnd = Gtk.SpinButton.NewWithRange(0, 9999, 50);
            _spinAudioPadEnd.Value = 250;
            padBox.Append(_spinAudioPadEnd);
            vbox.Append(padBox);

            _chkNormalize = Gtk.CheckButton.NewWithLabel(
                "Normalize audio (requires mp3gain)");
            vbox.Append(_chkNormalize);

            return vbox;
        }

        // ── SNAPSHOT TAB ────────────────────────────────────────────────────

        private Gtk.Widget BuildSnapshotTab()
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 8);
            vbox.SetMarginTop(10); vbox.SetMarginBottom(10);
            vbox.SetMarginStart(10); vbox.SetMarginEnd(10);

            _chkGenerateSnapshots = Gtk.CheckButton.NewWithLabel("Generate Snapshots");
            _chkGenerateSnapshots.SetActive(true);
            vbox.Append(_chkGenerateSnapshots);
            vbox.Append(Gtk.Separator.New(Gtk.Orientation.Horizontal));

            var grid = Gtk.Grid.New();
            grid.SetRowSpacing(6); grid.SetColumnSpacing(6);

            AttachLabel(grid, "Width:", 0, 0);
            _spinSnapshotWidth = Gtk.SpinButton.NewWithRange(16, 3840, 16);
            _spinSnapshotWidth.Value = 240;
            grid.Attach(_spinSnapshotWidth, 1, 0, 1, 1);
            grid.Attach(Gtk.Label.New("px"), 2, 0, 1, 1);

            AttachLabel(grid, "Height:", 0, 1);
            _spinSnapshotHeight = Gtk.SpinButton.NewWithRange(16, 2160, 2);
            _spinSnapshotHeight.Value = 160;
            grid.Attach(_spinSnapshotHeight, 1, 1, 1, 1);
            grid.Attach(Gtk.Label.New("px"), 2, 1, 1, 1);

            AttachLabel(grid, "Crop Bottom:", 0, 2);
            _spinSnapshotCropBottom = Gtk.SpinButton.NewWithRange(0, 2160, 2);
            _spinSnapshotCropBottom.Value = 0;
            grid.Attach(_spinSnapshotCropBottom, 1, 2, 1, 1);
            grid.Attach(Gtk.Label.New("px"), 2, 2, 1, 1);

            AttachLabel(grid, "JPEG Quality:", 0, 3);
            _spinSnapshotQuality = Gtk.SpinButton.NewWithRange(1, 31, 1);
            _spinSnapshotQuality.Value = 3;
            grid.Attach(_spinSnapshotQuality, 1, 3, 1, 1);
            var lblQual = Gtk.Label.New("(1 = best, 5 = good, 15 = low)");
            lblQual.SetHalign(Gtk.Align.Start);
            grid.Attach(lblQual, 2, 3, 1, 1);

            vbox.Append(grid);
            return vbox;
        }

        // ── VIDEO CLIP TAB ──────────────────────────────────────────────────

        private Gtk.Widget BuildVideoTab()
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 8);
            vbox.SetMarginTop(10); vbox.SetMarginBottom(10);
            vbox.SetMarginStart(10); vbox.SetMarginEnd(10);

            _chkGenerateVideo = Gtk.CheckButton.NewWithLabel("Generate Video Clips");
            vbox.Append(_chkGenerateVideo);
            vbox.Append(Gtk.Separator.New(Gtk.Orientation.Horizontal));

            var grid = Gtk.Grid.New();
            grid.SetRowSpacing(6); grid.SetColumnSpacing(6);
            int row = 0;

            AttachLabel(grid, "Width:", 0, row);
            _spinVideoWidth = Gtk.SpinButton.NewWithRange(16, 3840, 16);
            _spinVideoWidth.Value = 240;
            grid.Attach(_spinVideoWidth, 1, row, 1, 1);
            grid.Attach(Gtk.Label.New("px"), 2, row, 1, 1);
            row++;

            AttachLabel(grid, "Height:", 0, row);
            _spinVideoHeight = Gtk.SpinButton.NewWithRange(16, 2160, 2);
            _spinVideoHeight.Value = 160;
            grid.Attach(_spinVideoHeight, 1, row, 1, 1);
            grid.Attach(Gtk.Label.New("px"), 2, row, 1, 1);
            row++;

            AttachLabel(grid, "Crop Bottom:", 0, row);
            _spinVideoCropBottom = Gtk.SpinButton.NewWithRange(0, 2160, 2);
            _spinVideoCropBottom.Value = 0;
            grid.Attach(_spinVideoCropBottom, 1, row, 1, 1);
            grid.Attach(Gtk.Label.New("px"), 2, row, 1, 1);
            row++;

            AttachLabel(grid, "Video Bitrate:", 0, row);
            _spinVideoBitrateVideo = Gtk.SpinButton.NewWithRange(100, 10000, 100);
            _spinVideoBitrateVideo.Value = 800;
            grid.Attach(_spinVideoBitrateVideo, 1, row, 1, 1);
            grid.Attach(Gtk.Label.New("kb/s"), 2, row, 1, 1);
            row++;

            AttachLabel(grid, "Audio Bitrate:", 0, row);
            _videoBitrateModel = Gtk.StringList.New(BitrateOptions);
            _comboVideoBitrateAudio = Gtk.DropDown.New(_videoBitrateModel, null);
            _comboVideoBitrateAudio.SetSelected(3); // 128
            grid.Attach(_comboVideoBitrateAudio, 1, row, 1, 1);
            grid.Attach(Gtk.Label.New("kb/s"), 2, row, 1, 1);
            row++;

            vbox.Append(grid);

            // Pad
            _chkVideoPad = Gtk.CheckButton.NewWithLabel("Pad Timings");
            var padBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
            padBox.Append(_chkVideoPad);
            padBox.Append(Gtk.Label.New("Start (ms):"));
            _spinVideoPadStart = Gtk.SpinButton.NewWithRange(0, 9999, 50);
            _spinVideoPadStart.Value = 250;
            padBox.Append(_spinVideoPadStart);
            padBox.Append(Gtk.Label.New("End (ms):"));
            _spinVideoPadEnd = Gtk.SpinButton.NewWithRange(0, 9999, 50);
            _spinVideoPadEnd.Value = 250;
            padBox.Append(_spinVideoPadEnd);
            vbox.Append(padBox);

            _chkIPod = Gtk.CheckButton.NewWithLabel("iPod/iPhone support");
            vbox.Append(_chkIPod);

            return vbox;
        }

        // ═══════════════════════════════════════════════════════════════════
        //  SETTINGS
        // ═══════════════════════════════════════════════════════════════════

        private void LoadSettings()
        {
            PrefIO.read();
            Settings.Instance.reset();
            PopulateUIFromSettings();
        }

        private void PopulateUIFromSettings()
        {
            var s = Settings.Instance;

            // ── Main tab
            _txtDeckName.SetText(s.DeckName != "" ? s.DeckName : "MyDeck");
            _txtOutputDir.SetText(s.OutputDir != ""
                ? s.OutputDir
                : ConstantSettings.DefaultOutputDir != ""
                    ? ConstantSettings.DefaultOutputDir
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            _spinEpisodeStart.Value = s.EpisodeStartNumber;
            _spinEpisodeEnd.Value = s.EpisodeEndNumber;

            _txtSubs1.SetText(s.Subs[0].FilePattern);
            _txtSubs2.SetText(s.Subs[1].FilePattern);
            _txtVideo.SetText(s.VideoClips.FilePattern);

            SetEncodingDropDown(_comboEncodingSubs1, _encModel1, s.Subs[0].Encoding);
            SetEncodingDropDown(_comboEncodingSubs2, _encModel2, s.Subs[1].Encoding);

            if (s.Subs[1].TimingsEnabled)
                _radioTimingSubs2.SetActive(true);
            else
                _radioTimingSubs1.SetActive(true);

            _chkTimeShift.SetActive(s.TimeShiftEnabled);
            _spinTimeShiftSubs1.Value = s.Subs[0].TimeShift;
            _spinTimeShiftSubs2.Value = s.Subs[1].TimeShift;

            // Per-episode shift rules
            _shiftRulesStore.RemoveAll();
            _shiftItems.Clear();
            if (s.Subs[0].TimeShiftRules?.Count > 0)
            {
                for (int i = 0; i < s.Subs[0].TimeShiftRules.Count; i++)
                {
                    int fromEp = s.Subs[0].TimeShiftRules[i].FromEpisode;
                    int s1 = s.Subs[0].TimeShiftRules[i].ShiftMs;
                    int s2 = (s.Subs[1].TimeShiftRules?.Count > i)
                        ? s.Subs[1].TimeShiftRules[i].ShiftMs : 0;
                    _shiftItems.Add(ShiftRuleItem.Create(fromEp, s1, s2));
                    _shiftRulesStore.Append(Gtk.StringObject.New(""));
                }
            }

            _chkSpan.SetActive(s.SpanEnabled);
            _txtSpanStart.SetText(s.SpanStart.ToString(@"h\:mm\:ss"));
            _txtSpanEnd.SetText(s.SpanEnd.ToString(@"h\:mm\:ss"));

            // ── Audio tab
            _chkGenerateAudio.SetActive(s.AudioClips.Enabled);
            if (s.AudioClips.UseExistingAudio)
                _radioAudioExisting.SetActive(true);
            else
                _radioAudioFromVideo.SetActive(true);
            _txtAudioFile.SetText(s.AudioClips.FilePattern);
            SetBitrateDropDown(_comboAudioBitrate, _audioBitrateModel, s.AudioClips.Bitrate);
            _chkAudioPad.SetActive(s.AudioClips.PadEnabled);
            _spinAudioPadStart.Value = s.AudioClips.PadStart;
            _spinAudioPadEnd.Value = s.AudioClips.PadEnd;
            _chkNormalize.SetActive(s.AudioClips.Normalize);

            // ── Snapshot tab
            _chkGenerateSnapshots.SetActive(s.Snapshots.Enabled);
            _spinSnapshotWidth.Value = s.Snapshots.Size.Width > 0 ? s.Snapshots.Size.Width : 240;
            _spinSnapshotHeight.Value = s.Snapshots.Size.Height > 0 ? s.Snapshots.Size.Height : 160;
            _spinSnapshotCropBottom.Value = s.Snapshots.Crop.Bottom;
            _spinSnapshotQuality.Value = s.Snapshots.Quality > 0 ? s.Snapshots.Quality : 3;

            // ── Video tab
            _chkGenerateVideo.SetActive(s.VideoClips.Enabled);
            _spinVideoWidth.Value = s.VideoClips.Size.Width > 0 ? s.VideoClips.Size.Width : 240;
            _spinVideoHeight.Value = s.VideoClips.Size.Height > 0 ? s.VideoClips.Size.Height : 160;
            _spinVideoCropBottom.Value = s.VideoClips.Crop.Bottom;
            _spinVideoBitrateVideo.Value = s.VideoClips.BitrateVideo > 0 ? s.VideoClips.BitrateVideo : 800;
            SetBitrateDropDown(_comboVideoBitrateAudio, _videoBitrateModel, s.VideoClips.BitrateAudio);
            _chkVideoPad.SetActive(s.VideoClips.PadEnabled);
            _spinVideoPadStart.Value = s.VideoClips.PadStart;
            _spinVideoPadEnd.Value = s.VideoClips.PadEnd;
            _chkIPod.SetActive(s.VideoClips.IPodSupport);

            SetDefaultSize(ConstantSettings.MainWindowWidth, ConstantSettings.MainWindowHeight);
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            var projectPath = Settings.Instance.ProjectPath;
            if (!string.IsNullOrEmpty(projectPath))
                SetTitle($"subs2srs — {System.IO.Path.GetFileNameWithoutExtension(projectPath)}");
            else
                SetTitle("subs2srs");
        }

        private void SaveSettings()
        {
            try
            {
                Settings.Instance.DeckName = _txtDeckName.GetText().Trim();
                Settings.Instance.OutputDir = _txtOutputDir.GetText().Trim();
                string newOutputDir = Settings.Instance.OutputDir;
                if (newOutputDir.Length > 0 && newOutputDir != ConstantSettings.DefaultOutputDir)
                {
                    ConstantSettings.DefaultOutputDir = newOutputDir;
                    PrefIO.Write();
                }
                Settings.Instance.EpisodeStartNumber = (int)_spinEpisodeStart.Value;
                Settings.Instance.EpisodeEndNumber = (int)_spinEpisodeEnd.Value;

                // Subs
                Settings.Instance.Subs[0].FilePattern = _txtSubs1.GetText().Trim();
                Settings.Instance.Subs[0].Encoding = GetSelectedEncodingShort(_comboEncodingSubs1, _encModel1);
                Settings.Instance.Subs[0].TimingsEnabled = _radioTimingSubs1.GetActive();
                Settings.Instance.Subs[0].TimeShift = (int)_spinTimeShiftSubs1.Value;
                Settings.Instance.Subs[0].Files = UtilsSubs.getSubsFiles(
                    Settings.Instance.Subs[0].FilePattern).ToArray();

                Settings.Instance.Subs[1].FilePattern = _txtSubs2.GetText().Trim();
                Settings.Instance.Subs[1].Encoding = GetSelectedEncodingShort(_comboEncodingSubs2, _encModel2);
                Settings.Instance.Subs[1].TimingsEnabled = _radioTimingSubs2.GetActive();
                Settings.Instance.Subs[1].TimeShift = (int)_spinTimeShiftSubs2.Value;
                if (Settings.Instance.Subs[1].FilePattern.Length > 0)
                    Settings.Instance.Subs[1].Files = UtilsSubs.getSubsFiles(
                        Settings.Instance.Subs[1].FilePattern).ToArray();
                else
                    Settings.Instance.Subs[1].Files = Array.Empty<string>();

                Settings.Instance.TimeShiftEnabled = _chkTimeShift.GetActive();

                // Per-episode shift rules
                Settings.Instance.Subs[0].TimeShiftRules.Clear();
                Settings.Instance.Subs[1].TimeShiftRules.Clear();
                for (int i = 0; i < _shiftItems.Count; i++)
                {
                    var rule = _shiftItems[i];
                    Settings.Instance.Subs[0].TimeShiftRules.Add(
                        new TimeShiftRule(rule.FromEpisode, rule.Subs1Shift));
                    Settings.Instance.Subs[1].TimeShiftRules.Add(
                        new TimeShiftRule(rule.FromEpisode, rule.Subs2Shift));
                }
                Settings.Instance.Subs[0].TimeShiftRules.Sort(
                    (a, b) => a.FromEpisode.CompareTo(b.FromEpisode));
                Settings.Instance.Subs[1].TimeShiftRules.Sort(
                    (a, b) => a.FromEpisode.CompareTo(b.FromEpisode));

                Settings.Instance.SpanEnabled = _chkSpan.GetActive();
                if (_chkSpan.GetActive())
                {
                    Settings.Instance.SpanStart = UtilsSubs.stringToTime(
                        _txtSpanStart.GetText().Trim());
                    Settings.Instance.SpanEnd = UtilsSubs.stringToTime(
                        _txtSpanEnd.GetText().Trim());
                }

                // Video
                Settings.Instance.VideoClips.FilePattern = _txtVideo.GetText().Trim();
                Settings.Instance.VideoClips.Files = UtilsCommon.getNonHiddenFiles(
                    Settings.Instance.VideoClips.FilePattern);

                // Audio stream
                int streamIdx = (int)_comboAudioStream.GetSelected();
                if (streamIdx >= 0 && streamIdx < _audioStreams.Count)
                    Settings.Instance.VideoClips.AudioStream = _audioStreams[streamIdx];
                else
                    Settings.Instance.VideoClips.AudioStream =
                        new InfoStream("0:a:0", "0", "", "");

                // Audio
                Settings.Instance.AudioClips.Enabled = _chkGenerateAudio.GetActive();
                Settings.Instance.AudioClips.UseAudioFromVideo = _radioAudioFromVideo.GetActive();
                Settings.Instance.AudioClips.UseExistingAudio = _radioAudioExisting.GetActive();
                Settings.Instance.AudioClips.Bitrate = GetSelectedBitrate(
                    _comboAudioBitrate, _audioBitrateModel, 128);
                Settings.Instance.AudioClips.PadEnabled = _chkAudioPad.GetActive();
                Settings.Instance.AudioClips.PadStart = (int)_spinAudioPadStart.Value;
                Settings.Instance.AudioClips.PadEnd = (int)_spinAudioPadEnd.Value;
                Settings.Instance.AudioClips.Normalize = _chkNormalize.GetActive();
                Settings.Instance.AudioClips.FilePattern = _txtAudioFile.GetText().Trim();
                Settings.Instance.AudioClips.Files = UtilsCommon.getNonHiddenFiles(
                    Settings.Instance.AudioClips.FilePattern);

                // Snapshots
                Settings.Instance.Snapshots.Enabled = _chkGenerateSnapshots.GetActive();
                Settings.Instance.Snapshots.Size.Width = (int)_spinSnapshotWidth.Value;
                Settings.Instance.Snapshots.Size.Height = (int)_spinSnapshotHeight.Value;
                Settings.Instance.Snapshots.Crop.Bottom = (int)_spinSnapshotCropBottom.Value;
                Settings.Instance.Snapshots.Quality = (int)_spinSnapshotQuality.Value;

                // Video clips
                Settings.Instance.VideoClips.Enabled = _chkGenerateVideo.GetActive();
                Settings.Instance.VideoClips.Size.Width = (int)_spinVideoWidth.Value;
                Settings.Instance.VideoClips.Size.Height = (int)_spinVideoHeight.Value;
                Settings.Instance.VideoClips.Crop.Bottom = (int)_spinVideoCropBottom.Value;
                Settings.Instance.VideoClips.BitrateVideo = (int)_spinVideoBitrateVideo.Value;
                Settings.Instance.VideoClips.BitrateAudio = GetSelectedBitrate(
                    _comboVideoBitrateAudio, _videoBitrateModel, 128);
                Settings.Instance.VideoClips.PadEnabled = _chkVideoPad.GetActive();
                Settings.Instance.VideoClips.PadStart = (int)_spinVideoPadStart.Value;
                Settings.Instance.VideoClips.PadEnd = (int)_spinVideoPadEnd.Value;
                Settings.Instance.VideoClips.IPodSupport = _chkIPod.GetActive();

                // Truncate file arrays when Episode End # limits processing
                int endNum = Settings.Instance.EpisodeEndNumber;
                int startNum = Settings.Instance.EpisodeStartNumber;
                if (endNum > 0 && endNum >= startNum)
                {
                    int maxCount = endNum - startNum + 1;

                    if (Settings.Instance.Subs[0].Files.Length > maxCount)
                        Settings.Instance.Subs[0].Files =
                            Settings.Instance.Subs[0].Files.Take(maxCount).ToArray();

                    if (Settings.Instance.Subs[1].Files.Length > maxCount)
                        Settings.Instance.Subs[1].Files =
                            Settings.Instance.Subs[1].Files.Take(maxCount).ToArray();

                    if (Settings.Instance.VideoClips.Files.Length > maxCount)
                        Settings.Instance.VideoClips.Files =
                            Settings.Instance.VideoClips.Files.Take(maxCount).ToArray();

                    if (Settings.Instance.AudioClips.Files.Length > maxCount)
                        Settings.Instance.AudioClips.Files =
                            Settings.Instance.AudioClips.Files.Take(maxCount).ToArray();
                }
            }
            catch (Exception e1)
            {
                UtilsMsg.showErrMsg(
                    "Something went wrong while gathering interface data:\n" + e1);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  EVENT HANDLERS
        // ═══════════════════════════════════════════════════════════════════

        private async void OnVideoChanged(Gtk.Editable sender, EventArgs e)
        {
            string pattern = _txtVideo.GetText().Trim();
            if (string.IsNullOrEmpty(pattern)) return;

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
            var items = new List<string>();
            if (_audioStreams.Count > 0)
                foreach (var s in _audioStreams) items.Add(s.ToString());
            else
            {
                _audioStreams.Add(new InfoStream("0:a:0", "0", "", "Default"));
                items.Add("0 - (Default)");
            }
            _audioStreamModel = Gtk.StringList.New(items.ToArray());
            _comboAudioStream.SetModel(_audioStreamModel);
            _comboAudioStream.SetSelected(0);
        }

        private async void OnGoClicked(object? sender, EventArgs e)
        {
            if (!_btnGo.GetSensitive()) return;

            if (string.IsNullOrWhiteSpace(_txtSubs1.GetText()))
            {
                UtilsMsg.showErrMsg("Please provide at least Subtitle 1.");
                return;
            }
            if (string.IsNullOrWhiteSpace(_txtOutputDir.GetText()))
            {
                UtilsMsg.showErrMsg("Please provide Output Directory.");
                return;
            }
            if (string.IsNullOrWhiteSpace(_txtDeckName.GetText()))
            {
                UtilsMsg.showErrMsg("Please provide Deck Name.");
                return;
            }

            SaveSettings();

            bool needsAudioFromVideo =
                (Settings.Instance.AudioClips.Enabled
                    && Settings.Instance.AudioClips.UseAudioFromVideo)
                || Settings.Instance.VideoClips.Enabled;

            if (needsAudioFromVideo
                && Settings.Instance.VideoClips.Files?.Length > 1)
            {
                int streamIdx = (int)_comboAudioStream.GetSelected();
                if (streamIdx < 0) streamIdx = 0;
                var files = Settings.Instance.VideoClips.Files;
                string warning = await Task.Run(() =>
                    UtilsVideo.validateAudioStreamConsistency(files, streamIdx));
                if (warning != null)
                {
                    if (!UtilsMsg.showConfirm(warning))
                        return;
                }
            }

            _btnGo.SetSensitive(false);
            _btnCancel.SetSensitive(true);

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

            _btnGo.SetSensitive(true);
            _btnCancel.SetSensitive(false);
            if (_reporter != null)
            {
                _progressBar.SetText(_reporter.Cancel ? "Cancelled" : "Finished!");
                _progressBar.SetFraction(_reporter.Cancel ? 0.0 : 1.0);
            }
        }

        private void OnAboutClicked(Gtk.Button sender, EventArgs e)
        {
            var dlg = Gtk.Window.New();
            dlg.SetTitle("About subs2srs");
            dlg.SetDefaultSize(400, 300);
            dlg.SetModal(true);
            dlg.SetTransientFor(this);

            var box = Gtk.Box.New(Gtk.Orientation.Vertical, 8);
            box.SetMarginTop(20); box.SetMarginBottom(20);
            box.SetMarginStart(20); box.SetMarginEnd(20);

            box.Append(Gtk.Label.New(UtilsAssembly.Title));
            box.Append(Gtk.Label.New($"Version {UtilsAssembly.Version}"));
            box.Append(Gtk.Label.New(UtilsAssembly.Product));
            box.Append(Gtk.Label.New("Original author: Christopher Brochtrup"));
            box.Append(Gtk.Label.New(UtilsAssembly.Copyright));
            box.Append(Gtk.Label.New("GNU General Public License v3"));

            // Source code link
            var linkLabel = Gtk.Label.New(null);
            linkLabel.SetMarkup(
                "<a href=\"https://gitlab.com/fkzys/subs2srs\">"
                + "https://gitlab.com/fkzys/subs2srs</a>");
            linkLabel.SetUseMarkup(true);
            box.Append(linkLabel);

            var btn = Gtk.Button.NewWithLabel("OK");
            btn.SetHalign(Gtk.Align.Center);
            btn.OnClicked += (s2, e2) => dlg.Close();
            box.Append(btn);

            dlg.SetChild(box);
            dlg.Show();
        }

        // ── SAVE / LOAD PROJECT ─────────────────────────────────────────────

        private async void OnSaveProject(Gtk.Button sender, EventArgs e)
        {
            SaveSettings();

            var dlg = Gtk.FileDialog.New();
            dlg.SetTitle("Save Project");

            var filter = Gtk.FileFilter.New();
            filter.SetName("subs2srs Project (*.s2s.json)");
            filter.AddPattern("*.s2s.json");
            var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
            filters.Append(filter);
            dlg.SetFilters(filters);

            string initName = !string.IsNullOrEmpty(Settings.Instance.DeckName)
                ? Settings.Instance.DeckName + ".s2s.json"
                : "project.s2s.json";
            dlg.SetInitialName(initName);

            try
            {
                var file = await dlg.SaveAsync(this);
                if (file == null) return;
                string path = file.GetPath() ?? "";
                if (path == "") return;
                if (!path.EndsWith(".s2s.json", StringComparison.OrdinalIgnoreCase))
                    path += ".s2s.json";
                try
                {
                    ProjectIO.Save(path, Settings.Instance);
                    Settings.Instance.ProjectPath = path;
                    UpdateTitle();
                }
                catch (Exception ex)
                {
                    UtilsMsg.showErrMsg($"Failed to save project:\n{ex.Message}");
                }
            }
            catch { /* user cancelled */ }
        }

        private async void OnLoadProject(Gtk.Button sender, EventArgs e)
        {
            var dlg = Gtk.FileDialog.New();
            dlg.SetTitle("Load Project");

            var filter = Gtk.FileFilter.New();
            filter.SetName("subs2srs Project (*.s2s.json)");
            filter.AddPattern("*.s2s.json");
            var allFilter = Gtk.FileFilter.New();
            allFilter.SetName("All files");
            allFilter.AddPattern("*");
            var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
            filters.Append(filter);
            filters.Append(allFilter);
            dlg.SetFilters(filters);

            try
            {
                var file = await dlg.OpenAsync(this);
                if (file == null) return;
                string path = file.GetPath() ?? "";
                if (path == "") return;
                try
                {
                    ProjectIO.Load(path);
                    Settings.Instance.ProjectPath = path;
                    PopulateUIFromSettings();
                }
                catch (Exception ex)
                {
                    UtilsMsg.showErrMsg($"Failed to load project:\n{ex.Message}");
                }
            }
            catch { /* user cancelled */ }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Mutable reference wrapper used to connect Entry change handlers
        /// to the current ShiftRuleItem. On unbind, Target is set to null
        /// so that accumulated handlers from previous binds become no-ops.
        /// </summary>
        private class ShiftRuleRef
        {
            public ShiftRuleItem? Target;
        }

        /// <summary>
        /// Create a ColumnViewColumn with an editable Entry cell for shift rules.
        /// Uses ShiftRuleRef stored in _shiftRefMap to safely handle
        /// OnBind/OnUnbind without leaking stale handlers.
        /// OnChanged is installed once in OnSetup; it writes through
        /// refHolder.Target which is swapped on each bind cycle.
        /// </summary>
        private Gtk.ColumnViewColumn CreateEditableShiftColumn(
            string title, int fixedWidth,
            Func<ShiftRuleItem, string> getter,
            Action<ShiftRuleItem, string> setter)
        {
            var factory = Gtk.SignalListItemFactory.New();

            factory.OnSetup += (f, args) =>
            {
                var li = (Gtk.ListItem)args.Object;
                var entry = Gtk.Entry.New();
                entry.SetWidthChars(10);
                entry.SetHexpand(true);

                // Create and register the mutable reference holder
                var refHolder = new ShiftRuleRef();
                var handle = entry.Handle.DangerousGetHandle();
                _shiftRefMap[handle] = refHolder;

                // Single handler installed once — survives all rebinds.
                // Writes through refHolder.Target, updated on each bind.
                entry.OnChanged += (s, ev) =>
                {
                    if (refHolder.Target != null)
                        setter(refHolder.Target, entry.GetText());
                };

                li.SetChild(entry);
            };

            factory.OnBind += (f, args) =>
            {
                var li = (Gtk.ListItem)args.Object;
                uint pos = li.GetPosition();
                if (pos >= _shiftItems.Count) return;

                var item = _shiftItems[(int)pos];
                var entry = (Gtk.Entry?)li.GetChild();
                if (entry == null) return;

                // Point the reference holder to the current item
                var handle = entry.Handle.DangerousGetHandle();
                if (_shiftRefMap.TryGetValue(handle, out var refHolder))
                {
                    refHolder.Target = item;
                }

                entry.SetText(getter(item));
            };

            factory.OnUnbind += (f, args) =>
            {
                var li = (Gtk.ListItem)args.Object;
                var entry = (Gtk.Entry?)li.GetChild();
                if (entry == null) return;

                // Null out the reference so stale handlers become no-ops
                var handle = entry.Handle.DangerousGetHandle();
                if (_shiftRefMap.TryGetValue(handle, out var refHolder))
                {
                    refHolder.Target = null;
                }
            };

            var col = Gtk.ColumnViewColumn.New(title, factory);

            if (fixedWidth > 0)
            {
                GtkColumnViewHelper.SetFixedWidth(col, fixedWidth);
                GtkColumnViewHelper.SetResizable(col, true);
                GtkColumnViewHelper.SetExpand(col, false);
            }

            return col;
        }

        /// <summary>Attach a right-aligned label to a grid cell.</summary>
        private void AttachLabel(Gtk.Grid grid, string text, int col, int row)
        {
            var lbl = Gtk.Label.New(text);
            lbl.SetHalign(Gtk.Align.End);
            grid.Attach(lbl, col, row, 1, 1);
        }

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

        private string GetSelectedEncodingShort(Gtk.DropDown dd, Gtk.StringList model)
        {
            uint sel = dd.GetSelected();
            var encodings = InfoEncoding.getEncodings();
            int i = 0;
            foreach (var enc in encodings)
            {
                if (i == (int)sel) return enc.ShortName;
                i++;
            }
            return "utf-8";
        }

        private string GetSelectedEncodingLong(Gtk.DropDown dd, Gtk.StringList model)
        {
            uint sel = dd.GetSelected();
            if (sel < model.GetNItems())
                return model.GetString(sel) ?? "";
            return "";
        }

        private void SetEncodingDropDown(Gtk.DropDown dd, Gtk.StringList model,
            string shortName)
        {
            int i = 0;
            foreach (var enc in InfoEncoding.getEncodings())
            {
                if (enc.ShortName == shortName) { dd.SetSelected((uint)i); return; }
                i++;
            }
            dd.SetSelected(0);
        }

        private int GetSelectedBitrate(Gtk.DropDown dd, Gtk.StringList model,
            int defaultVal)
        {
            uint sel = dd.GetSelected();
            if (sel < model.GetNItems())
            {
                string text = model.GetString(sel);
                if (int.TryParse(text, out int val)) return val;
            }
            return defaultVal;
        }

        private void SetBitrateDropDown(Gtk.DropDown dd, Gtk.StringList model,
            int bitrate)
        {
            string target = bitrate.ToString();
            for (uint i = 0; i < model.GetNItems(); i++)
            {
                if (model.GetString(i) == target) { dd.SetSelected(i); return; }
            }
            dd.SetSelected(3); // fallback: 128
        }

        // ── FILE DIALOGS (GTK4 async) ───────────────────────────────────────

        private async void SelectFileAsync(string title, System.Action<string> onSelected)
        {
            var dlg = Gtk.FileDialog.New();
            dlg.SetTitle(title);
            try
            {
                var file = await dlg.OpenAsync(this);
                if (file != null)
                {
                    string path = file.GetPath() ?? "";
                    if (path != "") onSelected(path);
                }
            }
            catch { }
        }

        private async void SelectFolderAsync(string title, System.Action<string> onSelected)
        {
            var dlg = Gtk.FileDialog.New();
            dlg.SetTitle(title);
            try
            {
                var file = await dlg.SelectFolderAsync(this);
                if (file != null)
                {
                    string path = file.GetPath() ?? "";
                    if (path != "") onSelected(path);
                }
            }
            catch { }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  PROGRESS REPORTER
        // ═══════════════════════════════════════════════════════════════════

        private class GtkProgressReporter : IProgressReporter
        {
            private readonly Gtk.ProgressBar _bar;
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

            public GtkProgressReporter(Gtk.ProgressBar bar)
            {
                _bar = bar;
                GLib.Functions.TimeoutAdd(0, 50, OnPoll);
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
                    if (text != null) _bar.SetText(text);
                    if (frac >= 0) _bar.SetFraction(frac);
                }
                return _active;
            }

            public void NextStep(int step, string description)
            {
                lock (_sync)
                {
                    _text = $"[{step}/{StepsTotal}] {description}";
                    _fraction = 0.0;
                    _dirty = true;
                }
            }

            public void UpdateProgress(int percent, string text)
            {
                lock (_sync)
                {
                    _text = text;
                    _fraction = Math.Max(0, Math.Min(1, percent / 100.0));
                    _dirty = true;
                }
            }

            public void UpdateProgress(string text)
            {
                lock (_sync) { _text = text; _dirty = true; }
            }

            public void EnableDetail(bool enable) { }
            public void SetDuration(TimeSpan duration) { }
            public void OnFFmpegOutput(object sender,
                System.Diagnostics.DataReceivedEventArgs e) { }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ShiftRuleItem — plain data holder for per-episode shift rules list
    // ═══════════════════════════════════════════════════════════════════════

    public class ShiftRuleItem
    {
        public int FromEpisode { get; set; } = 1;
        public int Subs1Shift { get; set; }
        public int Subs2Shift { get; set; }

        public static ShiftRuleItem Create(int fromEp, int s1, int s2) =>
            new ShiftRuleItem { FromEpisode = fromEp, Subs1Shift = s1, Subs2Shift = s2 };
    }
}
