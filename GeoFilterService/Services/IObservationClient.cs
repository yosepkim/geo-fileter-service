using NetTopologySuite.Features;
using Newtonsoft.Json.Linq;

namespace GeoFilterService.Services
{
    public interface IObservationClient
    {
        public FeatureCollection GetAmbientData();

        public JArray? GetAmbientDataInJson();
    }
}
