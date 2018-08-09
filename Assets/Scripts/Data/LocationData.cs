using UnityEngine;

/**<summary> Location data structure </summary>*/
public struct LocationData
{
    public Vector3 position;
    public Vector3 direction;
    public float angle;
    public Quaternion rotation;
    public string areaID;

    /**<summary> Location data structure </summary>*/
    public LocationData(Vector3 position, Vector3 direction, Quaternion rotation, string areaID)
    {
        this.position = position;
        this.direction = direction;
        this.angle = Quaternion.LookRotation(direction, Vector3.up).eulerAngles.y;
        this.rotation = rotation;
        this.areaID = areaID;
    }

    /**<summary> Location data structure </summary>*/
    public LocationData(Vector3 position, Vector3 direction, Quaternion rotation)
    {
        this.position = position;
        this.direction = direction;
        this.angle = Quaternion.LookRotation(direction, Vector3.up).eulerAngles.y;
        this.rotation = rotation;
        this.areaID = "";
    }

    override public string ToString()
    {
        return "position: " + position + "\ndirection: " + direction + "\nangle: " + angle + "\norientation: " + rotation;
    }
}
