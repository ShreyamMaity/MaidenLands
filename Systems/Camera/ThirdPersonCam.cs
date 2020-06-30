using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ThirdPersonCam : MonoBehaviour
{

    [System.Serializable]
    public class PositionSettings
    {
        public Vector3 targetPosOffset;
        public float lookSmooth;
        public float distanceToTarget;
        public float adjustmentDistance;
        public float zoomSmooth;
        public float minZoom;
        public float maxZoom;

        public PositionSettings()
        {
            targetPosOffset = new Vector3(0f, 1.5f, 0f);
            lookSmooth = 100f;
            zoomSmooth = 100f;
            minZoom = 2f;
            maxZoom = 3f;
        }
    }

    [System.Serializable]
    public class OrbitSettings
    {
        public float xRotation;
        public float yRotation;
        public float maxXRotation;
        public float minXRotation;
        public float orbitSmooth;

        public OrbitSettings()
        {
            orbitSmooth = 50f;
        }
    }

    [System.Serializable]
    public class AimSettings
    {
        public Vector3 aimOffset;
        public float aimSpeed = 0.5f;
        public float aimSmooth = 30f;
    }

    public Transform target;
    public Camera cam;

    public bool lockOrbit = true;
    public bool debugMode = false;

    public PositionSettings positionSettings = new PositionSettings();
    public OrbitSettings orbitSettings = new OrbitSettings();
    public AimSettings aimSettings = new AimSettings();
    TPCameraCollision collisionHandler;

    Vector3 targetPos = Vector3.zero;
    Vector3 destination = Vector3.zero;
    Vector3 adjustedDestination = Vector3.zero;

    void Start()
    {
        collisionHandler = this.GetComponent<TPCameraCollision>();
        cam = this.GetComponent<Camera>();

        MoveToTarget();

        // collisionHandler.UpdateClipPoints(destination, ref collisionHandler.desiredClipPoints);
        // collisionHandler.UpdateClipPoints(transform.position, ref collisionHandler.adjustedClipPoints);
        // positionSettings.adjustmentDistance = positionSettings.distanceToTarget;
    }

    void Update()
    {
        Zoom();
    }

    void LateUpdate()
    {
        MoveToTarget();
        LookAtTarget();
        Orbit();
    }

    void FixedUpdate()
    {
    }

    void MoveToTarget()
    {
        targetPos = target.position + positionSettings.targetPosOffset;
        destination = Quaternion.Euler(orbitSettings.xRotation, orbitSettings.yRotation /*+ target.localEulerAngles.y*/, 0) * -Vector3.forward * positionSettings.distanceToTarget;
        destination += targetPos;
        transform.position = Vector3.Lerp(transform.position, destination, positionSettings.zoomSmooth);
    }

    void LookAtTarget()
    {
        Quaternion lookRotation = Quaternion.LookRotation((target.position + positionSettings.targetPosOffset) - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, positionSettings.lookSmooth * Time.deltaTime);
    }

    void Orbit()
    {
        if (!lockOrbit)
        {
            orbitSettings.yRotation += InputManager.mouse_axis.x * orbitSettings.orbitSmooth * Time.deltaTime;
            orbitSettings.yRotation = MouseLook.ClampAngle(orbitSettings.yRotation, -360f, 360f);
        }
    }

    void Zoom()
    {
        positionSettings.distanceToTarget += InputManager.zoom * positionSettings.zoomSmooth * Time.deltaTime;

        if (positionSettings.distanceToTarget < positionSettings.minZoom)
        {
            positionSettings.distanceToTarget = positionSettings.minZoom;
        }

        if (positionSettings.distanceToTarget > positionSettings.maxZoom)
        {
            positionSettings.distanceToTarget = positionSettings.maxZoom;
        }
    }
}
