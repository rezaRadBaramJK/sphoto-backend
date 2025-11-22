using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.ExecutePayment;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.GetPaymentStatus;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients
{
    
public class MyFatoorahHttpClient
{
	private readonly MyFatoorahSettings _myFatoorahPaymentSettings;

	private HttpClient _httpClient;

	private static readonly object padlock = new object();

	private readonly ILogger _logger;
	public HttpClient httpClient
	{
		get
		{
			if (_httpClient == null)
			{
				lock (padlock)
				{
					if (_httpClient == null)
					{
						_httpClient = new HttpClient();
					}
				}
			}
			return _httpClient;
		}
	}

	public MyFatoorahHttpClient(HttpClient client, MyFatoorahSettings myFatoorahPaymentSettings, ILogger logger)
	{
		_myFatoorahPaymentSettings = myFatoorahPaymentSettings;
		_logger = logger;
		client.Timeout = TimeSpan.FromSeconds(20.0);
		client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "nopCommerce-4.40");
		client.BaseAddress = new Uri(_myFatoorahPaymentSettings.MyFatoorahUseSandbox ? "https://apitest.myfatoorah.com/v2/" : "https://api.myfatoorah.com/v2/");
		if (!string.IsNullOrEmpty(_myFatoorahPaymentSettings.MyFatoorahAccessKey))
		{
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _myFatoorahPaymentSettings.MyFatoorahAccessKey);
		}
		_httpClient = client;
	}
	public enum APICallEnum
	{
		InitiatePayment,
		ExecutePayment,
		SendPayment,
		MakeRefund,
		GetPaymentStatus
	}
	
	public async Task<SendRefundResponse> CreateRefundAsync(SendRefundRequest request)
	{
		string requestJSON = JsonConvert.SerializeObject((object)request);
		string url = APICallEnum.MakeRefund.ToString();
		var requestContent = new StringContent(requestJSON, Encoding.UTF8, "application/json");
		if (ServicePointManager.SecurityProtocol == (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls))
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
		}
		var result = await _httpClient.PostAsync(url, requestContent);
		if (result.IsSuccessStatusCode)
		{
			result.EnsureSuccessStatusCode();
		}
		var response = JsonConvert.DeserializeObject<SendRefundResponse>(await result.Content.ReadAsStringAsync());
		return response ?? new SendRefundResponse
		{
			Message = result.ReasonPhrase
		};
	}
	public async Task<ExecutePaymentDataResponse> CreateInvoiceAsync(ExecutePaymentRequest request)
	{
		string requestJSON = JsonConvert.SerializeObject((object)request);
		string url = APICallEnum.ExecutePayment.ToString();
		var requestContent = new StringContent(requestJSON, Encoding.UTF8, "application/json");
		if (ServicePointManager.SecurityProtocol == (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls))
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
		}
		var result = await _httpClient.PostAsync(url, requestContent);
		if (result.IsSuccessStatusCode)
		{
			result.EnsureSuccessStatusCode();
		}
		var response = JsonConvert.DeserializeObject<ExecutePaymentDataResponse>(await result.Content.ReadAsStringAsync());
		return response;
	}
	
	public async Task<GetPaymentStatusResponse> GetPaymentStatusAsync(string paymentId)
	{
		const string url = nameof(APICallEnum.GetPaymentStatus);
		var request = new
		{
			Key = paymentId,
			KeyType = "PaymentId"
		};
		var requestJSON = JsonConvert.SerializeObject(request);
		var requestContent = new StringContent(requestJSON, Encoding.UTF8, "application/json");
		if (ServicePointManager.SecurityProtocol == (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls))
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
		}
		var result = await _httpClient.PostAsync(url, requestContent);
		if (result.IsSuccessStatusCode)
		{
			result.EnsureSuccessStatusCode();
		}
		var response = JsonConvert.DeserializeObject<GetPaymentStatusResponse>(await result.Content.ReadAsStringAsync());
		return response ?? new GetPaymentStatusResponse
		{
			Message = result.ReasonPhrase
		};
	}
}
}