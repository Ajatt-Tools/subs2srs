//  Copyright (C) 2026 fkzys
//  SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.IO;
using System.Text;
using Xunit;

namespace subs2srs.Tests
{
    public class PrefIOTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _jsonPath;
        private readonly string _origSettingsFilename;
        private readonly PreferencesData _origPrefs;

        public PrefIOTests()
        {
            _origSettingsFilename = ConstantSettings.SettingsFilename;
            _origPrefs = ConstantSettings.Prefs;

            _tempDir = Path.Combine(
                Path.GetTempPath(),
                "subs2srs_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);

            // Redirect so PrefIO.JsonPath resolves into temp dir
            ConstantSettings.SettingsFilename =
                Path.Combine(_tempDir, "preferences.txt");

            _jsonPath = Path.Combine(_tempDir, "preferences.json");
        }

        public void Dispose()
        {
            ConstantSettings.SettingsFilename = _origSettingsFilename;
            ConstantSettings.Prefs = _origPrefs;
            try { Directory.Delete(_tempDir, true); } catch { }
        }

        // ── Write creates a file ────────────────────────────────────────

        [Fact]
        public void Write_CreatesJsonFile()
        {
            ConstantSettings.Prefs = new PreferencesData();
            PrefIO.Write();
            Assert.True(File.Exists(_jsonPath));
        }

        // ── Round-trip with defaults ────────────────────────────────────

        [Fact]
        public void ReadAfterWrite_RoundTripsDefaults()
        {
            ConstantSettings.Prefs = new PreferencesData();
            PrefIO.Write();

            ConstantSettings.Prefs = new PreferencesData
                { MainWindowWidth = 9999 };

            PrefIO.read();

            Assert.Equal(PrefDefaults.MainWindowWidth,
                ConstantSettings.MainWindowWidth);
            Assert.Equal(PrefDefaults.MainWindowHeight,
                ConstantSettings.MainWindowHeight);
            Assert.Equal(PrefDefaults.DefaultSnapshotJpegQuality,
                ConstantSettings.DefaultSnapshotJpegQuality);
        }

        // ── Round-trip with custom values ───────────────────────────────

        [Fact]
        public void ReadAfterWrite_RoundTripsCustomValues()
        {
            ConstantSettings.Prefs = new PreferencesData
            {
                MainWindowWidth = 1234,
                MainWindowHeight = 567,
                DefaultSnapshotJpegQuality = 15,
                SrsDelimiter = "\t",
                EnableLogging = true,
                VideoPlayer = "/usr/bin/mpv",
            };
            PrefIO.Write();

            ConstantSettings.Prefs = new PreferencesData();
            PrefIO.read();

            Assert.Equal(1234, ConstantSettings.MainWindowWidth);
            Assert.Equal(567, ConstantSettings.MainWindowHeight);
            Assert.Equal(15, ConstantSettings.DefaultSnapshotJpegQuality);
            Assert.Equal("\t", ConstantSettings.SrsDelimiter);
            Assert.True(ConstantSettings.EnableLogging);
            Assert.Equal("/usr/bin/mpv", ConstantSettings.VideoPlayer);
        }

        // ── Missing file → defaults + writes json ───────────────────────

        [Fact]
        public void Read_NoFiles_CreatesDefaultsAndWritesJson()
        {
            // Neither old .txt nor .json exist
            PrefIO.read();

            Assert.True(File.Exists(_jsonPath));
            Assert.Equal(PrefDefaults.MainWindowWidth,
                ConstantSettings.MainWindowWidth);
        }

        // ── Corrupt JSON → falls back to defaults ───────────────────────

        [Fact]
        public void Read_CorruptJson_FallsBackToDefaults()
        {
            File.WriteAllText(_jsonPath,
                "{{not valid json!!", Encoding.UTF8);

            PrefIO.read();

            Assert.Equal(PrefDefaults.MainWindowWidth,
                ConstantSettings.MainWindowWidth);
            Assert.Equal(PrefDefaults.DefaultAudioClipBitrate,
                ConstantSettings.DefaultAudioClipBitrate);
        }

        // ── Tabs and newlines stored natively (no token encoding) ───────

        [Fact]
        public void Write_SpecialChars_NoTokenEncoding()
        {
            ConstantSettings.Prefs = new PreferencesData
            {
                SrsDelimiter = "\t",
                AudioNormalizeArgs = "a\nb\r\nc",
            };
            PrefIO.Write();

            string json = File.ReadAllText(_jsonPath, Encoding.UTF8);
            Assert.DoesNotContain("${tab}", json);
            Assert.DoesNotContain("${lf}", json);
            Assert.DoesNotContain("${cr}", json);

            // Round-trip
            ConstantSettings.Prefs = new PreferencesData();
            PrefIO.read();

            Assert.Equal("\t", ConstantSettings.SrsDelimiter);
            Assert.Equal("a\nb\r\nc",
                ConstantSettings.AudioNormalizeArgs);
        }

        // ── Template tokens in string values survive round-trip ─────────

        [Fact]
        public void ReadAfterWrite_TemplateTokens_Preserved()
        {
            ConstantSettings.Prefs = new PreferencesData
            {
                SrsFilenameFormat = "custom_${deck_name}",
                AudioFilenameFormat =
                    "${deck_name}_${episode_num}_${sequence_num}",
                VideoPlayerArgs = "--start=${s_total_sec}",
            };
            PrefIO.Write();

            ConstantSettings.Prefs = new PreferencesData();
            PrefIO.read();

            Assert.Equal("custom_${deck_name}",
                ConstantSettings.SrsFilenameFormat);
            Assert.Equal(
                "${deck_name}_${episode_num}_${sequence_num}",
                ConstantSettings.AudioFilenameFormat);
            Assert.Equal("--start=${s_total_sec}",
                ConstantSettings.VideoPlayerArgs);
        }

        // ── Bool prefs round-trip ───────────────────────────────────────

        [Fact]
        public void ReadAfterWrite_BoolPrefs_RoundTrip()
        {
            ConstantSettings.Prefs = new PreferencesData
            {
                DefaultEnableAudioClipGeneration = false,
                DefaultEnableSnapshotsGeneration = true,
                ReencodeBeforeSplittingAudio = true,
                DefaultIphoneSupport = true,
            };
            PrefIO.Write();

            ConstantSettings.Prefs = new PreferencesData();
            PrefIO.read();

            Assert.False(ConstantSettings.DefaultEnableAudioClipGeneration);
            Assert.True(ConstantSettings.DefaultEnableSnapshotsGeneration);
            Assert.True(ConstantSettings.ReencodeBeforeSplittingAudio);
            Assert.True(ConstantSettings.DefaultIphoneSupport);
        }

        // ── Int prefs round-trip ────────────────────────────────────────

        [Fact]
        public void ReadAfterWrite_IntPrefs_RoundTrip()
        {
            ConstantSettings.Prefs = new PreferencesData
            {
                DefaultAudioClipBitrate = 256,
                LongClipWarningSeconds = 42,
                MaxParallelTasks = 8,
                DefaultContextNumLeading = 3,
            };
            PrefIO.Write();

            ConstantSettings.Prefs = new PreferencesData();
            PrefIO.read();

            Assert.Equal(256, ConstantSettings.DefaultAudioClipBitrate);
            Assert.Equal(42, ConstantSettings.LongClipWarningSeconds);
            Assert.Equal(8, ConstantSettings.MaxParallelTasks);
            Assert.Equal(3, ConstantSettings.DefaultContextNumLeading);
        }
    }
}
