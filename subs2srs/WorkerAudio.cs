//  Copyright (C) 2009-2016 Christopher Brochtrup
//  Copyright (C) 2026 fkzys (GTK4/.NET 10 port)
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
  /// Responsible for processing audio in the worker thread.
  /// </summary>
  public class WorkerAudio
  {
    /// <summary>
    /// Generate Audio clips for all episodes.
    /// </summary>
    public bool genAudioClip(WorkerVars workerVars, IProgressReporter dialogProgress)
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

      dialogProgress.UpdateProgress(0, "Creating audio clips.");

      // For each episode
      foreach (List<InfoCombined> combArray in workerVars.CombinedAll)
      {
        episodeCount++;

        // It is possible for all lines in an episode to be set to inactive
        if (combArray.Count == 0)
        {
          continue;
        }

        // Is the audio input an mp3 file?
        bool inputFileIsMp3 = (Settings.Instance.AudioClips.Files.Length > 0)
          && (Path.GetExtension(Settings.Instance.AudioClips.Files[episodeCount - 1]).ToLower() == ".mp3");

        TimeSpan entireClipStartTime = combArray[0].Subs1.StartTime;
        TimeSpan entireClipEndTime = combArray[combArray.Count - 1].Subs1.EndTime;

        // Temp file for demuxed audio (stream copy, no re-encode)
        string tempDemuxFile = Path.Combine(Path.GetTempPath(),
          $"subs2srs_demux_{episodeCount}.mka");

        // Temp file for decoded PCM audio (sample-accurate seeking)
        string tempWavFile = Path.Combine(Path.GetTempPath(),
          $"subs2srs_decoded_{episodeCount}.wav");

        // Apply pad to entire clip timings (if requested)
        if (Settings.Instance.AudioClips.PadEnabled)
        {
          entireClipStartTime = UtilsSubs.applyTimePad(entireClipStartTime, -Settings.Instance.AudioClips.PadStart);
          entireClipEndTime = UtilsSubs.applyTimePad(entireClipEndTime, Settings.Instance.AudioClips.PadEnd);
        }

        // Skip entire episode (including expensive extraction) if all clips already exist
        if (checkAllAudioClipsExist(combArray, name, episodeCount, progressCount, workerVars.MediaDir))
        {
          progressCount += combArray.Count;
          continue;
        }

        // Extraction strategy: copy-demux (fast) when source needs extraction,
        // direct mp3 cut (copy) when source is already mp3
        bool useDemuxEncode = false;

        if (Settings.Instance.AudioClips.UseAudioFromVideo)
        {
          dialogProgress.UpdateProgress(
            $"Demuxing audio from video {episodeCount} of {totalEpisodes}");

          string streamNum = Settings.Instance.VideoClips.AudioStream?.Num ?? "";
          if (streamNum.Length == 0 || streamNum == "-" || !streamNum.Contains(":"))
            streamNum = "0:a:0";

          UtilsAudio.demuxAudioCopy(
            Settings.Instance.VideoClips.Files[episodeCount - 1],
            streamNum,
            entireClipStartTime,
            entireClipEndTime,
            tempDemuxFile);

          if (!File.Exists(tempDemuxFile) || new FileInfo(tempDemuxFile).Length == 0)
          {
            if (dialogProgress.Cancel)
              return false;

            UtilsMsg.showErrMsg("Failed to extract the audio from the video.\n" +
                                "Make sure that the video does not have any DRM restrictions.");
            return false;
          }

          useDemuxEncode = true;
        }
        else if (ConstantSettings.ReencodeBeforeSplittingAudio || !inputFileIsMp3)
        {
          dialogProgress.UpdateProgress(
            $"Demuxing audio file {episodeCount} of {totalEpisodes}");

          UtilsAudio.demuxAudioCopy(
            Settings.Instance.AudioClips.Files[episodeCount - 1],
            "0",
            entireClipStartTime,
            entireClipEndTime,
            tempDemuxFile);

          if (!File.Exists(tempDemuxFile) || new FileInfo(tempDemuxFile).Length == 0)
          {
            if (dialogProgress.Cancel)
              return false;

            UtilsMsg.showErrMsg("Failed to demux the audio file.\n" +
                                "Make sure that the audio file does not have any DRM restrictions.");
            return false;
          }

          useDemuxEncode = true;
        }

        // Phase 2: decode demuxed audio to WAV (PCM).
        // WAV seeking is byte-offset based = sample-accurate (±0.02ms).
        // This lets parallel phase 3 do frame-accurate cuts via re-encode,
        // where "decoding" WAV is just reading bytes (near-zero CPU cost).
        if (useDemuxEncode)
        {
          TimeSpan demuxDuration = entireClipEndTime - entireClipStartTime;

          dialogProgress.UpdateProgress(
            $"Decoding audio for episode {episodeCount} of {totalEpisodes}");
          dialogProgress.EnableDetail(true);
          dialogProgress.SetDuration(demuxDuration);

          // temp.mka already starts at 0:00, decode the whole file to WAV
          UtilsAudio.decodeToWav(tempDemuxFile, tempWavFile, dialogProgress);

          dialogProgress.EnableDetail(false);

          // Demuxed container no longer needed
          try { File.Delete(tempDemuxFile); } catch { }

          if (!File.Exists(tempWavFile) || new FileInfo(tempWavFile).Length == 0)
          {
            if (dialogProgress.Cancel)
              return false;

            UtilsMsg.showErrMsg("Failed to decode the audio.\n" +
                                "Make sure the source file is not corrupted.");
            return false;
          }
        }

        // Source file: decoded WAV (starts from 0, needs time shift)
        // or original mp3 file (absolute timings, no shift)
        bool needsShift = useDemuxEncode;
        string fileToCut = useDemuxEncode
          ? tempWavFile
          : Settings.Instance.AudioClips.Files[episodeCount - 1];

        int epNum = episodeCount; // capture for lambda
        int baseCount = progressCount;
        int audioBitrate = Settings.Instance.AudioClips.Bitrate;

        // Pre-compute work items with fixed sequence numbers
        var workItems = new List<(int seqNum, int epLineNum, InfoCombined comb)>(combArray.Count);
        for (int i = 0; i < combArray.Count; i++)
        {
          workItems.Add((baseCount + i + 1, i + 1, combArray[i]));
        }

        int completed = 0;
        bool cancelled = false;

        Parallel.ForEach(workItems, parallelOptions, (item, state) =>
        {
          if (dialogProgress.Cancel) { cancelled = true; state.Stop(); return; }

          TimeSpan startTime = item.comb.Subs1.StartTime;
          TimeSpan endTime = item.comb.Subs1.EndTime;
          TimeSpan filenameStartTime = item.comb.Subs1.StartTime;
          TimeSpan filenameEndTime = item.comb.Subs1.EndTime;

          if (needsShift)
          {
            startTime = UtilsSubs.shiftTiming(startTime, -((int)entireClipStartTime.TotalMilliseconds));
            endTime = UtilsSubs.shiftTiming(endTime, -((int)entireClipStartTime.TotalMilliseconds));
          }

          // Apply pad (if requested)
          if (Settings.Instance.AudioClips.PadEnabled)
          {
            startTime = UtilsSubs.applyTimePad(startTime, -Settings.Instance.AudioClips.PadStart);
            endTime = UtilsSubs.applyTimePad(endTime, Settings.Instance.AudioClips.PadEnd);
            filenameStartTime = UtilsSubs.applyTimePad(item.comb.Subs1.StartTime, -Settings.Instance.AudioClips.PadStart);
            filenameEndTime = UtilsSubs.applyTimePad(item.comb.Subs1.EndTime, Settings.Instance.AudioClips.PadEnd);
          }

          string lyricSubs2 = "";

          if (Settings.Instance.Subs[1].Files.Length != 0)
          {
            lyricSubs2 = item.comb.Subs2.Text.Trim();
          }

          string nameStr = name.createName(ConstantSettings.AudioFilenameFormat,
            epNum + Settings.Instance.EpisodeStartNumber - 1,
            item.seqNum, filenameStartTime, filenameEndTime, item.comb.Subs1.Text, lyricSubs2);

          string outName = $"{workerVars.MediaDir}{Path.DirectorySeparatorChar}{nameStr}";

          if (!File.Exists(outName))
          {
            // Write to temp file, then atomic rename
            string ext = Path.GetExtension(outName);
            string tmpName = Path.ChangeExtension(outName, ".tmp" + ext);

            try
            {
              // WAV source: cut + encode to mp3 (decode WAV = read bytes, ~0 CPU).
              // Seeking in WAV is byte-offset based = sample-accurate (±0.02ms).
              // MP3 source: stream copy (no re-encode, ±13ms frame boundary accuracy).
              if (useDemuxEncode)
                UtilsAudio.cutAndEncodeAudio(fileToCut, startTime, endTime, audioBitrate, tmpName);
              else
                UtilsAudio.cutAudio(fileToCut, startTime, endTime, tmpName);

              if (File.Exists(tmpName))
              {
                File.Move(tmpName, outName, overwrite: true);

                this.tagAudio(name, outName, epNum, item.epLineNum, item.seqNum, combArray.Count,
                  filenameStartTime, filenameEndTime, item.comb.Subs1.Text, lyricSubs2);
              }
            }
            catch (Exception)
            {
              // Clean up partial temp file
              try { if (File.Exists(tmpName)) File.Delete(tmpName); } catch { }

              // If source file was deleted (cancellation race), treat as cancel
              if (!File.Exists(fileToCut))
              {
                cancelled = true;
                state.Stop();
                return;
              }
              throw;
            }
          }

          int done = Interlocked.Increment(ref completed);
          int totalDone = baseCount + done;
          dialogProgress.UpdateProgress(
            Convert.ToInt32(totalDone * (100.0 / totalLines)),
            $"Generating audio clip: {totalDone} of {totalLines}");
        });

        progressCount += combArray.Count;

        // Delete temp files after Parallel.ForEach has fully completed
        try { File.Delete(tempDemuxFile); } catch { }
        try { File.Delete(tempWavFile); } catch { }

        if (cancelled)
          return false;
      }

      // Normalize all audio files in the media directory
      if (Settings.Instance.AudioClips.Normalize)
      {
        dialogProgress.UpdateProgress(-1, "Normalizing audio...");
        UtilsAudio.normalizeAudio(workerVars.MediaDir);
      }

      return true;
    }


    /// <summary>
    /// Check if all audio clips for an episode already exist.
    /// Used to skip the expensive audio extraction step on resume.
    /// </summary>
    private bool checkAllAudioClipsExist(List<InfoCombined> combArray, UtilsName name,
      int episodeCount, int progressCountBase, string mediaDir)
    {
      int tempCount = progressCountBase;

      foreach (InfoCombined comb in combArray)
      {
        tempCount++;

        TimeSpan filenameStartTime = comb.Subs1.StartTime;
        TimeSpan filenameEndTime = comb.Subs1.EndTime;

        if (Settings.Instance.AudioClips.PadEnabled)
        {
          filenameStartTime = UtilsSubs.applyTimePad(filenameStartTime, -Settings.Instance.AudioClips.PadStart);
          filenameEndTime = UtilsSubs.applyTimePad(filenameEndTime, Settings.Instance.AudioClips.PadEnd);
        }

        string lyricSubs2 = "";
        if (Settings.Instance.Subs[1].Files.Length != 0)
        {
          lyricSubs2 = comb.Subs2.Text.Trim();
        }

        string nameStr = name.createName(ConstantSettings.AudioFilenameFormat,
          episodeCount + Settings.Instance.EpisodeStartNumber - 1,
          tempCount, filenameStartTime, filenameEndTime, comb.Subs1.Text, lyricSubs2);

        string outName = $"{mediaDir}{Path.DirectorySeparatorChar}{nameStr}";

        if (!File.Exists(outName))
        {
          return false;
        }
      }

      return true;
    }


    /// <summary>
    /// Apply tag to audio file.
    /// </summary>
    private void tagAudio(UtilsName name, string outName, int episodeCount, int curEpisodeCount, int progressCount, int totalTracks,
      TimeSpan filenameStartTime, TimeSpan filenameEndTime, string lyricSubs1, string lyricSubs2)
    {
      int episodeNum = episodeCount + Settings.Instance.EpisodeStartNumber - 1;

      string tagArtist = name.createName(ConstantSettings.AudioId3Artist, episodeNum,
        progressCount, filenameStartTime, filenameEndTime, lyricSubs1, lyricSubs2);

      string tagAlbum = name.createName(ConstantSettings.AudioId3Album, episodeNum,
        progressCount, filenameStartTime, filenameEndTime, lyricSubs1, lyricSubs2);

      string tagTitle = name.createName(ConstantSettings.AudioId3Title, episodeNum,
        progressCount, filenameStartTime, filenameEndTime, lyricSubs1, lyricSubs2);

      string tagGenre = name.createName(ConstantSettings.AudioId3Genre, episodeNum,
        progressCount, filenameStartTime, filenameEndTime, lyricSubs1, lyricSubs2);

      string tagLyrics = name.createName(ConstantSettings.AudioId3Lyrics, episodeNum,
        progressCount, filenameStartTime, filenameEndTime, lyricSubs1, lyricSubs2);

      UtilsAudio.tagAudio(outName,
        tagArtist,
        tagAlbum,
        tagTitle,
        tagGenre,
        tagLyrics,
        curEpisodeCount,
        totalTracks);
    }


    /// <summary>
    /// Convert audio or video to mp3 and display progress dialog.
    /// </summary>
    private bool convertToMp3(string file, string stream, string progressText, IProgressReporter dialogProgress,
      TimeSpan entireClipStartTime, TimeSpan entireClipEndTime, string tempMp3Filename)
    {
      TimeSpan entireClipDuration = UtilsSubs.getDurationTime(entireClipStartTime, entireClipEndTime);

      dialogProgress.UpdateProgress(progressText);
      dialogProgress.EnableDetail(true);
      dialogProgress.SetDuration(entireClipDuration);

      UtilsAudio.ripAudioFromVideo(file,
        stream,
        entireClipStartTime, entireClipEndTime,
        Settings.Instance.AudioClips.Bitrate, tempMp3Filename, dialogProgress);

      dialogProgress.EnableDetail(false);

      FileInfo fileInfo = new FileInfo(tempMp3Filename);
      return File.Exists(tempMp3Filename) && fileInfo.Length > 0;
    }
  }
}
