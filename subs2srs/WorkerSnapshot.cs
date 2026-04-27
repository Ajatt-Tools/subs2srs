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
  /// Responsible for processing snapshots in the worker thread.
  /// </summary>
  public class WorkerSnapshot
  {
    /// <summary>
    /// Generate snapshots for all episodes.
    /// </summary>
    public bool genSnapshots(WorkerVars workerVars, IProgressReporter dialogProgress)
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

      // For each episode
      foreach (List<InfoCombined> combArray in workerVars.CombinedAll)
      {
        episodeCount++;
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

          InfoCombined comb = item.comb;
          TimeSpan startTime = comb.Subs1.StartTime;
          TimeSpan endTime = comb.Subs1.EndTime;
          TimeSpan midTime = UtilsSubs.getMidpointTime(startTime, endTime);

          string videoFileName = Settings.Instance.VideoClips.Files[epNum - 1];

          string nameStr = name.createName(ConstantSettings.SnapshotFilenameFormat,
            epNum + Settings.Instance.EpisodeStartNumber - 1,
            item.seqNum, startTime, endTime, comb.Subs1.Text, comb.Subs2.Text);

          string outFile = $"{workerVars.MediaDir}{Path.DirectorySeparatorChar}{nameStr}";

          if (!File.Exists(outFile))
          {
            string ext = Path.GetExtension(outFile);
            string tmpFile = Path.ChangeExtension(outFile, ".tmp" + ext);
            UtilsSnapshot.takeSnapshotFromVideo(videoFileName, midTime,
              Settings.Instance.Snapshots.Size,
              Settings.Instance.Snapshots.Crop,
              Settings.Instance.Snapshots.Quality,
              tmpFile);
            File.Move(tmpFile, outFile, overwrite: true);
          }

          int done = Interlocked.Increment(ref completed);
          int totalDone = baseCount + done;
          dialogProgress.UpdateProgress(
            Convert.ToInt32(totalDone * (100.0 / totalLines)),
            $"Generating snapshot: {totalDone} of {totalLines}");
        });

        progressCount += combArray.Count;

        if (cancelled)
          return false;
      }

      return true;
    }
  }
}
