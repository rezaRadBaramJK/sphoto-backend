using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models.Suppliers
{
    public class SupplierLocalizationModel: ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }
        
        public string Name { get; set; }
        
    }
}