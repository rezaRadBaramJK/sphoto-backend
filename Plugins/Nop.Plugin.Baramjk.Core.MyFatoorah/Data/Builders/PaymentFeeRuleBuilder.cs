using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains.PaymentFeeRule;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Data.Builders
{
    public class PaymentFeeRuleBuilder : NopEntityBuilder<PaymentFeeRule>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(PaymentFeeRule.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(PaymentFeeRule.PaymentMethodId)).AsInt32().NotNullable()
                .WithColumn(nameof(PaymentFeeRule.CountryId)).AsInt32().NotNullable()
                .WithColumn(nameof(PaymentFeeRule.AdditionalFee)).AsDecimal().NotNullable()
                .WithColumn(nameof(PaymentFeeRule.AdditionalFeePercentage)).AsBoolean().NotNullable()
                .WithColumn(nameof(PaymentFeeRule.Active)).AsBoolean().NotNullable()
                .WithColumn(nameof(PaymentFeeRule.Deleted)).AsBoolean().NotNullable();
        }
    }
}