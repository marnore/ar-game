using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**<summary> Copy height of another rect transform </summary>*/
public class CopyHeight : MonoBehaviour
{
    [SerializeField] internal float maxHeight;
    [SerializeField] private RectTransform target;
    private LayoutElement layout;

    private void Start()
    {
        layout = GetComponent<LayoutElement>();
    }

    private void Update()
    {
        if (layout.preferredHeight != target.sizeDelta.y)
            layout.preferredHeight = Mathf.Min(target.sizeDelta.y, maxHeight);
	}
}
