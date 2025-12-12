namespace FinanceTracker.BlazorUI.Models.Commons;

public class ApiResult
{
    public bool IsSuccess { get; }
    public string[] GeneralErrors { get; }

    public ApiResult(bool isSuccess, IEnumerable<string>? generalErrors = null)
    {
        IsSuccess = isSuccess;
        GeneralErrors = generalErrors?.ToArray() ?? [];
    }

    public static ApiResult Success()
    {
        return new ApiResult(true);
    }

    public static ApiResult Failure(IEnumerable<string>? generalErrors = null)
    {
        return new ApiResult(false, generalErrors);
    }
}
