using System;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services
{
    public class RegisterCustomerServiceParams
    {
        public string Email { get; set; }
        
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Phone { get; set; }
        
        public DateTime? BirthDay { get; set; }
        
        public int StoreId { get; set; }
        
        public string Gender { get; set; }
    }
}