//  Copyright (C) 2026 fkzys and contributors
//
//  This file is part of subs2srs.
//
//  subs2srs is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  subs2srs is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with subs2srs.  If not, see <http://www.gnu.org/licenses/>.
//
//////////////////////////////////////////////////////////////////////////////
using System;
using System.Diagnostics;
using System.Threading;

namespace subs2srs
{
    public interface IProgressReporter
    {
        bool Cancel { get; }
        int StepsTotal { get; set; }
        void NextStep(int step, string description);
        void UpdateProgress(int percent, string text);
        void UpdateProgress(string text);
        void EnableDetail(bool enable);
        void SetDuration(TimeSpan duration);
        void OnFFmpegOutput(object sender, DataReceivedEventArgs e);
        CancellationToken Token => CancellationToken.None;
    }
}
