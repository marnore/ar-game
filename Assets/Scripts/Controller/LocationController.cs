using MaterialUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static Parser;

public class LocationController : MonoBehaviour {

    [SerializeField] [Tooltip("Limit in kilometers for detecting closest building")] private float distanceLimit = 0.5f;

    internal Building[] buildings;
    internal Area[] areas;
    internal string buildingID;
    internal string areaID;
    
    private ARController arController;
    private WorldController worldController;

    private void Start()
    {
        arController = FindObjectOfType<ARController>();
        worldController = FindObjectOfType<WorldController>();
        ServerAPI.OnAreasResponse += OnAreasResponse;
        ServerAPI.OnGPSResponse += OnGPSResponse;

        ServerAPI.UpdateGPSLocation();
        // -> OnGPSResponse
#if UNITY_EDITOR
        //Buildings();
#endif
    }

    /**<summary> Get distance between two locations (lat, long) in Kilometers </summary>*/
    private float DistanceBetween(Vector2 location1, Vector2 location2)
    {
        double rlat1 = Math.PI * location1.x / 180;
        double rlat2 = Math.PI * location2.x / 180;
        double theta = location1.y - location2.y;
        double rtheta = Math.PI * theta / 180;
        double dist = Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) * Math.Cos(rlat2) * Math.Cos(rtheta);
        dist = Math.Acos(dist);
        dist = dist * 180 / Math.PI;
        dist = dist * 60 * 1.1515;
        return (float)(dist * 1.609344);
    }

    /**<summary> Called when Location response got from GPS </summary>*/
    private async void OnGPSResponse(bool success, LocationInfo locationInfo)
    {
        // Try to get buildings
        await Buildings();

        if (!success || buildingID.IsNullOrEmpty())
        {
            SelectLocationDialog();
        }
    }

    /**<summary> Show location selection dialog </summary>*/
    public void SelectLocationDialog()
    {
        DialogManager.ShowSimple("Select building manually", buildings.Select(b => b.name).ToArray(), !buildingID.IsNullOrEmpty(),
            async (int selectedIndex) => {
            NativeMethods.ShowToast(buildings[selectedIndex].name + " selected");

            buildingID = buildings[selectedIndex].buildingID;
            Tuple<bool, Area[]> areasResult = await ServerAPI.Areas(buildingID);
            OnAreasResponse(areasResult.Item1, areasResult.Item2);
        });
    }

    /**<summary> Get buildings from Location API </summary>*/
    private async Task Buildings()
    {
        Tuple<bool, Building[]> result = new Tuple<bool, Building[]>(false, null);
        result = await ServerAPI.Buildings();
        buildings = result.Item2;
        Vector2 currentLocation = ServerAPI.LastGPSLocation();

        Building closestBuilding = new Building();
        float closestDistance = distanceLimit;
        // Get closest building compared to gps location
        Array.ForEach(buildings, (Building b) => 
        {
            float distance = DistanceBetween(currentLocation, b.location);
            if (distance < closestDistance)
            {
                closestBuilding = b;
                closestDistance = distance;
            }
        });

        if (App.config.debug)
            Debug.Log("Closest building\n" + closestBuilding + " with distance: " + closestDistance);

        if (closestDistance <= distanceLimit)
        {
            buildingID = closestBuilding.buildingID;
            // Get areas of building
            Tuple<bool, Area[]> areasResult = await ServerAPI.Areas(buildingID);
            OnAreasResponse(areasResult.Item1, areasResult.Item2);
        }
        else
        {
            NativeMethods.ShowToast("Couldn't find building within " + distanceLimit * 100 + " meters");
            SelectLocationDialog();
        }
    }

    /**<summary> Called when areas response got </summary>*/
    private void OnAreasResponse(bool success, Area[] areas)
    {
        if (App.config.debug)
        {
            Array.ForEach(areas, (Area a) =>
            {
                Debug.Log(a);
#if UNITY_EDITOR
                areaID = a.areaID;
#endif
            });
        }

        this.areas = areas;

        NativeMethods.ShowToast("Point your device forward to locate accurate position");

        arController.StartTracking();
        ServerAPI.OnLocationResponse -= OnLocationResponse;
        ServerAPI.OnLocationResponse += OnLocationResponse;
        // Enable arController auto update
#if !UNITY_EDITOR
        arController.AutomaticLocationSync = true;
#endif
    }

    /**<summary> Called when response is got from image based Location API server </summary>*/
    private async void OnLocationResponse(bool locationFound, LocationData locationData)
    {
        if (locationFound)
        {
            if (App.config.debug)
                Debug.Log("Location detected -> initialise world");
            ServerAPI.OnLocationResponse -= OnLocationResponse;

            if (!locationData.areaID.IsNullOrEmpty())
                areaID = locationData.areaID;

            await Task.Delay(500);
            worldController.InitArea(Array.Find(areas, a => a.areaID == areaID));
        }
    }

}
