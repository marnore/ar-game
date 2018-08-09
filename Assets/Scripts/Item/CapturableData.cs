using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

/**<summary> Data for Weapon Items </summary>*/
[CreateAssetMenu(fileName = "Capturable", menuName = "Item/Capturable")]
[System.Serializable]
public class CapturableData : WeaponData
{
    [JsonProperty("$type", Order = -2)]
    override public string type { get { return "Capturable"; } }

    public int maxHp;

    public CapturableData() { }

    /**<summary> Create new Item class </summary>*/
    public CapturableData(string id, string itemPrefab, string name, string description, Vector3 position, Quaternion rotation, int value, bool local, string owner, long timestamp, int damage, int energyCost, float fireRate, float range, float hitForce, int maxHp)
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

        this.damage = damage;
        this.energyCost = energyCost;
        this.fireRate = fireRate;
        this.range = range;
        this.hitForce = hitForce;
        this.maxHp = maxHp;
    }

    override public string ToString()
    {
        return "id:" + id + "\nposition: " + position + "\nrotation: " + rotation + "\nvalue: " + value + "\nowner: " + owner;
    }
}