using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace GeoFilterService.Services
{
    public class ObservationClient : IObservationClient
    {

        private readonly string backendAPIUrl = "{api-url}";
        private readonly HttpClient httpClient;
        public ObservationClient()
        {
            httpClient = new();
            httpClient.BaseAddress = new(backendAPIUrl);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public FeatureCollection GetAmbientData()
        {
            var ambientText = getAmbientDataFromBackend();
            GeoJsonReader geoJsonReader = new GeoJsonReader();

            var result = geoJsonReader.Read<FeatureCollection>(ambientText);
            return result;
        }

        public JArray? GetAmbientDataInJson()
        {
            var ambientText = getAmbientDataFromBackend();
            JObject root = JObject.Parse(ambientText);
            JArray? features = root["features"] as JArray;

            return features;
        }

        private string getAmbientDataFromBackend()
        {
            string urlParameters = "observations/v1/ambient.geojson?apikey={api-key}&lightning=true&airquality=true&details=true";


            var response = httpClient.GetAsync(urlParameters).Result;
            if (response.IsSuccessStatusCode)
            {
                var ambientText = response.Content.ReadAsStringAsync().Result;

                return ambientText;
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode,
                              response.ReasonPhrase);
                return "";
            }
        }
    }
}
