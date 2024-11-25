using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace KhanhSkin_BackEnd.Services.Products
{
    public class ProductRecommenService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductRecommenService> _logger;

        public ProductRecommenService(HttpClient httpClient, ILogger<ProductRecommenService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Guid>> GetRecommendationsAsync(string id, int topN = 5)
        {
            try
            {
                string productId = id.ToUpper();
                _logger.LogInformation($"Fetching recommendations for Product ID: {productId}");

                string apiUrl = $"http://127.0.0.1:8000/recommendations/{productId}?top_n={topN}";
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Response from API: {jsonResponse}");

                    // Deserialize JSON trả về thành Dictionary
                    var recommendations = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonResponse);

                    var guids = recommendations["recommendations"]
                        .Select(Guid.Parse)  // Chuyển đổi từng chuỗi thành GUID
                        .ToList();

                    return guids;

                }

                _logger.LogWarning($"Failed to fetch recommendations. Status Code: {response.StatusCode}");
                return new List<Guid>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching recommendations: {ex.Message}");
                throw;
            }
       

    }
}
}
