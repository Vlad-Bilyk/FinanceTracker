namespace FinanceTracker.BlazorUI.Models.Commons;

public class ApiResult
{
    public bool IsSuccess { get; }
    public string[] GeneralErrors { get; }
    public IDictionary<string, string[]>? FieldErrors { get; }

    public ApiResult(bool isSuccess,
        IEnumerable<string>? generalErrors = null,
        IDictionary<string, string[]>? fieldErrors = null)
    {
        IsSuccess = isSuccess;
        GeneralErrors = generalErrors?.ToArray() ?? [];
        FieldErrors = fieldErrors ?? new Dictionary<string, string[]>();
    }

    public static ApiResult Success()
    {
        return new ApiResult(true);
    }

    public static ApiResult Failure(IEnumerable<string>? generalErrors = null,
        IDictionary<string, string[]>? fieldErrors = null)
    {
        return new ApiResult(false, generalErrors, fieldErrors);
    }
}
