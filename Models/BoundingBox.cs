using System;
using System.Collections.Generic;
using System.Text;

namespace ElevationModule.Models
{
    public class BoundingBox
    {
        //public Dictionary<int, BBNode> Box { get; set; }
        public BBNode[,] BBNodesArray = new BBNode[5,5];
        public BBNode MinElevationNode { get; set; }
        public BBNode MaxElevationNode { get; set; }

        public BBNode GetMinElevationNode()
        {
            BBNode getFirstIndex = BBNodesArray[0,0];

            BBNode minEleveationNode = getFirstIndex;

            int rowLength = BBNodesArray.GetLength(0);
            int columnLength = BBNodesArray.GetLength(1);

            for (var i = 0; i < columnLength - 1; i++)
            {
                for (var a = 0; a < rowLength - 1; a++)
                {
                    
                    if (BBNodesArray[i, a].Elevation < minEleveationNode.Elevation)
                    {
                        minEleveationNode = BBNodesArray[i, a];
                    }
                }
            }
            return minEleveationNode;
        }

        public BBNode GetMaxElevationNode()
        {
            BBNode getFirstIndex = BBNodesArray[0,0];

            BBNode maxElevationNode = getFirstIndex;

            int rowLength = BBNodesArray.GetLength(0);
            int columnLength = BBNodesArray.GetLength(1);

            for (var i = 0; i < columnLength; i++)
            {
                for (var a = 0; a < rowLength; a++)
                {
                    
                    if (BBNodesArray[i, a].Elevation > maxElevationNode.Elevation)
                    {
                        maxElevationNode = BBNodesArray[i, a];
                    }
                }
            }
            return maxElevationNode;
        }

        public double GetSlope()
        {
            var run = GetDistanceBetweenMinAndMaxNode();
            var rise = MaxElevationNode.Elevation - MinElevationNode.Elevation;

            var slope = rise / run;

            return slope;

        }

        public double GetDistanceBetweenMinAndMaxNode()
        {
            MinElevationNode = GetMinElevationNode();
            MaxElevationNode  = GetMaxElevationNode();

            var minGeoCoding = new GeoCode
            {
                Latitude = MinElevationNode.Latitude,
                Longitude = MinElevationNode.Longitude
            };

            var maxGeoCoding = new GeoCode
            {
                Latitude = MaxElevationNode.Latitude,
                Longitude = MaxElevationNode.Longitude
            };

            var distanceBetweenNodes = CalculateMeterDistance(minGeoCoding, maxGeoCoding);

            return distanceBetweenNodes;

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

    }
}
