using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Reports;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Abstractions
{
    public interface ITechnicianReportService
    {
        Task<byte[]> GetWeaklyReportAsync(
            Dictionary<DayOfWeek, TechnicianDailyReportModel> weeklyReportDictionary,
            DateTime fromDate,
            DateTime toDate);
    }
}