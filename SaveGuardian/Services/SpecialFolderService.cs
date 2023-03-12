using System.Diagnostics.CodeAnalysis;

namespace SaveGuardian.Services;

[ExcludeFromCodeCoverage]
public class SpecialFolderService : ISpecialFolderService
{
    public string LocalApplicationData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
}
