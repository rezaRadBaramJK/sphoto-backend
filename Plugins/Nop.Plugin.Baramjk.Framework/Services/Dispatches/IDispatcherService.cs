using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.Framework.Services.Dispatches
{
    public interface IDispatcherService
    {
        void AddConsumer(string topic, Func<object, Task> action);
        void AddHandler(string topic, Func<object, Task<object>> func);
        Task PublishAsync<T>(string topic, T data);
        Task PublishAndForgetAsync<T>(string topic, T data);
        IAsyncEnumerable<object> HandlesAsync(string topic, object data);
        IAsyncEnumerable<TResult> HandlesAsync<TResult>(string topic, object data);
        Task<TResult> HandleAsync<TResult>(string topic, object data);
    }
}