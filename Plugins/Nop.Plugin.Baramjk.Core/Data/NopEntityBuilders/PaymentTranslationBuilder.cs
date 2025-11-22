using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Core.Data.NopEntityBuilders
{
    public class GatewayPaymentTranslationBuilder : NopEntityBuilder<GatewayPaymentTranslation>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(GatewayPaymentTranslation.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(GatewayPaymentTranslation.Guid)).AsString(40)
                .WithColumn(nameof(GatewayPaymentTranslation.GatewayName)).AsString(100).Nullable()
                .WithColumn(nameof(GatewayPaymentTranslation.MethodName)).AsString(100).Nullable()
                .WithColumn(nameof(GatewayPaymentTranslation.PaymentUrl)).AsString(500).Nullable()
                .WithColumn(nameof(GatewayPaymentTranslation.PaymentId)).AsString().Nullable()
                .WithColumn(nameof(GatewayPaymentTranslation.PaymentOptionId)).AsInt32().Nullable().WithDefaultValue(string.Empty)
                .WithColumn(nameof(GatewayPaymentTranslation.InvoiceId)).AsString(100).Nullable()
                .WithColumn(nameof(GatewayPaymentTranslation.AmountToPay)).AsDecimal()
                .WithColumn(nameof(GatewayPaymentTranslation.AmountPayed)).AsDecimal().WithDefaultValue(0)
                .WithColumn(nameof(GatewayPaymentTranslation.Message)).AsString().Nullable()
                .WithColumn(nameof(GatewayPaymentTranslation.OwnerCustomerId)).AsInt32()
                .WithColumn(nameof(GatewayPaymentTranslation.ConsumerName)).AsString(100)
                .WithColumn(nameof(GatewayPaymentTranslation.ConsumerEntityType)).AsString(100)
                .WithColumn(nameof(GatewayPaymentTranslation.ConsumerEntityId)).AsInt32().WithDefaultValue(0)
                .WithColumn(nameof(GatewayPaymentTranslation.ConsumerData)).AsString()
                .WithColumn(nameof(GatewayPaymentTranslation.ConsumerCallBackUrl)).AsString(500)
                .WithColumn(nameof(GatewayPaymentTranslation.PaymentFeeRuleId)).AsInt32().Nullable().WithDefaultValue(0)
                .WithColumn(nameof(GatewayPaymentTranslation.PaymentFeeValue)).AsDecimal().WithDefaultValue(0)
                .WithColumn(nameof(GatewayPaymentTranslation.ConsumerStatus)).AsInt32().WithDefaultValue((ConsumerStatus.Pending))
                .WithColumn(nameof(GatewayPaymentTranslation.Status)).AsInt32().WithDefaultValue(GatewayPaymentStatus.Pending)
                .WithColumn(nameof(GatewayPaymentTranslation.OnCreateDateTimeUtc)).AsDateTime();
        }
    }
}