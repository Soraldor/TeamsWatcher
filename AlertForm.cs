using System.Media;

namespace TeamsWatcher;

internal enum AlertResult { None, StartTeams, Snooze, Ignore }

internal sealed class AlertForm : Form
{
    public AlertResult Result { get; private set; } = AlertResult.None;

    public AlertForm(DateTime offSince)
    {
        Text = "Teams-Wächter";
        TopMost = true;
        ShowInTaskbar = true;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(16);
        Icon = SystemIcons.Warning;

        var label = new Label
        {
            Text = $"Microsoft Teams läuft seit {offSince:HH:mm} Uhr nicht mehr.\n\nSoll Teams gestartet werden?",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 10f),
            Margin = new Padding(0, 0, 0, 16)
        };

        var btnStart = MakeButton("Teams starten", AlertResult.StartTeams);
        var btnSnooze = MakeButton("In 5 Min. erinnern", AlertResult.Snooze);
        var btnIgnore = MakeButton("Ignorieren", AlertResult.Ignore);

        var buttons = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        buttons.Controls.AddRange([btnStart, btnSnooze, btnIgnore]);

        var layout = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        layout.Controls.Add(label);
        layout.Controls.Add(buttons);
        Controls.Add(layout);

        AcceptButton = btnStart;
        CancelButton = btnIgnore;

        Shown += (_, _) => SystemSounds.Exclamation.Play();
    }

    private Button MakeButton(string text, AlertResult result)
    {
        var b = new Button { Text = text, AutoSize = true, Padding = new Padding(8, 2, 8, 2) };
        b.Click += (_, _) => { Result = result; Close(); };
        return b;
    }
}
