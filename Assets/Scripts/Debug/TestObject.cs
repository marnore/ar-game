using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Script for managing instantiated test objects </summary>*/
public class TestObject : MonoBehaviour
{
    private WorldSpaceLabel label;
    private Transform tr;

    private void Start()
    {
        tr = transform;
        label = GetComponentInChildren<WorldSpaceLabel>();
    }

    private void Update()
    {
        string text = tr.position.ToString("f1");
        if (label.text != text)
            label.text = text;
    }
}
