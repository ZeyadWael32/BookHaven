namespace BookHaven.API.Services;

public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public ServiceErrorType ErrorType { get; set; }
    public string? ErrorTitle { get; set; }
    public string? ErrorDetail { get; set; }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            ErrorType = ServiceErrorType.None
        };
    }

    public static ServiceResult<T> Failure(ServiceErrorType type, string title, string detail)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Data = default,
            ErrorType = type,
            ErrorTitle = title,
            ErrorDetail = detail
        };
    }
}
