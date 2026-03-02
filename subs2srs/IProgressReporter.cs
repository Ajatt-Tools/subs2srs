using System;
using System.Diagnostics;

namespace subs2srs
{
    public interface IProgressReporter
    {
        bool Cancel { get; }
        int StepsTotal { get; set; }
        void NextStep(int step, string description);
        void UpdateProgress(int percent, string text);
        void UpdateProgress(string text);
        void EnableDetail(bool enable);
        void SetDuration(DateTime duration);
        void OnFFmpegOutput(object sender, DataReceivedEventArgs e);
    }

    public class DialogProgress
    {
        public static void updateProgressInvoke(IProgressReporter reporter, int percent, string text) => reporter?.UpdateProgress(percent, text);
        public static void updateProgressInvoke(IProgressReporter reporter, string text) => reporter?.UpdateProgress(text);
        public static void nextStepInvoke(IProgressReporter reporter, int step, string text) => reporter?.NextStep(step, text);
        public static bool getCancelInvoke(IProgressReporter reporter) => reporter != null && reporter.Cancel;
        public static void enableDetailInvoke(IProgressReporter reporter, bool enable) => reporter?.EnableDetail(enable);
        public static void setDuration(IProgressReporter reporter, DateTime duration) => reporter?.SetDuration(duration);
        public static DataReceivedEventHandler getFFmpegOutputHandler(IProgressReporter reporter) => (s, e) => reporter?.OnFFmpegOutput(s, e);
    }
}
