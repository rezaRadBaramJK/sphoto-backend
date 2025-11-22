namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Services
{
    public class BaseServiceResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public BaseServiceResult()
        {
            IsSuccess = true;
        }

        public BaseServiceResult(string message)
        {
            Message = message;
        }
    }

    public class BaseServiceResult<T> : BaseServiceResult
    {
        public T Data { get; set; }

        public BaseServiceResult(string message) : base(message)
        {
        }

        public BaseServiceResult(T data)
        {
            Data = data;
        }
    }
}