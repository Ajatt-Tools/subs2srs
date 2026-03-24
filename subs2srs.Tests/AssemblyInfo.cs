//  Copyright (C) 2026 fkzys
//  SPDX-License-Identifier: GPL-3.0-or-later

using Xunit;

// All test classes share a mutable static Settings.Instance singleton,
// so parallel execution causes flaky failures due to race conditions.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
