using UnityEngine;
using System.Collections;

/**<summary> Prevent fast rigidbody from passing through colliders </summary>*/
public class CollisionEnforcer : MonoBehaviour
{
    public LayerMask layerMask;
    public float skinWidth = 0.1f;

    private float minimumExtent;
    private float partialExtent;
    private float sqrMinimumExtent;
    private Vector3 previousPosition;
    private Rigidbody rigi;

    void Awake()
    {
        rigi = GetComponent<Rigidbody>();
        previousPosition = rigi.position;
        minimumExtent = Mathf.Min(Mathf.Min(GetComponent<Collider>().bounds.extents.x, GetComponent<Collider>().bounds.extents.y), GetComponent<Collider>().bounds.extents.z);
        partialExtent = minimumExtent * (1.0f - skinWidth);
        sqrMinimumExtent = minimumExtent * minimumExtent;
    }

    void FixedUpdate()
    {
        Vector3 movementThisStep = rigi.position - previousPosition;
        float movementSqrMagnitude = movementThisStep.sqrMagnitude;

        if (movementSqrMagnitude > sqrMinimumExtent)
        {
            float movementMagnitude = Mathf.Sqrt(movementSqrMagnitude);
            RaycastHit hitInfo;
            
            if (Physics.Raycast(previousPosition, movementThisStep, out hitInfo, movementMagnitude, layerMask.value))
                rigi.position = hitInfo.point - (movementThisStep / movementMagnitude) * partialExtent;
        }

        previousPosition = rigi.position;
    }
}
