using System.Globalization;
using System.Text;

namespace MintBlockchainBotConsoleUI.Helpers;
public static class ExceptionLogger
{
    private static readonly string _logDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

    static ExceptionLogger()
    {
        // Ensure the logs directory exists
        if (!Directory.Exists(_logDirectoryPath))
        {
            Directory.CreateDirectory(_logDirectoryPath);
        }
    }

    public static void Log(Exception ex)
    {
        StringBuilder log = new StringBuilder();
        log.AppendLine("Exception Type: " + ex.GetType().FullName);
        log.AppendLine("Message: " + ex.Message);
        log.AppendLine("Stack Trace: " + ex.StackTrace);
        log.AppendLine("Date/Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        if (ex.InnerException != null)
        {
            log.AppendLine("Inner Exception Type: " + ex.InnerException.GetType().FullName);
            log.AppendLine("Inner Exception Message: " + ex.InnerException.Message);
            log.AppendLine("Inner Exception Stack Trace: " + ex.InnerException.StackTrace);
        }

        string logFileName = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss_") + $"---{RandomNumber()}.txt";
        string logFilePath = Path.Combine(_logDirectoryPath, logFileName);
        File.WriteAllText(logFilePath, log.ToString());
    }


    /// <summary>
    /// 5 günden eski LOG dosyalarını siler.
    /// </summary>
    public static void DeleteOldLogFiles()
    {
        try
        {
            var files = Directory.GetFiles(_logDirectoryPath);

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var parts = fileName.Split('_');

                if (parts.Length > 0)
                {
                    if (DateTime.TryParseExact(parts[0], "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fileDate))
                    {
                        if ((DateTime.Today - fileDate).TotalDays > 5)
                        {
                            File.Delete(file);
                        }
                    }
                }
            }
        }
        catch (Exception) { }
    }

    private static Random rnd = new Random();
    public static int RandomNumber() => rnd.Next(100000,999999);
}
