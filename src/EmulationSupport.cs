using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    SavePathTemplates = "<emulatorDir>/dev_hdd0/home/00000001/savedata/<gameId>*/**"
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
        /// <summary>
        /// Extracts PS3 game code from various sources (ROM path, game name, game ID).
        /// PS3 codes are typically: BLES, BLUS, BCES, BCUS, NPEA, NPUA, etc. followed by 5 digits.
        /// </summary>
        public static string ExtractPS3GameCode(EmulationInfo info, string gameName)
        {
            // Pattern for PS3 game codes: 4 letters + 5 digits
            var pattern = @"\b([A-Z]{4}\d{5})\b";
            var regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Try to extract from ROM path first (most reliable)
            if (!string.IsNullOrEmpty(info?.RomPath))
            {
                var romName = Path.GetFileNameWithoutExtension(info.RomPath);
                var match = regex.Match(romName);
                if (match.Success) return match.Groups[1].Value.ToUpperInvariant();
            }

            // Try from GameId
            if (!string.IsNullOrEmpty(info?.GameId))
            {
                var match = regex.Match(info.GameId);
                if (match.Success) return match.Groups[1].Value.ToUpperInvariant();
            }

            // Try from game name
            if (!string.IsNullOrEmpty(gameName))
            {
                var match = regex.Match(gameName);
                if (match.Success) return match.Groups[1].Value.ToUpperInvariant();
            }

            return null;
        }

        /// <summary>
        /// Finds the save folder for a PS3 game by searching for folders that contain the game code.
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
                var folders = Directory.GetDirectories(savedataBasePath);
                foreach (var folder in folders)
                {
                    var folderName = Path.GetFileName(folder);
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

            // Custom path first (highest priority)
            if (!string.IsNullOrEmpty(customPath))
            {
                paths.Add(customPath);
            }

            // Emulator directory (if provided)
            if (!string.IsNullOrEmpty(emulatorDir))
            {
                paths.Add(Path.Combine(emulatorDir, "dev_hdd0", "home", "00000001", "savedata"));
            }

            // Common locations
            paths.Add(Path.Combine(userProfile, "RPCS3", "dev_hdd0", "home", "00000001", "savedata"));
            paths.Add(Path.Combine(userProfile, "Documents", "RPCS3", "dev_hdd0", "home", "00000001", "savedata"));
            paths.Add(@"C:\RPCS3\dev_hdd0\home\00000001\savedata");

            return paths.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList();
        }
    }

    internal static class EmulatedSaveTemplate
    {
        public static List<string> ResolveMany(EmulationInfo info, string templates, string gameName = null, string customRpcs3Path = null)
        {
            var resolved = new List<string>();
            if (string.IsNullOrWhiteSpace(templates))
            {
                return resolved;
            }

            // Special handling for RPCS3
            bool isRpcs3 = (info?.EmulatorName ?? "").ToLowerInvariant().Contains("rpcs3") ||
                           (info?.EmulatorExecutablePath ?? "").ToLowerInvariant().Contains("rpcs3");

            if (isRpcs3)
            {
                var gameCode = RPCS3SaveFinder.ExtractPS3GameCode(info, gameName);
                if (!string.IsNullOrEmpty(gameCode))
                {
                    var searchPaths = RPCS3SaveFinder.GetCommonRPCS3SavePaths(info?.EmulatorExecutablePath, customRpcs3Path);
                    var saveFolder = RPCS3SaveFinder.FindSaveFolderInMultiplePaths(searchPaths, gameCode);
                    
                    if (!string.IsNullOrEmpty(saveFolder))
                    {
                        resolved.Add(saveFolder);
                        return resolved;
                    }
                }
            }

            // Standard template resolution
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
}


