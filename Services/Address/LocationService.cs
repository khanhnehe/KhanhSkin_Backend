using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace KhanhSkin_BackEnd.Services.Address
{
    public class LocationService
    {
        private readonly HttpClient _httpClient;

        public LocationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetProvinceAsync(int provinceId)
        {
            var response = await _httpClient.GetStringAsync($"https://provinces.open-api.vn/api/p/{provinceId}");
            var data = JObject.Parse(response);
            return data["name"].ToString();
        }

        public async Task<string> GetDistrictAsync(int districtId)
        {
            var response = await _httpClient.GetStringAsync($"https://provinces.open-api.vn/api/d/{districtId}?depth=2");
            var data = JObject.Parse(response);
            return data["name"].ToString();
        }

        public async Task<string> GetWardAsync(int wardId)
        {
            var response = await _httpClient.GetStringAsync($"https://provinces.open-api.vn/api/w/{wardId}?depth=2");
            var data = JObject.Parse(response);
            return data["name"].ToString();
        }
    }
}
