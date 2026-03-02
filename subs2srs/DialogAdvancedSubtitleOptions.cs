using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gtk;
using SysPath = System.IO.Path;

namespace subs2srs
{
    public class DialogAdvancedSubtitleOptions : Dialog
    {
        private string _subs1FilePattern = "";
        private string _subs2FilePattern = "";
        private string _subs1Encoding = "";
        private string _subs2Encoding = "";

        // Subs1
        private Entry _txtS1Include, _txtS1Exclude;
        private CheckButton _chkS1Styled, _chkS1NoCounter, _chkS1DupLines;
        private CheckButton _chkS1ExclFewer, _chkS1ExclShorter, _chkS1ExclLonger;
        private SpinButton _spinS1Fewer, _spinS1Shorter, _spinS1Longer;
        private CheckButton _chkS1Join;
        private Entry _txtS1JoinChars;

        // Subs2
        private Entry _txtS2Include, _txtS2Exclude;
        private CheckButton _chkS2Styled, _chkS2NoCounter, _chkS2DupLines;
        private CheckButton _chkS2ExclFewer, _chkS2ExclShorter, _chkS2ExclLonger;
        private SpinButton _spinS2Fewer, _spinS2Shorter, _spinS2Longer;
        private CheckButton _chkS2Join;
        private Entry _txtS2JoinChars;

        // Context
        private SpinButton _spinCtxLeading, _spinCtxTrailing;
        private CheckButton _chkLeadAudio, _chkLeadSnap, _chkLeadVideo;
        private CheckButton _chkTrailAudio, _chkTrailSnap, _chkTrailVideo;
        private SpinButton _spinLeadRange, _spinTrailRange;

        // Actors
        private RadioButton _radioActorS1, _radioActorS2;
        private TreeView _tvActors;
        private ListStore _actorStore;

        // Language
        private CheckButton _chkKanjiOnly;

        public string Subs1FilePattern { set => _subs1FilePattern = value; }
        public string Subs2FilePattern { set => _subs2FilePattern = value; }
        public string Subs1Encoding { set => _subs1Encoding = value; }
        public string Subs2Encoding { set => _subs2Encoding = value; }

        public DialogAdvancedSubtitleOptions(Window parent) : base(
            "Advanced Subtitle Options", parent, DialogFlags.Modal,
            "Cancel", ResponseType.Cancel, "OK", ResponseType.Ok)
        {
            SetDefaultSize(650, 550);
            BuildUI();
            LoadFromSettings();
        }

        private void BuildUI()
        {
            var notebook = new Notebook();
            notebook.AppendPage(BuildSubsPage(1), new Label("Subs1 Filtering"));
            notebook.AppendPage(BuildSubsPage(2), new Label("Subs2 Filtering"));
            notebook.AppendPage(BuildContextPage(), new Label("Context"));
            notebook.AppendPage(BuildActorsPage(), new Label("Actors"));
            notebook.AppendPage(BuildLangPage(), new Label("Language"));
            ContentArea.PackStart(notebook, true, true, 0);
            ContentArea.ShowAll();
        }

        // ── SUBS FILTERING ─────────────────────────────────────────────────

        private Widget BuildSubsPage(int num)
        {
            var vbox = new Box(Orientation.Vertical, 6) { BorderWidth = 8 };
            var grid = new Grid { RowSpacing = 6, ColumnSpacing = 6 };
            int r = 0;

            // Include
            grid.Attach(new Label("Include only (;):") { Halign = Align.End }, 0, r, 1, 1);
            var txtInc = new Entry { Hexpand = true };
            grid.Attach(txtInc, 1, r, 1, 1);
            var btnIncFile = new Button("From File...");
            btnIncFile.Clicked += (s, e) => { var t = LoadSemiFile(); if (t != null) txtInc.Text = t; };
            grid.Attach(btnIncFile, 2, r, 1, 1);
            r++;

            // Exclude
            grid.Attach(new Label("Exclude (;):") { Halign = Align.End }, 0, r, 1, 1);
            var txtExc = new Entry { Hexpand = true };
            grid.Attach(txtExc, 1, r, 1, 1);
            var btnExcFile = new Button("From File...");
            btnExcFile.Clicked += (s, e) => { var t = LoadSemiFile(); if (t != null) txtExc.Text = t; };
            grid.Attach(btnExcFile, 2, r, 1, 1);
            r++;

            vbox.PackStart(grid, false, false, 0);
            vbox.PackStart(new Separator(Orientation.Horizontal), false, false, 2);

            var chkStyled = new CheckButton("Remove styled lines (lines starting with '{')");
            var chkNoCounter = new CheckButton("Remove lines with no counterpart");
            var chkDup = new CheckButton("Exclude duplicate lines");
            vbox.PackStart(chkStyled, false, false, 0);
            vbox.PackStart(chkNoCounter, false, false, 0);
            vbox.PackStart(chkDup, false, false, 0);

            // Fewer chars
            var chkFewer = new CheckButton("Exclude lines fewer than");
            var spinFewer = new SpinButton(1, 999, 1) { Value = 8 };
            var hbFewer = new Box(Orientation.Horizontal, 4);
            hbFewer.PackStart(chkFewer, false, false, 0);
            hbFewer.PackStart(spinFewer, false, false, 0);
            hbFewer.PackStart(new Label("chars"), false, false, 0);
            chkFewer.Toggled += (s, e) => spinFewer.Sensitive = chkFewer.Active;
            vbox.PackStart(hbFewer, false, false, 0);

            // Shorter than
            var chkShorter = new CheckButton("Exclude lines shorter than");
            var spinShorter = new SpinButton(1, 99999, 100) { Value = 800 };
            var hbShorter = new Box(Orientation.Horizontal, 4);
            hbShorter.PackStart(chkShorter, false, false, 0);
            hbShorter.PackStart(spinShorter, false, false, 0);
            hbShorter.PackStart(new Label("ms"), false, false, 0);
            chkShorter.Toggled += (s, e) => spinShorter.Sensitive = chkShorter.Active;
            vbox.PackStart(hbShorter, false, false, 0);

            // Longer than
            var chkLonger = new CheckButton("Exclude lines longer than");
            var spinLonger = new SpinButton(1, 99999, 100) { Value = 5000 };
            var hbLonger = new Box(Orientation.Horizontal, 4);
            hbLonger.PackStart(chkLonger, false, false, 0);
            hbLonger.PackStart(spinLonger, false, false, 0);
            hbLonger.PackStart(new Label("ms"), false, false, 0);
            chkLonger.Toggled += (s, e) => spinLonger.Sensitive = chkLonger.Active;
            vbox.PackStart(hbLonger, false, false, 0);

            // Join sentences
            var chkJoin = new CheckButton("Join sentences ending with:");
            var txtJoin = new Entry { Text = ",、→", WidthChars = 12 };
            var hbJoin = new Box(Orientation.Horizontal, 4);
            hbJoin.PackStart(chkJoin, false, false, 0);
            hbJoin.PackStart(txtJoin, false, false, 0);
            chkJoin.Toggled += (s, e) => txtJoin.Sensitive = chkJoin.Active;
            vbox.PackStart(hbJoin, false, false, 0);

            // Store refs
            if (num == 1)
            {
                _txtS1Include = txtInc; _txtS1Exclude = txtExc;
                _chkS1Styled = chkStyled; _chkS1NoCounter = chkNoCounter; _chkS1DupLines = chkDup;
                _chkS1ExclFewer = chkFewer; _spinS1Fewer = spinFewer;
                _chkS1ExclShorter = chkShorter; _spinS1Shorter = spinShorter;
                _chkS1ExclLonger = chkLonger; _spinS1Longer = spinLonger;
                _chkS1Join = chkJoin; _txtS1JoinChars = txtJoin;
            }
            else
            {
                _txtS2Include = txtInc; _txtS2Exclude = txtExc;
                _chkS2Styled = chkStyled; _chkS2NoCounter = chkNoCounter; _chkS2DupLines = chkDup;
                _chkS2ExclFewer = chkFewer; _spinS2Fewer = spinFewer;
                _chkS2ExclShorter = chkShorter; _spinS2Shorter = spinShorter;
                _chkS2ExclLonger = chkLonger; _spinS2Longer = spinLonger;
                _chkS2Join = chkJoin; _txtS2JoinChars = txtJoin;
            }

            return vbox;
        }

        // ── CONTEXT ─────────────────────────────────────────────────────────

        private Widget BuildContextPage()
        {
            var vbox = new Box(Orientation.Vertical, 6) { BorderWidth = 8 };
            var grid = new Grid { RowSpacing = 6, ColumnSpacing = 8 };
            int r = 0;

            grid.Attach(new Label("Leading context lines:") { Halign = Align.End }, 0, r, 1, 1);
            _spinCtxLeading = new SpinButton(0, 10, 1) { Value = 0 };
            grid.Attach(_spinCtxLeading, 1, r, 1, 1);
            r++;

            _chkLeadAudio = new CheckButton("Include audio clips");
            grid.Attach(_chkLeadAudio, 1, r, 2, 1); r++;
            _chkLeadSnap = new CheckButton("Include snapshots");
            grid.Attach(_chkLeadSnap, 1, r, 2, 1); r++;
            _chkLeadVideo = new CheckButton("Include video clips");
            grid.Attach(_chkLeadVideo, 1, r, 2, 1); r++;

            grid.Attach(new Label("Leading range (sec):") { Halign = Align.End }, 0, r, 1, 1);
            _spinLeadRange = new SpinButton(0, 120, 1) { Value = 15 };
            grid.Attach(_spinLeadRange, 1, r, 1, 1);
            r++;

            grid.Attach(new Separator(Orientation.Horizontal), 0, r, 3, 1); r++;

            grid.Attach(new Label("Trailing context lines:") { Halign = Align.End }, 0, r, 1, 1);
            _spinCtxTrailing = new SpinButton(0, 10, 1) { Value = 0 };
            grid.Attach(_spinCtxTrailing, 1, r, 1, 1);
            r++;

            _chkTrailAudio = new CheckButton("Include audio clips");
            grid.Attach(_chkTrailAudio, 1, r, 2, 1); r++;
            _chkTrailSnap = new CheckButton("Include snapshots");
            grid.Attach(_chkTrailSnap, 1, r, 2, 1); r++;
            _chkTrailVideo = new CheckButton("Include video clips");
            grid.Attach(_chkTrailVideo, 1, r, 2, 1); r++;

            grid.Attach(new Label("Trailing range (sec):") { Halign = Align.End }, 0, r, 1, 1);
            _spinTrailRange = new SpinButton(0, 120, 1) { Value = 15 };
            grid.Attach(_spinTrailRange, 1, r, 1, 1);

            vbox.PackStart(grid, false, false, 0);
            return vbox;
        }

        // ── ACTORS ──────────────────────────────────────────────────────────

        private Widget BuildActorsPage()
        {
            var vbox = new Box(Orientation.Vertical, 6) { BorderWidth = 8 };

            var hbRadio = new Box(Orientation.Horizontal, 8);
            _radioActorS1 = new RadioButton("From Subs1");
            _radioActorS2 = new RadioButton(_radioActorS1, "From Subs2");
            hbRadio.PackStart(_radioActorS1, false, false, 0);
            hbRadio.PackStart(_radioActorS2, false, false, 0);
            _radioActorS1.Toggled += (s, e) => _actorStore.Clear();
            vbox.PackStart(hbRadio, false, false, 0);

            var btnCheck = new Button("Check for Actors");
            btnCheck.Clicked += OnActorCheck;
            vbox.PackStart(btnCheck, false, false, 0);

            _actorStore = new ListStore(typeof(bool), typeof(string));
            _tvActors = new TreeView(_actorStore);
            var togR = new CellRendererToggle { Activatable = true };
            togR.Toggled += (s, e) =>
            {
                if (_actorStore.GetIter(out var it, new Gtk.TreePath(e.Path)))
                {
                    bool cur = (bool)_actorStore.GetValue(it, 0);
                    _actorStore.SetValue(it, 0, !cur);
                }
            };
            _tvActors.AppendColumn("Select", togR, "active", 0);
            _tvActors.AppendColumn("Actor", new CellRendererText(), "text", 1);
            var sw = new ScrolledWindow { ShadowType = ShadowType.In, HeightRequest = 200 };
            sw.Add(_tvActors);
            vbox.PackStart(sw, true, true, 0);

            var hbBtn = new Box(Orientation.Horizontal, 4);
            var btnAll = new Button("All");
            btnAll.Clicked += (s, e) => SetAllActors(true);
            var btnNone = new Button("None");
            btnNone.Clicked += (s, e) => SetAllActors(false);
            var btnInv = new Button("Invert");
            btnInv.Clicked += (s, e) => InvertActors();
            hbBtn.PackStart(btnAll, false, false, 0);
            hbBtn.PackStart(btnNone, false, false, 0);
            hbBtn.PackStart(btnInv, false, false, 0);
            vbox.PackStart(hbBtn, false, false, 0);

            return vbox;
        }

        // ── LANGUAGE ────────────────────────────────────────────────────────

        private Widget BuildLangPage()
        {
            var vbox = new Box(Orientation.Vertical, 6) { BorderWidth = 8 };
            _chkKanjiOnly = new CheckButton("Japanese: Kanji lines only");
            vbox.PackStart(_chkKanjiOnly, false, false, 0);
            return vbox;
        }

        // ── LOAD / SAVE ────────────────────────────────────────────────────

        private void LoadFromSettings()
        {
            var s = Settings.Instance;

            _txtS1Include.Text = UtilsCommon.makeSemiString(s.Subs[0].IncludedWords);
            _txtS1Exclude.Text = UtilsCommon.makeSemiString(s.Subs[0].ExcludedWords);
            _chkS1Styled.Active = s.Subs[0].RemoveStyledLines;
            _chkS1NoCounter.Active = s.Subs[0].RemoveNoCounterpart;
            _chkS1DupLines.Active = s.Subs[0].ExcludeDuplicateLinesEnabled;
            _chkS1ExclFewer.Active = s.Subs[0].ExcludeFewerEnabled;
            _spinS1Fewer.Value = s.Subs[0].ExcludeFewerCount;
            _chkS1ExclShorter.Active = s.Subs[0].ExcludeShorterThanTimeEnabled;
            _spinS1Shorter.Value = s.Subs[0].ExcludeShorterThanTime;
            _chkS1ExclLonger.Active = s.Subs[0].ExcludeLongerThanTimeEnabled;
            _spinS1Longer.Value = s.Subs[0].ExcludeLongerThanTime;
            _chkS1Join.Active = s.Subs[0].JoinSentencesEnabled;
            _txtS1JoinChars.Text = s.Subs[0].JoinSentencesCharList ?? "";

            _txtS2Include.Text = UtilsCommon.makeSemiString(s.Subs[1].IncludedWords);
            _txtS2Exclude.Text = UtilsCommon.makeSemiString(s.Subs[1].ExcludedWords);
            _chkS2Styled.Active = s.Subs[1].RemoveStyledLines;
            _chkS2NoCounter.Active = s.Subs[1].RemoveNoCounterpart;
            _chkS2DupLines.Active = s.Subs[1].ExcludeDuplicateLinesEnabled;
            _chkS2ExclFewer.Active = s.Subs[1].ExcludeFewerEnabled;
            _spinS2Fewer.Value = s.Subs[1].ExcludeFewerCount;
            _chkS2ExclShorter.Active = s.Subs[1].ExcludeShorterThanTimeEnabled;
            _spinS2Shorter.Value = s.Subs[1].ExcludeShorterThanTime;
            _chkS2ExclLonger.Active = s.Subs[1].ExcludeLongerThanTimeEnabled;
            _spinS2Longer.Value = s.Subs[1].ExcludeLongerThanTime;
            _chkS2Join.Active = s.Subs[1].JoinSentencesEnabled;
            _txtS2JoinChars.Text = s.Subs[1].JoinSentencesCharList ?? "";

            _spinCtxLeading.Value = s.ContextLeadingCount;
            _chkLeadAudio.Active = s.ContextLeadingIncludeAudioClips;
            _chkLeadSnap.Active = s.ContextLeadingIncludeSnapshots;
            _chkLeadVideo.Active = s.ContextLeadingIncludeVideoClips;
            _spinLeadRange.Value = s.ContextLeadingRange;

            _spinCtxTrailing.Value = s.ContextTrailingCount;
            _chkTrailAudio.Active = s.ContextTrailingIncludeAudioClips;
            _chkTrailSnap.Active = s.ContextTrailingIncludeSnapshots;
            _chkTrailVideo.Active = s.ContextTrailingIncludeVideoClips;
            _spinTrailRange.Value = s.ContextTrailingRange;

            _radioActorS1.Active = s.Subs[0].ActorsEnabled;
            _radioActorS2.Active = s.Subs[1].ActorsEnabled;

            _chkKanjiOnly.Active = s.LangaugeSpecific.KanjiLinesOnly;

            // Sensitivity
            _spinS1Fewer.Sensitive = _chkS1ExclFewer.Active;
            _spinS1Shorter.Sensitive = _chkS1ExclShorter.Active;
            _spinS1Longer.Sensitive = _chkS1ExclLonger.Active;
            _txtS1JoinChars.Sensitive = _chkS1Join.Active;
            _spinS2Fewer.Sensitive = _chkS2ExclFewer.Active;
            _spinS2Shorter.Sensitive = _chkS2ExclShorter.Active;
            _spinS2Longer.Sensitive = _chkS2ExclLonger.Active;
            _txtS2JoinChars.Sensitive = _chkS2Join.Active;
        }

        public void SaveToSettings()
        {
            var s = Settings.Instance;

            s.Subs[0].IncludedWords = SplitSemi(_txtS1Include.Text);
            s.Subs[0].ExcludedWords = SplitSemi(_txtS1Exclude.Text);
            s.Subs[0].RemoveStyledLines = _chkS1Styled.Active;
            s.Subs[0].RemoveNoCounterpart = _chkS1NoCounter.Active;
            s.Subs[0].ExcludeDuplicateLinesEnabled = _chkS1DupLines.Active;
            s.Subs[0].ExcludeFewerEnabled = _chkS1ExclFewer.Active;
            s.Subs[0].ExcludeFewerCount = (int)_spinS1Fewer.Value;
            s.Subs[0].ExcludeShorterThanTimeEnabled = _chkS1ExclShorter.Active;
            s.Subs[0].ExcludeShorterThanTime = (int)_spinS1Shorter.Value;
            s.Subs[0].ExcludeLongerThanTimeEnabled = _chkS1ExclLonger.Active;
            s.Subs[0].ExcludeLongerThanTime = (int)_spinS1Longer.Value;
            s.Subs[0].JoinSentencesEnabled = _chkS1Join.Active;
            s.Subs[0].JoinSentencesCharList = _txtS1JoinChars.Text.Trim();
            s.Subs[0].ActorsEnabled = _radioActorS1.Active;

            s.Subs[1].IncludedWords = SplitSemi(_txtS2Include.Text);
            s.Subs[1].ExcludedWords = SplitSemi(_txtS2Exclude.Text);
            s.Subs[1].RemoveStyledLines = _chkS2Styled.Active;
            s.Subs[1].RemoveNoCounterpart = _chkS2NoCounter.Active;
            s.Subs[1].ExcludeDuplicateLinesEnabled = _chkS2DupLines.Active;
            s.Subs[1].ExcludeFewerEnabled = _chkS2ExclFewer.Active;
            s.Subs[1].ExcludeFewerCount = (int)_spinS2Fewer.Value;
            s.Subs[1].ExcludeShorterThanTimeEnabled = _chkS2ExclShorter.Active;
            s.Subs[1].ExcludeShorterThanTime = (int)_spinS2Shorter.Value;
            s.Subs[1].ExcludeLongerThanTimeEnabled = _chkS2ExclLonger.Active;
            s.Subs[1].ExcludeLongerThanTime = (int)_spinS2Longer.Value;
            s.Subs[1].JoinSentencesEnabled = _chkS2Join.Active;
            s.Subs[1].JoinSentencesCharList = _txtS2JoinChars.Text.Trim();
            s.Subs[1].ActorsEnabled = _radioActorS2.Active;

            s.ContextLeadingCount = (int)_spinCtxLeading.Value;
            s.ContextLeadingIncludeAudioClips = _chkLeadAudio.Active;
            s.ContextLeadingIncludeSnapshots = _chkLeadSnap.Active;
            s.ContextLeadingIncludeVideoClips = _chkLeadVideo.Active;
            s.ContextLeadingRange = (int)_spinLeadRange.Value;

            s.ContextTrailingCount = (int)_spinCtxTrailing.Value;
            s.ContextTrailingIncludeAudioClips = _chkTrailAudio.Active;
            s.ContextTrailingIncludeSnapshots = _chkTrailSnap.Active;
            s.ContextTrailingIncludeVideoClips = _chkTrailVideo.Active;
            s.ContextTrailingRange = (int)_spinTrailRange.Value;

            s.LangaugeSpecific.KanjiLinesOnly = _chkKanjiOnly.Active;

            // Actors
            s.ActorList.Clear();
            if (_actorStore.GetIterFirst(out var iter))
            {
                do
                {
                    if ((bool)_actorStore.GetValue(iter, 0))
                        s.ActorList.Add((string)_actorStore.GetValue(iter, 1));
                } while (_actorStore.IterNext(ref iter));
            }
        }

        // ── ACTORS LOGIC ────────────────────────────────────────────────────

        private void OnActorCheck(object s, EventArgs e)
        {
            _actorStore.Clear();

            string pattern = _radioActorS1.Active ? _subs1FilePattern : _subs2FilePattern;
            string enc = _radioActorS1.Active ? _subs1Encoding : _subs2Encoding;
            int subsNum = _radioActorS1.Active ? 1 : 2;

            if (string.IsNullOrEmpty(pattern))
            {
                UtilsMsg.showErrMsg("Can't check - subtitle file isn't valid.");
                return;
            }

            var files = UtilsCommon.getNonHiddenFiles(pattern);
            if (files.Length == 0)
            {
                UtilsMsg.showErrMsg("Can't check - No files found.");
                return;
            }

            foreach (string f in files)
            {
                string ext = SysPath.GetExtension(f).ToLower();
                if (ext != ".ass" && ext != ".ssa")
                {
                    UtilsMsg.showErrMsg("Only .ass/.ssa formats supported for actor detection.");
                    return;
                }
            }

            var actorList = new List<string>();
            Encoding fileEnc;
            try { fileEnc = Encoding.GetEncoding(InfoEncoding.longToShort(enc)); }
            catch { fileEnc = Encoding.UTF8; }

            foreach (string file in files)
            {
                var parser = new SubsParserASS(null, file, fileEnc, subsNum);
                var lines = parser.parse();
                foreach (var info in lines)
                {
                    string actor = info.Actor.Trim();
                    if (!actorList.Contains(actor))
                        actorList.Add(actor);
                }
            }

            foreach (string actor in actorList)
                _actorStore.AppendValues(true, actor);
        }

        private void SetAllActors(bool val)
        {
            if (!_actorStore.GetIterFirst(out var iter)) return;
            do { _actorStore.SetValue(iter, 0, val); } while (_actorStore.IterNext(ref iter));
        }

        private void InvertActors()
        {
            if (!_actorStore.GetIterFirst(out var iter)) return;
            do
            {
                bool cur = (bool)_actorStore.GetValue(iter, 0);
                _actorStore.SetValue(iter, 0, !cur);
            } while (_actorStore.IterNext(ref iter));
        }

        // ── HELPERS ─────────────────────────────────────────────────────────

        private string[] SplitSemi(string text) =>
            UtilsCommon.removeExtraSpaces(
                text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

        private string LoadSemiFile()
        {
            var dlg = new FileChooserDialog("Select text file", this,
                FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
            var filter = new FileFilter();
            filter.AddPattern("*.txt");
            filter.Name = "Text files";
            dlg.AddFilter(filter);

            string result = null;
            if (dlg.Run() == (int)ResponseType.Accept)
            {
                try
                {
                    string text = File.ReadAllText(dlg.Filename).Trim();
                    var words = text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    result = UtilsCommon.makeSemiString(words);
                }
                catch { UtilsMsg.showErrMsg("Could not open file."); }
            }
            dlg.Destroy();
            return result;
        }
    }
}
