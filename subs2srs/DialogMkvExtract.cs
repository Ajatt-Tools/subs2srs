//  Copyright (C) 2026 fkzys and contributors
//  SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.IO;
using SysPath = System.IO.Path;
using System.Threading;
using System.Threading.Tasks;

namespace subs2srs
{
    /// <summary>
    /// MKV Extract tool dialog — GTK4/Gir.Core port.
    /// Replaces GTK3 Dialog with a modal Gtk.Window + nested GLib.MainLoop.
    /// </summary>
    public class DialogMkvExtract : Gtk.Window
    {
        private List<string> _selectedFiles = new();
        private Gtk.Entry _txtFiles;
        private Gtk.Label _lblFileCount;
        private Gtk.DropDown _comboTrackType;
        private Gtk.StringList _trackTypeModel;
        private Gtk.Entry _txtOutDir;
        private Gtk.Button _btnExtract;
        private Gtk.Label _lblProgress;
        private Gtk.ProgressBar _progressEpisode;
        private Gtk.ProgressBar _progressTrack;
        private CancellationTokenSource _cts;
        private bool _closed;

        // Nested main loop for synchronous Run()
        private GLib.MainLoop _loop;

        public DialogMkvExtract(Gtk.Window parent) : base()
        {
            SetTitle("MKV Extract Tool");
            SetDefaultSize(580, 350);
            SetModal(true);
            if (parent != null)
                SetTransientFor(parent);

            OnCloseRequest += OnDialogCloseRequest;
            BuildUI();
        }

        /// <summary>
        /// Show the dialog modally using a nested GLib main loop.
        /// </summary>
        public int Run()
        {
            _loop = GLib.MainLoop.New(null, false);

            Show();
            _loop.Run();

            return 0;
        }

        private bool OnDialogCloseRequest(Gtk.Window sender, EventArgs args)
        {
            _closed = true;
            _cts?.Cancel();
            if (_loop != null && _loop.IsRunning())
                _loop.Quit();
            return false; // allow default close
        }

        private void BuildUI()
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 8);
            vbox.SetMarginTop(10);
            vbox.SetMarginBottom(10);
            vbox.SetMarginStart(10);
            vbox.SetMarginEnd(10);

            // Help text
            var helpLabel = Gtk.Label.New(
                "Use this tool to extract all subtitle and/or audio tracks from MKV files.");
            helpLabel.SetHalign(Gtk.Align.Center);
            vbox.Append(helpLabel);
            vbox.Append(Gtk.Separator.New(Gtk.Orientation.Horizontal));

            // MKV files
            var lblSelect = Gtk.Label.New("Select one or more .mkv files:");
            lblSelect.SetHalign(Gtk.Align.Start);
            vbox.Append(lblSelect);

            var fileRow = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
            var btnFiles = Gtk.Button.NewWithLabel("Files...");
            btnFiles.OnClicked += OnSelectFiles;
            fileRow.Append(btnFiles);
            _txtFiles = Gtk.Entry.New();
            _txtFiles.SetHexpand(true);
            _txtFiles.SetEditable(false);
            fileRow.Append(_txtFiles);
            vbox.Append(fileRow);

            _lblFileCount = Gtk.Label.New("");
            _lblFileCount.SetHalign(Gtk.Align.End);
            vbox.Append(_lblFileCount);

            // Track type
            var lblTracks = Gtk.Label.New("Tracks to extract:");
            lblTracks.SetHalign(Gtk.Align.Start);
            vbox.Append(lblTracks);
            _trackTypeModel = Gtk.StringList.New(new[]
            {
                "All subtitle tracks",
                "All audio tracks",
                "All subtitle and audio tracks"
            });
            _comboTrackType = Gtk.DropDown.New(_trackTypeModel, null);
            _comboTrackType.SetSelected(0);
            vbox.Append(_comboTrackType);

            // Output directory
            var lblOutDir = Gtk.Label.New(
                "Directory where the extracted tracks will be placed:");
            lblOutDir.SetHalign(Gtk.Align.Start);
            vbox.Append(lblOutDir);

            var outRow = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
            var btnOut = Gtk.Button.NewWithLabel("Output...");
            btnOut.OnClicked += OnSelectOutDir;
            outRow.Append(btnOut);
            _txtOutDir = Gtk.Entry.New();
            _txtOutDir.SetHexpand(true);
            _txtOutDir.SetText(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            outRow.Append(_txtOutDir);
            vbox.Append(outRow);

            // Progress (hidden until extraction starts)
            _lblProgress = Gtk.Label.New("");
            _lblProgress.SetHalign(Gtk.Align.Start);
            _lblProgress.SetVisible(false);
            vbox.Append(_lblProgress);

            _progressEpisode = Gtk.ProgressBar.New();
            _progressEpisode.SetShowText(true);
            _progressEpisode.SetVisible(false);
            vbox.Append(_progressEpisode);

            _progressTrack = Gtk.ProgressBar.New();
            _progressTrack.SetShowText(true);
            _progressTrack.SetVisible(false);
            vbox.Append(_progressTrack);

            // Extract/Stop button
            _btnExtract = Gtk.Button.NewWithLabel("Extract");
            _btnExtract.SetHalign(Gtk.Align.Center);
            _btnExtract.SetSizeRequest(120, -1);
            _btnExtract.OnClicked += OnExtractClicked;
            vbox.Append(_btnExtract);

            SetChild(vbox);
        }

        // ── FILE SELECTION ──────────────────────────────────────────────────

        private async void OnSelectFiles(Gtk.Button sender, EventArgs e)
        {
            var dlg = Gtk.FileDialog.New();
            dlg.SetTitle("Select One or More MKV Files");

            var filter = Gtk.FileFilter.New();
            filter.SetName("Matroska files (*.mkv)");
            filter.AddPattern("*.mkv");
            var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
            filters.Append(filter);
            dlg.SetFilters(filters);

            try
            {
                var fileList = await dlg.OpenMultipleAsync(this);
                if (fileList == null) return;

                _selectedFiles.Clear();
                for (uint i = 0; i < fileList.GetNItems(); i++)
                {
                    var obj = fileList.GetObject(i);
                    if (obj == null) continue;
                    // Wrap the same native GFile handle as FileHelper
                    var gfile = new Gio.FileHelper(obj.Handle);
                    string path = gfile.GetPath() ?? "";
                    if (SysPath.GetExtension(path).ToLowerInvariant() == ".mkv")
                        _selectedFiles.Add(path);
                }
                UpdateFileDisplay();
            }
            catch { /* user cancelled */ }
        }

        private async void OnSelectOutDir(Gtk.Button sender, EventArgs e)
        {
            var dlg = Gtk.FileDialog.New();
            dlg.SetTitle("Select Output Directory");

            try
            {
                var file = await dlg.SelectFolderAsync(this);
                if (file != null)
                {
                    string path = file.GetPath() ?? "";
                    if (path != "") _txtOutDir.SetText(path);
                }
            }
            catch { /* user cancelled */ }
        }

        private void UpdateFileDisplay()
        {
            var names = new List<string>(_selectedFiles.Count);
            foreach (string f in _selectedFiles)
                names.Add($"\"{SysPath.GetFileName(f)}\"");
            _txtFiles.SetText(string.Join(", ", names));

            _lblFileCount.SetText(_selectedFiles.Count == 1
                ? "1 file selected"
                : $"{_selectedFiles.Count} files selected");
        }

        // ── EXTRACTION ──────────────────────────────────────────────────────

        private async void OnExtractClicked(Gtk.Button sender, EventArgs e)
        {
            // Toggle stop
            if (_cts != null)
            {
                _cts.Cancel();
                return;
            }

            // Validate
            if (_selectedFiles.Count == 0)
            {
                UtilsMsg.showErrMsg("Please select one or more MKV files.");
                return;
            }

            string outDir = _txtOutDir.GetText().Trim();
            if (!Directory.Exists(outDir))
            {
                UtilsMsg.showErrMsg("Please enter a valid output directory.");
                return;
            }

            // Get track type from dropdown
            uint trackIdx = _comboTrackType.GetSelected();
            string trackType = _trackTypeModel.GetString(trackIdx) ?? "All subtitle tracks";
            var files = new List<string>(_selectedFiles);

            _btnExtract.SetLabel("Stop");
            _lblProgress.SetVisible(true);
            _progressEpisode.SetVisible(true);
            _progressTrack.SetVisible(true);
            _progressEpisode.SetFraction(0);
            _progressTrack.SetFraction(0);

            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            Exception error = null;
            int extracted = 0;

            try
            {
                extracted = await Task.Run(() =>
                    ExtractTracks(files, trackType, outDir, token));
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { error = ex; }

            _cts.Dispose();
            _cts = null;

            if (_closed) return;

            _btnExtract.SetLabel("Extract");
            _lblProgress.SetVisible(false);
            _progressEpisode.SetVisible(false);
            _progressTrack.SetVisible(false);

            if (error != null)
                UtilsMsg.showErrMsg($"Something went wrong: {error.Message}");
            else if (token.IsCancellationRequested)
            { /* cancelled */ }
            else if (extracted == 0)
                UtilsMsg.showErrMsg(
                    "No tracks were found in the selected files.\n\n"
                    + "Make sure mkvtoolnix is installed (mkvinfo, mkvextract).");
            else
                UtilsMsg.showInfoMsg(
                    $"Extraction complete. {extracted} track(s) extracted.");
        }

        private int ExtractTracks(List<string> files, string trackType,
            string outDir, CancellationToken token)
        {
            int totalExtracted = 0;

            for (int i = 0; i < files.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                List<MkvTrack> tracks;
                if (trackType == "All subtitle tracks")
                    tracks = UtilsMkv.getSubtitleTrackList(files[i]);
                else if (trackType == "All audio tracks")
                    tracks = UtilsMkv.getAudioTrackList(files[i]);
                else
                    tracks = UtilsMkv.getTrackList(files[i]);

                for (int t = 0; t < tracks.Count; t++)
                {
                    token.ThrowIfCancellationRequested();

                    var track = tracks[t];
                    string displayLang = UtilsLang.LangThreeLetter2Full(track.Lang);
                    if (string.IsNullOrEmpty(displayLang))
                        displayLang = "Unknown";

                    string fileName = SysPath.Combine(outDir,
                        $"{SysPath.GetFileNameWithoutExtension(files[i])}"
                        + $" - Track {Convert.ToInt32(track.TrackID):00}"
                        + $" - {displayLang}.{track.Extension}");

                    int curEp = i + 1, maxEp = files.Count;
                    int curTrack = t + 1, maxTrack = tracks.Count;

                    // Update UI from background thread via idle handler
                    GLib.Functions.IdleAdd(0, () =>
                    {
                        if (_closed) return false;
                        _lblProgress.SetText(
                            $"Extracting track {curTrack}/{maxTrack}"
                            + $" from file {curEp}/{maxEp}...");
                        _progressEpisode.SetFraction((double)curEp / maxEp);
                        _progressEpisode.SetText($"File {curEp}/{maxEp}");
                        _progressTrack.SetFraction((double)curTrack / maxTrack);
                        _progressTrack.SetText($"Track {curTrack}/{maxTrack}");
                        return false;
                    });

                    UtilsMkv.extractTrack(files[i], track.TrackID, fileName);
                    totalExtracted++;
                }
            }

            return totalExtracted;
        }
    }
}
