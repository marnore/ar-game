using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

/**<summary> RayGun Weapon shooting a ray towards specified direction </summary>*/
public class RayGun : Weapon
{

    [SerializeField] private GameObject rayVisual;
    [SerializeField] private LayerMask hitMask;
    private Camera cam;
    private float fireTime;

    override internal void Start()
    {
        weaponData = (WeaponData)data;
        cam = Camera.main;
    }

    /**<summary> Fire the weapon, raycast from touch position forward </summary>*/
    override public int Fire()
    {
        if (Time.time > fireTime && Input.touchCount > 0)
        {
            fireTime = Time.time + weaponData.fireRate;
            RaycastHit hit;

            Ray ray = cam.ScreenPointToRay(Input.GetTouch(0).position);

            if (Physics.Raycast(ray, out hit, weaponData.range, hitMask))
            {
                hit.collider.SendMessageUpwards("Damage", weaponData.damage, SendMessageOptions.DontRequireReceiver);
                if (hit.collider.attachedRigidbody)
                {
                    hit.collider.attachedRigidbody.AddForce(-hit.normal * weaponData.hitForce);
                }
                Effects(hit.point);
            }
            else
            {
                Effects(ray.origin + (ray.direction * weaponData.range));
            }
            return weaponData.energyCost;
        }
        return 0;
    }

    void Effects(Vector3 hitPosition)
    {
        Transform ray = Instantiate(rayVisual, transform.position, Quaternion.identity).transform;
        float travelTime = Vector3.Distance(transform.position, hitPosition) / 100;
        ray.DOMove(hitPosition, travelTime).SetDelay(0.04f).OnComplete(async () =>
        {
            await Task.Delay((int)(travelTime*1000) + 50);
            Destroy(ray.gameObject);
        });
    }
}