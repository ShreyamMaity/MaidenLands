using UnityEngine;


public class CameraManager : MonoBehaviour
{
    CameraBase currentCam;

    void Start()
    {
    }

    void LateUpdate()
    {
    }

    void FixedUpdate()
    {
    }

    void SwitchCam()
    {
    }
}


//public class PlayerCamera : MonoBehaviour
//{
//    [System.Serializable]
//    public class PositionSettings
//    {
//        public Vector3 targetPosOffset;
//        public float lookSmooth;
//        public float distanceToTarget;
//        public float adjustmentDistance;
//        public float zoomSmooth;
//        public float minZoom;
//        public float maxZoom;

//        public PositionSettings()
//        {
//            targetPosOffset = new Vector3(0f, 1.5f, 0f);
//            lookSmooth = 100f;
//            zoomSmooth = 100f;
//            minZoom = 2f;
//            maxZoom = 3f;
//        }
//    }

//    [System.Serializable]
//    public class OrbitSettings
//    {
//        public float xRotation;
//        public float yRotation;
//        public float maxXRotation;
//        public float minXRotation;
//        public float orbitSmooth;

//        public OrbitSettings()
//        {
//            orbitSmooth = 50f;
//        }
//    }

//    [System.Serializable]
//    public class AimSettings
//    {
//        public Vector3 aimOffset;
//        public float aimSpeed = 0.5f;
//        public float aimSmooth = 30f;
//    }

//    public Transform target;
//    public Camera cam;

//    public bool lockOrbit = true;
//    public bool DebugMode = false;
//    bool needToSwitchCam = false;

//    public PositionSettings positionSettings = new PositionSettings();
//    public OrbitSettings orbitSettings = new OrbitSettings();
//    public AimSettings aimSettings = new AimSettings();
//    TPCameraCollision collisionHandler;

//    Vector3 targetPos = Vector3.zero;
//    Vector3 destination = Vector3.zero;
//    Vector3 adjustedDestination = Vector3.zero;

//    void Start()
//    {
//        cam = Camera.main;
//        collisionHandler = this.GetComponent<TPCameraCollision>();

//        MoveToTarget();

//        collisionHandler.UpdateClipPoints(destination, ref collisionHandler.desiredClipPoints);
//        collisionHandler.UpdateClipPoints(transform.position, ref collisionHandler.adjustedClipPoints);

//        positionSettings.adjustmentDistance = positionSettings.distanceToTarget;
//    }


//    float mapToRange(float rotation)
//    {
//        if (rotation > Mathf.PI)
//            return rotation - 2 * Mathf.PI;
//        if (rotation < -Mathf.PI)
//            return rotation + 2 * Mathf.PI;
//        return rotation;
//    }

//    void Update()
//    {
//        Zoom();

//        // rotations as internally represented as quaternions hense radians.
//        // conversions are necessary.
//        // float rotation = transform.eulerAngles.y * Mathf.Deg2Rad;
//        // rotation = mapToRange(rotation);
//    }

//    void LateUpdate()
//    {
//        MoveToTarget();
//        LookAtTarget();
//        Orbit();

//    }

//    void FixedUpdate()
//    {
//        // ***************COLLISION AND OCCLUSION HANDLER**************** //

//        collisionHandler.UpdateClipPoints(destination, ref collisionHandler.desiredClipPoints);
//        collisionHandler.UpdateClipPoints(transform.position, ref collisionHandler.adjustedClipPoints);

//        collisionHandler.CheckColliding(targetPos);

//        // if camera collision or occlusion occurs this sets the desired distance.
//        positionSettings.adjustmentDistance = collisionHandler.GetAdjustedDistanceFromRay(targetPos, collisionHandler.desiredClipPoints);

//        // check for perpendicular collisions.
//        // collisionHandler.CheckPerpendicularCollisions(collisionHandler.adjustedClipPoints);

//        // debug
//        if (DebugMode)
//            for (int i = 0; i < 5; i++)
//            {
//                Debug.DrawRay(targetPos, collisionHandler.adjustedClipPoints[i] - targetPos, Color.blue);
//                Debug.DrawRay(targetPos, collisionHandler.desiredClipPoints[i] - targetPos, Color.green);
//            }

//        // ***************COLLISION AND OCCLUSION HANDLER**************** //
//    }

//    void MoveToTarget()
//    {
//        targetPos = target.position + positionSettings.targetPosOffset;

//        if (collisionHandler.colliding)
//        {
//            adjustedDestination = Quaternion.Euler(orbitSettings.xRotation, orbitSettings.yRotation, 0) * -Vector3.forward * positionSettings.adjustmentDistance;
//            adjustedDestination += targetPos;
//            transform.position = Vector3.Lerp(transform.position, adjustedDestination, positionSettings.zoomSmooth);
//        }
//        else
//        {
//            destination = Quaternion.Euler(orbitSettings.xRotation, orbitSettings.yRotation /*+ target.localEulerAngles.y*/, 0) * -Vector3.forward * positionSettings.distanceToTarget;
//            destination += targetPos;
//            transform.position = Vector3.Lerp(transform.position, destination, positionSettings.zoomSmooth);
//        }
//    }

//    void LookAtTarget()
//    {
//        Quaternion lookRotation = Quaternion.LookRotation((target.position + positionSettings.targetPosOffset) - transform.position);
//        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, positionSettings.lookSmooth * Time.deltaTime);
//    }

//    void Orbit()
//    {
//        // orbitSettings.xRotation += -vOrbitInput * orbitSettings.orbitSmooth * Time.deltaTime;
//        if (!lockOrbit)
//        {
//            orbitSettings.yRotation += InputManager.mouse_axis.x * orbitSettings.orbitSmooth * Time.deltaTime;
//        }
//        else
//        {
//            orbitSettings.yRotation = Mathf.Lerp(orbitSettings.yRotation, target.parent.GetComponent<Player_Controller>().transform.eulerAngles.y, 10f * Time.deltaTime);
//        }
//    }

//    void Zoom()
//    {
//        positionSettings.distanceToTarget += InputManager.zoom * positionSettings.zoomSmooth * Time.deltaTime;

//        if (positionSettings.distanceToTarget < positionSettings.minZoom)
//        {
//            positionSettings.distanceToTarget = positionSettings.minZoom;
//        }

//        if (positionSettings.distanceToTarget > positionSettings.maxZoom)
//        {
//            positionSettings.distanceToTarget = positionSettings.maxZoom;
//        }
//    }
//}
