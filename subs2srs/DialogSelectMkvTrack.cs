using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Gtk;
using IOPath = System.IO.Path;

namespace subs2srs
{
    public class DialogSelectMkvTrack : Dialog
    {
        private readonly string _mkvFile;
        private readonly int _subsNum;
        private readonly List<MkvTrack> _tracks;

        private ComboBoxText _comboTrack;
        private Button _btnExtract;
        private Label _lblProgress;
        private ProgressBar _progressBar;

        public string ExtractedFile { get; private set; } = "";

        public DialogSelectMkvTrack(Window parent, string mkvFile, int subsNum, List<MkvTrack> tracks)
            : base("Select MKV Subtitle Track", parent, DialogFlags.Modal)
        {
            _mkvFile = mkvFile;
            _subsNum = subsNum;
            _tracks = tracks;
            SetDefaultSize(340, 150);
            Resizable = false;
            BuildUI();
        }

        private void BuildUI()
        {
            var vbox = new Box(Orientation.Vertical, 6) { BorderWidth = 10 };

            vbox.PackStart(new Label("Select MKV subtitle track to use:") { Halign = Align.Start }, false, false, 0);

            _comboTrack = new ComboBoxText();
            foreach (var t in _tracks)
                _comboTrack.AppendText(t.ToString());
            if (_tracks.Count > 0) _comboTrack.Active = 0;
            vbox.PackStart(_comboTrack, false, false, 0);

            _lblProgress = new Label("Extracting subtitle track...") { Halign = Align.Start, Visible = false };
            vbox.PackStart(_lblProgress, false, false, 0);

            _progressBar = new ProgressBar { Visible = false };
            vbox.PackStart(_progressBar, false, false, 0);

            _btnExtract = new Button("Extract");
            _btnExtract.Clicked += OnExtractClicked;
            var btnBox = new Box(Orientation.Horizontal, 0);
            btnBox.PackEnd(_btnExtract, false, false, 0);
            vbox.PackStart(btnBox, false, false, 0);

            ContentArea.PackStart(vbox, true, true, 0);
            ContentArea.ShowAll();
        }

        private async void OnExtractClicked(object sender, EventArgs e)
        {
            if (_comboTrack.Active < 0) return;

            _btnExtract.Sensitive = false;
            _lblProgress.Visible = true;
            _progressBar.Visible = true;
            _progressBar.Pulse();

            var selectedTrack = _tracks[_comboTrack.Active];
            string tempFileName = _subsNum == 2
                ? ConstantSettings.TempMkvExtractSubs2Filename
                : ConstantSettings.TempMkvExtractSubs1Filename;

            string extractedFile = $"{IOPath.GetTempPath()}{tempFileName}.{selectedTrack.Extension}";

            uint pulseTimer = GLib.Timeout.Add(100, () => { _progressBar.Pulse(); return true; });

            await Task.Run(() => UtilsMkv.extractTrack(_mkvFile, selectedTrack.TrackID, extractedFile));

            GLib.Source.Remove(pulseTimer);

            ExtractedFile = extractedFile;
            if (IOPath.GetExtension(ExtractedFile) == ".sub")
                ExtractedFile = IOPath.ChangeExtension(ExtractedFile, ".idx");

            Respond(ResponseType.Ok);
        }
    }
}
