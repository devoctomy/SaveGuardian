using SaveGuardian.Model;

namespace SaveGuardian.Services;

public class DefaultBackupService : IBackupService
{
    private readonly ILogger<DefaultBackupService> _logger;

    public DefaultBackupService(ILogger<DefaultBackupService> logger)
    {
        _logger = logger;
    }

    public bool Process(
        VersionFolder versionFolder,
        string path)
    {
        var block = versionFolder.BackupBlockingProcesses.Any(x => System.Diagnostics.Process.GetProcessesByName(x).Length > 0);
        if (block)
        {
            return false;
        }

        var backupPath = GetVersionFolderBackupPath(versionFolder);
        var relativePath = path.Replace(versionFolder.Path, string.Empty);
        var cleanPath = relativePath.Replace('\\', '/').TrimStart('/');
        var fileName = cleanPath[(cleanPath.LastIndexOf('/') + 1)..];
        var pathNoFile = cleanPath[..cleanPath.LastIndexOf('/')];
        var backupFullPath = $"{backupPath}/{cleanPath}_{DateTime.Now.ToString("ddMMyyyy-HHmmss")}.bak";

        backupPath.CreateSubdirectory(pathNoFile);
        File.Copy(
            path,
            backupFullPath);

        _logger.LogInformation($"Creating versioned copy of file '{relativePath}' for version folder '{versionFolder.Name}'");
        return true;
    }

    private static DirectoryInfo GetVersionFolderBackupPath(VersionFolder versionFolder)
    {
        var backupRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var backupRootDI = new DirectoryInfo(backupRoot);
        var appDirDI = backupRootDI.CreateSubdirectory("SaveVersioningPoc");
        var versionFolderDir = appDirDI.CreateSubdirectory(versionFolder.Name);
        return versionFolderDir;
    }
}
