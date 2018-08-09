using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Energy collectable, autocollecting on proximity </summary>*/
public class Energy : Collectable
{
    /**<summary> Called when trigger is entered, Pickup Item </summary>*/
    public void OnTriggerEnter(Collider col)
    {
        Pickup(col.attachedRigidbody.gameObject);
    }

    /**<summary> Collect the Energy Item </summary>*/
    override public void Interact(GameObject caller)
    {
        Pickup(caller);
    }

    /**<summary> Send message to collector (player) </summary>*/
    override public void PickupMessage(GameObject collector)
    {
        collector.SendMessage("OnEnergyCollected", data, SendMessageOptions.DontRequireReceiver);
    }
}
