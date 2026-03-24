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

        [Fact]
        public void ConstantSettings_DefaultSnapshotJpegQuality_MatchesPrefDefault()
        {
            // Ensure fresh Prefs so other tests don't interfere
            var saved = ConstantSettings.Prefs;
            try
            {
                ConstantSettings.Prefs = new PreferencesData();
                Assert.Equal(
                    PrefDefaults.DefaultSnapshotJpegQuality,
                    ConstantSettings.DefaultSnapshotJpegQuality);
            }
            finally
            {
                ConstantSettings.Prefs = saved;
            }
        }
    }
}
