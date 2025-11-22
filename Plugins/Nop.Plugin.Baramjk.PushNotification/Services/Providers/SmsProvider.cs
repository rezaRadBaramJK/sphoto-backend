using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification;
using Nop.Plugin.Baramjk.PushNotification.Exceptions;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.PushNotification.Services.Providers
{
    public class RmlConnectSmsProvider : ISmsProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private RawSmsProviderSetting _smsProviderSetting;
        private readonly ISettingService _settingService;

        public RmlConnectSmsProvider(IHttpClientFactory httpClientFactory, ILogger logger,
            ISettingService settingService)
        {
            _logger = logger;
            _settingService = settingService;
            _httpClient = httpClientFactory.CreateClient("RmlConnectSmsProvider");
        }

        public async Task SendMessageAsync(string phoneNumber, string message)
        {
            await _logger.InformationAsync($"sms provider phone:{phoneNumber} message:{message}");
            if (!_smsProviderSetting.IsEnabled)
            {
                await _logger.WarningAsync($"sms provider is disabled for mode :{_smsProviderSetting.Mode}");
                return;
            }

            var queryParams = new Dictionary<string, string>
            {
                { "username", _smsProviderSetting.UserName },
                { "password", _smsProviderSetting.Password },
                { "type", "0" },
                { "dlr", "0" },
                { "destination", phoneNumber },
                { "source", _smsProviderSetting.Source },
                { "message", message },
            };

            var uri = "/bulksms/bulksms";
            var fullUrl = $"{uri}?{BuildQueryString(queryParams)}";
            var serverResponse = await _httpClient.PostAsync(fullUrl, new StringContent(""));
            var content = await serverResponse.Content.ReadAsStringAsync();
            if (serverResponse.IsSuccessStatusCode)
            {
                await _logger.InformationAsync(
                    $"OK sending otp sms for number {phoneNumber} and uri {fullUrl} : {content}");
            }
            else
            {
                await _logger.ErrorAsync(
                    $"Error sending otp sms for number {phoneNumber} and uri {fullUrl} : {content}");
                throw new SendSmsException();
            }
        }

        public async Task SetSetting(SmsProviderMode mode)
        {
            _smsProviderSetting = (await _settingService.LoadSettingAsync<SmsProviderSetting>()).ToRaw(mode);
            await _logger.InformationAsync($"setting setting :{JsonConvert.SerializeObject(_smsProviderSetting)}");
            _httpClient.BaseAddress = new Uri(string.IsNullOrEmpty(_smsProviderSetting.Url)
                ? "http://api.rmlconnect.net"
                : _smsProviderSetting.Url);
        }

        private static string BuildQueryString(Dictionary<string, string> queryParams)
        {
            var queryParts = new List<string>();
            foreach (var kvp in queryParams)
            {
                queryParts.Add($"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");
            }

            return string.Join("&", queryParts);
        }
    }
}