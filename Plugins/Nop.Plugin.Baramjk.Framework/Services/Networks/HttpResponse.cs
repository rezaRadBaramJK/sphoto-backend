using System.Net;
using Nop.Plugin.Baramjk.Framework.Extensions;

namespace Nop.Plugin.Baramjk.Framework.Services.Networks
{
    public class HttpResponse
    {
        public static HttpResponse NoContent = new()
        {
            StatusCode = HttpStatusCode.NoContent,
        };

        private string _reasonPhrase;

        public HttpStatusCode StatusCode { get; set; }

        public string ReasonPhrase
        {
            get => _reasonPhrase.ValueOrDefault(StatusCode.ToString());
            set => _reasonPhrase = value;
        }

        public bool IsSuccessStatusCode => ((int)StatusCode >= 200) && ((int)StatusCode <= 299);
    }

    public class HttpResponse<TResponse> : HttpResponse
    {
        public new static HttpResponse<TResponse> NoContent = new()
        {
            Body = default,
            StatusCode = HttpStatusCode.NoContent,
        };

        public TResponse Body { get; set; }
    }
}