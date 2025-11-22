namespace Nop.Plugin.Baramjk.Framework.Services.SpecificationRelCategory.Models.Domains
{
    public interface ISpecificationOptionRelCategoryRecord
    {
        int Id { get; set; }
        int SpecificationId { get; set; }
        int OptionId { get; set; }
        int CategoryId { get; set; }
    }
}