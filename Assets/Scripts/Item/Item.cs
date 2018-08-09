using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Item instance </summary>*/
public class Item : MonoBehaviour
{
    /**<summary> Data / configuration of the Item </summary>*/
    public ItemData data;

    // Setup variables in case manually added to scene
    private void Awake()
    {
        data = Instantiate(data);
        data.gObject = gameObject;
        data.position = transform.position;
        data.rotation = transform.rotation;
        data.SetTypeId();
    }

    /**<summary> Use the item </summary>*/
    virtual public int Use()
    {
        Debug.Log("This item cannot be used");
        return 0;
    }

    /**<summary> Interact with the Item, action depends on Item type (show info, collect etc.) </summary>*/
    virtual public void Interact(GameObject caller)
    {
        DialogManager.ShowAlert(data.name, data.description + "\nvalue: " + data.value, true,
            new DialogManager.DialogButton("Close", () => { })
        );
    }

    /**<summary> Pickup the Item </summary>*/
    virtual public void Pickup(GameObject collector)
    {
        Debug.Log("This item cannot be picked up");
    }

    /**<summary> Destroy the Item data and object </summary>*/
    virtual public void Destroy()
    {
        if (data) Destroy(data);
        if (gameObject) Destroy(gameObject);
    }
}
