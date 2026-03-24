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
using SysPath = System.IO.Path;

namespace subs2srs
{
    /// <summary>
    /// Advanced subtitle filtering, context, actors, language options.
    /// GTK4: Dialog → Window, RadioButton → grouped CheckButton,
    /// TreeView+ListStore kept (still available in GTK4), ComboBoxText → DropDown.
    /// </summary>
    public class DialogAdvancedSubtitleOptions : Gtk.Window
    {
        private string _subs1FilePattern = "";
        private string _subs2FilePattern = "";
        private string _subs1Encoding = "";
        private string _subs2Encoding = "";

        // Subs1
        private Gtk.Entry _txtS1Include, _txtS1Exclude;
        private Gtk.CheckButton _chkS1Styled, _chkS1NoCounter, _chkS1DupLines;
        private Gtk.CheckButton _chkS1ExclFewer, _chkS1ExclShorter, _chkS1ExclLonger;
        private Gtk.SpinButton _spinS1Fewer, _spinS1Shorter, _spinS1Longer;
        private Gtk.CheckButton _chkS1Join;
        private Gtk.Entry _txtS1JoinChars;

        // Subs2
        private Gtk.Entry _txtS2Include, _txtS2Exclude;
        private Gtk.CheckButton _chkS2Styled, _chkS2NoCounter, _chkS2DupLines;
        private Gtk.CheckButton _chkS2ExclFewer, _chkS2ExclShorter, _chkS2ExclLonger;
        private Gtk.SpinButton _spinS2Fewer, _spinS2Shorter, _spinS2Longer;
        private Gtk.CheckButton _chkS2Join;
        private Gtk.Entry _txtS2JoinChars;

        // Context
        private Gtk.SpinButton _spinCtxLeading, _spinCtxTrailing;
        private Gtk.CheckButton _chkLeadAudio, _chkLeadSnap, _chkLeadVideo;
        private Gtk.CheckButton _chkTrailAudio, _chkTrailSnap, _chkTrailVideo;
        private Gtk.SpinButton _spinLeadRange, _spinTrailRange;

        // Actors — using grouped CheckButtons instead of RadioButton
        private Gtk.CheckButton _radioActorS1, _radioActorS2;
        private Gtk.ListView _lvActors;
        private Gio.ListStore _actorStore;
        // We'll use a simple list of (bool selected, string name) via string encoding
        // Actually simpler: parallel lists
        private List<bool> _actorSelected = new();
        private List<string> _actorNames = new();

        // Language
        private Gtk.CheckButton _chkKanjiOnly;

        // Dialog result
        private bool? _result;
        private GLib.MainLoop _loop;

        public string Subs1FilePattern { set => _subs1FilePattern = value; }
        public string Subs2FilePattern { set => _subs2FilePattern = value; }
        public string Subs1Encoding { set => _subs1Encoding = value; }
        public string Subs2Encoding { set => _subs2Encoding = value; }

        public DialogAdvancedSubtitleOptions(Gtk.Window parent)
        {
            SetTitle("Advanced Subtitle Options");
            SetDefaultSize(650, 550);
            SetModal(true);
            if (parent != null) SetTransientFor(parent);

            BuildUI();
            LoadFromSettings();
        }

        public bool RunDialog()
        {
            _result = null;
            _loop = GLib.MainLoop.New(null, false);
            OnCloseRequest += (s, e) =>
            {
                if (_result == null) _result = false;
                _loop.Quit();
                return false;
            };
            Show();
            _loop.Run();
            return _result ?? false;
        }

        // Alias expected by MainWindow (1 = OK, 0 = Cancel)
        public int Run() => RunDialog() ? 1 : 0;

        private void BuildUI()
        {
            var outerBox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);

            var notebook = Gtk.Notebook.New();
            notebook.AppendPage(BuildSubsPage(1), Gtk.Label.New("Subs1 Filtering"));
            notebook.AppendPage(BuildSubsPage(2), Gtk.Label.New("Subs2 Filtering"));
            notebook.AppendPage(BuildContextPage(), Gtk.Label.New("Context"));
            notebook.AppendPage(BuildActorsPage(), Gtk.Label.New("Actors"));
            notebook.AppendPage(BuildLangPage(), Gtk.Label.New("Language"));
            outerBox.Append(notebook);

            // OK / Cancel
            var btnRow = Gtk.Box.New(Gtk.Orientation.Horizontal, 8);
            btnRow.SetHalign(Gtk.Align.End);
            btnRow.SetMarginTop(6); btnRow.SetMarginEnd(8); btnRow.SetMarginBottom(8);

            var btnCancel = Gtk.Button.NewWithLabel("Cancel");
            btnCancel.OnClicked += (s, e) => { _result = false; Close(); };
            btnRow.Append(btnCancel);

            var btnOk = Gtk.Button.NewWithLabel("OK");
            btnOk.OnClicked += (s, e) => { _result = true; Close(); };
            btnRow.Append(btnOk);

            outerBox.Append(btnRow);
            SetChild(outerBox);
        }

        // ── SUBS FILTERING ─────────────────────────────────────────────────

        private Gtk.Widget BuildSubsPage(int num)
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            vbox.SetMarginTop(8); vbox.SetMarginBottom(8);
            vbox.SetMarginStart(8); vbox.SetMarginEnd(8);

            var grid = Gtk.Grid.New();
            grid.SetRowSpacing(6); grid.SetColumnSpacing(6);
            int r = 0;

            // Include
            var lblInc = Gtk.Label.New("Include only (;):"); lblInc.SetHalign(Gtk.Align.End);
            grid.Attach(lblInc, 0, r, 1, 1);
            var txtInc = Gtk.Entry.New(); txtInc.SetHexpand(true);
            grid.Attach(txtInc, 1, r, 1, 1);
            var btnIncFile = Gtk.Button.NewWithLabel("From File...");
            btnIncFile.OnClicked += (s, e) => LoadSemiFileInto(txtInc);
            grid.Attach(btnIncFile, 2, r, 1, 1);
            r++;

            // Exclude
            var lblExc = Gtk.Label.New("Exclude (;):"); lblExc.SetHalign(Gtk.Align.End);
            grid.Attach(lblExc, 0, r, 1, 1);
            var txtExc = Gtk.Entry.New(); txtExc.SetHexpand(true);
            grid.Attach(txtExc, 1, r, 1, 1);
            var btnExcFile = Gtk.Button.NewWithLabel("From File...");
            btnExcFile.OnClicked += (s, e) => LoadSemiFileInto(txtExc);
            grid.Attach(btnExcFile, 2, r, 1, 1);
            r++;

            vbox.Append(grid);
            vbox.Append(Gtk.Separator.New(Gtk.Orientation.Horizontal));

            var chkStyled = Gtk.CheckButton.NewWithLabel("Remove styled lines (lines starting with '{')");
            var chkNoCounter = Gtk.CheckButton.NewWithLabel("Remove lines with no counterpart");
            var chkDup = Gtk.CheckButton.NewWithLabel("Exclude duplicate lines");
            vbox.Append(chkStyled);
            vbox.Append(chkNoCounter);
            vbox.Append(chkDup);

            // Fewer chars
            var chkFewer = Gtk.CheckButton.NewWithLabel("Exclude lines fewer than");
            var spinFewer = Gtk.SpinButton.NewWithRange(1, 999, 1); spinFewer.SetValue(8);
            var hbFewer = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
            hbFewer.Append(chkFewer); hbFewer.Append(spinFewer);
            hbFewer.Append(Gtk.Label.New("chars"));
            chkFewer.OnToggled += (s, e) => spinFewer.SetSensitive(chkFewer.GetActive());
            vbox.Append(hbFewer);

            // Shorter than
            var chkShorter = Gtk.CheckButton.NewWithLabel("Exclude lines shorter than");
            var spinShorter = Gtk.SpinButton.NewWithRange(1, 99999, 100); spinShorter.SetValue(800);
            var hbShorter = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
            hbShorter.Append(chkShorter); hbShorter.Append(spinShorter);
            hbShorter.Append(Gtk.Label.New("ms"));
            chkShorter.OnToggled += (s, e) => spinShorter.SetSensitive(chkShorter.GetActive());
            vbox.Append(hbShorter);

            // Longer than
            var chkLonger = Gtk.CheckButton.NewWithLabel("Exclude lines longer than");
            var spinLonger = Gtk.SpinButton.NewWithRange(1, 99999, 100); spinLonger.SetValue(5000);
            var hbLonger = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
            hbLonger.Append(chkLonger); hbLonger.Append(spinLonger);
            hbLonger.Append(Gtk.Label.New("ms"));
            chkLonger.OnToggled += (s, e) => spinLonger.SetSensitive(chkLonger.GetActive());
            vbox.Append(hbLonger);

            // Join sentences
            var chkJoin = Gtk.CheckButton.NewWithLabel("Join sentences ending with:");
            var txtJoin = Gtk.Entry.New(); txtJoin.SetText(",、→"); txtJoin.SetWidthChars(12);
            var hbJoin = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
            hbJoin.Append(chkJoin); hbJoin.Append(txtJoin);
            chkJoin.OnToggled += (s, e) => txtJoin.SetSensitive(chkJoin.GetActive());
            vbox.Append(hbJoin);

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

        private Gtk.Widget BuildContextPage()
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            vbox.SetMarginTop(8); vbox.SetMarginBottom(8);
            vbox.SetMarginStart(8); vbox.SetMarginEnd(8);

            var grid = Gtk.Grid.New();
            grid.SetRowSpacing(6); grid.SetColumnSpacing(8);
            int r = 0;

            var lbl1 = Gtk.Label.New("Leading context lines:"); lbl1.SetHalign(Gtk.Align.End);
            grid.Attach(lbl1, 0, r, 1, 1);
            _spinCtxLeading = Gtk.SpinButton.NewWithRange(0, 10, 1); _spinCtxLeading.SetValue(0);
            grid.Attach(_spinCtxLeading, 1, r, 1, 1);
            r++;

            _chkLeadAudio = Gtk.CheckButton.NewWithLabel("Include audio clips");
            grid.Attach(_chkLeadAudio, 1, r, 2, 1); r++;
            _chkLeadSnap = Gtk.CheckButton.NewWithLabel("Include snapshots");
            grid.Attach(_chkLeadSnap, 1, r, 2, 1); r++;
            _chkLeadVideo = Gtk.CheckButton.NewWithLabel("Include video clips");
            grid.Attach(_chkLeadVideo, 1, r, 2, 1); r++;

            var lbl2 = Gtk.Label.New("Leading range (sec):"); lbl2.SetHalign(Gtk.Align.End);
            grid.Attach(lbl2, 0, r, 1, 1);
            _spinLeadRange = Gtk.SpinButton.NewWithRange(0, 120, 1); _spinLeadRange.SetValue(15);
            grid.Attach(_spinLeadRange, 1, r, 1, 1);
            r++;

            grid.Attach(Gtk.Separator.New(Gtk.Orientation.Horizontal), 0, r, 3, 1); r++;

            var lbl3 = Gtk.Label.New("Trailing context lines:"); lbl3.SetHalign(Gtk.Align.End);
            grid.Attach(lbl3, 0, r, 1, 1);
            _spinCtxTrailing = Gtk.SpinButton.NewWithRange(0, 10, 1); _spinCtxTrailing.SetValue(0);
            grid.Attach(_spinCtxTrailing, 1, r, 1, 1);
            r++;

            _chkTrailAudio = Gtk.CheckButton.NewWithLabel("Include audio clips");
            grid.Attach(_chkTrailAudio, 1, r, 2, 1); r++;
            _chkTrailSnap = Gtk.CheckButton.NewWithLabel("Include snapshots");
            grid.Attach(_chkTrailSnap, 1, r, 2, 1); r++;
            _chkTrailVideo = Gtk.CheckButton.NewWithLabel("Include video clips");
            grid.Attach(_chkTrailVideo, 1, r, 2, 1); r++;

            var lbl4 = Gtk.Label.New("Trailing range (sec):"); lbl4.SetHalign(Gtk.Align.End);
            grid.Attach(lbl4, 0, r, 1, 1);
            _spinTrailRange = Gtk.SpinButton.NewWithRange(0, 120, 1); _spinTrailRange.SetValue(15);
            grid.Attach(_spinTrailRange, 1, r, 1, 1);

            vbox.Append(grid);
            return vbox;
        }

        // ── ACTORS ──────────────────────────────────────────────────────────

        private Gtk.Widget BuildActorsPage()
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            vbox.SetMarginTop(8); vbox.SetMarginBottom(8);
            vbox.SetMarginStart(8); vbox.SetMarginEnd(8);

            var hbRadio = Gtk.Box.New(Gtk.Orientation.Horizontal, 8);
            _radioActorS1 = Gtk.CheckButton.NewWithLabel("From Subs1");
            _radioActorS2 = Gtk.CheckButton.NewWithLabel("From Subs2");
            _radioActorS2.SetGroup(_radioActorS1);
            _radioActorS1.SetActive(true);
            hbRadio.Append(_radioActorS1);
            hbRadio.Append(_radioActorS2);
            vbox.Append(hbRadio);

            var btnCheck = Gtk.Button.NewWithLabel("Check for Actors");
            btnCheck.OnClicked += OnActorCheck;
            vbox.Append(btnCheck);

            // Simple list using Gtk.StringList + Gtk.ListView for actor names
            // For toggle functionality we use a simple list box approach
            _actorStore = Gio.ListStore.New(Gtk.StringObject.GetGType());

            var factory = Gtk.SignalListItemFactory.New();
            factory.OnSetup += (f, args) =>
            {
                var li = (Gtk.ListItem)args.Object;
                var chk = Gtk.CheckButton.NewWithLabel("");
                li.SetChild(chk);
            };
            factory.OnBind += (f, args) =>
            {
                var li = (Gtk.ListItem)args.Object;
                var chk = (Gtk.CheckButton)li.GetChild();
                var strObj = (Gtk.StringObject)li.GetItem();
                int idx = (int)li.GetPosition();
                chk.SetLabel(strObj.GetString());
                if (idx < _actorSelected.Count)
                    chk.SetActive(_actorSelected[idx]);
                chk.OnToggled += (s2, e2) =>
                {
                    int pos = (int)li.GetPosition();
                    if (pos < _actorSelected.Count)
                        _actorSelected[pos] = chk.GetActive();
                };
            };

            var sel = Gtk.NoSelection.New(_actorStore);
            _lvActors = Gtk.ListView.New(sel, factory);
            _lvActors.SetVexpand(true);

            var sw = Gtk.ScrolledWindow.New();
            sw.SetSizeRequest(-1, 200);
            sw.SetChild(_lvActors);
            vbox.Append(sw);

            var hbBtn = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
            var btnAll = Gtk.Button.NewWithLabel("All");
            btnAll.OnClicked += (s, e) => SetAllActors(true);
            var btnNone = Gtk.Button.NewWithLabel("None");
            btnNone.OnClicked += (s, e) => SetAllActors(false);
            var btnInv = Gtk.Button.NewWithLabel("Invert");
            btnInv.OnClicked += (s, e) => InvertActors();
            hbBtn.Append(btnAll); hbBtn.Append(btnNone); hbBtn.Append(btnInv);
            vbox.Append(hbBtn);

            return vbox;
        }

        // ── LANGUAGE ────────────────────────────────────────────────────────

        private Gtk.Widget BuildLangPage()
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            vbox.SetMarginTop(8); vbox.SetMarginBottom(8);
            vbox.SetMarginStart(8); vbox.SetMarginEnd(8);
            _chkKanjiOnly = Gtk.CheckButton.NewWithLabel("Japanese: Kanji lines only");
            vbox.Append(_chkKanjiOnly);
            return vbox;
        }

        // ── LOAD / SAVE ────────────────────────────────────────────────────

        private void LoadFromSettings()
        {
            var s = Settings.Instance;

            _txtS1Include.SetText(UtilsCommon.makeSemiString(s.Subs[0].IncludedWords));
            _txtS1Exclude.SetText(UtilsCommon.makeSemiString(s.Subs[0].ExcludedWords));
            _chkS1Styled.SetActive(s.Subs[0].RemoveStyledLines);
            _chkS1NoCounter.SetActive(s.Subs[0].RemoveNoCounterpart);
            _chkS1DupLines.SetActive(s.Subs[0].ExcludeDuplicateLinesEnabled);
            _chkS1ExclFewer.SetActive(s.Subs[0].ExcludeFewerEnabled);
            _spinS1Fewer.SetValue(s.Subs[0].ExcludeFewerCount);
            _chkS1ExclShorter.SetActive(s.Subs[0].ExcludeShorterThanTimeEnabled);
            _spinS1Shorter.SetValue(s.Subs[0].ExcludeShorterThanTime);
            _chkS1ExclLonger.SetActive(s.Subs[0].ExcludeLongerThanTimeEnabled);
            _spinS1Longer.SetValue(s.Subs[0].ExcludeLongerThanTime);
            _chkS1Join.SetActive(s.Subs[0].JoinSentencesEnabled);
            _txtS1JoinChars.SetText(s.Subs[0].JoinSentencesCharList ?? "");

            _txtS2Include.SetText(UtilsCommon.makeSemiString(s.Subs[1].IncludedWords));
            _txtS2Exclude.SetText(UtilsCommon.makeSemiString(s.Subs[1].ExcludedWords));
            _chkS2Styled.SetActive(s.Subs[1].RemoveStyledLines);
            _chkS2NoCounter.SetActive(s.Subs[1].RemoveNoCounterpart);
            _chkS2DupLines.SetActive(s.Subs[1].ExcludeDuplicateLinesEnabled);
            _chkS2ExclFewer.SetActive(s.Subs[1].ExcludeFewerEnabled);
            _spinS2Fewer.SetValue(s.Subs[1].ExcludeFewerCount);
            _chkS2ExclShorter.SetActive(s.Subs[1].ExcludeShorterThanTimeEnabled);
            _spinS2Shorter.SetValue(s.Subs[1].ExcludeShorterThanTime);
            _chkS2ExclLonger.SetActive(s.Subs[1].ExcludeLongerThanTimeEnabled);
            _spinS2Longer.SetValue(s.Subs[1].ExcludeLongerThanTime);
            _chkS2Join.SetActive(s.Subs[1].JoinSentencesEnabled);
            _txtS2JoinChars.SetText(s.Subs[1].JoinSentencesCharList ?? "");

            _spinCtxLeading.SetValue(s.ContextLeadingCount);
            _chkLeadAudio.SetActive(s.ContextLeadingIncludeAudioClips);
            _chkLeadSnap.SetActive(s.ContextLeadingIncludeSnapshots);
            _chkLeadVideo.SetActive(s.ContextLeadingIncludeVideoClips);
            _spinLeadRange.SetValue(s.ContextLeadingRange);

            _spinCtxTrailing.SetValue(s.ContextTrailingCount);
            _chkTrailAudio.SetActive(s.ContextTrailingIncludeAudioClips);
            _chkTrailSnap.SetActive(s.ContextTrailingIncludeSnapshots);
            _chkTrailVideo.SetActive(s.ContextTrailingIncludeVideoClips);
            _spinTrailRange.SetValue(s.ContextTrailingRange);

            _radioActorS1.SetActive(s.Subs[0].ActorsEnabled);
            _radioActorS2.SetActive(s.Subs[1].ActorsEnabled);

            _chkKanjiOnly.SetActive(s.LanguageSpecific.KanjiLinesOnly);

            // Sensitivity
            _spinS1Fewer.SetSensitive(_chkS1ExclFewer.GetActive());
            _spinS1Shorter.SetSensitive(_chkS1ExclShorter.GetActive());
            _spinS1Longer.SetSensitive(_chkS1ExclLonger.GetActive());
            _txtS1JoinChars.SetSensitive(_chkS1Join.GetActive());
            _spinS2Fewer.SetSensitive(_chkS2ExclFewer.GetActive());
            _spinS2Shorter.SetSensitive(_chkS2ExclShorter.GetActive());
            _spinS2Longer.SetSensitive(_chkS2ExclLonger.GetActive());
            _txtS2JoinChars.SetSensitive(_chkS2Join.GetActive());
        }

        public void SaveToSettings()
        {
            var s = Settings.Instance;

            s.Subs[0].IncludedWords = SplitSemi(_txtS1Include.GetText());
            s.Subs[0].ExcludedWords = SplitSemi(_txtS1Exclude.GetText());
            s.Subs[0].RemoveStyledLines = _chkS1Styled.GetActive();
            s.Subs[0].RemoveNoCounterpart = _chkS1NoCounter.GetActive();
            s.Subs[0].ExcludeDuplicateLinesEnabled = _chkS1DupLines.GetActive();
            s.Subs[0].ExcludeFewerEnabled = _chkS1ExclFewer.GetActive();
            s.Subs[0].ExcludeFewerCount = (int)_spinS1Fewer.GetValue();
            s.Subs[0].ExcludeShorterThanTimeEnabled = _chkS1ExclShorter.GetActive();
            s.Subs[0].ExcludeShorterThanTime = (int)_spinS1Shorter.GetValue();
            s.Subs[0].ExcludeLongerThanTimeEnabled = _chkS1ExclLonger.GetActive();
            s.Subs[0].ExcludeLongerThanTime = (int)_spinS1Longer.GetValue();
            s.Subs[0].JoinSentencesEnabled = _chkS1Join.GetActive();
            s.Subs[0].JoinSentencesCharList = _txtS1JoinChars.GetText().Trim();
            s.Subs[0].ActorsEnabled = _radioActorS1.GetActive();

            s.Subs[1].IncludedWords = SplitSemi(_txtS2Include.GetText());
            s.Subs[1].ExcludedWords = SplitSemi(_txtS2Exclude.GetText());
            s.Subs[1].RemoveStyledLines = _chkS2Styled.GetActive();
            s.Subs[1].RemoveNoCounterpart = _chkS2NoCounter.GetActive();
            s.Subs[1].ExcludeDuplicateLinesEnabled = _chkS2DupLines.GetActive();
            s.Subs[1].ExcludeFewerEnabled = _chkS2ExclFewer.GetActive();
            s.Subs[1].ExcludeFewerCount = (int)_spinS2Fewer.GetValue();
            s.Subs[1].ExcludeShorterThanTimeEnabled = _chkS2ExclShorter.GetActive();
            s.Subs[1].ExcludeShorterThanTime = (int)_spinS2Shorter.GetValue();
            s.Subs[1].ExcludeLongerThanTimeEnabled = _chkS2ExclLonger.GetActive();
            s.Subs[1].ExcludeLongerThanTime = (int)_spinS2Longer.GetValue();
            s.Subs[1].JoinSentencesEnabled = _chkS2Join.GetActive();
            s.Subs[1].JoinSentencesCharList = _txtS2JoinChars.GetText().Trim();
            s.Subs[1].ActorsEnabled = _radioActorS2.GetActive();

            s.ContextLeadingCount = (int)_spinCtxLeading.GetValue();
            s.ContextLeadingIncludeAudioClips = _chkLeadAudio.GetActive();
            s.ContextLeadingIncludeSnapshots = _chkLeadSnap.GetActive();
            s.ContextLeadingIncludeVideoClips = _chkLeadVideo.GetActive();
            s.ContextLeadingRange = (int)_spinLeadRange.GetValue();

            s.ContextTrailingCount = (int)_spinCtxTrailing.GetValue();
            s.ContextTrailingIncludeAudioClips = _chkTrailAudio.GetActive();
            s.ContextTrailingIncludeSnapshots = _chkTrailSnap.GetActive();
            s.ContextTrailingIncludeVideoClips = _chkTrailVideo.GetActive();
            s.ContextTrailingRange = (int)_spinTrailRange.GetValue();

            s.LanguageSpecific.KanjiLinesOnly = _chkKanjiOnly.GetActive();

            // Actors
            s.ActorList.Clear();
            for (int i = 0; i < _actorNames.Count; i++)
            {
                if (i < _actorSelected.Count && _actorSelected[i])
                    s.ActorList.Add(_actorNames[i]);
            }
        }

        // ── ACTORS LOGIC ────────────────────────────────────────────────────

        private void OnActorCheck(Gtk.Button sender, EventArgs e)
        {
            _actorNames.Clear();
            _actorSelected.Clear();
            // Clear the Gio.ListStore
            while (_actorStore.GetNItems() > 0)
                _actorStore.Remove(0);

            string pattern = _radioActorS1.GetActive() ? _subs1FilePattern : _subs2FilePattern;
            string enc = _radioActorS1.GetActive() ? _subs1Encoding : _subs2Encoding;
            int subsNum = _radioActorS1.GetActive() ? 1 : 2;

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
                    if (!_actorNames.Contains(actor))
                        _actorNames.Add(actor);
                }
            }

            foreach (string actor in _actorNames)
            {
                _actorSelected.Add(true);
                _actorStore.Append(Gtk.StringObject.New(actor));
            }
        }

        private void SetAllActors(bool val)
        {
            for (int i = 0; i < _actorSelected.Count; i++)
                _actorSelected[i] = val;
            // Force rebind by removing/re-adding items
            RefreshActorList();
        }

        private void InvertActors()
        {
            for (int i = 0; i < _actorSelected.Count; i++)
                _actorSelected[i] = !_actorSelected[i];
            RefreshActorList();
        }

        private void RefreshActorList()
        {
            while (_actorStore.GetNItems() > 0)
                _actorStore.Remove(0);
            foreach (string actor in _actorNames)
                _actorStore.Append(Gtk.StringObject.New(actor));
        }

        // ── HELPERS ─────────────────────────────────────────────────────────

        private string[] SplitSemi(string text) =>
            UtilsCommon.removeExtraSpaces(
                text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

        private async void LoadSemiFileInto(Gtk.Entry target)
        {
            var dlg = Gtk.FileDialog.New();
            dlg.SetTitle("Select text file");

            var filter = Gtk.FileFilter.New();
            filter.AddPattern("*.txt");
            filter.SetName("Text files");
            var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
            filters.Append(filter);
            dlg.SetFilters(filters);

            try
            {
                var file = await dlg.OpenAsync(this);
                if (file != null)
                {
                    string path = file.GetPath() ?? "";
                    if (path != "")
                    {
                        string text = File.ReadAllText(path).Trim();
                        var words = text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        target.SetText(UtilsCommon.makeSemiString(words));
                    }
                }
            }
            catch { /* user cancelled */ }
        }
    }
}
