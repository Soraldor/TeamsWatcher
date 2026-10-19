using System.Diagnostics;

namespace TeamsWatcher;

internal sealed class TrayContext : ApplicationContext
{
    // ---- Einstellungen ----
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan AlertAfter = TimeSpan.FromMinutes(5);

    // "ms-teams" = neues Teams (2.x), "Teams" = klassisches Teams
    private static readonly string[] ProcessNames = ["ms-teams", "Teams"];

    private readonly NotifyIcon _tray;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly ToolStripMenuItem _statusItem;
    private readonly ToolStripMenuItem _pauseItem;

    private DateTime? _offSince;
    private bool _alertShown;
    private bool _paused;
    private AlertForm? _openAlert;

    public TrayContext()
    {
        _statusItem = new ToolStripMenuItem("Status wird ermittelt …") { Enabled = false };
        _pauseItem = new ToolStripMenuItem("Überwachung pausieren", null, (_, _) => TogglePause());

        var menu = new ContextMenuStrip();
        menu.Items.Add(_statusItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Teams starten", null, (_, _) => TeamsLauncher.Start());
        menu.Items.Add(_pauseItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Beenden", null, (_, _) => ExitThread());

        _tray = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Teams-Wächter",
            ContextMenuStrip = menu,
            Visible = true
        };

        _timer = new System.Windows.Forms.Timer { Interval = (int)CheckInterval.TotalMilliseconds };
        _timer.Tick += (_, _) => Check();
        _timer.Start();

        Check();
    }

    private void Check()
    {
        if (_paused)
            return;

        if (IsTeamsRunning())
        {
            _offSince = null;
            _alertShown = false;
            _openAlert?.Close();   // Teams läuft wieder -> offenes Popup weg
            SetState("Teams läuft", SystemIcons.Application);
            return;
        }

        _offSince ??= DateTime.Now;
        var off = DateTime.Now - _offSince.Value;
        SetState($"Teams aus seit {(int)off.TotalMinutes} min", SystemIcons.Warning);

        if (!_alertShown && off >= AlertAfter)
        {
            _alertShown = true;
            ShowAlert();
        }
    }

    private void ShowAlert()
    {
        var alert = new AlertForm(_offSince ?? DateTime.Now);
        alert.FormClosed += (_, _) =>
        {
            _openAlert = null;
            switch (alert.Result)
            {
                case AlertResult.StartTeams:
                    TeamsLauncher.Start();
                    break;
                case AlertResult.Snooze:
                    // In 5 Minuten erneut erinnern
                    _offSince = DateTime.Now;
                    _alertShown = false;
                    break;
                // Ignore/X: keine weitere Meldung, bis Teams wieder lief
            }
        };
        _openAlert = alert;
        alert.Show();
        alert.Activate();
    }

    private void TogglePause()
    {
        _paused = !_paused;
        _pauseItem.Text = _paused ? "Überwachung fortsetzen" : "Überwachung pausieren";

        _offSince = null;
        _alertShown = false;

        if (_paused)
            SetState("Überwachung pausiert", SystemIcons.Information);
        else
            Check();
    }

    private void SetState(string text, Icon icon)
    {
        _statusItem.Text = text;
        var tip = $"Teams-Wächter – {text}";
        _tray.Text = tip.Length > 63 ? tip[..63] : tip;
        if (!ReferenceEquals(_tray.Icon, icon))
            _tray.Icon = icon;
    }

    private static bool IsTeamsRunning()
    {
        foreach (var name in ProcessNames)
        {
            var procs = Process.GetProcessesByName(name);
            bool found = procs.Length > 0;
            foreach (var p in procs) p.Dispose();
            if (found) return true;
        }
        return false;
    }

    protected override void ExitThreadCore()
    {
        _timer.Stop();
        _timer.Dispose();
        _openAlert?.Close();
        _tray.Visible = false;
        _tray.Dispose();
        base.ExitThreadCore();
    }
}
