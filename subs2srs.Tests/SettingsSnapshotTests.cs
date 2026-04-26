//  Copyright (C) 2026 fkzys
//  SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using Xunit;

namespace subs2srs.Tests
{
    public class SettingsSnapshotTests : IDisposable
    {
        // Reset singleton to a known state before each test
        // to avoid pollution from other tests in the sequential run.
        public SettingsSnapshotTests()
        {
            Settings.Instance.reset();
        }

        public void Dispose()
        {
            Settings.Instance.reset();
        }
        // ── Snapshot produces independent deep copy ─────────────────────────

        [Fact]
        public void Snapshot_IsDeepCopy_NotSameReference()
        {
            var snapshot = Settings.Instance.Snapshot();
            Assert.NotSame(Settings.Instance, snapshot);
            Assert.NotSame(Settings.Instance.Subs, snapshot.Subs);
            Assert.NotSame(Settings.Instance.Subs[0], snapshot.Subs[0]);
            Assert.NotSame(Settings.Instance.AudioClips, snapshot.AudioClips);
            Assert.NotSame(Settings.Instance.VideoClips, snapshot.VideoClips);
            Assert.NotSame(Settings.Instance.Snapshots, snapshot.Snapshots);
        }

        // ── Snapshot preserves values ───────────────────────────────────────

        [Fact]
        public void Snapshot_PreservesScalarValues()
        {
            Settings.Instance.DeckName = "TestDeck";
            Settings.Instance.EpisodeStartNumber = 42;
            Settings.Instance.SpanEnabled = true;
            Settings.Instance.SpanStart = TimeSpan.FromSeconds(90);

            var snapshot = Settings.Instance.Snapshot();

            Assert.Equal("TestDeck", snapshot.DeckName);
            Assert.Equal(42, snapshot.EpisodeStartNumber);
            Assert.True(snapshot.SpanEnabled);
            Assert.Equal(TimeSpan.FromSeconds(90), snapshot.SpanStart);
        }

        // ── Mutation after snapshot does not affect snapshot ─────────────────

        [Fact]
        public void Snapshot_IsolatedFromOriginal()
        {
            Settings.Instance.DeckName = "Before";
            var snapshot = Settings.Instance.Snapshot();

            Settings.Instance.DeckName = "After";
            Settings.Instance.EpisodeStartNumber = 999;

            Assert.Equal("Before", snapshot.DeckName);
            Assert.NotEqual(999, snapshot.EpisodeStartNumber);
        }

        // ── RestoreFrom overwrites current state ────────────────────────────

        [Fact]
        public void RestoreFrom_OverwritesCurrentState()
        {
            Settings.Instance.DeckName = "Original";
            Settings.Instance.EpisodeStartNumber = 1;
            Settings.Instance.ContextLeadingCount = 3;
            var snapshot = Settings.Instance.Snapshot();

            // Mutate
            Settings.Instance.DeckName = "Changed";
            Settings.Instance.EpisodeStartNumber = 99;
            Settings.Instance.ContextLeadingCount = 7;

            // Restore
            Settings.Instance.RestoreFrom(snapshot);

            Assert.Equal("Original", Settings.Instance.DeckName);
            Assert.Equal(1, Settings.Instance.EpisodeStartNumber);
            Assert.Equal(3, Settings.Instance.ContextLeadingCount);
        }

        // ── RestoreFrom clears transient Files arrays ───────────────────────

        [Fact]
        public void RestoreFrom_ClearsTransientFiles()
        {
            Settings.Instance.Subs[0].Files = new[] { "a.srt", "b.srt" };
            Settings.Instance.AudioClips.Files = new[] { "x.mp3" };
            Settings.Instance.VideoClips.Files = new[] { "v.mkv" };
            var snapshot = Settings.Instance.Snapshot();

            Settings.Instance.RestoreFrom(snapshot);

            Assert.Empty(Settings.Instance.Subs[0].Files);
            Assert.Empty(Settings.Instance.Subs[1].Files);
            Assert.Empty(Settings.Instance.AudioClips.Files);
            Assert.Empty(Settings.Instance.VideoClips.Files);
        }

        // ── CreateDefaults returns fresh defaults ───────────────────────────

        [Fact]
        public void CreateDefaults_HasExpectedValues()
        {
            var defaults = Settings.CreateDefaults();

            Assert.NotNull(defaults.Subs);
            Assert.Equal(2, defaults.Subs.Length);
            Assert.True(defaults.Subs[0].TimingsEnabled);
            Assert.True(defaults.Subs[0].ActorsEnabled);
            Assert.Equal("", defaults.DeckName);
            Assert.Equal(1, defaults.EpisodeStartNumber);
            Assert.NotNull(defaults.ActorList);
            Assert.Empty(defaults.ActorList);
        }

        // ── Reset restores instance to defaults ─────────────────────────────

        [Fact]
        public void Reset_RestoresToDefaults()
        {
            Settings.Instance.DeckName = "Dirty";
            Settings.Instance.EpisodeStartNumber = 777;
            Settings.Instance.SpanEnabled = true;

            Settings.Instance.reset();

            Assert.Equal("", Settings.Instance.DeckName);
            Assert.Equal(1, Settings.Instance.EpisodeStartNumber);
            Assert.False(Settings.Instance.SpanEnabled);
        }

        // ── Snapshot round-trip through SubSettings ─────────────────────────

        [Fact]
        public void Snapshot_PreservesSubSettings()
        {
            Settings.Instance.Subs[0].FilePattern = "*.srt";
            Settings.Instance.Subs[0].TimeShift = 500;
            Settings.Instance.Subs[0].Encoding = "shift_jis";
            Settings.Instance.Subs[1].JoinSentencesEnabled = false;

            var snapshot = Settings.Instance.Snapshot();

            Assert.Equal("*.srt", snapshot.Subs[0].FilePattern);
            Assert.Equal(500, snapshot.Subs[0].TimeShift);
            Assert.Equal("shift_jis", snapshot.Subs[0].Encoding);
            Assert.False(snapshot.Subs[1].JoinSentencesEnabled);
        }

        // ── Snapshot round-trip through media settings ──────────────────────

        [Fact]
        public void Snapshot_PreservesMediaSettings()
        {
            Settings.Instance.AudioClips.Bitrate = 256;
            Settings.Instance.AudioClips.Normalize = true;
            Settings.Instance.AudioClips.AudioFormat = "MP3";
            Settings.Instance.VideoClips.BitrateVideo = 1500;
            Settings.Instance.Snapshots.Quality = 10;

            var snapshot = Settings.Instance.Snapshot();

            Assert.Equal(256, snapshot.AudioClips.Bitrate);
            Assert.True(snapshot.AudioClips.Normalize);
            Assert.Equal("MP3", snapshot.AudioClips.AudioFormat);
            Assert.Equal(1500, snapshot.VideoClips.BitrateVideo);
            Assert.Equal(10, snapshot.Snapshots.Quality);
        }

        [Fact]
        public void Snapshot_PreservesOpusFormat()
        {
            Settings.Instance.AudioClips.AudioFormat = "Opus";

            var snapshot = Settings.Instance.Snapshot();

            Assert.Equal("Opus", snapshot.AudioClips.AudioFormat);
        }
    }
}
