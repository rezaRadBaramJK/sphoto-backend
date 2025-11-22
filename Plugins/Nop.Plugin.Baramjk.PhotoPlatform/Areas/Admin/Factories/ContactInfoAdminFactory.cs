using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ContactInfos;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Services.ContactUs;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories
{
    public class ContactInfoAdminFactory
    {
        private readonly PhotoPlatformContactInfoService _contactInfoService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly PhotoPlatformSubjectService _subjectService;

        public ContactInfoAdminFactory(
            PhotoPlatformContactInfoService contactInfoService,
            ICountryService countryService,
            ILocalizationService localizationService,
            PhotoPlatformSubjectService subjectService)
        {
            _contactInfoService = contactInfoService;
            _countryService = countryService;
            _localizationService = localizationService;
            _subjectService = subjectService;
        }

        public ContactInfoSearchModel PrepareSearchModel()
        {
            var model = new ContactInfoSearchModel();
            model.SetGridPageSize();
            return model;
        }

        public async Task<ContactInfoListModel> PrepareListModelAsync(ContactInfoSearchModel searchModel)
        {
            var pageIndex = searchModel.Page - 1;
            var pageSize = searchModel.PageSize;
            var entities = await _contactInfoService.GetAsync(pageIndex, pageSize);
            return await new ContactInfoListModel().PrepareToGridAsync(searchModel, entities, () =>
            {
                return entities.SelectAwait(async e =>
                {
                    var country = await _countryService.GetCountryByIdAsync(e.CountryId);
                    var subject = await _subjectService.GetByIdAsync(e.SubjectId);

                    return new ContactInfoViewModel
                    {
                        Id = e.Id,
                        Email = e.Email,
                        FullName = $"{e.FirstName} {e.LastName}",
                        PhoneNumber = e.PhoneNumber,
                        TicketId = e.TicketId,
                        CountryName = country == null
                            ? string.Empty
                            : await _localizationService.GetLocalizedAsync(country, c => c.Name),
                        SubjectName = subject == null
                            ? string.Empty
                            : await _localizationService.GetLocalizedAsync(subject, s => s.Name),
                    };
                });
            });
        }


        /// <exception cref="NopException"></exception>
        public async Task<ContactInfoViewModel> PrepareViewModelAsync(ContactInfoEntity entity)
        {
            var country = await _countryService.GetCountryByIdAsync(entity.CountryId);
            if (country == null)
                throw new NopException($"Invalid country. Country with id {entity.CountryId} not found.");

            var subject = await _subjectService.GetByIdAsync(entity.SubjectId);
            if (subject == null)
                throw new NopException($"Invalid subject. Subject with id {entity.SubjectId} not found.");

            return new ContactInfoViewModel
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                CountryId = entity.CountryId,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                SubjectId = entity.SubjectId,
                FileId = entity.FileId,
                TicketId = entity.TicketId,
                Message = entity.Message,
                AvailableCountries = await PrepareCountriesAsync(country),
                AvailableSubjects = await PrepareSubjectsAsync(subject)
            };
        }

        private async Task<IList<SelectListItem>> PrepareCountriesAsync(Country selectedCountry)
        {
            var countries = await _countryService.GetAllCountriesAsync();

            if (countries.Any(c => c.Id == selectedCountry.Id) == false)
                countries.Insert(0, selectedCountry);

            return await countries.SelectAwait(async country => new SelectListItem
            {
                Text = await _localizationService.GetLocalizedAsync(country, c => c.Name),
                Value = country.Id.ToString()
            }).ToListAsync();
        }

        private async Task<IList<SelectListItem>> PrepareSubjectsAsync(SubjectEntity selectedSubject)
        {
            var subjects = await _subjectService.GetAsync();

            if (subjects.Any(s => s.Id == selectedSubject.Id) == false)
                subjects.Insert(0, selectedSubject);

            return await subjects.SelectAwait(async subject => new SelectListItem
            {
                Text = await _localizationService.GetLocalizedAsync(subject, s => s.Name),
                Value = subject.Id.ToString()
            }).ToListAsync();
        }
    }
}