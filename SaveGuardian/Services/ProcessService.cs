using System.Diagnostics.CodeAnalysis;

namespace SaveGuardian.Services;

[ExcludeFromCodeCoverage]
public class ProcessService : IProcessService
{
    public bool IsRunning(List<string> processNames)
    {
        return processNames.Any(x => System.Diagnostics.Process.GetProcessesByName(x).Length > 0);
    }
}
