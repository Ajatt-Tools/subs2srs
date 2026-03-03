// See https://aka.ms/new-console-template for more information
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

            var win = new MainWindow();
            win.ShowAll();

            Application.Run();
        }

        /// <summary>
        /// Create XDG directories and default preferences file on first run.
        /// Must be called before Logger or PrefIO are accessed.
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

                // Generate default preferences.txt if missing
                if (!File.Exists(ConstantSettings.SettingsFilename))
                    PrefIO.writeDefaultPreferences();
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
