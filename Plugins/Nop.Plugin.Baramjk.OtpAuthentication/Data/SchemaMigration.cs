using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.OtpAuthentication.Domain;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Data
{
    [SkipMigrationOnUpdate]
    [NopMigration("2023/04/05 13:01:02:1687545", "Otp authentication ")]
    public class Migration : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public Migration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            _migrationManager.BuildTable<MobileOtp>(Create);
        }
    }
    
    [NopMigration("2023/11/05 13:01:02:1687545", "Otp authentication - Add Otp Type")]
    public class AddOtpTypeMigration : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public AddOtpTypeMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            var table = Schema.Table(nameof(MobileOtp));
            if (table.Exists() == false)
                this.BuildTableIfNotExists<MobileOtp>(_migrationManager);

            if (table.Column(nameof(MobileOtp.OtpType)).Exists() == false)
                Alter.Table(nameof(MobileOtp))
                    .AddColumn(nameof(MobileOtp.OtpType))
                    .AsInt32()
                    .NotNullable()
                    .WithDefaultValue(0);
        }
    }
    
}