using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/**<summary> Base class for Capturable Items </summary>*/
public class Capturable : Weapon, IHealth
{
    [SerializeField] private int defaultHp = 200;

    private int _hp;
    //private bool capturable;
    private float lastFireTime;
    private Transform target;
    private CapturableData capturableData;

    /**<summary> Current hit points, is capturable by player if 0 </summary>*/
    public int hp
    {
        get { return _hp; }

        set
        {
            _hp = Mathf.Clamp(value, 0, capturableData.maxHp);
            OnHealthChanged?.Invoke(hp, capturableData.maxHp);
            //capturable = hp <= 0;
        }
    }

    /**<summary> Callback function when health changes </summary>*/
    public Action<int, int> OnHealthChanged { get; set; }

    private void Awake()
    {
        capturableData = ((CapturableData)data);
    }

    override internal void Start()
    {
        hp = capturableData.maxHp;
        if (hp == 0)
        {
            capturableData.maxHp = defaultHp;
            hp = defaultHp;
        }

        target = FindObjectOfType<Player>().transform;
    }

    private void Update()
    {
        if (Time.time - lastFireTime > capturableData.fireRate && Vector3.Distance(target.position, transform.position) <= capturableData.range)
        {

            //TODO: Enable when firing logic is improved
            //Fire();
            lastFireTime = Time.time;
        }
    }

    /**<summary> Show Item information and available actions </summary>*/
    override public void Interact(GameObject caller)
    {
        DialogManager.ShowAlert(data.name,
            data.description +
            "\nValue: " + data.value +
            "\nHP: " + hp + "/" + capturableData.maxHp
            , true,
            new DialogManager.DialogButton("Close", () => { }),
            new DialogManager.DialogButton("Pickup", () =>
            {
                Pickup(caller);
            })
        );
    }

    /**<summary> Use the Item </summary>*/
    override public int Use()
    {
        return 0;
    }

    /**<summary> Capturable defensive Fire? </summary>*/
    override public int Fire()
    {
        target.SendMessageUpwards("Damage", capturableData.damage);
        return 0;
    }

    /**<summary> Apply damage to this capturable </summary>*/
    public void Damage(int damage)
    {
        hp -= damage;
    }
}