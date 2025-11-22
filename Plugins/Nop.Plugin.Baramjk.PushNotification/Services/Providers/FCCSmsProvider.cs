using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification;
using Nop.Plugin.Baramjk.PushNotification.Exceptions;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.PushNotification.Services.Providers
{
    /// <summary>
    /// Future Communications Company SMS Provider
    /// </summary>
    public class FCCSmsProvider : ISmsProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private FCCSmsProviderSetting _smsProviderSetting;
        private readonly ISettingService _settingService;

        public FCCSmsProvider(IHttpClientFactory httpClientFactory, ILogger logger, ISettingService settingService)
        {
            _logger = logger;
            _settingService = settingService;
            _httpClient = httpClientFactory.CreateClient("FCCSmsProvider");
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
                { "IID", _smsProviderSetting.AccountId.ToString() }, //! this parameter must be as a number
                { "UID", _smsProviderSetting.UserName },
                { "S", _smsProviderSetting.Source },
                { "G", phoneNumber },
                { "M", message },
                //{"L","Language Type"}, language type is optional
                /*Possible Values:
                1)  A for Arabic message
                2)  L for English message
                3)  H for  Unicode  message
                Default is L (English Message) */
                //{ "T", "Transaction ID" }, Transaction id is optional
            };

            var uri = string.IsNullOrEmpty(_smsProviderSetting.Url) ? "https://api.future-club.com/falconapi/fccsms.aspx" : _smsProviderSetting.Url;
            var fullUrl = $"{uri}?{BuildQueryString(queryParams)}";

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-API-KEY", _smsProviderSetting.Password);

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
            _smsProviderSetting = (await _settingService.LoadSettingAsync<SmsProviderSetting>()).ToRawFCCSmsSetting(mode);
            await _logger.InformationAsync($"setting setting :{JsonConvert.SerializeObject(_smsProviderSetting)}");
            _httpClient.BaseAddress = new Uri(string.IsNullOrEmpty(_smsProviderSetting.Url)
                ? "https://api.futureclub.com/falconapi/fccsms.aspx"
                : _smsProviderSetting.Url);
        }

        private static string BuildQueryString(Dictionary<string, string> queryParams)
        {
            var queryParts = new List<string>();
            foreach (var kvp in queryParams)
            {
                if (kvp.Key == "IID") //! this parameter must be as a number
                    queryParts.Add($"{Uri.EscapeDataString(kvp.Key)}={int.Parse(kvp.Value)}");
                else
                    queryParts.Add($"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");
            }

            return string.Join("&", queryParts);
        }

        private string GetMessageOfStatusCode(string code)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>()
            {
                { "00", "SMS successfully queued for sending" },
                { "01", "Mobile number is mandatory" },
                { "02", "Invalid mobile number exists" },
                { "03", "Message is mandatory" },
                { "04", "Invalid mobile number(s) (should be 11 digits including 965)" },

                { "11", "Invalid uid" },
                { "12", "Insufficient credit" },
                { "13", "Invalid message" },
                { "14", "No recipients" },
                { "15", "Long Arabic message through international \ngateway  16 - SMS schedule time not provided" },

                { "21", "Long Arabic message through international gateway" },
                { "23", "Cannot connect to host. Try again later" },
                { "29", "Server Error. HTTP Request failed" },

                { "30", "Unauthorized access" },
                { "31", "Invalid date" },
                { "32", "Invalid absolute date" },
                { "33", "Invalid relative lifetime value" },
                { "34", "Error in processing, TrackID not generated" },
                { "35", "Account deactivated. Contact administrator" },
                { "36", "Account  disabled.  Contact  administrator" },
                { "37", "Invalid iid and uid combination" },
                { "38", "Threshold limit reached. Contact administrator" },

                { "41", "Short code (S) is mandatory" },
                { "42", "UID is mandatory" },
                { "43", "IID is mandatory" },
                { "44", "Invalid Short code (S)" },
                { "45", "Message (M) is not a valid Unicode text" },
            };

            return messages.FirstOrDefault(x => x.Key == code).Value;
        }
    }
}