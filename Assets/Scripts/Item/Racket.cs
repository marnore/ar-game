using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

/**<summary> Player Equipment Items type of Racket or Bat etc. </summary>*/
public class Racket : PlayerEquipment
{
    [SerializeField] private Animation animator;
    [SerializeField] private float hitTime = 0.5f;
    [SerializeField] private Collider hitCollider;

    private GameObject player;

    private void Awake()
    {
        hitCollider.enabled = false;
        player = FindObjectOfType<Player>().gameObject;
    } 

    /**<summary> Called when the Item is used </summary>*/
    override public async void UseLogic()
    {
        hitCollider.enabled = true;
        animator.Play();
        // Wait for hit time
        await Task.Delay((int)(hitTime * 1000));
        hitCollider.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!animator.isPlaying)
            return;
        if (collision.rigidbody)
        {
            bool correctHit = false;
            if (collision.rigidbody.GetComponentInParent<Hittable>())
                correctHit = collision.rigidbody.GetComponentInParent<Hittable>().Hit(player, equipmentData);
            if (correctHit)
                collision.rigidbody.AddForce(player.transform.forward * equipmentData.hitForce, ForceMode.Impulse);
        }
    }
}