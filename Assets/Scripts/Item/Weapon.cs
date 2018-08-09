using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**<summary> Base class for Weapon Items </summary>*/
public abstract class Weapon : Collectable
{
    internal WeaponData weaponData;

    virtual internal void Start()
    {
        weaponData = (WeaponData)data;
    }

    /**<summary> Show Item information and available actions </summary>*/
    override public void Interact(GameObject caller)
    {
        DialogManager.ShowAlert(data.name,
            data.description +
            "\nValue: " + data.value +
            "\nDamage: " + weaponData.damage +
            "\nFire rate: " + weaponData.fireRate +
            "\nRange: " + weaponData.range +
            "\nEnergy cost: " + weaponData.energyCost
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
        return Fire();
    }

    /**<summary> Fire the weapon </summary>*/
    abstract public int Fire();
}