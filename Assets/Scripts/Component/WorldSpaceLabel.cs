using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/**<summary> A label that faces towards camera in world space </summary>*/
public class WorldSpaceLabel : MonoBehaviour
{
    [SerializeField] private string _text;
    
    private Transform tr, target;
    private TextMeshPro textMesh;

    private void Awake()
    {
        tr = transform;
        target = Camera.main.transform;
        textMesh = GetComponent<TextMeshPro>();
        //UpdateText();
    }

    private void Update()
    {
        tr.LookAt(tr.position + target.rotation * Vector3.forward, target.rotation * Vector3.up);
    }

    /**<summary> Text of the label </summary>*/
    public string text
    {
        get { return _text; }
        set
        {
            _text = text;
            UpdateText();
        }
    }

    /**<summary> Update actual text component of the label </summary>*/
    private void UpdateText()
    {
        textMesh.text = text;
    }

}
