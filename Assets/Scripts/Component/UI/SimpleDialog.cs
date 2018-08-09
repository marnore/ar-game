using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**<summary> Simple List Dialog </summary>*/
public class SimpleDialog : Dialog
{
    [SerializeField] private Text title;
    [SerializeField] private RectTransform listContent;
    [SerializeField] private RectTransform optionTemplate;
    private bool closeOnSelect = true;

    /**<summary> Setup dialog, create list from options </summary>*/
    public void Initialize(string titleText, string[] options, bool cancellable, bool closeOnSelect, Action<int> onItemClick)
    {
        foreach(Transform c in listContent.GetComponentInChildren<Transform>())
        {
            Destroy(c.gameObject);
        }

        this.closeOnSelect = closeOnSelect;
        this.cancellable = cancellable;
        title.text = titleText;

        for (int i = 0; i < options.Length; i++)
        {
            Button button = Instantiate(optionTemplate, listContent).GetComponent<Button>();
            button.GetComponentInChildren<Text>().text = options[i];
            int index = i;
            button.onClick.AddListener(() => OnItemClick(onItemClick, index));
        }

        listContent.GetComponentInParent<CopyHeight>().maxHeight = DialogManager.tr.rect.height - 256f;
        Open();
    }

    public void OnItemClick(Action<int> onItemClick, int index)
    {
        onItemClick?.Invoke(index);
        if (closeOnSelect)
            Close();
    }
}
