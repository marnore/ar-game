using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class App : MonoBehaviour {

    [SerializeField] private Configuration appConfig;

    public static Configuration config;

    private void Awake()
    {
        config = appConfig;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
    }
}
