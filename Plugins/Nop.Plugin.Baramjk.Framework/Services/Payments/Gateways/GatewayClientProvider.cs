using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Exceptions;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways
{
    public class GatewayClientProvider : IGatewayClientProvider
    {
        private static Type _defaultGatewayClientType = null;
        private static Dictionary<string, Type> _gatewayClients = new();

        private readonly ITypeFinder _typeFinder;
        private readonly FrameworkSettings _frameworkSettings;

        public GatewayClientProvider(ITypeFinder typeFinder, FrameworkSettings frameworkSettings)
        {
            _typeFinder = typeFinder;
            _frameworkSettings = frameworkSettings;
        }

        public IEnumerable<Type> GetGatewayClientTypes()
        {
            return _typeFinder.FindClassesOfType<IGatewayClient>();
        }

        public IGatewayClient GetGatewayClient(Type type)
        {
            NullBusinessException.Check(type);
            return (IGatewayClient)EngineContext.Current.Resolve(type);
        }

        public IGatewayClient GetGatewayClient(string name)
        {
            if (_gatewayClients.ContainsKey(name))
                return GetGatewayClient(_gatewayClients[name]);

            var type = GetGatewayClientTypes().FirstOrDefault(c => c.Name == name);
            if (type == null)
                NotFoundBusinessException.NotFound("GatewayClient not found");
            _gatewayClients[name] = type;

            return GetGatewayClient(type);
        }

        public IGatewayClient GetDefaultGatewayClient()
        {
            if (_defaultGatewayClientType != null)
                return GetGatewayClient(_defaultGatewayClientType);

            foreach (var type in GetGatewayClientTypes())
            {
                if (type.Name == _frameworkSettings.DefaultGatewayClientName)
                {
                    _defaultGatewayClientType = type;
                    break;
                }
            }

            return GetGatewayClient(_defaultGatewayClientType);
        }
    }
}