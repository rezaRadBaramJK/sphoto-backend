using System;
using Nop.Core;

namespace Nop.Plugin.Baramjk.Framework.Domain
{
    public class PluginLicenseRecord : BaseEntity
    {
        public string License { get; set; }
        public string Type { get; set; }
        public string PluginName { get; set; }
        public string Domains { get; set; }
        public DateTime ExpireDateTime { get; set; }
        public DateTime OnCreate { get; set; }
    }
}