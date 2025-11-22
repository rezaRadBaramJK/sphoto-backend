namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models
{
    public class VerifyBySignatureParams
    {
        public string MyFatoorahSignature { get; set; }
        
        public string TranslationGuid { get; set; }
        
        public decimal PaidAmount { get; set; }
    }
}