using System.IO;
using UnityEngine;

/**<summary> Testing ARController in editor </summary>*/
public class ARControllerTest : MonoBehaviour {

    [SerializeField] private string imagePath;
    private LocationController locationController;

    private void Start()
    {
        locationController = FindObjectOfType<LocationController>();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
            ProcessImages(imagePath);
#endif
    }

    /**<summary> Try to get location of random image in path directory </summary>*/
    public void RandomImage()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        imagePath = Application.persistentDataPath.Combine("test");
#endif
        if (imagePath.Contains(".jpg") || imagePath.Contains(".jpeg"))
        {
            ProcessImages(imagePath);
        }
        else
        {
            string[] files = Directory.GetFiles(imagePath);
            ProcessImages(files[Random.Range(0, files.Length)]);
        }
    }

    /**<summary> Process image or folder of images for debugging a session from device </summary>*/
    public void ProcessImages(string path)
    {
        ARController arController = FindObjectOfType<ARController>();
        if (!arController)
            return;

        arController.SnapshotPose();

#pragma warning disable 4014
        if (path.Contains(".jpg") || path.Contains(".jpeg"))
        {
            ServerAPI.LocationFine(locationController.buildingID, FileIO.GetFile(path));
            // -> ARController.OnLocationResponse
        }
        else
        {
            foreach (string file in Directory.GetFiles(path))
            {
                if (file.Contains("jpg") || file.Contains(".jpeg"))
                    ServerAPI.LocationFine(locationController.buildingID, FileIO.GetFile(file));
                // -> ARController.OnLocationResponse
            }
        }
#pragma warning restore 4014
    }

}
