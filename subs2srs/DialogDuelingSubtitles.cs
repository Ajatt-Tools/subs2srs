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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IOPath = System.IO.Path;

namespace subs2srs
{
    /// <summary>
    /// Dueling subtitles tool dialog.
    /// GTK4: Dialog → Window, RadioButton → grouped CheckButton,
    /// ComboBoxText → DropDown, Application.Invoke → GLib.Functions.IdleAdd.
    /// </summary>
    public class DialogDuelingSubtitles : Gtk.Window
    {
        private Settings _snapshot;
        private InfoStyle _styleSubs1 = new InfoStyle();
        private InfoStyle _styleSubs2 = new InfoStyle();

        private int _duelPattern;
        private bool _isSubs1First = true;
        private bool _quickReference = false;
        private bool _hasSubs2 = false;
        private string _lastDirPath = "";

        // Widgets
        private Gtk.Entry _txtSubs1, _txtSubs2, _txtOutputDir, _txtName;
        private Gtk.DropDown _dropEncSubs1, _dropEncSubs2, _dropPriority;
        private Gtk.StringList _encModel1, _encModel2, _prioModel;
        private Gtk.SpinButton _spinEpisodeStart, _spinPattern;
        private Gtk.SpinButton _spinTimeShiftSubs1, _spinTimeShiftSubs2;
        private Gtk.CheckButton _radioTimingSubs1, _radioTimingSubs2;
        private Gtk.CheckButton _chkTimeShift;
        private Gtk.CheckButton _chkRemoveStyledS1, _chkRemoveStyledS2;
        private Gtk.CheckButton _chkRemoveNoCounterS1, _chkRemoveNoCounterS2;
        private Gtk.CheckButton _chkQuickRef;
        private Gtk.Button _btnCreate;
        private Gtk.ProgressBar _progressBar;

        private GLib.MainLoop _loop;

        // Write-only properties for external setup
        public string Subs1FilePattern { set => _txtSubs1.SetText(value); }
        public string Subs2FilePattern { set => _txtSubs2.SetText(value); }
        public string OutputDir { set => _txtOutputDir.SetText(value); }
        public string DeckName { set => _txtName.SetText(value); }
        public int EpisodeStartNumber { set => _spinEpisodeStart.SetValue(value); }

        public bool UseSubs1Timings
        {
            set { _radioTimingSubs1.SetActive(value); _radioTimingSubs2.SetActive(!value); }
        }

        public bool UseTimeShift { set => _chkTimeShift.SetActive(value); }
        public int TimeShiftSubs1 { set => _spinTimeShiftSubs1.SetValue(value); }
        public int TimeShiftSubs2 { set => _spinTimeShiftSubs2.SetValue(value); }

        public string EncodingSubs1 { set => SetEncodingDrop(_dropEncSubs1, value); }
        public string EncodingSubs2 { set => SetEncodingDrop(_dropEncSubs2, value); }

        public string FileBrowserStartDir
        {
            get => Directory.Exists(_lastDirPath) ? _lastDirPath : "";
            set => _lastDirPath = value;
        }

        public DialogDuelingSubtitles(Gtk.Window parent)
        {
            SetTitle("Dueling Subtitles Tool");
            SetDefaultSize(620, 520);
            SetModal(true);
            if (parent != null) SetTransientFor(parent);

            BuildUI();
            LoadInitialState();
        }

        public void RunDialog()
        {
            _loop = GLib.MainLoop.New(null, false);
            OnCloseRequest += (s, e) =>
            {
                Settings.Instance.RestoreFrom(_snapshot);
                _loop.Quit();
                return false;
            };
            Show();
            _loop.Run();
        }

        // Alias expected by MainWindow
        public void Run() => RunDialog();

        // ── Build UI ─────────────────────────────────────────

        private void BuildUI()
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            vbox.SetMarginTop(8); vbox.SetMarginBottom(8);
            vbox.SetMarginStart(8); vbox.SetMarginEnd(8);

            // Help text
            var helpLabel = Gtk.Label.New(
                "Create a subtitle file that will simultaneously display a line from Subs1\n" +
                "and its corresponding line from Subs2. Only .ass/.ssa/.srt are supported.");
            helpLabel.SetWrap(true);
            helpLabel.SetHalign(Gtk.Align.Center);
            var helpFrame = Gtk.Frame.New(null);
            helpFrame.SetChild(helpLabel);
            vbox.Append(helpFrame);

            // File selection grid
            var fileGrid = Gtk.Grid.New();
            fileGrid.SetRowSpacing(4); fileGrid.SetColumnSpacing(6);
            int r = 0;

            var lblS1 = Gtk.Label.New("Subs1 (target language):"); lblS1.SetHalign(Gtk.Align.Start);
            fileGrid.Attach(lblS1, 0, r, 2, 1);
            var lblE1 = Gtk.Label.New("Subs1 Encoding:"); lblE1.SetHalign(Gtk.Align.End);
            fileGrid.Attach(lblE1, 2, r, 1, 1);
            r++;

            var btnS1 = Gtk.Button.NewWithLabel("Subs1...");
            btnS1.OnClicked += (s, e) => BrowseSubFile(_txtSubs1);
            fileGrid.Attach(btnS1, 0, r, 1, 1);
            _txtSubs1 = Gtk.Entry.New(); _txtSubs1.SetHexpand(true);
            fileGrid.Attach(_txtSubs1, 1, r, 1, 1);
            _dropEncSubs1 = BuildEncodingDrop(out _encModel1);
            fileGrid.Attach(_dropEncSubs1, 2, r, 1, 1);
            r++;

            var lblS2 = Gtk.Label.New("Subs2 (native language):"); lblS2.SetHalign(Gtk.Align.Start);
            fileGrid.Attach(lblS2, 0, r, 2, 1);
            var lblE2 = Gtk.Label.New("Subs2 Encoding:"); lblE2.SetHalign(Gtk.Align.End);
            fileGrid.Attach(lblE2, 2, r, 1, 1);
            r++;

            var btnS2 = Gtk.Button.NewWithLabel("Subs2...");
            btnS2.OnClicked += (s, e) => BrowseSubFile(_txtSubs2);
            fileGrid.Attach(btnS2, 0, r, 1, 1);
            _txtSubs2 = Gtk.Entry.New(); _txtSubs2.SetHexpand(true);
            fileGrid.Attach(_txtSubs2, 1, r, 1, 1);
            _dropEncSubs2 = BuildEncodingDrop(out _encModel2);
            fileGrid.Attach(_dropEncSubs2, 2, r, 1, 1);
            r++;

            var lblOut = Gtk.Label.New("Output directory:"); lblOut.SetHalign(Gtk.Align.Start);
            fileGrid.Attach(lblOut, 0, r, 3, 1);
            r++;

            var btnOut = Gtk.Button.NewWithLabel("Output...");
            btnOut.OnClicked += (s, e) => BrowseFolder(_txtOutputDir);
            fileGrid.Attach(btnOut, 0, r, 1, 1);
            _txtOutputDir = Gtk.Entry.New(); _txtOutputDir.SetHexpand(true);
            fileGrid.Attach(_txtOutputDir, 1, r, 2, 1);

            vbox.Append(fileGrid);

            // Subtitle Options
            var subOptFrame = Gtk.Frame.New("Subtitle Options");
            var subOptBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
            subOptBox.SetMarginTop(6); subOptBox.SetMarginBottom(6);
            subOptBox.SetMarginStart(6); subOptBox.SetMarginEnd(6);

            // Timings
            var timFrame = Gtk.Frame.New("Use Timings From");
            var timBox = Gtk.Box.New(Gtk.Orientation.Vertical, 2);
            timBox.SetMarginTop(4); timBox.SetMarginBottom(4);
            timBox.SetMarginStart(4); timBox.SetMarginEnd(4);
            _radioTimingSubs1 = Gtk.CheckButton.NewWithLabel("Subs1");
            _radioTimingSubs1.SetActive(true);
            _radioTimingSubs2 = Gtk.CheckButton.NewWithLabel("Subs2");
            _radioTimingSubs2.SetGroup(_radioTimingSubs1);
            timBox.Append(_radioTimingSubs1);
            timBox.Append(_radioTimingSubs2);
            timFrame.SetChild(timBox);
            subOptBox.Append(timFrame);

            // Time Shift
            var tsBox = Gtk.Box.New(Gtk.Orientation.Vertical, 2);
            tsBox.SetMarginTop(4); tsBox.SetMarginBottom(4);
            tsBox.SetMarginStart(4); tsBox.SetMarginEnd(4);
            _chkTimeShift = Gtk.CheckButton.NewWithLabel("Time Shift");
            _chkTimeShift.OnToggled += (s, e) =>
            {
                _spinTimeShiftSubs1.SetSensitive(_chkTimeShift.GetActive());
                _spinTimeShiftSubs2.SetSensitive(_chkTimeShift.GetActive());
            };
            tsBox.Append(_chkTimeShift);
            var tsGrid = Gtk.Grid.New();
            tsGrid.SetRowSpacing(2); tsGrid.SetColumnSpacing(4);
            tsGrid.Attach(Gtk.Label.New("Subs1:"), 0, 0, 1, 1);
            _spinTimeShiftSubs1 = Gtk.SpinButton.NewWithRange(-99999, 99999, 10);
            _spinTimeShiftSubs1.SetValue(0); _spinTimeShiftSubs1.SetSensitive(false);
            tsGrid.Attach(_spinTimeShiftSubs1, 1, 0, 1, 1);
            tsGrid.Attach(Gtk.Label.New("ms"), 2, 0, 1, 1);
            tsGrid.Attach(Gtk.Label.New("Subs2:"), 0, 1, 1, 1);
            _spinTimeShiftSubs2 = Gtk.SpinButton.NewWithRange(-99999, 99999, 10);
            _spinTimeShiftSubs2.SetValue(0); _spinTimeShiftSubs2.SetSensitive(false);
            tsGrid.Attach(_spinTimeShiftSubs2, 1, 1, 1, 1);
            tsGrid.Attach(Gtk.Label.New("ms"), 2, 1, 1, 1);
            tsBox.Append(tsGrid);
            subOptBox.Append(tsBox);

            // Remove w/o Counterpart
            var rcFrame = Gtk.Frame.New("Remove w/o Counterpart");
            var rcBox = Gtk.Box.New(Gtk.Orientation.Vertical, 2);
            rcBox.SetMarginTop(4); rcBox.SetMarginBottom(4);
            rcBox.SetMarginStart(4); rcBox.SetMarginEnd(4);
            _chkRemoveNoCounterS1 = Gtk.CheckButton.NewWithLabel("Subs1"); _chkRemoveNoCounterS1.SetActive(true);
            _chkRemoveNoCounterS2 = Gtk.CheckButton.NewWithLabel("Subs2"); _chkRemoveNoCounterS2.SetActive(true);
            rcBox.Append(_chkRemoveNoCounterS1);
            rcBox.Append(_chkRemoveNoCounterS2);
            rcFrame.SetChild(rcBox);
            subOptBox.Append(rcFrame);

            // Remove Styled
            var rsFrame = Gtk.Frame.New("Remove Styled Lines");
            var rsBox = Gtk.Box.New(Gtk.Orientation.Vertical, 2);
            rsBox.SetMarginTop(4); rsBox.SetMarginBottom(4);
            rsBox.SetMarginStart(4); rsBox.SetMarginEnd(4);
            _chkRemoveStyledS1 = Gtk.CheckButton.NewWithLabel("Subs1"); _chkRemoveStyledS1.SetActive(true);
            _chkRemoveStyledS2 = Gtk.CheckButton.NewWithLabel("Subs2"); _chkRemoveStyledS2.SetActive(true);
            rsBox.Append(_chkRemoveStyledS1);
            rsBox.Append(_chkRemoveStyledS2);
            rsFrame.SetChild(rsBox);
            subOptBox.Append(rsFrame);

            subOptFrame.SetChild(subOptBox);
            vbox.Append(subOptFrame);

            // Styles + Dueling Options
            var midBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);

            var styleFrame = Gtk.Frame.New("Text Styles");
            var styleBox = Gtk.Box.New(Gtk.Orientation.Vertical, 4);
            styleBox.SetMarginTop(6); styleBox.SetMarginBottom(6);
            styleBox.SetMarginStart(6); styleBox.SetMarginEnd(6);
            var styleBtnBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
            var btnStyleS1 = Gtk.Button.NewWithLabel("Subs1 Style...");
            btnStyleS1.OnClicked += OnStyleSubs1;
            var btnStyleS2 = Gtk.Button.NewWithLabel("Subs2 Style...");
            btnStyleS2.OnClicked += OnStyleSubs2;
            styleBtnBox.Append(btnStyleS1);
            styleBtnBox.Append(btnStyleS2);
            styleBox.Append(styleBtnBox);

            var prioBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
            prioBox.Append(Gtk.Label.New("Alignment priority:"));
            _prioModel = Gtk.StringList.New(new[] { "Subs1", "Subs2" });
            _dropPriority = Gtk.DropDown.New(_prioModel, null);
            _dropPriority.SetSelected(0);
            prioBox.Append(_dropPriority);
            styleBox.Append(prioBox);
            styleFrame.SetChild(styleBox);
            midBox.Append(styleFrame);

            var duelFrame = Gtk.Frame.New("Dueling Options");
            var duelBox = Gtk.Box.New(Gtk.Orientation.Vertical, 4);
            duelBox.SetMarginTop(6); duelBox.SetMarginBottom(6);
            duelBox.SetMarginStart(6); duelBox.SetMarginEnd(6);
            var patBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
            patBox.Append(Gtk.Label.New("Create a dueling subtitle every"));
            _spinPattern = Gtk.SpinButton.NewWithRange(1, 10, 1); _spinPattern.SetValue(1);
            patBox.Append(_spinPattern);
            patBox.Append(Gtk.Label.New("line(s)"));
            duelBox.Append(patBox);
            _chkQuickRef = Gtk.CheckButton.NewWithLabel("Also generate quick reference .txt file");
            duelBox.Append(_chkQuickRef);
            duelFrame.SetChild(duelBox);
            duelFrame.SetHexpand(true);
            midBox.Append(duelFrame);
            vbox.Append(midBox);

            // Naming
            var nameFrame = Gtk.Frame.New("Naming");
            var nameGrid = Gtk.Grid.New();
            nameGrid.SetRowSpacing(4); nameGrid.SetColumnSpacing(6);
            nameGrid.SetMarginTop(6); nameGrid.SetMarginBottom(6);
            nameGrid.SetMarginStart(6); nameGrid.SetMarginEnd(6);
            nameGrid.Attach(Gtk.Label.New("Name:"), 0, 0, 1, 1);
            _txtName = Gtk.Entry.New(); _txtName.SetHexpand(true);
            nameGrid.Attach(_txtName, 0, 1, 1, 1);
            nameGrid.Attach(Gtk.Label.New("First Episode:"), 1, 0, 1, 1);
            _spinEpisodeStart = Gtk.SpinButton.NewWithRange(0, 999, 1); _spinEpisodeStart.SetValue(1);
            nameGrid.Attach(_spinEpisodeStart, 1, 1, 1, 1);
            nameFrame.SetChild(nameGrid);
            vbox.Append(nameFrame);

            // Progress
            _progressBar = Gtk.ProgressBar.New();
            _progressBar.SetShowText(true);
            _progressBar.SetText("Ready");
            _progressBar.SetVisible(false);
            vbox.Append(_progressBar);

            // Buttons
            var btnRow = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
            _btnCreate = Gtk.Button.NewWithLabel("Create Dueling Subtitles!");
            _btnCreate.OnClicked += OnCreateClicked;
            btnRow.Append(_btnCreate);
            var btnClose = Gtk.Button.NewWithLabel("Close");
            btnClose.OnClicked += (s, e) => Close();
            btnRow.SetHalign(Gtk.Align.End);
            btnRow.Append(btnClose);
            vbox.Append(btnRow);

            SetChild(vbox);
        }

        // ── Initialization ───────────────────────────────────

        private void LoadInitialState()
        {
            _snapshot = Settings.Instance.Snapshot();
            _chkRemoveNoCounterS1.SetActive(Settings.Instance.Subs[0].RemoveNoCounterpart);
            _chkRemoveNoCounterS2.SetActive(Settings.Instance.Subs[1].RemoveNoCounterpart);
            _chkRemoveStyledS1.SetActive(Settings.Instance.Subs[0].RemoveStyledLines);
            _chkRemoveStyledS2.SetActive(Settings.Instance.Subs[1].RemoveStyledLines);
        }

        // ── Encoding Helpers ────────────────────────────────

        private Gtk.DropDown BuildEncodingDrop(out Gtk.StringList model)
        {
            var encodings = InfoEncoding.getEncodings();
            var names = new string[encodings.Length];
            int utf8Idx = 0;
            for (int i = 0; i < encodings.Length; i++)
            {
                names[i] = encodings[i].LongName;
                if (encodings[i].ShortName == "utf-8") utf8Idx = i;
            }
            model = Gtk.StringList.New(names);
            var drop = Gtk.DropDown.New(model, null);
            drop.SetSelected((uint)utf8Idx);
            return drop;
        }

        private void SetEncodingDrop(Gtk.DropDown drop, string longName)
        {
            var encodings = InfoEncoding.getEncodings();
            for (int i = 0; i < encodings.Length; i++)
                if (encodings[i].LongName == longName) { drop.SetSelected((uint)i); return; }
        }

        private string GetEncodingText(Gtk.DropDown drop)
        {
            var item = drop.GetSelectedItem() as Gtk.StringObject;
            return item?.GetString() ?? "Unicode (UTF-8)";
        }

        // ── File Browsing ────────────────────────────────────

        private async void BrowseSubFile(Gtk.Entry target)
        {
            var dlg = Gtk.FileDialog.New();
            dlg.SetTitle("Select Subtitle File");
            var filter = Gtk.FileFilter.New();
            filter.SetName("Subtitle Files (*.ass;*.ssa;*.srt;*.mkv)");
            filter.AddPattern("*.ass"); filter.AddPattern("*.ssa");
            filter.AddPattern("*.srt"); filter.AddPattern("*.mkv");
            var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
            filters.Append(filter);
            dlg.SetFilters(filters);

            try
            {
                var file = await dlg.OpenAsync(this);
                if (file != null)
                {
                    target.SetText(file.GetPath() ?? "");
                    _lastDirPath = IOPath.GetDirectoryName(file.GetPath() ?? "") ?? "";
                }
            }
            catch { /* user cancelled */ }

            // Check for MKV
            string path = target.GetText().Trim();
            if (IOPath.GetExtension(path) == ".mkv")
                OnSubsFileChanged(target == _txtSubs1 ? 1 : 2);
        }

        private async void BrowseFolder(Gtk.Entry target)
        {
            var dlg = Gtk.FileDialog.New();
            dlg.SetTitle("Select Output Directory");
            try
            {
                var file = await dlg.SelectFolderAsync(this);
                if (file != null)
                {
                    target.SetText(file.GetPath() ?? "");
                    _lastDirPath = file.GetPath() ?? "";
                }
            }
            catch { /* user cancelled */ }
        }

        // ── MKV track handling ───────────────────────────────

        private void OnSubsFileChanged(int subsNum)
        {
            var txt = subsNum == 1 ? _txtSubs1 : _txtSubs2;
            string file = txt.GetText().Trim();

            if (IOPath.GetExtension(file) != ".mkv") return;

            var allTracks = UtilsMkv.getSubtitleTrackList(file);
            var tracks = new List<MkvTrack>();
            foreach (var t in allTracks)
                if (t.Extension != "sub") tracks.Add(t);

            if (tracks.Count == 0)
            {
                UtilsMsg.showInfoMsg("This .mkv file does not contain any subtitle tracks.");
                txt.SetText("");
                return;
            }

            var dlg = new DialogSelectMkvTrack(this, file, subsNum, tracks);
            if (dlg.RunDialog())
                txt.SetText(dlg.ExtractedFile);
            else
                txt.SetText("");
            dlg.Close();
        }

        // ── Style buttons ────────────────────────────────────

        private void OnStyleSubs1(Gtk.Button sender, EventArgs e)
        {
            var dlg = new DialogSubtitleStyle(this, "Subs1 Style") { Style = _styleSubs1 };
            if (dlg.RunDialog()) _styleSubs1 = dlg.Style;
            dlg.Close();
        }

        private void OnStyleSubs2(Gtk.Button sender, EventArgs e)
        {
            var dlg = new DialogSubtitleStyle(this, "Subs2 Style") { Style = _styleSubs2 };
            if (dlg.RunDialog()) _styleSubs2 = dlg.Style;
            dlg.Close();
        }

        // ── Validation ───────────────────────────────────────

        private bool ValidateForm()
        {
            var errors = new List<string>();
            string s1 = _txtSubs1.GetText().Trim();
            string s2 = _txtSubs2.GetText().Trim();

            if (UtilsSubs.getNumSubsFiles(s1) == 0)
                errors.Add("Subs1: please provide a valid subtitle file.");
            if (UtilsSubs.getNumSubsFiles(s2) == 0)
                errors.Add("Subs2: please provide a valid subtitle file.");
            if (errors.Count == 0 && UtilsSubs.getNumSubsFiles(s1) != UtilsSubs.getNumSubsFiles(s2))
                errors.Add("The number of Subs1 and Subs2 files must match.");
            if (!Directory.Exists(_txtOutputDir.GetText().Trim()))
                errors.Add("Output directory does not exist.");
            string name = _txtName.GetText().Trim();
            if (name == "")
                errors.Add("Name must not be empty.");
            else if (name.IndexOfAny(new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' }) >= 0)
                errors.Add("Name contains invalid characters.");

            if (errors.Count > 0)
            {
                UtilsMsg.showErrMsg(string.Join("\n", errors));
                return false;
            }
            return true;
        }

        // ── Update Settings ──────────────────────────────────

        private void UpdateSettings()
        {
            Settings.Instance.Subs[0].FilePattern = _txtSubs1.GetText().Trim();
            Settings.Instance.Subs[0].TimingsEnabled = _radioTimingSubs1.GetActive();
            Settings.Instance.Subs[0].TimeShift = (int)_spinTimeShiftSubs1.GetValue();
            Settings.Instance.Subs[0].Files = UtilsSubs.getSubsFiles(Settings.Instance.Subs[0].FilePattern).ToArray();
            Settings.Instance.Subs[0].Encoding = InfoEncoding.longToShort(GetEncodingText(_dropEncSubs1));
            Settings.Instance.Subs[0].RemoveNoCounterpart = _chkRemoveNoCounterS1.GetActive();
            Settings.Instance.Subs[0].RemoveStyledLines = _chkRemoveStyledS1.GetActive();

            Settings.Instance.Subs[1].FilePattern = _txtSubs2.GetText().Trim();
            Settings.Instance.Subs[1].TimingsEnabled = _radioTimingSubs2.GetActive();
            Settings.Instance.Subs[1].TimeShift = (int)_spinTimeShiftSubs2.GetValue();
            Settings.Instance.Subs[1].Files = UtilsSubs.getSubsFiles(Settings.Instance.Subs[1].FilePattern).ToArray();
            Settings.Instance.Subs[1].Encoding = InfoEncoding.longToShort(GetEncodingText(_dropEncSubs2));
            Settings.Instance.Subs[1].RemoveNoCounterpart = _chkRemoveNoCounterS2.GetActive();
            Settings.Instance.Subs[1].RemoveStyledLines = _chkRemoveStyledS2.GetActive();

            Settings.Instance.OutputDir = _txtOutputDir.GetText().Trim();
            Settings.Instance.TimeShiftEnabled = _chkTimeShift.GetActive();
            Settings.Instance.DeckName = _txtName.GetText().Trim();
            Settings.Instance.EpisodeStartNumber = (int)_spinEpisodeStart.GetValue();

            _duelPattern = (int)_spinPattern.GetValue();
            _isSubs1First = (_dropPriority.GetSelected() == 0);
            _quickReference = _chkQuickRef.GetActive();
            _hasSubs2 = _txtSubs2.GetText().Trim().Length > 0;
        }

        // ── Create ──────────────────────────────────────────

        private async void OnCreateClicked(Gtk.Button sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            UpdateSettings();
            Logger.Instance.info("DuelingSubtitles: GO!");

            _btnCreate.SetSensitive(false);
            _progressBar.SetVisible(true);
            _progressBar.SetText("Starting...");
            _progressBar.SetFraction(0);

            var reporter = new InlineProgressReporter(_progressBar);
            bool success = false;
            string errorMsg = null;

            await Task.Run(() =>
            {
                try
                {
                    var workerVars = new WorkerVars(null, Settings.Instance.OutputDir,
                        WorkerVars.SubsProcessingType.Dueling);
                    var subsWorker = new WorkerSubs();
                    var combinedAll = subsWorker.combineAllSubs(workerVars, reporter);
                    if (combinedAll == null || reporter.Cancel) return;
                    workerVars.CombinedAll = combinedAll;

                    if (!CreateDuelingSubtitles(workerVars, reporter)) return;
                    if (_quickReference)
                        if (!CreateQuickReference(workerVars, reporter)) return;

                    success = !reporter.Cancel;
                }
                catch (Exception ex) { errorMsg = ex.ToString(); }
            });

            _btnCreate.SetSensitive(true);

            if (errorMsg != null)
                UtilsMsg.showErrMsg("Error:\n" + errorMsg);
            else if (reporter.Cancel)
                UtilsMsg.showInfoMsg("Action cancelled.");
            else if (success)
                UtilsMsg.showInfoMsg("Dueling subtitles have been created successfully.");

            _progressBar.SetText(success ? "Done!" : "Ready");
            _progressBar.SetFraction(success ? 1.0 : 0.0);
        }

        // ── ASS File Generation (unchanged logic) ───────────

        private bool CreateDuelingSubtitles(WorkerVars workerVars, IProgressReporter reporter)
        {
            int totalLines = 0, progressCount = 0;
            int totalEpisodes = workerVars.CombinedAll.Count;
            TimeSpan lastTime = UtilsSubs.getLastTime(workerVars.CombinedAll);
            foreach (var ep in workerVars.CombinedAll) totalLines += ep.Count;

            var name = new UtilsName(Settings.Instance.DeckName, totalEpisodes, totalLines, lastTime, 0, 0);
            for (int epIdx = 0; epIdx < workerVars.CombinedAll.Count; epIdx++)
            {
                var combArray = workerVars.CombinedAll[epIdx];
                string nameStr = name.createName(ConstantSettings.DuelingSubtitleFilenameFormat,
                    Settings.Instance.EpisodeStartNumber + epIdx, 0, TimeSpan.Zero, TimeSpan.Zero, "", "");
                string path = IOPath.Combine(Settings.Instance.OutputDir, nameStr);
                using var writer = new StreamWriter(path, false, Encoding.UTF8);
                writer.WriteLine(FormatScriptInfo(Settings.Instance.EpisodeStartNumber + epIdx));
                writer.WriteLine(FormatStyles());
                writer.WriteLine(FormatEventsHeader());
                for (int lineIdx = 0; lineIdx < combArray.Count; lineIdx++)
                {
                    progressCount++;
                    writer.WriteLine(FormatDialogPair(workerVars.CombinedAll, epIdx, lineIdx));
                    int pct = (int)(progressCount * 100.0 / totalLines);
                    reporter?.UpdateProgress(pct, $"Generating subtitle file: line {progressCount} of {totalLines}");
                    if (reporter.Cancel) return false;
                }
            }
            return true;
        }

        private bool CreateQuickReference(WorkerVars workerVars, IProgressReporter reporter)
        {
            int totalLines = 0, progressCount = 0;
            int totalEpisodes = workerVars.CombinedAll.Count;
            TimeSpan lastTime = UtilsSubs.getLastTime(workerVars.CombinedAll);
            foreach (var ep in workerVars.CombinedAll) totalLines += ep.Count;

            var name = new UtilsName(Settings.Instance.DeckName, totalEpisodes, totalLines, lastTime, 0, 0);
            for (int epIdx = 0; epIdx < workerVars.CombinedAll.Count; epIdx++)
            {
                var combArray = workerVars.CombinedAll[epIdx];
                string nameStr = name.createName(ConstantSettings.DuelingQuickRefFilenameFormat,
                    Settings.Instance.EpisodeStartNumber + epIdx, 0, TimeSpan.Zero, TimeSpan.Zero, "", "");
                string path = IOPath.Combine(Settings.Instance.OutputDir, nameStr);
                using var writer = new StreamWriter(path, false, Encoding.UTF8);
                for (int lineIdx = 0; lineIdx < combArray.Count; lineIdx++)
                {
                    progressCount++;
                    var comb = combArray[lineIdx];
                    int episode = Settings.Instance.EpisodeStartNumber + epIdx;
                    writer.WriteLine(FormatQuickRefPair(comb, name, episode, progressCount));
                    int pct = (int)(progressCount * 100.0 / totalLines);
                    reporter?.UpdateProgress(pct, $"Generating quick reference: line {progressCount} of {totalLines}");
                    if (reporter.Cancel) return false;
                }
            }
            return true;
        }

        // ── ASS Formatting (unchanged) ──────────────────────

        private string FormatScriptInfo(int episode) =>
            $"; Generated with {UtilsAssembly.Title}\n\n[Script Info]\nTitle:{Settings.Instance.DeckName}_{episode:000}\nScriptType:v4.00+\nCollisions:Normal\nTimer:100.0000\n";

        private string FormatStyles() =>
            "[V4+ Styles]\nFormat: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\n" +
            FormatSingleStyle("Subs1", _styleSubs1) + FormatSingleStyle("Subs2", _styleSubs2);

        private string FormatSingleStyle(string name, InfoStyle style)
        {
            int bold = style.Font.Bold ? -1 : 0, italic = style.Font.Italic ? -1 : 0;
            int underline = style.Font.Underline ? -1 : 0, strikeOut = style.Font.Strikeout ? -1 : 0;
            int borderStyle = style.OpaqueBox ? 3 : 1;
            return string.Format("Style: {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22}\n",
                name, style.Font.Name, style.Font.Size,
                UtilsSubs.formatAssColor(style.ColorPrimary, style.OpacityPrimary),
                UtilsSubs.formatAssColor(style.ColorSecondary, style.OpacitySecondary),
                UtilsSubs.formatAssColor(style.ColorOutline, style.OpacityOutline),
                UtilsSubs.formatAssColor(style.ColorShadow, style.OpacityShadow),
                bold, italic, underline, strikeOut,
                style.ScaleX, style.ScaleY, style.Spacing, style.Rotation,
                borderStyle, style.Outline, style.Shadow, style.Alignment,
                style.MarginLeft, style.MarginRight, style.MarginVertical, style.Encoding.Num);
        }

        private static string FormatEventsHeader() =>
            "[Events]\nFormat: Layer, Start, End, Style, Actor, MarginL, MarginR, MarginV, Effect, Text";

        private string FormatDialogSingle(bool isSubs1, InfoCombined comb) =>
            string.Format("Dialogue: 0,{0},{1},{2},NA,0000,0000,0000,,{3}",
                UtilsSubs.formatAssTime(comb.Subs1.StartTime), UtilsSubs.formatAssTime(comb.Subs1.EndTime),
                isSubs1 ? "Subs1" : "Subs2", isSubs1 ? comb.Subs1.Text : comb.Subs2.Text);

        private string FormatDialogPair(List<List<InfoCombined>> combinedAll, int epIdx, int lineIdx)
        {
            var comb = combinedAll[epIdx][lineIdx];
            if (_isSubs1First)
            {
                string pair = FormatDialogSingle(true, comb);
                if (lineIdx % _duelPattern == 0) pair += "\n" + FormatDialogSingle(false, comb);
                return pair;
            }
            else
            {
                string pair = "";
                if (lineIdx % _duelPattern == 0) pair += FormatDialogSingle(false, comb) + "\n";
                pair += FormatDialogSingle(true, comb);
                return pair;
            }
        }

        private string FormatQuickRefPair(InfoCombined comb, UtilsName name, int episode, int seqNum)
        {
            string s1 = comb.Subs1.Text, s2 = comb.Subs2.Text;
            string pair = name.createName(ConstantSettings.DuelingQuickRefSubs1Format,
                episode, seqNum, comb.Subs1.StartTime, comb.Subs1.StartTime, s1, s2);
            if (_hasSubs2 && ConstantSettings.DuelingQuickRefSubs2Format != "")
                pair += "\n" + name.createName(ConstantSettings.DuelingQuickRefSubs2Format,
                    episode, seqNum, comb.Subs1.StartTime, comb.Subs1.StartTime, s1, s2);
            return pair;
        }

        // ── Progress Reporter ───────────────────────────────

        private class InlineProgressReporter : IProgressReporter
        {
            private readonly Gtk.ProgressBar _bar;
            private readonly CancellationTokenSource _cts = new();
            private bool _cancel;
            public CancellationToken Token => _cts.Token;
            public bool Cancel
            {
                get => _cancel;
                set { _cancel = value; if (value) _cts.Cancel(); }
            }
            public int StepsTotal { get; set; }

            public InlineProgressReporter(Gtk.ProgressBar bar) { _bar = bar; }

            public void UpdateProgress(int percent, string text)
            {
                GLib.Functions.IdleAdd(0, () =>
                {
                    _bar.SetFraction(Math.Max(0, Math.Min(1, percent / 100.0)));
                    _bar.SetText(text);
                    return false;
                });
            }

            public void UpdateProgress(string text)
            {
                GLib.Functions.IdleAdd(0, () => { _bar.SetText(text); return false; });
            }

            public void NextStep(int step, string description) =>
                UpdateProgress(0, $"[{step}/{StepsTotal}] {description}");
            public void EnableDetail(bool enable) { }
            public void SetDuration(TimeSpan duration) { }
            public void OnFFmpegOutput(object sender, System.Diagnostics.DataReceivedEventArgs e) { }
        }
    }
}
