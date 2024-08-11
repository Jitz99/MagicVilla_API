using System.Text;
using MagicVilla_Web.Models;
using MagicVilla_Web.Services.IServices;
using Newtonsoft.Json;

namespace MagicVilla_Web.Services
{
    public class BaseService : IBaseService

    {
        public APIResponse responseModel { get; set; }
        public IHttpClientFactory httpClient { get; set; }
        public BaseService(IHttpClientFactory httpClient)
        {
            this.responseModel = new();
            this.httpClient = httpClient;
        }

        public async Task<T> SendAsync<T>(APIRequest apiRequest)
        {
           try
            {
                var client = httpClient.CreateClient("MagicAPI");
                HttpRequestMessage message = new HttpRequestMessage();
                message.Headers.Add("Accept", "application/json");
                message.RequestUri = new Uri(apiRequest.Url);
                if (apiRequest.Data != null)
                {
                    message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data),
                            Encoding.UTF8, "application/json");
                }
                switch (apiRequest.Apitype)
                {
                    case MagicVilla_Utility.SD.Apitype.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case MagicVilla_Utility.SD.Apitype.GET:
                        message.Method = HttpMethod.Get;
                        break;
                    case MagicVilla_Utility.SD.Apitype.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    case MagicVilla_Utility.SD.Apitype.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                }
                HttpResponseMessage apiResponse = null;

                if (!string.IsNullOrEmpty(apiRequest.Token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",apiRequest.Token);
                }

                apiResponse = await client.SendAsync(message);
                var apiContent = await apiResponse.Content.ReadAsStringAsync();
                try
                {
					APIResponse Apirespons = JsonConvert.DeserializeObject<APIResponse>(apiContent);
                    if (apiResponse!=null && (apiResponse.StatusCode==System.Net.HttpStatusCode.BadRequest||apiResponse.StatusCode==System.Net.HttpStatusCode.NotFound)) {
						Apirespons.StatusCode = System.Net.HttpStatusCode.BadRequest;
						Apirespons.IsSuccess = false;
						var res = JsonConvert.SerializeObject(Apirespons);
                        var returnobj = JsonConvert.DeserializeObject<T>(res);
                        return returnobj;
                    }
				}
				catch (Exception e)
                {
					var exceptionResponse = JsonConvert.DeserializeObject<T>(apiContent);
					return exceptionResponse;

				}
					var APIResponse = JsonConvert.DeserializeObject<T>(apiContent);
					return APIResponse;
			}
			catch (Exception e)
            {
                var dto = new APIResponse
                {
                    ErrorMessages = new List<string> { Convert.ToString(e.Message) },
                    IsSuccess = false
                };
                var res = JsonConvert.SerializeObject(dto);
                var APIResponse = JsonConvert.DeserializeObject<T>(res);
                return APIResponse;
            } 
        }
    }
}
