using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Catchable collectable, collect on Interact </summary>*/
public class Catchable : Collectable
{
    [SerializeField] private EquipmentCategory equipmentCategory;

    /**<summary> Collect the Catchable </summary>*/
    override public void Interact(GameObject caller)
    {
        Pickup(caller);
    }
}

