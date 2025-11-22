using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification;
using Nop.Plugin.Baramjk.Framework.Services.Sms;
using Nop.Plugin.Baramjk.OrderPaymentLink.Controllers;
using Nop.Plugin.Baramjk.OrderPaymentLink.Services.Model;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using OrderPaymentStatus = Nop.Core.Domain.Payments.PaymentStatus;

namespace Nop.Plugin.Baramjk.OrderPaymentLink.Services
{
    public class InvoiceService
    {
        public const string ConsumerName = "OrderPaymentLink";

        private readonly IAddressService _addressService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPdfService _pdfService;
        private readonly IRepository<GatewayPaymentTranslation> _repositoryMyFatoorahPaymentTranslation;
        private readonly IRepository<Order> _repositoryOrder;
        private readonly ITranslationService _translationService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IGatewayClientProvider _gatewayClientProvider;
        private readonly IGatewayService _gatewayService;
        private readonly ICustomerDtoFactory _customerDtoFactory;
        private readonly ILogger _logger;
        private readonly OrderPaymentLinkSetting _orderPaymentLinkSetting;
        private readonly ISettingService _settingService;
        private readonly ISmsService _smsService;
        private readonly ISmsProvider _smsProvider;

        public InvoiceService(IAddressService addressService, IHttpContextAccessor httpContextAccessor,
            IMessageTemplateService messageTemplateService, IOrderProcessingService orderProcessingService,
            IOrderService orderService, IPdfService pdfService,
            IRepository<GatewayPaymentTranslation> repositoryMyFatoorahPaymentTranslation,
            IRepository<Order> repositoryOrder, ITranslationService translationService,
            IWorkflowMessageService workflowMessageService, IGatewayClientProvider gatewayClientProvider,
            IGatewayService gatewayService, ICustomerDtoFactory customerDtoFactory, ILogger logger, OrderPaymentLinkSetting orderPaymentLinkSetting,
            ISettingService settingService, ISmsService smsService, ISmsProvider smsProvider)
        {
            _addressService = addressService;
            _httpContextAccessor = httpContextAccessor;
            _messageTemplateService = messageTemplateService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _pdfService = pdfService;
            _repositoryMyFatoorahPaymentTranslation = repositoryMyFatoorahPaymentTranslation;
            _repositoryOrder = repositoryOrder;
            _translationService = translationService;
            _workflowMessageService = workflowMessageService;
            _gatewayClientProvider = gatewayClientProvider;
            _gatewayService = gatewayService;
            _customerDtoFactory = customerDtoFactory;
            _logger = logger;
            _orderPaymentLinkSetting = orderPaymentLinkSetting;
            _settingService = settingService;
            _smsService = smsService;
            _smsProvider = smsProvider;
        }

        public async Task<string> CreateInvoice(int orderId, bool sendEmail = true, bool sendSms = true)
        {
            var translation = await CreatePayment(orderId, sendEmail, sendSms);
            return translation.PaymentUrl;
        }

        public async Task<GatewayPaymentTranslation> CreatePayment(int orderId, bool sendEmail = true, bool sendSms = true)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            var customerId = order.CustomerId;

            var translation = await _translationService.NewTranslationAsync(new CreateTranslationRequest
            {
                AmountToPay = order.OrderTotal,
                CustomerId = customerId,
                ConsumerName = ConsumerName,
                ConsumerEntityType = "Order",
                ConsumerEntityId = orderId,
                ConsumerCallBack = "/OrderPaymentLink/CallBack?guid={0}",
                ConsumerData = ""
            });

            var info = await _customerDtoFactory.PrepareCustomerDtoAsync(order.CustomerId, order.BillingAddressId);
            
            var customerInfoWithOutAddressDetails =
                await _customerDtoFactory.PrepareCustomerDtoAsync(order.CustomerId);
            
            // if (string.IsNullOrEmpty(info.Email))
            // {
            //     info = customerInfoWithOutAddressDetails;
            //     info.Email = customerInfoWithOutAddressDetails.Email;
            // }

            if (string.IsNullOrEmpty(info.Email) &&
                string.IsNullOrEmpty(customerInfoWithOutAddressDetails.Email) == false)
                info.Email = customerInfoWithOutAddressDetails.Email;
            
            if (string.IsNullOrEmpty(info.FirstName) && string.IsNullOrEmpty(info.LastName))
            {
                info.FirstName = customerInfoWithOutAddressDetails.FirstName;
                info.LastName = customerInfoWithOutAddressDetails.LastName;
            }

            if (string.IsNullOrEmpty(info.Phone))
            {
                info.Phone = customerInfoWithOutAddressDetails.Phone;
            }

            var paymentRequest = new InvoiceRequest
            {
                FirstName = info.FirstName,
                LastName = info.LastName,
                PhoneNumber = info.Phone,
                Email = info.Email,
                Amount = order.OrderTotal,
            };
            
            if (string.IsNullOrEmpty(paymentRequest.GetFullName()) || string.IsNullOrWhiteSpace(paymentRequest.GetFullName()))
                paymentRequest.FullName = ConsumerName;

            if (string.IsNullOrEmpty(paymentRequest.Email) || string.IsNullOrWhiteSpace(paymentRequest.Email))
                paymentRequest.Email = "payment@baramjk.com";
            
            var gatewayClient = _gatewayClientProvider.GetDefaultGatewayClient();
            try
            {
                await _gatewayService.SendInvoiceAsync(gatewayClient, translation, paymentRequest);
                
            }
            catch (BadRequestBusinessException e)
            {
                translation.Message = e.Message;
                await _translationService.UpdateAsync(translation);
                return translation;
            }
            
            await AddOrderNote(order, translation.PaymentUrl);

            if (sendEmail)
                await SendInvoiceByEmail(order, translation.PaymentUrl, info.Email);

            if (sendSms)
            {
                await SendInvoiceBySmsAsync(order, info.Phone);
            }
            
            return translation;
        }

        public async Task<GatewayPaymentTranslation> CallBack(string guid)
        {
            var translation = await _translationService.GetByGuidAsync(guid);
            if (translation == null)
            {
                await _logger.ErrorAsync($"Transaction with guid {guid} not found");
            }

            if (translation.Status == GatewayPaymentStatus.Paid)
            {
                await _logger.InformationAsync($"Transaction with guid {guid} already paid");
                return translation;
            }

            var order = await _orderService.GetOrderByIdAsync(translation.ConsumerEntityId);
            if (_orderProcessingService.CanMarkOrderAsPaid(order))
                await _orderProcessingService.MarkOrderAsPaidAsync(order);
            else
            {
                await _logger.ErrorAsync($"Order with id {translation.ConsumerEntityId} can't be marked as paid");
            }

            await _translationService.SetConsumerStatusAsync(guid, ConsumerStatus.Success);

            return translation;
        }

        public async Task<OrderPaymentStatus> GetOrderPaymentStatus(int orderId)
        {
            var status = (await _orderService.GetOrderByIdAsync(orderId)).PaymentStatus;
            return status;
        }

        public async Task<List<GatewayPaymentTranslation>> GetEntityTranslationsAsync(int orderId)
        {
            var translation = await _translationService.GetConsumerTranslationsAsync(
                ConsumerName, consumerEntityId: orderId);

            return translation;
        }

        public async Task<List<PaymentLinkItem>> GetPaymentLinkItemsAsync(int orderId)
        {
            var translation = await _translationService.GetConsumerTranslationsAsync(
                ConsumerName, consumerEntityId: orderId);

            var paymentItems = translation.Select(item => new PaymentLinkItem
            {
                Guild = item.Guid,
                PaymentUrl = item.PaymentUrl,
                PaymentId = item.PaymentId,
                InvoiceId = item.InvoiceId,
                AmountToPay = item.AmountToPay,
                AmountPayed = item.AmountPayed,
                Message = item.Message,
                CustomerId = item.OwnerCustomerId,
                OrderId = item.ConsumerEntityId,
                ConsumerStatus = item.ConsumerStatus,
                Status = item.Status,
                OnCreateDateTimeUtc = item.OnCreateDateTimeUtc
            }).ToList();

            return paymentItems;
        }

        public async Task<bool> IsSendPaymentLinkAsync(int orderId)
        {
            var translation = await _translationService.GetConsumerTranslationsAsync(
                ConsumerName, consumerEntityId: orderId);

            return translation.Any();
        }

        public async Task<List<ReportItem>> GetReportAsync(ReportRequest model)
        {
            var translations = await _repositoryMyFatoorahPaymentTranslation.Table.ToListAsync();
            var query = from translation in translations
                join order in _repositoryOrder.Table on translation.ConsumerEntityId equals order.Id
                where
                    translation.ConsumerName == ConsumerName &&
                    (model.From == null || order.CreatedOnUtc.Date >= model.From.Value.Date) &&
                    (model.To == null || order.CreatedOnUtc.Date <= model.To.Value.Date)
                group new { translation, order } by translation.ConsumerEntityId
                into g
                let order = g.First().order
                let translationItems = g.Select(item => item.translation).ToList()
                select new ReportItem
                {
                    OrderId = order.Id,
                    OrderStatus = order.OrderStatus.ToString(),
                    OrderPaymentStatus = order.PaymentStatus.ToString(),
                    OrderTotal = order.OrderTotal,
                    OrderCreatedOnUtc = order.CreatedOnUtc,
                    TransactionCount = translationItems.Count,
                    Transactions = translationItems
                };

            var reportItems = query.ToList();

            return reportItems;
        }

        public List<GetStatusListResponse> GetSentInvoiceState(GetStatusListRequest model)
        {
            var orderId = _repositoryMyFatoorahPaymentTranslation.Table
                .Where(item => model.OrderIds.Contains(item.ConsumerEntityId))
                .Where(item => item.ConsumerName == ConsumerName)
                .Select(item => item.ConsumerEntityId)
                .Distinct()
                .ToHashSet();

            var getStatusListResponses = model.OrderIds
                .Select(item => new GetStatusListResponse
                {
                    OrderId = item,
                    Status = orderId.Contains(item)
                }).ToList();

            return getStatusListResponses;
        }

        public async Task<GetOrderPaymentUrlResponseModel> GetPaymentUrlByPaymentGuid(string guid)
        {
            var result = new GetOrderPaymentUrlResponseModel();
            var translation = await _translationService.GetByGuidAsync(guid);
            if (translation == null)
            {
                result.ErrorMessage = "Transaction not found";
                return result;
            }

            if (translation.Status == GatewayPaymentStatus.Paid)
            {
                result.ErrorMessage = "Transaction already paid";
                return result;
            }

            if (_orderPaymentLinkSetting.LinkExpireAfterMinutes > 0 &&
                translation.OnCreateDateTimeUtc.AddMinutes(_orderPaymentLinkSetting.LinkExpireAfterMinutes) < DateTime.UtcNow)
            {
                result.ErrorMessage = "Payment link expired";
                return result;
            }

            var order = await _orderService.GetOrderByIdAsync(translation.ConsumerEntityId);
            if (!_orderProcessingService.CanMarkOrderAsPaid(order))
            {
                result.ErrorMessage = "Order can not be paid";
                return result;
            }

            result.PaymentUrl = translation.PaymentUrl;
            result.Success = true;
            return result;
        }

        private async Task AddOrderNote(Order order, string paymentUrl)
        {
            var note = $@"
Send payment link.
{paymentUrl}
";
            var orderNote = new OrderNote
            {
                OrderId = order.Id,
                Note = note,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _orderService.InsertOrderNoteAsync(orderNote);
        }

        private async Task SendInvoiceBySmsAsync(Order order, string customerPhoneNumber)
        {
            var settings = await _settingService.LoadSettingAsync<OrderPaymentLinkSetting>();

            if (string.IsNullOrEmpty(customerPhoneNumber))
                return;

            var path = await _pdfService.PrintOrderToPdfAsync(order);
            var fileName = Path.GetFileName(path);
            var url = $"{_httpContextAccessor.HttpContext.Request.Host}/files/exportimport/{fileName}";

            var message = string.Format(url, settings.Message);
            //temp log
            await _logger.InformationAsync(message);
            await _smsService.SendSms(customerPhoneNumber, message, order.CustomerId);
        }

        private async Task SendInvoiceByEmail(Order order, string paymentUrl, string customerEmail)
        {
            var templates = (await _messageTemplateService
                    .GetMessageTemplatesByNameAsync("OrderPaymentInvoiceLink"))
                .FirstOrDefault();

            if (templates == null)
                return;

            if (string.IsNullOrEmpty(customerEmail))
                return;

            var path = await _pdfService.PrintOrderToPdfAsync(order);
            var fileName = Path.GetFileName(path);
            var url = $"{_httpContextAccessor.HttpContext.Request.Host}/files/exportimport/{fileName}";

            var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(templates.Id);
            var tokens = new List<Token>
            {
                new("InvoiceLink", paymentUrl),
                new("InvoicePdfUrl", url)
            };

            await _workflowMessageService.SendTestEmailAsync(messageTemplate.Id, customerEmail, tokens, 1);
        }
    }

    public class PaymentLinkItem
    {
        public string Guild { get; set; }
        public string PaymentUrl { get; set; }
        public string PaymentId { get; set; }
        public string InvoiceId { get; set; }
        public decimal AmountToPay { get; set; }
        public decimal AmountPayed { get; set; }
        public string Message { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public ConsumerStatus ConsumerStatus { get; set; }
        public GatewayPaymentStatus Status { get; set; }
        public string ConsumerStatusTitle => ConsumerStatus.ToString();
        public string StatusTitle => Status.ToString();
        public DateTime OnCreateDateTimeUtc { get; set; }
    }
}