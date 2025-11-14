using FinanceTracker.Application.DTOs;

namespace FinanceTracker.Application.Interfaces.Services;

public interface IReportService
{
    Task<FinanceReport> CreateDailyReportAsync(DateOnly date, CancellationToken ct = default);
    Task<FinanceReport> CreatePeriodReportAsync(DateOnly start, DateOnly end, CancellationToken ct = default);

}
