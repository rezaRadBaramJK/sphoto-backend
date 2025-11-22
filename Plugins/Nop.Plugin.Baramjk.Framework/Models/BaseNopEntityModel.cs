
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Framework.Models
{
    public partial record FrameworkBaseNopEntityModel : BaseNopModel
    {
        public virtual int Id { get; set; }
    }
}