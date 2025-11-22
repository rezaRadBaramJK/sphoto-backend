using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Plugins
{
    public class PermissionProvider : IPermissionProvider
    {
        public const string ManagementName = "PhotoPlatformManagement";
        public const string ActorName = "PhotoPlatformActor";
        public const string CashierName = "PhotoPlatformCashier";
        public const string ScannerBoyName = "PhotoPlatformScannerBoy";
        public const string SupervisorName = "PhotoPlatformSupervisor";
        public const string ProductionName = "PhotoPlatformProduction";

        public static readonly PermissionRecord ManagementRecord = new()
        {
            Name = "Plugin Photo Platform Management",
            SystemName = ManagementName,
            Category = "Standard"
        };

        private static readonly PermissionRecord ActorRecord = new()
        {
            Name = "Plugin Photo Platform Actor",
            SystemName = ActorName,
            Category = "Standard"
        };

        private static readonly PermissionRecord CashierRecord = new()
        {
            Name = "Plugin Photo Platform Cashier",
            SystemName = CashierName,
            Category = "Standard"
        };

        private static readonly PermissionRecord ScannerBoyRecord = new()
        {
            Name = "Plugin Photo Platform Scanner Boy",
            SystemName = ScannerBoyName,
            Category = "Standard"
        };

        private static readonly PermissionRecord SupervisorRecord = new()
        {
            Name = "Plugin Photo Platform Supervisor",
            SystemName = SupervisorName,
            Category = "Standard"
        };

        private static readonly PermissionRecord ProductionRecord = new()
        {
            Name = "Plugin Photo Platform Production",
            SystemName = ProductionName,
            Category = "Standard"
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManagementRecord,
                ActorRecord,
                CashierRecord,
                ScannerBoyRecord,
                SupervisorRecord,
                ProductionRecord
            };
        }

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManagementRecord,
                        ActorRecord,
                        CashierRecord,
                        ScannerBoyRecord,
                        SupervisorRecord,
                        ProductionRecord
                    }
                ),
                (
                    DefaultValues.ActorRoleName,
                    new[]
                    {
                        ActorRecord
                    }
                ),
                (
                    DefaultValues.CashierRoleName,
                    new[]
                    {
                        CashierRecord
                    }
                ),
                (
                    DefaultValues.ScannerBoyRoleName,
                    new[]
                    {
                        ScannerBoyRecord
                    }
                ),
                (
                    DefaultValues.SupervisorRoleName,
                    new[]
                    {
                        SupervisorRecord
                    }
                ),
                (
                    DefaultValues.ProductionRoleName,
                    new[]
                    {
                        ProductionRecord
                    }
                )
            };
        }
    }
}