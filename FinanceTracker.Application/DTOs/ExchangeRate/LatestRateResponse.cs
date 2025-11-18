using System.Text.Json.Serialization;

namespace FinanceTracker.Application.DTOs.ExchangeRate;

public class LatestRateResponse
{
    [JsonPropertyName("data")]
    public Dictionary<string, decimal> Data { get; set; } = [];
}
