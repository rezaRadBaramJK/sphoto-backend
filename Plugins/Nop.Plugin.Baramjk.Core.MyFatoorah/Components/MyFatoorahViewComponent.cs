using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Components
{
    [ViewComponent(Name = "PaymentMyFatoorah")]

    public class MyFatoorahViewComponent : NopViewComponent
    {
        private readonly MyFatoorahPaymentClient _myFatoorahHttpClient;
        private readonly INotificationService _notificationService;
        private readonly ILogger _logger;

        public MyFatoorahViewComponent(MyFatoorahPaymentClient myFatoorahHttpClient, INotificationService notificationService, ILogger logger)
        {
            _myFatoorahHttpClient = myFatoorahHttpClient;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(object data)
        {
            var fetchPaymentMethods = await FetchPaymentMethodsAsync();
            return View("~/Plugins/Baramjk.Core.MyFatoorah/Views/PaymentMethod.cshtml", fetchPaymentMethods);
        }
        private async Task<List<MyFatoorahPaymentMethods>> FetchPaymentMethodsAsync()
        {
            try
            {
                var initiatePaymentRequest = new InitiatePaymentRequest
                {
                    CurrencyIso = "KWD",
                    InvoiceAmount = 100
                };
                var methods = await _myFatoorahHttpClient.GetPaymentMethodsAsync(initiatePaymentRequest);
                if (methods == null || !methods.Any())
                {
                    var messageSummary =  "Fetch payment methods was not successful. Retry in a minute";
                    _notificationService.ErrorNotification(messageSummary);
                    await _logger.ErrorAsync(messageSummary);

                    return new List<MyFatoorahPaymentMethods>();
                }

                return methods;
            }
            catch (Exception exception)
            {
                _notificationService.ErrorNotification("Fetch payment methods was not successful. Retry in a minute");
                await _logger.ErrorAsync("FetchPaymentMethods", exception);
                return new List<MyFatoorahPaymentMethods>();
            }
        }
    }
}