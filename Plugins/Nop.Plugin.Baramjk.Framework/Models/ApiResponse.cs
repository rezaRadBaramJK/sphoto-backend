using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Nop.Plugin.Baramjk.Framework.Models
{
    public class ApiResponse : IApiResponse
    {
        private bool _isSuccess = true;

        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

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

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var actionResult = new ObjectResult(this) { StatusCode = StatusCode };
            await actionResult.ExecuteResultAsync(context);
        }

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

        public static implicit operator ActionResult(ApiResponse data)
        {
            return new ObjectResult(data);
        }

        public static implicit operator ObjectResult(ApiResponse data)
        {
            return new ObjectResult(data);
        }
    }

    public class ApiResponse<T> : ApiResponse, IApiResponse<T>
    {
        public T Data { get; set; }

        public static implicit operator ActionResult(ApiResponse<T> data)
        {
            return new ObjectResult(data);
        }

        public static implicit operator ObjectResult(ApiResponse<T> data)
        {
            return new ObjectResult(data);
        }
    }
}