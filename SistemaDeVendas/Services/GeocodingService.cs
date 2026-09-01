using System.Globalization;
using System.Text.Json;

namespace SistemaDeVendas.Services
{
    public class GeocodingService
    {
        private readonly HttpClient _http;

        public GeocodingService(HttpClient http)
        {
            _http = http;
        }

        public async Task<(double Latitude, double Longitude)?> BuscarCoordenadas(string endereco)
        {
            if (string.IsNullOrWhiteSpace(endereco))
                return null;

            var url = $"search?format=json&limit=1&q={Uri.EscapeDataString(endereco)}";

            HttpResponseMessage resposta;
            try
            {
                resposta = await _http.GetAsync(url);
            }
            catch (HttpRequestException)
            {
                return null;
            }

            if (!resposta.IsSuccessStatusCode)
                return null;

            var conteudo = await resposta.Content.ReadAsStringAsync();
            using var documento = JsonDocument.Parse(conteudo);

            if (documento.RootElement.ValueKind != JsonValueKind.Array || documento.RootElement.GetArrayLength() == 0)
                return null;

            var item = documento.RootElement[0];
            var lat = double.Parse(item.GetProperty("lat").GetString()!, CultureInfo.InvariantCulture);
            var lon = double.Parse(item.GetProperty("lon").GetString()!, CultureInfo.InvariantCulture);

            return (lat, lon);
        }
    }
}
