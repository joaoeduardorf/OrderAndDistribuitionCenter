using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OrderSolution.Infrastructure.Integrations
{


    public class DistributionCenterAPI : IDistributionCenterAPI
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DistributionCenterAPI> _logger;

        public DistributionCenterAPI(HttpClient httpClient, ILogger<DistributionCenterAPI> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetDistributionCentersByItemAsync(int idSku, string correlationId)
        {
            _logger.LogInformation("Requesting distribution centers for SKU: {IdSku} with CorrelationId: {CorrelationId}", idSku, correlationId);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/DistribuitionCenters?itemId={idSku}");
                request.Headers.Add("X-Correlation-ID", correlationId);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                //var result = JsonSerializer.Deserialize<DcResponse>(content);
                var result = JsonSerializer.Deserialize<DcResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var distribuitionCenters = string.Join(", ", result.DistribuitionCenters);
                _logger.LogInformation("Successfully retrieved distribution centers for SKU: {IdSku} with CorrelationId: {CorrelationId}", idSku, correlationId);
                return distribuitionCenters;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve distribution centers for SKU: {IdSku} with CorrelationId: {CorrelationId}", idSku, correlationId);
                throw;
            }
        }
    }
}

public class DcResponse
{
    public List<string> DistribuitionCenters { get; set; }
}