using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Lock rotation of the object to start rotation </summary>*/
public class LockRotation : MonoBehaviour {

    private Transform tr;
    private Quaternion originalRotation;

    private void Start()
    {
        tr = transform;
        originalRotation = tr.rotation;
    }

    private void Update()
    {
        tr.rotation = originalRotation;
    }
}
