using GoogleARCore;
using MaterialUI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour {

    [SerializeField] private LayerMask itemLayers;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private Camera cam;
    [SerializeField] private string id;
    [SerializeField] private float interactRange = 3;

    [SerializeField] private int maxEnergy;
    [SerializeField] private int levelTarget; // TODO: define progression somewhere else with difficulty curve etc...
    [SerializeField] private int equipItemLimit = 1;
    [SerializeField] private int inventoryLimit = 100;

    [SerializeField] private Transform hand;
    [SerializeField] private ItemData[] defaultItems;

    private WorldController worldController;
    private TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal;
    private SimpleDialog inventoryDialog;

    public List<Item> equippedItems = new List<Item>();

    public Data data;
    // Class for player data, json serializable
    [System.Serializable]
    public class Data
    {
        // inventory of user owned objects
        public List<InventoryItem> inventory = new List<InventoryItem>();
        public List<ItemData> hiddenItems = new List<ItemData>();
        public List<ItemData> equippedItems = new List<ItemData>();
        public int activeItemIndex = -1;

        public int experience;
        public int energy;
        public int credits;
    }
    // Class for Inventory Item data, json serializable
    [System.Serializable]
    public class InventoryItem
    {
        public ItemData itemData;
        public int count;

        public InventoryItem(ItemData itemData, int count)
        {
            this.itemData = itemData;
            this.count = count;
        }
    }

    /**<summary> Name of the player </summary>*/
    new public string name { get { return id.ToString(); } }

    /**<summary> Current energy level of the player, if too low cannot do certain tasks </summary>*/
    public int energy
    {
        get { return data.energy; }

        set
        {
            if (value == energy)
                return;
            data.energy = Mathf.Clamp(value, 0, maxEnergy);
            OnEnergyUpdated?.Invoke(energy, maxEnergy);
        }
    }

    /**<summary> Current experience of the player </summary>*/
    public int experience
    {
        get { return data.experience; }

        set
        {
            if (value == experience)
                return;
            data.experience = Mathf.Clamp(value, 0, int.MaxValue);
            OnExpUpdated?.Invoke(experience, levelTarget);
        }
    }

    /**<summary> Current experience of the player </summary>*/
    public int credits
    {
        get { return data.credits; }

        set
        {
            if (value == credits)
                return;
            data.credits = Mathf.Clamp(value, 0, int.MaxValue);
            OnCreditsUpdated?.Invoke(credits);
        }
    }

    /** Get current player level */
    public int level
    {
        get { return (int)(experience / levelTarget); }
    }

    /**<summary> Enable / disable object edit mode </summary>*/
    public bool ObjectEditMode { get; set; }

    private void Awake()
    {
        id = SystemInfo.deviceUniqueIdentifier;
    }

    private async void Start()
    {
        worldController = FindObjectOfType<WorldController>();
        ServerAPI.OnGetItemsResponse += OnGetItemsResponse;
        await Load();
    }

    private void Update()
    {
        Controls();
    }

    /**<summary> Input and control management </summary>*/
    private void Controls()
    {
        TrackableHit arcHit;
        RaycastHit hit;

        if (EventSystem.current && (EventSystem.current.IsPointerOverGameObject() || EventSystem.current.IsPointerOverGameObject(0)) || StateManager.sceneState == SceneState.Minigame)
            return;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, interactRange, itemLayers))
            {
                Interact(hit.collider, hit.point, false);
            }
            else if (Frame.Raycast(Input.mousePosition.x, Input.mousePosition.y, raycastFilter, out arcHit))
            {
                Interact(null, arcHit.Pose.position, true);
            }
            else if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, interactRange, hitLayers))
            {
                Interact(hit.collider, hit.point, false);
            }
            else
            {
                Interact(null, Vector3.zero, false);
            }
        }
#endif
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            return;

        if (Physics.Raycast(cam.ScreenPointToRay(touch.position), out hit, interactRange, itemLayers))
        {
            Interact(hit.collider, hit.point, false);
        }
        else if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out arcHit))
        {
            Interact(null, arcHit.Pose.position, true);
        }
        else if (Physics.Raycast(cam.ScreenPointToRay(touch.position), out hit, interactRange, hitLayers))
        {
            Interact(hit.collider, hit.point, false);
        }
        else
        {
            Interact(null, Vector3.zero, false);
        }
    }

    /**<summary> Interaction on point hit by ray </summary>*/
    private void Interact(Collider col, Vector3 position, bool ar)
    {
        Item item = null;
        if (col) item = col.GetComponentInParent<Item>();

        if (item && !(item is Capturable))
        {
            item.Interact(gameObject);
        }
        else if (ObjectEditMode)
                InstantiateObject(position, ar);
        else if (equippedItems.Count > 0 && data.activeItemIndex >= 0)
        {
            if (energy > 0)
                energy -= equippedItems[data.activeItemIndex].Use();
            else
                Debug.Log("Out of energy");
        }
    }

    /**<summary> Instantiate an object to touch point </summary>*/
    private void InstantiateObject(Vector3 position, bool ar)
    {
        Vector3 cameraPosition = cam.transform.position;
        cameraPosition.y = position.y;
        Quaternion rotation = Quaternion.LookRotation(cameraPosition - position, Vector3.up);
#pragma warning disable 4014
        worldController.AddItem(new ItemData(null, "Andy", "Test Android", "Android Andy static test Item", position, rotation, 0, true, name, 0), ar, false);
#pragma warning restore 4014
    }

    /**<summary> Save player data </summary>*/
    public async Task Save()
    {
        data.equippedItems.Clear();
        equippedItems.ForEach(item => data.equippedItems.Add(item.data));
        await ServerAPI.SavePlayer(id, data);
    }

    /**<summary> Load saved player data </summary>*/
    public async Task Load()
    {
        string dataJson = await ServerAPI.GetPlayer(id);
        if (!dataJson.IsNullOrEmpty())
        {
            Debug.Log(dataJson);
            try
            {
                Data playerData = JsonConvert.DeserializeObject<Data>(dataJson, ServerAPI.deserialise);
                if (playerData != null)
                    data = playerData;
            }
            catch (Exception e) { Debug.LogError(e); }
        }
        if (data.inventory.Count == 0 && data.experience == 0)
            AddDefaultItems();

        if (data.equippedItems.Count > 0)
        {
            foreach (ItemData item in data.equippedItems)
            {
                EquipItem(item);
            }
            SelectItemAsActive(data.activeItemIndex);
            OnActiveItemChanged?.Invoke(data.activeItemIndex >= 0 ? data.equippedItems[data.activeItemIndex] : null, data.activeItemIndex);
        }
        UpdatePlayerStats(false);
    }

    /** Add default items for player (test)*/
    private void AddDefaultItems()
    {
        Array.ForEach(defaultItems, i =>
        {
            i.owner = name;
            if (data.inventory.Select(x => x.itemData).Any(x => x.typeID == i.typeID))
                data.inventory.Find(x => x.itemData.typeID == i.typeID).count++;
            else
                data.inventory.Add(new InventoryItem(i, 1));
        });
    }

    /**<summary> Equip an ItemData item </summary>*/
    private void EquipItem(ItemData itemData)
    {
        GameObject obj = (GameObject)Resources.Load(itemData.itemPrefab);
        if (!obj)
            obj = Resources.Load<GameObject>("Item");
        Item item = (Instantiate(obj)).GetComponent<Item>();
        itemData.gObject = item.gameObject;
        item.data = itemData;
        equippedItems.Add(item);


        // Unequip oldest item in case max limit reaceh
        if (equippedItems.Count > equipItemLimit)
            UnequipItem(0);
        else
            item.gameObject.SetActive(false);

        // Select as active if only one item equipped
        if (data.equippedItems.Count == 1)
            SelectItemAsActive(0);

        RefreshInventory();
        OnEquippedItemsChanged?.Invoke(equippedItems.Select(i => i.data).ToArray());
    }

    /**<summary> Unequip an Item </summary>*/
    private void UnequipItem(int itemIndex)
    {
        if (equippedItems.Count > 0)
        {
            Destroy(equippedItems[itemIndex].gameObject);
            equippedItems.RemoveAt(itemIndex);
            SelectItemAsActive(equippedItems.Count-1);
        }
        if (equippedItems.Count == 0)
        {
            data.activeItemIndex = -1;
            OnActiveItemChanged?.Invoke(new ItemData(), -1);
        }

        RefreshInventory();
        OnEquippedItemsChanged?.Invoke(equippedItems.Select(i => i.data).ToArray());
    }

    /**<summary> Set an equipped item as active item (in hand) </summary>*/
    public void SelectItemAsActive(int itemIndex)
    {
        data.activeItemIndex = itemIndex;
        equippedItems.ForEach(x => x.gameObject.SetActive(false));
        if (itemIndex >= 0)
        {
            Item active = equippedItems[itemIndex];
            active.gameObject.SetActive(true);
            active.transform.parent = hand;
            active.transform.localPosition = Vector3.zero;
            active.transform.localRotation = Quaternion.identity;

            int layer = LayerMask.NameToLayer("ActiveItem");
            active.gameObject.layer = layer;
            foreach (Transform t in active.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = layer;
            }
        }
    }

    /**<summary> Updates current player stats by calling subscribed events </summary>*/
    public void UpdatePlayerStats(bool save)
    {
        OnExpUpdated?.Invoke(experience, levelTarget);
        OnEnergyUpdated?.Invoke(energy, maxEnergy);
        OnCreditsUpdated?.Invoke(credits);
#pragma warning disable 4014
        if (save)
            Save();
#pragma warning restore 4014
    }

    /**<summary> Add item to player inventory </summary>*/
    public void AddToInventory(ItemData item)
    {
        if (data.inventory.Count < inventoryLimit)
        {
            item.owner = name;
            if (data.inventory.Select(x => x.itemData).Any(x => x.typeID == item.typeID))
                data.inventory.Find(x => x.itemData.typeID == item.typeID).count++;
            else
                data.inventory.Add(new InventoryItem(item, 1));
            RefreshInventory();
        }
        else
        {
            NativeMethods.ShowToast("Inventory full!");
        }
    }

    /**<summary> Refresh player inventory UI </summary>*/
    public void RefreshInventory()
    {
        if (!inventoryDialog)
            return;
        inventoryDialog.Initialize("Inventory", data.inventory.Select(b => (b.count > 1 ? " " + b.count + " | " : string.Empty) + b.itemData.name + (equippedItems.Count > 0 && equippedItems.Select(x => x.data.id).Contains(b.itemData.id) ? " (equipped)" : "")).ToArray(), true, false,
            (int selectedIndex) =>
            {
                DialogManager.ShowAlert(data.inventory[selectedIndex].itemData.name + " | " + data.inventory[selectedIndex].count, data.inventory[selectedIndex].itemData.description + "\nValue: " + data.inventory[selectedIndex].itemData.value, true,
                    new DialogManager.DialogButton("Close", () => { }),
                    new DialogManager.DialogButton("Discard", () =>
                    {
                        DiscardItem(selectedIndex);
                    }),
                    // Disable hiding for now, uncomment to enable
                    /*new DialogManager.DialogButton("Hide", () =>
                    {
                        DialogManager.ShowAlert("Hide " + data.inventory[selectedIndex].itemData.name + " to world?", "You get rewarded based on the time the item stays hidden and base value of it", true,
                            new DialogManager.DialogButton("No", () => { }),
                            new DialogManager.DialogButton("Yes", () =>
                            {
                                DropItem(selectedIndex);
                            })
                        );
                    }),*/
                    equippedItems.Select(x => x.data.id).Contains(data.inventory[selectedIndex].itemData.id) ?
                    new DialogManager.DialogButton("Unequip", () =>
                    {
                        UnequipItem(equippedItems.FindIndex(x => x.data.id == data.inventory[selectedIndex].itemData.id));
                    })
                    :
                    new DialogManager.DialogButton("Equip", () =>
                    {
                        EquipItem(data.inventory[selectedIndex].itemData);
                    })
                );
            });
    }

    /**<summary> Show player inventory items </summary>*/
    public void ShowInventory()
    {
        // Simple dialog until proper implementation
        inventoryDialog = DialogManager.ShowSimple("Inventory", data.inventory.Select(b => (b.count > 1 ? " " + b.count + " | " : string.Empty) + b.itemData.name + (equippedItems.Count > 0 && equippedItems.Select(x => x.data.id).Contains(b.itemData.id) ? " (equipped)" : "")).ToArray(), true, false,
            (int selectedIndex) =>
            {
                DialogManager.ShowAlert(data.inventory[selectedIndex].itemData.name + " | " + data.inventory[selectedIndex].count, data.inventory[selectedIndex].itemData.description + "\nValue: " + data.inventory[selectedIndex].itemData.value, true,
                    new DialogManager.DialogButton("Close", () => { }),
                    new DialogManager.DialogButton("Discard", () =>
                    {
                        DiscardItem(selectedIndex);
                    }),
                    // Disable hiding for now, uncomment to enable
                    /*new DialogManager.DialogButton("Hide", () =>
                    {
                        DialogManager.ShowAlert("Hide " + data.inventory[selectedIndex].itemData.name + " to world?", "You get rewarded based on the time the item stays hidden and base value of it", true,
                            new DialogManager.DialogButton("No", () => { }),
                            new DialogManager.DialogButton("Yes", () =>
                            {
                                DropItem(selectedIndex);
                            })
                        );
                    }),*/
                    equippedItems.Select(x => x.data.id).Contains(data.inventory[selectedIndex].itemData.id) ?
                    new DialogManager.DialogButton("Unequip", () =>
                    {
                        UnequipItem(equippedItems.FindIndex(x => x.data.id == data.inventory[selectedIndex].itemData.id));
                    })
                    :
                    new DialogManager.DialogButton("Equip", () =>
                    {
                        EquipItem(data.inventory[selectedIndex].itemData);
                    })
                );
            }
        );
    }

    /**<summary> Reset state of player </summary>*/
    public void Reset ()
    {

    }

    /**<summary> Called when GetItems finishes, check if hidden items have been picked up </summary>*/
    public void OnGetItemsResponse(bool success, List<ItemData> items)
    {
        for (int i = 0; i < data.hiddenItems.Count; i++)
        {
            if (!items.Exists(x => x.id == data.hiddenItems[i].id))
            {
                HideReward(data.hiddenItems[i]);
                Destroy(data.hiddenItems[i]);
                data.hiddenItems.RemoveAt(i);
            }
        }
    }

    /**<summary> Reward player from hidden Items </summary>*/
    public void HideReward(ItemData item)
    {
        TimeSpan time = DateTimeOffset.UtcNow.Subtract(DateTimeOffset.FromUnixTimeSeconds(item.timestamp));
        float timeBonus = Mathf.Max((float)time.TotalHours * 0.2f, 10);

        int expReward = (int)(item.value * timeBonus);
        int creditsReward = (int)(item.value / 10 * timeBonus);

        experience += expReward;
        credits += creditsReward;

        DialogManager.ShowAlert("Item hiding reward", $"Your {item.name} was hidden for {String.Format("{0:%d} Days {0:%h} Hours {0:%m} Minutes", time)}\n\nReward: {expReward} Exp and {creditsReward} Cr", true,
            new DialogManager.DialogButton("Ok", () => { })
        );
    }

    /**<summary> Drop / Hide an item to world from inventory </summary>*/
    private async void DropItem(int itemIndex)
    {
        ItemData itemData = Instantiate(data.inventory[itemIndex].itemData);
        itemData.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        itemData.position = FindObjectOfType<RaycastSpawner>().GetPointOnCollider(transform.position + transform.forward * 0.5f);
        itemData.rotation = transform.rotation;

        // Get world instance of item with ID and add it to hiddenItems List
        ItemData item = await worldController.AddItem(itemData, true);
        data.hiddenItems.Add(item);

        DiscardItem(itemIndex);
    }

    /**<summary> Discard / Destroy item from inventory </summary>*/
    private void DiscardItem(int itemIndex)
    {
        if (data.inventory[itemIndex].count > 1)
            data.inventory[itemIndex].count--;
        else
        {
            Destroy(data.inventory[itemIndex].itemData);
            data.inventory.RemoveAt(itemIndex);
        }

        RefreshInventory();
    }

    /**<summary> Called when player collects an Item </summary>*/
    public void OnItemCollected(ItemData item)
    {
        item = Instantiate(item);
        // Reward exp only if the object isn't owned by player
        if (item.owner != name)
        {
            experience += item.value;
            credits += item.value > 0 ? Math.Max(1, item.value / 10) : 0;
        } 
        
        AddToInventory(item);
#pragma warning disable 4014
        Save();
#pragma warning restore 4014
    }

    /**<summary> Called when player collects a Coin </summary>*/
    public void OnCoinCollected(ItemData item)
    {
        // Reward exp only if the object isn't owned by player
        if (item.owner != name)
        {
            experience += item.value > 0 ? Math.Max(1, item.value / 10) : 0;
            credits += item.value;
        }
#pragma warning disable 4014
        Save();
#pragma warning restore 4014
    }

    /**<summary> Called when player collects an Energy Item </summary>*/
    public void OnEnergyCollected(ItemData item)
    {
        // Reward exp only if the object isn't owned by player
        if (item.owner != name)
        {
            experience += item.value;
            credits += item.value > 0 ? Math.Max(1, item.value / 10) : 0;
        }
        energy += item.value;
#pragma warning disable 4014
        Save();
#pragma warning restore 4014
    }

    /**<summary> Apply damage to player </summary>*/
    public void Damage (int damage)
    {
        energy -= damage;
        if (energy == 0)
            credits -= 1;
    }

    /**<summary> Callback function when energy changes </summary>*/
    public Action<int, int> OnEnergyUpdated;

    /**<summary> Callback function when experience changes </summary>*/
    public Action<int, int> OnExpUpdated;

    /**<summary> Callback function when credits changes </summary>*/
    public Action<int> OnCreditsUpdated;

    /**<summary> Callback function when Active Item changes </summary>*/
    public Action<ItemData, int> OnActiveItemChanged;

    /**<summary> Callback function when Equipped Items change </summary>*/
    public Action<ItemData[]> OnEquippedItemsChanged;
}
