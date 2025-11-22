using System;
using Nop.Core;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class ContactInfoEntity: BaseEntity
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int CountryId { get; set; }
        
        public int TicketId { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public int SubjectId { get; set; }

        public int FileId { get; set; }

        public string Message { get; set; }

        public bool HasBeenPaid { get; set; }


        public DateTime? PaymentUtcDateTime { get; set; }
    }
}