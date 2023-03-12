using SaveGuardian.Model;

namespace SaveGuardian;

public class BackupOperation
{
    public event EventHandler? Ready;

    public VersionFolder VersionFolder { get; set; }
    public string Path { get; set; }
    public DateTime LastUpdate { get; set; }

    private System.Timers.Timer _graceTimer;

    public BackupOperation(
        VersionFolder versionFolder,
        string path)
    {
        VersionFolder = versionFolder;
        Path = path;
        LastUpdate = DateTime.UtcNow;
        _graceTimer = new System.Timers.Timer(versionFolder.ChangeGracePeriod.TotalMilliseconds);
        _graceTimer.Elapsed += _graceTimer_Elapsed;
    }

    public void Changed()
    {
        _graceTimer.Stop();
        LastUpdate = DateTime.UtcNow;
        _graceTimer.Start();
    }

    private void _graceTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _graceTimer.Stop();
        Console.WriteLine("Grace timer elapsed for backup operation.");
        Ready?.Invoke(this, EventArgs.Empty);
    }
}
