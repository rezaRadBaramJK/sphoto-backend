using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Baramjk.Core.Extensions;
using Nop.Plugin.Baramjk.Core.Models.Vendors;
using Nop.Plugin.Baramjk.Framework.Domain.Vendors;
using Nop.Plugin.Baramjk.Framework.Services.Vendors;
using Nop.Services.Localization;

namespace Nop.Plugin.Baramjk.Core.Factories
{
    public class VendorAdminFactory
    {
        private readonly VendorDetailsService _vendorDetailsService;
        private readonly ILocalizationService _localizationService;

        public VendorAdminFactory(
            VendorDetailsService vendorDetailsService,
            ILocalizationService localizationService)
        {
            _vendorDetailsService = vendorDetailsService;
            _localizationService = localizationService;
        }

        public async Task<VendorDetailsViewModel> PrepareVendorBranchViewModelAsync(int vendorId)
        {
            var vendorDetail = await _vendorDetailsService.GetDetailsByVendorIdAsync(vendorId) ?? new VendorDetail
            {
                VendorId = vendorId
            };
            return new VendorDetailsViewModel
            {
                VendorId = vendorId,
                StartTime = vendorDetail.StartTime,
                EndTime = vendorDetail.EndTime,
                OffDaysOfWeekIds = vendorDetail.OffDaysOfWeekIds.ToIntArray().Select(i => i + 1).ToList(),
                IsAvailable = vendorDetail.IsAvailable,
                AvailableDaysOfWeek = await PrepareAvailableDaysOfWeekAsync(),
            };
        }
        
        private async Task<IList<SelectListItem>> PrepareAvailableDaysOfWeekAsync()
        {
            return await Enum.GetValues<DayOfWeek>().SelectAwait(async day => new SelectListItem
            {
                Text = await _localizationService.GetLocalizedEnumAsync(day),
                Value = $"{(int)day + 1}"
            }).ToListAsync();
        }
    }
}