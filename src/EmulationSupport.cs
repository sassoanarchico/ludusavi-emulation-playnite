using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace LudusaviPlaynite
{
    public class EmulatedSaveMapping
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// A case-insensitive substring match against emulator name/path (e.g. "rpcs3", "desmume").
        /// </summary>
        public string EmulatorMatch { get; set; } = "";

        /// <summary>
        /// Logical platform label for this emulator (e.g. "Nintendo DS", "PlayStation 3").
        /// </summary>
        public string Platform { get; set; } = "";

        /// <summary>
        /// One template per line. Supports: &lt;romPath&gt;, &lt;romDir&gt;, &lt;romBase&gt;, &lt;emulatorExe&gt;, &lt;emulatorDir&gt;, &lt;appData&gt;, &lt;userProfile&gt;, &lt;gameId&gt;.
        /// </summary>
        public string SavePathTemplates { get; set; } = "";

        public EmulatedSaveMapping Clone()
        {
            return new EmulatedSaveMapping
            {
                Enabled = this.Enabled,
                EmulatorMatch = this.EmulatorMatch,
                Platform = this.Platform,
                SavePathTemplates = this.SavePathTemplates
            };
        }
    }

    /// <summary>
    /// Provides default emulator save mappings for common emulators.
    /// </summary>
    public static class DefaultEmulatorMappings
    {
        public static List<EmulatedSaveMapping> GetDefaults()
        {
            return new List<EmulatedSaveMapping>
            {
                // Nintendo DS (DeSmuME, melonDS)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "desmume",
                    Platform = "Nintendo DS",
                    SavePathTemplates = "<romDir>/<romBase>.dsv\n<romDir>/<romBase>.sav"
                },
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "melonds",
                    Platform = "Nintendo DS",
                    SavePathTemplates = "<romDir>/<romBase>.sav"
                },
                // Nintendo 3DS (Citra)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "citra",
                    Platform = "Nintendo 3DS",
                    SavePathTemplates = "<appData>/Citra/sdmc/Nintendo 3DS/**/<romBase>/**"
                },
                // Game Boy / Game Boy Color / Game Boy Advance (mGBA, VisualBoyAdvance)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "mgba",
                    Platform = "Game Boy Advance",
                    SavePathTemplates = "<romDir>/<romBase>.sav\n<romDir>/<romBase>.srm"
                },
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "visualboy",
                    Platform = "Game Boy Advance",
                    SavePathTemplates = "<romDir>/<romBase>.sav\n<romDir>/<romBase>.sgm"
                },
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "vba",
                    Platform = "Game Boy Advance",
                    SavePathTemplates = "<romDir>/<romBase>.sav\n<romDir>/<romBase>.sgm"
                },
                // PlayStation 1 (DuckStation, ePSXe)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "duckstation",
                    Platform = "PlayStation",
                    SavePathTemplates = "<userProfile>/Documents/DuckStation/memcards/<romBase>*.mcd\n<appData>/DuckStation/memcards/<romBase>*.mcd"
                },
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "epsxe",
                    Platform = "PlayStation",
                    SavePathTemplates = "<emulatorDir>/memcards/<romBase>*.mcr"
                },
                // PlayStation 2 (PCSX2)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "pcsx2",
                    Platform = "PlayStation 2",
                    SavePathTemplates = "<userProfile>/Documents/PCSX2/memcards/*.ps2\n<appData>/PCSX2/memcards/*.ps2"
                },
                // PlayStation 3 (RPCS3) - uses game ID (e.g., BLES01459) for save folder
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "rpcs3",
                    Platform = "PlayStation 3",
                    // Note: RPCS3 user can be 00000001 or another id. We allow any home/* here.
                    SavePathTemplates = "<emulatorDir>/dev_hdd0/home/*/savedata/<gameId>*/**"
                },
                // PlayStation Portable (PPSSPP)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "ppsspp",
                    Platform = "PlayStation Portable",
                    SavePathTemplates = "<userProfile>/Documents/PPSSPP/memstick/PSP/SAVEDATA/**\n<appData>/PPSSPP/PSP/SAVEDATA/**"
                },
                // Nintendo 64 (Project64, Mupen64Plus)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "project64",
                    Platform = "Nintendo 64",
                    SavePathTemplates = "<emulatorDir>/Save/<romBase>.*"
                },
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "mupen64",
                    Platform = "Nintendo 64",
                    SavePathTemplates = "<romDir>/<romBase>.sra\n<romDir>/<romBase>.eep\n<romDir>/<romBase>.fla\n<romDir>/<romBase>.mpk"
                },
                // Nintendo GameCube / Wii (Dolphin)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "dolphin",
                    Platform = "Nintendo GameCube",
                    SavePathTemplates = "<userProfile>/Documents/Dolphin Emulator/GC/**\n<userProfile>/Documents/Dolphin Emulator/Wii/title/**"
                },
                // Nintendo Switch (Yuzu, Ryujinx)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "yuzu",
                    Platform = "Nintendo Switch",
                    SavePathTemplates = "<appData>/yuzu/nand/user/save/**"
                },
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "ryujinx",
                    Platform = "Nintendo Switch",
                    SavePathTemplates = "<appData>/Ryujinx/bis/user/save/**"
                },
                // Sega Genesis / Mega Drive (Gens, Fusion, BlastEm)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "gens",
                    Platform = "Sega Genesis",
                    SavePathTemplates = "<romDir>/<romBase>.srm"
                },
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "fusion",
                    Platform = "Sega Genesis",
                    SavePathTemplates = "<romDir>/<romBase>.srm"
                },
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "blastem",
                    Platform = "Sega Genesis",
                    SavePathTemplates = "<romDir>/<romBase>.sram"
                },
                // Sega Dreamcast (Flycast, redream)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "flycast",
                    Platform = "Sega Dreamcast",
                    SavePathTemplates = "<appData>/flycast/data/**"
                },
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "redream",
                    Platform = "Sega Dreamcast",
                    SavePathTemplates = "<appData>/redream/save/**"
                },
                // SNES (Snes9x, bsnes, ZSNES)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "snes9x",
                    Platform = "Super Nintendo",
                    SavePathTemplates = "<romDir>/<romBase>.srm"
                },
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "bsnes",
                    Platform = "Super Nintendo",
                    SavePathTemplates = "<romDir>/<romBase>.srm"
                },
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "zsnes",
                    Platform = "Super Nintendo",
                    SavePathTemplates = "<romDir>/<romBase>.srm\n<romDir>/<romBase>.zst"
                },
                // NES (Mesen, FCEUX, Nestopia)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "mesen",
                    Platform = "Nintendo",
                    SavePathTemplates = "<userProfile>/Documents/Mesen/Saves/<romBase>.sav"
                },
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "fceux",
                    Platform = "Nintendo",
                    SavePathTemplates = "<romDir>/<romBase>.sav"
                },
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "nestopia",
                    Platform = "Nintendo",
                    SavePathTemplates = "<romDir>/<romBase>.sav"
                },
                // Xbox / Xbox 360 (Xemu, Xenia)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "xemu",
                    Platform = "Xbox",
                    SavePathTemplates = "<appData>/xemu/xemu/eeprom.bin\n<appData>/xemu/xemu/xbox_hdd.qcow2"
                },
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "xenia",
                    Platform = "Xbox 360",
                    SavePathTemplates = "<emulatorDir>/content/**"
                },
                // RetroArch (generic - saves usually near ROMs or in RetroArch saves folder)
                new EmulatedSaveMapping
                {
                    Enabled = true,
                    EmulatorMatch = "retroarch",
                    Platform = "RetroArch",
                    SavePathTemplates = "<romDir>/<romBase>.srm\n<appData>/RetroArch/saves/<romBase>.*"
                }
            };
        }
    }


    internal class EmulationInfo
    {
        public bool IsEmulated { get; set; }
        public string EmulatorName { get; set; }
        public string EmulatorExecutablePath { get; set; }
        public string RomPath { get; set; }
        public string Platform { get; set; }
        public string GameId { get; set; }
    }

    internal static class EmulationDetector
    {
        public static bool TryGetInfo(Game game, IPlayniteAPI api, out EmulationInfo info)
        {
            info = new EmulationInfo
            {
                IsEmulated = false,
                EmulatorName = null,
                EmulatorExecutablePath = null,
                RomPath = null,
                Platform = Etc.GetGamePlatform(game)?.Name,
                GameId = game?.GameId,
            };

            if (game == null)
            {
                return false;
            }

            var (romPath, _) = TryGetRomPath(game);
            info.RomPath = romPath;

            var action = TryGetPrimaryAction(game);
            if (action != null)
            {
                // Try to get emulator info from the action's EmulatorId
                var emulatorId = GetGuidProp(action, "EmulatorId");
                if (emulatorId.HasValue && emulatorId.Value != Guid.Empty && api?.Database?.Emulators != null)
                {
                    var emulator = api.Database.Emulators.FirstOrDefault(e => e.Id == emulatorId.Value);
                    if (emulator != null)
                    {
                        info.EmulatorName = emulator.Name;
                        info.EmulatorExecutablePath = emulator.InstallDir;
                        info.IsEmulated = true;
                    }
                }

                // Fallback to action properties if emulator not found
                if (string.IsNullOrEmpty(info.EmulatorName))
                {
                    info.EmulatorExecutablePath = GetStringProp(action, "Path");
                    info.EmulatorName = GetStringProp(action, "Name");
                }

                // Heuristics: GameAction type is emulator, or we have ROMs configured.
                var type = GetPropToString(action, "Type");
                if (!string.IsNullOrEmpty(type) && string.Equals(type, "Emulator", StringComparison.OrdinalIgnoreCase))
                {
                    info.IsEmulated = true;
                }
                else if (!string.IsNullOrEmpty(info.RomPath))
                {
                    info.IsEmulated = true;
                }
            }

            // Additional heuristic: if game has Roms collection, it's emulated.
            if (!info.IsEmulated && !string.IsNullOrEmpty(info.RomPath))
            {
                info.IsEmulated = true;
            }

            return info.IsEmulated;
        }

        private static Guid? GetGuidProp(object obj, string name)
        {
            try
            {
                if (obj == null)
                {
                    return null;
                }
                var prop = obj.GetType().GetProperty(name);
                if (prop == null)
                {
                    return null;
                }
                var value = prop.GetValue(obj, null);
                if (value is Guid g)
                {
                    return g;
                }
            }
            catch
            { }
            return null;
        }

        private static (string, string) TryGetRomPath(Game game)
        {
            try
            {
                var romsProp = game.GetType().GetProperty("Roms");
                if (romsProp == null)
                {
                    return (null, null);
                }

                var romsObj = romsProp.GetValue(game, null);
                var roms = romsObj as IEnumerable;
                if (roms == null)
                {
                    return (null, null);
                }

                foreach (var rom in roms)
                {
                    var path = GetStringProp(rom, "Path") ?? GetStringProp(rom, "RomPath");
                    if (!string.IsNullOrEmpty(path))
                    {
                        return (path, GetStringProp(rom, "Name"));
                    }
                }
            }
            catch
            { }

            return (null, null);
        }

        private static object TryGetPrimaryAction(Game game)
        {
            try
            {
                if (game.GameActions == null)
                {
                    return null;
                }

                // Prefer the play action if flagged.
                foreach (var action in game.GameActions)
                {
                    var isPlay = GetBoolProp(action, "IsPlayAction");
                    if (isPlay == true)
                    {
                        return action;
                    }
                }

                return game.GameActions.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static bool? GetBoolProp(object obj, string name)
        {
            try
            {
                if (obj == null)
                {
                    return null;
                }
                var prop = obj.GetType().GetProperty(name);
                if (prop == null)
                {
                    return null;
                }
                var value = prop.GetValue(obj, null);
                if (value is bool b)
                {
                    return b;
                }
            }
            catch
            { }
            return null;
        }

        private static string GetStringProp(object obj, string name)
        {
            try
            {
                if (obj == null)
                {
                    return null;
                }
                var prop = obj.GetType().GetProperty(name);
                if (prop == null)
                {
                    return null;
                }
                return prop.GetValue(obj, null) as string;
            }
            catch
            {
                return null;
            }
        }

        private static string GetPropToString(object obj, string name)
        {
            try
            {
                if (obj == null)
                {
                    return null;
                }
                var prop = obj.GetType().GetProperty(name);
                if (prop == null)
                {
                    return null;
                }
                var value = prop.GetValue(obj, null);
                return value?.ToString();
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Helper class for PS3/RPCS3 save game detection.
    /// Searches for save folders that contain the game ID code.
    /// </summary>
    internal static class RPCS3SaveFinder
    {
        private static List<string> ExpandSavedataBasesFromAnchor(string anchorPath)
        {
            var results = new List<string>();
            if (string.IsNullOrWhiteSpace(anchorPath))
            {
                return results;
            }

            string anchor = anchorPath;
            try
            {
                anchor = anchor.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            catch
            { }

            // If the anchor points to an executable/file (e.g., rpcs3.exe), use its directory.
            try
            {
                if (!string.IsNullOrWhiteSpace(anchor) && File.Exists(anchor))
                {
                    var dir = Path.GetDirectoryName(anchor);
                    if (!string.IsNullOrWhiteSpace(dir))
                    {
                        anchor = dir;
                    }
                }
            }
            catch
            { }

            if (string.IsNullOrWhiteSpace(anchor) || !Directory.Exists(anchor))
            {
                return results;
            }

            bool IsDirNamed(string p, string name)
            {
                try
                {
                    return string.Equals(Path.GetFileName(p.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)), name, StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }

            // If user already gave us the savedata base, accept it.
            if (IsDirNamed(anchor, "savedata"))
            {
                results.Add(anchor);
                return results;
            }

            // If user gave us a specific RPCS3 home user dir (e.g. .../home/00000002), add its savedata.
            try
            {
                var savedataUnderAnchor = Path.Combine(anchor, "savedata");
                if (Directory.Exists(savedataUnderAnchor))
                {
                    results.Add(savedataUnderAnchor);
                }
            }
            catch
            { }

            // If anchor is .../dev_hdd0, search .../dev_hdd0/home/*/savedata
            try
            {
                var home = IsDirNamed(anchor, "dev_hdd0") ? Path.Combine(anchor, "home") : null;
                if (!string.IsNullOrEmpty(home) && Directory.Exists(home))
                {
                    foreach (var userDir in Directory.GetDirectories(home))
                    {
                        var savedata = Path.Combine(userDir, "savedata");
                        if (Directory.Exists(savedata))
                        {
                            results.Add(savedata);
                        }
                    }
                }
            }
            catch
            { }

            // If anchor is .../dev_hdd0/home, search .../dev_hdd0/home/*/savedata
            try
            {
                if (IsDirNamed(anchor, "home"))
                {
                    foreach (var userDir in Directory.GetDirectories(anchor))
                    {
                        var savedata = Path.Combine(userDir, "savedata");
                        if (Directory.Exists(savedata))
                        {
                            results.Add(savedata);
                        }
                    }
                }
            }
            catch
            { }

            // If anchor is RPCS3 root, search .../dev_hdd0/home/*/savedata
            try
            {
                var devHdd0 = Path.Combine(anchor, "dev_hdd0");
                if (Directory.Exists(devHdd0))
                {
                    var home = Path.Combine(devHdd0, "home");
                    if (Directory.Exists(home))
                    {
                        foreach (var userDir in Directory.GetDirectories(home))
                        {
                            var savedata = Path.Combine(userDir, "savedata");
                            if (Directory.Exists(savedata))
                            {
                                results.Add(savedata);
                            }
                        }
                    }
                }
            }
            catch
            { }

            return results;
        }

        /// <summary>
        /// Extracts PS3 game code from various sources (ROM path, game name, game ID).
        /// PS3 codes are typically: BLES, BLUS, BCES, BCUS, NPEA, NPUA, etc. followed by 5 digits.
        /// Handles "dirty" folder names like "BCES00052_SAVE_1" or "BLES01459-00-FIXED-".
        /// </summary>
        public static string ExtractPS3GameCode(EmulationInfo info, string gameName)
        {
            // Pattern for PS3 game codes: 4 letters + 5 digits
            // Handles "dirty" folder names like "BCES00052_SAVE_1" or "BLES01459-00-FIXED-"
            // Matches: BCES00052, BCES00052_SAVE_1, BLES01459-00-FIXED-, NPEA00385_RC1_SAVEDATA_A
            // Uses word boundary or checks that code is followed by non-alphanumeric or end of string
            var pattern = @"\b([A-Z]{4}\d{5})\b|([A-Z]{4}\d{5})(?=[_\-\s]|$|[^A-Z0-9])";
            var regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Try to extract from ROM path first (most reliable)
            if (!string.IsNullOrEmpty(info?.RomPath))
            {
                var romName = Path.GetFileNameWithoutExtension(info.RomPath);
                var match = regex.Match(romName);
                if (match.Success)
                {
                    // Group 1 is word boundary match, Group 2 is non-word boundary match
                    var code = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                    if (!string.IsNullOrEmpty(code))
                        return code.ToUpperInvariant();
                }
            }

            // Try from GameId
            if (!string.IsNullOrEmpty(info?.GameId))
            {
                var match = regex.Match(info.GameId);
                if (match.Success)
                {
                    var code = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                    if (!string.IsNullOrEmpty(code))
                        return code.ToUpperInvariant();
                }
            }

            // Try from game name
            if (!string.IsNullOrEmpty(gameName))
            {
                var match = regex.Match(gameName);
                if (match.Success)
                {
                    var code = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                    if (!string.IsNullOrEmpty(code))
                        return code.ToUpperInvariant();
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the save folder for a PS3 game by searching for folders that contain the game code.
        /// Handles "dirty" folder names like "BCES00052_SAVE_1" or "BLES01459-00-FIXED-".
        /// </summary>
        public static string FindSaveFolder(string savedataBasePath, string gameCode)
        {
            if (string.IsNullOrEmpty(savedataBasePath) || string.IsNullOrEmpty(gameCode))
                return null;

            if (!Directory.Exists(savedataBasePath))
                return null;

            try
            {
                // Search for folders that contain the game code
                // This works even with "dirty" names like "BCES00052_SAVE_1" because we use IndexOf
                var folders = Directory.GetDirectories(savedataBasePath);
                foreach (var folder in folders)
                {
                    var folderName = Path.GetFileName(folder);
                    // Check if folder name contains the game code (handles names like "BCES00052_SAVE_1")
                    if (folderName.IndexOf(gameCode, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return folder;
                    }
                }
            }
            catch
            {
                // Ignore errors (permissions, etc.)
            }

            return null;
        }

        /// <summary>
        /// Searches multiple possible RPCS3 savedata paths for a game's save folder.
        /// </summary>
        public static string FindSaveFolderInMultiplePaths(IEnumerable<string> savedataPaths, string gameCode)
        {
            if (string.IsNullOrEmpty(gameCode)) return null;

            foreach (var path in savedataPaths)
            {
                var found = FindSaveFolder(path, gameCode);
                if (!string.IsNullOrEmpty(found))
                    return found;
            }

            return null;
        }

        /// <summary>
        /// Gets common RPCS3 savedata paths to search.
        /// </summary>
        public static List<string> GetCommonRPCS3SavePaths(string emulatorDir = null, string customPath = null)
        {
            var paths = new List<string>();
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // Collect anchors (may be root, dev_hdd0, home, user, or savedata itself)
            var anchors = new List<string>();

            // Custom path first (highest priority)
            if (!string.IsNullOrEmpty(customPath))
            {
                anchors.Add(customPath);
            }

            // Emulator directory (portable installs often keep dev_hdd0 inside the install folder)
            if (!string.IsNullOrEmpty(emulatorDir))
            {
                anchors.Add(emulatorDir);
            }

            // Common locations
            anchors.Add(Path.Combine(userProfile, "RPCS3"));
            anchors.Add(Path.Combine(userProfile, "Documents", "RPCS3"));
            anchors.Add(@"C:\RPCS3");

            // Expand anchors into actual savedata bases (supports home/00000001, home/00000002, etc.)
            foreach (var anchor in anchors.Where(a => !string.IsNullOrWhiteSpace(a)).Distinct())
            {
                foreach (var expanded in ExpandSavedataBasesFromAnchor(anchor))
                {
                    if (!string.IsNullOrWhiteSpace(expanded))
                    {
                        paths.Add(expanded);
                    }
                }
            }

            // Backward compatible explicit default (in case directory exists but anchor scanning missed it)
            try
            {
                var legacy = Path.Combine(userProfile, "RPCS3", "dev_hdd0", "home", "00000001", "savedata");
                if (Directory.Exists(legacy))
                {
                    paths.Add(legacy);
                }
            }
            catch
            { }

            return paths.Where(p => !string.IsNullOrEmpty(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// Searches for PS3 game code by examining existing save folders and matching game name.
        /// This is a fallback when the code cannot be extracted from ROM path, GameId, or game name.
        /// </summary>
        public static string FindPS3GameCodeByMatchingSaveFolders(IEnumerable<string> savedataPaths, string gameName)
        {
            if (string.IsNullOrWhiteSpace(gameName))
                return null;

            // Normalize game name for matching (remove common words, special chars, etc.)
            var normalizedGameName = NormalizeGameName(gameName);
            if (string.IsNullOrWhiteSpace(normalizedGameName))
                return null;

            // Pattern for PS3 game codes: 4 letters + 5 digits
            // Handles "dirty" folder names like "BCES00052_SAVE_1" or "BLES01459-00-FIXED-"
            // Uses word boundary or checks that code is followed by non-alphanumeric or end of string
            var codePattern = @"\b([A-Z]{4}\d{5})\b|([A-Z]{4}\d{5})(?=[_\-\s]|$|[^A-Z0-9])";
            var codeRegex = new System.Text.RegularExpressions.Regex(codePattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Note: Logging is done via CustomLogger in the calling code, not here
            // This method is static and doesn't have access to logger

            foreach (var savedataPath in savedataPaths)
            {
                if (string.IsNullOrWhiteSpace(savedataPath) || !Directory.Exists(savedataPath))
                    continue;

                try
                {
                    var folders = Directory.GetDirectories(savedataPath);
                    foreach (var folder in folders)
                    {
                        var folderName = Path.GetFileName(folder);
                        if (string.IsNullOrWhiteSpace(folderName))
                            continue;

                        // Extract PS3 code from folder name
                        var codeMatch = codeRegex.Match(folderName);
                        if (!codeMatch.Success)
                            continue;

                        // Group 1 is word boundary match, Group 2 is non-word boundary match
                        var gameCode = codeMatch.Groups[1].Success ? codeMatch.Groups[1].Value : codeMatch.Groups[2].Value;
                        if (string.IsNullOrEmpty(gameCode))
                            continue;
                        gameCode = gameCode.ToUpperInvariant();

                        // Normalize folder name for comparison
                        var normalizedFolderName = NormalizeGameName(folderName);

                        // Check if game name matches folder name (case-insensitive, partial match)
                        if (normalizedFolderName.IndexOf(normalizedGameName, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            normalizedGameName.IndexOf(normalizedFolderName, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return gameCode;
                        }

                        // Also check if folder contains common game name variations
                        if (ContainsGameNameVariations(folderName, normalizedGameName))
                        {
                            return gameCode;
                        }
                    }
                }
                catch
                {
                    // Ignore errors (permissions, etc.)
                }
            }

            return null;
        }

        /// <summary>
        /// Normalizes game name for matching by removing special characters, common words, etc.
        /// Handles "dirty" folder names by removing PS3 codes and common suffixes.
        /// </summary>
        private static string NormalizeGameName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            // First, remove PS3 game codes (e.g., BCES00052, BLES01459) to focus on game name
            var codePattern = @"\b([A-Z]{4}\d{5})\b|([A-Z]{4}\d{5})(?=[_\-\s]|$|[^A-Z0-9])";
            var codeRegex = new System.Text.RegularExpressions.Regex(codePattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            name = codeRegex.Replace(name, "");

            // Remove common save folder suffixes/prefixes
            var saveSuffixes = new[] { "_SAVE", "_SAVEDATA", "_RC1", "_FIXED", "-FIXED", "-00", "_1", "_A", "_B", "_C", "SAVE", "SAVEDATA" };
            foreach (var suffix in saveSuffixes)
            {
                if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - suffix.Length);
                }
                if (name.StartsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(suffix.Length);
                }
            }

            // Remove common prefixes/suffixes and special characters
            var normalized = name
                .Replace("&", "and")
                .Replace(":", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("{", "")
                .Replace("}", "");

            // Remove common words (but keep important game-related words)
            var wordsToRemove = new[] { "the", "a", "an", "of", "or", "but", "in", "on", "at", "to", "for", "with", "from", "future", "tools", "destruction" };
            var words = normalized.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var filteredWords = words.Where(w => !wordsToRemove.Contains(w.ToLowerInvariant()) && w.Length > 1).ToArray();

            // For matching, we want to keep key words like "ratchet", "clank", etc.
            // Join without spaces for substring matching
            return string.Join("", filteredWords).ToLowerInvariant();
        }

        /// <summary>
        /// Checks if folder name contains variations of the game name.
        /// Handles "dirty" folder names by normalizing both before comparison.
        /// </summary>
        private static bool ContainsGameNameVariations(string folderName, string normalizedGameName)
        {
            if (string.IsNullOrWhiteSpace(folderName) || string.IsNullOrWhiteSpace(normalizedGameName))
                return false;

            // Normalize folder name to remove PS3 codes and common suffixes
            var normalizedFolderName = NormalizeGameName(folderName);
            if (string.IsNullOrWhiteSpace(normalizedFolderName))
                return false;

            var folderLower = normalizedFolderName.ToLowerInvariant();
            var gameLower = normalizedGameName.ToLowerInvariant();

            // Direct match
            if (folderLower.Contains(gameLower) || gameLower.Contains(folderLower))
            {
                return true;
            }

            // For long game names, try matching key words (e.g., "ratchetclank" from "Ratchet & Clank Future: Tools of Destruction")
            // Extract key words from game name (words longer than 3 characters)
            var gameWords = normalizedGameName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3)
                .ToList();
            
            if (gameWords.Count > 0)
            {
                // Try matching with first 2-3 key words (e.g., "ratchetclank" from "Ratchet & Clank Future: Tools of Destruction")
                var keyWords = string.Join("", gameWords.Take(Math.Min(3, gameWords.Count)));
                if (keyWords.Length >= 6 && folderLower.Contains(keyWords))
                {
                    return true;
                }
            }

            // Check for common abbreviations and variations
            // For example, "Ratchet & Clank" might be "ratchetclank", "r&c", "ratchetandclank", etc.
            var variations = new List<string> { normalizedGameName };

            // Add variations without spaces
            variations.Add(normalizedGameName.Replace(" ", ""));

            // Add first letters of words (for acronyms)
            var words = normalizedGameName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 1)
            {
                var acronym = string.Join("", words.Select(w => w.Length > 0 ? w[0].ToString() : ""));
                if (acronym.Length >= 2) // Only use acronyms with at least 2 letters
                {
                    variations.Add(acronym);
                }
            }

            // Check each variation against normalized folder name
            foreach (var variation in variations)
            {
                if (string.IsNullOrWhiteSpace(variation) || variation.Length < 3)
                    continue;

                var variationLower = variation.ToLowerInvariant();
                if (folderLower.Contains(variationLower) || variationLower.Contains(folderLower))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Searches for PS3 game code in titleid.txt file by matching game name.
        /// Returns the first matching code found (for backward compatibility).
        /// </summary>
        public static string FindPS3GameCodeInTitleIdFile(string gameName, string titleIdFilePath = null)
        {
            var codes = FindAllPS3GameCodesInTitleIdFile(gameName, titleIdFilePath);
            return codes.FirstOrDefault();
        }

        /// <summary>
        /// Searches for ALL PS3 game codes in titleid.txt file by matching game name.
        /// Returns a list of all matching codes (a game can have multiple regional codes).
        /// </summary>
        public static List<string> FindAllPS3GameCodesInTitleIdFile(string gameName, string titleIdFilePath = null)
        {
            var codes = new List<string>();
            
            if (string.IsNullOrWhiteSpace(gameName))
                return codes;

            // Default path: look for titleid.txt in the same directory as the DLL, then in data/ subdirectory
            if (string.IsNullOrEmpty(titleIdFilePath))
            {
                var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(assemblyLocation))
                {
                    var assemblyDir = Path.GetDirectoryName(assemblyLocation);
                    // Try in DLL directory first
                    titleIdFilePath = Path.Combine(assemblyDir, "titleid.txt");
                    
                    // Try in data/ subdirectory of DLL directory
                    if (!File.Exists(titleIdFilePath))
                    {
                        titleIdFilePath = Path.Combine(assemblyDir, "data", "titleid.txt");
                    }
                    
                    // Also try parent directory (repository root)
                    if (!File.Exists(titleIdFilePath))
                    {
                        var parentDir = Path.GetDirectoryName(assemblyDir);
                        if (!string.IsNullOrEmpty(parentDir))
                        {
                            titleIdFilePath = Path.Combine(parentDir, "titleid.txt");
                        }
                    }
                    
                    // Try parent/data/ directory
                    if (!File.Exists(titleIdFilePath) && !string.IsNullOrEmpty(assemblyDir))
                    {
                        var parentDir = Path.GetDirectoryName(assemblyDir);
                        if (!string.IsNullOrEmpty(parentDir))
                        {
                            titleIdFilePath = Path.Combine(parentDir, "data", "titleid.txt");
                        }
                    }
                }
                
                // Fallback: try current directory
                if (string.IsNullOrEmpty(titleIdFilePath) || !File.Exists(titleIdFilePath))
                {
                    titleIdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "titleid.txt");
                }
                
                // Fallback: try current directory/data/
                if (string.IsNullOrEmpty(titleIdFilePath) || !File.Exists(titleIdFilePath))
                {
                    titleIdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "titleid.txt");
                }
            }

            if (string.IsNullOrEmpty(titleIdFilePath) || !File.Exists(titleIdFilePath))
                return codes;

            try
            {
                var normalizedGameName = NormalizeGameName(gameName);
                if (string.IsNullOrWhiteSpace(normalizedGameName))
                    return codes;

                // Pattern for PS3 game codes: 4 letters + 5 digits at the start of line
                var codePattern = @"^([A-Z]{4}\d{5})\s+(.+)";
                var codeRegex = new System.Text.RegularExpressions.Regex(codePattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                using (var reader = new StreamReader(titleIdFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var match = codeRegex.Match(line);
                        if (!match.Success)
                            continue;

                        var gameCode = match.Groups[1].Value.ToUpperInvariant();
                        var titleName = match.Groups[2].Value.Trim();

                        // Normalize title name from file
                        var normalizedTitleName = NormalizeGameName(titleName);
                        if (string.IsNullOrWhiteSpace(normalizedTitleName))
                            continue;

                        // Check if game name matches title name (case-insensitive, partial match)
                        if (normalizedTitleName.IndexOf(normalizedGameName, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            normalizedGameName.IndexOf(normalizedTitleName, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (!codes.Contains(gameCode))
                                codes.Add(gameCode);
                            continue;
                        }

                        // Also check if title contains common game name variations
                        if (ContainsGameNameVariations(titleName, normalizedGameName))
                        {
                            if (!codes.Contains(gameCode))
                                codes.Add(gameCode);
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors (file access, etc.)
            }

            return codes;
        }

        /// <summary>
        /// Attempts to find PS3 game code by searching existing save folders when direct extraction fails.
        /// This method tries multiple strategies:
        /// 1. Extract code from ROM path, GameId, or game name (existing method)
        /// 2. Search existing save folders and match by game name
        /// 3. Search titleid.txt file for ALL matching codes, then try each code against save folders
        ///    This handles cases where a game has multiple regional codes (e.g., BCES00052, BLES01459, etc.)
        ///    and we need to find which one actually has a save folder on disk.
        /// </summary>
        public static string FindPS3GameCodeWithFallback(EmulationInfo info, string gameName, IEnumerable<string> savedataPaths, string titleIdFilePath = null)
        {
            // First, try the standard extraction method
            var code = ExtractPS3GameCode(info, gameName);
            if (!string.IsNullOrEmpty(code))
            {
                // Verify this code has a save folder
                var saveFolder = FindSaveFolderInMultiplePaths(savedataPaths, code);
                if (!string.IsNullOrEmpty(saveFolder))
                    return code;
            }

            // If that fails, search existing save folders and match by game name
            code = FindPS3GameCodeByMatchingSaveFolders(savedataPaths, gameName);
            if (!string.IsNullOrEmpty(code))
                return code;

            // If that also fails, search titleid.txt file for ALL matching codes
            // Then try each code against save folders to find which one actually exists
            var candidateCodes = FindAllPS3GameCodesInTitleIdFile(gameName, titleIdFilePath);
            if (candidateCodes != null && candidateCodes.Count > 0)
            {
                // Try each candidate code against save folders
                foreach (var candidateCode in candidateCodes)
                {
                    var saveFolder = FindSaveFolderInMultiplePaths(savedataPaths, candidateCode);
                    if (!string.IsNullOrEmpty(saveFolder))
                    {
                        // Found a code that matches an existing save folder!
                        return candidateCode;
                    }
                }
                
                // If no save folder found, return the first candidate code anyway
                // (the save folder might be created later, or the user might need to create it)
                return candidateCodes.First();
            }

            return null;
        }
    }

    internal static class EmulatedSaveTemplate
    {
        /// <summary>
        /// Main entry point for resolving save paths. Delegates to per-emulator save finders when available.
        /// </summary>
        public static List<string> ResolveMany(EmulationInfo info, string templates, string gameName = null, string customRpcs3Path = null, Dictionary<string, string> emulatorCustomPaths = null)
        {
            var resolved = new List<string>();
            if (string.IsNullOrWhiteSpace(templates))
            {
                return resolved;
            }

            var emulatorLower = ((info?.EmulatorName ?? "") + " " + (info?.EmulatorExecutablePath ?? "")).ToLowerInvariant();

            // --- RPCS3 (PlayStation 3) ---
            if (emulatorLower.Contains("rpcs3"))
            {
                var rpcs3CustomPath = GetCustomPathForEmulator("rpcs3", emulatorCustomPaths) ?? customRpcs3Path;
                var searchPaths = RPCS3SaveFinder.GetCommonRPCS3SavePaths(info?.EmulatorExecutablePath, rpcs3CustomPath);
                var gameCode = RPCS3SaveFinder.FindPS3GameCodeWithFallback(info, gameName, searchPaths);

                if (!string.IsNullOrEmpty(gameCode))
                {
                    var saveFolder = RPCS3SaveFinder.FindSaveFolderInMultiplePaths(searchPaths, gameCode);
                    if (!string.IsNullOrEmpty(saveFolder))
                    {
                        resolved.Add(saveFolder);
                        return resolved;
                    }
                }
                return resolved;
            }

            // --- Ryujinx (Nintendo Switch) ---
            if (emulatorLower.Contains("ryujinx"))
            {
                var ryujinxCustomPath = GetCustomPathForEmulator("ryujinx", emulatorCustomPaths);
                var found = RyujinxSaveFinder.FindSavePaths(info, gameName, ryujinxCustomPath);
                // Always return here — never fall through to template resolution
                // which would resolve to <appData>/Ryujinx/bis/user/save/** and back up ALL saves
                return found;
            }

            // --- Yuzu (Nintendo Switch) ---
            if (emulatorLower.Contains("yuzu"))
            {
                var yuzuCustomPath = GetCustomPathForEmulator("yuzu", emulatorCustomPaths);
                var found = YuzuSaveFinder.FindSavePaths(info, gameName, yuzuCustomPath);
                // Always return here — never fall through to template resolution
                return found;
            }

            // --- PPSSPP (PlayStation Portable) ---
            if (emulatorLower.Contains("ppsspp"))
            {
                var ppssppCustomPath = GetCustomPathForEmulator("ppsspp", emulatorCustomPaths);
                var found = PPSSPPSaveFinder.FindSavePaths(info, gameName, ppssppCustomPath);
                if (found.Count > 0)
                    return found;
                // Fall through to template resolution if active finder fails
            }

            // --- Dolphin (GameCube / Wii) ---
            if (emulatorLower.Contains("dolphin"))
            {
                var dolphinCustomPath = GetCustomPathForEmulator("dolphin", emulatorCustomPaths);
                var found = DolphinSaveFinder.FindSavePaths(info, gameName, dolphinCustomPath);
                if (found.Count > 0)
                    return found;
                // Fall through to template resolution
            }

            // --- Citra (Nintendo 3DS) ---
            if (emulatorLower.Contains("citra"))
            {
                var citraCustomPath = GetCustomPathForEmulator("citra", emulatorCustomPaths);
                var found = CitraSaveFinder.FindSavePaths(info, gameName, citraCustomPath);
                if (found.Count > 0)
                    return found;
            }

            // --- Standard template resolution for all other emulators ---
            foreach (var line in templates.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    continue;
                }
                resolved.Add(ResolveOne(info, trimmed));
            }

            return resolved.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }

        private static string GetCustomPathForEmulator(string emulatorKey, Dictionary<string, string> emulatorCustomPaths)
        {
            if (emulatorCustomPaths == null) return null;
            foreach (var kvp in emulatorCustomPaths)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Key) &&
                    kvp.Key.Trim().ToLowerInvariant().Contains(emulatorKey) &&
                    !string.IsNullOrWhiteSpace(kvp.Value))
                {
                    return kvp.Value.Trim();
                }
            }
            return null;
        }

        public static string ResolveOne(EmulationInfo info, string template)
        {
            var romPath = info.RomPath ?? "";
            var romDir = "";
            var romBase = "";
            try
            {
                if (!string.IsNullOrEmpty(romPath))
                {
                    romDir = Path.GetDirectoryName(romPath) ?? "";
                    romBase = Path.GetFileNameWithoutExtension(romPath) ?? "";
                }
            }
            catch
            { }

            var emulatorExe = info.EmulatorExecutablePath ?? "";
            var emulatorDir = "";
            try
            {
                if (!string.IsNullOrEmpty(emulatorExe))
                {
                    emulatorDir = Path.GetDirectoryName(emulatorExe) ?? "";
                }
            }
            catch
            { }

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var gameId = info.GameId ?? "";

            return template
                .Replace("<romPath>", romPath)
                .Replace("<romDir>", romDir)
                .Replace("<romBase>", romBase)
                .Replace("<emulatorExe>", emulatorExe)
                .Replace("<emulatorDir>", emulatorDir)
                .Replace("<appData>", appData)
                .Replace("<userProfile>", userProfile)
                .Replace("<gameId>", gameId);
        }
    }

    /// <summary>
    /// Save finder for PPSSPP (PlayStation Portable).
    /// PSP saves use game codes like ULUS10041, UCUS98645, etc.
    /// Saves are stored in memstick/PSP/SAVEDATA/{gamecode}*/ folders.
    /// </summary>
    internal static class PPSSPPSaveFinder
    {
        private static readonly System.Text.RegularExpressions.Regex PSP_CODE_REGEX =
            new System.Text.RegularExpressions.Regex(@"\b(U[CLJ][AEUJ]S\d{5})\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        public static List<string> FindSavePaths(EmulationInfo info, string gameName, string customPath = null)
        {
            var results = new List<string>();

            // Try to extract game code from ROM path, GameId, game name
            var gameCode = ExtractPSPGameCode(info, gameName);

            // Get all possible SAVEDATA base paths
            var savedataPaths = GetSavedataPaths(info?.EmulatorExecutablePath, customPath);

            if (!string.IsNullOrEmpty(gameCode))
            {
                foreach (var basePath in savedataPaths)
                {
                    if (!Directory.Exists(basePath)) continue;
                    try
                    {
                        foreach (var dir in Directory.GetDirectories(basePath))
                        {
                            var folderName = Path.GetFileName(dir);
                            if (folderName.IndexOf(gameCode, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                results.Add(dir);
                            }
                        }
                    }
                    catch { }
                }
            }

            return results;
        }

        public static string ExtractPSPGameCode(EmulationInfo info, string gameName)
        {
            // Try ROM path
            if (!string.IsNullOrEmpty(info?.RomPath))
            {
                var match = PSP_CODE_REGEX.Match(Path.GetFileNameWithoutExtension(info.RomPath));
                if (match.Success) return match.Groups[1].Value.ToUpperInvariant();
            }
            // Try GameId
            if (!string.IsNullOrEmpty(info?.GameId))
            {
                var match = PSP_CODE_REGEX.Match(info.GameId);
                if (match.Success) return match.Groups[1].Value.ToUpperInvariant();
            }
            // Try game name
            if (!string.IsNullOrEmpty(gameName))
            {
                var match = PSP_CODE_REGEX.Match(gameName);
                if (match.Success) return match.Groups[1].Value.ToUpperInvariant();
            }
            return null;
        }

        public static List<string> GetSavedataPaths(string emulatorPath = null, string customPath = null)
        {
            var paths = new List<string>();
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Custom path first
            if (!string.IsNullOrWhiteSpace(customPath))
            {
                AddSavedataIfExists(paths, customPath);
            }

            // Emulator directory (portable)
            if (!string.IsNullOrWhiteSpace(emulatorPath))
            {
                var emuDir = emulatorPath;
                try { if (File.Exists(emuDir)) emuDir = Path.GetDirectoryName(emuDir); } catch { }
                if (!string.IsNullOrEmpty(emuDir))
                {
                    AddSavedataIfExists(paths, Path.Combine(emuDir, "memstick", "PSP", "SAVEDATA"));
                }
            }

            // Common locations
            AddSavedataIfExists(paths, Path.Combine(userProfile, "Documents", "PPSSPP", "memstick", "PSP", "SAVEDATA"));
            AddSavedataIfExists(paths, Path.Combine(appData, "PPSSPP", "PSP", "SAVEDATA"));

            return paths.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static void AddSavedataIfExists(List<string> paths, string path)
        {
            try
            {
                if (Directory.Exists(path))
                    paths.Add(path);
            }
            catch { }
        }
    }

    /// <summary>
    /// Save finder for Dolphin (GameCube / Wii).
    /// GC saves are stored by region and game ID (e.g., GC/USA/Card A/GALE01/).
    /// Wii saves are in Wii/title/{titleId}/.
    /// </summary>
    internal static class DolphinSaveFinder
    {
        private static readonly System.Text.RegularExpressions.Regex GCN_CODE_REGEX =
            new System.Text.RegularExpressions.Regex(@"\b([A-Z0-9]{4}[A-Z0-9]{2})\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        public static List<string> FindSavePaths(EmulationInfo info, string gameName, string customPath = null)
        {
            var results = new List<string>();
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var basePaths = new List<string>();
            if (!string.IsNullOrWhiteSpace(customPath))
                basePaths.Add(customPath);

            // Emulator directory (portable)
            if (!string.IsNullOrWhiteSpace(info?.EmulatorExecutablePath))
            {
                var emuDir = info.EmulatorExecutablePath;
                try { if (File.Exists(emuDir)) emuDir = Path.GetDirectoryName(emuDir); } catch { }
                if (!string.IsNullOrEmpty(emuDir))
                {
                    basePaths.Add(Path.Combine(emuDir, "User"));
                }
            }

            basePaths.Add(Path.Combine(userProfile, "Documents", "Dolphin Emulator"));

            // Search GC memory card directories and Wii title directories
            foreach (var basePath in basePaths)
            {
                // GC saves: GC/<Region>/Card A/<GameCode>/ or GC/<Region>/<GameCode>.*
                var gcDir = Path.Combine(basePath, "GC");
                if (Directory.Exists(gcDir))
                {
                    results.Add(gcDir);
                }

                // Wii saves: Wii/title/<titlePath>/
                var wiiDir = Path.Combine(basePath, "Wii", "title");
                if (Directory.Exists(wiiDir))
                {
                    results.Add(wiiDir);
                }

                // StateSaves directory
                var stateDir = Path.Combine(basePath, "StateSaves");
                if (Directory.Exists(stateDir))
                {
                    results.Add(stateDir);
                }
            }

            return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }

    /// <summary>
    /// Save finder for Citra (Nintendo 3DS).
    /// 3DS saves are stored in sdmc/Nintendo 3DS/{randomId}/{randomId}/title/{titleIdHigh}/{titleIdLow}/
    /// </summary>
    internal static class CitraSaveFinder
    {
        public static List<string> FindSavePaths(EmulationInfo info, string gameName, string customPath = null)
        {
            var results = new List<string>();
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var basePaths = new List<string>();
            if (!string.IsNullOrWhiteSpace(customPath))
                basePaths.Add(customPath);

            basePaths.Add(Path.Combine(appData, "Citra"));

            // Emulator directory (portable)
            if (!string.IsNullOrWhiteSpace(info?.EmulatorExecutablePath))
            {
                var emuDir = info.EmulatorExecutablePath;
                try { if (File.Exists(emuDir)) emuDir = Path.GetDirectoryName(emuDir); } catch { }
                if (!string.IsNullOrEmpty(emuDir))
                {
                    basePaths.Add(Path.Combine(emuDir, "user"));
                }
            }

            foreach (var basePath in basePaths)
            {
                // sdmc/Nintendo 3DS/{id}/{id}/title/ contains actual saves
                var sdmcDir = Path.Combine(basePath, "sdmc", "Nintendo 3DS");
                if (Directory.Exists(sdmcDir))
                {
                    try
                    {
                        // Look for the nested random ID dirs
                        foreach (var id1 in Directory.GetDirectories(sdmcDir))
                        {
                            foreach (var id2 in Directory.GetDirectories(id1))
                            {
                                var titleDir = Path.Combine(id2, "title");
                                if (Directory.Exists(titleDir))
                                {
                                    results.Add(titleDir);
                                }
                            }
                        }
                    }
                    catch { }
                }

                // Also check nand/data for system saves
                var nandDir = Path.Combine(basePath, "nand", "data");
                if (Directory.Exists(nandDir))
                {
                    results.Add(nandDir);
                }
            }

            return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }

    /// <summary>
    /// Save finder for Ryujinx (Nintendo Switch).
    /// Ryujinx stores saves in bis/user/save/{saveId}/ folders.
    /// Each save folder has an ExtraData0 binary file whose first 8 bytes
    /// (little-endian u64) contain the Title ID of the game.
    /// The finder extracts the game's Title ID from the ROM filename,
    /// GameId, or by scanning Ryujinx's games/ folder, then matches it
    /// against ExtraData0 to return only the correct save folder(s).
    /// </summary>
    internal static class RyujinxSaveFinder
    {
        // Nintendo Switch Title IDs are 16 hex digits
        private static readonly System.Text.RegularExpressions.Regex TITLE_ID_REGEX =
            new System.Text.RegularExpressions.Regex(@"\b([0-9a-fA-F]{16})\b", System.Text.RegularExpressions.RegexOptions.None);

        // Title IDs in brackets like [0100152000022000]
        private static readonly System.Text.RegularExpressions.Regex TITLE_ID_BRACKET_REGEX =
            new System.Text.RegularExpressions.Regex(@"\[([0-9a-fA-F]{16})\]", System.Text.RegularExpressions.RegexOptions.None);

        public static List<string> FindSavePaths(EmulationInfo info, string gameName, string customPath = null)
        {
            var results = new List<string>();

            var titleId = ExtractTitleId(info, gameName);
            if (string.IsNullOrEmpty(titleId))
                return results;

            // Parse Title ID as ulong for binary comparison
            if (!ulong.TryParse(titleId, System.Globalization.NumberStyles.HexNumber, null, out ulong targetTid))
                return results;

            // Get the Ryujinx save root(s)
            var saveRoots = GetSaveRoots(info?.EmulatorExecutablePath, customPath);

            foreach (var saveRoot in saveRoots)
            {
                if (!Directory.Exists(saveRoot)) continue;
                try
                {
                    foreach (var saveDir in Directory.GetDirectories(saveRoot))
                    {
                        var extraData = Path.Combine(saveDir, "ExtraData0");
                        if (!File.Exists(extraData)) continue;

                        try
                        {
                            var bytes = File.ReadAllBytes(extraData);
                            if (bytes.Length < 8) continue;

                            var saveTid = BitConverter.ToUInt64(bytes, 0);
                            if (saveTid == targetTid)
                            {
                                results.Add(saveDir);
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }

            return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static string ExtractTitleId(EmulationInfo info, string gameName)
        {
            // 1. Try bracketed Title ID from ROM filename: "Game Name [0100152000022000].nsp"
            if (!string.IsNullOrEmpty(info?.RomPath))
            {
                var fileName = Path.GetFileNameWithoutExtension(info.RomPath);
                var match = TITLE_ID_BRACKET_REGEX.Match(fileName);
                if (match.Success)
                    return match.Groups[1].Value.ToUpperInvariant();

                // Also try unbracketed 16-hex-digit pattern in filename
                match = TITLE_ID_REGEX.Match(fileName);
                if (match.Success && LooksLikeTitleId(match.Groups[1].Value))
                    return match.Groups[1].Value.ToUpperInvariant();
            }

            // 2. Try GameId (Playnite may store the Title ID here)
            if (!string.IsNullOrEmpty(info?.GameId))
            {
                var match = TITLE_ID_REGEX.Match(info.GameId);
                if (match.Success && LooksLikeTitleId(match.Groups[1].Value))
                    return match.Groups[1].Value.ToUpperInvariant();
            }

            // 3. Try to find in the ROM path directory structure
            //    Some users organize ROMs in folders named by Title ID
            if (!string.IsNullOrEmpty(info?.RomPath))
            {
                try
                {
                    var dirPath = Path.GetDirectoryName(info.RomPath) ?? "";
                    foreach (var segment in dirPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                    {
                        var match = TITLE_ID_REGEX.Match(segment);
                        if (match.Success && LooksLikeTitleId(match.Groups[1].Value))
                            return match.Groups[1].Value.ToUpperInvariant();
                    }
                }
                catch { }
            }

            // 4. Try to match game name against Ryujinx's metadata.json files
            //    Each games/{TitleID}/gui/metadata.json contains a "title" field
            if (!string.IsNullOrEmpty(gameName))
            {
                var metadataMatch = MatchTitleIdFromRyujinxMetadata(gameName, info?.EmulatorExecutablePath);
                if (!string.IsNullOrEmpty(metadataMatch))
                    return metadataMatch;
            }

            return null;
        }

        /// <summary>
        /// Quick heuristic: Switch Title IDs start with 01 and end with 000.
        /// This helps filter out random 16-hex strings.
        /// </summary>
        private static bool LooksLikeTitleId(string hex)
        {
            if (hex.Length != 16) return false;
            var upper = hex.ToUpperInvariant();
            // Most (but not all) game Title IDs start with "01" and end with "000"
            // We relax slightly: just check it starts with "01" or "05" (system/game prefixes)
            return upper.StartsWith("01") || upper.StartsWith("05");
        }

        public static List<string> GetSaveRoots(string emulatorPath = null, string customPath = null)
        {
            var paths = new List<string>();
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (!string.IsNullOrWhiteSpace(customPath))
            {
                paths.Add(customPath);
            }

            // Standard Ryujinx save location
            paths.Add(Path.Combine(appData, "Ryujinx", "bis", "user", "save"));

            // Portable mode: emulator directory
            if (!string.IsNullOrWhiteSpace(emulatorPath))
            {
                var emuDir = emulatorPath;
                try { if (File.Exists(emuDir)) emuDir = Path.GetDirectoryName(emuDir); } catch { }
                if (!string.IsNullOrEmpty(emuDir))
                {
                    paths.Add(Path.Combine(emuDir, "bis", "user", "save"));
                }
            }

            return paths.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// Scans Ryujinx games/{TitleID}/gui/metadata.json files to find a Title ID
        /// that matches the given game name. Uses normalized comparison with fallback
        /// to containment matching.
        /// </summary>
        public static string MatchTitleIdFromRyujinxMetadata(string gameName, string emulatorPath = null)
        {
            if (string.IsNullOrWhiteSpace(gameName))
                return null;

            var gamesRoots = GetGamesRoots(emulatorPath);
            var normalizedGame = NormalizeForComparison(gameName);

            string bestMatch = null;
            int bestScore = int.MaxValue;

            foreach (var gamesRoot in gamesRoots)
            {
                if (!Directory.Exists(gamesRoot)) continue;

                try
                {
                    foreach (var titleDir in Directory.GetDirectories(gamesRoot))
                    {
                        var dirName = Path.GetFileName(titleDir);
                        if (string.IsNullOrEmpty(dirName) || dirName.Length != 16) continue;
                        if (!LooksLikeTitleId(dirName)) continue;

                        var metadataPath = Path.Combine(titleDir, "gui", "metadata.json");
                        if (!File.Exists(metadataPath)) continue;

                        try
                        {
                            var json = File.ReadAllText(metadataPath, Encoding.UTF8);
                            var obj = JObject.Parse(json);
                            var title = obj["title"]?.ToString();
                            if (string.IsNullOrWhiteSpace(title)) continue;

                            var normalizedTitle = NormalizeForComparison(title);

                            // Exact normalized match
                            if (normalizedGame == normalizedTitle)
                                return dirName.ToUpperInvariant();

                            // Check if one contains the other (for subtitle differences)
                            if (normalizedGame.Contains(normalizedTitle) || normalizedTitle.Contains(normalizedGame))
                            {
                                // Prefer shorter distance (closer match)
                                int score = Math.Abs(normalizedGame.Length - normalizedTitle.Length);
                                if (score < bestScore)
                                {
                                    bestScore = score;
                                    bestMatch = dirName.ToUpperInvariant();
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }

            return bestMatch;
        }

        /// <summary>
        /// Gets the Ryujinx games/ folder root(s) where Title ID subfolders live.
        /// </summary>
        private static List<string> GetGamesRoots(string emulatorPath = null)
        {
            var paths = new List<string>();
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            paths.Add(Path.Combine(appData, "Ryujinx", "games"));

            if (!string.IsNullOrWhiteSpace(emulatorPath))
            {
                var emuDir = emulatorPath;
                try { if (File.Exists(emuDir)) emuDir = Path.GetDirectoryName(emuDir); } catch { }
                if (!string.IsNullOrEmpty(emuDir))
                {
                    paths.Add(Path.Combine(emuDir, "games"));
                }
            }

            return paths.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// Normalizes a game name for comparison: lowercase, remove diacritics,
        /// strip punctuation and extra whitespace.
        /// </summary>
        private static string NormalizeForComparison(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            // Lowercase
            var s = input.ToLowerInvariant();

            // Remove diacritics (é→e, ü→u, etc.)
            var normalized = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (var c in normalized)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            s = sb.ToString().Normalize(NormalizationForm.FormC);

            // Strip non-alphanumeric (keep letters, digits, spaces)
            sb.Clear();
            foreach (var c in s)
            {
                if (char.IsLetterOrDigit(c) || c == ' ')
                    sb.Append(c);
            }
            s = sb.ToString();

            // Collapse whitespace
            while (s.Contains("  "))
                s = s.Replace("  ", " ");

            return s.Trim();
        }
    }

    /// <summary>
    /// Save finder for Yuzu (Nintendo Switch).
    /// Yuzu stores saves in nand/user/save/{saveId}/ folders.
    /// Same ExtraData0 format as Ryujinx (first 8 bytes = Title ID as little-endian u64).
    /// </summary>
    internal static class YuzuSaveFinder
    {
        public static List<string> FindSavePaths(EmulationInfo info, string gameName, string customPath = null)
        {
            var results = new List<string>();

            var titleId = RyujinxSaveFinder.ExtractTitleId(info, gameName);
            if (string.IsNullOrEmpty(titleId))
                return results;

            if (!ulong.TryParse(titleId, System.Globalization.NumberStyles.HexNumber, null, out ulong targetTid))
                return results;

            var saveRoots = GetSaveRoots(info?.EmulatorExecutablePath, customPath);

            foreach (var saveRoot in saveRoots)
            {
                if (!Directory.Exists(saveRoot)) continue;
                try
                {
                    foreach (var saveDir in Directory.GetDirectories(saveRoot))
                    {
                        var extraData = Path.Combine(saveDir, "ExtraData0");
                        if (!File.Exists(extraData)) continue;

                        try
                        {
                            var bytes = File.ReadAllBytes(extraData);
                            if (bytes.Length < 8) continue;

                            var saveTid = BitConverter.ToUInt64(bytes, 0);
                            if (saveTid == targetTid)
                            {
                                results.Add(saveDir);
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }

            return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static List<string> GetSaveRoots(string emulatorPath = null, string customPath = null)
        {
            var paths = new List<string>();
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (!string.IsNullOrWhiteSpace(customPath))
            {
                paths.Add(customPath);
            }

            // Standard Yuzu save location
            paths.Add(Path.Combine(appData, "yuzu", "nand", "user", "save"));

            // Portable mode
            if (!string.IsNullOrWhiteSpace(emulatorPath))
            {
                var emuDir = emulatorPath;
                try { if (File.Exists(emuDir)) emuDir = Path.GetDirectoryName(emuDir); } catch { }
                if (!string.IsNullOrEmpty(emuDir))
                {
                    paths.Add(Path.Combine(emuDir, "nand", "user", "save"));
                }
            }

            return paths.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
