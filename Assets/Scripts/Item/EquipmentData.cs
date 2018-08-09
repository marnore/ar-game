using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

/**<summary> Equipment Category enum (sport type) of the Item </summary>*/
public enum EquipmentCategory { AmericanFootball, Baseball, Basketball, DiscGolf, Football, Golf, IceHockey, Tennis, None };

/**<summary> Data for sports equipment Items </summary>*/
[CreateAssetMenu(fileName = "Equipment", menuName = "Item/Equipment")]
[System.Serializable]
public class EquipmentData : ItemData
{
    [JsonProperty("$type", Order = -2)]
    override public string type { get { return "Equipment"; } }
    public int energyCost = 1;
    public float useRate, hitForce;
    public EquipmentCategory category;

    public EquipmentData() { }

    /**<summary> Create new Item class </summary>*/
    public EquipmentData(string id, string itemPrefab, string name, string description, Vector3 position, Quaternion rotation, int value, bool local, string owner, long timestamp, int energyCost, float useRate, float hitForce)
    {
        this.id = id;
        this.itemPrefab = itemPrefab;
        this.name = name;
        this.description = description;
        this.local = local;
        this.position = position;
        this.rotation = rotation;
        this.value = value;
        this.owner = owner;
        this.timestamp = timestamp > 0 ? timestamp : System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        this.energyCost = energyCost;
        this.useRate = useRate;
        this.hitForce = hitForce;

        SetTypeId();
    }

    override public string ToString()
    {
        return "id:" + id + "\nposition: " + position + "\nrotation: " + rotation + "\nvalue: " + value + "\nowner: " + owner;
    }
}