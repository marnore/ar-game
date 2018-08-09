using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

/**<summary> Parser for location API responses </summary>*/
public static class Parser {

    /**<summary> Parse and return LocationData from json </summary>*/
    public static LocationData ParseLocation(string json)
    {
        LocationData tmp = JsonConvert.DeserializeObject<LocationData>(json);
        LocationData locationData = new LocationData(new Vector3(-tmp.position.x, tmp.position.y, tmp.position.z), new Vector3(-tmp.direction.x, tmp.direction.y, tmp.direction.z), tmp.rotation);
        locationData.areaID = tmp.areaID;
        return locationData;
    }

    /**<summary> Parse and return Building array from json </summary>*/
    public static Building[] ParseBuildings(string json)
    {
        Building[] buildings = JsonConvert.DeserializeObject<List<Building>>(json).ToArray();
        return buildings;
    }

    /**<summary> Parse and return Area array from json </summary>*/
    public static Area[] ParseAreas(string json)
    {
        Area[] areas = JsonConvert.DeserializeObject<List<Area>>(json).ToArray();
        return areas;
    }

    /**<summary> Parse and return Item from json </summary>*/
    public static ItemData ParseItem(string json)
    {
        json = json.Replace("type", "$type");
        // Move $type to first for item (json.net requirement for inherited typecasting)
        JObject jItem = JObject.Parse(json);
        JToken temp = jItem["$type"];
        jItem["$type"].Parent.Remove();
        jItem.First.AddBeforeSelf(new JProperty("$type", temp.Value<string>()));
        json = jItem.ToString();

        ItemData item = JsonConvert.DeserializeObject<ItemData>(json, ServerAPI.deserialise);
        return item;
    }

    /**<summary> Parse and return Item array from json </summary>*/
    public static List<ItemData> ParseItems(string json)
    {
        json = json.Replace("type", "$type");
        // Move $type to first for every item (json.net requirement for inherited typecasting)
        JArray itemArray = JArray.Parse(json);
        foreach (JObject item in itemArray)
        {
            JToken temp = item["$type"];
            item["$type"].Parent.Remove();
            item.First.AddBeforeSelf(new JProperty("$type", temp.Value<string>()));
        }
        json = itemArray.ToString();

        List<ItemData> items = JsonConvert.DeserializeObject<List<ItemData>>(json, ServerAPI.deserialise);
        return items;
    }

    /**<summary> Parse and return Object array from json </summary>*/
    public static Game[] ParseGames(string json)
    {
        Game[] games = JsonConvert.DeserializeObject<List<Game>>(json, ServerAPI.deserialise).ToArray();
        return games;
    }

    /**<summary> Structure used for representing and (de)serializing game json </summary>*/
    public struct Game
    {
        public string id;
        public string name;
        public string description;
        public object properties;

        override public string ToString()
        {
            return "id: " + id + "\nname: " + name + "\ndescription: " + description + "\nproperties: " + properties;
        }
    }

    /**<summary> Structure used for representing and (de)serializing buildings json </summary>*/
    public struct Building
    {
        public string buildingID;
        public string areaID;
        public string alias;
        public string name;
        public string description;
        [JsonConverter(typeof(CoordinatesToVectorConverter))]
        public Vector2 location;

        override public string ToString ()
        {
            return "buildingID: " + buildingID + "\nareaID: " + areaID + "\nalias: " + alias + "\nname: " + name + "\ndescription: " + description + "\nlocation: " + location;
        }
    }

    /**<summary> Structure used for representing and (de)serializing areas json </summary>*/
    public struct Area
    {
        public string areaID;
        public string alias;
        public string name;
        public string description;
        public string type;
        //public int floorOrder;
        [JsonConverter(typeof(VectorConverter))]
        public Vector3 center;
        public Bounds bounds;

        override public string ToString()
        {
            return "areaID: " + areaID + "\nalias: " + alias + "\nname: " + name + "\ndescription: " + description + "\ntype: " + type + "\ncenter: " + center + "\nbounds\n" + bounds;
        }
    }

    /**<summary> Structure used for diagonal corner bounds </summary>*/
    public struct Bounds
    {
        [JsonConverter(typeof(VectorConverter))]
        public Vector3 topLeft;
        [JsonConverter(typeof(VectorConverter))]
        public Vector3 bottomRight;

        public float area
        {
            get { return Mathf.Abs(bottomRight.x - topLeft.x) * Mathf.Abs(bottomRight.z - topLeft.z); }
        } 

        override public string ToString()
        {
            return "topLeft: " + topLeft + "\nbottomRight: " + bottomRight;
        }

        /**<summary> Get UnityEngine.Bounds from Parser.Bounds </summary>*/
        public UnityEngine.Bounds GetUnityBounds()
        {
            UnityEngine.Bounds bounds = new UnityEngine.Bounds();
            bounds.min = topLeft;
            bounds.max = bottomRight;
            return bounds;
        }
    }
}


