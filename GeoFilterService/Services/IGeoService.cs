using Geohash;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Linq;

namespace GeoFilterService.Services
{
    public interface IGeoService
    {
        public BoundingBox GetBoundingBox(Coordinate coordinate, int distanceRadiusInKm);

        public void addGeohash(JArray ambientData);

        public void addCoordinates(JArray ambientData);
    }
}
