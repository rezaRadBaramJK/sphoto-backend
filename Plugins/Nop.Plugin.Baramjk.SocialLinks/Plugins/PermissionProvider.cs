using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Baramjk.SocialLinks.Plugins
{
    public class PermissionProvider : IPermissionProvider
    {
        public const string SocialLinksName = "SocialLinksManagement";
        public static readonly PermissionRecord SocialLinksManagement = new()
            { Name = "SocialLinks management", SystemName = SocialLinksName, Category = "Standard" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                SocialLinksManagement,
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
                        SocialLinksManagement,
                    }
                )
            };
        }
    }
}