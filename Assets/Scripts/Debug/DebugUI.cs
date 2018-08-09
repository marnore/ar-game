using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Text positionDebugText;
    private LocationController locationController;

    private void Start()
    {
        locationController = FindObjectOfType<LocationController>();

        ServerAPI.OnLocationResponse += OnLocationResponse;
    }

    private void Update()
    {
        positionDebugText.text =
            "Position: " + mainCamera.position +
            "\nRotation: " + mainCamera.rotation.eulerAngles;
        if (locationController.buildings != null && locationController.areas != null)
            positionDebugText.text += "\nBuilding / Area: " + Array.Find(locationController.buildings, b => b.buildingID == locationController.buildingID).name + " - " + Array.Find(locationController.areas, a => a.areaID == locationController.areaID).name;
    }

    private void OnLocationResponse (bool locationFound, LocationData locationData)
    {

    }

}
