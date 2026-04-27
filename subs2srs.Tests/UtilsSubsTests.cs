//  Copyright (C) 2026 fkzys and contributors
//  SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using Xunit;

namespace subs2srs.Tests
{
  public class UtilsSubsTests
  {
    // ── applyTimePad ────────────────────────────────────────────────────

    [Fact]
    public void ApplyTimePad_PositivePad_AddsMilliseconds()
    {
      var result = UtilsSubs.applyTimePad(TimeSpan.FromSeconds(10), 500);
      Assert.Equal(TimeSpan.FromMilliseconds(10_500), result);
    }

    [Fact]
    public void ApplyTimePad_NegativePad_SubtractsMilliseconds()
    {
      var result = UtilsSubs.applyTimePad(TimeSpan.FromSeconds(10), -3000);
      Assert.Equal(TimeSpan.FromSeconds(7), result);
    }

    [Fact]
    public void ApplyTimePad_NegativeBeyondZero_ClampsToZero()
    {
      var result = UtilsSubs.applyTimePad(TimeSpan.FromSeconds(2), -5000);
      Assert.Equal(TimeSpan.Zero, result);
    }

    [Fact]
    public void ApplyTimePad_ZeroPad_Unchanged()
    {
      var time = TimeSpan.FromSeconds(5);
      Assert.Equal(time, UtilsSubs.applyTimePad(time, 0));
    }

    // ── shiftTiming ─────────────────────────────────────────────────────

    [Fact]
    public void ShiftTiming_MatchesApplyTimePad()
    {
      var time = TimeSpan.FromSeconds(10);
      Assert.Equal(
        UtilsSubs.applyTimePad(time, 200),
        UtilsSubs.shiftTiming(time, 200));
    }

    // ── getOverlap ──────────────────────────────────────────────────────

    [Fact]
    public void GetOverlap_PerfectOverlap_ReturnsOne()
    {
      var s = TimeSpan.FromSeconds(0);
      var e = TimeSpan.FromSeconds(10);
      Assert.Equal(1.0, UtilsSubs.getOverlap(s, e, s, e));
    }

    [Fact]
    public void GetOverlap_HalfOverlap()
    {
      Assert.Equal(0.5, UtilsSubs.getOverlap(
        TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15)));
    }

    [Fact]
    public void GetOverlap_NoOverlap_Negative()
    {
      double overlap = UtilsSubs.getOverlap(
        TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30));
      Assert.True(overlap < 0);
    }

    // ── getMidpointTime ─────────────────────────────────────────────────

    [Fact]
    public void GetMidpointTime_ReturnsMiddle()
    {
      Assert.Equal(TimeSpan.FromSeconds(15),
        UtilsSubs.getMidpointTime(
          TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20)));
    }

    [Fact]
    public void GetMidpointTime_EqualTimes_ReturnsSame()
    {
      var t = TimeSpan.FromSeconds(5);
      Assert.Equal(t, UtilsSubs.getMidpointTime(t, t));
    }

    // ── getDurationTime ─────────────────────────────────────────────────

    [Fact]
    public void GetDurationTime_NormalRange()
    {
      Assert.Equal(TimeSpan.FromSeconds(15),
        UtilsSubs.getDurationTime(
          TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(25)));
    }

    [Fact]
    public void GetDurationTime_EndBeforeStart_Zero()
    {
      Assert.Equal(TimeSpan.Zero,
        UtilsSubs.getDurationTime(
          TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(10)));
    }

    // ── stringToTime ────────────────────────────────────────────────────

    [Theory]
    [InlineData("0:00:00", 0, 0, 0)]
    [InlineData("1:30:45", 1, 30, 45)]
    [InlineData("9:59:59", 9, 59, 59)]
    public void StringToTime_ValidInput(string input, int h, int m, int s)
    {
      Assert.Equal(new TimeSpan(h, m, s), UtilsSubs.stringToTime(input));
    }

    [Theory]
    [InlineData("10:00:00")]   // two-digit hour
    [InlineData("0:60:00")]    // minutes > 59
    [InlineData("not a time")]
    [InlineData("")]
    public void StringToTime_InvalidInput_Throws(string input)
    {
      Assert.Throws<Exception>(() => UtilsSubs.stringToTime(input));
    }

    // ── timeToString (bug-fix: trailing dots) ───────────────────────────

    [Theory]
    [InlineData(0, 0, 0, "0:00:00")]
    [InlineData(0, 1, 30, "0:01:30")]
    [InlineData(2, 15, 45, "2:15:45")]
    public void TimeToString_CorrectFormat(int h, int m, int s, string expected)
    {
      Assert.Equal(expected, UtilsSubs.timeToString(new TimeSpan(h, m, s)));
    }

    [Fact]
    public void TimeToString_NoTrailingDots()
    {
      string result = UtilsSubs.timeToString(new TimeSpan(0, 5, 30));
      Assert.DoesNotContain(".", result);
    }

    [Theory]
    [InlineData("0:00:00")]
    [InlineData("1:30:45")]
    [InlineData("9:59:59")]
    public void TimeToString_RoundTrips_WithStringToTime(string timeStr)
    {
      Assert.Equal(timeStr,
        UtilsSubs.timeToString(UtilsSubs.stringToTime(timeStr)));
    }

    // ── formatAssTime (bug-fix: trailing dots) ──────────────────────────

    [Fact]
    public void FormatAssTime_Zero()
    {
      Assert.Equal("0:00:00.00", UtilsSubs.formatAssTime(TimeSpan.Zero));
    }

    [Fact]
    public void FormatAssTime_TypicalTime()
    {
      // 36s 160ms → .16 centiseconds
      var time = new TimeSpan(0, 0, 0, 36, 160);
      Assert.Equal("0:00:36.16", UtilsSubs.formatAssTime(time));
    }

    [Fact]
    public void FormatAssTime_WithHours()
    {
      // 1h 2m 3s 40ms → .04 centiseconds
      var time = new TimeSpan(0, 1, 2, 3, 40);
      Assert.Equal("1:02:03.04", UtilsSubs.formatAssTime(time));
    }

    [Fact]
    public void FormatAssTime_ExactlyOneDot()
    {
      var time = new TimeSpan(0, 0, 5, 30, 0);
      string result = UtilsSubs.formatAssTime(time);

      // Exactly one '.' — before centiseconds
      int dots = result.Split('.').Length - 1;
      Assert.Equal(1, dots);
    }

    [Fact]
    public void FormatAssTime_NeverEndsWithDot()
    {
      var time = new TimeSpan(0, 0, 0, 10, 500);
      Assert.False(UtilsSubs.formatAssTime(time).EndsWith('.'));
    }

    // ── getLastTime ─────────────────────────────────────────────────────

    [Fact]
    public void GetLastTime_ReturnsMaxEndTime()
    {
      var combinedAll = new List<List<InfoCombined>>
      {
        new()
        {
          new InfoCombined(
            new InfoLine(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5), "a"),
            new InfoLine()),
          new InfoCombined(
            new InfoLine(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20), "b"),
            new InfoLine()),
        },
        new()
        {
          new InfoCombined(
            new InfoLine(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(15), "c"),
            new InfoLine()),
        },
      };

      Assert.Equal(TimeSpan.FromSeconds(20), UtilsSubs.getLastTime(combinedAll));
    }

    [Fact]
    public void GetLastTime_Empty_ReturnsZero()
    {
      Assert.Equal(TimeSpan.Zero,
        UtilsSubs.getLastTime(new List<List<InfoCombined>>()));
    }

    // ── getTotalLineCount ───────────────────────────────────────────────

    [Fact]
    public void GetTotalLineCount_SumsAcrossEpisodes()
    {
      var all = new List<List<InfoCombined>>
      {
        new() { new InfoCombined(), new InfoCombined() },
        new() { new InfoCombined() },
      };
      Assert.Equal(3, UtilsSubs.getTotalLineCount(all));
    }

    [Fact]
    public void GetTotalLineCount_Empty_Zero()
    {
      Assert.Equal(0,
        UtilsSubs.getTotalLineCount(new List<List<InfoCombined>>()));
    }
  }
}
