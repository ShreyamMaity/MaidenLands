using UnityEngine;


public abstract class CameraBase : MonoBehaviour
{
    public Transform target;
    protected TPCameraCollision collisionHandler;

    protected bool isColliding;
    protected bool debug;
    protected Vector3 targetPos = new Vector3();
    protected float collisionDistance;


    public virtual void Start() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void LateUpdate() { }


    public abstract void UpdatePosition();
    public abstract void UpdateRotation();



    public void CheckCollision()
    {
        collisionHandler.UpdateClipPoints(targetPos, ref collisionHandler.desiredClipPoints);
        // collisionHandler.UpdateClipPoints(thisCam.position, ref collisionHandler.adjustedClipPoints);

        collisionHandler.CheckColliding(target.position);

        // if camera collision or occlusion occurs this sets the desired distance.
        collisionDistance = collisionHandler.GetAdjustedDistanceFromRay(targetPos, collisionHandler.desiredClipPoints);

        // check for perpendicular collisions.
        // collisionHandler.CheckPerpendicularCollisions(collisionHandler.adjustedClipPoints).

        // debug.
        if (debug)
            for (int i = 0; i < 5; i++)
            {
                Debug.DrawRay(target.position, collisionHandler.adjustedClipPoints[i] - target.position, Color.blue);
                Debug.DrawRay(target.position, collisionHandler.desiredClipPoints[i] - target.position, Color.green);
            }
    }
}
