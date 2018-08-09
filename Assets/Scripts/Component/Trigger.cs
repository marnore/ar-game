using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Use to forward Unity trigger events to another class </summary>*/
public class Trigger : MonoBehaviour
{
    [SerializeField] private MonoBehaviour target;

    private void OnTriggerEnter(Collider other)
    {
        target.SendMessage("OnTriggerEnter", other, SendMessageOptions.DontRequireReceiver);
    }

    private void OnTriggerExit(Collider other)
    {
        target.SendMessage("OnTriggerExit", other, SendMessageOptions.DontRequireReceiver);
    }

}
