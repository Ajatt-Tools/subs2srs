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

using System;
using System.Runtime.InteropServices;

namespace subs2srs
{
    /// <summary>
    /// P/Invoke helpers for GTK4 ColumnViewColumn, Widget tree traversal,
    /// and CSS injection.
    ///
    /// gir.core 0.7.0 generates managed wrappers for all of these APIs
    /// (ColumnViewColumn.Resizable/FixedWidth/Expand,
    /// Widget.GetFirstChild/GetNextSibling/AddCssClass,
    /// CssProvider.LoadFromData, StyleContext.AddProviderForDisplay, etc.)
    /// but using the managed ColumnViewColumn properties produced
    /// unpredictable drag-resize behavior and inter-column gaps.
    /// Direct P/Invoke gives correct results, so it is used throughout
    /// for consistency.
    ///
    /// Important: never combine SetExpand(true) with SetFixedWidth on
    /// the same column — GTK4 layout engine will fight the drag-resize,
    /// causing the cursor to drift and wrong columns to move.
    /// Use either fixed_width (for resizable columns) or expand (for
    /// the last column that absorbs remaining space), not both.
    ///
    /// All symbols live in libgtk-4.so.1 on Linux (GTK, GDK, GSK
    /// are in the same shared library), so "gtk-4" works for
    /// both gtk_ and gdk_ functions via gir.core import resolver.
    /// </summary>
    public static class GtkColumnViewHelper
    {
        private const string GtkLib = "gtk-4";

        // ── ColumnViewColumn properties ────────────────────────────────

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

        // ── Widget tree traversal ────────────────────────────────────

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr gtk_widget_get_first_child(IntPtr widget);

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr gtk_widget_get_next_sibling(IntPtr widget);

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr gtk_widget_get_last_child(IntPtr widget);

        // ── Widget CSS class manipulation ────────────────────────────

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void gtk_widget_add_css_class(
            IntPtr widget, [MarshalAs(UnmanagedType.LPUTF8Str)] string cssClass);

        // ── Inline CSS on a single widget via CssProvider ────────────
        // GTK4 does not have gtk_widget_set_style(); instead we create
        // a per-widget CssProvider and add it to the widget's own
        // StyleContext (display-level provider would need a selector).

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr gtk_widget_get_style_context(IntPtr widget);

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void gtk_style_context_add_provider(
            IntPtr context, IntPtr provider, uint priority);

        /// <summary>
        /// Apply inline CSS to a single widget. Creates a CssProvider,
        /// loads the CSS wrapped in a wildcard selector, and attaches it
        /// to the widget's own StyleContext at priority 900 (above theme).
        /// </summary>
        public static void ApplyInlineCss(IntPtr widget, string css)
        {
            if (widget == IntPtr.Zero) return;
            IntPtr provider = gtk_css_provider_new();
            // Wrap in "* { ... }" so it matches the widget itself
            string wrapped = "* { " + css + " }";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(wrapped);
            gtk_css_provider_load_from_data(provider, data, data.Length);

            IntPtr ctx = gtk_widget_get_style_context(widget);
            if (ctx != IntPtr.Zero)
            {
                gtk_style_context_add_provider(ctx, provider, 900);
            }
        }

        /// <summary>
        /// Walk the ColumnView widget tree to find header row buttons
        /// and apply inline CSS to each one. GTK4 ColumnView internal
        /// structure: ColumnView → first child is the header listbox,
        /// header listbox → children are GtkColumnViewRowWidget,
        /// each row widget → children are button widgets for each column.
        ///
        /// Call this after the ColumnView has been mapped (shown),
        /// or defer with GLib.Functions.IdleAdd so the widget tree
        /// is fully built.
        /// </summary>
        public static void StyleColumnViewHeaders(Gtk.ColumnView columnView, string css)
        {
            IntPtr cv = columnView.Handle.DangerousGetHandle();
            if (cv == IntPtr.Zero) return;

            // The header is the first child of the ColumnView
            IntPtr header = gtk_widget_get_first_child(cv);
            if (header == IntPtr.Zero) return;

            // The header contains row widgets; iterate their children (buttons)
            IntPtr rowWidget = gtk_widget_get_first_child(header);
            while (rowWidget != IntPtr.Zero)
            {
                // Each button inside the row widget is a column header
                IntPtr button = gtk_widget_get_first_child(rowWidget);
                while (button != IntPtr.Zero)
                {
                    ApplyInlineCss(button, css);
                    button = gtk_widget_get_next_sibling(button);
                }
                rowWidget = gtk_widget_get_next_sibling(rowWidget);
            }
        }

        // ── CSS injection via P/Invoke ─────────────────────────────────
        // gir.core 0.7.0 auto-generates CssProvider but does not expose
        // LoadFromData or StyleContext.AddProviderForDisplay in usable
        // managed form. We call the C API directly.

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr gtk_css_provider_new();

        // Available in all GTK4 versions (deprecated in 4.12, not removed).
        // gssize length → nint; pass byte count (not -1) for safety.
        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void gtk_css_provider_load_from_data(
            IntPtr provider, byte[] data, nint length);

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void gtk_style_context_add_provider_for_display(
            IntPtr display, IntPtr provider, uint priority);

        // gdk_display_get_default lives in the same .so as gtk_ symbols
        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr gdk_display_get_default();

        /// <summary>
        /// Register a global CSS stylesheet for the default display.
        /// Priority 800 overrides theme defaults (600) but not user
        /// stylesheets (GTK_STYLE_PROVIDER_PRIORITY_USER = 800 is equal,
        /// so we use 800 to match user-level priority).
        /// </summary>
        public static void ApplyGlobalCss(string css)
        {
            IntPtr provider = gtk_css_provider_new();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(css);
            gtk_css_provider_load_from_data(provider, data, data.Length);

            IntPtr display = gdk_display_get_default();
            if (display != IntPtr.Zero)
            {
                gtk_style_context_add_provider_for_display(display, provider, 800);
            }
        }
    }
}
