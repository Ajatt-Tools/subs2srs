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

namespace subs2srs
{
    [Serializable]
    public struct SrsColor
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        public SrsColor(byte r, byte g, byte b, byte a = 255)
        { R = r; G = g; B = b; A = a; }

        public static SrsColor White => new(255, 255, 255);
        public static SrsColor Black => new(0, 0, 0);
        public static SrsColor Red => new(255, 0, 0);

        public static SrsColor FromArgb(int r, int g, int b) =>
            new((byte)r, (byte)g, (byte)b);

        public static SrsColor FromArgb(int a, int r, int g, int b) =>
            new((byte)r, (byte)g, (byte)b, (byte)a);

        public static SrsColor FromArgb(int a, SrsColor c) =>
            new(c.R, c.G, c.B, (byte)a);

        public float GetBrightness()
        {
            float r = R / 255f, g = G / 255f, b = B / 255f;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            return (max + min) / 2f;
        }

        public int ToArgb() => (A << 24) | (R << 16) | (G << 8) | B;

        public static SrsColor FromArgb(int argb) => new(
            (byte)((argb >> 16) & 0xFF),
            (byte)((argb >> 8) & 0xFF),
            (byte)(argb & 0xFF),
            (byte)((argb >> 24) & 0xFF));

        public Gdk.RGBA ToGdkRGBA() => new()
        {
            Red = R / 255.0,
            Green = G / 255.0,
            Blue = B / 255.0,
            Alpha = A / 255.0
        };

        public static SrsColor FromGdkRGBA(Gdk.RGBA c) => new(
            (byte)(c.Red * 255), (byte)(c.Green * 255),
            (byte)(c.Blue * 255), (byte)(c.Alpha * 255));

        public override string ToString() => $"[R={R}, G={G}, B={B}, A={A}]";
    }
}
