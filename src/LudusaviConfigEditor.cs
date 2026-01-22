using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace LudusaviPlaynite
{
    internal static class LudusaviConfigEditor
    {
        public static bool UpsertCustomGame(string executablePathSetting, string gameName, IEnumerable<string> files, out string error)
        {
            error = null;
            try
            {
                if (string.IsNullOrWhiteSpace(gameName))
                {
                    error = "Game name is empty.";
                    return false;
                }

                var list = (files ?? Enumerable.Empty<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim()
                        .Replace("\\\\", "\\")  // Fix double backslashes
                        .Replace("\\", "/"))    // Convert to forward slashes for YAML compatibility
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!list.Any())
                {
                    error = "No save paths were provided.";
                    return false;
                }

                var configPath = GetConfigPath(executablePathSetting);
                Directory.CreateDirectory(Path.GetDirectoryName(configPath));

                var yaml = LoadOrCreate(configPath);

                var root = (YamlMappingNode)yaml.Documents[0].RootNode;
                var customGames = EnsureSequence(root, "customGames");

                var existing = FindCustomGame(customGames, gameName);
                if (existing == null)
                {
                    existing = new YamlMappingNode();
                    customGames.Add(existing);
                }

                // IMPORTANT: Set name as the primary identifier - this is what Ludusavi uses to identify the game
                // Do NOT use alias - always put the game name in the "name" field
                existing.Children[new YamlScalarNode("name")] = new YamlScalarNode(gameName);
                existing.Children[new YamlScalarNode("integration")] = new YamlScalarNode("override");

                // Remove alias completely if it exists - we want Ludusavi to use "name" not "alias"
                var aliasKey = new YamlScalarNode("alias");
                if (existing.Children.ContainsKey(aliasKey))
                {
                    existing.Children.Remove(aliasKey);
                }

                var filesNode = new YamlSequenceNode(list.Select(x => (YamlNode)new YamlScalarNode(x)));
                existing.Children[new YamlScalarNode("files")] = filesNode;

                // Ensure other required fields are present
                if (!existing.Children.ContainsKey(new YamlScalarNode("registry")))
                {
                    existing.Children[new YamlScalarNode("registry")] = new YamlSequenceNode();
                }
                if (!existing.Children.ContainsKey(new YamlScalarNode("installDir")))
                {
                    existing.Children[new YamlScalarNode("installDir")] = new YamlSequenceNode();
                }

                using (var writer = new StreamWriter(configPath, false, new UTF8Encoding(false)))
                {
                    yaml.Save(writer, false);
                }

                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        private static string GetConfigPath(string executablePathSetting)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(executablePathSetting))
                {
                    var candidate = executablePathSetting;
                    if (File.Exists(candidate))
                    {
                        var dir = Path.GetDirectoryName(candidate);
                        var portableMarker = Path.Combine(dir, "ludusavi.portable");
                        if (File.Exists(portableMarker))
                        {
                            return Path.Combine(dir, "config.yaml");
                        }
                    }
                }
            }
            catch
            { }

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "ludusavi", "config.yaml");
        }

        private static YamlStream LoadOrCreate(string configPath)
        {
            var yaml = new YamlStream();

            if (File.Exists(configPath))
            {
                var text = File.ReadAllText(configPath);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    using (var reader = new StringReader(text))
                    {
                        yaml.Load(reader);
                    }
                    if (yaml.Documents.Count > 0 && yaml.Documents[0].RootNode is YamlMappingNode)
                    {
                        return yaml;
                    }
                }
            }

            yaml.Add(new YamlDocument(new YamlMappingNode()));
            return yaml;
        }

        private static YamlSequenceNode EnsureSequence(YamlMappingNode root, string key)
        {
            var k = new YamlScalarNode(key);

            if (root.Children.TryGetValue(k, out var node) && node is YamlSequenceNode seq)
            {
                return seq;
            }

            var created = new YamlSequenceNode();
            root.Children[k] = created;
            return created;
        }

        private static YamlMappingNode FindCustomGame(YamlSequenceNode customGames, string name)
        {
            foreach (var item in customGames.Children)
            {
                if (item is YamlMappingNode map)
                {
                    if (map.Children.TryGetValue(new YamlScalarNode("name"), out var n) && n is YamlScalarNode scalar)
                    {
                        if (string.Equals(scalar.Value, name, StringComparison.Ordinal))
                        {
                            return map;
                        }
                    }
                }
            }
            return null;
        }

        public static bool CustomGameExists(string executablePathSetting, string gameName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gameName))
                    return false;

                var configPath = GetConfigPath(executablePathSetting);
                if (!File.Exists(configPath))
                    return false;

                var yaml = LoadOrCreate(configPath);
                if (yaml.Documents.Count == 0)
                    return false;

                var root = (YamlMappingNode)yaml.Documents[0].RootNode;
                var customGames = EnsureSequence(root, "customGames");

                var exists = FindCustomGame(customGames, gameName) != null;
                return exists;
            }
            catch (Exception)
            {
                // If there's any error reading the config, assume the game doesn't exist
                // This prevents corruption attempts
                return false;
            }
        }
    }
}
