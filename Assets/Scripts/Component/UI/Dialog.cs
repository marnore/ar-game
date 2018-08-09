using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

/**<summary> Dialog component base class </summary>*/
public class Dialog : MonoBehaviour
{
    [SerializeField] private bool darkBackground = true;
    [SerializeField] private RectTransform background;

    internal bool cancellable = true;

    private RectTransform tr;
    private CanvasGroup canvasGroup, backroundCanvasGroup;

    private void Awake()
    {
        tr = gameObject.GetComponent<RectTransform>();
        backroundCanvasGroup = background.GetComponent<CanvasGroup>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        background.sizeDelta = new Vector2(Screen.width, Screen.height * 3f);
        background.SetParent(tr.parent, false);
        background.SetSiblingIndex(background.GetSiblingIndex()-1);
    }

    /**<summary> Setup dialog </summary>*/
    public void Initialize()
    {
        Open();
    }

    /**<summary> Open dialog with animation </summary>*/
    public void Open()
    {
        if (darkBackground)
            DOTween.To(() => backroundCanvasGroup.alpha, x => backroundCanvasGroup.alpha = x, 0.5f, 0.25f);
        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, 0.2f);
        tr.DOScale(1, 0.2f);
        backroundCanvasGroup.blocksRaycasts = true;
    }

    /**<summary> Cancel dialog (close if cancellable) </summary>*/
    public void Cancel()
    {
        if (cancellable)
        {
            Close();
        }
    }

    /**<summary> Close dialog with animation </summary>*/
    public void Close()
    {
        if (darkBackground)
            DOTween.To(() => backroundCanvasGroup.alpha, x => backroundCanvasGroup.alpha = x, 0, 0.12f).OnComplete(() =>
            {
                Destroy(background.gameObject);
                Destroy(gameObject);
            });
        tr.DOScale(0.8f, 0.12f);
        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, 0.12f);
        backroundCanvasGroup.blocksRaycasts = false;
    }
}
