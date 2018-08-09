using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Tracked minigame item / projectile </summary>*/
public class MinigameItem : MonoBehaviour
{
    [SerializeField] private float lifeTime;
    [HideInInspector] public bool scored;

    private bool _moved;
    /**<summary> Has this item been moved (hit, thrown etc.) </summary>*/
    public bool moved
    {
        get { return _moved; }
        set
        {
            _moved = value;
            Invoke("Destroy", lifeTime);
        }
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
