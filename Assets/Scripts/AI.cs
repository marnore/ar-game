using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/**<summary> Navmesh agent move logic </summary>*/
public class AI : MonoBehaviour {

    [SerializeField] private float targetUpdateRate = 1f;
    [SerializeField] private float targetUpdateDelta = 0.1f;
    private float lastUpdateTime;
    internal Transform tr;
    public Vector3 target;
    internal NavMeshAgent agent;

	void Awake ()
    {
        tr = transform;
        agent = GetComponent<NavMeshAgent>();
	}

	virtual public void Update ()
    {
        if (agent.isOnNavMesh && ((Time.time - lastUpdateTime) > targetUpdateRate || Vector2.Distance(new Vector2(target.x, target.z), new Vector2(agent.destination.x, agent.destination.z)) < targetUpdateDelta))
        {
            RandomTarget();
            lastUpdateTime = Time.time;
        }
    }

    /**<summary> Initialising setup </summary>*/
    virtual public void Initialise()
    {
        agent.enabled = true;
        RandomTarget();
    }

    /**<summary> Set random navmesh point in current area as target </summary>*/
    virtual public void RandomTarget()
    {
        if (agent.isOnNavMesh)
        {
            target = RaycastSpawner.GetRandomPoint(WorldController.CurrentAreaBounds());
            agent.destination = target;
        }
    }
}
