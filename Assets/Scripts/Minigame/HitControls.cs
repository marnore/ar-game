using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Hit / flick controls for physics objects </summary>*/
public class HitControls : MonoBehaviour
{
    [SerializeField] private float hitForce;
    [SerializeField] private float maxTouchVelocity;
    [SerializeField] private Vector3 positionAdjust;
    [SerializeField] private Vector3 forceAdjust;
    [SerializeField] private LayerMask hitLayers;

    private Vector2 touchVelocity;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void FixedUpdate()
    {
        touchVelocity = Vector2.zero;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            touchVelocity = Vector2.ClampMagnitude(Input.GetTouch(0).deltaPosition / Input.GetTouch(0).deltaTime, maxTouchVelocity) * 0.25f;

            RaycastHit hit;
            if (Physics.Raycast(cam.ScreenPointToRay(Input.GetTouch(0).position), out hit, 1.5f, hitLayers))
            {
                Hit(hit.collider, hit.point, touchVelocity);
            }
        }

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            touchVelocity = Vector2.ClampMagnitude(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) / Time.fixedDeltaTime, maxTouchVelocity);

            RaycastHit hit;
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 1.5f, hitLayers))
            {
                Hit(hit.collider, hit.point, touchVelocity);
            }
        }
#endif
    }

    /**<summary> Hit object with velocity and point to apply force </summary>*/
    private void Hit (Collider collider, Vector3 point, Vector2 velocity)
    {
        collider.attachedRigidbody.isKinematic = false;
        MinigameItem item = collider.attachedRigidbody.GetComponent<MinigameItem>();
        if (!item.moved)
            item.moved = true;
        collider.attachedRigidbody.transform.DOKill();
        collider.attachedRigidbody.transform.parent = null;
        Vector3 worldForce = cam.transform.TransformDirection((new Vector3(velocity.x, 0, velocity.y) + Vector3.ClampMagnitude(forceAdjust * velocity.y, forceAdjust.magnitude)) * hitForce);
        collider.attachedRigidbody.AddForceAtPosition(worldForce, point + positionAdjust);
    }
}
