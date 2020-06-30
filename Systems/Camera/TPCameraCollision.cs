using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TPCameraCollision : MonoBehaviour
{
    [HideInInspector]
    public Vector3[] adjustedClipPoints = new Vector3[5];

    [HideInInspector]
    public Vector3[] desiredClipPoints = new Vector3[5];

    [HideInInspector]
    public float adjustedDistance;

    Camera _camera;
    [HideInInspector]
    public bool colliding;
    public LayerMask collisionMask;

    void Start()
    {
        _camera = this.GetComponent<Camera>();
    }

    public void UpdateClipPoints(Vector3 camPos, ref Vector3[] clipPoints)
    {
        if (!this._camera)
            return;

        clipPoints = new Vector3[5];

        // Transform _camera = this._camera.transform;

        float halfFOV = (this._camera.fieldOfView / 2) * Mathf.Deg2Rad;
        float aspect = this._camera.aspect;
        float distance = this._camera.nearClipPlane;
        float height = distance * Mathf.Tan(halfFOV);
        float width = height * aspect;
        // width = height / aspect;

        // lower right
        clipPoints[0] = camPos + _camera.transform.right * width;
        clipPoints[0] -= _camera.transform.up * height;
        clipPoints[0] += _camera.transform.forward * distance;

        // lower left
        clipPoints[1] = camPos - _camera.transform.right * width;
        clipPoints[1] -= _camera.transform.up * height;
        clipPoints[1] += _camera.transform.forward * distance;

        // upper right
        clipPoints[2] = camPos + _camera.transform.right * width;
        clipPoints[2] += _camera.transform.up * height;
        clipPoints[2] += _camera.transform.forward * distance;

        // upper left
        clipPoints[3] = camPos - _camera.transform.right * width;
        clipPoints[3] += _camera.transform.up * height;
        clipPoints[3] += _camera.transform.forward * distance;

        // middle
        clipPoints[4] = camPos + this._camera.transform.forward * -this._camera.nearClipPlane;
    }

    public bool CollisionDetectedAtClipPoints(Vector3 from)
    {

        // loop through all the clip points and check for collision or occlusion
        // and get adjusted distance is case of collision or occlusion

        for (int i = 0; i < desiredClipPoints.Length; i++)
        {
            Ray ray = new Ray(from, desiredClipPoints[i] - from);
            RaycastHit hit;
            float rayDist = Vector3.Distance(desiredClipPoints[i], from);

            // if collision or occlusion occurs
            if (Physics.Raycast(ray, out hit, rayDist, collisionMask))
            { 
                // return true 
                return true;
            }
        }

        // else return false
        return false;
    }

    public float GetAdjustedDistanceFromRay(Vector3 from, Vector3[] clipPoints)
    {
        float distance = -1;

        for (int i = 0; i < clipPoints.Length; i++)
        {
            Ray ray = new Ray(from, clipPoints[i] - from);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (distance == -1)
                    distance = hit.distance;
                else
                {
                    if (hit.distance < distance)
                    {
                        distance = hit.distance;
                    }
                }
            }
        }

        if (distance == -1)
            return 0;
        else
            return distance;
    }

    public void CheckColliding(Vector3 from)
    {
        if (CollisionDetectedAtClipPoints(from))
        {
            colliding = true;
        }
        else
        {
            colliding = false;
        }
    }
}
