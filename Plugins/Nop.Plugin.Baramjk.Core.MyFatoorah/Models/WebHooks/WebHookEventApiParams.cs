using System;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models.WebHooks
{
    public class WebHookEventApiParams
    {
        public int Code { get; set; }
        public string Name { get; set; }
        public string CountryIsoCode { get; set; }
        public DateTime CreationDate { get; set; }
        public string Reference { get; set; }
    }
}