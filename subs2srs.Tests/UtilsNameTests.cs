//  Copyright (C) 2026 fkzys and contributors
//  SPDX-License-Identifier: GPL-3.0-or-later

using System;
using Xunit;

namespace subs2srs.Tests
{
  public class UtilsNameTests
  {
    private static UtilsName Make(
      int episodes = 10, int lines = 100,
      int w = 1920, int h = 1080, TimeSpan? last = null)
    {
      return new UtilsName("TestDeck", episodes, lines,
        last ?? TimeSpan.FromMinutes(30), w, h);
    }

    // ── string tokens ───────────────────────────────────────────────────

    [Fact]
    public void DeckName_Replaced()
    {
      string r = Make().createName("${deck_name}", 1, 1,
        TimeSpan.Zero, TimeSpan.Zero, "", "");
      Assert.Equal("TestDeck", r);
    }

    [Fact]
    public void SubsLines_Replaced()
    {
      string r = Make().createName("${subs1_line}|${subs2_line}", 1, 1,
        TimeSpan.Zero, TimeSpan.Zero, "hello", "world");
      Assert.Equal("hello|world", r);
    }

    [Fact]
    public void WidthHeight_Replaced()
    {
      string r = new UtilsName("D", 1, 1, TimeSpan.Zero, 640, 480)
        .createName("${width}x${height}", 1, 1,
          TimeSpan.Zero, TimeSpan.Zero, "", "");
      Assert.Equal("640x480", r);
    }

    // ── escape tokens ───────────────────────────────────────────────────

    [Fact]
    public void EscapeChars_Replaced()
    {
      string r = Make().createName("a${cr}b${lf}c${tab}d", 1, 1,
        TimeSpan.Zero, TimeSpan.Zero, "", "");
      Assert.Equal("a\rb\nc\td", r);
    }

    // ── number tokens — explicit zeroes ─────────────────────────────────

    [Fact]
    public void EpisodeNum_ExplicitZeroes()
    {
      string r = Make(episodes: 10).createName("ep${3:episode_num}", 5, 1,
        TimeSpan.Zero, TimeSpan.Zero, "", "");
      Assert.Equal("ep005", r);
    }

    // ── number tokens — auto zeroes (${0:…}) ────────────────────────────

    [Fact]
    public void EpisodeNum_AutoZeroes()
    {
      // 10 episodes → 2 digits
      string r = Make(episodes: 10).createName("ep${0:episode_num}", 5, 1,
        TimeSpan.Zero, TimeSpan.Zero, "", "");
      Assert.Equal("ep05", r);
    }

    [Fact]
    public void SequenceNum_AutoZeroes()
    {
      // 100 lines → 3 digits
      string r = Make(lines: 100).createName("seq${0:sequence_num}", 1, 7,
        TimeSpan.Zero, TimeSpan.Zero, "", "");
      Assert.Equal("seq007", r);
    }

    // ── number tokens — no zeroes prefix ────────────────────────────────

    [Fact]
    public void EpisodeNum_NoZeroes()
    {
      string r = Make().createName("ep${episode_num}", 5, 1,
        TimeSpan.Zero, TimeSpan.Zero, "", "");
      Assert.Equal("ep5", r);
    }

    // ── time tokens ─────────────────────────────────────────────────────

    [Fact]
    public void StartTimeTokens()
    {
      var start = new TimeSpan(0, 1, 23, 45, 678);
      string r = Make().createName(
        "${s_hour}h${s_min}m${s_sec}s${s_msec}ms", 1, 1,
        start, start, "", "");
      Assert.Equal("1h23m45s678ms", r);
    }

    [Fact]
    public void EndTimeTokens()
    {
      var end = new TimeSpan(0, 0, 5, 30, 100);
      string r = Make().createName("${e_min}:${e_sec}", 1, 1,
        TimeSpan.Zero, end, "", "");
      Assert.Equal("5:30", r);
    }

    [Fact]
    public void HsecToken_Centiseconds()
    {
      // 250 ms → 25 centiseconds
      var start = TimeSpan.FromMilliseconds(250);
      string r = Make().createName("${s_hsec}", 1, 1,
        start, start, "", "");
      Assert.Equal("25", r);
    }

    [Fact]
    public void TotalSecToken()
    {
      // 1 min 30 sec = 90 total seconds
      var start = TimeSpan.FromSeconds(90);
      string r = Make().createName("${s_total_sec}", 1, 1,
        start, start, "", "");
      Assert.Equal("90", r);
    }

    // ── plain literal (no tokens) ───────────────────────────────────────

    [Fact]
    public void NoTokens_Passthrough()
    {
      string r = Make().createName("plain.mp3", 1, 1,
        TimeSpan.Zero, TimeSpan.Zero, "", "");
      Assert.Equal("plain.mp3", r);
    }
  }
}
