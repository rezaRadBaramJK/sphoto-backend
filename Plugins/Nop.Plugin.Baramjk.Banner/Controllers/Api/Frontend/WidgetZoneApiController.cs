using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Data;
using Nop.Plugin.Baramjk.Banner.Domain.AnywhereSliders;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;

namespace Nop.Plugin.Baramjk.Banner.Controllers.Api.Frontend
{
    public class WidgetZoneApiController : BaseBaramjkApiController
    {
        private readonly IRepository<EntityWidgetMapping> _entityWidgetMappingRepository;

        public WidgetZoneApiController(IRepository<EntityWidgetMapping> entityWidgetMappingRepository)
        {
            _entityWidgetMappingRepository = entityWidgetMappingRepository;
        }

        [HttpGet("/api-frontend/WidgetZone/list")]
        [HttpGet("/FrontendApi/WidgetZone/list")]
        public Task<IActionResult> WidgetZones()
        {
            var widgetZones =
                "home_page_main_slider,content_before,content_after,main_column_before,main_column_after,left_side_column_before,left_side_column_after_category_navigation,left_side_column_after,home_page_before_best_sellers,home_page_before_categories,home_page_before_news,home_page_before_poll,home_page_before_products,home_page_top,home_page_bottom,categorydetails_top,categorydetails_after_breadcrumb,categorydetails_before_subcategories,categorydetails_before_featured_products,categorydetails_after_featured_products,categorydetails_before_filters,categorydetails_before_product_list,categorydetails_bottom,productdetails_top,productdetails_bottom,productdetails_before_pictures,productdetails_after_pictures,productdetails_overview_top,productdetails_overview_bottom,manufacturerdetails_top,mega_menu_categories_before_dropdown_end,mega_menu_categories_before_dropdown_end_1,mega_menu_categories_before_dropdown_end_2,mega_menu_categories_before_dropdown_end_3,mega_menu_categories_before_dropdown_end_4,mega_menu_categories_before_dropdown_end_5,mega_menu_categories_before_dropdown_end_6,mega_menu_categories_before_dropdown_end_7,mega_menu_categories_before_dropdown_end_8"
                    .Split(",");

            return Task.FromResult<IActionResult>(ApiResponseFactory.Success(widgetZones));
        }

        [HttpGet("/api-frontend/Slider/WidgetZone/list")]
        [HttpGet("/FrontendApi/Slider/WidgetZone/list")]
        public async Task<IActionResult> SliderWidgetZones(int id)
        {
            var entityWidgetMappings = await _entityWidgetMappingRepository.Table
                .Where(item => item.EntityType == 15)
                .Where(item => item.EntityId == id)
                .ToListAsync();

            return ApiResponseFactory.Success(entityWidgetMappings);
        }

        [HttpDelete("/api-frontend/Slider/WidgetZone")]
        [HttpDelete("/FrontendApi/Slider/WidgetZone")]
        public async Task<IActionResult> DeleteSliderWidgetZones(int id)
        {
            var widgetMapping = _entityWidgetMappingRepository.Table.FirstOrDefault(item => item.Id == id);
            await _entityWidgetMappingRepository.DeleteAsync(widgetMapping);

            return ApiResponseFactory.Success(widgetMapping);
        }

        [HttpPost("/api-frontend/Slider/WidgetZone")]
        [HttpPost("/FrontendApi/Slider/WidgetZone")]
        public async Task<IActionResult> AddSliderWidgetZones([FromBody] EntityWidgetMapping mapping)
        {
            await _entityWidgetMappingRepository.InsertAsync(mapping);
            return ApiResponseFactory.Success(mapping);
        }
    }
}

