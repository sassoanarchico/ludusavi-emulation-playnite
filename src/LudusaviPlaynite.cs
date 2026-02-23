using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.SDK.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LudusaviPlaynite
{
    public class LudusaviPlaynite : GenericPlugin
    {
        private static ILogger logger;
        public LudusaviPlayniteSettings settings { get; set; }
        public override Guid Id { get; } = Guid.Parse("72e2de43-d859-44d8-914e-4277741c8208");

        public Cli.App app;
        public Interactor interactor;
        private Translator translator;
        private bool pendingOperation { get; set; }
        private bool playedSomething { get; set; }
        private Game lastGamePlayed { get; set; }
        private bool multipleGamesRunning { get; set; }
        private Timer duringPlayBackupTimer { get; set; }
        private int duringPlayBackupTotal { get; set; }
        private int duringPlayBackupFailed { get; set; }
        private Timer checkAppUpdateTimer { get; set; }

        public LudusaviPlaynite(IPlayniteAPI api) : base(api)
        {
            // Inizializza il logger personalizzato
            string logDirectory = Path.Combine(GetPluginUserDataPath(), "logs");
            logger = new CustomLogger(LogManager.GetLogger(), logDirectory);
            try
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var asmLocation = asm?.Location ?? "(unknown)";
                var asmDir = "";
                try { asmDir = Path.GetDirectoryName(asmLocation) ?? ""; } catch { }
                var dllWriteTime = "";
                try { dllWriteTime = File.Exists(asmLocation) ? File.GetLastWriteTime(asmLocation).ToString("yyyy-MM-dd HH:mm:ss") : "(n/a)"; } catch { dllWriteTime = "(n/a)"; }
                var cwd = "";
                try { cwd = Directory.GetCurrentDirectory(); } catch { cwd = "(unknown)"; }
                var titleInAsm = "";
                try 
                { 
                    titleInAsm = Path.Combine(asmDir, "titleid.txt"); 
                    if (!File.Exists(titleInAsm)) 
                        titleInAsm = Path.Combine(asmDir, "data", "titleid.txt"); 
                } 
                catch { titleInAsm = ""; }
                var titleInCwd = "";
                try 
                { 
                    titleInCwd = Path.Combine(cwd, "titleid.txt"); 
                    if (!File.Exists(titleInCwd)) 
                        titleInCwd = Path.Combine(cwd, "data", "titleid.txt"); 
                } 
                catch { titleInCwd = ""; }

                logger.Info("LudusaviPlaynite plugin initialized");
                logger.Info($"Build/Load info: dll={asmLocation} | lastWrite={dllWriteTime} | cwd={cwd}");
                logger.Info($"titleid.txt check: inDllDir={(File.Exists(titleInAsm) ? "YES" : "NO")} ({titleInAsm}) | inCwd={(File.Exists(titleInCwd) ? "YES" : "NO")} ({titleInCwd})");
            }
            catch
            {
                logger.Info("LudusaviPlaynite plugin initialized");
            }

            translator = new Translator(PlayniteApi.ApplicationSettings.Language);
            settings = new LudusaviPlayniteSettings(this, translator);
            app = new Cli.App(LudusaviPlaynite.logger, this.settings);
            interactor = new Interactor(api, settings, translator);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            this.checkAppUpdateTimer = new Timer(
                x => CheckAppUpdate(),
                null,
                TimeSpan.FromHours(24.1),
                TimeSpan.FromHours(24.1)
            );
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs menuArgs)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = translator.Launch_Label(),
                    MenuSection = "@" + translator.Ludusavi(),
                    Action = args => {
                        app.Launch();
                    }
                },
                new MainMenuItem
                {
                    Description = translator.BackUpLastGame_Label(),
                    MenuSection = "@" + translator.Ludusavi(),
                    Action = async args => {
                        await InitiateOperation(null, Operation.Backup, OperationTiming.Free, BackupCriteria.Game);
                    }
                },
                new MainMenuItem
                {
                    Description = translator.BackUpAllGames_Label(),
                    MenuSection = "@" + translator.Ludusavi(),
                    Action = async args => {
                        if (!CanPerformOperation())
                        {
                            return;
                        }
                        if (interactor.UserConsents(translator.BackUpAllGames_Confirm()))
                        {
                            await Task.Run(() => BackUpAllGames());
                        }
                    }
                },
                new MainMenuItem
                {
                    Description = translator.RestoreLastGame_Label(),
                    MenuSection = "@" + translator.Ludusavi(),
                    Action = async args => {
                        await InitiateOperation(null, Operation.Restore, OperationTiming.Free, BackupCriteria.Game);
                    }
                },
                new MainMenuItem
                {
                    Description = translator.RestoreAllGames_Label(),
                    MenuSection = "@" + translator.Ludusavi(),
                    Action = async args => {
                        if (!CanPerformOperation())
                        {
                            return;
                        }
                        if (interactor.UserConsents(translator.RestoreAllGames_Confirm()))
                        {
                            await Task.Run(() => RestoreAllGames());
                        }
                    }
                },
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs menuArgs)
        {
            var items = new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = translator.BackUpSelectedGames_Label(),
                    MenuSection = translator.Ludusavi(),
                    Action = async args => {
                        if (args.Games.Count == 1)
                        {
                            await InitiateOperation(args.Games[0], Operation.Backup, OperationTiming.Free, BackupCriteria.Game);
                        }
                        else
                        {
                            if (!CanPerformOperation())
                            {
                                return;
                            }
                            if (interactor.UserConsents(translator.BackUpSelectedGames_Confirm(args.Games.Select(x => settings.GetGameName(x)).ToList())))
                            {
                                foreach (var game in args.Games)
                                {
                                    {
                                        await Task.Run(() => BackUpOneGame(game, OperationTiming.Free, BackupCriteria.Game));
                                    }
                                }
                            }
                        }
                    }
                },
            };

            if (menuArgs.Games.Count == 1 && IsBackedUp(menuArgs.Games[0]))
            {
                var game = menuArgs.Games[0];
                foreach (var backup in GetBackups(game))
                {
                    if (this.app.version.supportsEditBackup())
                    {
                        var section = string.Format("{0} | {1} | {2}", translator.Ludusavi(), translator.RestoreSelectedGames_Label(), Etc.GetBackupDisplayLine(backup));

                        items.Add(
                            new GameMenuItem
                            {
                                Description = translator.Restore(),
                                MenuSection = section,
                                Action = async args =>
                                {
                                    await InitiateOperation(game, Operation.Restore, OperationTiming.Free, BackupCriteria.Game, backup);
                                }
                            }
                        );

                        items.Add(
                            new GameMenuItem
                            {
                                Description = backup.Locked ? translator.Unlock() : translator.Lock(),
                                MenuSection = section,
                                Action = args =>
                                {
                                    var title = GetTitle(game);
                                    this.app.EditBackup(title, !backup.Locked, null);
                                    this.RefreshBackups();
                                }
                            }
                        );
                        items.Add(
                            new GameMenuItem
                            {
                                Description = translator.SetComment(),
                                MenuSection = section,
                                Action = args =>
                                {
                                    var comment = interactor.InputText(translator.SetComment(), backup.Comment);
                                    if (comment != null)
                                    {
                                        var title = GetTitle(game);
                                        this.app.EditBackup(title, null, comment);
                                        this.RefreshBackups();
                                    }
                                }
                            }
                        );
                    }
                    else
                    {
                        items.Add(
                            new GameMenuItem
                            {
                                Description = Etc.GetBackupDisplayLine(backup),
                                MenuSection = string.Format("{0} | {1}", translator.Ludusavi(), translator.RestoreSelectedGames_Label()),
                                Action = async args =>
                                {
                                    await InitiateOperation(game, Operation.Restore, OperationTiming.Free, BackupCriteria.Game, backup);
                                }
                            }
                        );
                    }
                }
            }
            else
            {
                items.Add(
                    new GameMenuItem
                    {
                        Description = translator.RestoreSelectedGames_Label(),
                        MenuSection = translator.Ludusavi(),
                        Action = async args =>
                        {
                            if (!CanPerformOperation())
                            {
                                return;
                            }
                            if (interactor.UserConsents(translator.RestoreSelectedGames_Confirm(args.Games.Select(x => settings.GetGameName(x)).ToList())))
                            {
                                foreach (var game in args.Games)
                                {
                                    {
                                        await Task.Run(() => RestoreOneGame(game, null, BackupCriteria.Game));
                                    }
                                }
                            }
                        }
                    }
                );
            }

            var backupPaths = menuArgs.Games.Select(GetBackupPath).Where(x => x != null);
            if (backupPaths.Any())
            {
                items.Add(
                    new GameMenuItem
                    {
                        Description = translator.OpenBackupDirectory(),
                        MenuSection = translator.Ludusavi(),
                        Action = args =>
                        {
                            var failed = new List<string>();

                            foreach (var backupPath in backupPaths)
                            {
                                if (!Etc.OpenDir(backupPath))
                                {
                                    failed.Add(backupPath);
                                }
                            }

                            if (failed.Any())
                            {
                                var message = this.translator.CannotOpenFolder();
                                var paths = string.Join("\n", failed);
                                var body = $"{message}\n\n{paths}";
                                interactor.ShowError(body);
                            }
                        }
                    }
                );
            }

            if (menuArgs.Games.Count == 1)
            {
                var title = menuArgs.Games[0].Name;
                string renamed = Etc.GetDictValue(settings.AlternativeTitles, title, null);

                items.Add(
                    new GameMenuItem
                    {
                        Description = translator.LookUpAsOtherTitle(renamed),
                        MenuSection = translator.Ludusavi(),
                        Action = args =>
                        {
                            GenericItemOption result = null;

                            if (this.app.version.supportsManifestShow() && this.app.manifestGames.Count > 0)
                            {
                                var options = this.app.manifestGames.Select(x => new GenericItemOption(x, "")).ToList();
                                result = PlayniteApi.Dialogs.ChooseItemWithSearch(
                                    new List<GenericItemOption>(options),
                                    (query) =>
                                    {
                                        if (query == null)
                                        {
                                            return options;
                                        }
                                        else
                                        {
                                            return this.app.manifestGames
                                                .Where(x => x.ToLower().Contains(query.ToLower()))
                                                .Select(x => new GenericItemOption(x, "")).ToList();
                                        }
                                    }
                                );
                            }
                            else
                            {
                                var input = PlayniteApi.Dialogs.SelectString(translator.LookUpAsOtherTitle(null), "", "");
                                if (!string.IsNullOrEmpty(input?.SelectedString?.Trim()))
                                {
                                    result = new GenericItemOption(input.SelectedString.Trim(), "");
                                }
                            }

                            if (result != null)
                            {
                                settings.AlternativeTitles[title] = result.Name;
                                SavePluginSettings(settings);
                                Refresh(RefreshContext.ConfiguredTitle);
                            }
                        }
                    }
                );

                if (settings.AlternativeTitles.ContainsKey(menuArgs.Games[0].Name))
                {
                    items.Add(
                        new GameMenuItem
                        {
                            Description = translator.LookUpAsNormalTitle(),
                            MenuSection = translator.Ludusavi(),
                            Action = args =>
                            {
                                settings.AlternativeTitles.Remove(title);
                                SavePluginSettings(settings);
                                Refresh(RefreshContext.ConfiguredTitle);
                            }
                        }
                    );
                }

                if (this.app.version.supportsGuiCommand())
                {
                    items.Add(
                        new GameMenuItem
                        {
                            Description = translator.CustomizeInLudusavi(),
                            MenuSection = translator.Ludusavi(),
                            Action = args =>
                            {
                                if (!this.app.OpenCustomGame(renamed ?? title))
                                {
                                    interactor.NotifyError(translator.UnableToRunLudusavi(), OperationTiming.Free);
                                }
                            }
                        }
                    );
                }
            }

            foreach (var entry in Tags.CONFLICTS)
            {
                var candidate = entry.Key;
                var conflicts = entry.Value;

                if (menuArgs.Games.Any(x => !Etc.HasTag(x, candidate)))
                {
                    items.Add(
                        new GameMenuItem
                        {
                            Description = translator.AddTagForSelectedGames_Label(candidate),
                            MenuSection = translator.Ludusavi(),
                            Action = async args =>
                            {
                                if (interactor.UserConsents(translator.AddTagForSelectedGames_Confirm(candidate, args.Games.Select(x => x.Name))))
                                {
                                    using (PlayniteApi.Database.BufferedUpdate())
                                    {
                                        foreach (var game in args.Games)
                                        {
                                            {
                                                await Task.Run(() =>
                                                {
                                                    interactor.AddTag(game, candidate);
                                                    foreach (var conflict in conflicts)
                                                    {
                                                        var removed = interactor.RemoveTag(game, conflict);
                                                        string replacement;
                                                        if (removed && Tags.REPLACEMENTS.TryGetValue((candidate, conflict), out replacement))
                                                        {
                                                            interactor.AddTag(game, replacement);
                                                        }
                                                    }
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    );
                }

                if (menuArgs.Games.Any(x => Etc.HasTag(x, candidate)))
                {
                    items.Add(
                        new GameMenuItem
                        {
                            Description = translator.RemoveTagForSelectedGames_Label(candidate),
                            MenuSection = translator.Ludusavi(),
                            Action = async args =>
                            {
                                if (interactor.UserConsents(translator.RemoveTagForSelectedGames_Confirm(candidate, args.Games.Select(x => x.Name))))
                                {
                                    using (PlayniteApi.Database.BufferedUpdate())
                                    {
                                        foreach (var game in args.Games)
                                        {
                                            {
                                                await Task.Run(() =>
                                                {
                                                    interactor.RemoveTag(game, candidate);
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    );
                }
            }

            return items;
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (!settings.MigratedTags)
            {
                var oldTag = PlayniteApi.Database.Tags.FirstOrDefault(x => x.Name == Tags.LEGACY_SKIP);
                var newTagExists = PlayniteApi.Database.Tags.Any(x => x.Name == Tags.SKIP);
                if (oldTag != null && !newTagExists)
                {
                    oldTag.Name = Tags.SKIP;
                    PlayniteApi.Database.Tags.Update(oldTag);
                }
                settings.MigratedTags = true;
                SavePluginSettings(settings);
            }

            Task.Run(() =>
            {
                Refresh(RefreshContext.Startup);

                if (app.version.inner < Etc.RECOMMENDED_APP_VERSION && new Version(settings.SuggestedUpgradeTo) < Etc.RECOMMENDED_APP_VERSION)
                {
                    interactor.NotifyInfo(
                        translator.UpgradePrompt(Etc.RECOMMENDED_APP_VERSION.ToString()),
                        () =>
                        {
                            Etc.OpenLudusaviReleasePage();
                        }
                    );
                    settings.SuggestedUpgradeTo = Etc.RECOMMENDED_APP_VERSION.ToString();
                    SavePluginSettings(settings);
                }
            });
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            playedSomething = true;
            lastGamePlayed = args.Game;
            if (CountGamesRunning() > 0)
            {
                multipleGamesRunning = true;
            }
            Game game = args.Game;
            var prefs = settings.GetPlayPreferences(game, multipleGamesRunning);

            if (prefs.Game.Restore.Do)
            {
                InitiateOperationSync(game, Operation.Restore, OperationTiming.BeforePlay, BackupCriteria.Game);
            }

            if (prefs.Platform.Restore.Do)
            {
                InitiateOperationSync(game, Operation.Restore, OperationTiming.BeforePlay, BackupCriteria.Platform);
            }

            if (settings.DoBackupDuringPlay)
            {
                this.duringPlayBackupTotal = 0;
                this.duringPlayBackupFailed = 0;
                this.duringPlayBackupTimer = new Timer(
                    x => BackUpOneGameDuringPlay((Game)x),
                    game,
                    TimeSpan.FromMinutes(settings.BackupDuringPlayInterval),
                    TimeSpan.FromMinutes(settings.BackupDuringPlayInterval)
                );
            }
        }

        public override void OnGameStopped(OnGameStoppedEventArgs arg)
        {
            playedSomething = true;
            lastGamePlayed = arg.Game;
            if (CountGamesRunning() == 0)
            {
                multipleGamesRunning = false;
            }
            Game game = arg.Game;
            var prefs = settings.GetPlayPreferences(game, multipleGamesRunning);

            if (this.duringPlayBackupTimer != null)
            {
                this.duringPlayBackupTimer.Change(Timeout.Infinite, Timeout.Infinite);
                if (this.duringPlayBackupFailed == 0)
                {
                    interactor.NotifyInfo(translator.BackUpDuringPlay_Success(game.Name, this.duringPlayBackupTotal));
                }
                else
                {
                    interactor.NotifyError(translator.BackUpDuringPlay_Failure(game.Name, this.duringPlayBackupTotal, this.duringPlayBackupFailed));
                }
            }

            Task.Run(async () =>
            {
                if (prefs.Game.Backup.Do)
                {
                    await InitiateOperation(game, Operation.Backup, OperationTiming.AfterPlay, BackupCriteria.Game);
                }

                if (prefs.Platform.Backup.Do)
                {
                    await InitiateOperation(game, Operation.Backup, OperationTiming.AfterPlay, BackupCriteria.Platform);
                }
            });
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new LudusaviPlayniteSettingsView(this, this.translator);
        }

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            return new List<SidebarItem>
            {
                new SidebarItem
                {
                    Title = "Ludusavi",
                    Type = SiderbarItemType.View,
                    Icon = new System.Windows.Controls.TextBlock
                    {
                        Text = "\uEF08",
                        FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"),
                        FontSize = 20
                    },
                    Opened = () =>
                    {
                        return new Views.LudusaviSidebarView(this);
                    }
                }
            };
        }

        public void Refresh(RefreshContext context)
        {
            switch (context)
            {
                case RefreshContext.Startup:
                    app.RefreshVersion();

                    if (!app.IsValid())
                    {
                        interactor.ShowError(translator.InitialSetupRequired());
                        Etc.OpenLudusaviMainPage();
                    }

                    app.RefreshTitles(PlayniteApi.Database.Games.ToList());
                    RefreshBackups();
                    RefreshGames();
                    CheckAppUpdate();
                    break;
                case RefreshContext.EditedConfig:
                    app.RefreshVersion();
                    app.RefreshTitles(PlayniteApi.Database.Games.ToList());
                    RefreshBackups();
                    break;
                case RefreshContext.ConfiguredTitle:
                    app.RefreshTitles(PlayniteApi.Database.Games.ToList());
                    RefreshBackups();
                    break;
                case RefreshContext.CreatedBackup:
                    RefreshBackups();
                    break;
            }
        }

        private void HandleSuccessDuringPlay()
        {
            this.duringPlayBackupTotal += 1;
        }

        private void HandleFailureDuringPlay()
        {
            this.duringPlayBackupTotal += 1;
            this.duringPlayBackupFailed += 1;
        }

        private void RefreshGames()
        {
            app.RefreshGames();
            if (this.settings.TagGamesWithUnknownSaveData)
            {
                TagGamesWithUnknownSaveData();
            }
        }

        private void RefreshBackups()
        {
            if (app.RefreshBackups() && this.settings.TagGamesWithBackups)
            {
                TagGamesWithBackups();
            }
        }

        private void CheckAppUpdate()
        {
            if (!(settings.CheckAppUpdate))
            {
                return;
            }

            if ((DateTime.UtcNow - settings.CheckedAppUpdate).TotalHours < 24)
            {
                return;
            }

            settings.CheckedAppUpdate = DateTime.UtcNow;

            var update = app.CheckAppUpdate();
            if (update != null && update?.version != settings.PresentedAppUpdate && update?.version != app.version.inner.ToString())
            {
                settings.PresentedAppUpdate = update?.version;
                interactor.NotifyInfo(
                    translator.UpgradeAvailable(update?.version),
                    () =>
                    {
                        Etc.OpenUrl(update?.url);
                    }
                );
            }

            SavePluginSettings(settings);
        }

        public void NotifyResponseErrors(Cli.Output.Response? response)
        {
            if (response?.Errors.CloudSyncFailed != null)
            {
                var prefix = translator.Ludusavi();
                var error = translator.CloudSyncFailed();
                interactor.NotifyError($"{prefix}: {error}");
            }
            if (response?.Errors.CloudConflict != null)
            {
                var prefix = translator.Ludusavi();
                var error = translator.CloudConflict();
                interactor.NotifyError($"{prefix}: {error}", () => app.Launch());
            }
        }

        private void ShowFullResults(Cli.Output.Response response)
        {
            var tempFile = Path.GetTempPath() + Guid.NewGuid().ToString() + ".html";
            using (StreamWriter sw = File.CreateText(tempFile))
            {
                sw.WriteLine("<html><head><style>body { background-color: black; color: white; font-family: sans-serif; }</style></head><body><ul>");
                foreach (var game in response.Games)
                {
                    sw.WriteLine(string.Format("<li>{0}</li>", translator.FullListGameLineItem(game.Key, game.Value)));
                }
                sw.WriteLine("</ul></body></html>");
            }

            var webview = PlayniteApi.WebViews.CreateView(640, 480);
            webview.Navigate(tempFile);
            webview.OpenDialog();

            try
            {
                File.Delete(tempFile);
            }
            catch
            { }
        }

        private bool CanPerformOperation()
        {
            if (pendingOperation)
            {
                PlayniteApi.Dialogs.ShowMessage(translator.OperationStillPending());
                return false;
            }
            return true;
        }

        private bool CanPerformOperationSuppressed()
        {
            return !pendingOperation;
        }

        private bool CanPerformOperationOnLastGamePlayed()
        {
            if (!playedSomething)
            {
                PlayniteApi.Dialogs.ShowMessage(translator.NoGamePlayedYet());
                return false;
            }
            return CanPerformOperation();
        }

        string GetTitle(Game game)
        {
            return Etc.GetDictValue(this.app.titles, Etc.GetTitleId(game), null);
        }




        private bool TryAutoConfigureEmulatedGame(Game game, out string customTitle, out string error)
        {
            customTitle = null;
            error = null;

            logger.Info($"TryAutoConfigureEmulatedGame invoked for game: '{game?.Name ?? "(null)"}'");

            // Check if this game already has an alternative title configured
            var existingTitle = settings.AlternativeTitle(game);
            if (!string.IsNullOrEmpty(existingTitle))
            {
                logger.Debug($"Found existing alternative title for '{game.Name}': '{existingTitle}'");

                // Verify the custom game still exists in Ludusavi config
                try
                {
                    if (LudusaviConfigEditor.CustomGameExists(settings.ExecutablePath, existingTitle))
                    {
                        logger.Debug($"Custom game '{existingTitle}' verified in Ludusavi config. Using existing configuration.");
                        customTitle = existingTitle;
                        return true;
                    }
                    else
                    {
                        logger.Warn($"Custom game '{existingTitle}' not found in Ludusavi config. Will reconfigure.");
                        // Remove stale alternative title
                        settings.AlternativeTitles.Remove(game.Name);
                        SavePluginSettings(settings);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error checking if custom game '{existingTitle}' exists. Will skip reconfiguration to avoid corruption.");
                    // On error, assume it exists to avoid potential corruption
                    customTitle = existingTitle;
                    return true;
                }
            }

            logger.Info($"TryAutoConfigureEmulatedGame invoked for '{game?.Name ?? "(null)"}' (EnableEmulatedGameAutomation: {this.settings.EnableEmulatedGameAutomation})");

            if (!this.settings.EnableEmulatedGameAutomation)
            {
                error = "Emulated game auto-configuration is disabled in plugin settings (EnableEmulatedGameAutomation=false).";
                logger.Warn(error);
                return false;
            }

            var detected = EmulationDetector.TryGetInfo(game, this.PlayniteApi, out var info);
            
            // Fallback: if game has PS3 platform but wasn't detected as emulated, treat it as RPCS3
            if ((!detected || !info.IsEmulated) && info != null)
            {
                var platform = info.Platform ?? "";
                var platformLower = platform.ToLowerInvariant();
                
                // Check if this is a PS3 game based on platform name
                if (platformLower.Contains("playstation 3") || platformLower.Contains("ps3") || 
                    platformLower.Contains("sony playstation 3"))
                {
                    logger.Info($"Game has PS3 platform but wasn't detected as emulated. Treating as RPCS3 emulated game.");
                    logger.Info($"  Platform: {platform}");
                    logger.Info($"  GameId: {info.GameId ?? "(empty)"}");
                    
                    // Force it to be treated as emulated RPCS3
                    info.IsEmulated = true;
                    info.Platform = "PlayStation 3";
                    info.EmulatorName = "RPCS3";
                    // Try to find RPCS3 executable in common locations
                    var commonRpcs3Paths = new[]
                    {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "RPCS3", "rpcs3.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "RPCS3", "rpcs3.exe"),
                        @"C:\RPCS3\rpcs3.exe",
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "RPCS3", "rpcs3.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "RPCS3", "rpcs3.exe")
                    };
                    
                    foreach (var rpcs3Path in commonRpcs3Paths)
                    {
                        if (File.Exists(rpcs3Path))
                        {
                            info.EmulatorExecutablePath = rpcs3Path;
                            logger.Info($"  Found RPCS3 at: {rpcs3Path}");
                            break;
                        }
                    }
                    
                    if (string.IsNullOrEmpty(info.EmulatorExecutablePath))
                    {
                        logger.Warn($"  RPCS3 executable not found in common locations. Will use default paths.");
                    }
                }
                else
                {
                    error =
                        "Game was not detected as emulated by EmulationDetector.\n" +
                        $"  Detected: {detected}\n" +
                        $"  IsEmulated: {info?.IsEmulated}\n" +
                        $"  Platform: {info?.Platform ?? "(empty)"}\n" +
                        $"  EmulatorName: {info?.EmulatorName ?? "(empty)"}\n" +
                        $"  EmulatorExecutablePath: {info?.EmulatorExecutablePath ?? "(empty)"}\n" +
                        $"  RomPath: {info?.RomPath ?? "(empty)"}\n" +
                        $"  GameId: {info?.GameId ?? "(empty)"}\n" +
                        "To fix: ensure the Play action is an Emulator action and/or the game has a ROM configured in Playnite.";
                    logger.Warn(error);
                    return false;
                }
            }
            
            if (!info.IsEmulated)
            {
                error = "Game is not emulated and platform fallback did not apply.";
                logger.Warn(error);
                return false;
            }

            var haystack = ((info.EmulatorName ?? "") + " " + (info.EmulatorExecutablePath ?? "")).ToLowerInvariant();

            EmulatedSaveMapping match = null;
            var mappings = this.settings.EmulatedSaveMappings ?? new System.Collections.ObjectModel.ObservableCollection<EmulatedSaveMapping>();
            foreach (var mapping in mappings)
            {
                if (mapping == null || !mapping.Enabled)
                {
                    continue;
                }
                var needle = (mapping.EmulatorMatch ?? "").Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(needle))
                {
                    continue;
                }
                if (needle == "*" || haystack.Contains(needle))
                {
                    match = mapping;
                    break;
                }
            }

            if (match == null)
            {
                var enabledNeedles = string.Join(", ",
                    mappings
                        .Where(m => m != null && m.Enabled && !string.IsNullOrWhiteSpace(m.EmulatorMatch))
                        .Select(m => (m.EmulatorMatch ?? "").Trim())
                );
                error =
                    "No enabled emulated save mapping matched this emulator.\n" +
                    $"  Emulator haystack: '{haystack}'\n" +
                    $"  Enabled mappings (EmulatorMatch): {(string.IsNullOrWhiteSpace(enabledNeedles) ? "(none)" : enabledNeedles)}\n" +
                    "To fix: enable a mapping that matches this emulator (e.g., EmulatorMatch contains 'rpcs3').";
                logger.Warn(error);
                return false;
            }

            var platformLabel = string.IsNullOrWhiteSpace(match.Platform) ? (info.Platform ?? "Unknown") : match.Platform.Trim();
            var emulatorLabel = (!string.IsNullOrWhiteSpace(info.EmulatorName) ? info.EmulatorName : Path.GetFileNameWithoutExtension(info.EmulatorExecutablePath ?? "")) ?? "Emulator";

            customTitle = (this.settings.EmulatedGameTitleFormat ?? "<name> (<platform> - <emulator>)")
                .Replace("<name>", game.Name ?? "")
                .Replace("<platform>", platformLabel)
                .Replace("<emulator>", emulatorLabel);
            customTitle = customTitle.Trim();

            // Resolve save paths based on templates.
            info.Platform = platformLabel;
            info.EmulatorName = emulatorLabel;

            // Log info for debugging
            logger.Info($"=== EMULATED GAME AUTO-CONFIG START ===");
            logger.Info($"Game: {game.Name}");
            logger.Info($"GameId: {info.GameId ?? "(empty)"}");
            logger.Info($"Emulator: {info.EmulatorName ?? "(empty)"}");
            logger.Info($"Emulator Executable Path: {info.EmulatorExecutablePath ?? "(empty)"}");
            logger.Info($"Platform: {platformLabel}");
            logger.Info($"RomPath: {info.RomPath ?? "(empty)"}");
            logger.Info($"Template: {match.SavePathTemplates}");

            // Build emulator custom paths dictionary for save finders
            var emulatorCustomPaths = new Dictionary<string, string>();
            if (settings.EmulatorCustomPaths != null)
            {
                foreach (var ep in settings.EmulatorCustomPaths)
                {
                    if (!string.IsNullOrWhiteSpace(ep.EmulatorName) && !string.IsNullOrWhiteSpace(ep.CustomPath))
                    {
                        emulatorCustomPaths[ep.EmulatorName.Trim()] = ep.CustomPath.Trim();
                    }
                }
            }

            // For RPCS3, try to extract PS3 game code and find save folder dynamically
            var paths = EmulatedSaveTemplate.ResolveMany(info, match.SavePathTemplates, game.Name, settings.RPCS3SaveDataPath, emulatorCustomPaths);

            logger.Info($"Resolved paths: {string.Join(", ", paths)}");
            
            // Log search paths for debugging when paths are empty
            if (paths.Count == 0)
            {
                var emulatorLower = ((info?.EmulatorName ?? "") + " " + (info?.EmulatorExecutablePath ?? "")).ToLowerInvariant();
                if (emulatorLower.Contains("rpcs3"))
                {
                    var rpcs3CustomPath = settings.GetEmulatorCustomPath("rpcs3") ?? settings.RPCS3SaveDataPath;
                    var searchPaths = RPCS3SaveFinder.GetCommonRPCS3SavePaths(info?.EmulatorExecutablePath, rpcs3CustomPath);
                    logger.Info($"RPCS3 search paths checked: {string.Join(", ", searchPaths)}");
                    logger.Info($"Game name for matching: '{game.Name}'");
                    LogRpcs3DetailedSearch(info, game, searchPaths);
                }
                else if (emulatorLower.Contains("ppsspp"))
                {
                    var savedataPaths = PPSSPPSaveFinder.GetSavedataPaths(info?.EmulatorExecutablePath, settings.GetEmulatorCustomPath("ppsspp"));
                    logger.Info($"PPSSPP search paths checked: {string.Join(", ", savedataPaths)}");
                    var pspCode = PPSSPPSaveFinder.ExtractPSPGameCode(info, game.Name);
                    logger.Info($"PSP game code: {pspCode ?? "(not found)"}");
                }
                else
                {
                    logger.Info($"No save paths resolved for emulator: {info?.EmulatorName ?? "(unknown)"}");
                }
            }

            if (!paths.Any())
            {
                // --- MANUAL FALLBACK: ask user to select save folder ---
                logger.Info($"No save paths found automatically for '{game.Name}'. Asking user for manual selection.");
                
                string manualPath = null;
                try
                {
                    manualPath = interactor.AskManualSavePath(game.Name);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error asking user for manual save path");
                }

                if (!string.IsNullOrWhiteSpace(manualPath))
                {
                    logger.Info($"User selected manual save path: {manualPath}");
                    paths = new List<string> { manualPath };
                }
                else
                {
                    error = $"No save paths found for '{game.Name}' and user did not provide a manual path.";
                    logger.Warn(error);
                    return false;
                }
            }

            // Validate that at least one path exists before configuring
            var validPaths = paths.Where(p => !string.IsNullOrWhiteSpace(p) && (Directory.Exists(p) || File.Exists(p))).ToList();
            if (!validPaths.Any())
            {
                error = $"Resolved save paths do not exist: {string.Join(", ", paths)}";
                logger.Warn($"All resolved paths for emulated game '{game.Name}' do not exist. Paths: {string.Join(", ", paths)}");
                // Still proceed with configuration - paths might be created later or Ludusavi will handle it
                validPaths = paths.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
                if (!validPaths.Any())
                {
                    return false;
                }
            }

            logger.Info($"Valid paths for backup ({validPaths.Count}): {string.Join(", ", validPaths)}");

            // Use valid paths if we filtered them, otherwise use all resolved paths
            var pathsToUse = validPaths.Any() ? validPaths : paths;

            logger.Info($"--- CREATING CUSTOM LUDUSAVI ENTRY ---");
            logger.Info($"Custom Title: '{customTitle}'");
            logger.Info($"Paths ({pathsToUse.Count}):");
            foreach (var path in pathsToUse)
            {
                var exists = Directory.Exists(path) || File.Exists(path);
                logger.Info($"  - {path} [Exists: {exists}]");
            }

            if (!LudusaviConfigEditor.UpsertCustomGame(this.settings.ExecutablePath, customTitle, pathsToUse, out error))
            {
                logger.Error($"✗ Failed to upsert custom game '{customTitle}' with paths: {string.Join(", ", pathsToUse)}. Error: {error}");
                logger.Info($"=== EMULATED GAME AUTO-CONFIG FAILED ===");
                return false;
            }

            logger.Info($"✓ Successfully configured custom game '{customTitle}' with {pathsToUse.Count} path(s)");
            logger.Info($"=== EMULATED GAME AUTO-CONFIG SUCCESS ===");

            // Persist a stable lookup name so subsequent backups/restores use the created custom entry.
            this.settings.AlternativeTitles[game.Name] = customTitle;
            SavePluginSettings(this.settings);
            this.Refresh(RefreshContext.EditedConfig);

            // Notify user of successful configuration
            interactor.NotifyInfo(translator.EmulatedGameConfigured(game.Name, customTitle));

            return true;
        }

        /// <summary>
        /// Logs detailed RPCS3 game code search info for debugging.
        /// </summary>
        private void LogRpcs3DetailedSearch(EmulationInfo info, Game game, List<string> searchPaths)
        {
            try
            {
                // Try to find titleid.txt
                var titleIdPath = "";
                var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly()?.Location;
                if (!string.IsNullOrEmpty(assemblyLocation))
                {
                    var assemblyDir = Path.GetDirectoryName(assemblyLocation);
                    titleIdPath = Path.Combine(assemblyDir, "titleid.txt");
                    if (!File.Exists(titleIdPath))
                        titleIdPath = Path.Combine(assemblyDir, "data", "titleid.txt");
                }
                if (!File.Exists(titleIdPath ?? ""))
                    titleIdPath = Path.Combine(Directory.GetCurrentDirectory(), "titleid.txt");
                if (!File.Exists(titleIdPath ?? ""))
                    titleIdPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "titleid.txt");

                logger.Info($"--- RPCS3 GAME CODE SEARCH ---");
                logger.Info($"Game Name: '{game.Name}'");
                logger.Info($"TitleId file: {titleIdPath ?? "(not found)"} [Exists: {File.Exists(titleIdPath ?? "")}]");
                logger.Info($"Search paths ({searchPaths.Count}):");
                foreach (var path in searchPaths)
                {
                    var exists = Directory.Exists(path);
                    logger.Info($"  - {path} [Exists: {exists}]");
                }

                var directCode = RPCS3SaveFinder.ExtractPS3GameCode(info, game.Name);
                logger.Info($"Direct extraction: {directCode ?? "(none)"}");

                if (File.Exists(titleIdPath ?? ""))
                {
                    var titleIdCodes = RPCS3SaveFinder.FindAllPS3GameCodesInTitleIdFile(game.Name, titleIdPath);
                    logger.Info($"titleid.txt codes: {(titleIdCodes?.Count > 0 ? string.Join(", ", titleIdCodes) : "(none)")}");
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"Error in RPCS3 detailed search logging: {ex.Message}");
            }
        }

        private string FindGame(Game game, string name, OperationTiming timing, BackupCriteria criteria, Mode mode)
        {
            if (this.app.version.supportsApiCommand() && criteria.ByGame())
            {
                var title = GetTitle(game);
                if (title != null)
                {
                    return title;
                }

            }

            if (!this.app.version.supportsFindCommand())
            {
                return null;
            }

            var invocation = new Cli.Invocation(Mode.Find).PathIf(settings.BackupPath, settings.OverrideBackupPath).Game(name);

            if (mode == Mode.Backup)
            {
                invocation.FindBackup();
            }
            if (criteria.ByGame() && settings.AlternativeTitle(game) == null)
            {
                // There can't be an alt title because the Steam ID/etc would take priority over it.

                if (Etc.TrySteamId(game, out var id))
                {
                    invocation.SteamId(id);
                }
                if (!Etc.IsOnPc(game) && settings.RetryNonPcGamesWithoutSuffix)
                {
                    invocation.AddGame(game.Name);
                }
                if (settings.RetryUnrecognizedGameWithNormalization)
                {
                    invocation.Normalized();
                }
            }

            var (code, response) = app.Invoke(invocation);
            if (response == null)
            {
                interactor.NotifyError(translator.UnableToRunLudusavi(), timing);
                HandleFailureDuringPlay();
                return null;
            }

            var officialName = response?.Games.Keys.FirstOrDefault();
            if (code != 0 || officialName == null)
            {
                // Try auto-configuration for emulated games if Ludusavi didn't find the game
                if (criteria.ByGame())
                {
                    logger.Info($"Ludusavi did not find game '{game.Name}' (exit code: {code}, officialName: {officialName ?? "null"}). Attempting auto-configuration for emulated games.");
                    logger.Info($"About to call TryAutoConfigureEmulatedGame for '{game.Name}'...");
                    try
                    {
                        if (TryAutoConfigureEmulatedGame(game, out var customTitle, out var autoError))
                    {
                        logger.Info($"Auto-configuration successful for '{game.Name}': using custom title '{customTitle}'");
                        return customTitle;
                    }
                    else if (!string.IsNullOrEmpty(autoError))
                    {
                        logger.Warn($"Auto-configuration failed for '{game.Name}': {autoError}");
                    }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Exception in TryAutoConfigureEmulatedGame for '{game.Name}'");
                    }
                }
                interactor.NotifyError(translator.UnrecognizedGame(name), timing);
                HandleFailureDuringPlay();
                return null;
            }

            return officialName;
        }

        private void InitiateOperationSync(Game game, Operation operation, OperationTiming timing, BackupCriteria criteria, Cli.Output.Backup? backup = null)
        {
            if (game == null)
            {
                if (!CanPerformOperationOnLastGamePlayed())
                {
                    return;
                }
                game = this.lastGamePlayed;
            }
            else
            {
                if (!CanPerformOperation())
                {
                    return;
                }
            }

            if (criteria.ByPlatform() && Etc.GetGamePlatform(game) == null)
            {
                return;
            }

            var prefs = settings.GetPlayPreferences(game, multipleGamesRunning);
            var ask = prefs.ShouldAsk(timing, criteria, operation);
            var displayName = settings.GetDisplayName(game, criteria);
            var consented = !ask;

            if (ask)
            {
                if (timing == OperationTiming.Free)
                {
                    switch (operation)
                    {
                        case Operation.Backup:
                            consented = interactor.UserConsents(translator.BackUpOneGame_Confirm(displayName));
                            break;
                        case Operation.Restore:
                            consented = interactor.UserConsents(translator.RestoreOneGame_Confirm(displayName));
                            break;
                    }
                }
                else
                {
                    var choice = Choice.No;
                    switch (operation)
                    {
                        case Operation.Backup:
                            choice = interactor.AskUser(translator.BackUpOneGame_Confirm(displayName), !multipleGamesRunning);
                            break;
                        case Operation.Restore:
                            choice = interactor.AskUser(translator.RestoreOneGame_Confirm(displayName), !multipleGamesRunning);
                            break;
                    }

                    consented = choice.Accepted();

                    switch (operation)
                    {
                        case Operation.Backup:
                            switch (criteria)
                            {
                                case BackupCriteria.Game:
                                    interactor.UpdateTagsForChoice(game, choice, Tags.GAME_BACKUP, Tags.GAME_NO_BACKUP);
                                    break;
                                case BackupCriteria.Platform:
                                    interactor.UpdateTagsForChoice(game, choice, Tags.PLATFORM_BACKUP, Tags.PLATFORM_NO_BACKUP);
                                    break;
                            }
                            break;
                        case Operation.Restore:
                            switch (criteria)
                            {
                                case BackupCriteria.Game:
                                    interactor.UpdateTagsForChoice(game, choice, Tags.GAME_BACKUP_AND_RESTORE, Tags.GAME_NO_RESTORE, Tags.GAME_BACKUP);
                                    break;
                                case BackupCriteria.Platform:
                                    interactor.UpdateTagsForChoice(game, choice, Tags.PLATFORM_BACKUP_AND_RESTORE, Tags.PLATFORM_NO_RESTORE, Tags.PLATFORM_BACKUP);
                                    break;
                            }
                            break;
                    }
                }
            }

            if (!consented)
            {
                return;
            }

            switch (operation)
            {
                case Operation.Backup:
                    BackUpOneGame(game, timing, criteria);
                    break;
                case Operation.Restore:
                    var error = RestoreOneGame(game, backup, criteria);
                    if (timing == OperationTiming.BeforePlay && !String.IsNullOrEmpty(error.Message) && !error.Empty)
                    {
                        interactor.ShowError(error.Message);
                    }
                    break;
            }
        }

        private async Task InitiateOperation(Game game, Operation operation, OperationTiming timing, BackupCriteria criteria, Cli.Output.Backup? backup = null)
        {
            await Task.Run(() => InitiateOperationSync(game, operation, timing, criteria, backup));
        }

        private void BackUpOneGame(Game game, OperationTiming timing, BackupCriteria criteria)
        {
            pendingOperation = true;
            var name = criteria.ByPlatform() ? game.Platforms[0].Name : settings.GetGameNameWithAlt(game);
            var displayName = game.Name;
            var refresh = true;

            if (this.app.version.supportsFindCommand())
            {
                var found = FindGame(game, name, timing, criteria, Mode.Backup);
                if (found == null)
                {
                    pendingOperation = false;
                    return;
                }
                name = found;
                if (name != displayName)
                {
                    displayName = $"{displayName} (↪ {name})";
                }
            }

            // PRE-BACKUP VALIDATION: if this game has a custom alternative title,
            // verify the entry still exists in config.yaml before running the backup.
            // The ludusavi GUI may have re-saved the config and dropped our custom entry.
            if (criteria.ByGame())
            {
                var altTitle = settings.AlternativeTitle(game);
                if (!string.IsNullOrEmpty(altTitle))
                {
                    try
                    {
                        if (!LudusaviConfigEditor.CustomGameExists(settings.ExecutablePath, altTitle))
                        {
                            logger.Warn($"Custom game entry '{altTitle}' is missing from ludusavi config. Re-creating it.");
                            // Force re-creation by clearing the stale alternative title
                            settings.AlternativeTitles.Remove(game.Name);
                            SavePluginSettings(settings);
                            // Trigger auto-configuration to re-create the entry
                            if (TryAutoConfigureEmulatedGame(game, out var recreatedTitle, out var recreateError))
                            {
                                logger.Info($"Re-created custom game entry: '{recreatedTitle}'");
                                name = recreatedTitle;
                                displayName = $"{displayName} (↪ {name})";
                            }
                            else
                            {
                                logger.Warn($"Failed to re-create custom game entry: {recreateError}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Error checking custom game existence: {ex.Message}");
                    }
                }
            }

            var invocation = new Cli.Invocation(Mode.Backup).PathIf(settings.BackupPath, settings.OverrideBackupPath).Game(name);

            var (code, response) = app.Invoke(invocation);

            // Try auto-configuration for emulated games if:
            // 1. Ludusavi returned unknown games error, OR
            // 2. Ludusavi returned exit code 1 (no data found) which might indicate an unrecognized emulated game
            // 3. Ludusavi panicked (exit code 101) - could happen if a custom entry was corrupted
            // Note: Ludusavi can return exit code 1 ("no data found") with an *empty* games dictionary (not null).
            // Also note: `response` is `Cli.Output.Response?` (nullable struct), so we must use null-safe access.
            // We treat both null and empty as "no games found" to trigger emulated auto-configuration.
            if (criteria.ByGame() && (response?.Errors.UnknownGames != null || code == 101 || (code == 1 && ((response?.Games == null) || ((response?.Games?.Count ?? 0) == 0)))))
            {
                logger.Info($"Attempting auto-configuration for emulated game: {game.Name} (Ludusavi exit code: {code}, UnknownGames: {response?.Errors.UnknownGames != null})");
                if (TryAutoConfigureEmulatedGame(game, out var customTitle, out var autoError))
                {
                    logger.Info($"Auto-configuration successful for '{game.Name}': using custom title '{customTitle}'");
                    name = customTitle;
                    displayName = $"{displayName} (↪ {name})";
                    (code, response) = app.Invoke(invocation.Game(name));
                }
                else if (!string.IsNullOrEmpty(autoError))
                {
                    logger.Warn($"Auto-configuration failed for '{game.Name}': {autoError}");
                }
                else
                {
                    logger.Warn($"Auto-configuration failed for '{game.Name}' (no detailed error was provided).");
                }
            }

            if (!this.app.version.supportsFindCommand() && criteria.ByGame() && settings.AlternativeTitle(game) == null)
            {
                if (response?.Errors.UnknownGames != null && Etc.IsOnSteam(game))
                {
                    (code, response) = app.Invoke(invocation.BySteamId(game.GameId));
                }
                if (response?.Errors.UnknownGames != null && !Etc.IsOnPc(game) && settings.RetryNonPcGamesWithoutSuffix)
                {
                    (code, response) = app.Invoke(invocation.Game(game.Name));
                }
            }

            if (response == null)
            {
                interactor.NotifyError(translator.UnableToRunLudusavi(), timing);
                HandleFailureDuringPlay();
            }
            else
            {
                var result = new OperationResult { Name = displayName, Response = (Cli.Output.Response)response };
                if (code == 0)
                {
                    if (response?.Overall.TotalGames > 0)
                    {
                        if ((response?.Overall.ChangedGames?.Same ?? 0) == 0)
                        {
                            interactor.NotifyInfo(translator.BackUpOneGame_Success(result), timing);
                            HandleSuccessDuringPlay();
                        }
                        else
                        {
                            refresh = false;
                            interactor.NotifyInfo(translator.BackUpOneGame_Unchanged(result), timing);
                            HandleSuccessDuringPlay();
                        }
                    }
                    else
                    {
                        refresh = false;
                        interactor.NotifyError(translator.BackUpOneGame_Empty(result), timing);
                        HandleFailureDuringPlay();
                    }
                }
                else
                {
                    if (response?.Errors.UnknownGames != null)
                    {
                        refresh = false;
                        interactor.NotifyError(translator.BackUpOneGame_Empty(result), timing);
                        HandleFailureDuringPlay();
                    }
                    else
                    {
                        interactor.NotifyError(translator.BackUpOneGame_Failure(result), timing);
                        HandleFailureDuringPlay();
                    }
                }
            }

            NotifyResponseErrors(response);
            if (refresh)
            {
                Refresh(RefreshContext.CreatedBackup);
            }
            pendingOperation = false;
        }

        private void BackUpAllGames()
        {
            pendingOperation = true;
            var (code, response) = app.Invoke(new Cli.Invocation(Mode.Backup).PathIf(settings.BackupPath, settings.OverrideBackupPath));

            if (response == null)
            {
                interactor.NotifyError(translator.UnableToRunLudusavi());
            }
            else
            {
                var result = new OperationResult { Response = (Cli.Output.Response)response };

                if (code == 0)
                {
                    interactor.NotifyInfo(translator.BackUpAllGames_Success(result), () => ShowFullResults(result.Response));
                }
                else
                {
                    interactor.NotifyError(translator.BackUpAllGames_Failure(result), () => ShowFullResults(result.Response));
                }
            }

            NotifyResponseErrors(response);
            Refresh(RefreshContext.CreatedBackup);
            pendingOperation = false;
        }

        private void BackUpOneGameDuringPlay(Game game)
        {
            if (!CanPerformOperationSuppressed())
            {
                return;
            }
            var prefs = settings.GetPlayPreferences(game, multipleGamesRunning);
            Task.Run(() =>
            {
                if (prefs.Game.Backup.Do && !prefs.Game.Backup.Ask && settings.DoBackupDuringPlay)
                {
                    BackUpOneGame(game, OperationTiming.DuringPlay, BackupCriteria.Game);
                }

                if (prefs.Platform.Backup.Do && !prefs.Platform.Backup.Ask && settings.DoBackupDuringPlay)
                {
                    BackUpOneGame(game, OperationTiming.DuringPlay, BackupCriteria.Platform);
                }
            });
        }

        private RestorationError RestoreOneGame(Game game, Cli.Output.Backup? backup, BackupCriteria criteria)
        {
            RestorationError error = new RestorationError
            {
                Message = null,
                Empty = false,
            };

            pendingOperation = true;
            var name = criteria.ByPlatform() ? game.Platforms[0].Name : settings.GetGameNameWithAlt(game);
            var displayName = game.Name;

            if (this.app.version.supportsFindCommand())
            {
                var found = FindGame(game, name, OperationTiming.Free, criteria, Mode.Restore);
                if (found == null)
                {
                    pendingOperation = false;
                    error.Message = translator.UnrecognizedGame(name);
                    error.Empty = true;
                    return error;
                }
                name = found;
                if (name != displayName)
                {
                    displayName = $"{displayName} (↪ {name})";
                }
            }

            var invocation = new Cli.Invocation(Mode.Restore).PathIf(settings.BackupPath, settings.OverrideBackupPath).Game(name).Backup(backup?.Name);

            var (code, response) = app.Invoke(invocation);
            if (!this.app.version.supportsFindCommand() && criteria.ByGame() && settings.AlternativeTitle(game) == null)
            {
                if (response?.Errors.UnknownGames != null && Etc.IsOnSteam(game) && this.app.version.supportsRestoreBySteamId())
                {
                    (code, response) = app.Invoke(invocation.BySteamId(game.GameId));
                }
                if (response?.Errors.UnknownGames != null && !Etc.IsOnPc(game) && settings.RetryNonPcGamesWithoutSuffix)
                {
                    (code, response) = app.Invoke(invocation.Game(game.Name));
                }
            }

            if (response == null)
            {
                error.Message = translator.UnableToRunLudusavi();
                interactor.NotifyError(error.Message);
            }
            else
            {
                var result = new OperationResult { Name = displayName, Response = (Cli.Output.Response)response };
                if (code == 0)
                {
                    if (response?.Overall.TotalGames == 0)
                    {
                        // This applies to Ludusavi v0.23.0 and later
                        error.Message = translator.RestoreOneGame_Empty(result);
                        error.Empty = true;
                        interactor.NotifyError(error.Message);
                    }
                    else if ((response?.Overall.ChangedGames?.Same ?? 0) == 0)
                    {
                        interactor.NotifyInfo(translator.RestoreOneGame_Success(result));
                    }
                    else
                    {
                        interactor.NotifyInfo(translator.RestoreOneGame_Unchanged(result));
                    }
                }
                else
                {
                    if (response?.Errors.UnknownGames != null)
                    {
                        // This applies to Ludusavi versions before v0.23.0
                        error.Message = translator.RestoreOneGame_Empty(result);
                        error.Empty = true;
                        interactor.NotifyError(error.Message);
                    }
                    else
                    {
                        error.Message = translator.RestoreOneGame_Failure(result);
                        interactor.NotifyError(error.Message);
                    }
                }
            }

            NotifyResponseErrors(response);
            pendingOperation = false;
            return error;
        }

        private void RestoreAllGames()
        {
            pendingOperation = true;
            var (code, response) = app.Invoke(new Cli.Invocation(Mode.Restore).PathIf(settings.BackupPath, settings.OverrideBackupPath));

            if (response == null)
            {
                interactor.NotifyError(translator.UnableToRunLudusavi());
            }
            else
            {
                var result = new OperationResult { Response = (Cli.Output.Response)response };

                if (code == 0)
                {
                    interactor.NotifyInfo(translator.RestoreAllGames_Success(result), () => ShowFullResults(result.Response));
                }
                else
                {
                    interactor.NotifyError(translator.RestoreAllGames_Failure(result), () => ShowFullResults(result.Response));
                }

            }

            NotifyResponseErrors(response);
            pendingOperation = false;
        }

        private void TagGamesWithBackups()
        {
            using (PlayniteApi.Database.BufferedUpdate())
            {
                foreach (var game in PlayniteApi.Database.Games)
                {
                    if (IsBackedUp(game))
                    {
                        interactor.AddTag(game, Tags.BACKED_UP);
                    }
                    else
                    {
                        interactor.RemoveTag(game, Tags.BACKED_UP);
                    }
                }
            }
        }

        private void TagGamesWithUnknownSaveData()
        {
            using (PlayniteApi.Database.BufferedUpdate())
            {
                foreach (var game in PlayniteApi.Database.Games)
                {
                    if (!GameHasKnownSaveData(game))
                    {
                        interactor.AddTag(game, Tags.UNKNOWN_SAVE_DATA);
                    }
                    else
                    {
                        interactor.RemoveTag(game, Tags.UNKNOWN_SAVE_DATA);
                    }
                }
            }
        }

        private int CountGamesRunning()
        {
            return PlayniteApi.Database.Games.Count(x => x.IsRunning);
        }

        private bool IsBackedUp(Game game)
        {
            return GetBackups(game).Count > 0;
        }

        private bool GameHasKnownSaveData(Game game)
        {
            string title;
            if (this.app.version.supportsApiCommand())
            {
                title = GetTitle(game);
            }
            else
            {
                // Ideally, we would use the `find` command, but that's too slow to run in bulk.
                title = settings.AlternativeTitle(game) ?? settings.GetGameName(game);
            }

            if (title != null && this.app.manifestGamesWithSaveDataByTitle.Contains(title))
            {
                return true;
            }

            if (Etc.TrySteamId(game, out var id) && this.app.manifestGamesWithSaveDataBySteamId.Contains(id))
            {
                return true;
            }

            return false;
        }

        private List<Cli.Output.Backup> GetBackups(Game game)
        {
            if (this.app.version.supportsApiCommand())
            {
                var title = GetTitle(game);
                var backups = Etc.GetDictValue(this.app.backups, title, new List<Cli.Output.Backup>());

                // Sort newest backups to the top.
                backups.Sort((x, y) => y.When.CompareTo(x.When));

                return backups;
            }

            var ret = new List<Cli.Output.Backup>();
            var alt = settings.AlternativeTitle(game);

            if (alt != null)
            {
                ret = Etc.GetDictValue(this.app.backups, alt, new List<Cli.Output.Backup>());
            }
            else
            {
                ret = Etc.GetDictValue(
                    this.app.backups,
                    settings.GetGameName(game),
                    Etc.GetDictValue(
                        this.app.backups,
                        game.Name,
                        new List<Cli.Output.Backup>()
                    )
                );
            }

            // Sort newest backups to the top.
            ret.Sort((x, y) => y.When.CompareTo(x.When));

            return ret;
        }

        private string GetBackupPath(Game game)
        {
            return Etc.GetDictValue(this.app.backupPaths, GetTitle(game) ?? settings.GetGameNameWithAlt(game), null);
        }
    }
}
