using SaveGuardian.Model;

namespace SaveGuardian.Services
{
    public class BackupFileNamingService : IBackupFileNamingService
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly ISpecialFolderService _specialFolderService;

        public BackupFileNamingService(
            IDateTimeService dateTimeService,
            ISpecialFolderService specialFolderService)
        {
            _dateTimeService = dateTimeService;
            _specialFolderService = specialFolderService;
        }

        public string Rename(
            VersionFolder versionFolder,
            string fullPath,
            string extension)
        {
            var backupPath = GetVersionFolderBackupPath(versionFolder);
            var relativePath = fullPath.Replace(versionFolder.Path, string.Empty).TrimStart('/');
            var backupFullPath = $"{backupPath}/{relativePath}_{_dateTimeService.Now:ddMMyyyy-HHmmss}.{extension}";
            return backupFullPath.Replace('\\', '/');
        }

        private string GetVersionFolderBackupPath(VersionFolder versionFolder)
        {
            var backupRoot = _specialFolderService.LocalApplicationData;
            if (!backupRoot.EndsWith("/"))
            {
                backupRoot += "/";
            }
            backupRoot += $"SaveVersioningPoc/{versionFolder.Name}";
            Directory.CreateDirectory(backupRoot);
            return backupRoot;
        }
    }
}
