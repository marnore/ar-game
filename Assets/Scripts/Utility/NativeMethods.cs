using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Native Android methods </summary>*/
public class NativeMethods {

    /**<summary> Show toast message </summary>*/
    public static void ShowToast (string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject unityActivity = GetUnityActivity();

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toast.Call("show");
            }));
        }
#endif
    }
    /**<summary> Get Unity native activity </summary>*/
#if UNITY_ANDROID
    private static AndroidJavaObject GetUnityActivity()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }
#endif

}
