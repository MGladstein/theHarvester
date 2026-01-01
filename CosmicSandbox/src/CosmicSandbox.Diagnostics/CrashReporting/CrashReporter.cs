using System.Diagnostics;
using System.Text;

namespace CosmicSandbox.Diagnostics.CrashReporting;

/// <summary>
/// Generates crash reports with system information
/// </summary>
public class CrashReporter
{
    private readonly string _crashReportPath;

    public CrashReporter(string? crashReportPath = null)
    {
        _crashReportPath = crashReportPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CosmicSandbox",
            "CrashReports"
        );

        Directory.CreateDirectory(_crashReportPath);
    }

    /// <summary>
    /// Generate a crash report
    /// </summary>
    public string GenerateCrashReport(Exception exception)
    {
        var timestamp = DateTime.Now;
        var fileName = $"crash_{timestamp:yyyyMMdd_HHmmss}.txt";
        var filePath = Path.Combine(_crashReportPath, fileName);

        var report = new StringBuilder();

        report.AppendLine("=".PadRight(80, '='));
        report.AppendLine("COSMIC SANDBOX CRASH REPORT");
        report.AppendLine("=".PadRight(80, '='));
        report.AppendLine();

        report.AppendLine($"Timestamp: {timestamp:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"Version: 1.0.0");
        report.AppendLine();

        report.AppendLine("SYSTEM INFORMATION");
        report.AppendLine("-".PadRight(80, '-'));
        report.AppendLine($"OS: {Environment.OSVersion}");
        report.AppendLine($"64-bit OS: {Environment.Is64BitOperatingSystem}");
        report.AppendLine($"64-bit Process: {Environment.Is64BitProcess}");
        report.AppendLine($"CLR Version: {Environment.Version}");
        report.AppendLine($"Processor Count: {Environment.ProcessorCount}");
        report.AppendLine($"Working Set: {Environment.WorkingSet / 1024 / 1024} MB");
        report.AppendLine();

        report.AppendLine("EXCEPTION DETAILS");
        report.AppendLine("-".PadRight(80, '-'));
        report.AppendLine($"Type: {exception.GetType().FullName}");
        report.AppendLine($"Message: {exception.Message}");
        report.AppendLine($"Source: {exception.Source}");
        report.AppendLine();

        report.AppendLine("STACK TRACE");
        report.AppendLine("-".PadRight(80, '-'));
        report.AppendLine(exception.StackTrace);
        report.AppendLine();

        if (exception.InnerException != null)
        {
            report.AppendLine("INNER EXCEPTION");
            report.AppendLine("-".PadRight(80, '-'));
            report.AppendLine($"Type: {exception.InnerException.GetType().FullName}");
            report.AppendLine($"Message: {exception.InnerException.Message}");
            report.AppendLine($"Stack Trace: {exception.InnerException.StackTrace}");
            report.AppendLine();
        }

        report.AppendLine("=".PadRight(80, '='));

        File.WriteAllText(filePath, report.ToString());

        return filePath;
    }

    /// <summary>
    /// Get all crash reports
    /// </summary>
    public List<string> GetCrashReports()
    {
        if (!Directory.Exists(_crashReportPath))
            return new List<string>();

        return Directory.GetFiles(_crashReportPath, "crash_*.txt")
            .OrderByDescending(f => File.GetCreationTime(f))
            .ToList();
    }

    /// <summary>
    /// Clean old crash reports (keep last 10)
    /// </summary>
    public void CleanOldReports(int keepCount = 10)
    {
        var reports = GetCrashReports();

        if (reports.Count > keepCount)
        {
            foreach (var report in reports.Skip(keepCount))
            {
                try
                {
                    File.Delete(report);
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        }
    }
}
