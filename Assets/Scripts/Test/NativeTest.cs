using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NativeTest : MonoBehaviour {

	public void TestToast (string message)
    {
        NativeMethods.ShowToast(message);
    }
}
