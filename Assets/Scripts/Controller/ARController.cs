using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using GoogleARCore;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GoogleARCore.Examples.ComputerVision;
#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

/**<summary> Controller for ARCore and image based location </summary>*/
public class ARController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject trackedPlaneVisualiser;
    [SerializeField] private bool disableARControl;

    private TextureReader textureReader;
    private Transform tr, cameraTransform;
    private LocationController locationController;
    private WorldController worldController;

    private List<DetectedPlane> m_NewPlanes = new List<DetectedPlane>();
    private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();

    private bool located = false;
    private bool tracking = false;
    private bool searchingPlanes = true;
    private float lastObjectUpdateTime;
    private float lastLocationTime;
    private bool locationRequestInProgress;

    private Vector3 moveOrigin;
    private Vector3 rotateOrigin;
    private float worldAngle;

    private Vector3 positionOnImageCapture;
    private float angleOnImageCapture;
    private int frame, lastFrame;

    private void Awake()
    {
        tr = transform;
        locationController = FindObjectOfType<LocationController>();
        worldController = FindObjectOfType<WorldController>();
        textureReader = GetComponent<TextureReader>();
        cameraTransform = mainCamera.transform;
    }

    private void Start()
    {
        ServerAPI.OnLocationResponse += OnLocationResponse;
        textureReader.OnImageAvailableCallback += OnImageCaptured;
    }

    /**<summary> Search image location continously </summary>*/
    public bool AutomaticLocationSync { get; set; }

    public void StartTracking ()
    {
        tracking = true;
    }

    /**<summary> Has a location been found (using image based Location API) </summary>*/
    public bool Located()
    {
        return located;
    }

    /**<summary> Is ARCore searching for planes </summary>*/
    public bool SearchingPlanes()
    {
        return searchingPlanes;
    }

    /**<summary> Get location with image based location API </summary>*/
    public void GetLocation()
    {
        if (App.config.debug)
            Debug.Log("Starting location request");
        locationRequestInProgress = true;
#if UNITY_EDITOR
        OnImageCapturedMock(0.2f);
#else
        textureReader.CaptureImage();
#endif
    }

    /**<summary> Mock image capture locations with response delay </summary>*/
    private async void OnImageCapturedMock (float delay)
    {
        angleOnImageCapture = Frame.Pose.rotation.eulerAngles.y;
        positionOnImageCapture = Frame.Pose.position;
        lastLocationTime = Time.time;

        // Wait for simulated response time
        await Task.Delay((int)(delay*1000));

        await ServerAPI.LocationFineDummy("dummy", new byte[0]);
    }

    /**<summary> Called when TextureReader returns image buffer </summary>*/
    private async void OnImageCaptured(TextureReaderApi.ImageFormatType format, int width, int height, IntPtr pixelBuffer, int bufferSize)
    {
        // Use threads to not block render thread with image handling
        //TODO ? Do jpg encoding withouth Unity Texture2D which cannot be created in another thread
        byte[] result = null;
        byte[] encodedJpg = null;

        await Task.Run(() =>
        {
            // 4 bytes per pixel, RGBA
            byte[] imageBytes = new byte[width * height * 4];
            Marshal.Copy(pixelBuffer, imageBytes, 0, imageBytes.Length);

            // Get pixels and apply transforms
            // TODO: check if default orientation is device specific
            Color32[] pixels = ImageUtils.GetPixels(imageBytes);
            pixels = ImageUtils.Flip(pixels, width, height, true);
            pixels = ImageUtils.Rotate(pixels, width, height, false);
            result = ImageUtils.Color32ArrayToByteArray(pixels);
        });

        // Create texture with the image data
        Texture2D texture = new Texture2D(height, width, TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(result);

        await Task.Run(() =>
        {
            // Get jpg encoded bytedata
            encodedJpg = texture.EncodeToJPG(80);
            //File.WriteAllBytes(Application.persistentDataPath.Combine(DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".jpg"), encodedJpg);

            // For delta offset
            SnapshotPose();
        });

        lastLocationTime = Time.time;
        if (App.config.debug)
            Debug.Log("Starting POST request for /location/fine");

        // Send image to LocationAPI
        await ServerAPI.LocationFine(locationController.buildingID, encodedJpg);
        // -> OnLocationResponse

    }

    /**<summary> Called when response is got from image based Location API server </summary>*/
    private void OnLocationResponse(bool locationFound, LocationData locationData)
    {
        located = locationFound;
        if (!locationFound)
        {
            Debug.Log("Image location not detected");
            locationRequestInProgress = false;
            return;
        }

        if (App.config.debug)
        {
            print(locationData);
        }

        // Calculate origins and world angle
        moveOrigin = locationData.position - positionOnImageCapture;
        rotateOrigin = positionOnImageCapture;
        worldAngle = locationData.angle - angleOnImageCapture;

        // Align ARCore visuals (planes and point cloud)

        LocationData location = CalculateNewLocation(moveOrigin, worldAngle, rotateOrigin, Frame.Pose.position, Frame.Pose.rotation);
        tr.position = location.position - Frame.Pose.position;
        tr.rotation = Quaternion.identity;
        tr.RotateAround(location.position, Vector3.up, worldAngle);
        // Update world objects
        worldController.UpdateObjects();

        locationRequestInProgress = false;
    }

    /**<summary> Save snapshot of Pose for delta offset </summary>*/
    public void SnapshotPose()
    {
        angleOnImageCapture = Frame.Pose.rotation.eulerAngles.y;
        positionOnImageCapture = Frame.Pose.position;
    }

    /**<summary> Calculate final location data using ARCore and image based location data </summary>*/
    public LocationData CalculateNewLocation(Vector3 origin, float angle, Vector3 rotationOrigin, Vector3 position, Quaternion rotation)
    {
        Quaternion angleRotation = Quaternion.AngleAxis(angle, Vector3.up);
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(angleRotation);

        position -= rotationOrigin;
        Vector3 newPosition = rotationMatrix.MultiplyVector(position);
        newPosition += rotationOrigin;
        newPosition += origin;

        Quaternion newRotation = rotation;
        Vector3 newDirection = angleRotation * Vector3.forward;

        return new LocationData(newPosition, newDirection, newRotation);
    }

    public Vector3 GetPointOnPlane (Vector3 point)
    {
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal;

        if (Frame.Raycast(point.x, point.z, raycastFilter, out hit))
        {
            return hit.Pose.position;
        }
        return point;
    }

    private void Update()
    {        
        if (!tracking || string.IsNullOrEmpty(locationController.buildingID))
            return;
        UpdateARLocation();

        if (Session.Status != SessionStatus.Tracking)
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            if (Session.Status.IsValid())
                searchingPlanes = true;
            return;
        }
        while (frame > lastFrame + 60)
        {
            ARCPlanes();
            lastFrame = frame;
        }
        frame++;
    }

    /**<summary> Update ARCore location in world aligned with image location </summary>*/
    private void UpdateARLocation()
    {
        // Search for location if automatic sync on
        if (AutomaticLocationSync && !locationRequestInProgress && (Time.time - lastLocationTime > App.config.locationRefreshRate || !located))
        {
            GetLocation();
        }

        if (!disableARControl)
        {
            LocationData location = CalculateNewLocation(moveOrigin, worldAngle, rotateOrigin, Frame.Pose.position, Frame.Pose.rotation);
            cameraTransform.position = location.position;
            cameraTransform.rotation = Quaternion.AngleAxis(worldAngle, Vector3.up) * location.rotation;
        }
    }

    /**<summary> Manage ARCore planes / surfaces </summary>*/
    private void ARCPlanes()
    {
        Session.GetTrackables(m_NewPlanes, TrackableQueryFilter.New);
        for (int i = 0; i < m_NewPlanes.Count; i++)
        {
            GameObject planeObject = Instantiate(trackedPlaneVisualiser, tr.position, Quaternion.identity, tr);
            planeObject.GetComponent<ARPlane>().Initialize(m_NewPlanes[i]);
        }

        Session.GetTrackables(m_AllPlanes);
        searchingPlanes = true;
        for (int i = 0; i < m_AllPlanes.Count; i++)
        {
            if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
            {
                searchingPlanes = false;
                break;
            }
        }
        // Update world objects
        if (m_NewPlanes.Count > 0)
        {
            worldController.UpdateObjects();
        }
    }
}
