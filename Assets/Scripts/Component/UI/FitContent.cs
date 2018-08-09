using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FitContent : MonoBehaviour {

    [SerializeField] internal float maxWidth, maxHeight, horizontalPadding, verticalPadding;
    [SerializeField] private RectTransform target;
    private LayoutElement layout;
	
	private void Awake ()
    {
        layout = GetComponent<LayoutElement>();
	}

    private void Update ()
    {
        if (layout.preferredWidth != target.sizeDelta.x && maxWidth > 0)
            layout.preferredWidth = Mathf.Min(target.sizeDelta.x, maxWidth) + horizontalPadding * 2;
        if (layout.preferredHeight != target.sizeDelta.y && maxHeight > 0)
            layout.preferredHeight = Mathf.Min(target.sizeDelta.y, maxHeight) + verticalPadding * 2;
    }
}
