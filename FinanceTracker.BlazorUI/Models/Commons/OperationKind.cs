using System.Text.Json.Serialization;

namespace FinanceTracker.BlazorUI.Models.Commons;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationKind
{
    Income = 1,
    Expense = 2,
}
