using SaveGuardian;
using SaveGuardian.Services;
using System.Diagnostics.CodeAnalysis;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IIOService, IOService>();
        services.AddSingleton<IProcessService, ProcessService>();
        services.AddSingleton<IBackupFileNamingService, BackupFileNamingService>();
        services.AddSingleton<ISpecialFolderService, SpecialFolderService>();
        services.AddSingleton<IGuardianServiceConfigurator, GuardianServiceConfigurator>();
        services.AddSingleton<IBackupService, DefaultBackupService>();
        services.AddSingleton<IMultiFileSystemWatcherService, MultiFileSystemWatcherService>();
        services.AddSingleton<IGuardianService, GuardianService>();
        services.AddSingleton<IHashingService, Sha256HashingService>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();


[ExcludeFromCodeCoverage]
public partial class Program { }
