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
using System.Threading.Tasks;
using IOPath = System.IO.Path;

namespace subs2srs
{
    /// <summary>
    /// Modal dialog to select and extract a subtitle track from an MKV file.
    /// GTK4: replaces Gtk.Dialog with Gtk.Window + nested main loop for
    /// synchronous-style RunDialog() calls.
    /// </summary>
    public class DialogSelectMkvTrack : Gtk.Window
    {
        private readonly string _mkvFile;
        private readonly int _subsNum;
        private readonly List<MkvTrack> _tracks;

        private Gtk.DropDown _dropTrack;
        private Gtk.StringList _trackModel;
        private Gtk.Button _btnExtract;
        private Gtk.Label _lblProgress;
        private Gtk.ProgressBar _progressBar;

        public string ExtractedFile { get; private set; } = "";

        // Result: true = OK (extracted), false = cancelled
        private bool? _result;
        private GLib.MainLoop _loop;

        public DialogSelectMkvTrack(Gtk.Window parent, string mkvFile, int subsNum, List<MkvTrack> tracks)
        {
            _mkvFile = mkvFile;
            _subsNum = subsNum;
            _tracks = tracks;

            SetTitle("Select MKV Subtitle Track");
            SetDefaultSize(340, 150);
            SetModal(true);
            if (parent != null) SetTransientFor(parent);

            BuildUI();
        }

        private void BuildUI()
        {
            var vbox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            vbox.SetMarginTop(10);
            vbox.SetMarginBottom(10);
            vbox.SetMarginStart(10);
            vbox.SetMarginEnd(10);

            var lbl = Gtk.Label.New("Select MKV subtitle track to use:");
            lbl.SetHalign(Gtk.Align.Start);
            vbox.Append(lbl);

            // Build track dropdown
            var trackNames = new string[_tracks.Count];
            for (int i = 0; i < _tracks.Count; i++)
                trackNames[i] = _tracks[i].ToString();
            _trackModel = Gtk.StringList.New(trackNames);
            _dropTrack = Gtk.DropDown.New(_trackModel, null);
            if (_tracks.Count > 0) _dropTrack.SetSelected(0);
            vbox.Append(_dropTrack);

            _lblProgress = Gtk.Label.New("Extracting subtitle track...");
            _lblProgress.SetHalign(Gtk.Align.Start);
            _lblProgress.SetVisible(false);
            vbox.Append(_lblProgress);

            _progressBar = Gtk.ProgressBar.New();
            _progressBar.SetVisible(false);
            vbox.Append(_progressBar);

            _btnExtract = Gtk.Button.NewWithLabel("Extract");
            _btnExtract.OnClicked += OnExtractClicked;
            var btnBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 0);
            btnBox.SetHalign(Gtk.Align.End);
            btnBox.Append(_btnExtract);
            vbox.Append(btnBox);

            SetChild(vbox);
        }

        /// <summary>
        /// Show the dialog modally and return the response.
        /// Spins a nested GLib main loop (same pattern as GTK3 Dialog.Run).
        /// </summary>
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

        private async void OnExtractClicked(Gtk.Button sender, EventArgs e)
        {
            uint sel = _dropTrack.GetSelected();
            if (sel == uint.MaxValue || sel >= (uint)_tracks.Count) return;

            _btnExtract.SetSensitive(false);
            _lblProgress.SetVisible(true);
            _progressBar.SetVisible(true);
            _progressBar.Pulse();

            var selectedTrack = _tracks[(int)sel];
            string tempFileName = _subsNum == 2
                ? ConstantSettings.TempMkvExtractSubs2Filename
                : ConstantSettings.TempMkvExtractSubs1Filename;

            string extractedFile = $"{IOPath.GetTempPath()}{tempFileName}.{selectedTrack.Extension}";

            // Pulse timer using GLib.Functions.TimeoutAdd
            uint pulseTimer = GLib.Functions.TimeoutAdd(0, 100, () =>
            {
                _progressBar.Pulse();
                return true;
            });

            await Task.Run(() => UtilsMkv.extractTrack(_mkvFile, selectedTrack.TrackID, extractedFile));

            GLib.Functions.SourceRemove(pulseTimer);

            ExtractedFile = extractedFile;
            if (IOPath.GetExtension(ExtractedFile) == ".sub")
                ExtractedFile = IOPath.ChangeExtension(ExtractedFile, ".idx");

            _result = true;
            Close();
        }
    }
}
