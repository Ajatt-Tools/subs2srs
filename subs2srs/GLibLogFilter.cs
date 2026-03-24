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

namespace subs2srs
{
    /// <summary>
    /// Formerly suppressed GtkSharp toggle_ref warnings via g_log_set_writer_func.
    /// Gir.Core does not use toggle_ref, so this filter is no longer needed.
    /// Kept as a no-op stub; will be removed in a cleanup commit.
    /// </summary>
    static class GLibLogFilter
    {
        public static void Install()
        {
            // No-op: Gir.Core does not produce toggle_ref warnings.
        }
    }
}
