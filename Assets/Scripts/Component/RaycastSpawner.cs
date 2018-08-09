using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastSpawner : MonoBehaviour {

    [SerializeField] private float range = 10, maxHeight = 0.3f;
    [SerializeField] private LayerMask layerMask;
    private static RaycastSpawner thisRef;

    /**<summary> Instantiate height offset from floor </summary>*/
    private Vector3 floorOffset { get { return Vector3.up * Random.Range(1, 1.8f); } }

    private void Awake()
    {
        thisRef = this;
    }

    /**<summary> Spawn objects in random positions on collider </summary>*/
    public GameObject[] Spawn(GameObject spawnObject, Parser.Bounds bounds, float minRadius, int count, float maxHeight, Transform parent)
    {
        return Spawn(spawnObject, bounds, Vector3.zero, minRadius, 1000, count, maxHeight, parent);
    }

    /**<summary> Spawn objects in random positions on collider with maximum range from center </summary>*/
    public GameObject[] Spawn(GameObject spawnObject, Parser.Bounds bounds, Vector3 center, float? minRadius, float radius, int count, float maxHeight, Transform parent)
    {
        return Spawn(spawnObject, bounds.GetUnityBounds(), center, minRadius.HasValue ? minRadius.Value : 0, radius, count, maxHeight, parent);
    }

    /**<summary> Spawn objects in random positions on collider </summary>*/
    public GameObject[] Spawn (GameObject spawnObject, Bounds bounds, Vector3 center, float? minRadius, float maxRadius, int count, float maxHeight, Transform parent)
	{
        List<GameObject> objects = new List<GameObject>();
        int cCount = 0;
        int loop = 0;
		while (cCount < count && loop < 10000)
        {
            objects.Add(Spawn(spawnObject, bounds, center, minRadius.HasValue ? minRadius.Value : 0, maxRadius, maxHeight, parent));
            cCount++; loop++;
        }
        return objects.ToArray();
	}

    /**<summary> Spawn a single object in a random position on collider </summary>*/
    public GameObject Spawn(GameObject spawnObject, Parser.Bounds bounds, Vector3 center, float? minRadius, float maxRadius, float maxHeight, Transform parent)
    {
        return Spawn(spawnObject, bounds.GetUnityBounds(), center, minRadius.HasValue ? minRadius.Value : 0, maxRadius, maxHeight, parent);
    }

    /**<summary> Spawn a single object in a random position on collider </summary>*/
    public GameObject Spawn(GameObject spawnObject, Bounds bounds, Vector3 center, float minRadius, float maxRadius, float maxHeight, Transform parent)
    {
        Vector3 point = GetRandomPoint(bounds, center, minRadius, maxRadius);
        int l = 0;
        while (point.y > maxHeight && l < 1000)
        {
            point = GetRandomPoint(bounds, center, minRadius, maxRadius);
            l++;
        }
        return Instantiate(spawnObject, point + floorOffset, Quaternion.identity, parent);
    }

    /**<summary> Get random point on collider by raycasting down until hit </summary>*/
    public static Vector3 GetRandomPoint(Bounds bounds)
    {
        return GetRandomPoint(bounds, bounds.center, 0, 1000);
    }

    /**<summary> Get random point on collider by raycasting down until hit </summary>*/
    public static Vector3 GetRandomPoint(Bounds bounds, Vector3 center, float minRadius, float maxRadius)
    {
        Vector3 point = Vector3.zero;
        int loop = 0;
        while ((point == Vector3.zero || Vector3.Distance(point, center) < minRadius || point.y > thisRef.maxHeight) && loop < 1000)
        {
            Vector3 rayStart = new Vector3(RandomLimitedRange(center.x, bounds.min.x, bounds.max.x, maxRadius),
                                           thisRef.range * 0.8f,
                                           RandomLimitedRange(center.z, bounds.min.z, bounds.max.z, maxRadius));
            RaycastHit hit;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, thisRef.range, thisRef.layerMask))
            {
                point = hit.point;
            }
            loop++;
        }
        return point;
    }

    /**<summary> Get point on collider by raycasting down until hit </summary>*/
    public Vector3 GetPointOnCollider(Vector3 center)
    {
        Vector3 point = center;
        RaycastHit hit;
        if (Physics.Raycast(center, Vector3.down, out hit, range, layerMask))
        {
            point = hit.point;
        }
        return point;
    }

    /**<summary> Get random value limited by bounds and max radius </summary>*/
    public static float RandomLimitedRange (float center, float min, float max, float maxRadius)
    {
        return Random.Range(Mathf.Max(min, center - maxRadius), Mathf.Min(max, center + maxRadius));
    }
}
