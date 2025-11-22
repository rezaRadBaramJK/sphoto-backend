using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains.PaymentFeeRule;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            { typeof(PaymentFeeRule), $"{DefaultValue.TABLENAME_PREFIX}{nameof(PaymentFeeRule)}" },
            { typeof(Supplier), $"{DefaultValue.TABLENAME_PREFIX}{nameof(Supplier)}" },
            { typeof(ProductSupplierMapping), $"{DefaultValue.TABLENAME_PREFIX}{nameof(ProductSupplierMapping)}" },
        };

        public Dictionary<(Type, string), string> ColumnName => new();
    }
}