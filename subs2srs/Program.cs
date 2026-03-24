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
﻿// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Threading;
using Gtk;
using Action = System.Action;

namespace subs2srs
{
    class Program
    {
        private static int _mainThreadId;

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                string msg = ex?.ToString() ?? e.ExceptionObject.ToString();
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

            UtilsMsg.OnShowError = (msg, title) =>
                InvokeOnMain(() => ShowMessageImpl(MessageType.Error, msg, title));
            UtilsMsg.OnShowInfo = (msg, title) =>
                InvokeOnMain(() => ShowMessageImpl(MessageType.Info, msg, title));
            UtilsMsg.OnShowConfirm = (msg, title) =>
                InvokeOnMainWithResult(() => ShowConfirmImpl(msg, title));

            Application.Init();

            SynchronizationContext.SetSynchronizationContext(new GtkSynchronizationContext());

            // Suppress GtkSharp toggle_ref warnings at the GLib writer level
            GLibLogFilter.Install();

            var win = new MainWindow();

            win.ShowAll();

            Application.Run();
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
                string configDir = Path.GetDirectoryName(ConstantSettings.SettingsFilename);
                if (!string.IsNullOrEmpty(configDir))
                    Directory.CreateDirectory(configDir); // no-op if exists

                // ~/.local/share/subs2srs/Logs/
                if (!string.IsNullOrEmpty(ConstantSettings.LogDir))
                    Directory.CreateDirectory(ConstantSettings.LogDir); // no-op if exists
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Warning: failed to initialize app directories: {ex.Message}");
            }
        }

        private static void InvokeOnMain(Action action)
        {
            if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
            {
                action();
                return;
            }
            var done = new ManualResetEventSlim(false);
            Application.Invoke((s, e) => { action(); done.Set(); });
            done.Wait();
        }

        private static T InvokeOnMainWithResult<T>(Func<T> func)
        {
            if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
                return func();
            T result = default!;
            var done = new ManualResetEventSlim(false);
            Application.Invoke((s, e) => { result = func(); done.Set(); });
            done.Wait();
            return result;
        }

        private static void ShowMessageImpl(MessageType type, string msg, string title)
        {
            var dialog = new MessageDialog(null, DialogFlags.Modal, type, ButtonsType.Ok,
                false, "%s", "");
            dialog.Title = title;
            dialog.Text = title;
            dialog.SecondaryText = msg;
            dialog.Run();
            dialog.Destroy();
        }

        private static bool ShowConfirmImpl(string msg, string title)
        {
            var dialog = new MessageDialog(null, DialogFlags.Modal,
                MessageType.Question, ButtonsType.YesNo, false, "%s", "");
            dialog.Title = title;
            dialog.Text = title;
            dialog.SecondaryText = msg;
            var response = (ResponseType)dialog.Run();
            dialog.Destroy();
            return response == ResponseType.Yes;
        }
    }
}
