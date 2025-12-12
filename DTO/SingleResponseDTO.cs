namespace DTO
{
    public class SingleResponseDTO<T>
    {
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; } = true;

        public static SingleResponseDTO<T> SuccessResponse(T data, string message = "")
            => new() { Data = data, Message = message, Success = true };

        public static SingleResponseDTO<T> NotFoundResponse(string message)
            => new() { Data = default, Message = message, Success = false };

        public static SingleResponseDTO<T> ErrorResponse(string message)
            => new() { Data = default, Message = message, Success = false };
    }
}