using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static Parser;
using System.IO;
using System;

/**<summary> <summary> Test script for API </summary>*/
public class APITest : MonoBehaviour {

    [SerializeField] private string apiUrl = "http://saimaa.netlab.hut.fi:5004";
    [SerializeField] private string imagePath;
    [SerializeField] private string testJsonPath;
    [SerializeField] private string buildingID;

    /**<summary> <summary> Test file loading from local test image </summary>*/
    public void FileTest ()
    {
        if (FileIO.GetFile(imagePath).Length > 100)
            Debug.Log("Read succesfully: " + imagePath);
        else
            Debug.Log("Couldn't read: " + imagePath);

    }

    /**<summary> <summary> Test parsing of local test json </summary>*/
    public void ParseTest()
    {
        LocationData locationData = ParseLocation(System.Text.Encoding.UTF8.GetString(FileIO.GetFile(testJsonPath)));
        Debug.Log(locationData);
    }

    public void ImageTest()
    {
        //bitmap.RotateFlip(RotateFlipType.Rotate270FlipX);
        //bitmap.Save(Application.persistentDataPath.Combine("img.jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
    }

    public async void Buildings()
    {
        Tuple<bool, Building[]> result = await ServerAPI.Buildings();
        Array.ForEach(result.Item2, x => Debug.Log(x.ToString()));
    }

    /**<summary> <summary> Get location from image </summary>*/
    public void LocationFine()
    {
        if (imagePath.Contains(".jpg") || imagePath.Contains(".jpeg"))
            LocationFine(buildingID, imagePath);
        else
        {
            foreach (string file in Directory.GetFiles(imagePath))
                LocationFine(buildingID, file);
        }
    }

    /**<summary> <summary> Get location from image </summary>*/
    public void LocationFine(string buildingID, string imagePath)
    {
        StartCoroutine(_LocationFine(buildingID, imagePath));
    }

    /**<summary> <summary> Get location from image Coroutine </summary>*/
    private IEnumerator _LocationFine (string buildingID, string imagePath)
    {
        Debug.Log("Starting /location/fine POST request for " + imagePath);
        byte[] image = FileIO.GetFile(imagePath);
        WWWForm formData = new WWWForm();
        formData.AddField("buildingID", buildingID);
        formData.AddField("useVice", "true");
        formData.AddBinaryData("image", image);

        UnityWebRequest www = UnityWebRequest.Post(apiUrl + "/location/fine", formData);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            Debug.Log(imagePath + " - " + www.downloadHandler.text);
        }
        else
        {
            Debug.Log(imagePath + " - " + www.downloadHandler.text);
            LocationData locationData = ParseLocation(www.downloadHandler.text);
            Debug.Log(locationData);
        }
    }
}
