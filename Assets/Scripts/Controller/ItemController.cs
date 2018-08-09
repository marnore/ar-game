using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/**<summary> Controller for acquiring Items and Item related utilities </summary>*/
public class ItemController : MonoBehaviour
{
    public ItemDatabase itemDb;

    private static ItemController thisRef;

    private void Start()
    {
        thisRef = this;
        itemDb.items.OrderBy(i => i.value);
    }

    /**<summary> Get Prefab of random Item from DB </summary>*/
    public static GameObject GetRandomItemPrefab ()
    {
        return (GameObject) Resources.Load(GetRandomItem().itemPrefab);
    }

    /**<summary> Get Data of random Item from DB </summary>*/
    public static ItemData GetRandomItem()
    {
        return Instantiate(thisRef.itemDb.items[Random.Range(0, thisRef.itemDb.items.Length)]);
    }
}
