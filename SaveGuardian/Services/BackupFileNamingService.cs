using SaveGuardian.Model;

namespace SaveGuardian.Services
{
    public class BackupFileNamingService : IBackupFileNamingService
    {
        private readonly IDateTimeService _dateTimeService;

        public BackupFileNamingService(IDateTimeService dateTimeService)
        {
            _dateTimeService = dateTimeService;
        }

        public string Rename(
            VersionFolder versionFolder,
            string fullPath,
            string extension)
        {
            var backupPath = GetVersionFolderBackupPath(versionFolder).FullName.Replace('\\', '/').TrimStart('/'); ;
            var relativePath = fullPath.Replace(versionFolder.Path, string.Empty);
            var cleanPath = relativePath.Replace('\\', '/').TrimStart('/');
            var backupFullPath = $"{backupPath}/{cleanPath}_{_dateTimeService.Now:ddMMyyyy-HHmmss}.{extension}";
            return backupFullPath;
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
}
