using SaveGuardian;
using SaveGuardian.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IGuardianServiceConfigurator, GuardianServiceConfigurator>();
        services.AddSingleton<IBackupService, DefaultBackupService>();
        services.AddSingleton<IGuardianService, GuardianService>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
