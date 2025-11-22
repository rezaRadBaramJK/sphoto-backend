using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Baramjk.Core.Plugins
{
    public class PermissionProvider : IPermissionProvider
    {
        public const string CoreManagement = "CoreManagement";
        public static readonly PermissionRecord Management = new()
            { Name = "Core management", SystemName = CoreManagement, Category = "Standard" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                Management,
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
                        Management,
                    }
                )
            };
        }
    }
}