using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedItem : MonoBehaviour
{
    [SerializeField] Text nameText;

    private string _name;
    new public string name
    {
        get { return _name; }

        set
        {
            _name = value;
            nameText.text = _name;
        }
    }
}
