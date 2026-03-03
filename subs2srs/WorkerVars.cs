//  Copyright (C) 2009-2016 Christopher Brochtrup
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

using System.Collections.Generic;

namespace subs2srs
{
  /// <summary>
  /// Object passed to the main worker thread.
  /// </summary>
  public class WorkerVars
  {
    public enum SubsProcessingType
    {
      Normal,
      Preview,
      Dueling
    }

    /// <summary>
    /// Combined lines for all episodes.
    /// </summary>
    public List<List<InfoCombined>> CombinedAll { get; set; }

    /// <summary>
    /// The media directory.
    /// </summary>
    public string MediaDir { get; set; }

    /// <summary>
    /// The type of processing.
    /// </summary>
    public SubsProcessingType ProcessingType { get; set; }

    public WorkerVars(List<List<InfoCombined>> combinedAll, string mediaDir, SubsProcessingType processingType)
    {
      CombinedAll = combinedAll;
      MediaDir = mediaDir;
      ProcessingType = processingType;
    }
  }
}
