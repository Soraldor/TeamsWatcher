namespace TeamsWatcher;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Verhindert, dass das Tool mehrfach gleichzeitig läuft
        using var mutex = new Mutex(true, "TeamsWatcher_SingleInstance", out bool isFirstInstance);
        if (!isFirstInstance)
            return;

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayContext());
    }
}
