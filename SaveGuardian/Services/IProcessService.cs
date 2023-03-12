namespace SaveGuardian.Services
{
    public interface IProcessService
    {
        public bool IsRunning(List<string> processNames);
    }
}
