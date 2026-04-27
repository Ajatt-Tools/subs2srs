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
using System.IO;
using System.Threading;

namespace subs2srs
{
    class Program
    {
        private static int _mainThreadId;
        private static Gtk.Application _app = null!;

        [STAThread]
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                string msg = ex?.ToString() ?? e.ExceptionObject?.ToString() ?? "Unknown error";
                Console.Error.WriteLine($"FATAL: {msg}");
                Logger.Instance.error(msg);
                Logger.Instance.flush();
            };

            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                Console.Error.WriteLine($"UNOBSERVED: {e.Exception}");
                Logger.Instance.error(e.Exception.ToString());
                Logger.Instance.flush();
            };

            // Must run before anything touches Logger or PrefIO
            EnsureAppDirectories();

            _mainThreadId = Thread.CurrentThread.ManagedThreadId;

            // Wire up message callbacks (GTK-independent delegates in UtilsMsg).
            // Actual dialog display is deferred until GTK is running via InvokeOnMain.
            UtilsMsg.OnShowError = (msg, title) =>
                InvokeOnMain(() => ShowAlertDialog(msg, title));
            UtilsMsg.OnShowInfo = (msg, title) =>
                InvokeOnMain(() => ShowAlertDialog(msg, title));
            UtilsMsg.OnShowConfirm = (msg, title) =>
                InvokeOnMainWithResult(() => ShowConfirmDialog(msg, title));

            _app = Gtk.Application.New("org.subs2srs.app", Gio.ApplicationFlags.FlagsNone);

            _app.OnActivate += OnAppActivate;

            // Run the GLib main loop. Returns exit code.
            return _app.RunWithSynchronizationContext(null);
        }

        /// <summary>
        /// Called once when the application is first activated.
        /// Creates the main window and installs the synchronization context.
        /// </summary>
        private static void OnAppActivate(Gio.Application sender, EventArgs args)
        {
            // Install custom SynchronizationContext so that async/await
            // continuations are marshalled back to the GTK main thread.
            SynchronizationContext.SetSynchronizationContext(
                new GtkSynchronizationContext());

            // GLibLogFilter suppressed toggle_ref warnings specific to GtkSharp.
            // Gir.Core does not use toggle_ref, so the filter is no longer needed.
            // GLibLogFilter.Install();  // removed — not needed with Gir.Core

            var win = new MainWindow(_app);
            win.Show();
        }

        /// <summary>
        /// Create XDG directories on first run.
        /// Must be called before Logger or PrefIO are accessed.
        /// Preferences file is created automatically by PrefIO.read().
        /// </summary>
        private static void EnsureAppDirectories()
        {
            try
            {
                // ~/.config/subs2srs/
                string? configDir = Path.GetDirectoryName(ConstantSettings.SettingsFilename);
                if (!string.IsNullOrEmpty(configDir))
                    Directory.CreateDirectory(configDir); // no-op if exists

                // ~/.local/share/subs2srs/Logs/
                if (!string.IsNullOrEmpty(ConstantSettings.LogDir))
                    Directory.CreateDirectory(ConstantSettings.LogDir); // no-op if exists
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Warning: failed to initialize app directories: {ex.Message}");
            }
        }

        /// <summary>
        /// Execute an action on the GTK main thread, blocking the caller until done.
        /// If already on the main thread, runs synchronously to avoid deadlock.
        /// </summary>
        private static void InvokeOnMain(System.Action action)
        {
            if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
            {
                action();
                return;
            }
            var done = new ManualResetEventSlim(false);
            GLib.Functions.IdleAdd(0, () =>
            {
                action();
                done.Set();
                return false;
            });
            done.Wait();
        }

        /// <summary>
        /// Execute a function on the GTK main thread and return the result.
        /// Blocks the caller until the function completes.
        /// </summary>
        private static T InvokeOnMainWithResult<T>(Func<T> func)
        {
            if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
                return func();
            T result = default!;
            var done = new ManualResetEventSlim(false);
            GLib.Functions.IdleAdd(0, () =>
            {
                result = func();
                done.Set();
                return false;
            });
            done.Wait();
            return result;
        }

        /// <summary>
        /// Show a simple alert dialog (replaces GTK3 MessageDialog).
        /// GTK4 AlertDialog is async; we use a modal window with a label
        /// and OK button as a simple synchronous replacement.
        /// </summary>
        private static void ShowAlertDialog(string msg, string title)
        {
            // Get the active window from the application
            var parent = _app.GetActiveWindow();

            var dialog = Gtk.Window.New();
            dialog.SetTitle(title);
            dialog.SetDefaultSize(400, 150);
            dialog.SetModal(true);
            if (parent != null)
                dialog.SetTransientFor(parent);

            var box = Gtk.Box.New(Gtk.Orientation.Vertical, 12);
            box.SetMarginTop(20);
            box.SetMarginBottom(20);
            box.SetMarginStart(20);
            box.SetMarginEnd(20);

            var label = Gtk.Label.New(msg);
            label.SetWrap(true);
            label.SetHalign(Gtk.Align.Start);
            box.Append(label);

            var btn = Gtk.Button.NewWithLabel("OK");
            btn.SetHalign(Gtk.Align.Center);
            btn.OnClicked += (s, e) => dialog.Close();
            box.Append(btn);

            dialog.SetChild(box);
            dialog.Show();
        }

        /// <summary>
        /// Show a confirmation dialog with Yes/No buttons.
        /// Since GTK4 has no synchronous Dialog.Run(), we spin
        /// a nested GLib main loop until the user responds.
        /// </summary>
        private static bool ShowConfirmDialog(string msg, string title)
        {
            var parent = _app.GetActiveWindow();
            bool? result = null;

            var dialog = Gtk.Window.New();
            dialog.SetTitle(title);
            dialog.SetDefaultSize(400, 150);
            dialog.SetModal(true);
            if (parent != null)
                dialog.SetTransientFor(parent);

            var box = Gtk.Box.New(Gtk.Orientation.Vertical, 12);
            box.SetMarginTop(20);
            box.SetMarginBottom(20);
            box.SetMarginStart(20);
            box.SetMarginEnd(20);

            var label = Gtk.Label.New(msg);
            label.SetWrap(true);
            label.SetHalign(Gtk.Align.Start);
            box.Append(label);

            var btnBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 8);
            btnBox.SetHalign(Gtk.Align.Center);

            var btnYes = Gtk.Button.NewWithLabel("Yes");
            btnYes.OnClicked += (s, e) => { result = true; dialog.Close(); };
            btnBox.Append(btnYes);

            var btnNo = Gtk.Button.NewWithLabel("No");
            btnNo.OnClicked += (s, e) => { result = false; dialog.Close(); };
            btnBox.Append(btnNo);

            box.Append(btnBox);
            dialog.SetChild(box);

            // When the window is closed (by button or WM), quit the nested loop
            var loop = GLib.MainLoop.New(null, false);
            dialog.OnCloseRequest += (s, e) =>
            {
                if (result == null) result = false; // closed via WM button
                loop.Quit();
                return false; // allow default close
            };

            dialog.Show();

            // Spin a nested main loop — blocks until the dialog is closed.
            // This preserves the synchronous semantics that callers expect.
            loop.Run();

            return result ?? false;
        }
    }
}
