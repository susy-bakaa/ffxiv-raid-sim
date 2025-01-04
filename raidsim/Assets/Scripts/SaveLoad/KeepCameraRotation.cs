using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ThirdPersonCamera))]
public class KeepCameraRotation : MonoBehaviour
{
    ThirdPersonCamera thirdPersonCamera;

    public static KeepCameraRotation instance;

    public static Vector3 SavedPosition = Vector3.zero;
    public static Quaternion SavedRotation = Quaternion.identity;
    public static float savedDistanceFromTarget = 0f;
    private static bool saved = false;

    public bool saveAutomatically = true;

    private void Awake()
    {
        instance = this;
        thirdPersonCamera = GetComponent<ThirdPersonCamera>();
    }

    private void Start()
    {
        if (saved)
            LoadCamera();
    }

    private void OnDisable()
    {
        if (saveAutomatically)
        {
            SaveCamera();
        }
    }

    public static void LoadCamera()
    {
        CameraSaveData data = new CameraSaveData(SavedPosition, SavedRotation, savedDistanceFromTarget);
        if (instance != null)
        {
            instance.transform.position = SavedPosition;
            instance.transform.rotation = SavedRotation;
            instance.thirdPersonCamera.dstFromTarget = savedDistanceFromTarget;
            instance.thirdPersonCamera.SetCameraTransform(data);
        }
    }

    public static void SaveCamera()
    {
        if (instance != null)
        {
            SavedPosition = instance.transform.position;
            SavedRotation = instance.transform.rotation;
            savedDistanceFromTarget = instance.thirdPersonCamera.dstFromTarget;
            saved = true;
        }
    }

    public struct CameraSaveData
    {
        public Vector3 position;
        public Quaternion rotation;
        public float distance;

        public CameraSaveData(Vector3 position, Quaternion rotation, float distance)
        {
            this.position = position;
            this.rotation = rotation;
            this.distance = distance;
        }
    }
}
