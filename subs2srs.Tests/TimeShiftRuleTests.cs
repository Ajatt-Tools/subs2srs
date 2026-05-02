//  Copyright (C) 2026 fkzys and contributors
//  SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Generic;
using Xunit;

namespace subs2srs.Tests
{
  public class TimeShiftRuleTests
  {
    // ── Constructor ────────────────────────────────────────────────────

    [Fact]
    public void DefaultConstructor_ZeroValues()
    {
      var rule = new TimeShiftRule();
      Assert.Equal(0, rule.FromEpisode);
      Assert.Equal(0, rule.ShiftMs);
    }

    [Fact]
    public void ParameterizedConstructor_SetsValues()
    {
      var rule = new TimeShiftRule(5, -200);
      Assert.Equal(5, rule.FromEpisode);
      Assert.Equal(-200, rule.ShiftMs);
    }

    // ── Properties round-trip ──────────────────────────────────────────

    [Fact]
    public void Properties_SetAndGet()
    {
      var rule = new TimeShiftRule();
      rule.FromEpisode = 12;
      rule.ShiftMs = 500;
      Assert.Equal(12, rule.FromEpisode);
      Assert.Equal(500, rule.ShiftMs);
    }
  }

  public class SubSettingsGetEffectiveTimeShiftTests
  {
    // ── No rules → falls back to global TimeShift ────────────────────

    [Fact]
    public void NoRules_ReturnsGlobalTimeShift()
    {
      var sub = new SubSettings { TimeShift = 100 };
      Assert.Equal(100, sub.GetEffectiveTimeShift(1));
      Assert.Equal(100, sub.GetEffectiveTimeShift(99));
    }

    [Fact]
    public void NullRules_ReturnsGlobalTimeShift()
    {
      var sub = new SubSettings { TimeShift = -50, TimeShiftRules = null };
      Assert.Equal(-50, sub.GetEffectiveTimeShift(1));
    }

    [Fact]
    public void EmptyRules_ReturnsGlobalTimeShift()
    {
      var sub = new SubSettings { TimeShift = 300, TimeShiftRules = new List<TimeShiftRule>() };
      Assert.Equal(300, sub.GetEffectiveTimeShift(5));
    }

    // ── Single rule ──────────────────────────────────────────────────

    [Fact]
    public void SingleRule_EpisodeBeforeRule_ReturnsGlobal()
    {
      var sub = new SubSettings
      {
        TimeShift = 100,
        TimeShiftRules = new List<TimeShiftRule> { new(5, 999) }
      };
      // Episode 3 is before rule's FromEpisode=5
      Assert.Equal(100, sub.GetEffectiveTimeShift(3));
    }

    [Fact]
    public void SingleRule_EpisodeExactMatch_ReturnsRuleShift()
    {
      var sub = new SubSettings
      {
        TimeShift = 100,
        TimeShiftRules = new List<TimeShiftRule> { new(5, 999) }
      };
      Assert.Equal(999, sub.GetEffectiveTimeShift(5));
    }

    [Fact]
    public void SingleRule_EpisodeAfterRule_ReturnsRuleShift()
    {
      var sub = new SubSettings
      {
        TimeShift = 100,
        TimeShiftRules = new List<TimeShiftRule> { new(5, 999) }
      };
      // Cascading: rule applies to episode 5 and onward
      Assert.Equal(999, sub.GetEffectiveTimeShift(10));
    }

    // ── Multiple cascading rules ─────────────────────────────────────

    [Fact]
    public void MultipleRules_CascadingLookup()
    {
      var sub = new SubSettings
      {
        TimeShift = 0,
        TimeShiftRules = new List<TimeShiftRule>
        {
          new(1, 100),
          new(5, 200),
          new(10, -300),
        }
      };

      // Episode 1-4: first rule (shift=100)
      Assert.Equal(100, sub.GetEffectiveTimeShift(1));
      Assert.Equal(100, sub.GetEffectiveTimeShift(4));

      // Episode 5-9: second rule (shift=200)
      Assert.Equal(200, sub.GetEffectiveTimeShift(5));
      Assert.Equal(200, sub.GetEffectiveTimeShift(7));
      Assert.Equal(200, sub.GetEffectiveTimeShift(9));

      // Episode 10+: third rule (shift=-300)
      Assert.Equal(-300, sub.GetEffectiveTimeShift(10));
      Assert.Equal(-300, sub.GetEffectiveTimeShift(50));
    }

    // ── Rule from episode 1 overrides global ─────────────────────────

    [Fact]
    public void RuleFromEpisode1_OverridesGlobal()
    {
      var sub = new SubSettings
      {
        TimeShift = 999,
        TimeShiftRules = new List<TimeShiftRule> { new(1, 42) }
      };
      // Global is 999 but rule from ep1 says 42
      Assert.Equal(42, sub.GetEffectiveTimeShift(1));
      Assert.Equal(42, sub.GetEffectiveTimeShift(100));
    }

    // ── Negative shift values ────────────────────────────────────────

    [Fact]
    public void NegativeShift_Preserved()
    {
      var sub = new SubSettings
      {
        TimeShift = 0,
        TimeShiftRules = new List<TimeShiftRule> { new(3, -500) }
      };
      Assert.Equal(0, sub.GetEffectiveTimeShift(2));
      Assert.Equal(-500, sub.GetEffectiveTimeShift(3));
    }

    // ── Zero shift in rule ───────────────────────────────────────────

    [Fact]
    public void ZeroShiftRule_OverridesNonZeroGlobal()
    {
      var sub = new SubSettings
      {
        TimeShift = 1000,
        TimeShiftRules = new List<TimeShiftRule> { new(5, 0) }
      };
      Assert.Equal(1000, sub.GetEffectiveTimeShift(4));
      Assert.Equal(0, sub.GetEffectiveTimeShift(5));
    }

    // ── Gap between rules falls back to last matching rule ───────────

    [Fact]
    public void GapBetweenRules_LastMatchWins()
    {
      var sub = new SubSettings
      {
        TimeShift = 0,
        TimeShiftRules = new List<TimeShiftRule>
        {
          new(3, 100),
          new(10, 200),
        }
      };
      // Episodes 1-2: no rule matches, global wins
      Assert.Equal(0, sub.GetEffectiveTimeShift(1));
      // Episodes 3-9: first rule wins
      Assert.Equal(100, sub.GetEffectiveTimeShift(6));
      // Episode 10+: second rule wins
      Assert.Equal(200, sub.GetEffectiveTimeShift(15));
    }

    // ── Default TimeShiftRules is empty list ─────────────────────────

    [Fact]
    public void DefaultSubSettings_HasEmptyRulesList()
    {
      var sub = new SubSettings();
      Assert.NotNull(sub.TimeShiftRules);
      Assert.Empty(sub.TimeShiftRules);
    }
  }
}
