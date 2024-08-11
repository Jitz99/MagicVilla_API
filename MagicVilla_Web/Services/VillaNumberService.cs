using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authorization;

namespace MagicVilla_Web.Services
{
    public class VillaService : BaseService, IVillaService
    {
        private readonly IHttpClientFactory _clientFactory;
        private string villaUrl;
        public VillaService(IHttpClientFactory clientFactory,IConfiguration configuration):base(clientFactory)
        {
            _clientFactory = clientFactory;
            villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
        }

        [Authorize(Roles ="admin")]
        public Task<T> CreateAsync<T>(VillaCreateDTO dto, string token)
        {
            return SendAsync<T>(new APIRequest()
            {
                Apitype = MagicVilla_Utility.SD.Apitype.POST,
                Data = dto,
                Url = villaUrl + "/api/v1/VillaAPI",
                Token = token
            });
        }

        [Authorize(Roles ="admin")]
        public Task<T> DeleteAsync<T>(int id, string token)
        {
            return SendAsync<T>(new APIRequest()
            {
                Apitype = MagicVilla_Utility.SD.Apitype.DELETE,
                Url = villaUrl + "/api/v1/VillaAPI/" + id,
                Token = token
            });
        }

        public Task<T> GetAllAsync<T>(string token)
        {
            return SendAsync<T>(new APIRequest()
            {
                Apitype = MagicVilla_Utility.SD.Apitype.GET,
                Url = villaUrl + "/api/v1/VillaAPI",
                Token = token
            });
        }

        public Task<T> GetAsync<T>(int id, string token)
        {
            return SendAsync<T>(new APIRequest()
            {
                Apitype = MagicVilla_Utility.SD.Apitype.GET,
                Url = villaUrl + "/api/v1/VillaAPI/" + id,
                Token = token
            });
        }

        [Authorize(Roles ="admin")]
        public Task<T> UpdateAsync<T>(VillaUpdateDTO dto, string token)
        {
            return SendAsync<T>(new APIRequest()
            {
                Apitype = MagicVilla_Utility.SD.Apitype.PUT,
                Data = dto,  
                Url = villaUrl + "/api/v1/VillaAPI/" + dto.Id,
                Token = token

            }); ;
        }
    }
}
