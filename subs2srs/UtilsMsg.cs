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

using System;

namespace subs2srs
{
  public class UtilsMsg
  {
    public static Action<string, string> OnShowError;
    public static Action<string, string> OnShowInfo;
    public static Func<string, string, bool> OnShowConfirm;

    public static void showErrMsg(string msg)
    {
      Console.Error.WriteLine($"ERROR: {msg}");
      Logger.Instance.error(msg);
      OnShowError?.Invoke(msg, "Error");
    }

    public static void showInfoMsg(string msg)
    {
      Console.Error.WriteLine($"INFO: {msg}");
      Logger.Instance.info(msg);
      OnShowInfo?.Invoke(msg, UtilsAssembly.Title);
    }

    public static bool showConfirm(string msg)
    {
      Console.Error.WriteLine($"CONFIRM: {msg}");
      if (OnShowConfirm != null) return OnShowConfirm(msg, "Confirmation");
      return false;
    }
  }
}
