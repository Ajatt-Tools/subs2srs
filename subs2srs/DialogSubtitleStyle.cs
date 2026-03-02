using System;
using System.Collections.Generic;
using Gtk;

namespace subs2srs
{
    public class DialogSubtitleStyle : Dialog
    {
        private FontButton _fontButton;
        private CheckButton _chkUnderline, _chkStrikeout;

        private ColorButton _colorPrimary, _colorSecondary, _colorOutline, _colorShadow;
        private SpinButton _opacityPrimary, _opacitySecondary, _opacityOutline, _opacityShadow;

        private RadioButton[] _alignRadios = new RadioButton[10]; // index 1-9

        private SpinButton _marginLeft, _marginRight, _marginVertical;
        private SpinButton _spinOutline, _spinShadow;
        private CheckButton _chkOpaqueBox;

        private SpinButton _scaleX, _scaleY, _rotation, _spacing;
        private ComboBoxText _comboEncoding;

        private InfoStyle _style = new InfoStyle();

        public InfoStyle Style
        {
            get { SaveToStyle(); return _style; }
            set { _style = value; LoadFromStyle(); }
        }

        public DialogSubtitleStyle(Window parent, string title = "Subtitle Style")
            : base(title, parent, DialogFlags.Modal,
                "Cancel", ResponseType.Cancel, "OK", ResponseType.Ok)
        {
            SetDefaultSize(660, 380);
            Resizable = false;
            BuildUI();
            LoadFromStyle();
        }

        private void BuildUI()
        {
            var mainBox = new Box(Orientation.Horizontal, 8) { BorderWidth = 8 };
            var leftBox = new Box(Orientation.Vertical, 6);
            var rightBox = new Box(Orientation.Vertical, 6);

            // ── Font ──────────────────────────────────────────
            var fontFrame = new Frame("Font");
            var fontBox = new Box(Orientation.Vertical, 4) { BorderWidth = 6 };
            _fontButton = new FontButton { Hexpand = true, Title = "Select Font" };
            fontBox.PackStart(_fontButton, false, false, 0);
            var fontOptBox = new Box(Orientation.Horizontal, 8);
            _chkUnderline = new CheckButton("Underline");
            _chkStrikeout = new CheckButton("Strikeout");
            fontOptBox.PackStart(_chkUnderline, false, false, 0);
            fontOptBox.PackStart(_chkStrikeout, false, false, 0);
            fontBox.PackStart(fontOptBox, false, false, 0);
            fontFrame.Add(fontBox);
            leftBox.PackStart(fontFrame, false, false, 0);

            // ── Colors ────────────────────────────────────────
            var colorFrame = new Frame("Colors");
            var cg = new Grid { RowSpacing = 4, ColumnSpacing = 6, BorderWidth = 6 };
            cg.Attach(new Label("Color") { Halign = Align.Center }, 1, 0, 1, 1);
            cg.Attach(new Label("Opacity") { Halign = Align.Center }, 2, 0, 1, 1);

            _colorPrimary = NewColorBtn(SrsColor.White);
            _colorSecondary = NewColorBtn(SrsColor.Red);
            _colorOutline = NewColorBtn(SrsColor.Black);
            _colorShadow = NewColorBtn(SrsColor.Black);
            _opacityPrimary = new SpinButton(0, 255, 1);
            _opacitySecondary = new SpinButton(0, 255, 1);
            _opacityOutline = new SpinButton(0, 255, 1);
            _opacityShadow = new SpinButton(0, 255, 1);

            AttachColorRow(cg, 1, "Primary:", _colorPrimary, _opacityPrimary);
            AttachColorRow(cg, 2, "Secondary:", _colorSecondary, _opacitySecondary);
            AttachColorRow(cg, 3, "Outline:", _colorOutline, _opacityOutline);
            AttachColorRow(cg, 4, "Shadow:", _colorShadow, _opacityShadow);
            colorFrame.Add(cg);
            leftBox.PackStart(colorFrame, false, false, 0);

            // ── Outline / Shadow ──────────────────────────────
            var outFrame = new Frame("Outline");
            var og = new Grid { RowSpacing = 4, ColumnSpacing = 6, BorderWidth = 6 };
            og.Attach(new Label("Outline:") { Halign = Align.End }, 0, 0, 1, 1);
            _spinOutline = new SpinButton(0, 4, 1) { Value = 2 };
            og.Attach(_spinOutline, 1, 0, 1, 1);
            og.Attach(new Label("px"), 2, 0, 1, 1);
            og.Attach(new Label("Shadow:") { Halign = Align.End }, 0, 1, 1, 1);
            _spinShadow = new SpinButton(0, 4, 1) { Value = 2 };
            og.Attach(_spinShadow, 1, 1, 1, 1);
            og.Attach(new Label("px"), 2, 1, 1, 1);
            _chkOpaqueBox = new CheckButton("Opaque box");
            og.Attach(_chkOpaqueBox, 0, 2, 3, 1);
            outFrame.Add(og);
            leftBox.PackStart(outFrame, false, false, 0);

            // ── Alignment ─────────────────────────────────────
            var alFrame = new Frame("Alignment");
            var ag = new Grid { RowSpacing = 2, ColumnSpacing = 4, BorderWidth = 6, Halign = Align.Center };
            ag.Attach(new Label("Top") { Halign = Align.Center }, 1, 0, 1, 1);
            ag.Attach(new Label("Left") { Halign = Align.End }, 0, 2, 1, 1);
            ag.Attach(new Label("Right"), 4, 2, 1, 1);
            ag.Attach(new Label("Bottom") { Halign = Align.Center }, 1, 4, 1, 1);

            // Top=7,8,9  Mid=4,5,6  Bot=1,2,3
            int[,] map = { { 7, 8, 9 }, { 4, 5, 6 }, { 1, 2, 3 } };
            RadioButton first = null;
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                {
                    int idx = map[r, c];
                    var rb = first == null ? new RadioButton("") : new RadioButton(first, "");
                    if (first == null) first = rb;
                    _alignRadios[idx] = rb;
                    ag.Attach(rb, c + 1, r + 1, 1, 1);
                }
            alFrame.Add(ag);
            rightBox.PackStart(alFrame, false, false, 0);

            // ── Margins ───────────────────────────────────────
            var mFrame = new Frame("Margins");
            var mg = new Grid { RowSpacing = 4, ColumnSpacing = 6, BorderWidth = 6 };
            mg.Attach(new Label("Left:") { Halign = Align.End }, 0, 0, 1, 1);
            _marginLeft = new SpinButton(0, 999, 1) { Value = 10 };
            mg.Attach(_marginLeft, 1, 0, 1, 1);
            mg.Attach(new Label("px"), 2, 0, 1, 1);
            mg.Attach(new Label("Right:") { Halign = Align.End }, 0, 1, 1, 1);
            _marginRight = new SpinButton(0, 999, 1) { Value = 10 };
            mg.Attach(_marginRight, 1, 1, 1, 1);
            mg.Attach(new Label("px"), 2, 1, 1, 1);
            mg.Attach(new Label("Vertical:") { Halign = Align.End }, 0, 2, 1, 1);
            _marginVertical = new SpinButton(0, 999, 1) { Value = 10 };
            mg.Attach(_marginVertical, 1, 2, 1, 1);
            mg.Attach(new Label("px"), 2, 2, 1, 1);
            mFrame.Add(mg);
            rightBox.PackStart(mFrame, false, false, 0);

            // ── Misc ──────────────────────────────────────────
            var miscFrame = new Frame("Misc");
            var xg = new Grid { RowSpacing = 4, ColumnSpacing = 6, BorderWidth = 6 };
            int xr = 0;
            xg.Attach(new Label("Scale X:") { Halign = Align.End }, 0, xr, 1, 1);
            _scaleX = new SpinButton(30, 150, 1) { Value = 100 };
            xg.Attach(_scaleX, 1, xr, 1, 1);
            xg.Attach(new Label("%"), 2, xr, 1, 1);
            xg.Attach(new Label("Scale Y:") { Halign = Align.End }, 3, xr, 1, 1);
            _scaleY = new SpinButton(30, 150, 1) { Value = 100 };
            xg.Attach(_scaleY, 4, xr, 1, 1);
            xg.Attach(new Label("%"), 5, xr, 1, 1);
            xr++;
            xg.Attach(new Label("Rotation:") { Halign = Align.End }, 0, xr, 1, 1);
            _rotation = new SpinButton(0, 359, 1) { Value = 0 };
            xg.Attach(_rotation, 1, xr, 1, 1);
            xg.Attach(new Label("deg"), 2, xr, 1, 1);
            xg.Attach(new Label("Spacing:") { Halign = Align.End }, 3, xr, 1, 1);
            _spacing = new SpinButton(0, 10, 1) { Value = 0 };
            xg.Attach(_spacing, 4, xr, 1, 1);
            xg.Attach(new Label("px"), 5, xr, 1, 1);
            xr++;
            xg.Attach(new Label("Encoding:") { Halign = Align.End }, 0, xr, 1, 1);
            _comboEncoding = new ComboBoxText();
            foreach (var enc in StyleEncoding.getDefaultList())
                _comboEncoding.AppendText(enc.ToString());
            _comboEncoding.Active = 1;
            _comboEncoding.Hexpand = true;
            xg.Attach(_comboEncoding, 1, xr, 5, 1);
            miscFrame.Add(xg);
            rightBox.PackStart(miscFrame, false, false, 0);

            mainBox.PackStart(leftBox, true, true, 0);
            mainBox.PackStart(rightBox, true, true, 0);
            ContentArea.PackStart(mainBox, true, true, 0);
            ContentArea.ShowAll();
        }

        // ── Helpers ───────────────────────────────────────────

        private static ColorButton NewColorBtn(SrsColor c)
        {
            var btn = new ColorButton { UseAlpha = false };
            btn.Rgba = ColorToRgba(c);
            return btn;
        }

        private static void AttachColorRow(Grid g, int row, string label, ColorButton btn, SpinButton spin)
        {
            g.Attach(new Label(label) { Halign = Align.End }, 0, row, 1, 1);
            btn.WidthRequest = 50;
            g.Attach(btn, 1, row, 1, 1);
            g.Attach(spin, 2, row, 1, 1);
        }

        private static Gdk.RGBA ColorToRgba(SrsColor c)
        {
            var r = new Gdk.RGBA();
            r.Red = c.R / 255.0; r.Green = c.G / 255.0; r.Blue = c.B / 255.0; r.Alpha = 1.0;
            return r;
        }

        private static SrsColor RgbaToColor(Gdk.RGBA r) =>
            SrsColor.FromArgb((int)(r.Red * 255), (int)(r.Green * 255), (int)(r.Blue * 255));

        private static string FontInfoToDesc(FontInfo f)
        {
            string s = f.Name;
            if (f.Bold) s += " Bold";
            if (f.Italic) s += " Italic";
            s += " " + (int)f.Size;
            return s;
        }
        
        private static FontInfo DescToFontInfo(string desc)
        {
            try
            {
                var pd = Pango.FontDescription.FromString(desc);
                string family = pd.Family ?? "Arial";
                float size = pd.Size / 1024f;
                if (size <= 0) size = 20;
                return new FontInfo(family, size,
                    bold: pd.Weight >= Pango.Weight.Bold,
                    italic: pd.Style == Pango.Style.Italic || pd.Style == Pango.Style.Oblique);
            }
            catch { return new FontInfo(); }
        }

        // ── Load / Save ──────────────────────────────────────

        private void LoadFromStyle()
        {
            _fontButton.Font = FontInfoToDesc(_style.Font);
            _chkUnderline.Active = _style.Font.Underline;
            _chkStrikeout.Active = _style.Font.Strikeout;

            _colorPrimary.Rgba = ColorToRgba(_style.ColorPrimary);
            _colorSecondary.Rgba = ColorToRgba(_style.ColorSecondary);
            _colorOutline.Rgba = ColorToRgba(_style.ColorOutline);
            _colorShadow.Rgba = ColorToRgba(_style.ColorShadow);
            _opacityPrimary.Value = _style.OpacityPrimary;
            _opacitySecondary.Value = _style.OpacitySecondary;
            _opacityOutline.Value = _style.OpacityOutline;
            _opacityShadow.Value = _style.OpacityShadow;

            _spinOutline.Value = _style.Outline;
            _spinShadow.Value = _style.Shadow;
            _chkOpaqueBox.Active = _style.OpaqueBox;

            for (int i = 1; i <= 9; i++)
                _alignRadios[i].Active = (_style.Alignment == i);

            _marginLeft.Value = _style.MarginLeft;
            _marginRight.Value = _style.MarginRight;
            _marginVertical.Value = _style.MarginVertical;

            _scaleX.Value = _style.ScaleX;
            _scaleY.Value = _style.ScaleY;
            _rotation.Value = _style.Rotation;
            _spacing.Value = _style.Spacing;

            var defaults = StyleEncoding.getDefaultList();
            for (int i = 0; i < defaults.Count; i++)
                if (defaults[i].Num == _style.Encoding.Num) { _comboEncoding.Active = i; break; }
        }

        private void SaveToStyle()
        {
            var fi = DescToFontInfo(_fontButton.Font);
            fi.Underline = _chkUnderline.Active;
            fi.Strikeout = _chkStrikeout.Active;
            _style.Font = fi;

            _style.ColorPrimary = RgbaToColor(_colorPrimary.Rgba);
            _style.ColorSecondary = RgbaToColor(_colorSecondary.Rgba);
            _style.ColorOutline = RgbaToColor(_colorOutline.Rgba);
            _style.ColorShadow = RgbaToColor(_colorShadow.Rgba);
            _style.OpacityPrimary = (int)_opacityPrimary.Value;
            _style.OpacitySecondary = (int)_opacitySecondary.Value;
            _style.OpacityOutline = (int)_opacityOutline.Value;
            _style.OpacityShadow = (int)_opacityShadow.Value;

            _style.Outline = (int)_spinOutline.Value;
            _style.Shadow = (int)_spinShadow.Value;
            _style.OpaqueBox = _chkOpaqueBox.Active;

            for (int i = 1; i <= 9; i++)
                if (_alignRadios[i].Active) { _style.Alignment = i; break; }

            _style.MarginLeft = (int)_marginLeft.Value;
            _style.MarginRight = (int)_marginRight.Value;
            _style.MarginVertical = (int)_marginVertical.Value;

            _style.ScaleX = (int)_scaleX.Value;
            _style.ScaleY = (int)_scaleY.Value;
            _style.Rotation = (int)_rotation.Value;
            _style.Spacing = (int)_spacing.Value;

            int encIdx = _comboEncoding.Active;
            if (encIdx >= 0)
                _style.Encoding = StyleEncoding.getDefaultList()[encIdx];
        }
    }
}
