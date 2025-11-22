namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Country
{
    public class CountryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public bool AllowsBilling { get; set; }
        
        public bool AllowsShipping { get; set; }
        
        public string TwoLetterIsoCode { get; set; }
        
        public string ThreeLetterIsoCode { get; set; }
        
        public int NumericIsoCode { get; set; }
        
        public bool SubjectToVat { get; set; }
        
        public bool Published { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public bool LimitedToStores { get; set; } 
    }
}