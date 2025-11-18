using System.Text.Json.Serialization;

namespace FinanceTracker.Application.DTOs.ExchangeRate;

public class CurrenciesResponse
{
    [JsonPropertyName("data")]
    public Dictionary<string, CurrencyShortInfo> Data { get; set; } = [];
}
