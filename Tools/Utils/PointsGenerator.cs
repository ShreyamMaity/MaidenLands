using System.Collections.Generic;
using UnityEngine;
using Location_Tool;


/// <summary>
/// This is just a "DEMO" script and needs to be just taken as a refrence or to rework to suit your need,
/// it also has an Editor "PointsGenEditor".
/// </summary>
[System.Serializable]
public class PointsGenerator : MonoBehaviour
{
    // public
    // this is refrence object that is needed for repositioning points after generating them.
    [HideInInspector]
    public GameObject refObj = null;

    [HideInInspector]
    public Location location;

    List<Vector3> points = null;
    [HideInInspector]
    public List<Vector3> Points { get { return points; } }

    [HideInInspector]
    public float pointRadius = 0.8f;
    [HideInInspector]
    public string locationName = "";

    [HideInInspector]
    public bool debugMode = true;

    // private
    Vector3 centre = Vector3.zero;
    Vector3 size = Vector3.zero;

    PoissonDiscSampler sampler;

    void OnEnable()
    {
    }

    void InitRefObj()
    {
        refObj = new GameObject();
    }

    void Start()
    {
    }

    public void GeneratePoints(Location l = null)
    {
        if (refObj == null) InitRefObj();

        location = LocationManager.GetLocation(locationName);

        if (l != null) location = l;

        if (location == null)
        { Debug.Log("Location not found"); return; }

        if (location.boundaries.Count < 4)
        { Debug.Log("location must have 4 or more boundary points"); return; }

        float minPos = (MaxBounds.x - MinBounds.x);
        float maxPos = (MaxBounds.z - MinBounds.z);

        // region is the min rectungular box that encloses, the bounderies of location 
        Vector3 region = new Vector3(minPos, 0, maxPos);

        centre = location.GetCentre();
        Debug.Log(centre);
        size = new Vector3(MaxBounds.x - MinBounds.x, 0f, MaxBounds.z - MinBounds.z);

        // corner of min rectungular box that encloses, the bounderies of location 
        Vector3 refPos = new Vector3(centre.x - (size.x / 2), 0, centre.z - (size.z / 2));

        refObj.transform.position = refPos;

        points = new List<Vector3>();
        sampler = new PoissonDiscSampler(region.x, region.z, pointRadius); // the sampler generates the points in 2d
        // convert sampler generated points to 3d and setting Y height comp to the heighest point in location.
        float heightestPoint = location.HeighestPoint();
        foreach (var item in sampler.Samples())
        {
            points.Add(new Vector3(item.x, heightestPoint, item.y));
        }
        List<Vector3> refPoints = new List<Vector3>();

        // a simple test to check if point is inside the bounderies of this location
        foreach (Vector3 point in points)
        {
            Vector3 finalPoint = refObj.transform.TransformPoint(point);
            if (Utils.GeomUtils2d.PointInsidePolygon(
                                                    location.Bounderies2d,
                                                    location.Bounderies2d.Length,
                                                    new Vector2(finalPoint.x, finalPoint.z)))
            {
                refPoints.Add(finalPoint);
            }

        }

        // refPoints = TestForRedZone(refPoints);
        points = refPoints;
        DestroyImmediate(refObj);
        Debug.Log("gen " + (points.Count));
    }

    public Vector3 MinBounds { get { return location.GetMinBounds(); } }
    public Vector3 MaxBounds { get { return location.GetMaxBounds(); } }

    /// <summary>
    /// if a point is !inside this location it will be added,
    /// all other points are discarded
    /// </summary>
    public List<Vector3> TestForRedZone(List<Vector3> refPoints)
    {
        List<Vector3> sampels = new List<Vector3>();

        //foreach (var item in exclusiveZones)
        //{
        //    Location redLocation = LocationManager.GetLocation(item);
        //    if (redLocation == null)
        //    {
        //        return refPoints;
        //    }

        //    for (int i = 0; i < refPoints.Count; i++)
        //    {
        //        Vector3 point = refPoints[i];
        //        if (!PointInLocation(redLocation, point))
        //        {
        //            sampels.Add(point);
        //        }
        //    }
        //}

        return sampels;
    }

    bool PointInLocation(Location l, Vector3 testPoint)
    {
        if (Utils.GeomUtils2d.PointInsidePolygon(l.Bounderies2d, l.Bounderies2d.Length, new Vector2(testPoint.x, testPoint.z)))
        {
            return true;
        }
        return false;
    }


    /// <summary>
    /// This is just a "Demo" function.
    /// Distributes the points across the location, based on the position 
    /// of Generated Points.
    /// </summary>
    public void Distribute(FoilageGroup group)
    {
        if (Points == null || group == null)
        {
            Debug.Log("Points not generated or foilage groups is null");
            return;
        }

        RaycastHit hitInfo;
        // create a parent for distributed objects
        GameObject parent = new GameObject();
        parent.name = location.name + " -Parent";

        foreach (Vector3 position in Points)
        {
            if (Physics.Raycast(position, Vector3.down, out hitInfo)) // create a ray from destinaion position in -Y direction.
            {
                // choose a random object from foilage group objects
                int rand = Random.Range(0, group.groupObjects.Count);
                GameObject originalObj = group.groupObjects[rand];

                if (originalObj.GetComponent<GrassPatch>())
                {
                    GameObject newObj = Instantiate(originalObj);
                    newObj.transform.position = hitInfo.point + new Vector3(0, 3f, 0);
                    newObj.transform.parent = parent.transform;

                    group.Randomize(newObj, group.foilageRules[rand]);

                    GrassPatch instPatch = newObj.GetComponent<GrassPatch>();
                    GrassPatch originalPatch = originalObj.GetComponent<GrassPatch>();

                    for (int i = 0; i < originalPatch.grassPlanes.Count; i++)
                    {
                        GrassPlane originalPlane = originalPatch.grassPlanes[i]; // original patch
                        GrassPlane instPlane = instPatch.grassPlanes[i]; // instantiated patch

                        instPlane.transform = Instantiate(originalPlane.transform.gameObject).transform;
                        instPlane.transform.parent = newObj.transform;

                        // copy settings
                        instPlane.groundVerts = originalPlane.groundVerts;

                        // make position its relative position is same as original
                        Vector3 worldPos = newObj.transform.TransformPoint(originalPlane.transform.localPosition);
                        Vector3 localPos = newObj.transform.InverseTransformPoint(worldPos);
                        instPlane.transform.localPosition = localPos;

                        // make it rotate according to ground normal
                        hitInfo = new RaycastHit();
                        if (Physics.Raycast(instPlane.transform.position, -Vector3.up*5f, out hitInfo, LayerMask.NameToLayer("Ground")))
                        {
                            if(group.foilageRules[rand].useNormal) instPlane.transform.localEulerAngles = hitInfo.normal;
                            instPlane.transform.position = hitInfo.point;
                        }
                    }
                    instPatch.Place();
                }
            }
        }
    }

    void GetLocation()
    {
        location = LocationManager.GetLocation(locationName);
    }

    public void ClearPoints()
    {
        if(points != null)
            points.Clear();
    }
}
