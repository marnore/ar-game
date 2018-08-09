using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Create and show dialogs </summary>*/
public class DialogManager : MonoBehaviour {

    public static RectTransform tr;

    /**<summary> Data structure for buttons in dialogs with title/text and action on click </summary>*/
    public struct DialogButton
    {
        public string text;
        public Action action;

        /**<summary> Create new DialogButton </summary>*/
        public DialogButton(string text, Action action)
        {
            this.text = text;
            this.action = action;
        }
    }

    private void Awake()
    {
        tr = GetComponent<RectTransform>();
    } 

    /**<summary> Show Simple Dialog list </summary>*/
    public static SimpleDialog ShowSimple(string titleText, string[] options, bool cancellable, Action<int> onItemClick)
    {
        return ShowSimple(titleText, options, cancellable, true, onItemClick);
    }

    /**<summary> Show Simple Dialog list </summary>*/
    public static SimpleDialog ShowSimple(string titleText, string[] options, bool cancellable, bool closeOnSelect, Action<int> onItemClick)
    {
        SimpleDialog dialog = Instantiate((GameObject)Resources.Load("SimpleDialog"), tr).GetComponent<SimpleDialog>();
        dialog.Initialize(titleText, options, cancellable, closeOnSelect, onItemClick);
        return dialog;
    }

    /**<summary> Show Alert Dialog with buttons </summary>*/
    public static AlertDialog ShowAlert(string titleText, string content, bool cancellable, params DialogButton[] buttons)
    {
        AlertDialog dialog = Instantiate((GameObject)Resources.Load("AlertDialog"), tr).GetComponent<AlertDialog>();
        dialog.Initialize(titleText, content, cancellable, buttons);
        return dialog;
    }
}
