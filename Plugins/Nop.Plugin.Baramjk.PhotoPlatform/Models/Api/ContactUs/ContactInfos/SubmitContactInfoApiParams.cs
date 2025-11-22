namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.ContactUs.ContactInfos
{
    public class SubmitContactInfoApiParams
    {
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public int TicketId { get; set; }
        
        public int CountryId { get; set; }
        
        public string PhoneNumber { get; set; }
        
        public string Email { get; set; }
        
        public int SubjectId { get; set; }
        
        public string FileGuid { get; set; }
        
        public string Message { get; set; }
        
    }
}