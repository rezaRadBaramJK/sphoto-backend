using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Nop.Plugin.Baramjk.BackendApi
{
    public class DelayMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Random _random = new Random();

        public DelayMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            int delay = _random.Next(90000, 100000); // 5–10 seconds
            await Task.Delay(delay);

            await _next(context);
        }
    }
}