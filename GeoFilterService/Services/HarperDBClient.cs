using Geohash;
using HarperNetClient.models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net.Http;
using System.Net.Http.Headers;

namespace GeoFilterService.Services
{
    public class HarperDBClient : IHarperDBClient
    {
        private readonly string harperDBUrl = "{harper-url}";
        private readonly string harperDBAccessToken = "{harper-access-token";
        private readonly HttpClient httpClient;
        public HarperDBClient() 
        {
            httpClient = new();
            httpClient.BaseAddress = new(harperDBUrl);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + harperDBAccessToken);
        }
        public string GetAmbientDataByBoundingBox(BoundingBox boundingBox)
        {
            var query = $$"""
                {
                    "operation": "search_by_conditions",
                    "database": "geocoord",
                    "table": "ambient",
                    "operator": "and",
                    "get_attributes": [
                        "geometry", "properties", "type"
                    ],
                    "conditions": [
                        {
                            "search_attribute": "lattitude",
                            "search_type": "between",
                            "search_value": [
                                {{boundingBox.MinLat}},
                                {{boundingBox.MaxLat}}
                            ]
                        },
                        {
                            "search_attribute": "longitude",
                            "search_type": "between",
                            "search_value": [
                                {{boundingBox.MinLng}},
                                {{boundingBox.MaxLng}}
                            ]
                        }
                    ]
                }
            """;

            var restResponse = ExecuteQuery(query);

            return restResponse;
        }


        public string GetAllAmbientData()
        {
            var query = $$"""
                {
                    "operation": "search_by_value",
                    "database": "geocoord",
                    "table": "ambient",
                    "search_attribute": "*",
                    "search_value": "*",
                    "get_attributes": [
                        "geometry", "properties", "type"
                    ]
                }
            """;

            var restResponse = ExecuteQuery(query);

            return restResponse;
        }

        public Task<Stream> GetAllAmbientDataInStream()
        {
            var query = $$"""
                {
                    "operation": "search_by_value",
                    "database": "geocoord",
                    "table": "ambient",
                    "search_attribute": "*",
                    "search_value": "*",
                     "get_attributes": [
                        "geometry", "properties", "type"
                    ]
                }
            """;

            return ExecuteAndReturnQueryAsync(query);
        }

        public string StoreGeoJson(JArray geoJsonArray)
        {
            foreach (var item in geoJsonArray)
            {
                List<object> list = [item];

                ExecuteQuery(CreateSerializedQuery("insert", "geodata", "ambient", list));
            }

            return "done";

        }

        public string StoreGeoHash(JArray geoJsonArray)
        {
            foreach (var item in geoJsonArray)
            {
                List<object> list = [item];

                ExecuteQuery(CreateSerializedQuery("insert", "geohashdata", "ambient", list));
            }

            return "done";

        }

        public string StoreCoordinates(JArray geoJsonArray)
        {
            int count = 0;
            List<object> list = new List<object>();
            foreach (var item in geoJsonArray)
            {
                list.Add(item);
                if (count >= 1000)
                {
                    ExecuteQueryAsync(CreateSerializedQuery("insert", "geocoord", "ambient", list));
                    list.Clear();
                    count = 0;
                }
                count++;
            }
            if (list.Count > 0)
            {
                ExecuteQueryAsync(CreateSerializedQuery("insert", "geocoord", "ambient", list));
            }
            return "done";
        }

        private string CreateSerializedQuery(string operation, string schema, string table, List<object> itemList)
        {
            HarperRequest query = new()
            {
                Operation = operation,
                Schema = schema,
                Table = table,
                Records = itemList
            };

            var serializedQuery = JsonConvert.SerializeObject(query, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                }
            );
            return serializedQuery;
        }

        private string ExecuteQuery(string query)
        {
            var restRequest = CreateHarperDBRequest();
            restRequest.Content = new StringContent(query, System.Text.Encoding.UTF8, "application/json");

            var restResponse = httpClient.SendAsync(restRequest).Result;
            var serverTiming = restResponse.Headers.GetValues("server-timing").FirstOrDefault(); ;
            Console.WriteLine($"server-timing: {serverTiming}");
            return restResponse.Content.ReadAsStringAsync().Result;
        }

        private void ExecuteQueryAsync(string query)
        {
            var restRequest = CreateHarperDBRequest();
            restRequest.Content = new StringContent(query, System.Text.Encoding.UTF8, "application/json");

            httpClient.SendAsync(restRequest);
        }

        private async Task<Stream> ExecuteAndReturnQueryAsync(string query)
        {
            var restRequest = CreateHarperDBRequest();
            restRequest.Content = new StringContent(query, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(restRequest, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStreamAsync();
            return content;
        }

        private HttpRequestMessage CreateHarperDBRequest()
        {
            HttpRequestMessage restRequest = new (HttpMethod.Post, harperDBUrl);

            return restRequest;
        }
    }
}
