using System;
using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways
{
    public interface IGatewayClientProvider
    {
        IEnumerable<Type> GetGatewayClientTypes();
        IGatewayClient GetGatewayClient(Type type);
        IGatewayClient GetDefaultGatewayClient();
        IGatewayClient GetGatewayClient(string name);
    }
}