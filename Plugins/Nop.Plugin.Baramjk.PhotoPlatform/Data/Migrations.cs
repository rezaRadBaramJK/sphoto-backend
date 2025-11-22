using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data
{
    [NopMigration("2025/05/01 10:00:00:1687541", "PhotoPlatform - Base Migration")]
    public class BaseMigration : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public BaseMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<TimeSlot>(_migrationManager);
            this.BuildTableIfNotExists<EventDetail>(_migrationManager);
            this.BuildTableIfNotExists<Actor>(_migrationManager);
            this.BuildTableIfNotExists<ActorEvent>(_migrationManager);
            this.BuildTableIfNotExists<ShoppingCartItemTimeSlot>(_migrationManager);
            this.BuildTableIfNotExists<ReservationItem>(_migrationManager);
            this.BuildTableIfNotExists<ReservationHistory>(_migrationManager);
            this.BuildTableIfNotExists<CashierEvent>(_migrationManager);
            this.BuildTableIfNotExists<ActorPicture>(_migrationManager);
            this.BuildTableIfNotExists<CashierBalanceHistory>(_migrationManager);
            this.BuildTableIfNotExists<ContactInfoEntity>(_migrationManager);
            this.BuildTableIfNotExists<SubjectEntity>(_migrationManager);
            this.BuildTableIfNotExists<EventCountryMapping>(_migrationManager);
            this.BuildTableIfNotExists<SupervisorEvent>(_migrationManager);
            this.BuildTableIfNotExists<ProductionEvent>(_migrationManager);
            this.BuildTableIfNotExists<CashierDailyBalance>(_migrationManager);
            this.BuildTableIfNotExists<ActorEventTimeSlot>(_migrationManager);
        }
    }

    [NopMigration("2025/05/14 18:18:00:1687541",
        "PhotoPlatform - Add UsedCameraManPhotoCount & UsedCustomerMobilePhotoCount Columns to the ReservationItem Table")]
    public class AddReservationItemUsedCountFields : AutoReversingMigration
    {
        public override void Up()
        {
            var baseNameCompatibility = new BaseNameCompatibility();

            if (baseNameCompatibility.TableNames.TryGetValue(typeof(ReservationItem), out var tableName) == false)
                tableName = $"{DefaultValues.TableNamePrefix}{nameof(ReservationItem)}";

            if (Schema.Table(tableName).Column(nameof(ReservationItem.UsedCameraManPhotoCount)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(ReservationItem.UsedCameraManPhotoCount)).AsInt32().WithDefaultValue(0);

            if (Schema.Table(tableName).Column(nameof(ReservationItem.UsedCustomerMobilePhotoCount)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(ReservationItem.UsedCustomerMobilePhotoCount)).AsInt32().WithDefaultValue(0);
        }
    }

    [NopMigration("2025/05/14 20:18:00:1687541",
        "PhotoPlatform - add Active column to cashierEvent table ")]
    public class AddCashierEventActiveColumn : AutoReversingMigration
    {
        public override void Up()
        {
            var baseNameCompatibility = new BaseNameCompatibility();

            if (baseNameCompatibility.TableNames.TryGetValue(typeof(CashierEvent), out var tableName) == false)
                tableName = $"{DefaultValues.TableNamePrefix}{nameof(CashierEvent)}";

            if (Schema.Table(tableName).Column(nameof(CashierEvent.Active)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(CashierEvent.Active)).AsBoolean().WithDefaultValue(true);
        }
    }

    [NopMigration("2025/05/18 18:00:00:0000000",
        "PhotoPlatform - add IsRefundPermitted column to cashierEvent table ")]
    public class AddCashierEventIsRefundPermittedColumn : AutoReversingMigration
    {
        public override void Up()
        {
            var baseNameCompatibility = new BaseNameCompatibility();

            if (baseNameCompatibility.TableNames.TryGetValue(typeof(CashierEvent), out var tableName) == false)
                tableName = $"{DefaultValues.TableNamePrefix}{nameof(CashierEvent)}";

            if (Schema.Table(tableName).Column(nameof(CashierEvent.IsRefundPermitted)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(CashierEvent.IsRefundPermitted)).AsBoolean().WithDefaultValue(true);
        }
    }


    [NopMigration("2025/05/21 15:00:00:0000000",
        "PhotoPlatform - add ActorPicture table")]
    public class AddActorPictureTable : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public AddActorPictureTable(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<ActorPicture>(_migrationManager);
        }
    }


    [NopMigration("2025/05/22 15:00:00:0000000",
        "PhotoPlatform - add PhotoShootShare share, Actor share and ProductionShare columns to EventDetail table")]
    public class AddSharesColumnsToEventDetail : AutoReversingMigration
    {
        public override void Up()
        {
            var baseNameCompatibility = new BaseNameCompatibility();

            if (baseNameCompatibility.TableNames.TryGetValue(typeof(EventDetail), out var tableName) == false)
                tableName = $"{DefaultValues.TableNamePrefix}{nameof(EventDetail)}";

            if (Schema.Table(tableName).Column(nameof(EventDetail.ProductionShare)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(EventDetail.ProductionShare)).AsDecimal().WithDefaultValue(0);

            if (Schema.Table(tableName).Column(nameof(EventDetail.ActorShare)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(EventDetail.ActorShare)).AsDecimal().WithDefaultValue(0);

            if (Schema.Table(tableName).Column(nameof(EventDetail.PhotoShootShare)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(EventDetail.PhotoShootShare)).AsDecimal().WithDefaultValue(0);

            if (Schema.Table(tableName).Column(nameof(EventDetail.PhotoPrice)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(EventDetail.PhotoPrice)).AsDecimal().WithDefaultValue(0);
        }
    }

    [NopMigration("2025/05/24 18:00:00:0000000",
        "PhotoPlatform - add cashierWalletHistory table")]
    public class AddCashierWalletHistoryTable : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public AddCashierWalletHistoryTable(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<CashierBalanceHistory>(_migrationManager);
        }
    }

    [NopMigration("2025/06/03 15:30:00:0000000",
        "PhotoPlatform - alter TermsAndConditions column to NVARCHAR(MAX) in EventDetail table")]
    public class AlterTermsAndConditionsColumn : AutoReversingMigration
    {
        public override void Up()
        {
            var baseNameCompatibility = new BaseNameCompatibility();

            if (!baseNameCompatibility.TableNames.TryGetValue(typeof(EventDetail), out var tableName))
                tableName = $"{DefaultValues.TableNamePrefix}{nameof(EventDetail)}";

            if (Schema.Table(tableName).Column(nameof(EventDetail.TermsAndConditions)).Exists())
            {
                Alter.Column(nameof(EventDetail.TermsAndConditions))
                    .OnTable(tableName)
                    .AsCustom("NVARCHAR(MAX)")
                    .Nullable();
            }
        }
    }


    [NopMigration("2025/06/08 15:30:00:0000000",
        "PhotoPlatform - Add ContactUs Tables")]
    public class AddContactUsTables : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public AddContactUsTables(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<ContactInfoEntity>(_migrationManager);
            this.BuildTableIfNotExists<SubjectEntity>(_migrationManager);
        }
    }


    [NopMigration("2025/06/10 11:30:00:0000000",
        "PhotoPlatform - Add DisplayOrder to ActorEvents")]
    public class AddDisplayOrderColumnToActorEventsTable : AutoReversingMigration
    {
        public override void Up()
        {
            var baseNameCompatibility = new BaseNameCompatibility();

            if (baseNameCompatibility.TableNames.TryGetValue(typeof(ActorEvent), out var tableName) == false)
                tableName = $"{DefaultValues.TableNamePrefix}{nameof(ActorEvent)}";

            if (Schema.Table(tableName).Column(nameof(ActorEvent.DisplayOrder)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(ActorEvent.DisplayOrder)).AsInt32().WithDefaultValue(0);
        }
    }


    [NopMigration("2025/06/30 14:30:00:0000000",
        "PhotoPlatform - Add CardNumber & CardHolderName to Actor ")]
    public class AddCardNumberCardHolderNameColumnsToActorsTable : AutoReversingMigration
    {
        public override void Up()
        {
            var baseNameCompatibility = new BaseNameCompatibility();

            if (baseNameCompatibility.TableNames.TryGetValue(typeof(Actor), out var tableName) == false)
                tableName = $"{DefaultValues.TableNamePrefix}{nameof(Actor)}";

            if (Schema.Table(tableName).Column(nameof(Actor.CardNumber)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(Actor.CardNumber)).AsString().Nullable();

            if (Schema.Table(tableName).Column(nameof(Actor.CardHolderName)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(Actor.CardHolderName)).AsString().Nullable();
        }
    }

    [NopMigration("2025/06/30 18:30:00:0000000",
        "PhotoPlatform - Add actor specific share and specific production share to actorEvents")]
    public class AddActorShareAndProductionShareToActorEventsTable : AutoReversingMigration
    {
        public override void Up()
        {
            var baseNameCompatibility = new BaseNameCompatibility();

            if (baseNameCompatibility.TableNames.TryGetValue(typeof(ActorEvent), out var tableName) == false)
                tableName = $"{DefaultValues.TableNamePrefix}{nameof(ActorEvent)}";

            if (Schema.Table(tableName).Column(nameof(ActorEvent.ActorShare)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(ActorEvent.ActorShare)).AsString().Nullable();

            if (Schema.Table(tableName).Column(nameof(ActorEvent.ProductionShare)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(ActorEvent.ProductionShare)).AsString().Nullable();
        }
    }


    [NopMigration("2025/07/24 09:18:00:1687541",
        "PhotoPlatform - add Note column to TimeSlot table ")]
    public class AddNoteColumnToTimeSlotTable : AutoReversingMigration
    {
        public override void Up()
        {
            var baseNameCompatibility = new BaseNameCompatibility();

            if (baseNameCompatibility.TableNames.TryGetValue(typeof(TimeSlot), out var tableName) == false)
                tableName = $"{DefaultValues.TableNamePrefix}{nameof(TimeSlot)}";

            if (Schema.Table(tableName).Column(nameof(TimeSlot.Note)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(TimeSlot.Note)).AsString(int.MaxValue).Nullable();
        }
    }

    [NopMigration("2025/07/27 18:08:00:1687541",
        "PhotoPlatform - add EventCountryMapping table ")]
    public class AddEventCountryMappingTable : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public AddEventCountryMappingTable(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<EventCountryMapping>(_migrationManager);
        }
    }

    [NopMigration("2025/09/02 10:30:00:0000000",
        "PhotoPlatform - Add SupervisorEvent and ProductionEvent tables")]
    public class AddSupervisorEventAndProductionEventTables : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public AddSupervisorEventAndProductionEventTables(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<SupervisorEvent>(_migrationManager);
            this.BuildTableIfNotExists<ProductionEvent>(_migrationManager);
        }
    }

    [NopMigration("2025/09/11 14:00:00:0000000",
        "PhotoPlatform - add Note & locationUrl columns to EventDetail table")]
    public class AddNoteAndLocationUrlColumnsToEventDetail : AutoReversingMigration
    {
        public override void Up()
        {
            var baseNameCompatibility = new BaseNameCompatibility();

            if (baseNameCompatibility.TableNames.TryGetValue(typeof(EventDetail), out var tableName) == false)
                tableName = $"{DefaultValues.TableNamePrefix}{nameof(EventDetail)}";

            if (Schema.Table(tableName).Column(nameof(EventDetail.Note)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(EventDetail.Note)).AsString().Nullable();

            if (Schema.Table(tableName).Column(nameof(EventDetail.LocationUrlTitle)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(EventDetail.LocationUrlTitle)).AsString().Nullable();

            if (Schema.Table(tableName).Column(nameof(EventDetail.LocationUrl)).Exists() == false)
                Alter.Table(tableName).AddColumn(nameof(EventDetail.LocationUrl)).AsString().Nullable();
        }
    }

    [NopMigration("2025/09/29 10:00:00:1687541", "PhotoPlatform - Add CashierDailyBalance Table")]
    public class AddCashierDailyBalanceTable : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public AddCashierDailyBalanceTable(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<CashierDailyBalance>(_migrationManager);
        }
    }

    [NopMigration("2025/09/30 08:00:00:1687541", "PhotoPlatform - Add AddActorEventTimeSlot Table")]
    public class AddActorEventTimeSlotTable : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public AddActorEventTimeSlotTable(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<ActorEventTimeSlot>(_migrationManager);
        }
    }
}