using System.Diagnostics;

namespace TeamsWatcher;

internal static class TeamsLauncher
{
    public static void Start()
    {
        try
        {
            // Neues Teams (Store-/MSIX-App)
            Process.Start(new ProcessStartInfo(
                "explorer.exe",
                @"shell:AppsFolder\MSTeams_8wekyb3d8bbwe!MSTeams")
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Teams konnte nicht gestartet werden:\n{ex.Message}",
                "Teams-Wächter",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
