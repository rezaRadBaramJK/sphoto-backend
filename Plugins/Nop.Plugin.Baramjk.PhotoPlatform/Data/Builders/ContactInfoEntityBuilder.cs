using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class ContactInfoEntityBuilder : NopEntityBuilder<ContactInfoEntity>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ContactInfoEntity.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(ContactInfoEntity.FirstName)).AsString()
                .WithColumn(nameof(ContactInfoEntity.LastName)).AsString()
                .WithColumn(nameof(ContactInfoEntity.Email)).AsString()
                .WithColumn(nameof(ContactInfoEntity.PhoneNumber)).AsString()
                .WithColumn(nameof(ContactInfoEntity.CountryId)).AsInt32()
                .WithColumn(nameof(ContactInfoEntity.SubjectId)).AsInt32()
                .WithColumn(nameof(ContactInfoEntity.TicketId)).AsInt32()
                .WithColumn(nameof(ContactInfoEntity.FileId)).AsInt32()
                .WithColumn(nameof(ContactInfoEntity.PaymentUtcDateTime)).AsDateTime().Nullable()
                .WithColumn(nameof(ContactInfoEntity.Message)).AsCustom("NVARCHAR(MAX)")
                .WithColumn(nameof(ContactInfoEntity.HasBeenPaid)).AsBoolean().WithDefaultValue(false);
        }
    }
}