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
using System.Text;
using System.Threading.Tasks;
using Gtk;
using IOPath = System.IO.Path;

namespace subs2srs
{
    public class DialogDuelingSubtitles : Dialog
    {
        // Settings backup
        private SaveSettings _oldSettings = new SaveSettings();

        // Styles
        private InfoStyle _styleSubs1 = new InfoStyle();
        private InfoStyle _styleSubs2 = new InfoStyle();

        // Local vars set by UpdateSettings, used by worker
        private int _duelPattern;
        private bool _isSubs1First = true;
        private bool _quickReference = false;
        private bool _hasSubs2 = false;

        private string _lastDirPath = "";

        // ── Widgets ──────────────────────────────────────────

        private Entry _txtSubs1, _txtSubs2, _txtOutputDir, _txtName;
        private ComboBoxText _comboEncSubs1, _comboEncSubs2, _comboPriority;
        private SpinButton _spinEpisodeStart, _spinPattern;
        private SpinButton _spinTimeShiftSubs1, _spinTimeShiftSubs2;
        private RadioButton _radioTimingSubs1, _radioTimingSubs2;
        private CheckButton _chkTimeShift;
        private CheckButton _chkRemoveStyledS1, _chkRemoveStyledS2;
        private CheckButton _chkRemoveNoCounterS1, _chkRemoveNoCounterS2;
        private CheckButton _chkQuickRef;
        private Button _btnCreate;
        private ProgressBar _progressBar;

        // ── Properties (write-only, for external setup) ──────

        public string Subs1FilePattern { set => _txtSubs1.Text = value; }
        public string Subs2FilePattern { set => _txtSubs2.Text = value; }
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

        public string EncodingSubs1 { set => SetEncodingCombo(_comboEncSubs1, value); }
        public string EncodingSubs2 { set => SetEncodingCombo(_comboEncSubs2, value); }

        public string FileBrowserStartDir
        {
            get => Directory.Exists(_lastDirPath) ? _lastDirPath : "";
            set => _lastDirPath = value;
        }

        // ── Constructor ──────────────────────────────────────

        public DialogDuelingSubtitles(Window parent)
            : base("Dueling Subtitles Tool", parent, DialogFlags.Modal,
                "Close", ResponseType.Close)
        {
            SetDefaultSize(620, 520);
            Resizable = false;
            BuildUI();
            LoadInitialState();
        }

        // ── Build UI ─────────────────────────────────────────

        private void BuildUI()
        {
            var vbox = new Box(Orientation.Vertical, 6) { BorderWidth = 8 };

            // Help text
            var helpLabel = new Label(
                "Create a subtitle file that will simultaneously display a line from Subs1\n" +
                "and its corresponding line from Subs2. Only .ass/.ssa/.srt are supported.")
            { Wrap = true, Halign = Align.Center };
            var helpFrame = new Frame();
            helpFrame.Add(helpLabel);
            vbox.PackStart(helpFrame, false, false, 0);

            // ── File selection ──
            var fileGrid = new Grid { RowSpacing = 4, ColumnSpacing = 6 };
            int r = 0;

            // Subs1
            fileGrid.Attach(new Label("Subs1 (target language):") { Halign = Align.Start }, 0, r, 2, 1);
            fileGrid.Attach(new Label("Subs1 Encoding:") { Halign = Align.End }, 2, r, 1, 1);
            r++;
            var btnS1 = new Button("Subs1...");
            btnS1.Clicked += (s, e) => BrowseSubFile(_txtSubs1);
            fileGrid.Attach(btnS1, 0, r, 1, 1);
            _txtSubs1 = new Entry { Hexpand = true };
            _txtSubs1.Changed += (s, e) => OnSubsFileChanged(1);
            fileGrid.Attach(_txtSubs1, 1, r, 1, 1);
            _comboEncSubs1 = BuildEncodingCombo();
            fileGrid.Attach(_comboEncSubs1, 2, r, 1, 1);
            r++;

            // Subs2
            fileGrid.Attach(new Label("Subs2 (native language):") { Halign = Align.Start }, 0, r, 2, 1);
            fileGrid.Attach(new Label("Subs2 Encoding:") { Halign = Align.End }, 2, r, 1, 1);
            r++;
            var btnS2 = new Button("Subs2...");
            btnS2.Clicked += (s, e) => BrowseSubFile(_txtSubs2);
            fileGrid.Attach(btnS2, 0, r, 1, 1);
            _txtSubs2 = new Entry { Hexpand = true };
            _txtSubs2.Changed += (s, e) => OnSubsFileChanged(2);
            fileGrid.Attach(_txtSubs2, 1, r, 1, 1);
            _comboEncSubs2 = BuildEncodingCombo();
            fileGrid.Attach(_comboEncSubs2, 2, r, 1, 1);
            r++;

            // Output dir
            fileGrid.Attach(new Label("Output directory:") { Halign = Align.Start }, 0, r, 3, 1);
            r++;
            var btnOut = new Button("Output...");
            btnOut.Clicked += (s, e) => BrowseFolder(_txtOutputDir);
            fileGrid.Attach(btnOut, 0, r, 1, 1);
            _txtOutputDir = new Entry { Hexpand = true };
            fileGrid.Attach(_txtOutputDir, 1, r, 2, 1);

            vbox.PackStart(fileGrid, false, false, 0);

            // ── Subtitle Options ──
            var subOptFrame = new Frame("Subtitle Options");
            var subOptBox = new Box(Orientation.Horizontal, 6) { BorderWidth = 6 };

            // Timings
            var timFrame = new Frame("Use Timings From");
            var timBox = new Box(Orientation.Vertical, 2) { BorderWidth = 4 };
            _radioTimingSubs1 = new RadioButton("Subs1") { Active = true };
            _radioTimingSubs2 = new RadioButton(_radioTimingSubs1, "Subs2");
            timBox.PackStart(_radioTimingSubs1, false, false, 0);
            timBox.PackStart(_radioTimingSubs2, false, false, 0);
            timFrame.Add(timBox);
            subOptBox.PackStart(timFrame, false, false, 0);

            // Time Shift
            var tsBox = new Box(Orientation.Vertical, 2) { BorderWidth = 4 };
            _chkTimeShift = new CheckButton("Time Shift");
            _chkTimeShift.Toggled += (s, e) =>
            {
                _spinTimeShiftSubs1.Sensitive = _chkTimeShift.Active;
                _spinTimeShiftSubs2.Sensitive = _chkTimeShift.Active;
            };
            tsBox.PackStart(_chkTimeShift, false, false, 0);
            var tsGrid = new Grid { RowSpacing = 2, ColumnSpacing = 4 };
            tsGrid.Attach(new Label("Subs1:") { Halign = Align.End }, 0, 0, 1, 1);
            _spinTimeShiftSubs1 = new SpinButton(-99999, 99999, 10) { Value = 0, Sensitive = false };
            tsGrid.Attach(_spinTimeShiftSubs1, 1, 0, 1, 1);
            tsGrid.Attach(new Label("ms"), 2, 0, 1, 1);
            tsGrid.Attach(new Label("Subs2:") { Halign = Align.End }, 0, 1, 1, 1);
            _spinTimeShiftSubs2 = new SpinButton(-99999, 99999, 10) { Value = 0, Sensitive = false };
            tsGrid.Attach(_spinTimeShiftSubs2, 1, 1, 1, 1);
            tsGrid.Attach(new Label("ms"), 2, 1, 1, 1);
            tsBox.PackStart(tsGrid, false, false, 0);
            subOptBox.PackStart(tsBox, false, false, 0);

            // Remove w/o Counterpart
            var rcFrame = new Frame("Remove w/o Counterpart");
            var rcBox = new Box(Orientation.Vertical, 2) { BorderWidth = 4 };
            _chkRemoveNoCounterS1 = new CheckButton("Subs1") { Active = true };
            _chkRemoveNoCounterS2 = new CheckButton("Subs2") { Active = true };
            rcBox.PackStart(_chkRemoveNoCounterS1, false, false, 0);
            rcBox.PackStart(_chkRemoveNoCounterS2, false, false, 0);
            rcFrame.Add(rcBox);
            subOptBox.PackStart(rcFrame, false, false, 0);

            // Remove Styled
            var rsFrame = new Frame("Remove Styled Lines");
            var rsBox = new Box(Orientation.Vertical, 2) { BorderWidth = 4 };
            _chkRemoveStyledS1 = new CheckButton("Subs1") { Active = true };
            _chkRemoveStyledS2 = new CheckButton("Subs2") { Active = true };
            rsBox.PackStart(_chkRemoveStyledS1, false, false, 0);
            rsBox.PackStart(_chkRemoveStyledS2, false, false, 0);
            rsFrame.Add(rsBox);
            subOptBox.PackStart(rsFrame, false, false, 0);

            subOptFrame.Add(subOptBox);
            vbox.PackStart(subOptFrame, false, false, 0);

            // ── Styles + Dueling Options (side by side) ──
            var midBox = new Box(Orientation.Horizontal, 6);

            // Text Styles
            var styleFrame = new Frame("Text Styles");
            var styleBox = new Box(Orientation.Vertical, 4) { BorderWidth = 6 };
            var styleBtnBox = new Box(Orientation.Horizontal, 4);
            var btnStyleS1 = new Button("Subs1 Style...");
            btnStyleS1.Clicked += OnStyleSubs1;
            var btnStyleS2 = new Button("Subs2 Style...");
            btnStyleS2.Clicked += OnStyleSubs2;
            styleBtnBox.PackStart(btnStyleS1, false, false, 0);
            styleBtnBox.PackStart(btnStyleS2, false, false, 0);
            styleBox.PackStart(styleBtnBox, false, false, 0);
            var prioBox = new Box(Orientation.Horizontal, 4);
            prioBox.PackStart(new Label("Alignment priority:"), false, false, 0);
            _comboPriority = new ComboBoxText();
            _comboPriority.AppendText("Subs1");
            _comboPriority.AppendText("Subs2");
            _comboPriority.Active = 0;
            prioBox.PackStart(_comboPriority, false, false, 0);
            styleBox.PackStart(prioBox, false, false, 0);
            styleFrame.Add(styleBox);
            midBox.PackStart(styleFrame, false, false, 0);

            // Dueling Options
            var duelFrame = new Frame("Dueling Options");
            var duelBox = new Box(Orientation.Vertical, 4) { BorderWidth = 6 };
            var patBox = new Box(Orientation.Horizontal, 4);
            patBox.PackStart(new Label("Create a dueling subtitle every"), false, false, 0);
            _spinPattern = new SpinButton(1, 10, 1) { Value = 1 };
            patBox.PackStart(_spinPattern, false, false, 0);
            patBox.PackStart(new Label("line(s)"), false, false, 0);
            duelBox.PackStart(patBox, false, false, 0);
            _chkQuickRef = new CheckButton("Also generate quick reference .txt file");
            duelBox.PackStart(_chkQuickRef, false, false, 0);
            duelFrame.Add(duelBox);
            midBox.PackStart(duelFrame, true, true, 0);

            vbox.PackStart(midBox, false, false, 0);

            // ── Naming ──
            var nameFrame = new Frame("Naming");
            var nameGrid = new Grid { RowSpacing = 4, ColumnSpacing = 6, BorderWidth = 6 };
            nameGrid.Attach(new Label("Name:") { Halign = Align.Start }, 0, 0, 1, 1);
            _txtName = new Entry { Hexpand = true };
            nameGrid.Attach(_txtName, 0, 1, 1, 1);
            nameGrid.Attach(new Label("First Episode:") { Halign = Align.Start }, 1, 0, 1, 1);
            _spinEpisodeStart = new SpinButton(0, 999, 1) { Value = 1 };
            nameGrid.Attach(_spinEpisodeStart, 1, 1, 1, 1);
            nameFrame.Add(nameGrid);
            vbox.PackStart(nameFrame, false, false, 0);

            // ── Progress + Create ──
            _progressBar = new ProgressBar { ShowText = true, Text = "Ready", NoShowAll = true };
            vbox.PackStart(_progressBar, false, false, 0);

            _btnCreate = new Button("Create Dueling Subtitles!");
            _btnCreate.Clicked += OnCreateClicked;
            vbox.PackStart(_btnCreate, false, false, 4);

            ContentArea.PackStart(vbox, true, true, 0);
            ContentArea.ShowAll();
        }

        // ── Initialization ───────────────────────────────────

        private void LoadInitialState()
        {
            // Save global settings
            var cur = new SaveSettings();
            cur.gatherData();
            _oldSettings = ObjectCopier.Clone<SaveSettings>(cur);

            _chkRemoveNoCounterS1.Active = Settings.Instance.Subs[0].RemoveNoCounterpart;
            _chkRemoveNoCounterS2.Active = Settings.Instance.Subs[1].RemoveNoCounterpart;
            _chkRemoveStyledS1.Active = Settings.Instance.Subs[0].RemoveStyledLines;
            _chkRemoveStyledS2.Active = Settings.Instance.Subs[1].RemoveStyledLines;
        }

        protected override void OnResponse(ResponseType response_id)
        {
            // Restore global settings on any close
            Settings.Instance.loadSettings(_oldSettings);
            base.OnResponse(response_id);
        }

        // ── Encoding Combo Builder ───────────────────────────

        private ComboBoxText BuildEncodingCombo()
        {
            var combo = new ComboBoxText();
            var encodings = InfoEncoding.getEncodings();
            int idx = 0, utf8Idx = 0;
            foreach (var enc in encodings)
            {
                combo.AppendText(enc.LongName);
                if (enc.ShortName == "utf-8") utf8Idx = idx;
                idx++;
            }
            combo.Active = utf8Idx;
            return combo;
        }

        private void SetEncodingCombo(ComboBoxText combo, string longName)
        {
            var encodings = InfoEncoding.getEncodings();
            for (int i = 0; i < encodings.Length; i++)
                if (encodings[i].LongName == longName) { combo.Active = i; return; }
        }

        // ── File Browsing ────────────────────────────────────

        private void BrowseSubFile(Entry target)
        {
            var dlg = new FileChooserDialog("Select Subtitle File", this,
                FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
            var filter = new FileFilter { Name = "Subtitle Files (*.ass;*.ssa;*.srt;*.mkv)" };
            filter.AddPattern("*.ass"); filter.AddPattern("*.ssa");
            filter.AddPattern("*.srt"); filter.AddPattern("*.mkv");
            dlg.AddFilter(filter);
            if (_lastDirPath != "" && Directory.Exists(_lastDirPath))
                dlg.SetCurrentFolder(_lastDirPath);
            if (dlg.Run() == (int)ResponseType.Accept)
            {
                target.Text = dlg.Filename;
                _lastDirPath = IOPath.GetDirectoryName(dlg.Filename) ?? "";
            }
            dlg.Destroy();
        }

        private void BrowseFolder(Entry target)
        {
            var dlg = new FileChooserDialog("Select Output Directory", this,
                FileChooserAction.SelectFolder, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
            string cur = target.Text.Trim();
            if (Directory.Exists(cur)) dlg.SetCurrentFolder(cur);
            else if (_lastDirPath != "") dlg.SetCurrentFolder(_lastDirPath);
            if (dlg.Run() == (int)ResponseType.Accept)
            {
                target.Text = dlg.Filename;
                _lastDirPath = dlg.Filename;
            }
            dlg.Destroy();
        }

        // ── MKV track handling ───────────────────────────────

        private void OnSubsFileChanged(int subsNum)
        {
            var txt = subsNum == 1 ? _txtSubs1 : _txtSubs2;
            string file = txt.Text.Trim();

            if (IOPath.GetExtension(file) != ".mkv") return;

            var allTracks = UtilsMkv.getSubtitleTrackList(file);
            var tracks = new List<MkvTrack>();
            foreach (var t in allTracks)
                if (t.Extension != "sub") tracks.Add(t);

            if (tracks.Count == 0)
            {
                UtilsMsg.showInfoMsg("This .mkv file does not contain any subtitle tracks.");
                txt.Text = "";
                return;
            }

            var dlg = new DialogSelectMkvTrack(this, file, subsNum, tracks);
            if (dlg.Run() == (int)ResponseType.Ok)
                txt.Text = dlg.ExtractedFile;
            else
                txt.Text = "";
            dlg.Destroy();
        }

        // ── Style buttons ────────────────────────────────────

        private void OnStyleSubs1(object s, EventArgs e)
        {
            var dlg = new DialogSubtitleStyle(this, "Subs1 Style") { Style = _styleSubs1 };
            if (dlg.Run() == (int)ResponseType.Ok) _styleSubs1 = dlg.Style;
            dlg.Destroy();
        }

        private void OnStyleSubs2(object s, EventArgs e)
        {
            var dlg = new DialogSubtitleStyle(this, "Subs2 Style") { Style = _styleSubs2 };
            if (dlg.Run() == (int)ResponseType.Ok) _styleSubs2 = dlg.Style;
            dlg.Destroy();
        }

        // ── Validation ───────────────────────────────────────

        private bool ValidateForm()
        {
            var errors = new List<string>();
            string s1 = _txtSubs1.Text.Trim();
            string s2 = _txtSubs2.Text.Trim();

            if (UtilsSubs.getNumSubsFiles(s1) == 0)
                errors.Add("Subs1: please provide a valid subtitle file (.srt/.ass/.ssa).");

            if (UtilsSubs.getNumSubsFiles(s2) == 0)
                errors.Add("Subs2: please provide a valid subtitle file (.srt/.ass/.ssa).");

            if (errors.Count == 0 && UtilsSubs.getNumSubsFiles(s1) != UtilsSubs.getNumSubsFiles(s2))
                errors.Add("The number of Subs1 and Subs2 files must match.");

            if (!Directory.Exists(_txtOutputDir.Text.Trim()))
                errors.Add("Output directory does not exist.");

            string name = _txtName.Text.Trim();
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
            Settings.Instance.Subs[0].FilePattern = _txtSubs1.Text.Trim();
            Settings.Instance.Subs[0].TimingsEnabled = _radioTimingSubs1.Active;
            Settings.Instance.Subs[0].TimeShift = (int)_spinTimeShiftSubs1.Value;
            Settings.Instance.Subs[0].Files = UtilsSubs.getSubsFiles(Settings.Instance.Subs[0].FilePattern).ToArray();
            Settings.Instance.Subs[0].Encoding = InfoEncoding.longToShort(_comboEncSubs1.ActiveText ?? "Unicode (UTF-8)");
            Settings.Instance.Subs[0].RemoveNoCounterpart = _chkRemoveNoCounterS1.Active;
            Settings.Instance.Subs[0].RemoveStyledLines = _chkRemoveStyledS1.Active;

            Settings.Instance.Subs[1].FilePattern = _txtSubs2.Text.Trim();
            Settings.Instance.Subs[1].TimingsEnabled = _radioTimingSubs2.Active;
            Settings.Instance.Subs[1].TimeShift = (int)_spinTimeShiftSubs2.Value;
            Settings.Instance.Subs[1].Files = UtilsSubs.getSubsFiles(Settings.Instance.Subs[1].FilePattern).ToArray();
            Settings.Instance.Subs[1].Encoding = InfoEncoding.longToShort(_comboEncSubs2.ActiveText ?? "Unicode (UTF-8)");
            Settings.Instance.Subs[1].RemoveNoCounterpart = _chkRemoveNoCounterS2.Active;
            Settings.Instance.Subs[1].RemoveStyledLines = _chkRemoveStyledS2.Active;

            Settings.Instance.OutputDir = _txtOutputDir.Text.Trim();
            Settings.Instance.TimeShiftEnabled = _chkTimeShift.Active;
            Settings.Instance.DeckName = _txtName.Text.Trim();
            Settings.Instance.EpisodeStartNumber = (int)_spinEpisodeStart.Value;

            _duelPattern = (int)_spinPattern.Value;
            _isSubs1First = (_comboPriority.Active == 0);
            _quickReference = _chkQuickRef.Active;
            _hasSubs2 = _txtSubs2.Text.Trim().Length > 0;
        }

        // ── Create button handler ────────────────────────────

        private async void OnCreateClicked(object s, EventArgs e)
        {
            if (!ValidateForm()) return;

            UpdateSettings();
            Logger.Instance.info("DuelingSubtitles: GO!");

            _btnCreate.Sensitive = false;
            _progressBar.Visible = true;
            _progressBar.Text = "Starting...";
            _progressBar.Fraction = 0;

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

                    // Parse and combine
                    var combinedAll = subsWorker.combineAllSubs(workerVars, reporter);
                    if (combinedAll == null || reporter.Cancel) return;
                    workerVars.CombinedAll = combinedAll;

                    // Create .ass files
                    if (!CreateDuelingSubtitles(workerVars, reporter)) return;

                    // Create quick reference
                    if (_quickReference)
                        if (!CreateQuickReference(workerVars, reporter)) return;

                    success = !reporter.Cancel;
                }
                catch (Exception ex) { errorMsg = ex.ToString(); }
            });

            _btnCreate.Sensitive = true;

            if (errorMsg != null)
                UtilsMsg.showErrMsg("Error:\n" + errorMsg);
            else if (reporter.Cancel)
                UtilsMsg.showInfoMsg("Action cancelled.");
            else if (success)
                UtilsMsg.showInfoMsg("Dueling subtitles have been created successfully.");

            _progressBar.Text = success ? "Done!" : "Ready";
            _progressBar.Fraction = success ? 1.0 : 0.0;
        }

        // ── ASS File Generation ──────────────────────────────

        private bool CreateDuelingSubtitles(WorkerVars workerVars, IProgressReporter reporter)
        {
            int totalLines = 0, progressCount = 0;
            int totalEpisodes = workerVars.CombinedAll.Count;
            DateTime lastTime = UtilsSubs.getLastTime(workerVars.CombinedAll);

            foreach (var ep in workerVars.CombinedAll)
                totalLines += ep.Count;

            var name = new UtilsName(Settings.Instance.DeckName, totalEpisodes, totalLines, lastTime, 0, 0);

            for (int epIdx = 0; epIdx < workerVars.CombinedAll.Count; epIdx++)
            {
                var combArray = workerVars.CombinedAll[epIdx];
                string nameStr = name.createName(ConstantSettings.DuelingSubtitleFilenameFormat,
                    Settings.Instance.EpisodeStartNumber + epIdx, 0, new DateTime(), new DateTime(), "", "");

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
                    reporter?.UpdateProgress(pct,
                        $"Generating subtitle file: line {progressCount} of {totalLines}");

                    if (reporter.Cancel) return false;
                }
            }
            return true;
        }

        private bool CreateQuickReference(WorkerVars workerVars, IProgressReporter reporter)
        {
            int totalLines = 0, progressCount = 0;
            int totalEpisodes = workerVars.CombinedAll.Count;
            DateTime lastTime = UtilsSubs.getLastTime(workerVars.CombinedAll);

            foreach (var ep in workerVars.CombinedAll)
                totalLines += ep.Count;

            var name = new UtilsName(Settings.Instance.DeckName, totalEpisodes, totalLines, lastTime, 0, 0);

            for (int epIdx = 0; epIdx < workerVars.CombinedAll.Count; epIdx++)
            {
                var combArray = workerVars.CombinedAll[epIdx];
                string nameStr = name.createName(ConstantSettings.DuelingQuickRefFilenameFormat,
                    Settings.Instance.EpisodeStartNumber + epIdx, 0, new DateTime(), new DateTime(), "", "");

                string path = IOPath.Combine(Settings.Instance.OutputDir, nameStr);
                using var writer = new StreamWriter(path, false, Encoding.UTF8);

                for (int lineIdx = 0; lineIdx < combArray.Count; lineIdx++)
                {
                    progressCount++;
                    var comb = combArray[lineIdx];
                    int episode = Settings.Instance.EpisodeStartNumber + epIdx;

                    writer.WriteLine(FormatQuickRefPair(comb, name, episode, progressCount));

                    int pct = (int)(progressCount * 100.0 / totalLines);
                    reporter?.UpdateProgress(pct,
                        $"Generating quick reference: line {progressCount} of {totalLines}");

                    if (reporter.Cancel) return false;
                }
            }
            return true;
        }

        // ── ASS Formatting ───────────────────────────────────

        private string FormatScriptInfo(int episode)
        {
            return $"; Generated with {UtilsAssembly.Title}\n\n" +
                   "[Script Info]\n" +
                   $"Title:{Settings.Instance.DeckName}_{episode:000}\n" +
                   "ScriptType:v4.00+\n" +
                   "Collisions:Normal\n" +
                   "Timer:100.0000\n";
        }

        private string FormatStyles()
        {
            return "[V4+ Styles]\n" +
                "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, " +
                "Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, " +
                "Alignment, MarginL, MarginR, MarginV, Encoding\n" +
                FormatSingleStyle("Subs1", _styleSubs1) +
                FormatSingleStyle("Subs2", _styleSubs2);
        }

        private string FormatSingleStyle(string name, InfoStyle style)
        {
            int bold = style.Font.Bold ? -1 : 0;
            int italic = style.Font.Italic ? -1 : 0;
            int underline = style.Font.Underline ? -1 : 0;
            int strikeOut = style.Font.Strikeout ? -1 : 0;
            int borderStyle = style.OpaqueBox ? 3 : 1;

            return string.Format(
                "Style: {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22}\n",
                name, style.Font.Name, style.Font.Size,
                UtilsSubs.formatAssColor(style.ColorPrimary, style.OpacityPrimary),
                UtilsSubs.formatAssColor(style.ColorSecondary, style.OpacitySecondary),
                UtilsSubs.formatAssColor(style.ColorOutline, style.OpacityOutline),
                UtilsSubs.formatAssColor(style.ColorShadow, style.OpacityShadow),
                bold, italic, underline, strikeOut,
                style.ScaleX, style.ScaleY, style.Spacing, style.Rotation,
                borderStyle, style.Outline, style.Shadow, style.Alignment,
                style.MarginLeft, style.MarginRight, style.MarginVertical,
                style.Encoding.Num);
        }

        private static string FormatEventsHeader()
        {
            return "[Events]\n" +
                   "Format: Layer, Start, End, Style, Actor, MarginL, MarginR, MarginV, Effect, Text";
        }

        private string FormatDialogSingle(bool isSubs1, InfoCombined comb)
        {
            string styleName = isSubs1 ? "Subs1" : "Subs2";
            string text = isSubs1 ? comb.Subs1.Text : comb.Subs2.Text;

            return string.Format("Dialogue: 0,{0},{1},{2},NA,0000,0000,0000,,{3}",
                UtilsSubs.formatAssTime(comb.Subs1.StartTime),
                UtilsSubs.formatAssTime(comb.Subs1.EndTime),
                styleName, text);
        }

        private string FormatDialogPair(List<List<InfoCombined>> combinedAll, int epIdx, int lineIdx)
        {
            var comb = combinedAll[epIdx][lineIdx];
            string pair = "";

            if (_isSubs1First)
            {
                pair += FormatDialogSingle(true, comb);
                if (lineIdx % _duelPattern == 0)
                    pair += "\n" + FormatDialogSingle(false, comb);
            }
            else
            {
                if (lineIdx % _duelPattern == 0)
                    pair += FormatDialogSingle(false, comb) + "\n";
                pair += FormatDialogSingle(true, comb);
            }
            return pair;
        }

        private string FormatQuickRefPair(InfoCombined comb, UtilsName name, int episode, int seqNum)
        {
            string s1 = comb.Subs1.Text, s2 = comb.Subs2.Text;
            string pair = name.createName(ConstantSettings.DuelingQuickRefSubs1Format,
                episode, seqNum, comb.Subs1.StartTime, comb.Subs1.StartTime, s1, s2);

            if (_hasSubs2 && ConstantSettings.DuelingQuickRefSubs2Format != "")
            {
                pair += "\n" + name.createName(ConstantSettings.DuelingQuickRefSubs2Format,
                    episode, seqNum, comb.Subs1.StartTime, comb.Subs1.StartTime, s1, s2);
            }
            return pair;
        }

        // ── Inline Progress Reporter ─────────────────────────

        private class InlineProgressReporter : IProgressReporter
        {
            private readonly ProgressBar _bar;
            public bool Cancel { get; set; }
            public int StepsTotal { get; set; }

            public InlineProgressReporter(ProgressBar bar) { _bar = bar; }

            public void UpdateProgress(int percent, string text)
            {
                Gtk.Application.Invoke((s, e) =>
                {
                    _bar.Fraction = Math.Max(0, Math.Min(1, percent / 100.0));
                    _bar.Text = text;
                });
            }

            public void UpdateProgress(string text)
            {
                Gtk.Application.Invoke((s, e) => { _bar.Text = text; });
            }

            public void NextStep(int step, string description) => UpdateProgress(0, $"[{step}/{StepsTotal}] {description}");
            public void EnableDetail(bool enable) { }
            public void SetDuration(DateTime duration) { }
            public void OnFFmpegOutput(object sender, System.Diagnostics.DataReceivedEventArgs e) { }
        }
    }
}
