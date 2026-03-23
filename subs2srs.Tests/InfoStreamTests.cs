//  Copyright (C) 2026 fkzys
//  SPDX-License-Identifier: GPL-3.0-or-later

using Xunit;

namespace subs2srs.Tests
{
  public class InfoStreamTests
  {
    // ── ToString — default stream ────────────────────────────────────────

    [Fact]
    public void ToString_DefaultConstructor_ReturnsDefault()
    {
      var s = new InfoStream();
      Assert.Equal("(Default)", s.ToString());
    }

    [Fact]
    public void ToString_NumIsDash_ReturnsDefault()
    {
      var s = new InfoStream("-", "0", "Japanese", "aac");
      Assert.Equal("(Default)", s.ToString());
    }

    // ── ToString — normal stream without title ───────────────────────────

    [Fact]
    public void ToString_WithLang_NoTitle_ShowsParenthesizedLang()
    {
      var s = new InfoStream("0:1", "0", "Japanese", "aac");
      Assert.Equal("0 — (Japanese)", s.ToString());
    }

    [Fact]
    public void ToString_EmptyTitle_TreatedAsNoTitle()
    {
      var s = new InfoStream("0:1", "1", "English", "mp3");
      s.Title = "";
      Assert.Equal("1 — (English)", s.ToString());
    }

    [Fact]
    public void ToString_WhitespaceTitle_TreatedAsNoTitle()
    {
      var s = new InfoStream("0:2", "2", "French", "flac");
      s.Title = "   ";
      Assert.Equal("2 — (French)", s.ToString());
    }

    // ── ToString — normal stream with title ──────────────────────────────

    [Fact]
    public void ToString_WithTitle_ShowsQuotedTitle()
    {
      var s = new InfoStream("0:1", "0", "Japanese", "aac");
      s.Title = "Commentary";
      Assert.Equal("0 — Japanese — \"Commentary\"", s.ToString());
    }

    [Fact]
    public void ToString_WithTitle_LangNotParenthesized()
    {
      // When title is present, lang is shown without parentheses
      var s = new InfoStream("0:3", "3", "English", "aac");
      s.Title = "Original Soundtrack";
      string result = s.ToString();
      Assert.Contains("English", result);
      Assert.DoesNotContain("(English)", result);
    }

    // ── ToString — empty / missing lang ──────────────────────────────────

    [Fact]
    public void ToString_EmptyLang_ShowsQuestionMarks()
    {
      var s = new InfoStream("0:1", "0", "", "aac");
      Assert.Equal("0 — (???)", s.ToString());
    }

    [Fact]
    public void ToString_WhitespaceLang_ShowsQuestionMarks()
    {
      var s = new InfoStream("0:1", "0", "   ", "aac");
      Assert.Equal("0 — (???)", s.ToString());
    }

    [Fact]
    public void ToString_EmptyLang_WithTitle_ShowsQuestionMarksAndTitle()
    {
      var s = new InfoStream("0:1", "0", "", "aac");
      s.Title = "Director Cut";
      Assert.Equal("0 — ??? — \"Director Cut\"", s.ToString());
    }

    // ── Title property defaults ──────────────────────────────────────────

    [Fact]
    public void DefaultConstructor_TitleIsEmptyString()
    {
      var s = new InfoStream();
      Assert.Equal("", s.Title);
    }

    [Fact]
    public void FourArgConstructor_TitleIsEmptyString()
    {
      var s = new InfoStream("0:1", "0", "eng", "aac");
      Assert.Equal("", s.Title);
    }

    // ── Title property set/get ───────────────────────────────────────────

    [Fact]
    public void Title_SetAndGet_RoundTrips()
    {
      var s = new InfoStream();
      s.Title = "My Track";
      Assert.Equal("My Track", s.Title);
    }
  }
}
