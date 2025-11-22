using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Models
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public interface IApiResponse : IActionResult
    {
        List<string> Errors { get; }
        bool IsSuccess { get; set; }
        string Message { get; set; }
        int StatusCode { get; set; }
    }

    public interface IApiResponse<out T> : IApiResponse
    {
        T Data { get; }
    }
}