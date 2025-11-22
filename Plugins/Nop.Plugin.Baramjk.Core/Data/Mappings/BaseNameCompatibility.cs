using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Baramjk.Framework.Domain.Vendors;

namespace Nop.Plugin.Baramjk.Core.Data.Mappings
{
    public class BaseNameCompatibility: INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            {typeof(VendorDetail), $"Framework_{nameof(VendorDetail)}"},
        };

        public Dictionary<(Type, string), string> ColumnName => new();
    }
}