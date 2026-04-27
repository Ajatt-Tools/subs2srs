//  Copyright (C) 2009-2016 Christopher Brochtrup
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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace subs2srs
{
  /// <summary>
  /// Responsible for processing video in the worker thread.
  /// </summary>
  public class WorkerVideo
  {
    /// <summary>
    /// Generate video clips for all episodes.
    /// </summary>
    public bool genVideoClip(WorkerVars workerVars, IProgressReporter dialogProgress)
    {
      int progressCount = 0;
      int episodeCount = 0;
      int totalEpisodes = workerVars.CombinedAll.Count;
      int totalLines = UtilsSubs.getTotalLineCount(workerVars.CombinedAll);
      TimeSpan lastTime = UtilsSubs.getLastTime(workerVars.CombinedAll);

      UtilsName name = new UtilsName(Settings.Instance.DeckName, totalEpisodes,
        totalLines, lastTime, Settings.Instance.VideoClips.Size.Width, Settings.Instance.VideoClips.Size.Height);

      var parallelOptions = new ParallelOptions
      {
        MaxDegreeOfParallelism = ConstantSettings.EffectiveParallelism
      };

      dialogProgress.UpdateProgress(0, "Creating video clips.");

      string videoExtension = Settings.Instance.VideoClips.IPodSupport ? ".mp4" : ".avi";

      // For each episode
      foreach (List<InfoCombined> combArray in workerVars.CombinedAll)
      {
        episodeCount++;

        if (combArray.Count == 0)
        {
          continue;
        }

        TimeSpan entireClipStartTime = combArray[0].Subs1.StartTime;
        TimeSpan entireClipEndTime = combArray[combArray.Count - 1].Subs1.EndTime;

        // Apply pad to entire clip timings (if requested)
        if (Settings.Instance.VideoClips.PadEnabled)
        {
          entireClipStartTime = UtilsSubs.applyTimePad(entireClipStartTime, -Settings.Instance.VideoClips.PadStart);
          entireClipEndTime = UtilsSubs.applyTimePad(entireClipEndTime, Settings.Instance.VideoClips.PadEnd);
        }

        // Skip entire episode (including expensive video conversion) if all clips already exist
        if (checkAllVideoClipsExist(combArray, name, episodeCount, progressCount, workerVars.MediaDir, videoExtension))
        {
          progressCount += combArray.Count;
          continue;
        }

        string progressText = $"Converting video file {episodeCount} of {totalEpisodes}";
        dialogProgress.UpdateProgress(progressText);

        dialogProgress.EnableDetail(true);

        TimeSpan entireClipDuration = UtilsSubs.getDurationTime(entireClipStartTime, entireClipEndTime);
        dialogProgress.SetDuration(entireClipDuration);

        string tempVideoFilename = Path.GetTempPath() + ConstantSettings.TempVideoFilename + videoExtension;

        if (Settings.Instance.VideoClips.IPodSupport)
        {
          UtilsVideo.convertVideo(Settings.Instance.VideoClips.Files[episodeCount - 1],
            Settings.Instance.VideoClips.AudioStream.Num,
            entireClipStartTime, entireClipEndTime,
            Settings.Instance.VideoClips.Size, Settings.Instance.VideoClips.Crop,
            Settings.Instance.VideoClips.BitrateVideo, Settings.Instance.VideoClips.BitrateAudio,
            UtilsVideo.VideoCodec.h264, UtilsVideo.AudioCodec.AAC,
            UtilsVideo.Profilex264.IPod640, UtilsVideo.Presetx264.SuperFast,
            tempVideoFilename, dialogProgress);
        }
        else
        {
          UtilsVideo.convertVideo(Settings.Instance.VideoClips.Files[episodeCount - 1],
            Settings.Instance.VideoClips.AudioStream.Num,
            entireClipStartTime, entireClipEndTime,
            Settings.Instance.VideoClips.Size, Settings.Instance.VideoClips.Crop,
            Settings.Instance.VideoClips.BitrateVideo, Settings.Instance.VideoClips.BitrateAudio,
            UtilsVideo.VideoCodec.MPEG4, UtilsVideo.AudioCodec.MP3,
            UtilsVideo.Profilex264.None, UtilsVideo.Presetx264.None,
            tempVideoFilename, dialogProgress);
        }

        dialogProgress.EnableDetail(false);

        int epNum = episodeCount; // capture for lambda
        int baseCount = progressCount;

        // Pre-compute work items with fixed sequence numbers
        var workItems = new List<(int seqNum, InfoCombined comb)>(combArray.Count);
        for (int i = 0; i < combArray.Count; i++)
        {
          workItems.Add((baseCount + i + 1, combArray[i]));
        }

        int completed = 0;
        bool cancelled = false;

        Parallel.ForEach(workItems, parallelOptions, (item, state) =>
        {
          if (dialogProgress.Cancel) { cancelled = true; state.Stop(); return; }

          TimeSpan startTime = UtilsSubs.shiftTiming(item.comb.Subs1.StartTime, -((int)entireClipStartTime.TotalMilliseconds));
          TimeSpan endTime = UtilsSubs.shiftTiming(item.comb.Subs1.EndTime, -((int)entireClipStartTime.TotalMilliseconds));

          TimeSpan filenameStartTime = item.comb.Subs1.StartTime;
          TimeSpan filenameEndTime = item.comb.Subs1.EndTime;

          if (Settings.Instance.VideoClips.PadEnabled)
          {
            startTime = UtilsSubs.applyTimePad(startTime, -Settings.Instance.VideoClips.PadStart);
            endTime = UtilsSubs.applyTimePad(endTime, Settings.Instance.VideoClips.PadEnd);
            filenameStartTime = UtilsSubs.applyTimePad(item.comb.Subs1.StartTime, -Settings.Instance.VideoClips.PadStart);
            filenameEndTime = UtilsSubs.applyTimePad(item.comb.Subs1.EndTime, Settings.Instance.VideoClips.PadEnd);
          }

          string nameStr = name.createName(ConstantSettings.VideoFilenameFormat,
            epNum + Settings.Instance.EpisodeStartNumber - 1,
            item.seqNum, filenameStartTime, filenameEndTime, item.comb.Subs1.Text, item.comb.Subs2.Text);

          string outFile = $"{workerVars.MediaDir}{Path.DirectorySeparatorChar}{nameStr}{videoExtension}";

          if (!File.Exists(outFile))
          {
            string ext = Path.GetExtension(outFile);
            string tmpFile = Path.ChangeExtension(outFile, ".tmp" + ext);
            UtilsVideo.cutVideo(tempVideoFilename, startTime, endTime, tmpFile);
            File.Move(tmpFile, outFile, overwrite: true);
          }

          int done = Interlocked.Increment(ref completed);
          int totalDone = baseCount + done;
          dialogProgress.UpdateProgress(
            Convert.ToInt32(totalDone * (100.0 / totalLines)),
            $"Generating video clip: {totalDone} of {totalLines}");
        });

        progressCount += combArray.Count;

        File.Delete(tempVideoFilename);

        if (cancelled)
          return false;
      }

      return true;
    }


    /// <summary>
    /// Check if all video clips for an episode already exist.
    /// Used to skip the expensive video conversion step on resume.
    /// </summary>
    private bool checkAllVideoClipsExist(List<InfoCombined> combArray, UtilsName name,
      int episodeCount, int progressCountBase, string mediaDir, string videoExtension)
    {
      int tempCount = progressCountBase;

      foreach (InfoCombined comb in combArray)
      {
        tempCount++;

        TimeSpan filenameStartTime = comb.Subs1.StartTime;
        TimeSpan filenameEndTime = comb.Subs1.EndTime;

        if (Settings.Instance.VideoClips.PadEnabled)
        {
          filenameStartTime = UtilsSubs.applyTimePad(filenameStartTime, -Settings.Instance.VideoClips.PadStart);
          filenameEndTime = UtilsSubs.applyTimePad(filenameEndTime, Settings.Instance.VideoClips.PadEnd);
        }

        string nameStr = name.createName(ConstantSettings.VideoFilenameFormat,
          episodeCount + Settings.Instance.EpisodeStartNumber - 1,
          tempCount, filenameStartTime, filenameEndTime, comb.Subs1.Text, comb.Subs2.Text);

        string outFile = $"{mediaDir}{Path.DirectorySeparatorChar}{nameStr}{videoExtension}";

        if (!File.Exists(outFile))
        {
          return false;
        }
      }

      return true;
    }
  }
}
