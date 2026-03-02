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

using System;

namespace subs2srs
{
    public class UtilsSnapshot
    {
        public static void takeSnapshotFromVideo(string inFile, DateTime snapTime,
            ImageSize size, ImageCrop crop, string outFile)
        {
            string startTimeArg = UtilsVideo.formatStartTimeArg(snapTime);
            string videoSizeArg = UtilsVideo.formatVideoSizeArg(inFile, size, crop, 2, 2);
            string cropArg = UtilsVideo.formatCropArg(inFile, size, crop);

            string ffmpegSnapshotProgArgs = String.Format(
                "-y -an {0} -i \"{1}\" -f image2 -vf \"{2}, {3}\" -vframes 1 \"{4}\"",
                startTimeArg, inFile, videoSizeArg, cropArg, outFile);

            UtilsCommon.startFFmpeg(ffmpegSnapshotProgArgs, false, true);
        }
    }
}
