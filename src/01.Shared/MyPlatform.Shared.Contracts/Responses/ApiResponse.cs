namespace MyPlatform.Shared.Contracts.Responses;

/// <summary>
/// Standard API response wrapper.
/// </summary>
/// <typeparam name="T">The type of data in the response.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the request was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response data.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets the response message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the error details if the request failed.
    /// </summary>
    public ErrorDetails? Error { get; set; }

    /// <summary>
    /// Gets or sets the request trace identifier.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <param name="message">Optional success message.</param>
    /// <returns>A successful API response.</returns>
    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Success"
        };
    }

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="error">The error details.</param>
    /// <param name="message">Optional error message.</param>
    /// <returns>A failed API response.</returns>
    public static ApiResponse<T> Fail(ErrorDetails error, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error,
            Message = message ?? error.Message
        };
    }

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <returns>A failed API response.</returns>
    public static ApiResponse<T> Fail(string message, string? code = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = new ErrorDetails
            {
                Code = code ?? "ERROR",
                Message = message
            }
        };
    }
}

/// <summary>
/// Non-generic API response for operations without data.
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Creates a successful response without data.
    /// </summary>
    /// <param name="message">Optional success message.</param>
    /// <returns>A successful API response.</returns>
    public static ApiResponse Ok(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message ?? "Success"
        };
    }
}

/// <summary>
/// Error details for API responses.
/// </summary>
public class ErrorDetails
{
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation errors.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    /// <summary>
    /// Gets or sets additional details about the error.
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }
}
