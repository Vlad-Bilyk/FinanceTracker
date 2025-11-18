using System.Text.Json.Serialization;

namespace FinanceTracker.Application.DTOs.ExchangeRate;

public class HistoricalRateResponse
{
    [JsonPropertyName("data")]
    public Dictionary<string, Dictionary<string, decimal>> Data { get; set; } = [];
}
