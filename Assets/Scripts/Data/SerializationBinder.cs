using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class SerializationBinder : DefaultSerializationBinder
{
    override public Type BindToType(string assemblyName, string typeName)
    {
        switch (typeName)
        {
            case "Item": return typeof(ItemData);
            case "Equipment": return typeof(EquipmentData);
            case "Weapon": return typeof(WeaponData);
            case "Capturable": return typeof(CapturableData);
            default: return null;
        }
    }
    override public void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        assemblyName = null;
        switch (serializedType.Name)
        {
            case "ItemData" : typeName = "Item"; break;
            case "EquipmentData": typeName = "EquipmentData"; break;
            case "WeaponData": typeName = "Weapon"; break;
            case "CapturableData": typeName = "Capturable"; break;
            default: typeName = null; break;
        }
    }
}
