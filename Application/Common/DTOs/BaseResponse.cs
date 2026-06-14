namespace Application.Common.Dtos
{
    public class BaseResponse<T>
    {
        public string Message { get; set; } = default!;
        public T? Data { get; set; }
        public bool IsSuccessful { get; set; }

        public static BaseResponse<T> Success(string message, T data)
        {
            return new BaseResponse<T>
            {
                Message = message,
                Data = data,
                IsSuccessful = true
            };
        }

        public static BaseResponse<T> Failure(string message)
        {
            return new BaseResponse<T>
            {
                Message = message
            };
        }
    }

}
