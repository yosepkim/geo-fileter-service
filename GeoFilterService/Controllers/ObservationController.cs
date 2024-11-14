using GeoFilterService.Services;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO.Compression;
using RestSharp.Extensions;
using System.Text;

namespace GeoFilterService.Controllers
{
    [ApiController]
    [Route("observations/v1")]
    public class ObservationController : ControllerBase
    {
        private readonly ILogger<ObservationController> _logger;
        private readonly IObservationClient _observationClient;
        private readonly IGeoService _geoService;
        private readonly IHarperDBClient _harperDBClient;

        public ObservationController(ILogger<ObservationController> logger, 
                                        IObservationClient observationClient, 
                                        IGeoService geoService, IHarperDBClient harperDBClient)
        {
            _logger = logger;
            _observationClient = observationClient;
            _geoService = geoService;
            _harperDBClient = harperDBClient;
        }

        [HttpGet("ambient")]
        public string Get([Required] string q, [Required] int distranceRadius)
        {
            var coordinates = GetCoordinatesFromQueryParam(q);
            var boundingBox = _geoService.GetBoundingBox(coordinates, distranceRadius);
            var result = _harperDBClient.GetAmbientDataByBoundingBox(boundingBox);

            return result;
        }

        [HttpGet("ambient/all")]
        public string GetAll()
        {
            var result = _harperDBClient.GetAllAmbientData();

            return result;
        }

        [HttpGet("ambient/all/compressed")]
        public byte[] GetAllCompressed()
        {
            var result = _harperDBClient.GetAllAmbientDataInStream();

            byte[] compressed;

            using (var outStream = new MemoryStream())
            {
                using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
                {
                    result.Result.CopyTo(tinyStream);
                }

                compressed = outStream.ToArray();
                return compressed;
            }
        }

        [HttpGet("ambient/all/stream")]
        public async Task GetAllInStream()
        {
            using (var responseStream = _harperDBClient.GetAllAmbientDataInStream())
            {

                Response.ContentType = " application/octet-stream";
                var sw = new StreamWriter(Response.Body);
                await responseStream.Result.CopyToAsync(sw.BaseStream);
                await sw.FlushAsync().ConfigureAwait(false);
            }
        }

        [HttpPost("ambient/store/coords")]
        public string StoreCoordinates()
        {
            var result = _observationClient.GetAmbientDataInJson();
            _geoService.addCoordinates(result);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _harperDBClient.StoreCoordinates(result);
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            return $"Storing Time: {elapsedMs}";
        }

        [HttpPost("ambient/store")]
        public string StoreGeoJson()
        {
            var result = _observationClient.GetAmbientDataInJson();
            _harperDBClient.StoreCoordinates(result);

            return "done";

        }

        [HttpPost("ambient/store/hash")]
        public string StoreGeoHash()
        {
            var result = _observationClient.GetAmbientDataInJson();
            _geoService.addGeohash(result);
            _harperDBClient.StoreCoordinates(result);

            return "done"; ;

        }

        private Coordinate GetCoordinatesFromQueryParam(string queryParam)
        {
            var coordinatePair = queryParam.Split(',').ToList<string>();
            var coordinate = new Coordinate(Convert.ToDouble(coordinatePair[0]), Convert.ToDouble(coordinatePair[1]));
            return coordinate;

        }
    }
}