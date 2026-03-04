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
using System.Threading.Tasks;
using Gtk;
using System.IO;
using SysPath = System.IO.Path;

namespace subs2srs
{
    public class DialogPreview : Window
    {
        private const string ActiveBg = "#F5FFFA";
        private const string InactiveBg = "#FFB6C1";
        private const string WarnFg = "#FF0000";
        private const string NormalFg = "#000000";

        private enum C { Subs1, Subs2, Start, End, Dur, Bg, Fg, Idx }

        // Widgets
        private ComboBoxText _comboEp;
        private TreeView _tv;
        private ListStore _store;
        private Entry _txtS1, _txtS2, _txtFind;
        private Label _lblTime;
        private Gtk.Image _imgSnap;
        private CheckButton _chkSnap;
        private Button _btnAudio, _btnGo;
        private Label _lblEpL, _lblEpA, _lblEpI, _lblTL, _lblTA, _lblTI;
        private ProgressBar _progress;

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

        public DialogPreview() : base(WindowType.Toplevel)
        {
            Title = "Preview";
            SetDefaultSize(1000, 750);
            WindowPosition = WindowPosition.Center;
            DeleteEvent += (o, a) => { a.RetVal = true; Hide(); };
            BuildUI();
        }

        public void StartPreview()
        {
            _destroyed = false;
            ShowAll();
            PopulateEpCombo();
            RunPreviewAsync();
        }

        /// <summary>
        /// Clean up temp files and dispose snapshot pixbuf.
        /// Called internally and by MainWindow on application exit.
        /// </summary>
        public void CleanupAndDestroy()
        {
            _destroyed = true;
            Cleanup();
            Destroy();
        }

        private void Cleanup()
        {
            // Dispose snapshot pixbuf deterministically
            try
            {
                var pb = _imgSnap?.Pixbuf;
                if (pb != null)
                {
                    _imgSnap.Pixbuf = null;
                    pb.Dispose();
                }
            }
            catch { }

            if (_wv?.MediaDir != null && Directory.Exists(_wv.MediaDir))
                try { Directory.Delete(_wv.MediaDir, true); } catch { }
        }

        // ── UI ──────────────────────────────────────────────────────────────

        private void BuildUI()
        {
            var vbox = new Box(Orientation.Vertical, 4) { BorderWidth = 6 };

            // Top bar
            var top = new Box(Orientation.Horizontal, 6);
            top.PackStart(new Label("Episode:"), false, false, 0);
            _comboEp = new ComboBoxText();
            _comboEp.Changed += (s, e) => OnEpisodeChanged();
            top.PackStart(_comboEp, false, false, 0);
            var btnRegen = new Button("Regenerate");
            btnRegen.Clicked += OnRegenClicked;
            top.PackStart(btnRegen, false, false, 0);
            vbox.PackStart(top, false, false, 0);

            // TreeView
            _store = new ListStore(typeof(string), typeof(string), typeof(string),
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(int));
            _tv = new TreeView(_store) { EnableSearch = false };
            _tv.Selection.Mode = SelectionMode.Multiple;
            _tv.Selection.Changed += OnSelChanged;
            AddCol("Subs1", (int)C.Subs1, 300);
            AddCol("Subs2", (int)C.Subs2, 200);
            AddCol("Start", (int)C.Start, 110);
            AddCol("End", (int)C.End, 110);
            AddCol("Duration", (int)C.Dur, 110);
            var sw = new ScrolledWindow { ShadowType = ShadowType.In };
            sw.Add(_tv);
            vbox.PackStart(sw, true, true, 0);

            // Text preview
            var pg = new Grid { RowSpacing = 4, ColumnSpacing = 6 };
            pg.Attach(new Label("Subs1:") { Halign = Align.End }, 0, 0, 1, 1);
            _txtS1 = new Entry { Hexpand = true };
            _txtS1.Changed += OnS1Changed;
            pg.Attach(_txtS1, 1, 0, 1, 1);
            pg.Attach(new Label("Subs2:") { Halign = Align.End }, 0, 1, 1, 1);
            _txtS2 = new Entry { Hexpand = true };
            _txtS2.Changed += OnS2Changed;
            pg.Attach(_txtS2, 1, 1, 1, 1);
            _lblTime = new Label("") { Halign = Align.Start };
            pg.Attach(_lblTime, 1, 2, 1, 1);

            _chkSnap = new CheckButton("Snapshot preview") { Active = true };
            pg.Attach(_chkSnap, 0, 3, 1, 1);
            _imgSnap = new Gtk.Image();
            pg.Attach(_imgSnap, 1, 3, 1, 1);
            vbox.PackStart(pg, false, false, 0);

            vbox.PackStart(new Separator(Orientation.Horizontal), false, false, 2);

            // Action buttons row 1
            var ab1 = new Box(Orientation.Horizontal, 4);
            BtnPack(ab1, "Select All", (s, e) => SelectAll());
            BtnPack(ab1, "Select None", (s, e) => SelectNone());
            BtnPack(ab1, "Invert", (s, e) => InvertSel());
            ab1.PackStart(new Separator(Orientation.Vertical), false, false, 4);
            BtnPack(ab1, "Activate", OnActivate);
            BtnPack(ab1, "Deactivate", OnDeactivate);
            vbox.PackStart(ab1, false, false, 0);

            // Find + audio
            var ab2 = new Box(Orientation.Horizontal, 4);
            ab2.PackStart(new Label("Find:"), false, false, 0);
            _txtFind = new Entry { WidthChars = 25 };
            _txtFind.Activated += (s, e) => FindNext();
            ab2.PackStart(_txtFind, false, false, 0);
            BtnPack(ab2, "Find Next", (s, e) => FindNext());
            ab2.PackStart(new Separator(Orientation.Vertical), false, false, 8);
            _btnAudio = new Button("Preview Audio");
            _btnAudio.Clicked += OnPreviewAudio;
            ab2.PackStart(_btnAudio, false, false, 0);
            vbox.PackStart(ab2, false, false, 0);

            // Stats
            var sf = new Frame("Statistics");
            var sg = new Grid { RowSpacing = 2, ColumnSpacing = 8, BorderWidth = 4 };
            sg.Attach(new Label("Episode —"), 0, 0, 1, 1);
            sg.Attach(new Label("Lines:"), 1, 0, 1, 1);
            _lblEpL = new Label("0"); sg.Attach(_lblEpL, 2, 0, 1, 1);
            sg.Attach(new Label("Active:"), 3, 0, 1, 1);
            _lblEpA = new Label("0"); sg.Attach(_lblEpA, 4, 0, 1, 1);
            sg.Attach(new Label("Inactive:"), 5, 0, 1, 1);
            _lblEpI = new Label("0"); sg.Attach(_lblEpI, 6, 0, 1, 1);
            sg.Attach(new Label("Total —"), 0, 1, 1, 1);
            sg.Attach(new Label("Lines:"), 1, 1, 1, 1);
            _lblTL = new Label("0"); sg.Attach(_lblTL, 2, 1, 1, 1);
            sg.Attach(new Label("Active:"), 3, 1, 1, 1);
            _lblTA = new Label("0"); sg.Attach(_lblTA, 4, 1, 1, 1);
            sg.Attach(new Label("Inactive:"), 5, 1, 1, 1);
            _lblTI = new Label("0"); sg.Attach(_lblTI, 6, 1, 1, 1);
            sf.Add(sg);
            vbox.PackStart(sf, false, false, 0);

            _progress = new ProgressBar { ShowText = true, Text = "" };
            vbox.PackStart(_progress, false, false, 0);

            // Bottom
            var bot = new Box(Orientation.Horizontal, 6);
            _btnGo = new Button("Go!") { WidthRequest = 100 };
            _btnGo.Clicked += OnGoClicked;
            var btnClose = new Button("Close") { WidthRequest = 100 };
            btnClose.Clicked += (s, e) => Hide();
            bot.PackEnd(_btnGo, false, false, 0);
            bot.PackEnd(btnClose, false, false, 0);
            vbox.PackStart(bot, false, false, 0);

            Add(vbox);
        }

        private void AddCol(string title, int modelCol, int width)
        {
            var r = new CellRendererText();
            var c = new TreeViewColumn(title, r, "text", modelCol,
                "background", (int)C.Bg, "foreground", (int)C.Fg);
            c.Resizable = true;
            c.FixedWidth = width;
            c.Sizing = TreeViewColumnSizing.Fixed;
            _tv.AppendColumn(c);
        }

        private void BtnPack(Box box, string label, EventHandler handler)
        {
            var b = new Button(label);
            b.Clicked += handler;
            box.PackStart(b, false, false, 0);
        }

        // ── PREVIEW GENERATION ──────────────────────────────────────────────

        private void PopulateEpCombo()
        {
            _guard = true;
            int prev = _comboEp.Active;
            _comboEp.RemoveAll();
            int n = UtilsSubs.getNumSubsFiles(Settings.Instance.Subs[0].FilePattern);
            int first = Settings.Instance.EpisodeStartNumber;
            for (int i = 0; i < n; i++)
                _comboEp.AppendText((i + first).ToString());
            _comboEp.Active = (prev >= 0 && prev < n) ? prev : 0;
            _guard = false;
        }

        private async void RunPreviewAsync()
        {
            if (_running) return;
            _running = true;
            Sensitive = false;
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
            Sensitive = true;
            if (err != null)
            {
                _progress.Text = "Error";
                _progress.Fraction = 0;
                if (!(err is OperationCanceledException))
                    UtilsMsg.showErrMsg(err.Message);
                return;
            }

            _wv = result;
            int ep = _comboEp.Active >= 0 ? _comboEp.Active : 0;
            _guard = true;
            PopulateTree(ep);
            _guard = false;
            UpdateStats();
            _progress.Text = "Preview ready";
            _progress.Fraction = 1.0;

            if (_store.GetIterFirst(out var iter))
            {
                _tv.Selection.SelectIter(iter);
                _tv.GrabFocus();
            }
        }

        private WorkerVars DoPreviewWork(IProgressReporter rpt)
        {
            string dir = SysPath.Combine(SysPath.GetTempPath(), ConstantSettings.TempPreviewDirName);
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
            if (total == 0) throw new Exception("No lines could be parsed from subtitle files.");

            c = sw.inactivateLines(wv, rpt);
            if (c == null) throw new OperationCanceledException();
            wv.CombinedAll = c;

            return wv;
        }

        // ── TREE VIEW ───────────────────────────────────────────────────────

        private void PopulateTree(int epIdx)
        {
            _store.Clear();
            if (_wv?.CombinedAll == null || epIdx < 0 || epIdx >= _wv.CombinedAll.Count) return;

            var arr = _wv.CombinedAll[epIdx];
            for (int i = 0; i < arr.Count; i++)
            {
                var cb = arr[i];
                string dur = FmtTime(UtilsSubs.getDurationTime(cb.Subs1.StartTime, cb.Subs1.EndTime));
                bool tooLong = IsLong(cb);
                if (tooLong) dur += " (Long!)";

                _store.AppendValues(
                    cb.Subs1.Text,
                    cb.Subs2.Text,
                    FmtTime(cb.Subs1.StartTime),
                    FmtTime(cb.Subs1.EndTime),
                    dur,
                    cb.Active ? ActiveBg : InactiveBg,
                    tooLong ? WarnFg : NormalFg,
                    i);
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

        private void OnSelChanged(object s, EventArgs e)
        {
            if (_guard) return;
            var comb = GetFirstSelected();
            if (comb == null) return;

            _cur = comb;
            _guard = true;
            _txtS1.Text = comb.Subs1.Text;
            _txtS2.Text = comb.Subs2.Text;
            _txtS2.Sensitive = Settings.Instance.Subs[1].Files.Length > 0;
            _lblTime.Text = FmtTime(comb.Subs1.StartTime) + "  —  " + FmtTime(comb.Subs1.EndTime);
            _guard = false;

            UpdateSnapshot(comb);
        }

        private InfoCombined GetFirstSelected()
        {
            var paths = _tv.Selection.GetSelectedRows();
            if (paths.Length == 0) return null;
            if (!_store.GetIter(out var iter, paths[0])) return null;
            int idx = (int)_store.GetValue(iter, (int)C.Idx);
            int ep = _comboEp.Active;
            if (_wv?.CombinedAll == null || ep < 0 || ep >= _wv.CombinedAll.Count) return null;
            var arr = _wv.CombinedAll[ep];
            return idx >= 0 && idx < arr.Count ? arr[idx] : null;
        }

        // ── SNAPSHOT PREVIEW ────────────────────────────────────────────────

        private void UpdateSnapshot(InfoCombined comb)
        {
            if (!_chkSnap.Active || Settings.Instance.VideoClips.Files == null ||
                Settings.Instance.VideoClips.Files.Length == 0) return;

            int ep = _comboEp.Active;
            if (ep < 0 || ep >= Settings.Instance.VideoClips.Files.Length) return;

            string video = Settings.Instance.VideoClips.Files[ep];
            TimeSpan mid = UtilsSubs.getMidpointTime(comb.Subs1.StartTime, comb.Subs1.EndTime);
            string outFile = SysPath.Combine(_wv.MediaDir, ConstantSettings.TempImageFilename);

            try { File.Delete(outFile); } catch { }

            Task.Run(() =>
            {
                UtilsSnapshot.takeSnapshotFromVideo(video, mid,
                    Settings.Instance.Snapshots.Size, Settings.Instance.Snapshots.Crop, outFile);
            }).ContinueWith(_ =>
            {
                if (_destroyed) return;
                Application.Invoke((s2, e2) =>
                {
                    if (_destroyed) return;
                    try
                    {
                        if (File.Exists(outFile))
                        {
                            var pb = new Gdk.Pixbuf(outFile);
                            var old = _imgSnap.Pixbuf;
                            _imgSnap.Pixbuf = pb;
                            old?.Dispose();
                        }
                    }
                    catch { }
                });
            });
        }

        // ── TEXT EDITING ────────────────────────────────────────────────────

        private void OnS1Changed(object s, EventArgs e)
        {
            if (_guard || _cur == null || _wv == null) return;
            _cur.Subs1.Text = _txtS1.Text;
            UpdateCurrentRow();
            _changed = true;
        }

        private void OnS2Changed(object s, EventArgs e)
        {
            if (_guard || _cur == null || _wv == null) return;
            _cur.Subs2.Text = _txtS2.Text;
            UpdateCurrentRow();
            _changed = true;
        }

        private void UpdateCurrentRow()
        {
            var paths = _tv.Selection.GetSelectedRows();
            if (paths.Length == 0) return;
            if (!_store.GetIter(out var iter, paths[0])) return;
            _store.SetValue(iter, (int)C.Subs1, _cur.Subs1.Text);
            _store.SetValue(iter, (int)C.Subs2, _cur.Subs2.Text);
        }

        // ── ACTIVATE / DEACTIVATE ───────────────────────────────────────────

        private void OnActivate(object s, EventArgs e) => SetActiveSelected(true);
        private void OnDeactivate(object s, EventArgs e) => SetActiveSelected(false);

        private void SetActiveSelected(bool active)
        {
            if (_wv == null) return;
            int ep = _comboEp.Active;
            if (ep < 0) return;

            foreach (var path in _tv.Selection.GetSelectedRows())
            {
                if (!_store.GetIter(out var iter, path)) continue;
                int idx = (int)_store.GetValue(iter, (int)C.Idx);
                _wv.CombinedAll[ep][idx].Active = active;
                _store.SetValue(iter, (int)C.Bg, active ? ActiveBg : InactiveBg);
            }
            UpdateStats();
            _changed = true;
        }

        // ── SELECT ALL / NONE / INVERT ──────────────────────────────────────

        private void SelectAll()
        {
            _guard = true;
            _tv.Selection.SelectAll();
            _guard = false;
            var c = GetFirstSelected();
            if (c != null) OnSelChanged(null, null);
        }

        private void SelectNone()
        {
            _guard = true;
            _tv.Selection.UnselectAll();
            _guard = false;
        }

        private void InvertSel()
        {
            _guard = true;
            if (!_store.GetIterFirst(out var iter)) { _guard = false; return; }
            do
            {
                var treePath = _store.GetPath(iter);
                if (_tv.Selection.PathIsSelected(treePath))
                    _tv.Selection.UnselectPath(treePath);
                else
                    _tv.Selection.SelectPath(treePath);
            } while (_store.IterNext(ref iter));
            _guard = false;
            var c = GetFirstSelected();
            if (c != null) OnSelChanged(null, null);
        }

        // ── FIND ────────────────────────────────────────────────────────────

        private void FindNext()
        {
            string text = _txtFind.Text.Trim().ToLower();
            if (text.Length == 0 || _wv == null) return;

            int ep = _comboEp.Active;
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
                    var treePath = new Gtk.TreePath(new[] { i });
                    _tv.Selection.UnselectAll();
                    _tv.Selection.SelectPath(treePath);
                    _tv.ScrollToCell(treePath, null, true, 0.5f, 0);
                    return;
                }
            }
        }

        // ── AUDIO PREVIEW ───────────────────────────────────────────────────

        private async void OnPreviewAudio(object s, EventArgs e)
        {
            if (_cur == null || _wv == null) return;
            int ep = _comboEp.Active;
            if (ep < 0) return;

            _btnAudio.Sensitive = false;
            _btnAudio.Label = "Extracting...";

            var comb = _cur;
            string mp3 = SysPath.Combine(_wv.MediaDir, ConstantSettings.TempAudioFilename);
            string wav = SysPath.Combine(_wv.MediaDir, ConstantSettings.TempAudioPreviewFilename);

            await Task.Run(() =>
            {
                try { if (File.Exists(mp3)) File.Delete(mp3); } catch { }
                try { if (File.Exists(wav)) File.Delete(wav); } catch { }

                TimeSpan st = comb.Subs1.StartTime, en = comb.Subs1.EndTime;
                if (Settings.Instance.AudioClips.PadEnabled)
                {
                    st = UtilsSubs.applyTimePad(st, -Settings.Instance.AudioClips.PadStart);
                    en = UtilsSubs.applyTimePad(en, Settings.Instance.AudioClips.PadEnd);
                }

                if (Settings.Instance.AudioClips.UseAudioFromVideo &&
                    Settings.Instance.VideoClips.Files?.Length > ep)
                {
                    string streamNum = Settings.Instance.VideoClips.AudioStream?.Num;
                    if (string.IsNullOrEmpty(streamNum) || streamNum == "-" || !streamNum.Contains(":"))
                        streamNum = "0:a:0";

                    UtilsAudio.ripAudioFromVideo(
                        Settings.Instance.VideoClips.Files[ep],
                        streamNum,
                        st, en, Settings.Instance.AudioClips.Bitrate, mp3, null);
                }

                if (File.Exists(mp3) && new FileInfo(mp3).Length > 0)
                    UtilsAudio.convertAudioFormat(mp3, wav, 2);
            });

            if (_destroyed) return;

            _btnAudio.Sensitive = true;
            _btnAudio.Label = "Preview Audio";

            if (File.Exists(wav))
            {
                try
                {
                    var p = new Process();
                    p.StartInfo.FileName = "ffplay";
                    p.StartInfo.Arguments = $"-nodisp -autoexit -loglevel quiet \"{wav}\"";
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
            int ep = _comboEp.Active;
            if (ep < 0 || ep >= _wv.CombinedAll.Count) return;
            _guard = true;
            PopulateTree(ep);
            _guard = false;
            UpdateStats();
            _findIdx = 0;
        }

        // ── STATS ───────────────────────────────────────────────────────────

        private void UpdateStats()
        {
            if (_wv?.CombinedAll == null) return;
            int ep = _comboEp.Active;
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

            _lblEpL.Text = epL.ToString();
            _lblEpA.Text = epA.ToString();
            _lblEpI.Text = epI.ToString();
            _lblTL.Text = tL.ToString();
            _lblTA.Text = tA.ToString();
            _lblTI.Text = tI.ToString();
        }

        // ── REGENERATE ──────────────────────────────────────────────────────

        private void OnRegenClicked(object s, EventArgs e)
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
            _store.Clear();
            _guard = false;

            PopulateEpCombo();
            RunPreviewAsync();
        }

        // ── GO (delegate to MainWindow) ─────────────────────────────────────

        private void OnGoClicked(object s, EventArgs e)
        {
            if (_wv?.CombinedAll == null) return;

            // Save any text edits / active state back to settings
            RefreshSettings?.Invoke(this, EventArgs.Empty);

            // Raise event — MainWindow handles actual processing
            GoRequested?.Invoke(this, EventArgs.Empty);
        }

        // ── PROGRESS REPORTER ───────────────────────────────────────────────

        private class PProgress : IProgressReporter
        {
            private readonly ProgressBar _b;
            private readonly Func<bool> _isDestroyed;
            public bool Cancel { get; set; }
            public int StepsTotal { get; set; }

            private string _text;
            private double _fraction = -1;
            private bool _dirty;
            private readonly object _sync = new object();
            private bool _active = true;

            public PProgress(ProgressBar b, Func<bool> isDestroyed)
            {
                _b = b;
                _isDestroyed = isDestroyed;
                GLib.Timeout.Add(50, OnPoll);
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
                    if (text != null) _b.Text = text;
                    if (frac >= 0) _b.Fraction = frac;
                }
                return _active;
            }

            public void NextStep(int step, string desc)
            {
                lock (_sync) { _text = $"[{step}/{StepsTotal}] {desc}"; _fraction = 0.0; _dirty = true; }
            }

            public void UpdateProgress(int pct, string text)
            {
                lock (_sync) { _text = text; _fraction = Math.Max(0, Math.Min(1, pct / 100.0)); _dirty = true; }
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
}
