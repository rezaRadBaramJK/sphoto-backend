using System;
using Nop.Core.Domain.Directory;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Currencies
{
    public class CurrencyDto
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string CurrencyCode { get; set; }
        
        public string LocalizationCurrencyCode { get; set; }
        
        public decimal Rate { get; set; }
        
        public string DisplayLocale { get; set; }
        
        public string CustomFormatting { get; set; }
        
        public bool LimitedToStores { get; set; }
        
        public bool Published { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public bool IsSelected { get; set; }
        
        public DateTime CreatedOnUtc { get; set; }
        
        public DateTime UpdatedOnUtc { get; set; }
        
        public int RoundingTypeId { get; set; }
        
        public RoundingType RoundingType
        {
            get => (RoundingType)RoundingTypeId;
            set => RoundingTypeId = (int)value;
        }
    }
}