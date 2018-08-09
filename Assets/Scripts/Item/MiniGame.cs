using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/**<summary> Base class for MiniGame Item </summary>*/
public class MiniGame : Item, IMiniGame
{
    [SerializeField] private bool debug;
    [SerializeField] private string equipmentName;
    internal Player player;
    internal bool active;


    private void Awake()
    {
        player = FindObjectOfType<Player>();
    } 

    /**<summary> Show Item information and available actions </summary>*/
    override public void Interact(GameObject caller)
    {
        if (active)
            return;
        if (AvailableEquipment > 0)
        {
            DialogManager.ShowAlert("Start " + data.name + "?",
                data.description,
                true,
                new DialogManager.DialogButton("No", () => { }),
                new DialogManager.DialogButton("Yes", () =>
                {
                    StartMinigame();
                })
            );
        }
        else
        {
            DialogManager.ShowAlert("Equipment requirements",
                NoEquipmentMessage,
                true,
                new DialogManager.DialogButton("Ok", () => { })
            );
            return;
        }
    }


    /**<summary> Get count of playable equipment in player inventory </summary>*/
    public int AvailableEquipment
    {
        get
        {
            if (debug)
                return 10;
            else
                return player.data.inventory.Find(x => x.itemData.name == equipmentName).count;
        }
    }

    /**<summary> Consume one playable equipment from player inventory </summary>*/
    public void ConsumeEquipment()
    {
        if (debug)
            return;
        int count = player.data.inventory.Find(x => x.itemData.name == equipmentName).count--;
        if (count == 0)
        {
            ItemData data = player.data.inventory.Find(x => x.itemData.name == equipmentName).itemData;
            player.data.inventory.Remove(player.data.inventory.Find(x => x.itemData.name == equipmentName));
            Destroy(data);
        }
    }

    /**<summary> Player runs out of required equipment </summary>*/
    public void OutOfEquipment()
    {
        DialogManager.ShowAlert("Equipment requirements",
                NoEquipmentMessage,
                true,
                new DialogManager.DialogButton("Ok", () => { StopMinigame(); })
            );
        return;
    }

    virtual public void StartMinigame() { }

    virtual public void StopMinigame() { }

    virtual public string NoEquipmentMessage { get { return string.Empty; } }

    /**<summary> Callback function when score changes </summary>*/
    public Action<int> OnScoreChanged;

    /**<summary> Callback function when item count changes </summary>*/
    public Action<int> OnItemCountChanged;
}