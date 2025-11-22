using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Models.ViewModels.ContactInfos
{
    public record ContactInfoViewModel : BaseNopEntityModel
    {
        public ContactInfoViewModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableSubjects = new List<SelectListItem>();
        }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.ContactInfos.FirstName")]
        public string FirstName { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.ContactInfos.LastName")]
        public string LastName { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.ContactInfos.FullName")]
        public string FullName { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.ContactInfos.Country")]
        public int CountryId { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.ContactInfos.Country")]
        public string CountryName { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.ContactInfos.PhoneNumber")]
        public string PhoneNumber { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.ContactInfos.Email")]
        public string Email { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.ContactInfos.Subject")]
        public int SubjectId { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.ContactInfos.Subject")]
        public string SubjectName { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.ContactInfos.File")]
        [UIHint("Download")]
        public int FileId { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.ContactInfos.Message")]
        public string Message { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.ContactInfos.HasBeenPaid")]
        public bool HasBeenPaid { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.ContactInfos.PaymentDateTime")]
        public string PaymentDateTime { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.ContactInfos.PaymentStatus")]
        public string PaymentStatus { get; set; }
        
        public IList<SelectListItem> AvailableSubjects { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
    }
}