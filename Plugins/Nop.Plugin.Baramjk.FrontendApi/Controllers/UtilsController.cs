using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Framework.Models;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class UtilsController : BaseNopWebApiFrontendController
    {
        [HttpPost("/api-frontend/Utils/Date/GenerateDate")]
        [HttpPost("/FrontendApi/Utils/Date/GenerateDate")]
        public IActionResult GenerateDate([FromBody] GenerateDateRequest model)
        {
            var gapCount = model.PeriodType switch
            {
                RecurringProductCyclePeriod.Days => 1,
                RecurringProductCyclePeriod.Weeks => 7,
                RecurringProductCyclePeriod.Months => 30,
                RecurringProductCyclePeriod.Years => 365,
                _ => 0
            };
            gapCount *= model.GapCount + 1;

            var dates = new List<string>();
            if (model.SkipStartDate == false)
                dates.Add(model.StartDate.ToString("yyyy/MM/dd"));

            for (var i = 0; i < model.RepetitionCount; i++)
            {
                model.StartDate = model.StartDate.AddDays(gapCount);
                dates.Add(model.StartDate.ToString("yyyy/MM/dd"));
            }

            return ApiResponseFactory.Success(dates);
        }
    }

    public class GenerateDateRequest
    {
        public DateTime StartDate { get; set; }
        public RecurringProductCyclePeriod PeriodType { get; set; }
        public int GapCount { get; set; }
        public int RepetitionCount { get; set; }
        public bool SkipStartDate { get; set; }
    }
}