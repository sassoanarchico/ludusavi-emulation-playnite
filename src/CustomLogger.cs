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
        private readonly object lockObject = new object();

        public CustomLogger(ILogger playniteLogger, string logDirectory)
        {
            this.playniteLogger = playniteLogger;
            this.logDirectory = logDirectory;
            
            // Crea la directory se non esiste
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Crea il percorso del file di log con timestamp
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd");
            logFilePath = Path.Combine(logDirectory, $"LudusaviPlaynite_{timestamp}.log");
            
            // Pulisci i vecchi file di log (più vecchi di 30 giorni)
            CleanOldLogFiles(30);
            
            // Scrivi intestazione
            WriteToFile($"=== LudusaviPlaynite Log Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
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
                    File.AppendAllText(logFilePath, message + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // Fallback al logger di Playnite se c'è un errore nella scrittura
                playniteLogger.Error(ex, "Failed to write to custom log file");
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
