using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DialogManager;

/**<summary> Simple List Dialog </summary>*/
public class AlertDialog : Dialog
{
    [SerializeField] private Text title;
    [SerializeField] private Text message;
    [SerializeField] private RectTransform buttonContainer;
    [SerializeField] private RectTransform buttonTemplate;

    /**<summary> Setup dialog, create alert with buttons </summary>*/
    public void Initialize(string title, string content, bool cancellable, params DialogButton[] buttons)
    {
        this.cancellable = cancellable;
        this.title.text = title;
        this.message.text = content;

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = Instantiate(buttonTemplate, buttonContainer).GetComponent<Button>();
            button.GetComponentInChildren<Text>().text = buttons[i].text;
            int index = i;
            button.onClick.AddListener(() => OnClick(buttons[index].action));
        }

        Open();
    }

    public void OnClick(Action onClick)
    {
        onClick?.Invoke();

        Close();
    }
}
