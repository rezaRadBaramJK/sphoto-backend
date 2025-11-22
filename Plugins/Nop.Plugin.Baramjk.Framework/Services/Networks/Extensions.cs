using Nop.Plugin.Baramjk.Framework.Extensions;

namespace Nop.Plugin.Baramjk.Framework.Services.Networks
{
    public static class Extensions
    {
        public static string GetMessage<T>(this HttpResponse<T> httpResponse, string msg)
            where T : IResponseModel
        {
            if (httpResponse == null)
                return msg;

            if (httpResponse.Body == null)
                return msg.Plus(httpResponse.ReasonPhrase);

            if (httpResponse.Body.IsSuccess == false)
                return httpResponse.Body.Message.ValueOrDefault(msg.Plus(httpResponse.ReasonPhrase));

            return httpResponse.Body.Message;
        }

        public static bool IsSuccess<T>(this HttpResponse<T> httpResponse)
            where T : IResponseModel
        {
            return httpResponse?.Body?.IsSuccess == true;
        }

        public static bool IsSuccess<T>(this HttpResponse<T> httpResponse, bool checkHasData)
            where T : IGenericResponseModel
        {
            return httpResponse?.Body?.IsSuccess == true && httpResponse?.Body?.HasData == true;
        }
    }
}