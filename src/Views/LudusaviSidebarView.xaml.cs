using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace LudusaviPlaynite.Views
{
    public partial class LudusaviSidebarView : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly LudusaviPlaynite plugin;
        private ObservableCollection<BackedUpGameItem> gameItems;
        private List<BackedUpGameItem> allGameItems;

        public LudusaviSidebarView(LudusaviPlaynite plugin)
        {
            InitializeComponent();
            this.plugin = plugin;
            gameItems = new ObservableCollection<BackedUpGameItem>();
            allGameItems = new List<BackedUpGameItem>();
            GamesItemsControl.ItemsSource = gameItems;

            UpdateVersionText();
            LoadBackedUpGames();
        }

        private void UpdateVersionText()
        {
            try
            {
                var version = plugin.app?.version?.inner;
                VersionText.Text = version != null && version != new Version(0, 0, 0) 
                    ? $"Ludusavi v{version}" 
                    : "Ludusavi not found";
            }
            catch
            {
                VersionText.Text = "";
            }
        }

        private void LoadBackedUpGames()
        {
            try
            {
                LoadingPanel.Visibility = Visibility.Visible;
                EmptyStateText.Visibility = Visibility.Collapsed;
                gameItems.Clear();
                allGameItems.Clear();

                var backups = plugin.app?.backups;
                if (backups == null || backups.Count == 0)
                {
                    LoadingPanel.Visibility = Visibility.Collapsed;
                    EmptyStateText.Visibility = Visibility.Visible;
                    GameCountText.Text = "0 games backed up";
                    return;
                }

                foreach (var kvp in backups.OrderBy(b => b.Key))
                {
                    var gameName = kvp.Key;
                    var backupList = kvp.Value;

                    string lastBackupStr = "Unknown";
                    if (backupList.Count > 0)
                    {
                        var latestBackup = backupList.OrderByDescending(b => b.When).First();
                        if (latestBackup.When != DateTime.MinValue)
                        {
                            lastBackupStr = latestBackup.When.ToString("yyyy-MM-dd HH:mm");
                        }
                    }

                    allGameItems.Add(new BackedUpGameItem
                    {
                        GameName = gameName,
                        BackupCount = backupList.Count,
                        LastBackupDate = $"Last: {lastBackupStr}"
                    });
                }

                ApplyFilter();
                LoadingPanel.Visibility = Visibility.Collapsed;

                var count = allGameItems.Count;
                GameCountText.Text = $"{count} {(count == 1 ? "game" : "games")} backed up";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading backed up games in sidebar");
                LoadingPanel.Visibility = Visibility.Collapsed;
                EmptyStateText.Text = "Error loading backups.";
                EmptyStateText.Visibility = Visibility.Visible;
            }
        }

        private void ApplyFilter()
        {
            var query = SearchTextBox?.Text?.Trim() ?? "";
            gameItems.Clear();

            var filtered = string.IsNullOrEmpty(query)
                ? allGameItems
                : allGameItems.Where(g => g.GameName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            foreach (var item in filtered)
            {
                gameItems.Add(item);
            }

            EmptyStateText.Visibility = filtered.Count == 0 && allGameItems.Count > 0 
                ? Visibility.Visible 
                : Visibility.Collapsed;

            if (filtered.Count == 0 && allGameItems.Count > 0)
            {
                EmptyStateText.Text = "No games match the search.";
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void OpenLudusaviButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                plugin.app?.Launch();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error launching Ludusavi");
            }
        }

        private async void BackupAllButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                plugin.app?.Launch();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error launching Ludusavi for backup all");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Task.Run(() => plugin.Refresh(RefreshContext.Startup));
                System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3);
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    LoadBackedUpGames();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error refreshing backups");
            }
        }

        private void BackupGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is Button btn) || !(btn.Tag is string gameName)) return;

                // Open Ludusavi GUI for this specific game
                if (plugin.app != null && plugin.app.version.supportsGuiCommand())
                {
                    plugin.app.OpenCustomGame(gameName);
                }
                else
                {
                    plugin.app?.Launch();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error backing up game from sidebar");
            }
        }

        private void RestoreGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is Button btn) || !(btn.Tag is string gameName)) return;

                // Open Ludusavi GUI for this specific game
                if (plugin.app != null && plugin.app.version.supportsGuiCommand())
                {
                    plugin.app.OpenCustomGame(gameName);
                }
                else
                {
                    plugin.app?.Launch();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error restoring game from sidebar");
            }
        }

        private void OpenBackupFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is Button btn) || !(btn.Tag is string gameName)) return;

                var backupPath = Etc.GetDictValue(plugin.app.backupPaths, gameName, null);
                if (!string.IsNullOrEmpty(backupPath) && Directory.Exists(backupPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = backupPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    plugin.interactor.ShowError($"Backup folder not found for: {gameName}");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error opening backup folder");
            }
        }
    }

    public class BackedUpGameItem
    {
        public string GameName { get; set; } = "";
        public int BackupCount { get; set; }
        public string LastBackupDate { get; set; } = "";
    }
}
