using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Simplified aerodynamic physics for spherical objects </summary>*/
public class BallPhysics : MonoBehaviour
{
    [SerializeField] private float magnusCoefficient = 0.01f;

    private Rigidbody rigi;
    private Collider col;
    private float radius;

    private void Start()
    {
        rigi = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        radius = col.bounds.extents.magnitude;
        rigi.maxAngularVelocity = 300;
    }

    private void FixedUpdate()
    {
        rigi.AddForce(magnusCoefficient * Mathf.PI * Mathf.PI * Mathf.Pow(radius, 3) * Vector3.Cross(rigi.velocity, -rigi.angularVelocity));
    }

}
