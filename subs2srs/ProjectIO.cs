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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace subs2srs
{
    /// <summary>
    /// Save / load per-session project state as .s2s.json files.
    /// This is separate from <see cref="PrefIO"/> which handles global preferences.
    /// </summary>
    public static class ProjectIO
    {
        private static readonly JsonSerializerOptions Opts = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Serialize <paramref name="settings"/> to a human-readable JSON file.
        /// </summary>
        public static void Save(string path, Settings settings)
        {
            var json = JsonSerializer.Serialize(settings, Opts);
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        /// <summary>
        /// Deserialize a project file and apply it to <see cref="Settings.Instance"/>.
        /// Throws on I/O or parse errors — the caller is expected to catch and show a dialog.
        /// If deserialization fails, <see cref="Settings.Instance"/> is left unchanged.
        /// </summary>
        public static void Load(string path)
        {
            var json = File.ReadAllText(path, Encoding.UTF8);
            var loaded = JsonSerializer.Deserialize<Settings>(json, Opts)
                         ?? throw new InvalidOperationException("Deserialized project is null.");

            // Guard against incomplete JSON: ensure required sub-objects exist
            var def = Settings.CreateDefaults();
            if (loaded.Subs == null || loaded.Subs.Length < 2) loaded.Subs = def.Subs;
            loaded.AudioClips      ??= def.AudioClips;
            loaded.VideoClips      ??= def.VideoClips;
            loaded.Snapshots       ??= def.Snapshots;
            loaded.VobSubColors    ??= def.VobSubColors;
            loaded.LanguageSpecific ??= def.LanguageSpecific;
            loaded.ActorList       ??= new List<string>();

            Settings.Instance.RestoreFrom(loaded);
        }
    }
}
