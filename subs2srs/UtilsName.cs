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
using System.Text.RegularExpressions;

namespace subs2srs
{
  /// <summary>
  /// Utilities related to creation of filenames for output files based on tokens.
  /// </summary>
  public class UtilsName
  {
    private readonly string deckName;
    private readonly int totalNumEpisodes;
    private readonly TimeSpan lastTime;
    private readonly int width;
    private readonly int height;
    private readonly int vobsubStreamNum;

    public int TotalNumLines { get; set; }

    private static readonly Regex NumberTokenRegex = new(
      @"\$\{((\d*):)?(" +
      "s_hour|s_min|s_sec|s_hsec|s_msec|s_total_hour|s_total_min|s_total_sec|s_total_hsec|s_total_msec|" +
      "e_hour|e_min|e_sec|e_hsec|e_msec|e_total_hour|e_total_min|e_total_sec|e_total_hsec|e_total_msec|" +
      "d_hour|d_min|d_sec|d_hsec|d_msec|d_total_hour|d_total_min|d_total_sec|d_total_hsec|d_total_msec|" +
      "m_hour|m_min|m_sec|m_hsec|m_msec|m_total_hour|m_total_min|m_total_sec|m_total_hsec|m_total_msec|" +
      "episode_num|sequence_num|total_line_num|vobsub_stream_num" +
      @")\}",
      RegexOptions.Compiled);

    public UtilsName(string deckName, int totalNumEpisodes, int totalNumLines,
      TimeSpan lastTime, int width, int height)
    {
      this.deckName = deckName;
      this.totalNumEpisodes = totalNumEpisodes;
      this.TotalNumLines = totalNumLines;
      this.lastTime = lastTime;
      this.width = width;
      this.height = height;
      this.vobsubStreamNum = 0;
    }

    public UtilsName(string deckName, int totalNumEpisodes, int totalNumLines,
      TimeSpan lastTime, int width, int height, int vobsubStreamNum)
    {
      this.deckName = deckName;
      this.totalNumEpisodes = totalNumEpisodes;
      this.TotalNumLines = totalNumLines;
      this.lastTime = lastTime;
      this.width = width;
      this.height = height;
      this.vobsubStreamNum = vobsubStreamNum;
    }


    /// <summary>
    /// If the provided integer were converted to a string, how many characters would it use?
    /// </summary>
    private static int getMaxNecessaryLeadingZeroes(int num)
    {
      return num.ToString().Length;
    }


    /// <summary>
    /// Format tokens related to numbers that can have leading zeroes.
    /// All per-call state is passed explicitly — safe for concurrent use.
    /// </summary>
    private string formatNumberTokens(Match match, int episodeNum, int sequenceNum,
      TimeSpan startTime, TimeSpan endTime)
    {
      int numZeroes = 0;
      bool numZeroesGiven = false;
      string token = "";
      string zeroString = "";
      int value = 0;
      TimeSpan diffTime = UtilsSubs.getDurationTime(startTime, endTime);
      TimeSpan midTime = UtilsSubs.getMidpointTime(startTime, endTime);
      int totalNumLines = TotalNumLines;

      // Get number of leading zeroes (if any)
      if (match.Groups[2].Success)
      {
        numZeroes = int.Parse(match.Groups[2].ToString());
        numZeroesGiven = true;
      }

      // Get the token
      if (match.Groups[3].Success)
      {
        token = match.Groups[3].ToString();
      }

      if (numZeroesGiven)
      {
        // Zero is special: use minimum necessary leading zeroes based on some maximum
        if (numZeroes == 0)
        {
          // Start times
          if (token == "s_hour") numZeroes = 1;
          else if (token == "s_min") numZeroes = getMaxNecessaryLeadingZeroes(lastTime.Minutes);
          else if (token == "s_sec") numZeroes = 2;
          else if (token == "s_hsec") numZeroes = 2;
          else if (token == "s_msec") numZeroes = 3;
          else if (token == "s_total_hour") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalHours);
          else if (token == "s_total_min") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalMinutes);
          else if (token == "s_total_sec") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalSeconds);
          else if (token == "s_total_hsec") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalMilliseconds / 10);
          else if (token == "s_total_msec") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalMilliseconds);
          // End times
          else if (token == "e_hour") numZeroes = 1;
          else if (token == "e_min") numZeroes = getMaxNecessaryLeadingZeroes(lastTime.Minutes);
          else if (token == "e_sec") numZeroes = 2;
          else if (token == "e_hsec") numZeroes = 2;
          else if (token == "e_msec") numZeroes = 3;
          else if (token == "e_total_hour") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalHours);
          else if (token == "e_total_min") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalMinutes);
          else if (token == "e_total_sec") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalSeconds);
          else if (token == "e_total_hsec") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalMilliseconds / 10);
          else if (token == "e_total_msec") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalMilliseconds);
          // Duration times
          else if (token == "d_hour") numZeroes = 1;
          else if (token == "d_min") numZeroes = getMaxNecessaryLeadingZeroes(lastTime.Minutes);
          else if (token == "d_sec") numZeroes = 2;
          else if (token == "d_hsec") numZeroes = 2;
          else if (token == "d_msec") numZeroes = 3;
          else if (token == "d_total_hour") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalHours);
          else if (token == "d_total_min") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalMinutes);
          else if (token == "d_total_sec") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalSeconds);
          else if (token == "d_total_hsec") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalMilliseconds / 10);
          else if (token == "d_total_msec") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalMilliseconds);
          // Middle times
          else if (token == "m_hour") numZeroes = 1;
          else if (token == "m_min") numZeroes = getMaxNecessaryLeadingZeroes(lastTime.Minutes);
          else if (token == "m_sec") numZeroes = 2;
          else if (token == "m_hsec") numZeroes = 2;
          else if (token == "m_msec") numZeroes = 3;
          else if (token == "m_total_hour") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalHours);
          else if (token == "m_total_min") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalMinutes);
          else if (token == "m_total_sec") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalSeconds);
          else if (token == "m_total_hsec") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalMilliseconds / 10);
          else if (token == "m_total_msec") numZeroes = getMaxNecessaryLeadingZeroes((int)lastTime.TotalMilliseconds);
          // The rest
          else if (token == "episode_num") numZeroes = getMaxNecessaryLeadingZeroes(totalNumEpisodes);
          else if (token == "sequence_num") numZeroes = getMaxNecessaryLeadingZeroes(totalNumLines);
          else if (token == "total_line_num") numZeroes = getMaxNecessaryLeadingZeroes(totalNumLines);
          else if (token == "vobsub_stream_num") numZeroes = 1;
        }

        if (numZeroes > 9)
        {
          numZeroes = 9;
        }

        if (numZeroes > 0)
        {
          zeroString += ":";
        }

        for (int i = 0; i < numZeroes; i++)
        {
          zeroString += "0";
        }
      }

      string formatString = "{0" + zeroString + "}";

      // Start times
      if (token == "s_hour") value = startTime.Hours;
      else if (token == "s_min") value = startTime.Minutes;
      else if (token == "s_sec") value = startTime.Seconds;
      else if (token == "s_hsec") value = startTime.Milliseconds / 10;
      else if (token == "s_msec") value = startTime.Milliseconds;
      else if (token == "s_total_hour") value = (int)startTime.TotalHours;
      else if (token == "s_total_min") value = (int)startTime.TotalMinutes;
      else if (token == "s_total_sec") value = (int)startTime.TotalSeconds;
      else if (token == "s_total_hsec") value = (int)startTime.TotalMilliseconds / 10;
      else if (token == "s_total_msec") value = (int)startTime.TotalMilliseconds;
      // End times
      else if (token == "e_hour") value = endTime.Hours;
      else if (token == "e_min") value = endTime.Minutes;
      else if (token == "e_sec") value = endTime.Seconds;
      else if (token == "e_hsec") value = endTime.Milliseconds / 10;
      else if (token == "e_msec") value = endTime.Milliseconds;
      else if (token == "e_total_hour") value = (int)endTime.TotalHours;
      else if (token == "e_total_min") value = (int)endTime.TotalMinutes;
      else if (token == "e_total_sec") value = (int)endTime.TotalSeconds;
      else if (token == "e_total_hsec") value = (int)endTime.TotalMilliseconds / 10;
      else if (token == "e_total_msec") value = (int)endTime.TotalMilliseconds;
      // Duration times
      else if (token == "d_hour") value = diffTime.Hours;
      else if (token == "d_min") value = diffTime.Minutes;
      else if (token == "d_sec") value = diffTime.Seconds;
      else if (token == "d_hsec") value = diffTime.Milliseconds / 10;
      else if (token == "d_msec") value = diffTime.Milliseconds;
      else if (token == "d_total_hour") value = (int)diffTime.TotalHours;
      else if (token == "d_total_min") value = (int)diffTime.TotalMinutes;
      else if (token == "d_total_sec") value = (int)diffTime.TotalSeconds;
      else if (token == "d_total_hsec") value = (int)diffTime.TotalMilliseconds / 10;
      else if (token == "d_total_msec") value = (int)diffTime.TotalMilliseconds;
      // Middle times
      else if (token == "m_hour") value = midTime.Hours;
      else if (token == "m_min") value = midTime.Minutes;
      else if (token == "m_sec") value = midTime.Seconds;
      else if (token == "m_hsec") value = midTime.Milliseconds / 10;
      else if (token == "m_msec") value = midTime.Milliseconds;
      else if (token == "m_total_hour") value = (int)midTime.TotalHours;
      else if (token == "m_total_min") value = (int)midTime.TotalMinutes;
      else if (token == "m_total_sec") value = (int)midTime.TotalSeconds;
      else if (token == "m_total_hsec") value = (int)midTime.TotalMilliseconds / 10;
      else if (token == "m_total_msec") value = (int)midTime.TotalMilliseconds;
      // The rest
      else if (token == "episode_num") value = episodeNum;
      else if (token == "sequence_num") value = sequenceNum;
      else if (token == "total_line_num") value = totalNumLines;
      else if (token == "vobsub_stream_num") value = vobsubStreamNum;

      return string.Format(formatString, value);
    }


    /// <summary>
    /// Create a filename based on the provided format.
    /// Thread-safe: all per-call state is captured in the lambda closure.
    /// </summary>
    public string createName(string format, int episodeNum, int sequenceNum,
      TimeSpan startTime, TimeSpan endTime, string subs1Text, string subs2Text)
    {
      string finalName = NumberTokenRegex.Replace(format,
        match => formatNumberTokens(match, episodeNum, sequenceNum, startTime, endTime));

      if (width > -1)
      {
        finalName = finalName.Replace("${width}", width.ToString());
        finalName = finalName.Replace("${height}", height.ToString());
      }

      // Replace tokens related to strings
      finalName = finalName.Replace("${deck_name}", deckName);
      finalName = finalName.Replace("${subs1_line}", subs1Text);
      finalName = finalName.Replace("${subs2_line}", subs2Text);

      // Escape chars
      finalName = finalName.Replace("${cr}", "\r");
      finalName = finalName.Replace("${lf}", "\n");
      finalName = finalName.Replace("${tab}", "\t");

      return finalName;
    }
  }
}
