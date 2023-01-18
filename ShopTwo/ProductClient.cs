using Newtonsoft.Json;
using ShopTwo.Models;

namespace ShopTwo
{
    public class ProductClient
    {
        public async Task<List<Product>> GetProducts(string baseUrl, HttpClient? httpClient = null)
        {
            using var client = httpClient == null ? new HttpClient() : httpClient;

            var response = await client.GetAsync(baseUrl + "Product/list");
            response.EnsureSuccessStatusCode();

            var responseStr = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Product>>(responseStr);
        }


        public async Task<Product> GetProduct(string baseUrl, int productId, HttpClient? httpClient = null)
        {
            using var client = httpClient == null ? new HttpClient() : httpClient;

            var response = await client.GetAsync(baseUrl + "Product/" + productId);
            response.EnsureSuccessStatusCode();

            var resp = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Product>(resp);
        }
    }
}
