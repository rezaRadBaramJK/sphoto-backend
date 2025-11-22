using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.ExecutePayment;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.GetPaymentStatus;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.SendPayment;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Suppliers;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Services.Networks;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients
{
    public class MyFatoorahPaymentClient : IMyFatoorahPaymentClient, IGatewayClient
    {
        private readonly string _baseUrl;
        private readonly HttpClientHelper _httpClientHelper;
        private readonly ILogger _logger;

        public MyFatoorahPaymentClient(HttpClientHelper httpClientHelper, MyFatoorahSettings myFatoorahSettings,
            ILogger logger)
        {
            _httpClientHelper = httpClientHelper;
            _logger = logger;
            if (string.IsNullOrEmpty(myFatoorahSettings.MyFatoorahAccessKey) == false)
                _httpClientHelper.AddHeader("authorization", "Bearer " + myFatoorahSettings.MyFatoorahAccessKey);

            _baseUrl = myFatoorahSettings.MyFatoorahUseSandbox
                ? "https://apitest.myfatoorah.com/v2/"
                : "https://api.myfatoorah.com/v2/";
        }
        
        public string GatewayName => "MyFatoorahPaymentClient";
        public string ErrorCallBackUrl => DefaultValue.ERROR_URL;
        public string SuccessCallBackUrl => DefaultValue.CALL_BACK_URL;

        public async Task<HttpResponse<MyFatoorahResponse<SendPaymentDataResponse>>> SendPaymentAsync(
            SendPaymentRequest request)
        {
            var url = $"{_baseUrl}SendPayment";
            var response = await _httpClientHelper
                .PostJsonAsync<SendPaymentRequest, MyFatoorahResponse<SendPaymentDataResponse>>(url, request);

            if (response.IsSuccessStatusCode == false)
            {
                await _logger.ErrorAsync($"My Fatoorah({nameof(MyFatoorahPaymentClient)})- SendPayment method: The request body: {request.ToJson()}");
            }
            
            return response;
        }

        public async Task<MyFatoorahResponse<ExecutePaymentDataResponse>> ExecutePaymentAsync(ExecutePaymentRequest request)
        {
            var url = $"{_baseUrl}ExecutePayment";
            var response = await _httpClientHelper
                .PostJsonAsync<ExecutePaymentRequest, MyFatoorahResponse<ExecutePaymentDataResponse>>(url, request);

            if (response.Body == null)
                return new MyFatoorahResponse<ExecutePaymentDataResponse>
                {
                    Message = response.StatusCode.ToString()
                };

            return response.Body;
        }
        
        public async Task<MyFatoorahResponse<GetPaymentStatusResponse>> GetPaymentStatusAsync(
            GetPaymentStatusRequest request)
        {
            var url = $"{_baseUrl}GetPaymentStatus";
            var response = await _httpClientHelper
                .PostJsonAsync<GetPaymentStatusRequest, MyFatoorahResponse<GetPaymentStatusResponse>>(url, request);

            if (response.Body == null)
                return new MyFatoorahResponse<GetPaymentStatusResponse>
                {
                    Message = response.StatusCode.ToString()
                };

            return response.Body;
        }
        
        public async Task<List<MyFatoorahPaymentMethods>> GetPaymentMethodsAsync(
            InitiatePaymentRequest request)
        {
            var url = $"{_baseUrl}InitiatePayment";
            var response = await _httpClientHelper
                // .PostJsonAsync<InitiatePayment>()
                .PostJsonAsync<InitiatePaymentRequest, MyFatoorahResponse<MyFatoorahPaymentMethodsDto>>(url, request);
            
            if (response.Body == null)
            {
                await _logger.ErrorAsync(JsonConvert.SerializeObject(response.Body));
                return new List<MyFatoorahPaymentMethods>();
            }

            return response.Body.Data.PaymentMethods;
        }
        
        public async Task<HttpResponse<CreateInvoiceResponse>> CreateInvoiceAsync(IInvoiceRequest request)
        {
            var customerName = request.GetFullName();
            if (string.IsNullOrEmpty(customerName) || string.IsNullOrWhiteSpace(customerName))
            {
                return new HttpResponse<CreateInvoiceResponse>
                {
                    Body = new CreateInvoiceResponse
                    {
                        Message = "Invalid customer name.",
                    },
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Invalid customer name.",

                };
            }
            var sendPaymentRequest = new SendPaymentRequest
            {
                CustomerName = customerName,
                NotificationOption = request.NotificationOption.ValueOrDefault("ALL"),
                CustomerMobile = request.PhoneNumber.ReplaceSafe("+", "").Replace(" ", ""),
                CustomerEmail = request.Email,
                InvoiceValue = request.Amount,
                CallBackUrl = request.SuccessCallBackUrl,
                ErrorUrl = request.ErrorUrl,
                UserDefinedField = request.ClientReferenceId,
                CustomerReference = request.ClientReferenceId,
                DisplayCurrencyIso = request.CurrencyIsoCode.ValueOrDefault("KWD"),
                ExpiryDate = DateTime.Now.AddHours(1)
            };
            
            var result = await SendPaymentAsync(sendPaymentRequest);
            if (result?.Body == null)
                return new HttpResponse<CreateInvoiceResponse>
                {
                    StatusCode = HttpStatusCode.Gone,
                    ReasonPhrase = "Response is null",
                };
            
            if (result.Body?.IsSuccess == false)
            {
                if (result.Body?.Data == null)
                    return new HttpResponse<CreateInvoiceResponse>
                    {
                        StatusCode = result.StatusCode,
                        ReasonPhrase = result.ReasonPhrase,
                        Body = new CreateInvoiceResponse
                        {
                            Message = result.Body?.GetFullMessage($"No Message")
                        }
                    };

                return new HttpResponse<CreateInvoiceResponse>
                {
                    StatusCode = result.StatusCode,
                    ReasonPhrase = result.ReasonPhrase,
                    Body = new CreateInvoiceResponse
                    {
                        Message = result.Body.GetFullMessage(result.ReasonPhrase)
                    }
                };
            }

            var sendPaymentResponse = result.Body?.Data;
            if (sendPaymentResponse == null)
                return new HttpResponse<CreateInvoiceResponse>
                {
                    StatusCode = result.StatusCode,
                    ReasonPhrase = result.ReasonPhrase,
                    Body = new CreateInvoiceResponse
                    {
                        Message = result.Body?.GetFullMessage("Send payment data is null")
                    }
                };

            var createInvoiceResponse = new CreateInvoiceResponse
            {
                IsSuccess = result.Body?.IsSuccess ?? true,
                InvoiceId = sendPaymentResponse.InvoiceId.ToString(),
                PaymentUrl = sendPaymentResponse.Url,
                ClientReferenceId = sendPaymentResponse.CustomerReference,
                Message = result.Body?.GetFullMessage(result.ReasonPhrase),
            };

            return new HttpResponse<CreateInvoiceResponse>
            {
                StatusCode = result.StatusCode,
                ReasonPhrase = result.ReasonPhrase,
                Body = createInvoiceResponse
            };
        }

        public async Task<HttpResponse<InvoiceStatusResponse>> GetInvoiceStatusAsync(GetInvoiceStatusRequest request)
        {
            var paymentRequest = request as GetPaymentStatusRequest ?? new GetPaymentStatusRequest
            {
                InvoiceId = request.InvoiceId,
                KeyType = "InvoiceId"
            };
            
            var result = await GetPaymentStatusAsync(paymentRequest);
            if (result?.Data == null)
            {
                return new HttpResponse<InvoiceStatusResponse>
                {
                    StatusCode = HttpStatusCode.Gone,
                    ReasonPhrase = "Get payment status was not success. data is null",
                };
            }

            var createInvoiceResponse = new InvoiceStatusResponse
            {
                InvoiceId = result.Data.InvoiceId.ToString(),
                InvoiceStatus = result.Data.InvoiceStatus,
                InvoiceReference = result.Data.InvoiceReference,
                ClientReferenceId = result.Data.CustomerReference,
                CreatedDate = result.Data.CreatedDate,
                Amount = result.Data.InvoiceValue,
                Message = result.GetFullMessage(""),
                IsPaid = result.Data.IsPaid,
                PaymentStatus = result.Data.PaymentStatus,
                IsSuccess = result.IsSuccess,
            };

            return new HttpResponse<InvoiceStatusResponse>
            {
                StatusCode = result.StatusCode,
                ReasonPhrase = result.GetFullMessage(""),
                Body = createInvoiceResponse
            };
        }

        public async Task<HttpResponse<SupplierResponse[]>> GetSuppliersAsync()
        {
            var url = $"{_baseUrl}GetSuppliers";
            var response = await _httpClientHelper
                .GetAsync<SupplierResponse[]>(url);

            return response;


        }
    }
}