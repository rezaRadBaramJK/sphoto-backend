using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.CalculateShippingCharge.Request;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.CalculateShippingCharge.Response;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.GetShippingOrderList;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.RequestPickup;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.UpdateShippingStatus;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins;
using Nop.Plugin.Baramjk.Framework.Services.Networks;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients
{
    public class MyFatoorahShippingClient
    {
        private readonly string _baseUrl;
        private readonly HttpClientHelper _httpClientHelper;

        public MyFatoorahShippingClient(HttpClientHelper httpClientHelper, MyFatoorahSettings myFatoorahSettings)
        {
            _httpClientHelper = httpClientHelper;
            if (string.IsNullOrEmpty(myFatoorahSettings.MyFatoorahAccessKey) == false)
                _httpClientHelper.AddHeader("authorization", "Bearer "+myFatoorahSettings.MyFatoorahAccessKey);
            
            _baseUrl = myFatoorahSettings.MyFatoorahUseSandbox
                ? "https://apitest.myfatoorah.com/v2/"
                : "https://api.myfatoorah.com/v2/";
        }

        public async Task<HttpResponse<CalculateShippingChargeResponse>> CalculateShippingChargeAsync
            (CalculateShippingChargeRequest request)
        {
            var url = $"{_baseUrl}CalculateShippingCharge";
            var response = await _httpClientHelper.PostJsonAsync<CalculateShippingChargeRequest
                , CalculateShippingChargeResponse>(url, request);

            return response;
        }

        public async Task<HttpResponse<UpdateShippingStatusResponse>> UpdateShippingStatusAsync(
            UpdateShippingStatusRequest request)
        {
            var url = $"{_baseUrl}UpdateShippingStatus";
            var response = await _httpClientHelper
                .PostJsonAsync<UpdateShippingStatusRequest, UpdateShippingStatusResponse>(url, request);

            return response;
        }

        public async Task<HttpResponse<List<RequestPickupResponse>>> RequestPickupAsync
            (ShippingMethod shippingMethod = ShippingMethod.DHL)
        {
            var url = $"{_baseUrl}RequestPickup?shippingMethod={ShippingMethod.DHL}";
            var response = await _httpClientHelper.GetAsync<List<RequestPickupResponse>>(url);

            return response;
        }

        public async Task<HttpResponse<GetShippingOrderListResponse>> GetShippingOrderListAsync(
            int shippingMethod,
            int orderStatus, int? start, int? length)
        {
            var param = new List<KeyValuePair<string, string>>
            {
                new("shippingMethod", shippingMethod.ToString()),
                new("orderStatus", orderStatus.ToString())
            };

            if (start.HasValue)
                param.Add(new KeyValuePair<string, string>("start", start.ToString()));
            if (length.HasValue)
                param.Add(new KeyValuePair<string, string>("length", length.ToString()));

            var url = $"{_baseUrl}GetShippingOrderList";
            var response =
                await _httpClientHelper.GetAsync<GetShippingOrderListResponse>(url, param.ToArray());

            return response;
        }
    }
}