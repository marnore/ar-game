using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using static Parser;

/**<summary> Dummy server implementation for testing purposes </summary>*/
public class DummyServer : MonoBehaviour {



    private static List<ItemData> objects = new List<ItemData>();

    public static void SavePlayer(string id, string playerDataJson)
    {
        File.WriteAllText(Application.persistentDataPath.Combine("Player" + id + ".json"), playerDataJson);
    }

    public static string GetPlayer(string id)
    {
        if (File.Exists(Application.persistentDataPath.Combine("Player" + id + ".json")))
        {
            return File.ReadAllText(Application.persistentDataPath.Combine("Player" + id + ".json"));
        }
        else
        {
            return null;
        }
    }

    public static void SaveObjects()
    {
        File.WriteAllText(Application.persistentDataPath.Combine("Objects.json"), JsonConvert.SerializeObject(objects.ToArray(), ServerAPI.serialise));
    }

    public static void LoadObjects()
    {
        if (File.Exists(Application.persistentDataPath.Combine("Objects.json")))
        {
            objects.Clear();
            objects.AddRange(ParseItems(File.ReadAllText(Application.persistentDataPath.Combine("Objects.json"))));
        }
    }
    
    public static string GetItems(string buildingID, string areaID)
    {
        if (objects.Count == 0)
            LoadObjects();
        return JsonConvert.SerializeObject(objects, ServerAPI.serialise);
    }

    public static void SetItem(string json)
    {
        objects.Add(JsonConvert.DeserializeObject<ItemData>(json, ServerAPI.deserialise));
        SaveObjects();
    }

    public static void SetObjects(string json)
    {
        objects.AddRange(JsonConvert.DeserializeObject<List<ItemData>>(json, ServerAPI.deserialise));
        SaveObjects();
    }

    public static void DeleteObjects()
    {
        objects.Clear();
    }
}
