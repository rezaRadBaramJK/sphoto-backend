using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification.Models;
using Nop.Plugin.Baramjk.PushNotification.Plugins;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.PushNotification.Services.Providers.WhatsApp
{
    public class RmlWhatsAppProvider : IWhatsAppProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly WhatsAppSettings _whatsAppSettings;
        private readonly ILogger _logger;

        public RmlWhatsAppProvider(
            IHttpClientFactory httpClientFactory,
            WhatsAppSettings whatsAppSettings,
            ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _whatsAppSettings = whatsAppSettings;
            _logger = logger;
        }

        public string ProviderName => "Rml";


        public async Task<WhatAppMessageResult> SendOtpAsync(WhatsAppSendOtpParams sendParams)
        {
            if (string.IsNullOrEmpty(sendParams.PhoneNumber))
                throw new ArgumentNullException(nameof(sendParams.PhoneNumber));

            if (string.IsNullOrEmpty(sendParams.OtpCode))
                throw new ArgumentNullException(sendParams.OtpCode);

            var phoneNumber = sendParams.PhoneNumber;

            if (phoneNumber.StartsWith("+") == false)
                phoneNumber = $"+{phoneNumber}";

            var loginResponse = await LoginAsync();
            if (loginResponse.IsSuccessful == false)
            {
                await _logger.ErrorAsync(
                    $"{nameof(RmlWhatsAppProvider)}: RML service login has been failed. status code {loginResponse.StatusCode}. response {loginResponse.JsonResponse}");
                return WhatAppMessageResult.GetFailedResult("RML service login has been failed.");
            }

            var sendMessageRequestBody = new SendMessageRequestBody
            {
                Phone = phoneNumber,
                Media = new SendMessageRequestBody.MediaBody
                {
                    Type = "media_template",
                    TemplateName = "otp",
                    LangCode = "en",
                    Body = new List<Dictionary<string, string>>
                    {
                        new()
                        {
                            { "text", sendParams.OtpCode }
                        }
                    },
                    Button = new List<Dictionary<string, string>>
                    {
                        new()
                        {
                            { "button_no", "0" },
                            { "url", sendParams.OtpCode }
                        }
                    }
                }
            };

            return await SendMessageAsync(sendMessageRequestBody, loginResponse.Token);
        }
        
        public async Task<WhatAppMessageResult> SendMessageAsync(string phoneNumber,string message)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                throw new ArgumentNullException(nameof(phoneNumber));

            if (phoneNumber.StartsWith("+") == false)
                phoneNumber = $"+{phoneNumber}";

            var loginResponse = await LoginAsync();
            if (loginResponse.IsSuccessful == false)
            {
                await _logger.ErrorAsync(
                    $"{nameof(RmlWhatsAppProvider)}: RML service login has been failed. status code {loginResponse.StatusCode}. response {loginResponse.JsonResponse}");
                return WhatAppMessageResult.GetFailedResult("RML service login has been failed.");
            }

            var sendMessageRequestBody = new SendMessageRequestBody
            {
                Phone = phoneNumber,
                Media = new SendMessageRequestBody.MediaBody
                {
                    Type = "media_template",
                    TemplateName = "message",
                    LangCode = "en",
                    Body = new List<Dictionary<string, string>>
                    {
                        new()
                        {
                            { "text", message }
                        }
                    },
                    Button = new List<Dictionary<string, string>>
                    {
                        new()
                        {
                            { "button_no", "0" },
                            { "url", "" }
                        }
                    }
                }
            };

            return await SendMessageAsync(sendMessageRequestBody, loginResponse.Token);
        }

        public async Task<WhatAppMessageResult> SendStatusHasChangedAsync(string phoneNumber, string eventName, string statusName, string templateName = "")
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                await _logger.ErrorAsync($"{nameof(RmlWhatsAppProvider)}: Invalid phone number.");
                return WhatAppMessageResult.GetFailedResult("Invalid phone number.");
            }

            if (string.IsNullOrEmpty(templateName))
            {
                await _logger.ErrorAsync($"{nameof(RmlWhatsAppProvider)}: Invalid template name.");
                return WhatAppMessageResult.GetFailedResult("Invalid template name.");
            }
            
            if (phoneNumber.StartsWith("+") == false)
                phoneNumber = $"+{phoneNumber}";

            var loginResponse = await LoginAsync();
            if (loginResponse.IsSuccessful == false)
            {
                await _logger.ErrorAsync(
                    $"{nameof(RmlWhatsAppProvider)}: RML service login has been failed. status code {loginResponse.StatusCode}. response {loginResponse.JsonResponse}");
                return WhatAppMessageResult.GetFailedResult("RML service login has been failed.");
            }
            
            var sendMessageRequestBody = new SendMessageRequestBody
            {
                Phone = phoneNumber,
                Media = new SendMessageRequestBody.MediaBody
                {
                    Type = "media_template",
                    TemplateName = templateName,
                    LangCode = "en"
                }
            };

            return await SendMessageAsync(sendMessageRequestBody, loginResponse.Token);
        }
        
        

        private async Task<WhatAppMessageResult> SendMessageAsync(SendMessageRequestBody sendMessageRequestBody,
            string token)
        {
            var sendMessageClient = _httpClientFactory.CreateClient();
            var requestJson = JsonConvert.SerializeObject(sendMessageRequestBody, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            sendMessageClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
            var sendMessageHttpResponse = await sendMessageClient.PostAsync(
                "https://apis.rmlconnect.net/wba/v1/messages",
                content);

            if (sendMessageHttpResponse.IsSuccessStatusCode)
                return WhatAppMessageResult.GetSuccessfulResult();

            var jsonResponse = await sendMessageHttpResponse.Content.ReadAsStringAsync();
            await _logger.ErrorAsync(
                $"{nameof(RmlWhatsAppProvider)}: RML service send message has been failed. status code {sendMessageHttpResponse.StatusCode}. response is : {jsonResponse}");
            return WhatAppMessageResult.GetFailedResult("RML service send message has been failed.");
        }

        private async Task<LoginResponse> LoginAsync()
        {
            var loginClient = _httpClientFactory.CreateClient();
            var loginApiParams = new
            {
                username = _whatsAppSettings.Username,
                password = _whatsAppSettings.Password
            };
            var loginHttpResponse =
                await loginClient.PostAsJsonAsync("https://apis.rmlconnect.net/auth/v1/login/", loginApiParams);

            if (loginHttpResponse.IsSuccessStatusCode == false)
                return new LoginResponse
                {
                    JsonResponse = await loginHttpResponse.Content.ReadAsStringAsync(),
                    StatusCode = loginHttpResponse.StatusCode
                };

            var loginJsonResponse = await loginHttpResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<LoginResponse>(loginJsonResponse);
        }

        private class LoginResponse
        {
            [JsonProperty("JWTAUTH")] public string Token { get; set; }

            public bool IsSuccessful => string.IsNullOrEmpty(Token) == false;

            public string JsonResponse { get; set; }
            
            public HttpStatusCode StatusCode { get; set; }
        }


        [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        private class SendMessageRequestBody
        {
            public string Phone { get; set; }

            public MediaBody Media { get; set; }

            public bool EnableAcculync { get; set; } = true;

            [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
            public class MediaBody
            {
                public string Type { get; set; }

                public string TemplateName { get; set; }

                public string LangCode { get; set; }

                public List<Dictionary<string, string>> Body { get; set; }

                public List<Dictionary<string, string>> Button { get; set; }
            }
        }
    }
}