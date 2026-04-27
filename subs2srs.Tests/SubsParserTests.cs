//  Copyright (C) 2026 fkzys and contributors
//  SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.IO;
using System.Text;
using Xunit;

namespace subs2srs.Tests
{
    public class SubsParserTests : IDisposable
    {
        private readonly string _tempDir;

        public SubsParserTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "subs2srs_parser_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            Settings.Instance.reset();
        }

        public void Dispose()
        {
            try { Directory.Delete(_tempDir, true); } catch { }
        }

        [Fact]
        public void ParseSRT_ValidFile_ReturnsLines()
        {
            var srt = "1\n00:00:01,000 --> 00:00:04,000\nHello World\n\n2\n00:00:05,000 --> 00:00:08,000\nSecond Line\n";
            var path = Path.Combine(_tempDir, "test.srt");
            File.WriteAllText(path, srt, Encoding.UTF8);

            var parser = new SubsParserSRT(path, Encoding.UTF8);
            var lines = parser.parse();

            Assert.Equal(2, lines.Count);
            Assert.Equal("Hello World", lines[0].Text);
            Assert.Equal(TimeSpan.FromSeconds(1), lines[0].StartTime);
            Assert.Equal(TimeSpan.FromSeconds(4), lines[0].EndTime);
        }

        [Fact]
        public void ParseASS_ValidFile_ReturnsLines()
        {
            var ass = "[Events]\nFormat: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\nDialogue: 0,0:00:01.00,0:00:04.00,Default,,0,0,0,,Hello ASS\n";
            var path = Path.Combine(_tempDir, "test.ass");
            File.WriteAllText(path, ass, Encoding.UTF8);

            var parser = new SubsParserASS(null, path, Encoding.UTF8, 1);
            var lines = parser.parse();

            Assert.Single(lines);
            Assert.Equal("Hello ASS", lines[0].Text);
        }

        [Fact]
        public void ParseLRC_ValidFile_ReturnsLines()
        {
            var lrc = "[00:01.00]Line one\n[00:05.00]Line two\n";
            var path = Path.Combine(_tempDir, "test.lrc");
            File.WriteAllText(path, lrc, Encoding.UTF8);

            var parser = new SubsParserLyrics(path, Encoding.UTF8);
            var lines = parser.parse();

            Assert.Equal(2, lines.Count);
            Assert.Equal("Line one", lines[0].Text);
        }

        [Fact]
        public void ParseSRT_InvalidTime_ThrowsException()
        {
            var srt = "1\n00:00:01.000 --> 00:00:04.000\nBad format\n";
            var path = Path.Combine(_tempDir, "bad.srt");
            File.WriteAllText(path, srt, Encoding.UTF8);

            var parser = new SubsParserSRT(path, Encoding.UTF8);
            Assert.Throws<Exception>(() => parser.parse());
        }

        [Fact]
        public void Parse_FileHandleReleasedAfterParsing()
        {
            var srt = "1\n00:00:01,000 --> 00:00:04,000\nTest\n";
            var path = Path.Combine(_tempDir, "lock.srt");
            File.WriteAllText(path, srt, Encoding.UTF8);

            var parser = new SubsParserSRT(path, Encoding.UTF8);
            parser.parse();

            // Verify file is not locked (would fail on Windows if StreamReader wasn't disposed)
            File.Delete(path);
            Assert.False(File.Exists(path));
        }
    }
}
