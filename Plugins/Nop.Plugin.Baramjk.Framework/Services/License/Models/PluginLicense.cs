using System;
using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Framework.Services.License.Models
{
    public class PluginLicense
    {
        public string License { get; set; }

        public string Type { get; set; }

        public List<string> Plugins { get; set; }

        public List<string> Domains { get; set; }

        public DateTime ExpireDateTime { get; set; }
    }
}