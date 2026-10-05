using Microsoft.IdentityModel.Tokens;

namespace GridLink.Helpers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse <T> SuccessResult(T data, string message = "Completeed Successfully")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> FailureResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }

    public class ApiResponse: ApiResponse<object>
    {
        public static ApiResponse SuccessResult(string message = "Completeed Successfully")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
               
            };
        }

        public static new ApiResponse FailureResult(string message, List<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }

    }
}
