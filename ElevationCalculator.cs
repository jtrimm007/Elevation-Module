using ElevationModule.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ElevationModule
{
    public class ElevationCalculator
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double[] LowerBoundingBoxPoint { get; private set; }
        public double[] UpperBoundingBoxPoint { get; private set; }
        public BoundingBox BoundingBox { get; set; }

        private double Displacement = 0.0003;
        private string _apiKey;
        public ElevationCalculator(string apiKey)
        {
            _apiKey = apiKey;
        }

        public void SetHouseGeocoordinates(GeoCode geoCode)
        {
            Latitude = geoCode.Latitude;
            Longitude = geoCode.Longitude;
        }

        public double CalculateMeterDistance(GeoCode locationOne, GeoCode locationTwo)
        {
            var earthRadius = 6371e3; //in meters

            var radiansLat1 = locationOne.Latitude * Math.PI / 180;
            var radiansLat2 = locationTwo.Latitude * Math.PI / 180;

            var latDistance = (locationTwo.Latitude - locationOne.Latitude) * Math.PI / 180;
            var lngDistance = (locationTwo.Longitude - locationOne.Longitude) * Math.PI / 180;

            var a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2) + Math.Cos(radiansLat1) * Math.Cos(radiansLat2) * Math.Sin(lngDistance / 2) * Math.Sin(lngDistance / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var distance = earthRadius * c;

            return distance;
        }



        public void SetUpperAndLowerBoundingBoxPoints()
        {
            double lowerLat = Latitude - Displacement;
            double lowerLong = Longitude - Displacement;

            double uppperLat = Latitude + Displacement;
            double uppperLong = Longitude + Displacement;

            LowerBoundingBoxPoint = new double[]{ lowerLat, lowerLong };
            UpperBoundingBoxPoint = new double[]{ uppperLat, uppperLong };
        }

        public async Task SetBoundingBoxNodes()
        {
            var bingBoundingBox = await GetBoundingBoxFromBing();

            var elevations = bingBoundingBox["resourceSets"][0]["resources"][0]["elevations"];
            var elevationsArray = elevations.ToObject<string[]>();

            BoundingBox boundingBox = new BoundingBox();

            var indexOfElevationArray = 0;

                for (var e = 0; e < 5; e++)
                {
                    for (var a = 0; a < 5; a++)
                    {

                        BBNode bbNode = new BBNode
                        {
                            Latitude = this.Latitude + (e * 0.00015),
                            Longitude = this.Longitude + (a * 0.00015),
                            Elevation = Int32.Parse(elevationsArray[indexOfElevationArray]),
                            Index = indexOfElevationArray
                        };

                        boundingBox.BBNodesArray[e, a] = bbNode;
                        indexOfElevationArray++;
                    }
                }
            BoundingBox = boundingBox;
        }


        public async Task<JObject> GetBoundingBoxFromBing()
        {
            var requestUri = $"http://dev.virtualearth.net/REST/v1/Elevation/Bounds?bounds={LowerBoundingBoxPoint[0]},{LowerBoundingBoxPoint[1]},{UpperBoundingBoxPoint[0]},{UpperBoundingBoxPoint[1]}&rows=5&cols=5&heights=sealevel&key={_apiKey}";

            using(var client = new HttpClient())
            {
                var request = await client.GetAsync(Uri.EscapeUriString(requestUri));

                var content = await request.Content.ReadAsStringAsync();

                JObject json = JObject.Parse(content);

                return json;
            }
        }
    }
}
