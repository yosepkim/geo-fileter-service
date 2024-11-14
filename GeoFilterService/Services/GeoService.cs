using Geohash;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Linq;

namespace GeoFilterService.Services
{
    public class GeoService : IGeoService
    {
        public BoundingBox GetBoundingBox(Coordinate coordinate, int distanceRadiusInKm)
        {
            var latChange = distanceRadiusInKm / 111.2;
            var lonChange = Math.Abs(Math.Cos(coordinate.X * (Math.PI / 180)));

            BoundingBox boundingBox = new BoundingBox();
            boundingBox.MinLat = coordinate.X - latChange;
            boundingBox.MinLng = coordinate.Y - lonChange;
            boundingBox.MaxLat = coordinate.X + latChange;
            boundingBox.MaxLng = coordinate.Y + lonChange;
            
            return boundingBox;
        }

        public void addGeohash(JArray ambientData)
        {
            var geohasher = new Geohasher();
            foreach (JObject item in ambientData)
            {
                double longitude = (double)item["geometry"]["coordinates"][0];
                double lattitude = (double)item["geometry"]["coordinates"][1];
                string geohash = geohasher.Encode(lattitude, longitude, 9);
                item.Add("hash", geohash);
            }
        }

        public void addCoordinates(JArray ambientData)
        {
            foreach (JObject item in ambientData)
            {
                double longitude = (double)item["geometry"]["coordinates"][0];
                double lattitude = (double)item["geometry"]["coordinates"][1];
                item.Add("longitude", longitude);
                item.Add("lattitude", lattitude);
            }
        }
    }
}
