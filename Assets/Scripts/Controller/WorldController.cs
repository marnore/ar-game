using System;
using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.IO;
using UnityExtension;
using System.Threading.Tasks;
using MaterialUI;

public class WorldController : MonoBehaviour
{
    [SerializeField] private ItemData[] energyTemplates;
    [SerializeField] private GameObject[] movingGameEquipmentPrefabs;
    [SerializeField] private GameObject[] gameEquipmentPrefabs;

    [SerializeField] private float energyDensity = 0.12f;
    [SerializeField] private float energyMaxDistance = 20;
    [SerializeField] private float energySpawnRate = 8;

    [SerializeField] private float movingEquipmentDensity = 0.01f;
    [SerializeField] private float movingEquipmentMinDistance = 5;
    [SerializeField] private float movingEquipmentSpawnRate = 8;

    [SerializeField] private float equipmentDensity = 0.001f;
    [SerializeField] private float equipmentMinDistance = 5;
    [SerializeField] private float equipmentSpawnRate = 12;

    [SerializeField] private Renderer worldObject;
    [SerializeField] internal Transform itemRoot;
    [SerializeField] private Transform arObjectRoot;

    internal string gameID;
    internal Parser.Game[] games;

    private Parser.Area currentArea;
    private GameObject[] energyItems;
    private List<GameObject> movingItems = new List<GameObject>();
    [ContextMenuItem("Find Items", "FindItems")]
    [ContextMenuItem("Save Items", "SaveItems")]
    [SerializeField] private List<ItemData> items = new List<ItemData>();
    private Transform player;
    private RaycastSpawner raycastSpawner;

    private bool finished = false, paused = false;
    private float lastObjectUpdateTime;

    private float lastCollectableTime, lastEquipmentTime, lastMovingEquipmentTime;
    private int energyAmount, equipmentAmount, movingEquipmentAmount;

    private static WorldController thisRef;

    private int _collected = 0;
    /**<summary> Number of collectables collected in a game session </summary>*/
    public int collected
    {
        get { return _collected; }
        set
        {
            _collected = Mathf.Clamp(value, 0, int.MaxValue);
            OnCollectedUpdated?.Invoke(collected);
        }
    }

    private void Start()
    {
        thisRef = this;
        player = FindObjectOfType<Player>().transform;
        raycastSpawner = FindObjectOfType<RaycastSpawner>();
        ServerAPI.OnGetItemsResponse += OnGetItemsResponse;
        ServerAPI.OnSetItemsResponse += OnSetItemsResponse;
        ServerAPI.OnDeleteItemsResponse += OnDeleteItemsResponse;
        //SetupWorld(new Area());
#if UNITY_EDITOR
#endif
    }

    private void Update()
    {
        if (paused || gameID.IsNullOrEmpty())
            return;
        AddCollectables();
        UpdateGameEquipment();
        UpdateMovingGameEquipment();
        if (Time.time - lastObjectUpdateTime > App.config.locationRefreshRate)
        {
            lastObjectUpdateTime = Time.time;
            UpdateObjects();
#pragma warning disable 4014
            ServerAPI.GetItems(gameID);
            // -> OnGetItemsResponse
#pragma warning restore 4014
        }
    }

    /**<summary> Pause world state </summary>*/
    public void Pause()
    {
        paused = true;
        itemRoot.gameObject.SetActive(false);
        worldObject.gameObject.SetActive(false);
    }

    /**<summary> Resume world state </summary>*/
    public void Resume()
    {
        itemRoot.gameObject.SetActive(true);
        worldObject.gameObject.SetActive(true);
        paused = false;
    }

    public static Bounds CurrentAreaBounds()
    {
        return thisRef.currentArea.bounds.GetUnityBounds();
    }

    /**<summary> Instantiate collectables based on time </summary>*/
    private void AddCollectables()
    {
        if (Time.time - lastCollectableTime < energySpawnRate
            || currentArea.areaID.IsNullOrEmpty()
            || energyAmount > MaxAmount(energyDensity, energyMaxDistance, currentArea.bounds))
        {
            return;
        }

        AddCollectable();
    }

    /**<summary> Add Collectable to the world </summary>*/
    public void AddCollectable()
    {
        ItemData item = Instantiate(energyTemplates[UnityEngine.Random.Range(0, energyTemplates.Length)]);
        item.position = RaycastSpawner.GetRandomPoint(currentArea.bounds.GetUnityBounds(), player.position, 3, energyMaxDistance);
#pragma warning disable 4014
        AddItem(item, false);
#pragma warning restore 4014
        lastCollectableTime = Time.time + UnityEngine.Random.Range(-energySpawnRate * 0.1f, energySpawnRate * 0.1f);
        energyAmount++;
    }

    /**<summary> Update enemies and instantiate new based on time </summary>*/
    private void UpdateMovingGameEquipment()
    {
        if (Time.time - lastMovingEquipmentTime < movingEquipmentSpawnRate
            || currentArea.areaID.IsNullOrEmpty()
            || movingEquipmentAmount > MaxAmount(movingEquipmentDensity, 50, currentArea.bounds))
        {
            return;
        }

        AddMovingGameEquipment();
    }

    /**<summary> Update enemies and instantiate new based on time </summary>*/
    private void UpdateGameEquipment()
    {
        if (Time.time - lastEquipmentTime < equipmentSpawnRate
            || currentArea.areaID.IsNullOrEmpty()
            || equipmentAmount > MaxAmount(equipmentDensity, 50, currentArea.bounds))
        {
            return;
        }

        AddGameEquipment();
    }

    /**<summary> Add moving Game Equipment (ball, puck, etc.) to the world </summary>*/
    public void AddMovingGameEquipment()
    {
        Debug.Log("Bounds:");
        Debug.Log(currentArea.bounds);
        GameObject obj = raycastSpawner.Spawn(movingGameEquipmentPrefabs[UnityEngine.Random.Range(0, movingGameEquipmentPrefabs.Length)],
         currentArea.bounds, player.position, movingEquipmentMinDistance, 50, 0.3f, itemRoot);
        obj.GetComponent<Item>().data = Instantiate(obj.GetComponent<Item>().data);
        obj.GetComponent<AI>().Initialise();

        movingItems.Add(obj);

        lastMovingEquipmentTime = Time.time + UnityEngine.Random.Range(-movingEquipmentSpawnRate * 0.1f, movingEquipmentSpawnRate * 0.1f);
        movingEquipmentAmount++;
    }

    /**<summary> Add Game Equipment (racket, bat etc.) to the world </summary>*/
    public void AddGameEquipment()
    {
        GameObject obj = raycastSpawner.Spawn(gameEquipmentPrefabs[UnityEngine.Random.Range(0, gameEquipmentPrefabs.Length)], currentArea.bounds, player.position, equipmentMinDistance, 50, 0.3f, itemRoot);
        obj.GetComponent<Item>().data = Instantiate(obj.GetComponent<Item>().data);
        items.Add(obj.GetComponent<Item>().data);

        lastEquipmentTime = Time.time + UnityEngine.Random.Range(-equipmentSpawnRate * 0.1f, equipmentSpawnRate * 0.1f);
        equipmentAmount++;
    }

    /**<summary> Get maximum amount / count resulting from density and area constraints </summary>*/
    private int MaxAmount(float density, float distance, Parser.Bounds bounds)
    {
        return (int)(density * Mathf.Min(bounds.area, distance * distance * 4));
    }

    /**<summary> Initialise world, get obstacles and build navmesh </summary>*/
    public void InitArea()
    {
        InitArea(currentArea);
    }

    /**<summary> Initialise world, get obstacles and build navmesh </summary>*/
    public async void InitArea(Parser.Area area)
    {
        finished = false;
        currentArea = area;
        collected = 0;
        ClearWorld();

        Tuple<bool, byte[]> result = await ServerAPI.Obstacles(area.areaID);

        if (result.Item1)
        {
            Stream s = new MemoryStream(result.Item2);
            OBJData objData = OBJLoader.LoadOBJ(s);
            s.Dispose();
            if (objData == null || objData.m_Vertices.Count <= 0)
            {
                if (App.config.debug)
                    Debug.Log("Couldn't load obstacle obj");
                return;
            }
            Mesh mesh = new Mesh();
            mesh.LoadOBJ(objData);

            worldObject.GetComponent<MeshFilter>().mesh = mesh;
            worldObject.GetComponent<MeshCollider>().sharedMesh = mesh;

            worldObject.GetComponent<NavMeshSurface>().BuildNavMesh();
        }
        else
        {
            if (App.config.debug)
                Debug.Log("No obstacle obj");
        }

        SelectGame();
    }

    /**<summary> Get games and show game selection dialog </summary>*/
    public async void SelectGame()
    {
        Tuple<bool, Parser.Game[]> result = await ServerAPI.GetGames();
        if (!result.Item1)
            return;
        games = result.Item2;
        if (games.Length == 0)
        {
            NativeMethods.ShowToast("No games found on server");
            return;
        }
        DialogManager.ShowSimple("Select game", games.Select(b => b.name + " " + b.description).ToArray(), false, (int selectedIndex) => {
            //ToastManager.Show(games[selectedIndex].name + " selected");

            gameID = games[selectedIndex].id;
            SetupWorld(gameID);
        });
    }

    /**<summary> Setup world, instantiate items, enemies etc. </summary>*/
    public async void SetupWorld(string gameID)
    {
        await ServerAPI.GetItems(gameID);
        this.gameID = gameID;
    }

    /**<summary> Clear world state, remove old objects </summary>*/
    public void ClearWorld()
    {
        if (energyItems != null)
        {
            foreach (GameObject g in energyItems)
                if (g) Destroy(g);
            energyItems = null;
        }
        if (movingItems != null)
        {
            foreach (GameObject g in movingItems)
                if (g) Destroy(g);
            movingItems.Clear();
        }
        ClearObjects();
        energyAmount = 0;
        movingEquipmentAmount = 0;
    }

    /**<summary> Clear server-syncable added objects </summary>*/
    public void ClearObjects()
    {
        if (items != null)
        {
            foreach (ItemData o in items)
                if (o.gObject) Destroy(o.gObject);
            items.Clear();
        }
    }

    /**<summary> Update added objects </summary>*/
    public void UpdateObjects()
    {
        // remove objects that doesn't exist anymore
        items.RemoveAll(o => !o || !o.gObject);
        foreach (ItemData o in items)
        {
            // Raycast for ARCore planes and set height accordingly
            if (!o.local && o.gObject.GetComponent<RaycastHeight>())
                o.gObject.GetComponent<RaycastHeight>().SetHeight();
        }
    }

    /**<summary> Add server-syncable Item to world </summary>*/
    public async Task<ItemData> AddItem(ItemData item, bool save)
    {
        return await AddItem(item, false, save);
    }

    /**<summary> Add server-syncable Item to world </summary>*/
    public async Task<ItemData> AddItem(ItemData item, bool ar, bool save)
    {
        Transform itemInstance = Instantiate(GetItem(item.itemPrefab), Vector3.zero, Quaternion.identity).transform;
        if (ar)
            itemInstance.SetParent(arObjectRoot);
        else
            itemInstance.SetParent(itemRoot);
        itemInstance.localPosition = item.position;
        itemInstance.rotation = item.rotation;

        // Raycast for ARCore planes and set height accordingly
        if (itemInstance.GetComponent<RaycastHeight>())
            itemInstance.GetComponent<RaycastHeight>().SetHeight();

        item.gObject = itemInstance.gameObject;

        itemInstance.GetComponent<Item>().data = item;
        items.Add(item);

        // Send Item to server, and return the server instance (has id)
        if (save)
        {
            Tuple<bool, ItemData> response = await ServerAPI.SetItem(gameID, item);
            if (response.Item1)
                item = response.Item2;
        }

        return item;
    }

    /**<summary> Find all items and add them to items List </summary>*/
    public void FindItems()
    {
        items.AddRange(FindObjectsOfType<Item>().Select(i => i.data));
    }

    /**<summary> Save Items to server </summary>*/
    public async void SaveItems()
    {
        await ServerAPI.SetItems(gameID, items/*.Where(o => o.local)*/.ToArray());
        // -> OnGetObjectsResponse
    }

    /**<summary> Add Items from server, called when GetItems finishes </summary>*/
    public void OnGetItemsResponse(bool success, List<ItemData> items)
    {
        foreach (ItemData obj in items)
        {
            obj.local = false;
            // Add new object if it doesn't exist in world
            if (!this.items.Exists(x => x.id == obj.id))
#pragma warning disable 4014
                AddItem(obj, false);
#pragma warning restore 4014
        }
        UpdateObjects();
    }

    /**<summary> Called when SetItems finishes </summary>*/
    public void OnSetItemsResponse(bool success)
    {
        if (success)
            items.All(o => o.local = false);
    }

    /**<summary> Delete added Items, called when DeleteItems finishes </summary>*/
    public void OnDeleteItemsResponse(bool success)
    {
        ClearObjects();
    }

    /**<summary> Get Item according to type requested </summary>*/
    private GameObject GetItem (string itemPrefab)
    {
        GameObject obj = Resources.Load<GameObject>(itemPrefab);
        if (obj)
            return obj;
        else
            return Resources.Load<GameObject>("Item");
    }

    /**<summary> Game is finished, win or lose </summary>*/
    public void Finish (bool win)
    {
        if (finished)
            return;
        finished = true;
        OnFinish?.Invoke(win);
    }

    /**<summary> Callback function when a collectable is picked </summary>*/
    public Action<int> OnCollectedUpdated;

    /**<summary> Callback function when game is finished (win or lose) </summary>*/
    public Action<bool> OnFinish;
}
