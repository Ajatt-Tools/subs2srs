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

namespace subs2srs
{
    /// <summary>
    /// Subtitle style editor dialog.
    /// GTK4: Gtk.Dialog replaced with Gtk.Window + nested main loop.
    /// FontButton/ColorButton replaced with GTK4 FontDialogButton/ColorDialogButton.
    /// RadioButton replaced with grouped CheckButtons.
    /// </summary>
    public class DialogSubtitleStyle : Gtk.Window
    {
        private Gtk.FontDialogButton _fontButton;
        private Gtk.CheckButton _chkUnderline, _chkStrikeout;

        private Gtk.ColorDialogButton _colorPrimary, _colorSecondary, _colorOutline, _colorShadow;
        private Gtk.SpinButton _opacityPrimary, _opacitySecondary, _opacityOutline, _opacityShadow;

        // Alignment radio group: index 1-9, stored as CheckButtons with grouping
        private Gtk.CheckButton[] _alignRadios = new Gtk.CheckButton[10];

        private Gtk.SpinButton _marginLeft, _marginRight, _marginVertical;
        private Gtk.SpinButton _spinOutline, _spinShadow;
        private Gtk.CheckButton _chkOpaqueBox;

        private Gtk.SpinButton _scaleX, _scaleY, _rotation, _spacing;
        private Gtk.DropDown _dropEncoding;
        private Gtk.StringList _encodingModel;

        private InfoStyle _style = new InfoStyle();

        // Dialog result
        private bool? _result;
        private GLib.MainLoop _loop;

        public InfoStyle Style
        {
            get { SaveToStyle(); return _style; }
            set { _style = value; LoadFromStyle(); }
        }

        public DialogSubtitleStyle(Gtk.Window parent, string title = "Subtitle Style")
        {
            SetTitle(title);
            SetDefaultSize(660, 380);
            SetModal(true);
            if (parent != null) SetTransientFor(parent);

            BuildUI();
            LoadFromStyle();
        }

        /// <summary>
        /// Show modally. Returns true if OK was clicked.
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

        private void BuildUI()
        {
            var mainBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 8);
            mainBox.SetMarginTop(8);
            mainBox.SetMarginBottom(8);
            mainBox.SetMarginStart(8);
            mainBox.SetMarginEnd(8);
            var leftBox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            var rightBox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);

            // ── Font ──────────────────────────────────────────
            var fontFrame = Gtk.Frame.New("Font");
            var fontBox = Gtk.Box.New(Gtk.Orientation.Vertical, 4);
            fontBox.SetMarginTop(6); fontBox.SetMarginBottom(6);
            fontBox.SetMarginStart(6); fontBox.SetMarginEnd(6);

            var fontDialog = Gtk.FontDialog.New();
            _fontButton = Gtk.FontDialogButton.New(fontDialog);
            _fontButton.SetHexpand(true);
            fontBox.Append(_fontButton);

            var fontOptBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 8);
            _chkUnderline = Gtk.CheckButton.NewWithLabel("Underline");
            _chkStrikeout = Gtk.CheckButton.NewWithLabel("Strikeout");
            fontOptBox.Append(_chkUnderline);
            fontOptBox.Append(_chkStrikeout);
            fontBox.Append(fontOptBox);
            fontFrame.SetChild(fontBox);
            leftBox.Append(fontFrame);

            // ── Colors ────────────────────────────────────────
            var colorFrame = Gtk.Frame.New("Colors");
            var cg = Gtk.Grid.New();
            cg.SetRowSpacing(4); cg.SetColumnSpacing(6);
            cg.SetMarginTop(6); cg.SetMarginBottom(6);
            cg.SetMarginStart(6); cg.SetMarginEnd(6);

            var lblColor = Gtk.Label.New("Color");
            lblColor.SetHalign(Gtk.Align.Center);
            cg.Attach(lblColor, 1, 0, 1, 1);
            var lblOpacity = Gtk.Label.New("Opacity");
            lblOpacity.SetHalign(Gtk.Align.Center);
            cg.Attach(lblOpacity, 2, 0, 1, 1);

            _colorPrimary = NewColorBtn(SrsColor.White);
            _colorSecondary = NewColorBtn(SrsColor.Red);
            _colorOutline = NewColorBtn(SrsColor.Black);
            _colorShadow = NewColorBtn(SrsColor.Black);
            _opacityPrimary = Gtk.SpinButton.NewWithRange(0, 255, 1);
            _opacitySecondary = Gtk.SpinButton.NewWithRange(0, 255, 1);
            _opacityOutline = Gtk.SpinButton.NewWithRange(0, 255, 1);
            _opacityShadow = Gtk.SpinButton.NewWithRange(0, 255, 1);

            AttachColorRow(cg, 1, "Primary:", _colorPrimary, _opacityPrimary);
            AttachColorRow(cg, 2, "Secondary:", _colorSecondary, _opacitySecondary);
            AttachColorRow(cg, 3, "Outline:", _colorOutline, _opacityOutline);
            AttachColorRow(cg, 4, "Shadow:", _colorShadow, _opacityShadow);
            colorFrame.SetChild(cg);
            leftBox.Append(colorFrame);

            // ── Outline / Shadow ──────────────────────────────
            var outFrame = Gtk.Frame.New("Outline");
            var og = Gtk.Grid.New();
            og.SetRowSpacing(4); og.SetColumnSpacing(6);
            og.SetMarginTop(6); og.SetMarginBottom(6);
            og.SetMarginStart(6); og.SetMarginEnd(6);

            var lblOut = Gtk.Label.New("Outline:");
            lblOut.SetHalign(Gtk.Align.End);
            og.Attach(lblOut, 0, 0, 1, 1);
            _spinOutline = Gtk.SpinButton.NewWithRange(0, 4, 1);
            _spinOutline.SetValue(2);
            og.Attach(_spinOutline, 1, 0, 1, 1);
            og.Attach(Gtk.Label.New("px"), 2, 0, 1, 1);

            var lblShd = Gtk.Label.New("Shadow:");
            lblShd.SetHalign(Gtk.Align.End);
            og.Attach(lblShd, 0, 1, 1, 1);
            _spinShadow = Gtk.SpinButton.NewWithRange(0, 4, 1);
            _spinShadow.SetValue(2);
            og.Attach(_spinShadow, 1, 1, 1, 1);
            og.Attach(Gtk.Label.New("px"), 2, 1, 1, 1);

            _chkOpaqueBox = Gtk.CheckButton.NewWithLabel("Opaque box");
            og.Attach(_chkOpaqueBox, 0, 2, 3, 1);
            outFrame.SetChild(og);
            leftBox.Append(outFrame);

            // ── Alignment ─────────────────────────────────────
            var alFrame = Gtk.Frame.New("Alignment");
            var ag = Gtk.Grid.New();
            ag.SetRowSpacing(2); ag.SetColumnSpacing(4);
            ag.SetMarginTop(6); ag.SetMarginBottom(6);
            ag.SetMarginStart(6); ag.SetMarginEnd(6);
            ag.SetHalign(Gtk.Align.Center);

            var lblTop = Gtk.Label.New("Top"); lblTop.SetHalign(Gtk.Align.Center);
            ag.Attach(lblTop, 1, 0, 1, 1);
            var lblLeft = Gtk.Label.New("Left"); lblLeft.SetHalign(Gtk.Align.End);
            ag.Attach(lblLeft, 0, 2, 1, 1);
            ag.Attach(Gtk.Label.New("Right"), 4, 2, 1, 1);
            var lblBot = Gtk.Label.New("Bottom"); lblBot.SetHalign(Gtk.Align.Center);
            ag.Attach(lblBot, 1, 4, 1, 1);

            // Top=7,8,9  Mid=4,5,6  Bot=1,2,3
            int[,] map = { { 7, 8, 9 }, { 4, 5, 6 }, { 1, 2, 3 } };
            Gtk.CheckButton first = null;
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                {
                    int idx = map[r, c];
                    var rb = Gtk.CheckButton.New();
                    if (first != null) rb.SetGroup(first);
                    else first = rb;
                    _alignRadios[idx] = rb;
                    ag.Attach(rb, c + 1, r + 1, 1, 1);
                }
            alFrame.SetChild(ag);
            rightBox.Append(alFrame);

            // ── Margins ───────────────────────────────────────
            var mFrame = Gtk.Frame.New("Margins");
            var mg = Gtk.Grid.New();
            mg.SetRowSpacing(4); mg.SetColumnSpacing(6);
            mg.SetMarginTop(6); mg.SetMarginBottom(6);
            mg.SetMarginStart(6); mg.SetMarginEnd(6);

            var lblML = Gtk.Label.New("Left:"); lblML.SetHalign(Gtk.Align.End);
            mg.Attach(lblML, 0, 0, 1, 1);
            _marginLeft = Gtk.SpinButton.NewWithRange(0, 999, 1); _marginLeft.SetValue(10);
            mg.Attach(_marginLeft, 1, 0, 1, 1);
            mg.Attach(Gtk.Label.New("px"), 2, 0, 1, 1);

            var lblMR = Gtk.Label.New("Right:"); lblMR.SetHalign(Gtk.Align.End);
            mg.Attach(lblMR, 0, 1, 1, 1);
            _marginRight = Gtk.SpinButton.NewWithRange(0, 999, 1); _marginRight.SetValue(10);
            mg.Attach(_marginRight, 1, 1, 1, 1);
            mg.Attach(Gtk.Label.New("px"), 2, 1, 1, 1);

            var lblMV = Gtk.Label.New("Vertical:"); lblMV.SetHalign(Gtk.Align.End);
            mg.Attach(lblMV, 0, 2, 1, 1);
            _marginVertical = Gtk.SpinButton.NewWithRange(0, 999, 1); _marginVertical.SetValue(10);
            mg.Attach(_marginVertical, 1, 2, 1, 1);
            mg.Attach(Gtk.Label.New("px"), 2, 2, 1, 1);
            mFrame.SetChild(mg);
            rightBox.Append(mFrame);

            // ── Misc ──────────────────────────────────────────
            var miscFrame = Gtk.Frame.New("Misc");
            var xg = Gtk.Grid.New();
            xg.SetRowSpacing(4); xg.SetColumnSpacing(6);
            xg.SetMarginTop(6); xg.SetMarginBottom(6);
            xg.SetMarginStart(6); xg.SetMarginEnd(6);
            int xr = 0;

            var lblSX = Gtk.Label.New("Scale X:"); lblSX.SetHalign(Gtk.Align.End);
            xg.Attach(lblSX, 0, xr, 1, 1);
            _scaleX = Gtk.SpinButton.NewWithRange(30, 150, 1); _scaleX.SetValue(100);
            xg.Attach(_scaleX, 1, xr, 1, 1);
            xg.Attach(Gtk.Label.New("%"), 2, xr, 1, 1);
            var lblSY = Gtk.Label.New("Scale Y:"); lblSY.SetHalign(Gtk.Align.End);
            xg.Attach(lblSY, 3, xr, 1, 1);
            _scaleY = Gtk.SpinButton.NewWithRange(30, 150, 1); _scaleY.SetValue(100);
            xg.Attach(_scaleY, 4, xr, 1, 1);
            xg.Attach(Gtk.Label.New("%"), 5, xr, 1, 1);
            xr++;

            var lblRot = Gtk.Label.New("Rotation:"); lblRot.SetHalign(Gtk.Align.End);
            xg.Attach(lblRot, 0, xr, 1, 1);
            _rotation = Gtk.SpinButton.NewWithRange(0, 359, 1); _rotation.SetValue(0);
            xg.Attach(_rotation, 1, xr, 1, 1);
            xg.Attach(Gtk.Label.New("deg"), 2, xr, 1, 1);
            var lblSpc = Gtk.Label.New("Spacing:"); lblSpc.SetHalign(Gtk.Align.End);
            xg.Attach(lblSpc, 3, xr, 1, 1);
            _spacing = Gtk.SpinButton.NewWithRange(0, 10, 1); _spacing.SetValue(0);
            xg.Attach(_spacing, 4, xr, 1, 1);
            xg.Attach(Gtk.Label.New("px"), 5, xr, 1, 1);
            xr++;

            var lblEnc = Gtk.Label.New("Encoding:"); lblEnc.SetHalign(Gtk.Align.End);
            xg.Attach(lblEnc, 0, xr, 1, 1);
            var encList = StyleEncoding.getDefaultList();
            var encNames = new string[encList.Count];
            for (int i = 0; i < encList.Count; i++) encNames[i] = encList[i].ToString();
            _encodingModel = Gtk.StringList.New(encNames);
            _dropEncoding = Gtk.DropDown.New(_encodingModel, null);
            _dropEncoding.SetSelected(1);
            _dropEncoding.SetHexpand(true);
            xg.Attach(_dropEncoding, 1, xr, 5, 1);
            miscFrame.SetChild(xg);
            rightBox.Append(miscFrame);

            leftBox.SetHexpand(true);
            rightBox.SetHexpand(true);
            mainBox.Append(leftBox);
            mainBox.Append(rightBox);

            // Wrap with OK/Cancel buttons at bottom
            var outerBox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
            outerBox.Append(mainBox);

            var btnRow = Gtk.Box.New(Gtk.Orientation.Horizontal, 8);
            btnRow.SetHalign(Gtk.Align.End);
            btnRow.SetMarginTop(6);
            btnRow.SetMarginEnd(8);
            btnRow.SetMarginBottom(8);

            var btnCancel = Gtk.Button.NewWithLabel("Cancel");
            btnCancel.OnClicked += (s, e) => { _result = false; Close(); };
            btnRow.Append(btnCancel);

            var btnOk = Gtk.Button.NewWithLabel("OK");
            btnOk.OnClicked += (s, e) => { _result = true; Close(); };
            btnRow.Append(btnOk);

            outerBox.Append(btnRow);
            SetChild(outerBox);
        }

        // ── Helpers ───────────────────────────────────────────

        private static Gtk.ColorDialogButton NewColorBtn(SrsColor c)
        {
            var dlg = Gtk.ColorDialog.New();
            var btn = Gtk.ColorDialogButton.New(dlg);
            var rgba = new Gdk.RGBA();
            rgba.Red = c.R / 255.0f;
            rgba.Green = c.G / 255.0f;
            rgba.Blue = c.B / 255.0f;
            rgba.Alpha = 1.0f;
            btn.SetRgba(rgba);
            return btn;
        }

        private static void AttachColorRow(Gtk.Grid g, int row, string label,
            Gtk.ColorDialogButton btn, Gtk.SpinButton spin)
        {
            var lbl = Gtk.Label.New(label);
            lbl.SetHalign(Gtk.Align.End);
            g.Attach(lbl, 0, row, 1, 1);
            btn.SetSizeRequest(50, -1);
            g.Attach(btn, 1, row, 1, 1);
            g.Attach(spin, 2, row, 1, 1);
        }

        private static Gdk.RGBA ColorToRgba(SrsColor c)
        {
            var r = new Gdk.RGBA();
            r.Red = c.R / 255.0f; r.Green = c.G / 255.0f; r.Blue = c.B / 255.0f; r.Alpha = 1.0f;
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

        private static FontInfo DescToFontInfo(Pango.FontDescription pd)
        {
            try
            {
                string family = pd.GetFamily() ?? "Arial";
                float size = pd.GetSize() / (float)Pango.Constants.SCALE;
                if (size <= 0) size = 20;
                return new FontInfo(family, size,
                    bold: pd.GetWeight() >= Pango.Weight.Bold,
                    italic: pd.GetStyle() == Pango.Style.Italic ||
                            pd.GetStyle() == Pango.Style.Oblique);
            }
            catch { return new FontInfo(); }
        }

        // ── Load / Save ──────────────────────────────────────

        private void LoadFromStyle()
        {
            var fd = Pango.FontDescription.FromString(FontInfoToDesc(_style.Font));
            _fontButton.SetFontDesc(fd);
            _chkUnderline.SetActive(_style.Font.Underline);
            _chkStrikeout.SetActive(_style.Font.Strikeout);

            _colorPrimary.SetRgba(ColorToRgba(_style.ColorPrimary));
            _colorSecondary.SetRgba(ColorToRgba(_style.ColorSecondary));
            _colorOutline.SetRgba(ColorToRgba(_style.ColorOutline));
            _colorShadow.SetRgba(ColorToRgba(_style.ColorShadow));
            _opacityPrimary.SetValue(_style.OpacityPrimary);
            _opacitySecondary.SetValue(_style.OpacitySecondary);
            _opacityOutline.SetValue(_style.OpacityOutline);
            _opacityShadow.SetValue(_style.OpacityShadow);

            _spinOutline.SetValue(_style.Outline);
            _spinShadow.SetValue(_style.Shadow);
            _chkOpaqueBox.SetActive(_style.OpaqueBox);

            for (int i = 1; i <= 9; i++)
                _alignRadios[i].SetActive(_style.Alignment == i);

            _marginLeft.SetValue(_style.MarginLeft);
            _marginRight.SetValue(_style.MarginRight);
            _marginVertical.SetValue(_style.MarginVertical);

            _scaleX.SetValue(_style.ScaleX);
            _scaleY.SetValue(_style.ScaleY);
            _rotation.SetValue(_style.Rotation);
            _spacing.SetValue(_style.Spacing);

            var defaults = StyleEncoding.getDefaultList();
            for (int i = 0; i < defaults.Count; i++)
                if (defaults[i].Num == _style.Encoding.Num) { _dropEncoding.SetSelected((uint)i); break; }
        }

        private void SaveToStyle()
        {
            var pd = _fontButton.GetFontDesc();
            var fi = DescToFontInfo(pd);
            fi.Underline = _chkUnderline.GetActive();
            fi.Strikeout = _chkStrikeout.GetActive();
            _style.Font = fi;

            _style.ColorPrimary = RgbaToColor(_colorPrimary.GetRgba());
            _style.ColorSecondary = RgbaToColor(_colorSecondary.GetRgba());
            _style.ColorOutline = RgbaToColor(_colorOutline.GetRgba());
            _style.ColorShadow = RgbaToColor(_colorShadow.GetRgba());
            _style.OpacityPrimary = (int)_opacityPrimary.GetValue();
            _style.OpacitySecondary = (int)_opacitySecondary.GetValue();
            _style.OpacityOutline = (int)_opacityOutline.GetValue();
            _style.OpacityShadow = (int)_opacityShadow.GetValue();

            _style.Outline = (int)_spinOutline.GetValue();
            _style.Shadow = (int)_spinShadow.GetValue();
            _style.OpaqueBox = _chkOpaqueBox.GetActive();

            for (int i = 1; i <= 9; i++)
                if (_alignRadios[i].GetActive()) { _style.Alignment = i; break; }

            _style.MarginLeft = (int)_marginLeft.GetValue();
            _style.MarginRight = (int)_marginRight.GetValue();
            _style.MarginVertical = (int)_marginVertical.GetValue();

            _style.ScaleX = (int)_scaleX.GetValue();
            _style.ScaleY = (int)_scaleY.GetValue();
            _style.Rotation = (int)_rotation.GetValue();
            _style.Spacing = (int)_spacing.GetValue();

            uint encIdx = _dropEncoding.GetSelected();
            if (encIdx != uint.MaxValue)
                _style.Encoding = StyleEncoding.getDefaultList()[(int)encIdx];
        }
    }
}
