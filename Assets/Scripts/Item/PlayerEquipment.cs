using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**<summary> Base class for Player Equipment Items (rackets, shoes etc.) </summary>*/
public abstract class PlayerEquipment : Collectable
{
    internal EquipmentData equipmentData;

    private float useTime;

    virtual internal void Start()
    {
        equipmentData = (EquipmentData)data;
    }

    /**<summary> Show Item information and available actions </summary>*/
    override public void Interact(GameObject caller)
    {
        DialogManager.ShowAlert(data.name,
            data.description +
            "\nValue: " + data.value +
            "\nUse rate: " + equipmentData.useRate +
            "\nEnergy cost: " + equipmentData.energyCost
            , true,
            new DialogManager.DialogButton("Close", () => { }),
            new DialogManager.DialogButton("Pickup", () =>
            {
                Pickup(caller);
            })
        );
    }

    /**<summary> Use the Item </summary>*/
    override public int Use()
    {
        if (Time.time > useTime)
        {
            useTime = Time.time + equipmentData.useRate;
            UseLogic();
            return equipmentData.energyCost;
        }
        return 0;
    }

    /**<summary> Called when the Item is used </summary>*/
    abstract public void UseLogic();
}