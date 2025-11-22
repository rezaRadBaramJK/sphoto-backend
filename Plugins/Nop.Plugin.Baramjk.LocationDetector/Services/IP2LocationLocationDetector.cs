using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Plugin.Baramjk.LocationDetector.Models;
using Nop.Plugin.Baramjk.LocationDetector.Services.Interfaces;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.LocationDetector.Services
{
    public class IP2LocationLocationDetector : ILocationDetector
    {
        private readonly LocationDetectorSettings _settings;
        private readonly ILogger _logger;
        public IP2LocationLocationDetector(LocationDetectorSettings settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<string> GetLocationByIp(string ip)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://api.ip2location.io/?key={_settings.Ip2LocationToken}&ip={ip}");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var resp = JsonConvert.DeserializeObject<Ip2LocationResponseModel>(
                    await response.Content.ReadAsStringAsync());
                return resp.country_code;

            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message);
                throw;
            }

        }
    }
}