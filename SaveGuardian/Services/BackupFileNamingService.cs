using SaveGuardian.Model;

namespace SaveGuardian.Services
{
    public class BackupFileNamingService : IBackupFileNamingService
    {
        public string Rename(
            VersionFolder versionFolder,
            string fullPath,
            string extension)
        {
            var backupPath = GetVersionFolderBackupPath(versionFolder);
            var relativePath = fullPath.Replace(versionFolder.Path, string.Empty);
            var cleanPath = relativePath.Replace('\\', '/').TrimStart('/');
            var backupFullPath = $"{backupPath}/{cleanPath}_{DateTime.Now.ToString("ddMMyyyy-HHmmss")}.{extension}";
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
