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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using SysPath = System.IO.Path;

namespace subs2srs
{
    /// <summary>
    /// Preview window — GTK4/Gir.Core port.
    ///
    /// GTK4 removed TreeView/ListStore. This port uses Gtk.ColumnView
    /// backed by a Gio.ListStore of PreviewItem GObject wrappers,
    /// providing a sortable, scrollable multi-column list.
    ///
    /// Snapshot preview uses Gtk.Picture (replaces Gtk.Image with Pixbuf).
    /// </summary>
    public class DialogPreview : Gtk.Window
    {
        // CSS class names for row background styling
        private const string RowActiveCss = "preview-row-active";
        private const string RowInactiveCss = "preview-row-inactive";
        private const string WarnCss = "color: #FF0000;";

        // Widgets
        private Gtk.DropDown _comboEp;
        private Gtk.StringList _epModel;
        private Gtk.ListView _listView;
        private Gio.ListStore _store;
        private List<PreviewItem> _items = new();
        private Gtk.SingleSelection _selection;
        private Gtk.Entry _txtS1, _txtS2, _txtFind;
        private Gtk.Label _lblTime;
        private Gtk.Picture _imgSnap;
        private Gtk.CheckButton _chkSnap;
        private Gtk.Button _btnAudio, _btnGo;
        private Gtk.Label _lblEpL, _lblEpA, _lblEpI, _lblTL, _lblTA, _lblTI;
        private Gtk.ProgressBar _progress;

        // State
        private WorkerVars _wv;
        private InfoCombined _cur;
        private bool _guard, _changed;
        private int _findIdx;
        private bool _running;
        private bool _destroyed;

        /// <summary>Whether this window has been destroyed or hidden permanently.</summary>
        public bool IsDestroyed => _destroyed;

        public event EventHandler RefreshSettings;

        /// <summary>
        /// Raised when the user clicks Go in Preview.
        /// MainWindow subscribes and triggers its own Go logic.
        /// </summary>
        public event EventHandler GoRequested;

        public DialogPreview() : base()
        {
            SetTitle("Preview");
            SetDefaultSize(1000, 750);

            // Hide instead of destroying on close
            OnCloseRequest += (s, e) =>
            {
                SetVisible(false);
                return true; // prevent destruction
            };

            BuildUI();
        }

        public void StartPreview()
        {
            _destroyed = false;
            Show();
            PopulateEpCombo();
            RunPreviewAsync();
        }

        /// <summary>
        /// Clean up temp files and dispose snapshot texture.
        /// Called internally and by MainWindow on application exit.
        /// </summary>
        public void CleanupAndDestroy()
        {
            _destroyed = true;
            Cleanup();
            Close();
        }

        private void Cleanup()
        {
            // Clear snapshot picture
            try { _imgSnap?.SetPaintable(null); } catch { }

            if (_wv?.MediaDir != null && Directory.Exists(_wv.MediaDir))
                try { Directory.Delete(_wv.MediaDir, true); } catch { }
        }

        // ── UI ──────────────────────────────────────────────────────────────

        private void BuildUI()
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 4);
            vbox.SetMarginTop(6);
            vbox.SetMarginBottom(6);
            vbox.SetMarginStart(6);
            vbox.SetMarginEnd(6);

            // Top bar
            var top = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
            top.Append(Gtk.Label.New("Episode:"));
            _epModel = Gtk.StringList.New(Array.Empty<string>());
            _comboEp = Gtk.DropDown.New(_epModel, null);
            _comboEp.OnNotify += (s, e) =>
            {
                if (e.Pspec.GetName() == "selected") OnEpisodeChanged();
            };
            top.Append(_comboEp);
            var btnRegen = Gtk.Button.NewWithLabel("Regenerate");
            btnRegen.OnClicked += OnRegenClicked;
            top.Append(btnRegen);
            vbox.Append(top);

            // List view — takes all available vertical space
            _store = Gio.ListStore.New(Gtk.StringObject.GetGType());
            _selection = Gtk.SingleSelection.New(_store);
            _selection.OnNotify += (s, e) =>
            {
                if (e.Pspec.GetName() == "selected") OnSelChanged();
            };

            _listView = Gtk.ListView.New(_selection, BuildListItemFactory());
            _listView.SetVexpand(true);

            var sw = Gtk.ScrolledWindow.New();
            sw.SetChild(_listView);
            sw.SetVexpand(true);
            // Ensure the list keeps space — minimum height
            sw.SetSizeRequest(-1, 250);
            vbox.Append(sw);

            // === Bottom detail panel — fixed size, never expands ===
            var detailBox = Gtk.Box.New(Gtk.Orientation.Vertical, 4);
            detailBox.SetVexpand(false);
            detailBox.SetValign(Gtk.Align.End);

            // Text fields grid
            var pg = Gtk.Grid.New();
            pg.SetRowSpacing(4);
            pg.SetColumnSpacing(6);
            pg.SetVexpand(false);

            var lblS1 = Gtk.Label.New("Subs1:");
            lblS1.SetHalign(Gtk.Align.End);
            pg.Attach(lblS1, 0, 0, 1, 1);
            _txtS1 = Gtk.Entry.New();
            _txtS1.SetHexpand(true);
            _txtS1.OnChanged += OnS1Changed;
            pg.Attach(_txtS1, 1, 0, 1, 1);

            var lblS2 = Gtk.Label.New("Subs2:");
            lblS2.SetHalign(Gtk.Align.End);
            pg.Attach(lblS2, 0, 1, 1, 1);
            _txtS2 = Gtk.Entry.New();
            _txtS2.SetHexpand(true);
            _txtS2.OnChanged += OnS2Changed;
            pg.Attach(_txtS2, 1, 1, 1, 1);

            _lblTime = Gtk.Label.New("");
            _lblTime.SetHalign(Gtk.Align.Start);
            pg.Attach(_lblTime, 1, 2, 1, 1);

            // Snapshot row: checkbox on the left, picture in a clipping container
            _chkSnap = Gtk.CheckButton.NewWithLabel("Snapshot preview");
            _chkSnap.SetActive(true);
            _chkSnap.SetValign(Gtk.Align.Start);
            pg.Attach(_chkSnap, 0, 3, 1, 1);

            // Picture inside a ScrolledWindow that acts as a fixed-size viewport.
            // ScrolledWindow respects size request and clips content,
            // preventing Picture from inflating the grid.
            _imgSnap = Gtk.Picture.New();
            _imgSnap.SetContentFit(Gtk.ContentFit.Contain);
            _imgSnap.SetCanShrink(true);
            _imgSnap.SetHalign(Gtk.Align.Center);
            _imgSnap.SetValign(Gtk.Align.Center);

            var snapScroll = Gtk.ScrolledWindow.New();
            snapScroll.SetPolicy(Gtk.PolicyType.Never, Gtk.PolicyType.Never);
            snapScroll.SetSizeRequest(200, 120);
            snapScroll.SetHexpand(false);
            snapScroll.SetVexpand(false);
            snapScroll.SetHalign(Gtk.Align.Start);
            snapScroll.SetValign(Gtk.Align.Start);
            snapScroll.SetChild(_imgSnap);
            pg.Attach(snapScroll, 1, 3, 1, 1);

            detailBox.Append(pg);
            detailBox.Append(Gtk.Separator.New(Gtk.Orientation.Horizontal));

            // Action buttons row 1
            var ab1 = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
            AppendBtn(ab1, "Activate", OnActivate);
            AppendBtn(ab1, "Deactivate", OnDeactivate);
            detailBox.Append(ab1);

            // Find + audio
            var ab2 = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
            ab2.Append(Gtk.Label.New("Find:"));
            _txtFind = Gtk.Entry.New();
            _txtFind.SetWidthChars(25);
            _txtFind.OnActivate += (s, e) => FindNext();
            ab2.Append(_txtFind);
            AppendBtn(ab2, "Find Next", (s, e) => FindNext());
            ab2.Append(Gtk.Separator.New(Gtk.Orientation.Vertical));
            _btnAudio = Gtk.Button.NewWithLabel("Preview Audio");
            _btnAudio.OnClicked += OnPreviewAudio;
            ab2.Append(_btnAudio);
            detailBox.Append(ab2);

            // Stats
            var sf = Gtk.Frame.New("Statistics");
            var sg = Gtk.Grid.New();
            sg.SetRowSpacing(2);
            sg.SetColumnSpacing(8);
            sg.SetMarginTop(4);
            sg.SetMarginBottom(4);
            sg.SetMarginStart(4);
            sg.SetMarginEnd(4);
            sg.Attach(Gtk.Label.New("Episode —"), 0, 0, 1, 1);
            sg.Attach(Gtk.Label.New("Lines:"), 1, 0, 1, 1);
            _lblEpL = Gtk.Label.New("0"); sg.Attach(_lblEpL, 2, 0, 1, 1);
            sg.Attach(Gtk.Label.New("Active:"), 3, 0, 1, 1);
            _lblEpA = Gtk.Label.New("0"); sg.Attach(_lblEpA, 4, 0, 1, 1);
            sg.Attach(Gtk.Label.New("Inactive:"), 5, 0, 1, 1);
            _lblEpI = Gtk.Label.New("0"); sg.Attach(_lblEpI, 6, 0, 1, 1);
            sg.Attach(Gtk.Label.New("Total —"), 0, 1, 1, 1);
            sg.Attach(Gtk.Label.New("Lines:"), 1, 1, 1, 1);
            _lblTL = Gtk.Label.New("0"); sg.Attach(_lblTL, 2, 1, 1, 1);
            sg.Attach(Gtk.Label.New("Active:"), 3, 1, 1, 1);
            _lblTA = Gtk.Label.New("0"); sg.Attach(_lblTA, 4, 1, 1, 1);
            sg.Attach(Gtk.Label.New("Inactive:"), 5, 1, 1, 1);
            _lblTI = Gtk.Label.New("0"); sg.Attach(_lblTI, 6, 1, 1, 1);
            sf.SetChild(sg);
            detailBox.Append(sf);

            _progress = Gtk.ProgressBar.New();
            _progress.SetShowText(true);
            _progress.SetText("");
            detailBox.Append(_progress);

            // Bottom buttons
            var bot = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
            bot.SetHalign(Gtk.Align.End);
            var btnClose = Gtk.Button.NewWithLabel("Close");
            btnClose.SetSizeRequest(100, -1);
            btnClose.OnClicked += (s, e) => SetVisible(false);
            bot.Append(btnClose);
            _btnGo = Gtk.Button.NewWithLabel("Go!");
            _btnGo.SetSizeRequest(100, -1);
            _btnGo.OnClicked += OnGoClicked;
            bot.Append(_btnGo);
            detailBox.Append(bot);

            vbox.Append(detailBox);
            // Register CSS provider for row background and selection styling
            var cssProvider = Gtk.CssProvider.New();
            cssProvider.LoadFromString(
                // Normal state
                ".preview-row-active  { background-color: #FFFFF5; color: #1A1A1A; }" +
                ".preview-row-inactive { background-color: #FFB6C1; color: #1A1A1A; }" +
                ".preview-row-active  label { color: inherit; }" +
                ".preview-row-inactive label { color: inherit; }" +
                // Selected state — darker shade + visible outline
                "listview > row:selected .preview-row-active  " +
                    "{ background-color: #F0F0E5; outline: 2px solid #3584E4; outline-offset: -2px; }" +
                "listview > row:selected .preview-row-inactive " +
                    "{ background-color: #E8849A; outline: 2px solid #3584E4; outline-offset: -2px; }" +
                "listview > row:selected .preview-row-active  label { color: inherit; }" +
                "listview > row:selected .preview-row-inactive label { color: inherit; }");
            Gtk.StyleContext.AddProviderForDisplay(
                Gdk.Display.GetDefault()!,
                cssProvider,
                800); // Higher priority to override theme defaults
            SetChild(vbox);
        }

        /// <summary>
        /// Build a SignalListItemFactory that renders each PreviewItem
        /// as a horizontal box with multiple labels.
        /// </summary>
        private Gtk.SignalListItemFactory BuildListItemFactory()
        {
            var factory = Gtk.SignalListItemFactory.New();

            factory.OnSetup += (f, args) =>
            {
                var listItem = (Gtk.ListItem)args.Object;
                var box = Gtk.Box.New(Gtk.Orientation.Horizontal, 8);
                box.SetHexpand(true); // Ensure background fills entire row width

                var lblS1 = Gtk.Label.New("");
                lblS1.SetHexpand(true);
                lblS1.SetHalign(Gtk.Align.Start);
                lblS1.SetEllipsize(Pango.EllipsizeMode.End);
                lblS1.SetWidthChars(35);
                box.Append(lblS1);

                var lblS2 = Gtk.Label.New("");
                lblS2.SetHexpand(true);
                lblS2.SetHalign(Gtk.Align.Start);
                lblS2.SetEllipsize(Pango.EllipsizeMode.End);
                lblS2.SetWidthChars(25);
                box.Append(lblS2);

                var lblStart = Gtk.Label.New("");
                lblStart.SetWidthChars(14);
                box.Append(lblStart);

                var lblEnd = Gtk.Label.New("");
                lblEnd.SetWidthChars(14);
                box.Append(lblEnd);

                var lblDur = Gtk.Label.New("");
                lblDur.SetWidthChars(14);
                box.Append(lblDur);

                listItem.SetChild(box);
            };

            factory.OnBind += (f, args) =>
            {
                var listItem = (Gtk.ListItem)args.Object;
                uint pos = listItem.GetPosition();
                if (pos >= _items.Count) return;

                var item = _items[(int)pos];
                var box = (Gtk.Box)listItem.GetChild();
                if (box == null) return;

                // Apply background color based on active/inactive state
                box.RemoveCssClass(RowActiveCss);
                box.RemoveCssClass(RowInactiveCss);
                box.AddCssClass(item.IsActive ? RowActiveCss : RowInactiveCss);

                var lblS1 = (Gtk.Label)box.GetFirstChild();
                var lblS2 = (Gtk.Label)lblS1.GetNextSibling();
                var lblStart = (Gtk.Label)lblS2.GetNextSibling();
                var lblEnd = (Gtk.Label)lblStart.GetNextSibling();
                var lblDur = (Gtk.Label)lblEnd.GetNextSibling();

                lblS1.SetText(item.Subs1Text);
                lblS2.SetText(item.Subs2Text);
                lblStart.SetText(item.StartText);
                lblEnd.SetText(item.EndText);
                lblDur.SetText(item.DurText);
            };

            return factory;
        }

        /// <summary>
        /// Helper: create a labelled button, attach handler, append to box.
        /// Gir.Core Gtk.Button.OnClicked uses EventHandler&lt;EventArgs&gt;.
        /// </summary>
        private void AppendBtn(Gtk.Box box, string label,
            GObject.SignalHandler<Gtk.Button> handler)
        {
            var b = Gtk.Button.NewWithLabel(label);
            b.OnClicked += handler;
            box.Append(b);
        }

        // ── PREVIEW GENERATION ──────────────────────────────────────────────

        private void PopulateEpCombo()
        {
            _guard = true;
            uint prev = _comboEp.GetSelected();
            int n = UtilsSubs.getNumSubsFiles(Settings.Instance.Subs[0].FilePattern);
            int first = Settings.Instance.EpisodeStartNumber;
            var names = new string[n];
            for (int i = 0; i < n; i++)
                names[i] = (i + first).ToString();
            _epModel = Gtk.StringList.New(names);
            _comboEp.SetModel(_epModel);
            _comboEp.SetSelected(
                (prev < (uint)n) ? prev : 0);
            _guard = false;
        }

        private async void RunPreviewAsync()
        {
            if (_running) return;
            _running = true;
            SetSensitive(false);
            var reporter = new PProgress(_progress, () => _destroyed);

            WorkerVars result = null;
            Exception err = null;

            try
            {
                result = await Task.Run(() => DoPreviewWork(reporter));
            }
            catch (Exception ex)
            {
                err = ex;
            }

            reporter.Stop();

            if (_destroyed) return;

            _running = false;
            SetSensitive(true);
            if (err != null)
            {
                _progress.SetText("Error");
                _progress.SetFraction(0);
                if (!(err is OperationCanceledException))
                    UtilsMsg.showErrMsg(err.Message);
                return;
            }

            _wv = result;
            int ep = (int)_comboEp.GetSelected();
            if (ep < 0) ep = 0;
            _guard = true;
            PopulateList(ep);
            _guard = false;
            UpdateStats();
            _progress.SetText("Preview ready");
            _progress.SetFraction(1.0);

            if (_store.GetNItems() > 0)
            {
                _selection.SetSelected(0);
                // Explicitly trigger detail update for the first item
                // because SetSelected(0) may not fire notify when already 0
                OnSelChanged();
            }
        }
        private WorkerVars DoPreviewWork(IProgressReporter rpt)
        {
            string dir = SysPath.Combine(SysPath.GetTempPath(),
                ConstantSettings.TempPreviewDirName);
            if (Directory.Exists(dir))
                try { Directory.Delete(dir, true); } catch { }
            Directory.CreateDirectory(dir);

            var wv = new WorkerVars(null, dir, WorkerVars.SubsProcessingType.Preview);
            var sw = new WorkerSubs();

            var c = sw.combineAllSubs(wv, rpt);
            if (c == null) throw new OperationCanceledException();
            wv.CombinedAll = c;

            int total = 0;
            foreach (var a in wv.CombinedAll) total += a.Count;
            if (total == 0)
                throw new Exception("No lines could be parsed from subtitle files.");

            c = sw.inactivateLines(wv, rpt);
            if (c == null) throw new OperationCanceledException();
            wv.CombinedAll = c;

            return wv;
        }

        // ── LIST VIEW ───────────────────────────────────────────────────────

        private void PopulateList(int epIdx)
        {
            _store.RemoveAll();
            _items.Clear();
            if (_wv?.CombinedAll == null || epIdx < 0
                || epIdx >= _wv.CombinedAll.Count) return;

            var arr = _wv.CombinedAll[epIdx];
            for (int i = 0; i < arr.Count; i++)
            {
                var cb = arr[i];
                string dur = FmtTime(UtilsSubs.getDurationTime(
                    cb.Subs1.StartTime, cb.Subs1.EndTime));
                if (IsLong(cb)) dur += " (Long!)";

                _items.Add(PreviewItem.Create(
                    cb.Subs1.Text, cb.Subs2.Text,
                    FmtTime(cb.Subs1.StartTime), FmtTime(cb.Subs1.EndTime),
                    dur, cb.Active, i));
                _store.Append(Gtk.StringObject.New(""));
            }
        }

        private string FmtTime(TimeSpan t) =>
            $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds:000}";

        private bool IsLong(InfoCombined cb)
        {
            var d = UtilsSubs.getDurationTime(cb.Subs1.StartTime, cb.Subs1.EndTime);
            return ConstantSettings.LongClipWarningSeconds > 0 &&
                   d.TotalMilliseconds > ConstantSettings.LongClipWarningSeconds * 1000;
        }

        // ── SELECTION ───────────────────────────────────────────────────────

        private void OnSelChanged()
        {
            if (_guard) return;
            var comb = GetSelectedCombined();
            if (comb == null) return;

            _cur = comb;
            _guard = true;
            _txtS1.SetText(comb.Subs1.Text);
            _txtS2.SetText(comb.Subs2.Text);
            _txtS2.SetSensitive(Settings.Instance.Subs[1].Files.Length > 0);
            _lblTime.SetText(FmtTime(comb.Subs1.StartTime)
                + "  —  " + FmtTime(comb.Subs1.EndTime));
            _guard = false;

            UpdateSnapshot(comb);
        }

        private InfoCombined GetSelectedCombined()
        {
            uint sel = _selection.GetSelected();
            if (sel == Gtk.Constants.INVALID_LIST_POSITION) return null;
            if (sel >= _items.Count) return null;

            var item = _items[(int)sel];
            int ep = (int)_comboEp.GetSelected();
            if (_wv?.CombinedAll == null || ep < 0
                || ep >= _wv.CombinedAll.Count) return null;
            var arr = _wv.CombinedAll[ep];
            int idx = item.Index;
            return idx >= 0 && idx < arr.Count ? arr[idx] : null;
        }

        // ── SNAPSHOT PREVIEW ────────────────────────────────────────────────

        private void UpdateSnapshot(InfoCombined comb)
        {
            if (!_chkSnap.GetActive()
                || Settings.Instance.VideoClips.Files == null
                || Settings.Instance.VideoClips.Files.Length == 0) return;

            int ep = (int)_comboEp.GetSelected();
            if (ep < 0 || ep >= Settings.Instance.VideoClips.Files.Length) return;

            string video = Settings.Instance.VideoClips.Files[ep];
            TimeSpan mid = UtilsSubs.getMidpointTime(
                comb.Subs1.StartTime, comb.Subs1.EndTime);
            string outFile = SysPath.Combine(_wv.MediaDir,
                ConstantSettings.TempImageFilename);

            try { File.Delete(outFile); } catch { }

            Task.Run(() =>
            {
                UtilsSnapshot.takeSnapshotFromVideo(video, mid,
                    Settings.Instance.Snapshots.Size,
                    Settings.Instance.Snapshots.Crop,
                    Settings.Instance.Snapshots.Quality,
                    outFile);
            }).ContinueWith(_ =>
            {
                if (_destroyed) return;
                GLib.Functions.IdleAdd(0, () =>
                {
                    if (_destroyed) return false;
                    try
                    {
                        if (File.Exists(outFile))
                        {
                            // GTK4: load texture from file for Gtk.Picture
                            var gfile = Gio.FileHelper.NewForPath(outFile);
                            _imgSnap.SetFile(gfile);
                        }
                    }
                    catch { }
                    return false;
                });
            });
        }

        // ── TEXT EDITING ────────────────────────────────────────────────────

        private void OnS1Changed(Gtk.Editable sender, EventArgs e)
        {
            if (_guard || _cur == null || _wv == null) return;
            _cur.Subs1.Text = _txtS1.GetText();
            _changed = true;
        }

        private void OnS2Changed(Gtk.Editable sender, EventArgs e)
        {
            if (_guard || _cur == null || _wv == null) return;
            _cur.Subs2.Text = _txtS2.GetText();
            _changed = true;
        }

        // ── ACTIVATE / DEACTIVATE ───────────────────────────────────────────

        private void OnActivate(Gtk.Button s, EventArgs e) => SetActiveSel(true);
        private void OnDeactivate(Gtk.Button s, EventArgs e) => SetActiveSel(false);

        private void SetActiveSel(bool active)
        {
            if (_wv == null) return;
            int ep = (int)_comboEp.GetSelected();
            if (ep < 0) return;

            uint sel = _selection.GetSelected();
            if (sel == Gtk.Constants.INVALID_LIST_POSITION) return;
            if (sel >= _items.Count) return;

            var item = _items[(int)sel];
            int idx = item.Index;
            _wv.CombinedAll[ep][idx].Active = active;
            item.IsActive = active;

            // Force row rebind so CSS class updates visually
            _guard = true;
            _store.Remove(sel);
            _store.Insert(sel, Gtk.StringObject.New(""));
            _selection.SetSelected(sel);
            _guard = false;

            UpdateStats();
            _changed = true;
        }

        // ── FIND ────────────────────────────────────────────────────────────

        private void FindNext()
        {
            string text = _txtFind.GetText().Trim().ToLower();
            if (text.Length == 0 || _wv == null) return;

            int ep = (int)_comboEp.GetSelected();
            if (ep < 0) return;
            var arr = _wv.CombinedAll[ep];
            int count = arr.Count;

            for (int offset = 1; offset <= count; offset++)
            {
                int i = (_findIdx + offset) % count;
                var cb = arr[i];
                if (cb.Subs1.Text.ToLower().Contains(text) ||
                    cb.Subs2.Text.ToLower().Contains(text))
                {
                    _findIdx = i;
                    _selection.SetSelected((uint)i);
                    _listView.ScrollTo((uint)i,
                        Gtk.ListScrollFlags.Focus, null);
                    return;
                }
            }
        }

        // ── AUDIO PREVIEW ───────────────────────────────────────────────────

        private async void OnPreviewAudio(Gtk.Button s, EventArgs e)
        {
            if (_cur == null || _wv == null) return;
            int ep = (int)_comboEp.GetSelected();
            if (ep < 0) return;

            _btnAudio.SetSensitive(false);
            _btnAudio.SetLabel("Extracting...");

            var comb = _cur;
            string mp3 = SysPath.Combine(_wv.MediaDir,
                ConstantSettings.TempAudioFilename);
            string wav = SysPath.Combine(_wv.MediaDir,
                ConstantSettings.TempAudioPreviewFilename);

            await Task.Run(() =>
            {
                try { if (File.Exists(mp3)) File.Delete(mp3); } catch { }
                try { if (File.Exists(wav)) File.Delete(wav); } catch { }

                TimeSpan st = comb.Subs1.StartTime, en = comb.Subs1.EndTime;
                if (Settings.Instance.AudioClips.PadEnabled)
                {
                    st = UtilsSubs.applyTimePad(st,
                        -Settings.Instance.AudioClips.PadStart);
                    en = UtilsSubs.applyTimePad(en,
                        Settings.Instance.AudioClips.PadEnd);
                }

                if (Settings.Instance.AudioClips.UseAudioFromVideo &&
                    Settings.Instance.VideoClips.Files?.Length > ep)
                {
                    string streamNum =
                        Settings.Instance.VideoClips.AudioStream?.Num;
                    if (string.IsNullOrEmpty(streamNum)
                        || streamNum == "-" || !streamNum.Contains(":"))
                        streamNum = "0:a:0";

                    UtilsAudio.ripAudioFromVideo(
                        Settings.Instance.VideoClips.Files[ep],
                        streamNum, st, en,
                        Settings.Instance.AudioClips.Bitrate, mp3, null);
                }

                if (File.Exists(mp3) && new FileInfo(mp3).Length > 0)
                    UtilsAudio.convertAudioFormat(mp3, wav, 2);
            });

            if (_destroyed) return;

            _btnAudio.SetSensitive(true);
            _btnAudio.SetLabel("Preview Audio");

            if (File.Exists(wav))
            {
                try
                {
                    var p = new Process();
                    p.StartInfo.FileName = "ffplay";
                    p.StartInfo.Arguments =
                        $"-nodisp -autoexit -loglevel quiet \"{wav}\"";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                }
                catch { }
            }
        }

        // ── EPISODE CHANGE ──────────────────────────────────────────────────

        private void OnEpisodeChanged()
        {
            if (_guard || _wv == null) return;
            int ep = (int)_comboEp.GetSelected();
            if (ep < 0 || ep >= _wv.CombinedAll.Count) return;
            _guard = true;
            PopulateList(ep);
            _guard = false;
            UpdateStats();
            _findIdx = 0;
        }

        // ── STATS ───────────────────────────────────────────────────────────

        private void UpdateStats()
        {
            if (_wv?.CombinedAll == null) return;
            int ep = (int)_comboEp.GetSelected();
            if (ep < 0 || ep >= _wv.CombinedAll.Count) return;

            int epL = 0, epA = 0, epI = 0, tL = 0, tA = 0, tI = 0;
            foreach (var cb in _wv.CombinedAll[ep])
            {
                epL++;
                if (cb.Active) epA++; else epI++;
            }
            foreach (var arr in _wv.CombinedAll)
                foreach (var cb in arr)
                {
                    tL++;
                    if (cb.Active) tA++; else tI++;
                }

            _lblEpL.SetText(epL.ToString());
            _lblEpA.SetText(epA.ToString());
            _lblEpI.SetText(epI.ToString());
            _lblTL.SetText(tL.ToString());
            _lblTA.SetText(tA.ToString());
            _lblTI.SetText(tI.ToString());
        }

        // ── REGENERATE ──────────────────────────────────────────────────────

        private void OnRegenClicked(Gtk.Button s, EventArgs e)
        {
            if (_running) return;
            if (_changed)
            {
                if (!UtilsMsg.showConfirm("Regenerate and discard changes?"))
                    return;
            }
            _changed = false;
            RefreshSettings?.Invoke(this, EventArgs.Empty);

            _guard = true;
            _wv = null;
            _cur = null;
            _store.RemoveAll();
            _items.Clear();
            _guard = false;

            PopulateEpCombo();
            RunPreviewAsync();
        }

        // ── GO (delegate to MainWindow) ─────────────────────────────────────

        private void OnGoClicked(Gtk.Button s, EventArgs e)
        {
            if (_wv?.CombinedAll == null) return;
            RefreshSettings?.Invoke(this, EventArgs.Empty);
            GoRequested?.Invoke(this, EventArgs.Empty);
        }

        // ── PROGRESS REPORTER ───────────────────────────────────────────────

        private class PProgress : IProgressReporter
        {
            private readonly Gtk.ProgressBar _b;
            private readonly Func<bool> _isDestroyed;
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

            public PProgress(Gtk.ProgressBar b, Func<bool> isDestroyed)
            {
                _b = b;
                _isDestroyed = isDestroyed;
                // Poll for UI updates every 50ms
                GLib.Functions.TimeoutAdd(0, 50, OnPoll);
            }

            public void Stop()
            {
                _active = false;
                if (!_isDestroyed()) OnPoll();
            }

            private bool OnPoll()
            {
                if (_isDestroyed()) return false;
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
                    if (text != null) _b.SetText(text);
                    if (frac >= 0) _b.SetFraction(frac);
                }
                return _active;
            }

            public void NextStep(int step, string desc)
            {
                lock (_sync)
                {
                    _text = $"[{step}/{StepsTotal}] {desc}";
                    _fraction = 0.0;
                    _dirty = true;
                }
            }

            public void UpdateProgress(int pct, string text)
            {
                lock (_sync)
                {
                    _text = text;
                    _fraction = Math.Max(0, Math.Min(1, pct / 100.0));
                    _dirty = true;
                }
            }

            public void UpdateProgress(string text)
            {
                lock (_sync) { _text = text; _dirty = true; }
            }

            public void EnableDetail(bool en) { }
            public void SetDuration(TimeSpan d) { }
            public void OnFFmpegOutput(object sender, DataReceivedEventArgs e) { }
        }
    }

    // ── PreviewItem GObject wrapper ─────────────────────────────────────────
    // Gio.ListStore requires items that are GObject subclasses.
    // This lightweight wrapper holds the display data for one preview row.

    /// <summary>
    /// Plain data holder for a single preview list row.
    /// Cannot subclass GObject.Object in Gir.Core 0.7 (no public parameterless ctor).
    /// Data is stored in a parallel List; ListStore holds dummy StringObjects.
    /// </summary>
    public class PreviewItem
    {
        public string Subs1Text { get; set; } = "";
        public string Subs2Text { get; set; } = "";
        public string StartText { get; set; } = "";
        public string EndText { get; set; } = "";
        public string DurText { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public int Index { get; set; } = -1;

        public static PreviewItem Create(string s1, string s2,
            string start, string end, string dur, bool active, int idx)
        {
            return new PreviewItem
            {
                Subs1Text = s1, Subs2Text = s2,
                StartText = start, EndText = end, DurText = dur,
                IsActive = active, Index = idx
            };
        }
    }
}
