//  Copyright (C) 2026 fkzys
//  SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.IO;
using SysPath = System.IO.Path;
using System.Threading;
using System.Threading.Tasks;
using Gtk;

namespace subs2srs
{
    public class DialogMkvExtract : Dialog
    {
        private List<string> _selectedFiles = new();
        private Entry _txtFiles;
        private Label _lblFileCount;
        private ComboBoxText _comboTrackType;
        private Entry _txtOutDir;
        private Button _btnExtract;
        private Label _lblProgress;
        private ProgressBar _progressEpisode;
        private ProgressBar _progressTrack;
        private CancellationTokenSource _cts;
        private bool _destroyed;

        public DialogMkvExtract(Window parent) : base(
            "MKV Extract Tool", parent, DialogFlags.Modal,
            "Close", ResponseType.Close)
        {
            SetDefaultSize(580, 350);
            Destroyed += (s, e) => { _destroyed = true; _cts?.Cancel(); };
            BuildUI();
        }

        private void BuildUI()
        {
            var vbox = new Box(Orientation.Vertical, 8) { BorderWidth = 10 };

            // Help text
            vbox.PackStart(new Label(
                "Use this tool to extract all subtitle and/or audio tracks from MKV files.")
                { Halign = Align.Center }, false, false, 0);
            vbox.PackStart(new Separator(Orientation.Horizontal), false, false, 2);

            // MKV files
            vbox.PackStart(new Label("Select one or more .mkv files:")
                { Halign = Align.Start }, false, false, 0);

            var fileRow = new Box(Orientation.Horizontal, 6);
            var btnFiles = new Button("Files...");
            btnFiles.Clicked += OnSelectFiles;
            fileRow.PackStart(btnFiles, false, false, 0);
            _txtFiles = new Entry { Hexpand = true, IsEditable = false };
            fileRow.PackStart(_txtFiles, true, true, 0);
            vbox.PackStart(fileRow, false, false, 0);

            _lblFileCount = new Label("") { Halign = Align.End };
            vbox.PackStart(_lblFileCount, false, false, 0);

            // Track type
            vbox.PackStart(new Label("Tracks to extract:") { Halign = Align.Start },
                false, false, 0);
            _comboTrackType = new ComboBoxText();
            _comboTrackType.AppendText("All subtitle tracks");
            _comboTrackType.AppendText("All audio tracks");
            _comboTrackType.AppendText("All subtitle and audio tracks");
            _comboTrackType.Active = 0;
            vbox.PackStart(_comboTrackType, false, false, 0);

            // Output directory
            vbox.PackStart(new Label("Directory where the extracted tracks will be placed:")
                { Halign = Align.Start }, false, false, 0);

            var outRow = new Box(Orientation.Horizontal, 6);
            var btnOut = new Button("Output...");
            btnOut.Clicked += OnSelectOutDir;
            outRow.PackStart(btnOut, false, false, 0);
            _txtOutDir = new Entry
            {
                Hexpand = true,
                Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            outRow.PackStart(_txtOutDir, true, true, 0);
            vbox.PackStart(outRow, false, false, 0);

            // Progress (hidden until extraction starts)
            _lblProgress = new Label("") { Halign = Align.Start, NoShowAll = true };
            vbox.PackStart(_lblProgress, false, false, 0);

            _progressEpisode = new ProgressBar { ShowText = true, NoShowAll = true };
            vbox.PackStart(_progressEpisode, false, false, 0);

            _progressTrack = new ProgressBar { ShowText = true, NoShowAll = true };
            vbox.PackStart(_progressTrack, false, false, 0);

            // Extract/Stop button
            _btnExtract = new Button("Extract") { Halign = Align.Center, WidthRequest = 120 };
            _btnExtract.Clicked += OnExtractClicked;
            vbox.PackStart(_btnExtract, false, false, 4);

            ContentArea.PackStart(vbox, true, true, 0);
            ContentArea.ShowAll();
        }

        // ── FILE SELECTION ──────────────────────────────────────────────────

        private void OnSelectFiles(object sender, EventArgs e)
        {
            var dlg = new FileChooserDialog("Select One or More MKV Files", this,
                FileChooserAction.Open,
                "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
            dlg.SelectMultiple = true;

            var filter = new FileFilter { Name = "Matroska files (*.mkv)" };
            filter.AddPattern("*.mkv");
            dlg.AddFilter(filter);

            if (dlg.Run() == (int)ResponseType.Accept)
            {
                _selectedFiles.Clear();
                foreach (string f in dlg.Filenames)
                {
                    if (SysPath.GetExtension(f).ToLowerInvariant() == ".mkv")
                        _selectedFiles.Add(f);
                }
                UpdateFileDisplay();
            }
            dlg.Destroy();
        }

        private void OnSelectOutDir(object sender, EventArgs e)
        {
            var dlg = new FileChooserDialog("Select Output Directory", this,
                FileChooserAction.SelectFolder,
                "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);

            if (dlg.Run() == (int)ResponseType.Accept)
                _txtOutDir.Text = dlg.Filename;
            dlg.Destroy();
        }

        private void UpdateFileDisplay()
        {
            var names = new List<string>(_selectedFiles.Count);
            foreach (string f in _selectedFiles)
                names.Add($"\"{SysPath.GetFileName(f)}\"");
            _txtFiles.Text = string.Join(", ", names);

            _lblFileCount.Text = _selectedFiles.Count == 1
                ? "1 file selected"
                : $"{_selectedFiles.Count} files selected";
        }

        // ── EXTRACTION ──────────────────────────────────────────────────────

        private async void OnExtractClicked(object sender, EventArgs e)
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

            string outDir = _txtOutDir.Text.Trim();
            if (!Directory.Exists(outDir))
            {
                UtilsMsg.showErrMsg("Please enter a valid output directory.");
                return;
            }

            string trackType = _comboTrackType.ActiveText;
            var files = new List<string>(_selectedFiles);

            _btnExtract.Label = "Stop";
            _lblProgress.Visible = true;
            _progressEpisode.Visible = true;
            _progressTrack.Visible = true;
            _progressEpisode.Fraction = 0;
            _progressTrack.Fraction = 0;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            Exception error = null;

            try
            {
                await Task.Run(() => ExtractTracks(files, trackType, outDir, token));
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { error = ex; }

            _cts.Dispose();
            _cts = null;

            if (_destroyed) return;

            _btnExtract.Label = "Extract";
            _lblProgress.Visible = false;
            _progressEpisode.Visible = false;
            _progressTrack.Visible = false;

            if (error != null)
                UtilsMsg.showErrMsg($"Something went wrong: {error.Message}");
            else if (!token.IsCancellationRequested)
                UtilsMsg.showInfoMsg("Extraction complete.");
        }

        private void ExtractTracks(List<string> files, string trackType,
            string outDir, CancellationToken token)
        {
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
                        $"{SysPath.GetFileNameWithoutExtension(files[i])} - Track {Convert.ToInt32(track.TrackID):00} - {displayLang}.{track.Extension}");

                    // Capture for closure
                    int curEp = i + 1, maxEp = files.Count;
                    int curTrack = t + 1, maxTrack = tracks.Count;

                    GLib.Idle.Add(() =>
                    {
                        if (_destroyed) return false;
                        _lblProgress.Text =
                            $"Extracting track {curTrack}/{maxTrack} from file {curEp}/{maxEp}...";
                        _progressEpisode.Fraction = (double)curEp / maxEp;
                        _progressEpisode.Text = $"File {curEp}/{maxEp}";
                        _progressTrack.Fraction = (double)curTrack / maxTrack;
                        _progressTrack.Text = $"Track {curTrack}/{maxTrack}";
                        return false;
                    });

                    UtilsMkv.extractTrack(files[i], track.TrackID, fileName);
                }
            }
        }
    }
}
