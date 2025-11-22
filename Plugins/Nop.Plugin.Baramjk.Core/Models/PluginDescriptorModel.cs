using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.Ui.DataTables;
using Nop.Web.Framework.Models.DataTables;

namespace Nop.Plugin.Baramjk.Core.Models
{
    public class PluginDescriptorModel : IDomainModel
    {
        public int Id { get; set; }

        [RenderPicture()]
        public virtual string Logo => $"/Plugins/{SystemName}/logo.jpg";

        public virtual string SystemName { get; set; }
        public virtual string FriendlyName { get; set; }
        public virtual bool Installed { get; set; }
        public virtual string Version { get; set; }

        [RenderButtonCustomUrl("CreateDemoLicense/", Title = "Create Demo License", ClassName = "RenderButtonUrl")]
        public string CreateDemoLicense => SystemName;

        [RenderButtonCustomFunction("troubleshoot", Title = "Troubleshoot", ClassName = "btn btn-default")]
        public string Troubleshoot => $"'{SystemName}'";
    }
}