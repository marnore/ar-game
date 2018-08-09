using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**<summary> Show information about Items in sight </summary>*/
public class ItemSightInformation : MonoBehaviour
{
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private TextMeshProUGUI title, type, description, value;

    private Transform tr, camTr;
    private ItemData lastItem;

	private void Start ()
    {
        tr = transform;
        camTr = Camera.main.transform;
	}

    void Update()
    {
        RaySight();
    }

	/**<summary> Check forward to sight direction and show information about Items </summary>*/
    private void RaySight()
    {
        RaycastHit hit;
        if (Physics.Raycast(camTr.position, camTr.forward, out hit, 6, hitLayers))
        {
            Item item = hit.collider.GetComponent<Item>();
            if (item)
            {
                tr.position = hit.point;
                if (item.data == lastItem)
                    return;
                lastItem = item.data;
                SetStats(item.data);
            }
        }
    }

    /**<summary> Set stats of the UI component </summary>*/
    public void SetStats(ItemData item)
    {
        title.text = item.name;
    }
}
