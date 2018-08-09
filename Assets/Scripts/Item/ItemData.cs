using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Representing and (de)serializing Item data </summary>*/
[CreateAssetMenu(fileName = "Item", menuName = "Item/Item")]
[System.Serializable]
public class ItemData : ScriptableObject
{
    internal bool local;
    [HideInInspector] public string id;
    [JsonProperty("$type", Order = -2)]
    virtual public string type { get { return "Item"; } }
    public string itemPrefab;
    new public string name;
    public string description;
    [HideInInspector] public Vector3 position;
    [HideInInspector] public Quaternion rotation;
    public int value;
    [HideInInspector] public string owner;
    [HideInInspector] public long timestamp;

    private string _typeID;
    // Compare this to check if items are same
    [JsonIgnore] [HideInInspector]
    public string typeID
    {
        get
        {
            if (_typeID.IsNullOrEmpty())
                SetTypeId();
            return _typeID;
        }
        set
        {
            _typeID = value;
        }
    }

    internal GameObject _gObject;
    [JsonIgnore]
    public GameObject gObject
    {
        get { return _gObject; }
        set
        {
            _gObject = value;
            if (id.IsNullOrEmpty() && gObject)
                id = gObject.GetInstanceID().ToString();
        }
    }

    /**<summary> Create new Item class </summary>*/
    public ItemData() { }

    /**<summary> Create new Item class </summary>*/
    public ItemData(string id, string itemPrefab, string name, string description, Vector3 position, Quaternion rotation, int value, bool local, string owner, long timestamp)
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

        SetTypeId();
    }

    public string SetTypeId()
    {
        typeID = name + itemPrefab;
        return typeID;
    }

    override public string ToString()
    {
        return "id:" + id + "\nname:" + name + "\ndescription:" + description + "\nposition: " + position + "\nrotation: " + rotation + "\nvalue: " + value + "\nowner: " + owner;
    }
}
