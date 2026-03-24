//  Copyright (C) 2009-2016 Christopher Brochtrup
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
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace subs2srs
{
  /// <summary>
  /// Used for extract information from .mkv files.
  /// </summary>
  public class UtilsMkv
  {
    public enum TrackType
    {
      UNKNOWN,
      SUBTITLES,
      AUDIO
    }

    /// <summary>
    /// Get list of audio and subtitle tracks in the provided .mkv file.
    /// Handles both old and modern mkvinfo output formats by parsing
    /// tree depth instead of matching exact indentation prefixes.
    /// </summary>
    public static List<MkvTrack> getTrackList(string mkvFile)
    {
      List<MkvTrack> trackList = new List<MkvTrack>();

      if (Path.GetExtension(mkvFile).ToLowerInvariant() != ".mkv")
        return trackList;

      string args = $"\"{mkvFile}\"";
      string output = UtilsCommon.startProcessAndGetStdout(
        ConstantSettings.ExeMkvInfo, ConstantSettings.PathMkvInfoExeFull, args);

      if (output == "Error.")
        return trackList;

      output = output.Replace("\r", "");
      string[] lines = output.Split('\n');

      int segDepth = -1;
      bool inTracks = false;
      MkvTrack curTrack = null;

      foreach (string rawLine in lines)
      {
        // Extract depth (whitespace length before '+') and content from tree line
        var lineMatch = Regex.Match(rawLine, @"^\|(\s*)\+\s*(.*)$");
        if (!lineMatch.Success)
          continue;

        int depth = lineMatch.Groups[1].Value.Length;
        string content = lineMatch.Groups[2].Value.Trim();

        if (!inTracks)
        {
          if (Regex.IsMatch(content, @"^(Segment\s+)?Tracks$", RegexOptions.IgnoreCase))
          {
            segDepth = depth;
            inTracks = true;
          }
          continue;
        }

        // Past the tracks section — a sibling or ancestor of "Segment tracks"
        if (depth <= segDepth)
        {
          if (curTrack != null && IsValidTrack(curTrack))
            trackList.Add(curTrack);
          break;
        }

        // New track entry (one nesting level below "Segment tracks")
        if (depth == segDepth + 1)
        {
          if (curTrack != null && IsValidTrack(curTrack))
            trackList.Add(curTrack);

          curTrack = Regex.IsMatch(content, @"^(A\s+)?[Tt]rack\s*$")
            ? new MkvTrack()
            : null;
          continue;
        }

        if (curTrack == null)
          continue;

        // Track properties (deeper nesting)
        var m = Regex.Match(content,
          @"^Track number:.*\(track ID for mkvmerge & mkvextract:\s*(?<Num>\d+)\)");
        if (m.Success)
        {
          curTrack.TrackID = m.Groups["Num"].Value;
          continue;
        }

        m = Regex.Match(content, @"^Track type:\s*(?<Type>\w+)");
        if (m.Success)
        {
          string t = m.Groups["Type"].Value;
          if (t == "subtitles")
            curTrack.TrackType = TrackType.SUBTITLES;
          else if (t == "audio")
            curTrack.TrackType = TrackType.AUDIO;
          else
            curTrack = null; // video or other — skip
          continue;
        }

        m = Regex.Match(content, @"^Codec ID:\s*(?<Codec>.+)");
        if (m.Success)
        {
          string ext = MapCodecToExtension(m.Groups["Codec"].Value.Trim());
          if (ext != null)
            curTrack.Extension = ext;
          else
            curTrack = null; // unrecognized codec — skip
          continue;
        }

        // Match "Language:" but not "Language (IETF BCP 47):" to keep 3-letter codes
        m = Regex.Match(content, @"^Language:\s*(?<Lang>\S+)");
        if (m.Success)
        {
          curTrack.Lang = m.Groups["Lang"].Value;
          continue;
        }
      }

      // Handle last track when file ends without a following section
      if (curTrack != null && IsValidTrack(curTrack))
        trackList.Add(curTrack);

      return trackList;
    }


    private static bool IsValidTrack(MkvTrack track)
    {
      return track.TrackType != TrackType.UNKNOWN
        && !string.IsNullOrEmpty(track.Extension)
        && !string.IsNullOrEmpty(track.TrackID);
    }


    private static string MapCodecToExtension(string codecID)
    {
      // Subtitle codecs
      if (codecID == "S_VOBSUB") return "sub";
      if (codecID == "S_TEXT/UTF8") return "srt";
      if (codecID == "S_TEXT/ASS") return "ass";
      if (codecID == "S_TEXT/SSA") return "ssa";
      if (codecID == "S_HDMV/PGS") return "sup";

      // Audio codecs
      if (codecID == "A_MPEG/L3") return "mp3";
      if (codecID == "A_MPEG/L2") return "mp2";
      if (codecID == "A_MPEG/L1") return "mp1";
      if (codecID.StartsWith("A_PCM")) return "wav";
      if (codecID == "A_MPC") return "mpc";
      if (codecID.StartsWith("A_AC3")) return "ac3";
      if (codecID.StartsWith("A_EAC3")) return "eac3";
      if (codecID.StartsWith("A_ALAC")) return "m4a";
      if (codecID.StartsWith("A_DTS")) return "dts";
      if (codecID == "A_VORBIS") return "ogg";
      if (codecID == "A_OPUS") return "opus";
      if (codecID == "A_FLAC") return "flac";
      if (codecID.StartsWith("A_REAL")) return "rm";
      if (codecID.StartsWith("A_AAC")) return "aac";
      if (codecID.StartsWith("A_QUICKTIME")) return "aiff";
      if (codecID == "A_TTA1") return "tta";
      if (codecID == "A_WAVPACK4") return "wv";

      return null;
    }



    /// <summary>
    /// Get list of subtitle tracks in the provided .mkv file.
    /// </summary>
    public static List<MkvTrack> getSubtitleTrackList(string mkvFile)
    {
      List<MkvTrack> subtitleTrackList = new List<MkvTrack>();
      List<MkvTrack> allTrackList = UtilsMkv.getTrackList(mkvFile);

      foreach (MkvTrack track in allTrackList)
      {
        if (track.TrackType == TrackType.SUBTITLES)
        {
          subtitleTrackList.Add(track);
        }
      }

      return subtitleTrackList;
    }


    /// <summary>
    /// Get list of audio tracks in the provided .mkv file.
    /// </summary>
    public static List<MkvTrack> getAudioTrackList(string mkvFile)
    {
      List<MkvTrack> audioTrackList = new List<MkvTrack>();
      List<MkvTrack> allTrackList = UtilsMkv.getTrackList(mkvFile);

      foreach (MkvTrack track in allTrackList)
      {
        if (track.TrackType == TrackType.AUDIO)
        {
          audioTrackList.Add(track);
        }
      }

      return audioTrackList;
    }


    /// <summary>
    /// Extract track from the provided mvk file.
    /// </summary>
    public static void extractTrack(string mkvFile, string trackID, string outName)
    {
      string args = String.Format("tracks \"{0}\" {1}:\"{2}\"", mkvFile, trackID, outName);

      UtilsCommon.startProcess(ConstantSettings.ExeMkvExtract, ConstantSettings.PathMkvExtractExeFull, args);
    }
  }


  [Serializable]
  public class MkvTrack
  {
    public string TrackID { get; set; } // 1, 2, etc.
    public UtilsMkv.TrackType TrackType { get; set; }
    public string CodecID { get; set; } // S_TEXT/ASS, A_MPEG/L3, etc.
    public string Extension { get; set; } // ass, mp3, etc.
    public string Lang { get; set; } // eng, jpn, etc.

    public MkvTrack()
    {
      this.TrackID = "";
      this.TrackType = UtilsMkv.TrackType.UNKNOWN;
      this.CodecID = "";
      this.Extension = "";
      this.Lang = "";
    }


    public override string ToString()
    {
      string displayLang = UtilsLang.LangThreeLetter2Full(this.Lang);
      string displayedExtension = this.Extension.ToUpper();

      if ((this.Lang == "und") || (this.Lang == "") || (displayLang == ""))
      {
        displayLang = "Unknown Language";
      }

      if (this.TrackType == UtilsMkv.TrackType.SUBTITLES)
      {
        if (displayedExtension == "SUB")
        {
          displayedExtension = "VOBSUB";
        }
      }

      return String.Format("{0} - {1} ({2})",
        this.TrackID, displayedExtension, displayLang);
    }
  }
}
