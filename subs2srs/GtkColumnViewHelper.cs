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

using System;
using System.Runtime.InteropServices;

namespace subs2srs
{
    /// <summary>
    /// P/Invoke helpers for GTK4 ColumnViewColumn features
    /// not exposed by gir.core 0.7.0:
    ///   - gtk_column_view_column_set_resizable
    ///   - gtk_column_view_column_set_fixed_width
    ///   - gtk_column_view_column_set_expand
    ///
    /// Important: never combine SetExpand(true) with SetFixedWidth on
    /// the same column — GTK4 layout engine will fight the drag-resize,
    /// causing the cursor to drift and wrong columns to move.
    /// Use either fixed_width (for resizable columns) or expand (for
    /// the last column that absorbs remaining space), not both.
    /// </summary>
    public static class GtkColumnViewHelper
    {
        private const string GtkLib = "gtk-4";

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void gtk_column_view_column_set_resizable(
            IntPtr column, bool resizable);

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void gtk_column_view_column_set_fixed_width(
            IntPtr column, int fixedWidth);

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void gtk_column_view_column_set_expand(
            IntPtr column, bool expand);

        /// <summary>
        /// Enable or disable drag-resize handle on a ColumnViewColumn.
        /// </summary>
        public static void SetResizable(Gtk.ColumnViewColumn column, bool resizable)
        {
            gtk_column_view_column_set_resizable(
                column.Handle.DangerousGetHandle(), resizable);
        }

        /// <summary>
        /// Set fixed width in pixels. Pass -1 to unset.
        /// Do not use together with SetExpand(true) on the same column.
        /// </summary>
        public static void SetFixedWidth(Gtk.ColumnViewColumn column, int width)
        {
            gtk_column_view_column_set_fixed_width(
                column.Handle.DangerousGetHandle(), width);
        }

        /// <summary>
        /// Set whether this column expands to fill remaining space.
        /// Do not use together with SetFixedWidth on the same column.
        /// </summary>
        public static void SetExpand(Gtk.ColumnViewColumn column, bool expand)
        {
            gtk_column_view_column_set_expand(
                column.Handle.DangerousGetHandle(), expand);
        }
    }
}
