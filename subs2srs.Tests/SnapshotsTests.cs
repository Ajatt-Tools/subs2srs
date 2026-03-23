//  Copyright (C) 2026 fkzys
//  SPDX-License-Identifier: GPL-3.0-or-later

using Xunit;

namespace subs2srs.Tests
{
  public class SnapshotsTests
  {
    [Fact]
    public void Quality_DefaultValue_Is3()
    {
      var snap = new Snapshots();
      Assert.Equal(3, snap.Quality);
    }

    [Fact]
    public void Quality_SetAndGet_RoundTrips()
    {
      var snap = new Snapshots();
      snap.Quality = 15;
      Assert.Equal(15, snap.Quality);
    }

    [Fact]
    public void Quality_BoundaryMin()
    {
      var snap = new Snapshots();
      snap.Quality = 1;
      Assert.Equal(1, snap.Quality);
    }

    [Fact]
    public void Quality_BoundaryMax()
    {
      var snap = new Snapshots();
      snap.Quality = 31;
      Assert.Equal(31, snap.Quality);
    }
  }
}
