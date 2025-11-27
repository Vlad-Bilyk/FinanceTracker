using System.Text.Json.Serialization;

namespace FinanceTracker.Application.DTOs.ExchangeRate;

public class CurrencyShortInfo
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}
