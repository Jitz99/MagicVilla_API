using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services
{
	public class AuthService : BaseService, IAuthService
	{
		private readonly IHttpClientFactory _clientFactory;
		private string villaUrl;
		public AuthService(IHttpClientFactory clientFactory, IConfiguration configuration) : base(clientFactory)
		{
			_clientFactory = clientFactory;
			villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
		}

		public Task<T> LoginAsync<T>(LoginRequestDTO obj)
		{
			return SendAsync<T>(new APIRequest()
			{
				Apitype = MagicVilla_Utility.SD.Apitype.POST,
				Data = obj,
				Url = villaUrl + "/api/v1/UserAuth/login"
			});
		}

		public Task<T> RegisterAsync<T>(RegistrationRequestDTO obj)
		{
			return SendAsync<T>(new APIRequest()
			{
				Apitype = MagicVilla_Utility.SD.Apitype.POST,
				Data = obj,
				Url = villaUrl + "/api/v1/UserAuth/register"
			});
		}
	}
}
