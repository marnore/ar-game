using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : AI, IHealth
{
    [SerializeField] private Transform visual;
    [SerializeField] private int maxHp = 20;
    [SerializeField] private float detectRange = 10f;
    [SerializeField] private int damage = 2;
    
    private int _hp;

    /**<summary> Current hit points of the enemy, destroy if 0 </summary>*/
    public int hp
    {
        get { return _hp; }

        set
        {
            _hp = Mathf.Clamp(value, 0, maxHp);
            OnHealthChanged?.Invoke(hp, maxHp);
            if (hp <= 0)
                Destroy();
        }
    }

    /**<summary> Callback function when health changes </summary>*/
    public Action<int, int> OnHealthChanged { get; set; }

    private void Start()
    {
        hp = maxHp;
    }

    /**<summary> Apply damage to enemy </summary>*/
    public void Damage(int damage)
    {
        hp -= damage;
    }

    /**<summary> Destroy this enemy </summary>*/
    public void Destroy()
    {
        DropItems();
        Destroy(gameObject);
    }

    /**<summary> Drop random items </summary>*/
    public void DropItems()
    {
        ItemData item = ItemController.GetRandomItem();
        item.position = tr.position;
#pragma warning disable 4014
        FindObjectOfType<WorldController>().AddItem(item, false);
#pragma warning restore 4014
    }

    override public void Update()
    {
        if (Vector3.Distance(target, tr.position) > detectRange)
            return;
        base.Update();
        Vector3 targetPosition = visual.position;
        targetPosition.y = Mathf.Clamp(target.y, tr.position.y, 10);
        visual.position = Vector3.Lerp(visual.position, targetPosition, Time.deltaTime);
    }

    private void OnCollisionEnter(Collision col)
    {
        // Send damage to collided object (player)
        col.collider.SendMessageUpwards("Damage", damage, SendMessageOptions.DontRequireReceiver);
    }
}
