using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastHeight : MonoBehaviour {

    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float minChange = 0.01f;
    [SerializeField] private float maxChange = 0.5f;
    private Transform tr;

    private void Awake()
    {
        tr = transform;
    }

    /**<summary> Check and set height on plane hit position </summary>*/
    public void SetHeight ()
    {
        tr.position = Raycast(maxChange);
    }

    /**<summary> Raycast vertically and return possible plane hit position </summary>*/
    public Vector3 Raycast (float maxChange)
    {
        RaycastHit hit;
        if (Physics.Raycast(tr.position + Vector3.up, Vector3.down, out hit, 3, layerMask))
        {
            float change = Mathf.Abs(tr.position.y - hit.point.y);
            ItemData item = GetComponent<Item>().data;
            if ((!item || (item && item.local)) && (change >= minChange && change <= maxChange))
            {
                if (item && item.local)
                    item.position = hit.point;
                return hit.point;
            }
        }
        return tr.position;
    }

}
