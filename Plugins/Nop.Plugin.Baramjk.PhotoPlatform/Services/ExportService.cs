using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Tickets;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.Actor;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.Production;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.Supervisor;
using Nop.Services.Directory;
using Nop.Services.Localization;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class ExportService
    {
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;


        public ExportService(ILocalizationService localizationService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings)
        {
            _localizationService = localizationService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
        }


        private static readonly string[] TicketsRevenueHeaders =
        {
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.EventName",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.EventDate",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.Time",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.OrderId",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.MyFatoorahRef",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.TicketPrice",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.TicketType",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.VisaFees",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.KNetFees",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ExchangeRate",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.NetPrice",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.PaymentType",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.WalletUsed",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.PhotosNotUsed",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.PhotosUsed",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.AccountantName",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ActorName",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ActorShare",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ProductionShare",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ClientName",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ClientPhone",
            "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ClientEmail"
        };


        private static readonly string[] ActorRevenueHeaders =
        {
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.EventName",
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.Date",
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TimeSlot",
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TicketPrice",
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.ActorShare",
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.CustomerMobilePhotoCount",
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.CameraManPhotoCount",
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalPhotoCount",
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalPhotosPrice"
        };

        private static readonly string[] ActorRevenueSubHeaders =
        {
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TimeSlot",
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TicketPrice",
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.ActorShare",
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.CustomerMobilePhotoCount",
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.CameraManPhotoCount",
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalPhotoCount",
            "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalPhotosPrice"
        };


        public async Task<byte[]> ExportTicketsRevenueAsync(List<TicketsReportExportModel> tickets)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(
                await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ReportName"));

            await WriteHeader(worksheet);
            WriteBody(worksheet, tickets);

            var allCells = worksheet.Cells[worksheet.Dimension.Address];
            allCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            allCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return await Task.FromResult(package.GetAsByteArray());
        }

        private async Task WriteHeader(ExcelWorksheet worksheet)
        {
            for (var i = 0; i < TicketsRevenueHeaders.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = await _localizationService.GetResourceAsync(TicketsRevenueHeaders[i]);
            }

            worksheet.Row(1).Style.Font.Bold = true;
        }

        private static void WriteBody(ExcelWorksheet worksheet, List<TicketsReportExportModel> tickets)
        {
            tickets.Aggregate(2, (current, ticket) => WriteTicketRow(worksheet, ticket, current));
        }

        private static int WriteTicketRow(ExcelWorksheet worksheet, TicketsReportExportModel ticket, int startRow)
        {
            var actors = ticket.ActorsData?.Any() == true
                ? ticket.ActorsData
                : new List<TicketsReportActorPartModel> { new() };

            var actorCount = actors.Count;
            var endRow = startRow + actorCount - 1;

            var mergedColumnData = new (int Column, object Value)[]
            {
                (1, ticket.EventName), (2, ticket.EventDate.ToString("yyyy-MM-dd")), (3, ticket.TimeSlot),
                (4, ticket.OrderId), (5, ticket.MyFatoorahReference), (6, ticket.TicketPrice), (7, ticket.TicketType),
                (8, ticket.VisaFee), (9, ticket.KNetFee), (10, ticket.ExchangeRate), (11, ticket.NetPrice),
                (12, ticket.PaymentType), (13, ticket.WalletUsedAmount), (14, ticket.NotUsedPhotosCount),
                (15, ticket.UsedPhotosCount), (16, ticket.AccountantName), (20, ticket.ClientName),
                (21, ticket.ClientPhoneNumber), (22, ticket.ClientEmail)
            };

            foreach (var data in mergedColumnData)
            {
                var cell = worksheet.Cells[startRow, data.Column];
                cell.Value = data.Value;

                if (actorCount <= 1) continue;
                worksheet.Cells[startRow, data.Column, endRow, data.Column].Merge = true;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            for (var i = 0; i < actorCount; i++)
            {
                var currentRow = startRow + i;
                var actor = actors[i];
                worksheet.Cells[currentRow, 17].Value = actor.ActorName;
                worksheet.Cells[currentRow, 18].Value = actor.ActorShare;
                worksheet.Cells[currentRow, 19].Value = actor.ProductionShare;
            }

            return endRow + 1;
        }

        public async Task<byte[]> ExportActorRevenueAsync(ActorTimeSlotRevenueListModel model)
        {
            using var package = new ExcelPackage();
            var worksheet =
                package.Workbook.Worksheets.Add(
                    await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.ReportName"));

            var fontColor = ColorTranslator.FromHtml("#0066b3");
            var borderColor = ColorTranslator.FromHtml("#b0d235");

            worksheet.Cells.Style.Font.Size = 12;
            worksheet.Cells.Style.Font.Color.SetColor(fontColor);

            var row = 2;

            var currencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
            var currencyFormat = $"#,##0.00 \"{currencyCode}\"";

            if (model.GroupedByDate.Count > 1)
            {
                foreach (var group in model.GroupedByDate)
                {
                    var groupTitleCell = worksheet.Cells[row, 1];
                    groupTitleCell.Value = $"{group.Items.First().EventName} - {group.Date}";
                    groupTitleCell.Style.Font.Bold = true;
                    groupTitleCell.Style.Font.Color.SetColor(fontColor);
                    groupTitleCell.Style.Font.Size = 14;
                    groupTitleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    SetBorders(groupTitleCell, borderColor);
                    row++;

                    var subHeaderRange = worksheet.Cells[row, 1, row, ActorRevenueSubHeaders.Length];
                    for (int i = 0; i < ActorRevenueSubHeaders.Length; i++)
                    {
                        worksheet.Cells[row, i + 1].Value = await _localizationService.GetResourceAsync(ActorRevenueSubHeaders[i]);
                    }

                    subHeaderRange.Style.Font.Bold = true;
                    subHeaderRange.Style.Font.Color.SetColor(fontColor);
                    SetBorders(subHeaderRange, borderColor);
                    row++;

                    foreach (var item in group.Items)
                    {
                        row = WriteGroupItemRow(worksheet, item, row, fontColor, borderColor, currencyFormat);
                    }

                    var dayTotalRowRange = worksheet.Cells[row, 1, row, ActorRevenueSubHeaders.Length];
                    dayTotalRowRange.Style.Font.Bold = true;
                    dayTotalRowRange.Style.Font.Color.SetColor(fontColor);
                    dayTotalRowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    dayTotalRowRange.Style.Fill.BackgroundColor.SetColor(Color.White);
                    SetBorders(dayTotalRowRange, borderColor);

                    worksheet.Cells[row, 1].Value =
                        await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.Total");
                    worksheet.Cells[row, 1, row, 3].Merge = true;

                    worksheet.Cells[row, 4].Value = group.Items.Sum(x => x.CustomerMobilePhotoCount);
                    worksheet.Cells[row, 5].Value = group.Items.Sum(x => x.CameraManPhotoCount);
                    worksheet.Cells[row, 6].Value = group.TotalDayPhotoCount;
                    worksheet.Cells[row, 7].Value = group.TotalDayPhotoPrice;
                    worksheet.Cells[row, 7].Style.Numberformat.Format = currencyFormat;

                    row += 2;
                }

                var grandTotalStartRow = row;

                var grandTotalTitleCell = worksheet.Cells[grandTotalStartRow, 1, grandTotalStartRow + 1, 3];
                grandTotalTitleCell.Merge = true;
                grandTotalTitleCell.Value =
                    await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.GrandTotal");
                grandTotalTitleCell.Style.Font.Bold = true;
                grandTotalTitleCell.Style.Font.Color.SetColor(fontColor);
                grandTotalTitleCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                grandTotalTitleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                SetBorders(grandTotalTitleCell, borderColor);


                string[] grandTotalHeaders =
                {
                    "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalCustomerMobilePhotoCount",
                    "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalCameraManPhotoCount",
                    "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalTickets",
                    "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalPrice"
                };
                var grandTotalHeaderRange = worksheet.Cells[grandTotalStartRow, 4, grandTotalStartRow, 4 + grandTotalHeaders.Length - 1];
                for (int i = 0; i < grandTotalHeaders.Length; i++)
                {
                    var cell = worksheet.Cells[grandTotalStartRow, 4 + i];
                    cell.Value = await _localizationService.GetResourceAsync(grandTotalHeaders[i]);
                }

                grandTotalHeaderRange.Style.Font.Bold = true;
                grandTotalHeaderRange.Style.Font.Color.SetColor(fontColor);
                SetBorders(grandTotalHeaderRange, borderColor);

                var grandTotalValuesRow = grandTotalStartRow + 1;
                worksheet.Cells[grandTotalValuesRow, 4].Value = model.GroupedByDate.SelectMany(g => g.Items).Sum(i => i.CustomerMobilePhotoCount);
                worksheet.Cells[grandTotalValuesRow, 5].Value = model.GroupedByDate.SelectMany(g => g.Items).Sum(i => i.CameraManPhotoCount);
                worksheet.Cells[grandTotalValuesRow, 6].Value = model.TotalPhotoCount;
                worksheet.Cells[grandTotalValuesRow, 7].Value = model.TotalPhotoPrice;
                worksheet.Cells[grandTotalValuesRow, 7].Style.Numberformat.Format = currencyFormat;

                var grandTotalValuesRange = worksheet.Cells[grandTotalValuesRow, 4, grandTotalValuesRow, 4 + grandTotalHeaders.Length - 1];
                grandTotalValuesRange.Style.Font.Color.SetColor(fontColor);
                grandTotalValuesRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                grandTotalValuesRange.Style.Fill.BackgroundColor.SetColor(Color.White);
                SetBorders(grandTotalValuesRange, borderColor);
            }
            else
            {
                if (model.GroupedByDate.Count == 1 && model.GroupedByDate.First().Items.Count > 1)
                {
                    var summaryStartRow = row;
                    var headerStartColumn = 6;

                    var summaryTitleCell = worksheet.Cells[summaryStartRow, 1, summaryStartRow + 1, 1];
                    summaryTitleCell.Merge = true;
                    summaryTitleCell.Value =
                        await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.Summary");
                    summaryTitleCell.Style.Font.Bold = true;
                    summaryTitleCell.Style.Font.Color.SetColor(fontColor);
                    summaryTitleCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    summaryTitleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    SetBorders(summaryTitleCell, borderColor);

                    var blankCells = worksheet.Cells[summaryStartRow, 2, summaryStartRow + 1, headerStartColumn - 1];
                    blankCells.Merge = true;
                    SetBorders(blankCells, borderColor);

                    string[] summaryHeaders =
                    {
                        "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalCustomerMobilePhotoCount",
                        "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalCameraManPhotoCount",
                        "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalTickets",
                        "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalPrice"
                    };
                    var summaryHeaderRange = worksheet.Cells[summaryStartRow, headerStartColumn, summaryStartRow,
                        headerStartColumn + summaryHeaders.Length - 1];
                    for (int i = 0; i < summaryHeaders.Length; i++)
                    {
                        var cell = worksheet.Cells[summaryStartRow, headerStartColumn + i];
                        cell.Value = await _localizationService.GetResourceAsync(summaryHeaders[i]);
                    }

                    summaryHeaderRange.Style.Font.Bold = true;
                    summaryHeaderRange.Style.Font.Color.SetColor(fontColor);
                    SetBorders(summaryHeaderRange, borderColor);

                    var summaryValuesRow = summaryStartRow + 1;
                    worksheet.Cells[summaryValuesRow, headerStartColumn].Value =
                        model.GroupedByDate.SelectMany(g => g.Items).Sum(i => i.CustomerMobilePhotoCount);
                    worksheet.Cells[summaryValuesRow, headerStartColumn + 1].Value =
                        model.GroupedByDate.SelectMany(g => g.Items).Sum(i => i.CameraManPhotoCount);
                    worksheet.Cells[summaryValuesRow, headerStartColumn + 2].Value = model.TotalPhotoCount;
                    worksheet.Cells[summaryValuesRow, headerStartColumn + 3].Value = model.TotalPhotoPrice;
                    worksheet.Cells[summaryValuesRow, headerStartColumn + 3].Style.Numberformat.Format = currencyFormat;

                    var summaryValuesRange = worksheet.Cells[summaryValuesRow, headerStartColumn, summaryValuesRow,
                        headerStartColumn + summaryHeaders.Length - 1];
                    summaryValuesRange.Style.Font.Color.SetColor(fontColor);
                    SetBorders(summaryValuesRange, borderColor);
                    row = summaryStartRow + 3;
                }

                row = await WriteHeaderAsync(worksheet, row, borderColor, fontColor);

                foreach (var group in model.GroupedByDate)
                {
                    foreach (var item in group.Items)
                    {
                        row = WriteItemRow(worksheet, item, row, fontColor, borderColor, currencyFormat);
                    }

                    if (model.GroupedByDate.Count == 1 && model.GroupedByDate.First().Items.Count > 1)
                    {
                        var dayTotalRowRange = worksheet.Cells[row, 1, row, ActorRevenueHeaders.Length];
                        dayTotalRowRange.Style.Font.Bold = true;
                        dayTotalRowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        dayTotalRowRange.Style.Fill.BackgroundColor.SetColor(Color.White);
                        dayTotalRowRange.Style.Font.Color.SetColor(fontColor);
                        SetBorders(dayTotalRowRange, borderColor);
                        worksheet.Cells[row, 1].Value =
                            await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.Total");
                        worksheet.Cells[row, 1, row, 5].Merge = true;
                        worksheet.Cells[row, 6].Value = group.Items.Sum(x => x.CustomerMobilePhotoCount);
                        worksheet.Cells[row, 7].Value = group.Items.Sum(x => x.CameraManPhotoCount);
                        worksheet.Cells[row, 8].Value = group.TotalDayPhotoCount;
                        worksheet.Cells[row, 9].Value = group.TotalDayPhotoPrice;
                        worksheet.Cells[row, 9].Style.Numberformat.Format = currencyFormat;
                    }


                    row += 2;
                }
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            return await Task.FromResult(package.GetAsByteArray());
        }

        private static int WriteGroupItemRow(ExcelWorksheet worksheet, ActorTimeSlotRevenueItemModel item, int row, Color fontColor,
            Color borderColor, string currencyFormat)
        {
            worksheet.Cells[row, 1].Value = item.TimeSlot;
            worksheet.Cells[row, 2].Value = item.TicketPrice;
            worksheet.Cells[row, 2].Style.Numberformat.Format = currencyFormat;
            worksheet.Cells[row, 3].Value = item.ActorShare;
            worksheet.Cells[row, 3].Style.Numberformat.Format = currencyFormat;
            worksheet.Cells[row, 4].Value = item.CustomerMobilePhotoCount;
            worksheet.Cells[row, 5].Value = item.CameraManPhotoCount;
            worksheet.Cells[row, 6].Value = item.TotalPhotoCount;
            worksheet.Cells[row, 7].Value = item.TotalPhotoPrice;
            worksheet.Cells[row, 7].Style.Numberformat.Format = currencyFormat;

            var rowRange = worksheet.Cells[row, 1, row, ActorRevenueSubHeaders.Length];
            rowRange.Style.Font.Color.SetColor(fontColor);
            SetBorders(rowRange, borderColor);

            return row + 1;
        }

        private async Task<int> WriteHeaderAsync(ExcelWorksheet worksheet, int row, Color borderColor, Color fontColor)
        {
            var headerCells = worksheet.Cells[row, 1, row, ActorRevenueHeaders.Length];
            for (var i = 0; i < ActorRevenueHeaders.Length; i++)
            {
                worksheet.Cells[row, i + 1].Value = await _localizationService.GetResourceAsync(ActorRevenueHeaders[i]);
            }

            headerCells.Style.Font.Bold = true;
            headerCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerCells.Style.Fill.BackgroundColor.SetColor(Color.White);
            headerCells.Style.Font.Color.SetColor(fontColor);
            SetBorders(headerCells, borderColor);

            return row + 1;
        }

        private static int WriteItemRow(ExcelWorksheet worksheet, ActorTimeSlotRevenueItemModel item, int row, Color fontColor, Color borderColor,
            string currencyFormat)
        {
            worksheet.Cells[row, 1].Value = item.EventName;
            worksheet.Cells[row, 2].Value = item.Date;
            worksheet.Cells[row, 3].Value = item.TimeSlot;
            worksheet.Cells[row, 4].Value = item.TicketPrice;
            worksheet.Cells[row, 4].Style.Numberformat.Format = currencyFormat;

            worksheet.Cells[row, 5].Value = item.ActorShare;
            worksheet.Cells[row, 5].Style.Numberformat.Format = currencyFormat;
            worksheet.Cells[row, 6].Value = item.CustomerMobilePhotoCount;
            worksheet.Cells[row, 7].Value = item.CameraManPhotoCount;
            worksheet.Cells[row, 8].Value = item.TotalPhotoCount;
            worksheet.Cells[row, 9].Value = item.TotalPhotoPrice;
            worksheet.Cells[row, 9].Style.Numberformat.Format = currencyFormat;


            var rowRange = worksheet.Cells[row, 1, row, ActorRevenueHeaders.Length];
            rowRange.Style.Font.Color.SetColor(fontColor);
            SetBorders(rowRange, borderColor);

            return row + 1;
        }

        private static void SetBorders(ExcelRange range, Color borderColor)
        {
            range.Style.Border.Top.Style = ExcelBorderStyle.Medium;
            range.Style.Border.Left.Style = ExcelBorderStyle.Medium;
            range.Style.Border.Right.Style = ExcelBorderStyle.Medium;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;

            range.Style.Border.Top.Color.SetColor(borderColor);
            range.Style.Border.Left.Color.SetColor(borderColor);
            range.Style.Border.Right.Color.SetColor(borderColor);
            range.Style.Border.Bottom.Color.SetColor(borderColor);
        }

        private async Task PrepareProductionInfoCellsAsync(ExcelWorksheet worksheet, int row, Color borderColor, bool isSingleShowReport,
            string eventName, DateTime eventDate, TimeSpan eventTime)
        {
            int lastHeaderCol = isSingleShowReport ? 3 : 2;
            var infoHeader = worksheet.Cells[row, 1, row, lastHeaderCol];
            worksheet.Cells[row, 1].Value =
                await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.EventName");
            worksheet.Cells[row, 2].Value =
                await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.Date");

            if (isSingleShowReport)
            {
                worksheet.Cells[row, 3].Value =
                    await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TimeSlot");
            }

            infoHeader.Style.Font.Bold = true;
            infoHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
            infoHeader.Style.Fill.BackgroundColor.SetColor(borderColor);
            infoHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            infoHeader.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            SetBorders(infoHeader, borderColor);

            row++;

            int lastValueCol = isSingleShowReport ? 3 : 2;
            var infoValues = worksheet.Cells[row, 1, row, lastValueCol];

            worksheet.Cells[row, 1].Value = eventName;
            worksheet.Cells[row, 2].Value = eventDate.ToString("d");
            if (isSingleShowReport)
            {
                worksheet.Cells[row, 3].Value = eventTime.ToString(@"hh\:mm");
            }

            SetBorders(infoValues, borderColor);
            infoValues.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            infoValues.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        private async Task PrepareSupervisorInfoCellsAsync(ExcelWorksheet worksheet, int row, Color borderColor, bool isSingleShowReport,
            string eventName, DateTime eventDate, TimeSpan eventTime, string reportType)
        {
            int lastHeaderCol = isSingleShowReport ? 4 : 3;
            var infoHeader = worksheet.Cells[row, 1, row, lastHeaderCol];
            worksheet.Cells[row, 1].Value =
                await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.EventName");
            worksheet.Cells[row, 2].Value =
                await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.Date");

            if (isSingleShowReport)
            {
                worksheet.Cells[row, 3].Value =
                    await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.TimeSlot");
                worksheet.Cells[row, 4].Value =
                    await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.ReportType");
            }
            else
            {
                worksheet.Cells[row, 3].Value =
                    await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.ReportType");
            }

            infoHeader.Style.Font.Bold = true;
            infoHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
            infoHeader.Style.Fill.BackgroundColor.SetColor(borderColor);
            infoHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            infoHeader.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            SetBorders(infoHeader, borderColor);

            row++;

            int lastValueCol = isSingleShowReport ? 4 : 3;
            var infoValues = worksheet.Cells[row, 1, row, lastValueCol];

            worksheet.Cells[row, 1].Value = eventName;
            worksheet.Cells[row, 2].Value = eventDate.ToString("d");
            if (isSingleShowReport)
            {
                worksheet.Cells[row, 3].Value = eventTime.ToString(@"hh\:mm");
                worksheet.Cells[row, 4].Value = reportType;
            }

            else
            {
                worksheet.Cells[row, 3].Value = reportType;
            }

            SetBorders(infoValues, borderColor);
            infoValues.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            infoValues.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        private async Task PrepareReportHeadersAsync(
            ExcelWorksheet worksheet,
            int row,
            Color borderColor,
            bool showActors,
            bool isSingleShowReport)
        {
            var headers = new List<string>();

            if (showActors)
                headers.Add("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.ActorName");


            if (isSingleShowReport == false)
                headers.Add("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TimeSlot");

            headers.Add("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TicketPrice");
            headers.Add("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.ActorShare");
            headers.Add("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.CameraManPhotoCount");
            headers.Add("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.CustomerMobilePhotoCount");
            headers.Add("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TotalPhotoCount");
            headers.Add("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TotalPhotosPrice");

            var headerRange = worksheet.Cells[row, 1, row, headers.Count];
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cells[row, i + 1].Value = await _localizationService.GetResourceAsync(headers[i]);
            }

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.White);
            SetBorders(headerRange, borderColor);
        }

        private async Task<int> PrepareReportDataCellsAsync(ExcelWorksheet worksheet, int row, Color borderColor, string currencyFormat,
            TimeSlotGroupedData timeSlot, bool showActors, bool isSingleShowReport, decimal ticketPrice, decimal actorShare)
        {
            if (showActors)
            {
                foreach (var actor in timeSlot.ActorsData)
                {
                    int col = 1;
                    worksheet.Cells[row, col++].Value = actor.ActorName;
                    if (isSingleShowReport == false)
                        worksheet.Cells[row, col++].Value = timeSlot.EventTime.ToString(@"hh\:mm");


                    worksheet.Cells[row, col++].Value = actor.TicketPrice;
                    worksheet.Cells[row, col - 1].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, col++].Value = actor.ActorShare;
                    worksheet.Cells[row, col - 1].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, col++].Value = actor.CameraManPhotoCount;
                    worksheet.Cells[row, col++].Value = actor.CustomerMobilePhotoCount;
                    worksheet.Cells[row, col++].Value = actor.TotalPhotoCount;
                    worksheet.Cells[row, col++].Value = actor.TotalPhotoPrice;
                    worksheet.Cells[row, col - 1].Style.Numberformat.Format = currencyFormat;

                    SetBorders(worksheet.Cells[row, 1, row, worksheet.Dimension.End.Column], borderColor);
                    row++;
                }

                var tsTotalRow = worksheet.Cells[row, 1, row, worksheet.Dimension.End.Column];
                tsTotalRow.Style.Font.Bold = true;
                SetBorders(tsTotalRow, borderColor);

                worksheet.Row(row).Height = 30;
                worksheet.Cells[row, 1].Value =
                    await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.Total");

                int totalCol = worksheet.Dimension.End.Column - 3;

                worksheet.Cells[row, totalCol++].Value = timeSlot.ActorsData.Sum(x => x.CameraManPhotoCount);
                worksheet.Cells[row, totalCol++].Value = timeSlot.ActorsData.Sum(x => x.CustomerMobilePhotoCount);
                worksheet.Cells[row, totalCol++].Value = timeSlot.TotalTimeSlotPhotoCount;
                worksheet.Cells[row, totalCol].Value = timeSlot.TotalTimeSlotPhotoPrice;
                worksheet.Cells[row, totalCol].Style.Numberformat.Format = currencyFormat;
            }
            else
            {
                int col = 1;

                if (isSingleShowReport == false)
                    worksheet.Cells[row, col++].Value = timeSlot.EventTime.ToString(@"hh\:mm");


                worksheet.Cells[row, col++].Value = ticketPrice;
                worksheet.Cells[row, col - 1].Style.Numberformat.Format = currencyFormat;

                worksheet.Cells[row, col++].Value = actorShare;
                worksheet.Cells[row, col - 1].Style.Numberformat.Format = currencyFormat;

                worksheet.Cells[row, col++].Value = timeSlot.ActorsData.Sum(x => x.CameraManPhotoCount);

                worksheet.Cells[row, col++].Value = timeSlot.ActorsData.Sum(x => x.CustomerMobilePhotoCount);

                worksheet.Cells[row, col++].Value = timeSlot.TotalTimeSlotPhotoCount;

                worksheet.Cells[row, col++].Value = timeSlot.TotalTimeSlotPhotoPrice;
                worksheet.Cells[row, col - 1].Style.Numberformat.Format = currencyFormat;

                SetBorders(worksheet.Cells[row, 1, row, worksheet.Dimension.End.Column], borderColor);
            }

            return row + 1;
        }

        public async Task<byte[]> ExportProductionReportAsync(ProductionReportModel model, bool showActors)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(
                await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.Title"));


            var fontColor = ColorTranslator.FromHtml("#0066b3");
            var borderColor = ColorTranslator.FromHtml("#b0d235");
            worksheet.Cells.Style.Font.Size = 12;
            worksheet.Cells.Style.Font.Color.SetColor(fontColor);

            var currencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
            var currencyFormat = $"#,##0.00 \"{currencyCode}\"";

            int row = 1;
            var isSingleShowReport = model.GroupedByDate.Count == 1 && model.GroupedByDate.First().TimeSlotsData.Count == 1;

            if (isSingleShowReport == false)
            {
                var summaryStartRow = row;
                var headerStartColumn = 2;

                var summaryTitleCell = worksheet.Cells[summaryStartRow, 1, summaryStartRow + 1, 1];
                summaryTitleCell.Value = model.GroupedByDate.Count > 1
                    ? await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.GrandTotal")
                    : await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.Summary");
                summaryTitleCell.Merge = true;
                summaryTitleCell.Style.Font.Bold = true;
                summaryTitleCell.Style.Font.Color.SetColor(fontColor);
                summaryTitleCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                summaryTitleCell.Style.Fill.BackgroundColor.SetColor(borderColor);
                summaryTitleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                summaryTitleCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                summaryTitleCell.Style.WrapText = true;
                SetBorders(summaryTitleCell, borderColor);


                string[] summaryHeaders =
                {
                    "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TotalCameraManPhotoCount",
                    "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TotalCustomerMobilePhotoCount",
                    "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TotalTickets",
                    "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TotalPrice"
                };

                var summaryHeaderRange = worksheet.Cells[summaryStartRow, headerStartColumn, summaryStartRow,
                    headerStartColumn + summaryHeaders.Length - 1];
                for (int i = 0; i < summaryHeaders.Length; i++)
                {
                    var cell = worksheet.Cells[summaryStartRow, headerStartColumn + i];
                    cell.Value = await _localizationService.GetResourceAsync(summaryHeaders[i]);
                }

                summaryHeaderRange.Style.Font.Bold = true;
                summaryHeaderRange.Style.Font.Color.SetColor(fontColor);
                summaryHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                summaryHeaderRange.Style.Fill.BackgroundColor.SetColor(borderColor);
                summaryHeaderRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                SetBorders(summaryHeaderRange, borderColor);


                var summaryValuesRow = summaryStartRow + 1;
                worksheet.Cells[summaryValuesRow, headerStartColumn].Value = model.TotalCameraManPhotoCount;
                worksheet.Cells[summaryValuesRow, headerStartColumn + 1].Value = model.TotalCustomerMobilePhotoCount;
                worksheet.Cells[summaryValuesRow, headerStartColumn + 2].Value = model.TotalPhotoCount;
                worksheet.Cells[summaryValuesRow, headerStartColumn + 3].Value = model.TotalPhotoPrice;
                worksheet.Cells[summaryValuesRow, headerStartColumn + 3].Style.Numberformat.Format = currencyFormat;

                var summaryValuesRange = worksheet.Cells[summaryValuesRow, headerStartColumn, summaryValuesRow,
                    headerStartColumn + summaryHeaders.Length - 1];
                summaryValuesRange.Style.Font.Color.SetColor(fontColor);
                SetBorders(summaryValuesRange, borderColor);

                row = summaryStartRow + 3;
            }


            foreach (var dateGroup in model.GroupedByDate)
            {
                await PrepareProductionInfoCellsAsync(worksheet, row, borderColor, isSingleShowReport, dateGroup.EventName, dateGroup.EventDate,
                    dateGroup.TimeSlotsData.First().EventTime);
                row += 3;

                await PrepareReportHeadersAsync(worksheet, row, borderColor, showActors, isSingleShowReport);
                row++;

                foreach (var timeSlot in dateGroup.TimeSlotsData)
                {
                    row = await PrepareReportDataCellsAsync(worksheet, row, borderColor, currencyFormat, timeSlot, showActors, isSingleShowReport,
                        model.GeneralPhotoPrice, model.GeneralActorShare);
                }


                row += 2;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


            return await Task.FromResult(package.GetAsByteArray());
        }

        public async Task<byte[]> ExportSupervisorReportAsync(SupervisorReportModel model, bool showActors)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(
                await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.Title"));


            var fontColor = ColorTranslator.FromHtml("#0066b3");
            var borderColor = ColorTranslator.FromHtml("#b0d235");
            worksheet.Cells.Style.Font.Size = 12;
            worksheet.Cells.Style.Font.Color.SetColor(fontColor);

            var currencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
            var currencyFormat = $"#,##0.00 \"{currencyCode}\"";

            int row = 1;
            var isSingleShowReport = model.GroupedByDate.Count == 1 && model.GroupedByDate.First().TimeSlotsData.Count == 1;


            if (isSingleShowReport == false)
            {
                var summaryStartRow = row;
                var headerStartColumn = 2;

                var summaryTitleCell = worksheet.Cells[summaryStartRow, 1, summaryStartRow + 1, 1];
                summaryTitleCell.Value = model.GroupedByDate.Count > 1
                    ? await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.GrandTotal")
                    : await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.Summary");
                summaryTitleCell.Merge = true;
                summaryTitleCell.Style.Font.Bold = true;
                summaryTitleCell.Style.Font.Color.SetColor(fontColor);
                summaryTitleCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                summaryTitleCell.Style.Fill.BackgroundColor.SetColor(borderColor);
                summaryTitleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                summaryTitleCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                summaryTitleCell.Style.WrapText = true;
                SetBorders(summaryTitleCell, borderColor);


                string[] summaryHeaders =
                {
                    "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.TotalCameraManPhotoCount",
                    "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.TotalCustomerMobilePhotoCount",
                    "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.TotalTickets",
                    "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.TotalPrice"
                };

                var summaryHeaderRange = worksheet.Cells[summaryStartRow, headerStartColumn, summaryStartRow,
                    headerStartColumn + summaryHeaders.Length - 1];
                for (int i = 0; i < summaryHeaders.Length; i++)
                {
                    var cell = worksheet.Cells[summaryStartRow, headerStartColumn + i];
                    cell.Value = await _localizationService.GetResourceAsync(summaryHeaders[i]);
                }

                summaryHeaderRange.Style.Font.Bold = true;
                summaryHeaderRange.Style.Font.Color.SetColor(fontColor);
                summaryHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                summaryHeaderRange.Style.Fill.BackgroundColor.SetColor(borderColor);
                summaryHeaderRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                SetBorders(summaryHeaderRange, borderColor);


                var summaryValuesRow = summaryStartRow + 1;
                worksheet.Cells[summaryValuesRow, headerStartColumn].Value = model.TotalCameraManPhotoCount;
                worksheet.Cells[summaryValuesRow, headerStartColumn + 1].Value = model.TotalCustomerMobilePhotoCount;

                worksheet.Cells[summaryValuesRow, headerStartColumn + 2].Value = model.TotalPhotoCount;
                worksheet.Cells[summaryValuesRow, headerStartColumn + 3].Value = model.TotalPhotoPrice;
                worksheet.Cells[summaryValuesRow, headerStartColumn + 3].Style.Numberformat.Format = currencyFormat;

                var summaryValuesRange = worksheet.Cells[summaryValuesRow, headerStartColumn, summaryValuesRow,
                    headerStartColumn + summaryHeaders.Length - 1];
                summaryValuesRange.Style.Font.Color.SetColor(fontColor);
                SetBorders(summaryValuesRange, borderColor);

                row = summaryStartRow + 3;
            }


            foreach (var dateGroup in model.GroupedByDate)
            {
                await PrepareSupervisorInfoCellsAsync(worksheet, row, borderColor, isSingleShowReport, dateGroup.EventName, dateGroup.EventDate,
                    dateGroup.TimeSlotsData.First().EventTime, model.ReportType);
                row += 3;

                await PrepareReportHeadersAsync(worksheet, row, borderColor, showActors, isSingleShowReport);
                row++;

                foreach (var timeSlot in dateGroup.TimeSlotsData)
                {
                    row = await PrepareReportDataCellsAsync(worksheet, row, borderColor, currencyFormat, timeSlot, showActors, isSingleShowReport,
                        model.GeneralPhotoPrice, model.GeneralActorShare);
                }


                row += 2;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


            return await Task.FromResult(package.GetAsByteArray());
        }
    }
}