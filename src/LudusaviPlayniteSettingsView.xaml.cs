using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LudusaviPlaynite
{
    public partial class LudusaviPlayniteSettingsView : UserControl
    {
        private LudusaviPlaynite plugin;
        private Translator translator;

        public LudusaviPlayniteSettingsView(LudusaviPlaynite plugin, Translator translator)
        {
            InitializeComponent();
            this.plugin = plugin;
            this.translator = translator;
        }

        public void OnBrowseExecutablePath(object sender, RoutedEventArgs e)
        {
            var choice = this.plugin.PlayniteApi.Dialogs.SelectFile(translator.SelectFileExecutableFilter());
            if (choice.Length > 0)
            {
                this.plugin.settings.ExecutablePath = choice;
            }
        }

        public void OnBrowseBackupPath(object sender, RoutedEventArgs e)
        {
            var choice = this.plugin.PlayniteApi.Dialogs.SelectFolder();
            if (choice.Length > 0)
            {
                this.plugin.settings.BackupPath = choice;
            }
        }

        public void OnOpenBackupPath(object sender, RoutedEventArgs e)
        {
            if (!Etc.OpenDir(plugin.settings.BackupPath))
            {
                this.plugin.interactor.ShowError(this.translator.CannotOpenFolder());
            }
        }

        public void OnLoadDefaultMappings(object sender, RoutedEventArgs e)
        {
            var result = this.plugin.PlayniteApi.Dialogs.ShowMessage(
                translator.LoadDefaultMappings_Confirm(),
                "",
                MessageBoxButton.YesNo
            );

            if (result == MessageBoxResult.Yes)
            {
                // Get emulators configured in Playnite
                var playniteEmulators = this.plugin.PlayniteApi.Database.Emulators;
                if (playniteEmulators == null || !playniteEmulators.Any())
                {
                    this.plugin.PlayniteApi.Dialogs.ShowMessage(
                        "No emulators configured in Playnite. Please configure emulators first in Library → Configure Emulators.",
                        "Ludusavi",
                        MessageBoxButton.OK
                    );
                    return;
                }

                var defaults = DefaultEmulatorMappings.GetDefaults();
                var existingMatches = new HashSet<string>(
                    this.plugin.settings.EmulatedSaveMappings
                        .Select(m => (m.EmulatorMatch ?? "").ToLowerInvariant())
                );

                int addedCount = 0;
                foreach (var emulator in playniteEmulators)
                {
                    // Find matching default mapping for this emulator
                    var emulatorNameLower = (emulator.Name ?? "").ToLowerInvariant();
                    var emulatorInstallDirLower = (emulator.InstallDir ?? "").ToLowerInvariant();

                    foreach (var defaultMapping in defaults)
                    {
                        var matchKeyLower = (defaultMapping.EmulatorMatch ?? "").ToLowerInvariant();
                        
                        // Check if emulator name or install dir contains the match key
                        if (emulatorNameLower.Contains(matchKeyLower) || emulatorInstallDirLower.Contains(matchKeyLower))
                        {
                            // Use emulator name as the match key for better accuracy
                            var actualMatchKey = emulator.Name.ToLowerInvariant();
                            
                            if (!existingMatches.Contains(actualMatchKey) && !existingMatches.Contains(matchKeyLower))
                            {
                                var newMapping = defaultMapping.Clone();
                                newMapping.EmulatorMatch = emulator.Name; // Use exact emulator name
                                this.plugin.settings.EmulatedSaveMappings.Add(newMapping);
                                existingMatches.Add(actualMatchKey);
                                addedCount++;
                            }
                            break; // Found a match, move to next emulator
                        }
                    }
                }

                this.plugin.PlayniteApi.Dialogs.ShowMessage(
                    $"Added {addedCount} emulator mapping(s) based on your Playnite emulator configuration.",
                    "Ludusavi",
                    MessageBoxButton.OK
                );
            }
        }

        public void OnClearAllMappings(object sender, RoutedEventArgs e)
        {
            var result = this.plugin.PlayniteApi.Dialogs.ShowMessage(
                translator.ClearAllMappings_Confirm(),
                "",
                MessageBoxButton.YesNo
            );

            if (result == MessageBoxResult.Yes)
            {
                this.plugin.settings.EmulatedSaveMappings.Clear();
            }
        }
    }
}
