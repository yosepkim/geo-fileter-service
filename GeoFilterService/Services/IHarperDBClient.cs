using Geohash;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace GeoFilterService.Services
{
    public interface IHarperDBClient
    {
        public string GetAmbientDataByBoundingBox(BoundingBox boundingBox);

        public string GetAllAmbientData();

        public Task<Stream> GetAllAmbientDataInStream();
        public string StoreGeoJson(JArray geoJsonArray);

        public string StoreGeoHash(JArray geoJsonArray);

        public string StoreCoordinates(JArray geoJsonArray);
    }
}
