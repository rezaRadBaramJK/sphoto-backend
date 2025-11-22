using System;
using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Customer
{
    public class CustomerInfoDto : ModelDto
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? DateOfBirthDay { get; set; }
        public int? DateOfBirthMonth { get; set; }
        public int? DateOfBirthYear { get; set; }
        public string Company { get; set; }
        public string StreetAddress { get; set; }
        public string StreetAddress2 { get; set; }
        public string ZipPostalCode { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public int CountryId { get; set; }
        public int StateProvinceId { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Signature { get; set; }
        public string TimeZoneId { get; set; }
        public string VatNumber { get; set; }
        public string VatNumberStatusNote { get; set; }
        public bool DisplayVatNumber { get; set; }
        public string AvatarUrl { get; set; }
        public IList<CustomerAttributeModelDto> CustomerAttributes { get; set; }
        public DateTime RegisterDateTimeOnUtc { get; set; }
        public int? BillingAddressId { get; set; }
        public int? ShippingAddressId { get; set; }
    }
}