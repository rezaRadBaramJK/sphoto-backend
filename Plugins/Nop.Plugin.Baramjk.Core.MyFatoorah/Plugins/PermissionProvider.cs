using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins
{
    public class PermissionProvider : IPermissionProvider
    {
        public const string MyFatoorahManagement = "MyFatoorahManagement";
        public static readonly PermissionRecord ManagementRecord = new()
            { Name = "MyFatoorah management", SystemName = MyFatoorahManagement, Category = "Standard" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManagementRecord,
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
                        ManagementRecord,
                    }
                )
            };
        }
    }
}