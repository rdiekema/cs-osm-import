using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mapper.OSM;

namespace Mapper
{
    public enum RoadTypes
    {
        None,
        PedestrianGravel,
        PedestrianPavement,
        PedestrianElevated,

        MetroTrack,
        BusLine,
        MetroLine,
        TrainLine,
        TrainCargoTrack,

        BasicRoad,
        BasicRoadDecorationTrees,
        BasicRoadDecorationGrass,
        BasicRoadBridge,
        BasicRoadElevated,
        OnewayRoad,
        OnewayRoadDecorationTrees,
        OnewayRoadDecorationGrass,
        OnewayRoadElevated,
        OnewayRoadBridge,
        LargeOneway,
        LargeOnewayDecorationGrass,
        LargeOnewayDecorationTrees,
        LargeOnewayBridge,
        LargeOnewayElevated,
        MediumRoad,
        MediumRoadDecorationGrass,
        MediumRoadDecorationTrees,
        MediumRoadBridge,
        MediumRoadElevated,
        LargeRoad,
        LargeRoadDecorationGrass,
        LargeRoadDecorationTrees,
        LargeRoadBridge,
        LargeRoadElevated,
        GravelRoad,

        TrainTrack,
        TrainTrackBridge,
        TrainTrackElevated,
        TrainConnectionTrack,
        Highway,
        HighwayBridge,
        HighwayElevated,
        HighwayRamp,
        HighwayRampElevated,
        HighwayBarrier,
        
        TwoLaneHighway,

        AirplaneTaxiway,
        Dam,
    }


    public class RoadMapping
    {
        public const int GameSizeMetres = 18000;
        public const int GameSizeGameCoordinates = 1920 * 9;
        double maxBounds;

        private static Dictionary<KeyValuePair<string, string>, RoadTypes> roadTypeMapping = new Dictionary<KeyValuePair<string, string>, RoadTypes>();

        // private static Dictionary<RoadTypes, KeyValuePair<string, string>> reverseMapping = new Dictionary<RoadTypes, KeyValuePair<string, string>>();

        private static Dictionary<string, bool> pavedMapping = new Dictionary<string, bool>();

        //private Vector2 startLatLon = new Vector2(float.MaxValue, float.MaxValue);
        private Vector2 middleLatLon = new Vector2(float.MinValue, float.MinValue);
        //private Vector2 endLatLon;
        double scaleX;
        double scaleY;

        static RoadMapping()
        {
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "motorway"), RoadTypes.TwoLaneHighway);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "trunk"), RoadTypes.BasicRoad);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "primary"), RoadTypes.BasicRoad);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "secondary"), RoadTypes.BasicRoad);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "tertiary"), RoadTypes.BasicRoad);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "unclassified"), RoadTypes.BasicRoad);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "bus_guideway"), RoadTypes.BasicRoad);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "road"), RoadTypes.BasicRoad);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "residential"), RoadTypes.BasicRoad);
            // roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "service"), RoadTypes.GravelRoad);
            // roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "living_street"), RoadTypes.GravelRoad);
            // roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "track"), RoadTypes.GravelRoad);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "motorway_link"), RoadTypes.HighwayRamp);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "trunk_link"), RoadTypes.HighwayRamp);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "primary_link"), RoadTypes.HighwayRamp);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "secondary_link"), RoadTypes.HighwayRamp);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "tertiary_link"), RoadTypes.HighwayRamp);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "raceway"), RoadTypes.HighwayRamp);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "pedestrian"), RoadTypes.PedestrianPavement);
            // roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "footway"), RoadTypes.PedestrianPavement);
            // roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "steps"), RoadTypes.PedestrianPavement);
            // roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "bridleway"), RoadTypes.PedestrianPavement);
            roadTypeMapping.Add(new KeyValuePair<string, string>("highway", "cycleway"), RoadTypes.PedestrianPavement);
            roadTypeMapping.Add(new KeyValuePair<string, string>("railway", "miniature"), RoadTypes.TrainTrack);
            roadTypeMapping.Add(new KeyValuePair<string, string>("railway", "monorail"), RoadTypes.TrainTrack);
            roadTypeMapping.Add(new KeyValuePair<string, string>("railway", "narrow_gauge"), RoadTypes.TrainTrack);
            roadTypeMapping.Add(new KeyValuePair<string, string>("railway", "preserved"), RoadTypes.TrainTrack);
            roadTypeMapping.Add(new KeyValuePair<string, string>("railway", "rail"), RoadTypes.TrainTrack);

            pavedMapping.Add("paved", true);
            pavedMapping.Add("asphalt", true);
            pavedMapping.Add("cobblestone", true);
            pavedMapping.Add("cobblestone:flattened", true);
            pavedMapping.Add("concrete", true);
            pavedMapping.Add("concrete:lanes", true);
            pavedMapping.Add("concrete:plates", true);
            pavedMapping.Add("paving_stones", true);
            pavedMapping.Add("metal", true);

            pavedMapping.Add("wood", false);
            pavedMapping.Add("unpaved", false);
            pavedMapping.Add("compacted", false);
            pavedMapping.Add("dirt", false);
            pavedMapping.Add("earth", false);
            pavedMapping.Add("fine_gravel", false);
            pavedMapping.Add("grass", false);
            pavedMapping.Add("gravel", false);
            pavedMapping.Add("ground", false);
            pavedMapping.Add("pebblestone", false);
            pavedMapping.Add("salt", false);
            pavedMapping.Add("sand", false);
        }

        public RoadMapping(double tiles)
        {
            maxBounds = tiles * 1920;
        }


        public bool Mapped(osmWay way, ref List<string> points, ref RoadTypes rt, ref int layer)
        {
            if (way.tag == null || way.nd == null || way.nd.Count() < 2)
            {
                return false;
            }
            rt = RoadTypes.None;
            bool oneWay = false;
            bool invert = false;
            var surface = "";

            foreach (var tag in way.tag)
            {
                if (tag.k.Trim().ToLower() == "oneway")
                {
                    oneWay = true;
                    if (tag.v.Trim() == "-1")
                    {
                        invert = true;
                    }
                }
                else if (tag.k.Trim().ToLower() == "bridge")
                {
                    layer = Math.Max(layer, 1);
                }
                else if (tag.k.Trim().ToLower() == "layer")
                {
                    int.TryParse(tag.v.Trim(), out layer);
                }
                else if (tag.k.Trim().ToLower() == "surface")
                {
                    surface = tag.v.Trim().ToLower();
                }
                else
                {
                    var kvp = new KeyValuePair<string, string>(tag.k.Trim(), tag.v.Trim());
                    if (roadTypeMapping.ContainsKey(kvp))
                    {
                        rt = roadTypeMapping[kvp];
                    }
                }
            }

            if (oneWay)
            {
                rt = ConvertToOneWayRoadType(rt);
            }

            if (rt != RoadTypes.None)
            {
                if (surface != "")
                {
                    rt = CheckSurface(rt, surface);
                }

                points = new List<string>();
                if (invert)
                {
                    for (var i = way.nd.Count() - 1; i >= 0; i -= 1)
                    {
                        points.Add(way.nd[i].@ref);
                    }
                }
                else
                {
                    foreach (var nd in way.nd)
                    {
                        points.Add(nd.@ref);
                    }
                }
                return true;
            }
            return false;
        }

        private RoadTypes CheckSurface(RoadTypes rt, string surface)
        {
            if (pavedMapping.ContainsKey(surface))
            {
                if (pavedMapping[surface])
                {
                    if (rt == RoadTypes.GravelRoad)
                    {
                        return RoadTypes.BasicRoad;
                    }

                    if (rt == RoadTypes.PedestrianGravel)
                    {
                        return RoadTypes.PedestrianPavement;
                    }
                }
                else
                {
                    if (rt == RoadTypes.PedestrianPavement || rt == RoadTypes.PedestrianGravel)
                    {
                        return RoadTypes.PedestrianGravel;
                    }

                    return RoadTypes.GravelRoad;
                }
            }
            return rt;
        }

        private static RoadTypes ConvertToOneWayRoadType(RoadTypes rt)
        {
            switch (rt)
            {
                case RoadTypes.BasicRoad:
                case RoadTypes.MediumRoad:
                    return RoadTypes.OnewayRoad;
                case RoadTypes.BasicRoadDecorationTrees:
                case RoadTypes.MediumRoadDecorationTrees:
                    return RoadTypes.OnewayRoadDecorationTrees;
                case RoadTypes.MediumRoadDecorationGrass:
                case RoadTypes.BasicRoadDecorationGrass:
                    return RoadTypes.OnewayRoadDecorationGrass;
                case RoadTypes.LargeRoad:
                case RoadTypes.LargeRoadDecorationGrass:
                case RoadTypes.LargeRoadDecorationTrees:
                case RoadTypes.Highway:
                    return RoadTypes.Highway;
                case RoadTypes.TwoLaneHighway:
                    return RoadTypes.TwoLaneHighway;
                case RoadTypes.GravelRoad:
                    return RoadTypes.OnewayRoad;
                case RoadTypes.HighwayRamp:
                    return RoadTypes.HighwayRamp;
                case RoadTypes.LargeOneway:
                    return RoadTypes.LargeOneway;
            }
            return RoadTypes.None;
        }

        //public void InitBoundingBox(osmNode node)
        //{
        //    startLatLon = new Vector2(Math.Min(startLatLon.x, (float)node.lon), Math.Min(startLatLon.y, (float)node.lat));
        //    endLatLon = new Vector2(Math.Max(endLatLon.x, (float)node.lon), Math.Max(endLatLon.y, (float)node.lat));
        //}

        public void InitBoundingBox(osmBounds bounds, double scale)
        {
            this.middleLatLon = new Vector2((float) (bounds.minlon + bounds.maxlon) / 2f,
                (float) (bounds.minlat + bounds.maxlat) / 2f);
            var lat = Deg2rad(this.middleLatLon.y);
            var radius = WGS84EarthRadius(lat);
            var pradius = radius * Math.Cos(lat);
            scaleX = scale * GameSizeGameCoordinates / Rad2deg(GameSizeMetres / pradius);
            scaleY = scale * GameSizeGameCoordinates / Rad2deg(GameSizeMetres / pradius);
        }

        public bool GetPos(decimal lat, decimal lon, ref Vector2 pos)
        {
            pos = new Vector2((float) (((float) lat - middleLatLon.x) * scaleX),
                (float) (((float) lon - middleLatLon.y) * scaleY));

            if (Math.Abs(pos.x) > maxBounds || Math.Abs(pos.y) > maxBounds)
            {
                return false;
            }

            //pos += new Vector2(1920f * 0.5f, 1920f * -0.5f);
            return true;
        }

        private const double WGS84_a = 6378137.0; // Major semiaxis [m]
        private const double WGS84_b = 6356752.3; // Minor semiaxis [m]

        private static double Deg2rad(double degrees)
        {
            return Math.PI * degrees / 180.0;
        }


        private static double Rad2deg(double radians)
        {
            return 180.0 * radians / Math.PI;
        }

        private static double WGS84EarthRadius(double lat)
        {
            var An = WGS84_a * WGS84_a * Math.Cos(lat);
            var Bn = WGS84_b * WGS84_b * Math.Sin(lat);
            var Ad = WGS84_a * Math.Cos(lat);
            var Bd = WGS84_b * Math.Sin(lat);
            return Math.Sqrt((An * An + Bn * Bn) / (Ad * Ad + Bd * Bd));
        }


        internal void GetTags(ushort buildingId, Building data, List<osmWayTag> tags, ref string amenity)
        {
            var service = data.Info.m_class.m_service;
            var ss = data.Info.m_class.m_subService;
            var landuse = "";
            var building = "";
            var name = "";
            var className = data.Info.m_class.name.ToLower();
            switch (service)
            {
                case ItemClass.Service.Beautification:
                    if (className.Contains("marker"))
                    {
                        return;
                    }
                    landuse = "recreation_ground";
                    break;
                case ItemClass.Service.Commercial:
                    building = "retail";
                    break;
                case ItemClass.Service.Residential:
                    building = "residential";
                    break;
                case ItemClass.Service.Office:
                    building = "commercial";
                    break;
                case ItemClass.Service.Industrial:
                    building = "industrial";
                    break;
                case ItemClass.Service.Garbage:
                    landuse = "landfill";
                    break;
                case ItemClass.Service.Education:
                    amenity = "school";
                    building = "school";
                    break;
                case ItemClass.Service.Electricity:
                    tags.Add(new osmWayTag {k = "power", v = "plant"});
                    break;
                case ItemClass.Service.FireDepartment:
                    amenity = "fire_station";
                    building = "yes";
                    break;
                case ItemClass.Service.HealthCare:
                    amenity = "hospital";
                    building = "yes";
                    break;
                case ItemClass.Service.Monument:
                    name = data.Info.name;
                    building = "yes";
                    break;
                case ItemClass.Service.PoliceDepartment:
                    amenity = "police";
                    building = "yes";
                    break;
                case ItemClass.Service.PublicTransport:
                    if (!className.Contains("facility"))
                    {
                        return;
                    }
                    if (ss == ItemClass.SubService.PublicTransportMetro)
                    {
                    }
                    building = "train_station";
                    break;
                case ItemClass.Service.Tourism:
                    building = "hotel";
                    break;
                default:
                    return;
            }

            if ((data.m_flags & Building.Flags.CustomName) != Building.Flags.None)
            {
                var id = new InstanceID();
                id.Building = buildingId;
                name = Singleton<InstanceManager>.instance.GetName(id);
            }

            if (landuse != "")
            {
                tags.Add(new osmWayTag {k = "landuse", v = landuse});
            }
            if (building != "")
            {
                tags.Add(new osmWayTag {k = "building", v = building});
            }
            if (name != "")
            {
                tags.Add(new osmWayTag {k = "name", v = name});
            }
        }
    }
}