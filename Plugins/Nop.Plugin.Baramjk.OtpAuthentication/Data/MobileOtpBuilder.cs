using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.OtpAuthentication.Domain;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Data
{
    public class MobileOtpBuilder: NopEntityBuilder<MobileOtp>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(MobileOtp.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(MobileOtp.PhoneNumber)).AsString().NotNullable()
                .WithColumn(nameof(MobileOtp.OldPhoneNumber)).AsString().Nullable()
                .WithColumn(nameof(MobileOtp.Otp)).AsString().NotNullable()
                .WithColumn(nameof(MobileOtp.OtpType)).AsInt32().NotNullable()
                .WithColumn(nameof(MobileOtp.CreateDateTime)).AsDateTime().NotNullable()
                .WithColumn(nameof(MobileOtp.AttemptNumber)).AsInt32().NotNullable()
                ;
        }
    }
}