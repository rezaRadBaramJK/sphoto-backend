using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Baramjk.Wallet.Plugins
{
    public class PermissionProvider : IPermissionProvider
    {
        public const string Wallet_MANAGEMEN = "WalletManagement";
        public static readonly PermissionRecord WalletManagementRecord = new()
            { Name = "Wallet management", SystemName = Wallet_MANAGEMEN, Category = "Standard" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                WalletManagementRecord,
            };
        }

        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        WalletManagementRecord,
                    }
                )
            };
        }
    }
}