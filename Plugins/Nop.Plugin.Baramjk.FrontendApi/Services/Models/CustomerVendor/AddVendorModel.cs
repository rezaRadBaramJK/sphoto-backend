using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.Models.CustomerVendor
{
    public class AddVendorModel
    {
        [DataType(DataType.EmailAddress)]
        [Required]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [MinLength(5)]
        public string Phone { get; set; }

        public string CompanyName { get; set; }

        public DateTime? BirthDay { get; set; }
        public int PictureId { get; set; }
        public string Description { get; set; }

        public Dictionary<string, string> Attributes { get; set; }
        
        public Dictionary<string, string> GenericAttributes { get; set; }
    }
}