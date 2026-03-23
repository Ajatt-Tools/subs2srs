//  Copyright (C) 2026 fkzys
//  SPDX-License-Identifier: GPL-3.0-or-later

using Xunit;

namespace subs2srs.Tests
{
  public class PrefDefaultsTests
  {
    [Fact]
    public void DefaultSnapshotJpegQuality_Is3()
    {
      Assert.Equal(3, PrefDefaults.DefaultSnapshotJpegQuality);
    }

    // Sanity: the mutable copy in ConstantSettings starts with the same default
    [Fact]
    public void ConstantSettings_DefaultSnapshotJpegQuality_MatchesPrefDefault()
    {
      Assert.Equal(PrefDefaults.DefaultSnapshotJpegQuality,
        ConstantSettings.DefaultSnapshotJpegQuality);
    }
  }
}
