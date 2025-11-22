using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.LocationDetector.Domain;
using Nop.Plugin.Baramjk.LocationDetector.Models;
using Nop.Plugin.Baramjk.LocationDetector.Plugin;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.LocationDetector.Controllers
{
    [AuthorizeAdmin]
    [Permission(PermissionProvider.LocationDetectorName)]
    [Area(AreaNames.Admin)]
    public class LocationDetectorAdminController : BaseBaramjkPluginController
    {
        private readonly IRepository<CountryCurrencyMapping> _repository;

        public LocationDetectorAdminController(IRepository<CountryCurrencyMapping> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            return View("LocationDetector/List.cshtml", new CountryCurrencyMappingSearchModel());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View("LocationDetector/Create.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Store([FromForm] CountryCurrencyMappingModel form)
        {
            await _repository.InsertAsync(new CountryCurrencyMapping
            {
                IsoCountryCode = form.IsoCountryCode,
                IsoCurrencyCode = form.IsoCountryCode
            });
            return RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> GetCountryCurrencyMappings(CountryCurrencyMappingSearchModel searchModel)
        {
            try
            {
                var results =  _repository.GetAll();
                var dto = new List<CountryCurrencyMappingModel>();
                foreach (var row in results)
                {
                    var item = new CountryCurrencyMappingModel
                    {
                        Id = row.Id,
                        IsoCountryCode = row.IsoCountryCode,
                        IsoCurrencyCode = row.IsoCurrencyCode,
                    };
                   
                    dto.Add(item);
                }

                var r = new CountryCurrencyMappingListModel
                {
                    Data = dto,
                    Draw = searchModel.Draw,
                    RecordsTotal = dto.Count,
                    RecordsFiltered = dto.Count
                };
                return Json(r);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.Message);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, [FromForm] CountryCurrencyMappingModel form)
        {
            var row = await _repository.Table.Where(x => x.Id == id).FirstOrDefaultAsync();
            row.IsoCountryCode = form.IsoCountryCode;
            row.IsoCurrencyCode = form.IsoCurrencyCode;
            

            await _repository.UpdateAsync(row);

            return RedirectToAction("List");
        }


        public async Task<IActionResult> Edit(int id)
        {
            ViewData["id"] = id;
            var item = await _repository.Table.Where(x => x.Id == id).FirstOrDefaultAsync();
            var model = new CountryCurrencyMappingModel
            {
                IsoCountryCode = item.IsoCountryCode,
                IsoCurrencyCode = item.IsoCurrencyCode,
                Id = item.Id
            };
            return View("LocationDetector/Edit.cshtml", model);
        }
    }
}