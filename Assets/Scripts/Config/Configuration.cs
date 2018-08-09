using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "App Config.asset", menuName = "App Configuration")]
public class Configuration : ScriptableObject
{
    public bool debug;
    public string apiURL;
    public float locationRefreshRate = 2f;
}

