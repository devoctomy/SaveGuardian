using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaveGuardian.Model;

namespace SaveGuardian.Services;

public class GuardianServiceConfigurator : IGuardianServiceConfigurator
{
    public IReadOnlyList<VersionFolder>? VersionFolders => _versionFolders;

    private List<VersionFolder>? _versionFolders;

    public async Task<bool> Initialise()
    {
        var configJson = await File.ReadAllTextAsync("Config/folders.json");
        var config = JObject.Parse(configJson);

        if (config == null)
        {
            return false;
        }

        var versionFoldersArray = config["VersionFolders"]?.Value<JArray>();
        if (versionFoldersArray == null || versionFoldersArray.Count == 0)
        {
            return false;
        }

        _versionFolders = JsonConvert.DeserializeObject<List<VersionFolder>>(versionFoldersArray.ToString());
        return true;
    }
}
