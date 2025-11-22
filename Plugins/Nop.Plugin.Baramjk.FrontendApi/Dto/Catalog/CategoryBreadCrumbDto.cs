namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class CategoryBreadCrumbDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BreadCrumb { get; set; }
        public int ParentId { get; set; }
    }
}