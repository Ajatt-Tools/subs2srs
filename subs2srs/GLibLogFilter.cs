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
using System.Runtime.InteropServices;

namespace subs2srs
{
    static class GLibLogFilter
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct GLogField
        {
            public IntPtr key;
            public IntPtr value;
            public nint length;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int LogWriterFunc(int logLevel, IntPtr fields, nuint nFields, IntPtr userData);

        [DllImport("libglib-2.0.so.0")]
        private static extern void g_log_set_writer_func(LogWriterFunc func, IntPtr userData, IntPtr notify);

        [DllImport("libglib-2.0.so.0")]
        private static extern int g_log_writer_default(int logLevel, IntPtr fields, nuint nFields, IntPtr userData);

        // prevent GC of the delegate
        private static LogWriterFunc _callback;

        public static void Install()
        {
            _callback = OnWrite;
            g_log_set_writer_func(_callback, IntPtr.Zero, IntPtr.Zero);
        }

        private static int OnWrite(int logLevel, IntPtr fields, nuint nFields, IntPtr userData)
        {
            // G_LOG_LEVEL_CRITICAL = 1 << 3 = 8
            if ((logLevel & 8) != 0)
            {
                int fieldSize = Marshal.SizeOf<GLogField>();
                for (nuint i = 0; i < nFields; i++)
                {
                    var f = Marshal.PtrToStructure<GLogField>(fields + (nint)(i * (nuint)fieldSize));
                    string key = Marshal.PtrToStringUTF8(f.key);
                    if (key == "MESSAGE")
                    {
                        string msg = f.length >= 0
                            ? Marshal.PtrToStringUTF8(f.value, (int)f.length)
                            : Marshal.PtrToStringUTF8(f.value);
                        if (msg != null && msg.Contains("toggle_ref"))
                            return 1; // G_LOG_WRITER_HANDLED — suppress completely
                        break;
                    }
                }
            }

            return g_log_writer_default(logLevel, fields, nFields, userData);
        }
    }
}
