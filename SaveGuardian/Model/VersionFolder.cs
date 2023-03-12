namespace SaveGuardian.Model;

public class VersionFolder
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Filter { get; set; } = string.Empty;
    public bool IncludeSubdirectories { get; set; }
    public TimeSpan ChangeGracePeriod { get; set; }
    public List<string> BackupBlockingProcesses { get; } = new List<string>();
}
