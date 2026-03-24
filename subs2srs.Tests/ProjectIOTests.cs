using System;
using System.IO;
using Xunit;

namespace subs2srs.Tests
{
    public class ProjectIOTests
    {
        [Fact]
        public void SaveLoad_RoundTrip_PreservesSettings()
        {
            var path = Path.GetTempFileName() + ".s2s.json";
            try
            {
                Settings.Instance.Reset();
                Settings.Instance.DeckName = "test_deck";
                Settings.Instance.OutputDir = "/tmp/output";
                Settings.Instance.EpisodeStartNumber = 5;
                Settings.Instance.Subs[0].FilePattern = "/path/to/*.srt";
                Settings.Instance.AudioClips.Bitrate = 192;
                Settings.Instance.Snapshots.Quality = 5;
                Settings.Instance.VideoClips.BitrateVideo = 1200;

                ProjectIO.Save(path, Settings.Instance);

                // Reset everything, then reload
                Settings.Instance.Reset();
                Assert.Equal("", Settings.Instance.DeckName);

                ProjectIO.Load(path);

                Assert.Equal("test_deck", Settings.Instance.DeckName);
                Assert.Equal("/tmp/output", Settings.Instance.OutputDir);
                Assert.Equal(5, Settings.Instance.EpisodeStartNumber);
                Assert.Equal("/path/to/*.srt", Settings.Instance.Subs[0].FilePattern);
                Assert.Equal(192, Settings.Instance.AudioClips.Bitrate);
                Assert.Equal(5, Settings.Instance.Snapshots.Quality);
                Assert.Equal(1200, Settings.Instance.VideoClips.BitrateVideo);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void Load_NonexistentFile_ThrowsAndSettingsUnchanged()
        {
            Settings.Instance.Reset();
            Settings.Instance.DeckName = "before";

            Assert.ThrowsAny<IOException>(()
                => ProjectIO.Load("/nonexistent/path.s2s.json"));

            // Settings must not have changed
            Assert.Equal("before", Settings.Instance.DeckName);
        }

        [Fact]
        public void Load_CorruptJson_ThrowsAndSettingsUnchanged()
        {
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, "not valid json {{{");
                Settings.Instance.Reset();
                Settings.Instance.DeckName = "before";

                Assert.ThrowsAny<Exception>(() => ProjectIO.Load(path));

                Assert.Equal("before", Settings.Instance.DeckName);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void DeckName_Transformation_PreservedThroughRoundTrip()
        {
            var path = Path.GetTempFileName() + ".s2s.json";
            try
            {
                Settings.Instance.Reset();
                Settings.Instance.DeckName = "  My Deck Name  ";
                Assert.Equal("My_Deck_Name", Settings.Instance.DeckName);

                ProjectIO.Save(path, Settings.Instance);
                Settings.Instance.Reset();
                ProjectIO.Load(path);

                Assert.Equal("My_Deck_Name", Settings.Instance.DeckName);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void FilesArrays_NotSerialized_FilePatternIs()
        {
            var path = Path.GetTempFileName() + ".s2s.json";
            try
            {
                Settings.Instance.Reset();
                Settings.Instance.Subs[0].Files = new[] { "a.srt", "b.srt" };
                Settings.Instance.Subs[0].FilePattern = "*.srt";

                ProjectIO.Save(path, Settings.Instance);

                var json = File.ReadAllText(path);
                Assert.DoesNotContain("a.srt", json);
                Assert.Contains("*.srt", json);

                ProjectIO.Load(path);
                Assert.Empty(Settings.Instance.Subs[0].Files);
                Assert.Equal("*.srt", Settings.Instance.Subs[0].FilePattern);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void EmptyProject_SavesAndLoads()
        {
            var path = Path.GetTempFileName() + ".s2s.json";
            try
            {
                // Save a fully default (empty) project
                Settings.Instance.Reset();
                Assert.Equal("", Settings.Instance.DeckName);
                ProjectIO.Save(path, Settings.Instance);

                // Dirty the singleton, then reload the empty project
                Settings.Instance.DeckName = "dirty";
                Assert.Equal("dirty", Settings.Instance.DeckName);

                ProjectIO.Load(path);

                // DeckName should be restored to the empty default
                Assert.Equal("", Settings.Instance.DeckName);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void ProjectPath_NotSerialized()
        {
            var path = Path.GetTempFileName() + ".s2s.json";
            try
            {
                Settings.Instance.Reset();
                Settings.Instance.ProjectPath = "/some/path.s2s.json";

                ProjectIO.Save(path, Settings.Instance);

                var json = File.ReadAllText(path);
                Assert.DoesNotContain("projectPath", json, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void TimeShiftRules_PreservedThroughRoundTrip()
        {
            var path = Path.GetTempFileName() + ".s2s.json";
            try
            {
                Settings.Instance.Reset();
                Settings.Instance.Subs[0].TimeShiftRules.Add(new TimeShiftRule(1, 100));
                Settings.Instance.Subs[0].TimeShiftRules.Add(new TimeShiftRule(5, -200));
                Settings.Instance.Subs[1].TimeShiftRules.Add(new TimeShiftRule(1, 50));

                ProjectIO.Save(path, Settings.Instance);
                Settings.Instance.Reset();
                Assert.Empty(Settings.Instance.Subs[0].TimeShiftRules);

                ProjectIO.Load(path);

                Assert.Equal(2, Settings.Instance.Subs[0].TimeShiftRules.Count);
                Assert.Equal(1, Settings.Instance.Subs[0].TimeShiftRules[0].FromEpisode);
                Assert.Equal(100, Settings.Instance.Subs[0].TimeShiftRules[0].ShiftMs);
                Assert.Equal(5, Settings.Instance.Subs[0].TimeShiftRules[1].FromEpisode);
                Assert.Equal(-200, Settings.Instance.Subs[0].TimeShiftRules[1].ShiftMs);
                Assert.Single(Settings.Instance.Subs[1].TimeShiftRules);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
