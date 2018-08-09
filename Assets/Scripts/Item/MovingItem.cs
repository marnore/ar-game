using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MovingItem : AI
{
    [SerializeField] private Transform visual;

    [SerializeField] private float minHeight = 0, maxHeight = 0;
    [SerializeField] private float heightUpdateRate;
    [SerializeField] private float speed;
    
    private float lastHeightUpdateTime;
    private bool isHit = false;

    override public void Update()
    {
        if (isHit)
            return;
        base.Update();
        if (Time.time - lastHeightUpdateTime > heightUpdateRate)
            RandomizeHeight();

        Vector3 targetPosition = visual.position;
        targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);
        visual.position = targetPosition;

        targetPosition.y = target.y;
        visual.position = Vector3.Lerp(visual.position, targetPosition, Time.deltaTime * speed);
    }

    private void OnEnable()
    {
        RandomTarget();
    }

    /**<summary> Randomize target height of visuals </summary>*/
    private void RandomizeHeight ()
    {
        target.y = Random.Range(minHeight, maxHeight);
        lastHeightUpdateTime = Time.time;
    }
}
