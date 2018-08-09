using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Catchable collectable, collect on Interact </summary>*/
public class Hittable : Collectable
{
    [SerializeField] private EquipmentCategory equipmentCategory;
    private bool isHit = false;

    /**<summary> No Interaction </summary>*/
    override public void Interact(GameObject caller)
    {
        
    }

    /**<summary> This item is hit </summary>*/
    public bool Hit(GameObject player, EquipmentData equipment)
    {
        if (isHit)
            return false;

        if (equipment.category == equipmentCategory || equipment.category == EquipmentCategory.None)
        {
            isHit = true;
            Pickup(player);
            return true;
        }

        return false;
    }
}

