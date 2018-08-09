using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

/**<summary> Data for Weapon Items </summary>*/
[CreateAssetMenu(fileName = "Weapon", menuName = "Item/Weapon")]
[System.Serializable]
public class WeaponData : ItemData
{
    [JsonProperty("$type", Order = -2)]
    override public string type { get { return "Weapon"; } }
    public int damage = 1;
    public int energyCost = 1;
    public float fireRate, range, hitForce;

    public WeaponData() { }

    /**<summary> Create new Item class </summary>*/
    public WeaponData(string id, string itemPrefab, string name, string description, Vector3 position, Quaternion rotation, int value, bool local, string owner, long timestamp, int damage, int energyCost, float fireRate, float range, float hitForce)
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

        SetTypeId();
    }

    override public string ToString()
    {
        return "id:" + id + "\nposition: " + position + "\nrotation: " + rotation + "\nvalue: " + value + "\nowner: " + owner;
    }
}