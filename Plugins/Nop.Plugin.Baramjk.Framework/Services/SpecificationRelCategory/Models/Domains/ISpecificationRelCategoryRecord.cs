namespace Nop.Plugin.Baramjk.Framework.Services.SpecificationRelCategory.Models.Domains
{
    public interface ISpecificationRelCategoryRecord
    {
        int Id { get; set; }
        int SpecificationId { get; set; }
        int CategoryId { get; set; }
        
    }
}