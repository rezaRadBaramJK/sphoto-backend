using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Nop.Plugin.Baramjk.Framework.Services.Networks;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models
{
    public class MyFatoorahResponse<T>:IGenericResponseModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public List<FieldsError> FieldsErrors { get; set; }
        public List<FieldsError> ValidationErrors { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public T Data { get; set; }

        public string GetFullMessage(string defaultValue)
        {
            var msg = Message ?? "";

            if (FieldsErrors != null && FieldsErrors.Count > 0)
            {
                msg += Environment.NewLine;
                var errors = FieldsErrors.Select(item => $"{item.Name}: {item.Error}");
                msg += string.Join(Environment.NewLine, errors);
            }

            if (ValidationErrors != null && ValidationErrors.Count > 0)
            {
                msg += Environment.NewLine;
                var errors = ValidationErrors.Select(item => $"{item.Name}: {item.Error}");
                msg += string.Join(Environment.NewLine, errors);
            }

            if (string.IsNullOrEmpty(msg))
                msg = defaultValue;

            return msg;
        }

        public bool HasData => Data != null;
    }

    public class FieldsError
    {
        public string Name { get; set; }
        public string Error { get; set; }
    }
}