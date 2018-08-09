using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Item database for Items that can be spawned </summary>*/
[CreateAssetMenu(fileName = "Item Database", menuName = "Item/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public ItemData[] items;
}
