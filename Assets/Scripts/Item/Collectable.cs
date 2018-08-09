using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

/**<summary> Base class for Collectable Item </summary>*/
public class Collectable : Item
{
    [SerializeField] internal float collectTime = 0.5f;

    /**<summary> Show Item information and available actions </summary>*/
    override public void Interact(GameObject caller)
    {
        DialogManager.ShowAlert(data.name,
            data.description +
            "\nvalue: " + data.value,
            true,
            new DialogManager.DialogButton("Close", () => { }),
            new DialogManager.DialogButton("Pickup", () =>
            {
                Pickup(caller);
            })
        );
    }

    override public int Use()
    {
        Debug.Log("This item cannot be used");
        return 0;
    }

    /**<summary> Pickup this collectable </summary>*/
    override public async void Pickup(GameObject collector)
    {
        PickupMessage(collector);
        transform.DOMove(collector.transform.position, collectTime);
        await Task.Delay((int)(collectTime * 1000));
        Destroy();
    }

    /**<summary> Send message to collector (player) </summary>*/
    virtual public void PickupMessage (GameObject collector)
    {
        collector.SendMessage("OnItemCollected", data, SendMessageOptions.DontRequireReceiver);
    }
}
