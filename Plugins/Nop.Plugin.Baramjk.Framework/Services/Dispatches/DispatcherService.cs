using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Utils;

namespace Nop.Plugin.Baramjk.Framework.Services.Dispatches
{
    public class DispatcherService : IDispatcherService
    {
        private static ConcurrentDictionary<string, ConcurrentBag<Func<object, Task>>> _consumers = new();
        private static ConcurrentDictionary<string, ConcurrentBag<Func<object, Task<object>>>> _handlers = new();

        public void AddConsumer(string topic, Func<object, Task> action)
        {
            if (_consumers.ContainsKey(topic) == false)
            {
                _consumers.TryAdd(topic, new ConcurrentBag<Func<object, Task>>() { action });
                return;
            }

            _consumers[topic].Add(action);
        }

        public void AddHandler(string topic, Func<object, Task<object>> func)
        {
            if (_handlers.ContainsKey(topic) == false)
            {
                _handlers.TryAdd(topic, new ConcurrentBag<Func<object, Task<object>>>() { func });
                return;
            }

            _handlers[topic].Add(func);
        }

        public async Task PublishAsync<T>(string topic, T data)
        {
            await Task.Run(async () =>
            {
                if (_consumers.ContainsKey(topic) == false)
                {
                    _consumers.TryAdd(topic, new ConcurrentBag<Func<object, Task>>());
                    return;
                }

                foreach (var action in _consumers[topic])
                    await action(data);
            });
        }

        public async Task PublishAndForgetAsync<T>(string topic, T data)
        {
            await Task.Run(() =>
            {
                if (_consumers.ContainsKey(topic) == false)
                {
                    _consumers.TryAdd(topic, new ConcurrentBag<Func<object, Task>>());
                    return;
                }

                foreach (var action in _consumers[topic])
                    action(data);
            });
        }

        public async IAsyncEnumerable<object> HandlesAsync(string topic, object data)
        {
            if (_handlers.ContainsKey(topic) == false)
            {
                _handlers.TryAdd(topic, new ConcurrentBag<Func<object, Task<object>>>());
                yield break;
            }

            foreach (var func in _handlers[topic])
            {
                yield return await func(data);
            }
        }

        public async IAsyncEnumerable<TResult> HandlesAsync<TResult>(string topic, object data)
        {
            if (_handlers.ContainsKey(topic) == false)
            {
                _handlers.TryAdd(topic, new ConcurrentBag<Func<object, Task<object>>>());
                yield break;
            }

            foreach (var func in _handlers[topic])
            {
                var funcResult = await func(data);
                if (funcResult == null)
                    yield return default;
                else if (funcResult is TResult result)
                    yield return result;
                else
                    yield return MapUtils.Map<TResult>(funcResult);
            }
        }

        public async Task<TResult> HandleAsync<TResult>(string topic, object data)
        {
            if (_handlers.ContainsKey(topic) == false)
            {
                _handlers.TryAdd(topic, new ConcurrentBag<Func<object, Task<object>>>());
                return default;
            }

            var func = _handlers[topic].FirstOrDefault();
            if (func == null)
                return default;
            var funcResult = await func(data);
            if (funcResult == null)
                return default;
            if (funcResult is TResult result)
                return result;
            return MapUtils.Map<TResult>(funcResult);
        }
    }
}