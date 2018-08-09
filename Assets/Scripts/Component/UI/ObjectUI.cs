using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

/**<summary> World space UI for objects (with stats) </summary>*/
public class ObjectUI : MonoBehaviour
{
    private void Start()
    {
        ConstraintSource constraint = new ConstraintSource();
        constraint.sourceTransform = Camera.main.transform;
        constraint.weight = 1;
        GetComponent<AimConstraint>().AddSource(constraint);
    }
}
