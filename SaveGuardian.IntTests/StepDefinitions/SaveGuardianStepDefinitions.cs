using Newtonsoft.Json;
using SaveGuardian.Model;
using SaveGuardian.Services;
using System;
using System.Diagnostics;
using System.Reflection;

namespace SaveGuardian.IntTests.StepDefinitions
{
    [Binding]
    public sealed class SaveGuardianStepDefinitions
    {
        private readonly ScenarioContext _context;

        public SaveGuardianStepDefinitions(ScenarioContext context)
        {
            _context = context;
        }

        [Given(@"VersionFolder configured with name (.*) for path (.*) with filter (.*) and grace period of (.*) seconds")]
        public void GivenVersionFolderConfiguredWithNameForPathAndFilter(
            string name,
            string path,
            string filter,
            int gracePeriodSeconds)
        {
            var versionFolders = _context.ContainsKey("VersionFolders") ?
                _context.Get<List<VersionFolder>>("VersionFolders") :
                new List<VersionFolder>();
            var testVersionFolderPath = Path.Combine(GetExecutingDirectory() ?? string.Empty, path).Replace("\\", "/");

            versionFolders.Add(new VersionFolder
            {
                Name = name,
                Path = testVersionFolderPath,
                Filter = filter,
                ChangeGracePeriod = new TimeSpan(0, 0, gracePeriodSeconds)
            });

            Directory.CreateDirectory(path);

            _context.Set(
                versionFolders,
                "VersionFolders");
        }

        [Given(@"VersionFolders configuration file created at (.*)")]
        public async Task GivenVersionFoldersConfigurationFileCreatedAt(string configurationPath)
        {
            var folders = _context.Get<List<VersionFolder>>("VersionFolders");
            var versionFoldersConfiguration = new
            {
                VersionFolders = folders
            };

            await File.WriteAllTextAsync(
                configurationPath,
                JsonConvert.SerializeObject(versionFoldersConfiguration, Formatting.Indented));
        }

        [When(@"SaveGuardian is started")]
        public void WhenSaveGuardianIsStarted()
        {
            var directory = GetExecutingDirectory() ?? string.Empty;
            Assert.False(string.IsNullOrEmpty(directory));

            var executablePath = Path.Combine(directory, "SaveGuardian.exe");
            Assert.True(File.Exists(executablePath));

            var processStartInfo = new ProcessStartInfo(executablePath)
            {
                RedirectStandardOutput = true
            };
            var process = Process.Start(processStartInfo);
            _context.Set(process, "SaveGuardianProcess");
        }

        [When(@"file created in (.*) with extension (.*) with key (.*)")]
        public async Task WhenFileCreatedInWithExtensionExtension(
            string path,
            string newFileExtension,
            string key)
        {
            var fileKeys = _context.ContainsKey("FileKeys") ?
                _context.Get<Dictionary<string, string>>("FileKeys") :
                new Dictionary<string, string>();

            var testVersionFolderPath = Path.Combine(GetExecutingDirectory() ?? string.Empty, path).Replace("\\", "/");

            var newFileName = $"{testVersionFolderPath}{Guid.NewGuid()}{newFileExtension}";
            var contents = Guid.NewGuid().ToString();
            await File.WriteAllTextAsync(newFileName, contents);

            fileKeys.Add(key, newFileName);
            _context.Set(fileKeys, "FileKeys");
            _context.Set(contents, $"{key}Contents");
        }

        [Given(@"wait for (.*) seconds")]
        [When(@"wait for (.*) seconds")]
        [Then(@"wait for (.*) seconds")]
        public async Task WhenWaitForSeconds(int seconds)
        {
            await Task.Delay(new TimeSpan(0, 0, seconds));
        }

        [Then(@"copy of (.*) created in version folder (.*) backup path")]
        public async Task ThenCopyOfCreatedIn(
            string fileKey,
            string name)
        {
            var fileKeys = _context.Get<Dictionary<string, string>>("FileKeys");
            var source = fileKeys[fileKey];
            var sourceName = new FileInfo(source).Name;
            var backupPath = GetVersionFolderBackupPath(name).Replace("\\", "/");
            var backupFullPathWithoutSuffix = Path.Combine(backupPath, sourceName).Replace("\\", "/");
            var allBackupFiles = Directory.GetFiles(backupPath, "*", SearchOption.TopDirectoryOnly);
            allBackupFiles = allBackupFiles.Select(x => x.Replace("\\", "/")).ToArray();
            var suitableBackup = allBackupFiles.SingleOrDefault(x => x.StartsWith(backupFullPathWithoutSuffix, StringComparison.InvariantCultureIgnoreCase) && ! x.EndsWith(".hash"));
            Assert.NotNull(suitableBackup);
            var expectedContents = _context.Get<string>($"{fileKey}Contents");
            Assert.Equal(expectedContents, await File.ReadAllTextAsync(suitableBackup));

            using var hashingService = new Sha256HashingService();
            var suitableHashFile = allBackupFiles.SingleOrDefault(x => x.StartsWith(backupFullPathWithoutSuffix, StringComparison.InvariantCultureIgnoreCase) && x.EndsWith(".hash"));
            Assert.NotNull(suitableHashFile);
            Assert.Equal(await File.ReadAllTextAsync(suitableHashFile), await hashingService.HashFileAsync(suitableBackup, CancellationToken.None));
        }

        [Then(@"SaveGuardian process is closed")]
        public async Task ThenSaveGuardianProcessIsClosed()
        {
            var saveGuardianProcess = _context.Get<Process>("SaveGuardianProcess");
            try
            {
                var output = new List<string>();
                while (saveGuardianProcess.StandardOutput.Peek() > -1)
                {
                    var line = saveGuardianProcess.StandardOutput.ReadLine();
                    if(line != null)
                    {
                        output.Add(line);
                    }
                }
                saveGuardianProcess.Close();
                while (!saveGuardianProcess.HasExited)
                {
                    await Task.Delay(new TimeSpan(0, 0, 2));
                    saveGuardianProcess.Close();
                }
            }
            catch (InvalidOperationException)
            {
                // Do nothing
            }
        }

        private static string? GetExecutingDirectory()
        {
            string location = Assembly.GetExecutingAssembly().Location;
            return new FileInfo(location).Directory?.FullName;
        }

        private string GetVersionFolderBackupPath(string versionFolderName)
        {
            var backupRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!backupRoot.EndsWith("/"))
            {
                backupRoot += "/";
            }
            backupRoot += $"SaveGuardian/{versionFolderName}";
            return backupRoot;
        }
    }
}