using System.Windows;

namespace CosmicSandbox.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize logging
        Diagnostics.Logging.Logger.Initialize();

        // Set up crash reporting
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        Diagnostics.Logging.Logger.Information("Cosmic Sandbox started");
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        Diagnostics.Logging.Logger.Fatal(exception, "Unhandled exception occurred");

        MessageBox.Show(
            $"A fatal error occurred:\n\n{exception?.Message}\n\nPlease check the log file for details.",
            "Cosmic Sandbox - Fatal Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Diagnostics.Logging.Logger.Information("Cosmic Sandbox exiting");
        Diagnostics.Logging.Logger.Shutdown();
        base.OnExit(e);
    }
}
