using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Framework.Models
{
    public interface IResponse 
    {
        List<string> Errors { get; }
        bool IsSuccess { get; set; }
        string Message { get; set; }
        int StatusCode { get; set; }
    }

    public interface IResponse<out T> 
    {
        T Data { get; }
    }
}