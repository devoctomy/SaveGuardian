using System.Diagnostics.CodeAnalysis;

namespace SaveGuardian.Services;

[ExcludeFromCodeCoverage]
public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
}
