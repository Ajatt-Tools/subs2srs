//  Copyright (C) 2026 fkzys
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
using System.Threading;

namespace subs2srs
{
    /// <summary>
    /// Routes async/await continuations back to the GTK main loop.
    /// Without this, code after 'await' runs on thread-pool threads,
    /// causing GTK threading violations.
    ///
    /// GTK4/Gir.Core equivalent: GLib.Functions.IdleAdd replaces GLib.Idle.Add.
    /// </summary>
    sealed class GtkSynchronizationContext : SynchronizationContext
    {
        private readonly int _mainThreadId = Thread.CurrentThread.ManagedThreadId;

        public override void Post(SendOrPostCallback d, object? state)
        {
            // Schedule callback on the GLib main loop (idle handler).
            // Priority 0 = G_PRIORITY_DEFAULT_IDLE.
            GLib.Functions.IdleAdd(0, () =>
            {
                d(state);
                return false; // one-shot, do not repeat
            });
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            // If already on the main thread, run directly to avoid deadlock
            if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
            {
                d(state);
                return;
            }

            using var done = new ManualResetEventSlim(false);
            Exception? caught = null;
            GLib.Functions.IdleAdd(0, () =>
            {
                try { d(state); }
                catch (Exception ex) { caught = ex; }
                finally { done.Set(); }
                return false;
            });
            done.Wait();
            if (caught != null)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo
                    .Capture(caught).Throw();
        }

        public override SynchronizationContext CreateCopy() => this;
    }
}
