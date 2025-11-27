using FinanceTracker.Application.DTOs.Report;

namespace FinanceTracker.Application.Interfaces.Services;

public interface IReportService
{
    Task<FinanceReportDto> CreateDailyReportAsync(Guid walletId, DateOnly date, CancellationToken ct = default);
    Task<FinanceReportDto> CreatePeriodReportAsync(Guid walletId, DateOnly start, DateOnly end, CancellationToken ct = default);

}
