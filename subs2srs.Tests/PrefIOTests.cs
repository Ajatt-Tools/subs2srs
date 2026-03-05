//  Copyright (C) 2026 fkzys
//  SPDX-License-Identifier: GPL-3.0-or-later

using Xunit;

namespace subs2srs.Tests
{
  public class PrefIOTests
  {
    // ── convertFromTokens ───────────────────────────────────────────────

    [Theory]
    [InlineData("${tab}", "\t")]
    [InlineData("${cr}", "\r")]
    [InlineData("${lf}", "\n")]
    public void ConvertFromTokens_SingleToken(string input, string expected)
    {
      Assert.Equal(expected, PrefIO.convertFromTokens(input));
    }

    [Fact]
    public void ConvertFromTokens_AllTokens()
    {
      Assert.Equal("a\tb\r\nc",
        PrefIO.convertFromTokens("a${tab}b${cr}${lf}c"));
    }

    [Fact]
    public void ConvertFromTokens_NoTokens_Passthrough()
    {
      Assert.Equal("plain text", PrefIO.convertFromTokens("plain text"));
    }

    [Fact]
    public void ConvertFromTokens_EmptyString()
    {
      Assert.Equal("", PrefIO.convertFromTokens(""));
    }

    [Fact]
    public void ConvertFromTokens_MultipleOfSameToken()
    {
      Assert.Equal("\t\t", PrefIO.convertFromTokens("${tab}${tab}"));
    }
  }
}
