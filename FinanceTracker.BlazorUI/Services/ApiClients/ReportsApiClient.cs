using FinanceTracker.BlazorUI.Models.Report;
using System.Net.Http.Json;

namespace FinanceTracker.BlazorUI.Services.ApiClients;

public class ReportsApiClient
{
    private readonly HttpClient _httpClient;

    public ReportsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<FinanceReportDto> GetDailyReportAsync(
        Guid walletId, DateOnly date, CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<FinanceReportDto>(
            $"api/wallets/{walletId}/reports/daily?date={date}", ct);
        return result ?? new FinanceReportDto();
    }

    public async Task<FinanceReportDto> GetPeriodReportAsync(
        Guid walletId, DateOnly startDate, DateOnly endDate, CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<FinanceReportDto>(
            $"api/wallets/{walletId}/reports/period?start={startDate}&end={endDate}", ct);
        return result ?? new FinanceReportDto();
    }
}
