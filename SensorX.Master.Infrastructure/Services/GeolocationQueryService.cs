using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SensorX.Master.Application.Services;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Infrastructure.Services
{
    public class GeolocationQueryService : IGeolocationQueryService
    {
        private readonly HttpClient _httpClient;

        public GeolocationQueryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SensorX.Master/1.0");
            }
        }

        public async Task<List<Geolocation?>?> GetGeolocationByAddress(string address, CancellationToken cancellationToken = default)
        {
            var requestUri = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=jsonv2";

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(2)); // 2-second timeout

                var response = await _httpClient.GetAsync(requestUri, cts.Token);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cts.Token);
                var places = JsonSerializer.Deserialize<List<NominatimPlaceResponse>>(content);

                if (places == null || places.Count == 0)
                {
                    return null;
                }

                var geolocations = new List<Geolocation?>();

                foreach (var location in places)
                {
                    if (!double.TryParse(location.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude))
                    {
                        continue;
                    }

                    if (!double.TryParse(location.Lon, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
                    {
                        continue;
                    }

                    geolocations.Add(new Geolocation(latitude, longitude));
                }

                return geolocations.Count > 0 ? geolocations : null;
            }
            catch (Exception)
            {
                // Fallback immediately on timeout, network, or DNS errors
                return null;
            }
        }

        private sealed class NominatimPlaceResponse
        {
            [JsonPropertyName("lat")]
            public string Lat { get; set; } = string.Empty;

            [JsonPropertyName("lon")]
            public string Lon { get; set; } = string.Empty;

            [JsonPropertyName("display_name")]
            public string? DisplayName { get; set; }
        }
    }
}