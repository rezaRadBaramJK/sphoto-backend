using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ContactInfos
{
    public record ContactInfoViewModel : BaseNopEntityModel
    {
        public ContactInfoViewModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableSubjects = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.LastName")]
        public string LastName { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.FullName")]
        public string FullName { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.Country")]
        public int CountryId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.Country")]
        public string CountryName { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.Subject")]
        public int SubjectId { get; set; }


        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.TicketId")]
        public int TicketId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.Subject")]
        public string SubjectName { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.File")]
        [UIHint("Download")]
        public int FileId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.Message")]
        public string Message { get; set; }


        public IList<SelectListItem> AvailableSubjects { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
    }
}