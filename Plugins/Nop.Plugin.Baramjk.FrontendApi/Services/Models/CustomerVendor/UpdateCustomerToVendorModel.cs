using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.Models.CustomerVendor
{
    public class UpdateCustomerToVendorModel
    {
        public Customer Customer { get; set; }
        
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string Password { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Phone { get; set; }

        public string CompanyName { get; set; }

        public DateTime? BirthDay { get; set; }
        
        public int PictureId { get; set; }
        
        public string Description { get; set; }
        
        public string AdminComment { get; set; }

        public Dictionary<string, string> Attributes { get; set; }
        
        public Dictionary<string, string> GenericAttributes { get; set; }
    }
}