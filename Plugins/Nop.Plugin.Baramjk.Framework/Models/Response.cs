using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Framework.Models
{
    public class Response : IResponse
    {
        private bool _isSuccess = true;
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public bool IsSuccess
        {
            get => _isSuccess && (Errors is null || Errors.Count == 0);
            set
            {
                if (value && Errors != null)
                    Errors.Clear();

                _isSuccess = value;
            }
        }
        public List<string> Errors { get; private set; } = new();
        public void AddError(string errorMessage)
        {
            if (Errors == null)
                Errors = new List<string>();

            Errors.Add(errorMessage);
            IsSuccess = false;
        }
        public void AddError(IEnumerable<string> errorMessage)
        {
            if (Errors == null)
                Errors = new List<string>();

            Errors.AddRange(errorMessage);
            IsSuccess = false;
        }
    }

    public class Response<T> : Response, IResponse<T>
    {
        public T Data { get; set; }
    }
}