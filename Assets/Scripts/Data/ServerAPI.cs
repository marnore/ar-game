using GoogleARCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static Parser;

/**<summary> Intermediate API for image based location server and other querys </summary>*/
public class ServerAPI : MonoBehaviour
{

    /**<summary> Settings for Deserialising </summary>*/
    public static JsonSerializerSettings deserialise
    {
        get
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;
            settings.Binder = new SerializationBinder();
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            return settings;
        }
    }

    /**<summary> Settings for Serialising </summary>*/
    public static JsonSerializerSettings serialise
    {
        get
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Binder = new SerializationBinder();
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            return settings;
        }
    }

    /**<summary> Get last known GPS location. Update location before calling! </summary>*/
    public static Vector2 LastGPSLocation()
    {
        return new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);
    }

    /**<summary> Update current GPS location </summary>*/
    public static void UpdateGPSLocation ()
    {
        AndroidPermissionsManager.RequestPermission("android.permission.ACCESS_FINE_LOCATION").ThenAction((grantResult) =>
        {
            UpdateGPSLocationA();
        });
    }

    /**<summary> Update current GPS location </summary>*/
    private static async void UpdateGPSLocationA()
    {
        if (!Input.location.isEnabledByUser)
        {
            NativeMethods.ShowToast("Unable to get GPS location");
            OnGPSResponse?.Invoke(false, new LocationInfo());
            return;
        }

        Input.location.Start(10, 0.2f);

        while (Input.location.status == LocationServiceStatus.Initializing)
            await Task.Delay(500);

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            NativeMethods.ShowToast("Unable to get GPS location");
            OnGPSResponse?.Invoke(false, new LocationInfo());
        }
        else
        {
            Input.location.Stop();
            OnGPSResponse?.Invoke(true, Input.location.lastData);
        }
    }

    /**<summary> Get available buildings </summary>*/
    public static async Task<Tuple<bool, Building[]>> Buildings()
    {
        UnityWebRequest www = UnityWebRequest.Get(App.config.apiURL + "/buildings");
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SendWebRequest();

        while (!www.isDone)
            await Task.Delay(100);

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
            return new Tuple<bool, Building[]>(false, new Building[0]);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            Building[] buildings = ParseBuildings(www.downloadHandler.text);
            return new Tuple<bool, Building[]>(true, buildings);
        }
    }

    /**<summary> Get available buildings </summary>*/
    public static async Task<Tuple<bool, Area[]>> Areas(string buildingID)
    {
        UnityWebRequest www = UnityWebRequest.Get(App.config.apiURL + "/buildings/" + buildingID + "/areas");
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SendWebRequest();

        while (!www.isDone)
            await Task.Delay(100);

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
            return new Tuple<bool, Area[]>(false, new Area[0]);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            Area[] areas = ParseAreas(www.downloadHandler.text);
            return new Tuple<bool, Area[]>(true, areas);
        }
    }

    /**<summary> Get location from image </summary>*/
    public static async Task<Tuple<bool, LocationData>> LocationFine(string buildingID, byte[] image)
    {
#if UNITY_EDITOR
        //return await LocationFineDummy(buildingID, image);
#endif
        if (App.config.debug)
            Debug.Log("Starting location/fine request for buildingID: " + buildingID);
        WWWForm formData = new WWWForm();
        formData.AddField("buildingID", buildingID);
        formData.AddField("useVise", "true");
        formData.AddBinaryData("image", image);

        UnityWebRequest www = UnityWebRequest.Post(App.config.apiURL + "/location/fine", formData);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SendWebRequest();
        
        while (!www.isDone)
            await Task.Delay(100);

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
            OnLocationResponse?.Invoke(false, new LocationData());
            return new Tuple<bool, LocationData>(false, new LocationData());
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            LocationData locationData = ParseLocation(www.downloadHandler.text);
            OnLocationResponse?.Invoke(true, locationData);
            return new Tuple<bool, LocationData>(true, locationData);
        }
    }

    /**<summary> Get location from image </summary>*/
    public static async Task<Tuple<bool, LocationData>> LocationFineDummy(string buildingID, byte[] image)
    {
        if (App.config.debug)
            Debug.Log("Starting dummy location/fine request for buildingID: " + buildingID);

        await Task.Delay(100);
        LocationData locationData = new LocationData(new Vector3(-30, 1.6f, 20), Vector3.forward, Quaternion.identity);
        OnLocationResponse?.Invoke(true, locationData);
        return new Tuple<bool, LocationData>(true, locationData);
    }

    /**<summary> Get obstacles mesh for area </summary>*/
    public static async Task<Tuple<bool, byte[]>> Obstacles(string areaID)
    {
        UnityWebRequest www = UnityWebRequest.Get(App.config.apiURL + "/areas/" + areaID + "/obstacles");
        www.SetRequestHeader("Accept", "string");
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SendWebRequest();

        while (!www.isDone)
            await Task.Delay(100);

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
            return new Tuple<bool, byte[]>(false, new byte[0]);
        }
        else
        {
            //Debug.Log(www.downloadHandler.text);
            return new Tuple<bool, byte[]>(true, www.downloadHandler.data);
        }
    }

    /**<summary> Get Items from server </summary>*/
    public static async Task<Tuple<bool, List<ItemData>>> GetItems(string gameID)
    {
        UnityWebRequest www = UnityWebRequest.Get(App.config.apiURL + "/games/" + gameID + "/items");
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SendWebRequest();

        while (!www.isDone)
            await Task.Delay(100);

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
            OnGetItemsResponse?.Invoke(false, null);
            return new Tuple<bool, List<ItemData>>(false, null);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            List<ItemData> items = ParseItems(www.downloadHandler.text);
            OnGetItemsResponse?.Invoke(true, items);
            return new Tuple<bool, List<ItemData>>(true, items);
        }
    }

    /**<summary> Get Items from server </summary>*/
    public static async Task<Tuple<bool, List<ItemData>>> GetItemsDummy(string buildingID, string areaID)
    {
        await Task.Delay(10);
        List<ItemData> items = ParseItems(DummyServer.GetItems(buildingID, areaID));
        OnGetItemsResponse?.Invoke(true, items);
        return new Tuple<bool, List<ItemData>>(true, items);
    }


    /**<summary> Set Item to server </summary>*/
    public static async Task<Tuple<bool, ItemData>> SetItem(string gameID, ItemData item)
    {
        UnityWebRequest www = new UnityWebRequest(App.config.apiURL + "/games/" + gameID + "/items", UnityWebRequest.kHttpVerbPOST);
        www.SetRequestHeader("Content-Type", "application/json");
        www.downloadHandler = new DownloadHandlerBuffer();
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item, serialise).Replace("$type", "type")));
        www.SendWebRequest();
        print(JsonConvert.SerializeObject(item, serialise).Replace("$type", "type"));

        while (!www.isDone)
            await Task.Delay(100);

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
            OnSetItemResponse?.Invoke(false, null);
            return new Tuple<bool, ItemData>(false, null);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            ItemData responseItem = ParseItem(www.downloadHandler.text);
            OnSetItemResponse?.Invoke(true, responseItem);
            return new Tuple<bool, ItemData>(true, responseItem);
        }
    }

    /**<summary> Set Item to server </summary>*/
    public static async Task<bool> SetItemDummy(string gameID, ItemData item)
    {
        await Task.Delay(10);
        string json = JsonConvert.SerializeObject(item, serialise);
        print(json);
        DummyServer.SetItem(json);
        OnSetItemResponse?.Invoke(true, item);
        return true;
    }

    /**<summary> Set Items to server </summary>*/
    public static async Task<bool> SetItems(string gameID, ItemData[] items)
    {
        int itemsSet = 0;
        foreach (ItemData item in items)
        {
            if (await SetItemDummy(gameID, item))
                itemsSet++;
        }
        OnSetItemsResponse?.Invoke(itemsSet == items.Length);
        return itemsSet == items.Length;
    }

    /**<summary> Set Items to dummy server </summary>*/
    public static async Task<bool> SetItemsDummy(string buildingID, string areaID, ItemData[] items)
    {
        await Task.Delay(10);
        string json = JsonConvert.SerializeObject(items, serialise);
        DummyServer.SetObjects(json);
        OnSetItemsResponse?.Invoke(true);
        return true;
    }

    /**<summary> Delete Item from server </summary>*/
    public static async Task<bool> DeleteItem(string gameID, string itemID)
    {
        UnityWebRequest www = UnityWebRequest.Delete(App.config.apiURL + "/games/" + gameID + "/items/" + itemID);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SendWebRequest();

        while (!www.isDone)
            await Task.Delay(100);

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
            OnDeleteItemResponse?.Invoke(false);
            return false;
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            OnDeleteItemResponse?.Invoke(true);
            return true;
        }
    }

    /**<summary> Delete Items from server </summary>*/
    public static async Task<bool> DeleteItemsDummy(string buildingID, string areaID)
    {
        await Task.Delay(10);
        DummyServer.DeleteObjects();
        OnDeleteItemsResponse?.Invoke(true);
        return true;
    }

    /**<summary> Get available games from server </summary>*/
    public static async Task<Tuple<bool, Game[]>> GetGames()
    {
        UnityWebRequest www = UnityWebRequest.Get(App.config.apiURL + "/games");
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SendWebRequest();

        while (!www.isDone)
            await Task.Delay(100);

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
            OnGetGamesResponse?.Invoke(false, null);
            return new Tuple<bool, Game[]>(false, null);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            Game[] games = ParseGames(www.downloadHandler.text);
            OnGetGamesResponse?.Invoke(true, games);
            return new Tuple<bool, Game[]>(true, games);
        }
    }

    /**<summary> Get player data from server </summary>*/
    public static async Task<string> GetPlayer(string id)
    {
        await Task.Delay(10);
        return DummyServer.GetPlayer(id);
    }

    /**<summary> Save player data to server </summary>*/
    public static async Task SavePlayer(string id, object data)
    {
        await Task.Delay(10);
        Debug.Log(JsonConvert.SerializeObject(data, serialise));
        DummyServer.SavePlayer(id, JsonConvert.SerializeObject(data, serialise));
    }

    private void Awake ()
    {
        // null static delegates to prevent duplicate subscriptions
        OnLocationResponse = null;
        OnBuildingsResponse = null;
        OnAreasResponse = null;
        OnObstaclesResponse = null;
        OnGPSResponse = null;
        OnGetItemsResponse = null;
        OnSetItemsResponse = null;
        OnSetItemResponse = null;
        OnDeleteItemsResponse = null;
    }

    /**<summary> Callback function when LocationFine request completes </summary>*/
    public static Action<bool, LocationData> OnLocationResponse;

    /**<summary> Callback function when Buildings request completes </summary>*/
    public static Action<bool, Building[]> OnBuildingsResponse;

    /**<summary> Callback function when Areas request completes </summary>*/
    public static Action<bool, Area[]> OnAreasResponse;

    /**<summary> Callback function when Obstacles request completes </summary>*/
    public static Action<bool, byte[]> OnObstaclesResponse;

    /**<summary> Callback function when GPSLocation completes </summary>*/
    public static Action<bool, LocationInfo> OnGPSResponse;

    /**<summary> Callback function when GetItems completes </summary>*/
    public static Action<bool, List<ItemData>> OnGetItemsResponse;

    /**<summary> Callback function when SetItem completes </summary>*/
    public static Action<bool, ItemData> OnSetItemResponse;

    /**<summary> Callback function when SetItems completes </summary>*/
    public static Action<bool> OnSetItemsResponse;

    /**<summary> Callback function when DeleteItems completes </summary>*/
    public static Action<bool> OnDeleteItemsResponse;

    /**<summary> Callback function when DeleteItem completes </summary>*/
    public static Action<bool> OnDeleteItemResponse;

    /**<summary> Callback function when GetGames completes </summary>*/
    public static Action<bool, Game[]> OnGetGamesResponse;
}
