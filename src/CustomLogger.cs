using System;
using System.IO;
using System.Linq;
using Playnite.SDK;

namespace LudusaviPlaynite
{
    public class CustomLogger : ILogger
    {
        private readonly ILogger playniteLogger;
        private readonly string logFilePath;
        private readonly string logDirectory;
        private readonly string projectLogFilePath;
        private readonly string extraLogFilePath;
        private readonly object lockObject = new object();

        public CustomLogger(ILogger playniteLogger, string logDirectory)
        {
            this.playniteLogger = playniteLogger;
            this.logDirectory = logDirectory;

            try
            {
                // Log diagnostico per capire cosa succede
                playniteLogger.Info($"CustomLogger: Initializing with logDirectory='{logDirectory}'");

                // Crea la directory se non esiste
                if (string.IsNullOrWhiteSpace(logDirectory))
                {
                    playniteLogger.Error("CustomLogger: logDirectory is null or empty! Cannot create log files.");
                    throw new ArgumentException("logDirectory cannot be null or empty", nameof(logDirectory));
                }

                if (!Directory.Exists(logDirectory))
                {
                    playniteLogger.Info($"CustomLogger: Creating log directory: {logDirectory}");
                    try
                    {
                        Directory.CreateDirectory(logDirectory);
                        playniteLogger.Info($"CustomLogger: Successfully created log directory: {logDirectory}");
                    }
                    catch (Exception ex)
                    {
                        playniteLogger.Error(ex, $"CustomLogger: Failed to create log directory: {logDirectory}");
                        throw;
                    }
                }
                else
                {
                    playniteLogger.Info($"CustomLogger: Log directory already exists: {logDirectory}");
                }

                // Crea il percorso del file di log con timestamp
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd");
                logFilePath = Path.Combine(logDirectory, $"LudusaviPlaynite_{timestamp}.log");
                playniteLogger.Info($"CustomLogger: Log file path: {logFilePath}");

                // Cerca la cartella del progetto e crea un log anche lì
                projectLogFilePath = FindProjectLogPath(timestamp);
                if (!string.IsNullOrEmpty(projectLogFilePath))
                {
                    playniteLogger.Info($"CustomLogger: Project log file: {projectLogFilePath}");
                }

                // Optional extra log directory configured by user via a file next to the DLL
                extraLogFilePath = FindExtraLogPath(timestamp);
                if (!string.IsNullOrEmpty(extraLogFilePath))
                {
                    playniteLogger.Info($"CustomLogger: Extra log file: {extraLogFilePath}");
                }

                // Pulisci i vecchi file di log (più vecchi di 30 giorni)
                CleanOldLogFiles(30);

                // Scrivi intestazione
                WriteToFile($"=== LudusaviPlaynite Log Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
                playniteLogger.Info("CustomLogger: Initialization complete");
            }
            catch (Exception ex)
            {
                playniteLogger.Error(ex, "CustomLogger: Critical error during initialization");
                throw;
            }
        }

        /// <summary>
        /// Trova la cartella del progetto cercando titleid.txt o altri file caratteristici del progetto.
        /// </summary>
        private string FindProjectLogPath(string timestamp)
        {
            try
            {
                // Prova a trovare la cartella del progetto cercando titleid.txt
                var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(assemblyLocation))
                {
                    var currentDir = Path.GetDirectoryName(assemblyLocation);
                    
                    // Cerca titleid.txt nella directory corrente e nelle parent
                    for (int i = 0; i < 5 && !string.IsNullOrEmpty(currentDir); i++)
                    {
                        var titleIdPath = Path.Combine(currentDir, "titleid.txt");
                        if (File.Exists(titleIdPath))
                        {
                            // Consider it a "project" folder only if it looks like a repo (has .git)
                            var gitDir = Path.Combine(currentDir, ".git");
                            if (Directory.Exists(gitDir))
                            {
                                var projectLogDir = Path.Combine(currentDir, "logs");
                                if (!Directory.Exists(projectLogDir))
                                {
                                    Directory.CreateDirectory(projectLogDir);
                                }
                                return Path.Combine(projectLogDir, $"LudusaviPlaynite_{timestamp}.log");
                            }
                        }
                        currentDir = Path.GetDirectoryName(currentDir);
                    }
                }
            }
            catch
            {
                // Ignora errori nella ricerca della cartella progetto
            }
            
            return null;
        }

        /// <summary>
        /// Optional: user can create a file next to the plugin DLL named 'extra-log-dir.txt'
        /// containing an absolute directory path where logs should also be written.
        /// </summary>
        private string FindExtraLogPath(string timestamp)
        {
            try
            {
                var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(assemblyLocation))
                {
                    return null;
                }

                var assemblyDir = Path.GetDirectoryName(assemblyLocation);
                if (string.IsNullOrEmpty(assemblyDir))
                {
                    return null;
                }

                var cfgPath = Path.Combine(assemblyDir, "extra-log-dir.txt");
                if (!File.Exists(cfgPath))
                {
                    return null;
                }

                var targetDir = File.ReadAllText(cfgPath)?.Trim();
                if (string.IsNullOrWhiteSpace(targetDir))
                {
                    return null;
                }

                // Allow relative paths relative to the DLL directory
                if (!Path.IsPathRooted(targetDir))
                {
                    targetDir = Path.GetFullPath(Path.Combine(assemblyDir, targetDir));
                }

                var extraLogDir = Path.Combine(targetDir, "logs");
                if (!Directory.Exists(extraLogDir))
                {
                    Directory.CreateDirectory(extraLogDir);
                }

                return Path.Combine(extraLogDir, $"LudusaviPlaynite_{timestamp}.log");
            }
            catch
            {
                return null;
            }
        }

        private void CleanOldLogFiles(int daysToKeep)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var oldFiles = Directory.GetFiles(logDirectory, "LudusaviPlaynite_*.log")
                    .Where(file =>
                    {
                        var fileInfo = new FileInfo(file);
                        return fileInfo.LastWriteTime < cutoffDate;
                    })
                    .ToList();

                foreach (var file in oldFiles)
                {
                    try
                    {
                        File.Delete(file);
                        playniteLogger.Debug($"Deleted old log file: {Path.GetFileName(file)}");
                    }
                    catch (Exception ex)
                    {
                        playniteLogger.Warn(ex, $"Failed to delete old log file: {Path.GetFileName(file)}");
                    }
                }

                if (oldFiles.Count > 0)
                {
                    WriteToFile($"Cleaned up {oldFiles.Count} old log file(s)");
                }
            }
            catch (Exception ex)
            {
                playniteLogger.Error(ex, "Failed to clean old log files");
            }
        }

        private void WriteToFile(string message)
        {
            try
            {
                lock (lockObject)
                {
                    // Scrivi nel log principale (cartella estensione)
                    if (string.IsNullOrEmpty(logFilePath))
                    {
                        playniteLogger.Warn("CustomLogger: logFilePath is empty, cannot write log");
                        return;
                    }

                    try
                    {
                        // Assicurati che la directory esista ancora (potrebbe essere stata cancellata)
                        var dir = Path.GetDirectoryName(logFilePath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        File.AppendAllText(logFilePath, message + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        playniteLogger.Error(ex, $"CustomLogger: Failed to write to main log file: {logFilePath}");
                        // Non rilanciare, continua con gli altri log
                    }
                    
                    // Scrivi anche nel log del progetto se disponibile
                    if (!string.IsNullOrEmpty(projectLogFilePath))
                    {
                        try
                        {
                            var projDir = Path.GetDirectoryName(projectLogFilePath);
                            if (!string.IsNullOrEmpty(projDir) && !Directory.Exists(projDir))
                            {
                                Directory.CreateDirectory(projDir);
                            }
                            File.AppendAllText(projectLogFilePath, message + Environment.NewLine);
                        }
                        catch (Exception ex)
                        {
                            // Ignora errori nella scrittura del log progetto (non critico)
                            playniteLogger.Debug(ex, $"CustomLogger: Failed to write to project log: {projectLogFilePath}");
                        }
                    }

                    // Scrivi anche nel log extra se configurato
                    if (!string.IsNullOrEmpty(extraLogFilePath))
                    {
                        try
                        {
                            var extraDir = Path.GetDirectoryName(extraLogFilePath);
                            if (!string.IsNullOrEmpty(extraDir) && !Directory.Exists(extraDir))
                            {
                                Directory.CreateDirectory(extraDir);
                            }
                            File.AppendAllText(extraLogFilePath, message + Environment.NewLine);
                        }
                        catch (Exception ex)
                        {
                            // Ignora errori nella scrittura del log extra (non critico)
                            playniteLogger.Debug(ex, $"CustomLogger: Failed to write to extra log: {extraLogFilePath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback al logger di Playnite se c'è un errore nella scrittura
                playniteLogger.Error(ex, "CustomLogger: Critical error in WriteToFile");
            }
        }

        private string FormatMessage(string level, string message)
        {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}|{level,-5}|LudusaviPlaynite: {message}";
        }

        public void Debug(string message)
        {
            WriteToFile(FormatMessage("DEBUG", message));
            playniteLogger.Debug(message);
        }

        public void Debug(Exception exception, string message)
        {
            string fullMessage = $"{message}{Environment.NewLine}Exception: {exception}";
            WriteToFile(FormatMessage("DEBUG", fullMessage));
            playniteLogger.Debug(exception, message);
        }

        public void Error(string message)
        {
            WriteToFile(FormatMessage("ERROR", message));
            playniteLogger.Error(message);
        }

        public void Error(Exception exception, string message)
        {
            string fullMessage = $"{message}{Environment.NewLine}Exception: {exception}";
            WriteToFile(FormatMessage("ERROR", fullMessage));
            playniteLogger.Error(exception, message);
        }

        public void Info(string message)
        {
            WriteToFile(FormatMessage("INFO", message));
            playniteLogger.Info(message);
        }

        public void Info(Exception exception, string message)
        {
            string fullMessage = $"{message}{Environment.NewLine}Exception: {exception}";
            WriteToFile(FormatMessage("INFO", fullMessage));
            playniteLogger.Info(exception, message);
        }

        public void Trace(string message)
        {
            WriteToFile(FormatMessage("TRACE", message));
            playniteLogger.Trace(message);
        }

        public void Trace(Exception exception, string message)
        {
            string fullMessage = $"{message}{Environment.NewLine}Exception: {exception}";
            WriteToFile(FormatMessage("TRACE", fullMessage));
            playniteLogger.Trace(exception, message);
        }

        public void Warn(string message)
        {
            WriteToFile(FormatMessage("WARN", message));
            playniteLogger.Warn(message);
        }

        public void Warn(Exception exception, string message)
        {
            string fullMessage = $"{message}{Environment.NewLine}Exception: {exception}";
            WriteToFile(FormatMessage("WARN", fullMessage));
            playniteLogger.Warn(exception, message);
        }
    }
}
