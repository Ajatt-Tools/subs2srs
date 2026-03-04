//  Copyright (C) 2009-2016 Christopher Brochtrup
//  Copyright (C) 2026 fkzys (GTK3/.NET 10 port)
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace subs2srs
{
  /// <summary>
  /// General utilities.
  /// </summary>
  public class UtilsCommon
  {
    /// <summary>
    /// Check if an integer is within its valid range. If it isn't, set to a default.
    /// </summary>
    public static int checkRange(int value, int min, int max, int def)
    {
      return (value >= min && value <= max) ? value : def;
    }


    /// <summary>
    /// If value is in set of valid values, use it. Otherwise, set to the default.
    /// </summary>
    public static T checkRangeInSet<T>(T value, List<T> validValues, T def)
    {
      return validValues.Contains(value) ? value : def;
    }


    /// <summary>
    /// Get the directory that the executable resides in.
    /// </summary>
    public static string getAppDir(bool addSlash)
    {
      string appDir = AppContext.BaseDirectory;
      if (!addSlash)
        appDir = appDir.TrimEnd(Path.DirectorySeparatorChar);
      return appDir;
    }


    /// <summary>
    /// Get the multiple nearest to the provided value.
    /// </summary>
    public static int getNearestMultiple(int value, int multiple)
    {
      int remainder = value % multiple;

      if (remainder == 0)
        return value;
      if (remainder < multiple / 2)
        return value - remainder;
      return value + multiple - remainder;
    }


    /// <summary>
    /// Get a list of non-hidden files in a directory that match the given file pattern.
    /// </summary>
    public static string[] getNonHiddenFilesInDir(string dir, string searchPattern)
    {
      if (!Directory.Exists(dir))
        return Array.Empty<string>();

      List<string> files = Directory.GetFiles(dir, searchPattern, SearchOption.TopDirectoryOnly).ToList();
      files.Sort();
      return files.Where(f => (File.GetAttributes(f) & FileAttributes.Hidden) == 0).ToArray();
    }


    /// <summary>
    /// Get a list of non-hidden files in a directory.
    /// </summary>
    public static string[] getNonHiddenFilesInDir(string dir)
    {
      return getNonHiddenFilesInDir(dir, "*");
    }


    /// <summary>
    /// Get the non-hidden files based on the provided file pattern.
    /// File pattern can be the full path to a dir or it can be a dir + wildcard (D:\temp\*.mp3).
    /// </summary>
    public static string[] getNonHiddenFiles(string filePattern)
    {
      if (filePattern.Length == 0)
        return Array.Empty<string>();

      string dir = Path.GetDirectoryName(filePattern);

      if (!Directory.Exists(dir))
        return Array.Empty<string>();

      List<string> allFiles = Directory.GetFiles(Path.GetFullPath(dir), Path.GetFileName(filePattern)).ToList();
      allFiles.Sort();
      return allFiles.Where(f => (File.GetAttributes(f) & FileAttributes.Hidden) == 0).ToArray();
    }


    /// <summary>
    /// Return string containing each element of provided list separated by semicolons.
    /// </summary>
    public static string makeSemiString(string[] words)
    {
      return string.Join(";", words.Select(w => w.Trim()));
    }


    /// <summary>
    /// Trim spaces from words in provided list.
    /// </summary>
    public static string[] removeExtraSpaces(string[] words)
    {
      for (int i = 0; i < words.Length; i++)
        words[i] = words[i].Trim();
      return words;
    }


    // ── Exe resolution ──────────────────────────────────────────────────

    /// <summary>
    /// Get the list of exe paths to try, in order: relative, absolute, bare name (after PATH fix).
    /// </summary>
    private static IEnumerable<string> getExePaths(string relPath, string fullPath)
    {
      yield return relPath;
      yield return fullPath;

      // Ensure directory of fullPath is in PATH, then try bare filename
      string dir = Path.GetDirectoryName(fullPath);
      if (!string.IsNullOrEmpty(dir))
      {
        string oldPath = Environment.GetEnvironmentVariable("PATH") ?? "";
        var pathDirs = new HashSet<string>(oldPath.Split(Path.PathSeparator));
        if (!pathDirs.Contains(dir))
          Environment.SetEnvironmentVariable("PATH", oldPath + Path.PathSeparator + dir);
      }

      yield return Path.GetFileName(fullPath);
    }

    private static IEnumerable<string> getFFmpegPaths()
    {
      return getExePaths(ConstantSettings.PathFFmpegExe, ConstantSettings.PathFFmpegFullExe);
    }


    // ── Simple process launching (blocking, no progress) ────────────────

    /// <summary>
    /// Try to call an exe with provided arguments. Returns true on success.
    /// </summary>
    private static bool callExe(string exe, string args, bool useShellExecute, bool createNoWindow)
    {
      try
      {
        using var process = new Process();
        process.StartInfo.FileName = exe;
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = useShellExecute;
        process.StartInfo.CreateNoWindow = createNoWindow;
        if (!useShellExecute)
        {
          process.StartInfo.RedirectStandardError = true;
          process.StartInfo.RedirectStandardOutput = true;
          process.ErrorDataReceived += (s, e) => { };
          process.OutputDataReceived += (s, e) => { };
        }
        process.Start();
        if (!useShellExecute)
        {
          process.BeginErrorReadLine();
          process.BeginOutputReadLine();
        }
        process.WaitForExit();
        return true;
      }
      catch
      {
        return false;
      }
    }


    /// <summary>
    /// Try to call an exe and return stdout. Returns "Error." on failure.
    /// </summary>
    private static string callExeAndGetStdout(string exe, string args)
    {
      try
      {
        using var process = new Process();
        process.StartInfo.FileName = exe;
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        return process.StandardOutput.ReadToEnd();
      }
      catch
      {
        return null;
      }
    }


    /// <summary>
    /// Try to call an exe and return stderr. Returns null on failure.
    /// </summary>
    private static string callExeAndGetStderr(string exe, string args)
    {
      try
      {
        using var process = new Process();
        process.StartInfo.FileName = exe;
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();
        return process.StandardError.ReadToEnd();
      }
      catch
      {
        return null;
      }
    }


    /// <summary>
    /// Run a process with progress monitoring and cancel support.
    /// Returns true if process completed (not cancelled).
    /// </summary>
    private static bool runProcessWithProgress(string exe, string args, IProgressReporter dialogProgress)
    {
      try
      {
        using var process = new Process();
        process.StartInfo.FileName = exe;
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardError = true;
        process.ErrorDataReceived += new DataReceivedEventHandler(dialogProgress.OnFFmpegOutput);
        process.Start();
        process.BeginErrorReadLine();

        while (!process.HasExited)
        {
          Thread.Sleep(100);

          if (dialogProgress.Cancel)
          {
            try { process.Kill(); } catch { }
            return true; // started OK, just cancelled
          }
        }

        return true;
      }
      catch
      {
        return false;
      }
    }


    // ── Public API ──────────────────────────────────────────────────────

    /// <summary>
    /// Call an exe with the provided arguments, trying multiple paths.
    /// </summary>
    public static void startProcess(string relExePath, string fullExePath, string args,
      bool useShellExecute, bool createNoWindow)
    {
      foreach (string exe in getExePaths(relExePath, fullExePath))
      {
        if (callExe(exe, args, useShellExecute, createNoWindow))
          return;
      }
    }


    /// <summary>
    /// Call an exe with the provided arguments. Don't open a window.
    /// </summary>
    public static void startProcess(string relExePath, string fullExePath, string args)
    {
      startProcess(relExePath, fullExePath, args, false, true);
    }


    /// <summary>
    /// Call an exe with the provided arguments. Return stdout.
    /// </summary>
    public static string startProcessAndGetStdout(string relExePath, string fullExePath, string args)
    {
      foreach (string exe in getExePaths(relExePath, fullExePath))
      {
        string result = callExeAndGetStdout(exe, args);
        if (result != null)
          return result;
      }

      return "Error.";
    }


    /// <summary>
    /// Call ffmpeg with provided arguments. Blocking.
    /// </summary>
    public static void startFFmpeg(string ffmpegArgs, bool useShellExecute, bool createNoWindow)
    {
      startProcess(ConstantSettings.PathFFmpegExe, ConstantSettings.PathFFmpegFullExe,
        ffmpegArgs, useShellExecute, createNoWindow);
    }


    /// <summary>
    /// Call ffmpeg with provided arguments and update the progress dialog.
    /// </summary>
    public static void startFFmpegProgress(string ffmpegArgs, IProgressReporter dialogProgress)
    {
      foreach (string exe in getFFmpegPaths())
      {
        if (runProcessWithProgress(exe, ffmpegArgs, dialogProgress))
          return;
      }
    }


    /// <summary>
    /// Call ffmpeg with the provided arguments. Return the ffmpeg console text (stderr).
    /// </summary>
    public static string getFFmpegText(string ffmpegArgs)
    {
      foreach (string exe in getFFmpegPaths())
      {
        string result = callExeAndGetStderr(exe, ffmpegArgs);
        if (result != null)
          return result;
      }

      return "";
    }
  }
}
