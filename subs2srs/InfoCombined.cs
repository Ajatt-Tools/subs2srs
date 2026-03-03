//  Copyright (C) 2009-2016 Christopher Brochtrup
//  Copyright (C) 2026 fkzys (GTK3/.NET 10 port)
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
  /// Represents paired lines: Subs1 and its corresponding Subs2.
  /// </summary>
  public class InfoCombined
  {
    public InfoLine Subs1 { get; set; }
    public InfoLine Subs2 { get; set; }

    /// <summary>
    /// Is the line active? (That is, will it be processed?)
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Is the line only needed for context information?
    /// If true, Active is false for this line.
    /// </summary>
    public bool OnlyNeededForContext { get; set; }

    public InfoCombined()
    {
      Subs1 = new InfoLine();
      Subs2 = new InfoLine();
      Active = true;
      OnlyNeededForContext = false;
    }

    public InfoCombined(InfoLine subs1, InfoLine subs2)
    {
      Subs1 = subs1;
      Subs2 = subs2;
      Active = true;
      OnlyNeededForContext = false;
    }

    public InfoCombined(InfoLine subs1, InfoLine subs2, bool active)
    {
      Subs1 = subs1;
      Subs2 = subs2;
      Active = active;
      OnlyNeededForContext = false;
    }

    public override string ToString()
    {
      return $"{Active}, {OnlyNeededForContext}, {Subs1.StartTime}, {Subs1.EndTime}";
    }
  }
}
