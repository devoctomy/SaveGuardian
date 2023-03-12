using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaveGuardian.Model;

namespace SaveGuardian.Services;

public class GuardianServiceConfigurator : IGuardianServiceConfigurator
{
    public IReadOnlyList<VersionFolder>? VersionFolders => _versionFolders;

    private readonly IIOService _ioService;
    private List<VersionFolder>? _versionFolders;

    public GuardianServiceConfigurator(IIOService ioService)
    {
        _ioService = ioService;
    }

    public async Task<bool> InitialiseAsync(CancellationToken cancellationToken)
    {
        var configJson = await _ioService.ReadAllTextAsync("Config/folders.json", cancellationToken);
        if(string.IsNullOrEmpty(configJson))
        {
            return false;
        }

        var config = JObject.Parse(configJson);
        var versionFoldersArray = config["VersionFolders"]?.Value<JArray>();
        if (versionFoldersArray == null || versionFoldersArray.Count == 0)
        {
            return false;
        }

        _versionFolders = JsonConvert.DeserializeObject<List<VersionFolder>>(versionFoldersArray.ToString());
        return true;
    }
}
