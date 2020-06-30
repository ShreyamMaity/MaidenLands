// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class GeoPainterGroup : MonoBehaviour
{
    public GameObject groupParentObj;

    List<GameObject> scatterObjects = new List<GameObject>();
    public List<GameObject> ScatterObjects { get { return scatterObjects; } }

    public List<GeoPainterPoint> myPointsList = new List<GeoPainterPoint>();

    //Brush Rules
    public int rndSeed = 1;

    public float distanceRadius = 0.5f;
    public float sprayRadius = 1;
    public float deleteRadius = 2;

    public float minSlopeVal = 0;
    public float maxSlopeVal = 90;

    public float minAltitudeVal = -10000;
    public float maxAltitudeVal = 10000;

    public bool useNormal = true;
    public bool scaleUniform = true;

    public bool useFoilageGroups = true;

    // ************************************************* //
    public Vector3 posOffset = new Vector3();
    public Vector3 minPosOffset = new Vector3();
    public Vector3 maxPosOffset = new Vector3();

    public Vector3 rotOffset = new Vector3(90f, 0f, 0f);
    public Vector3 minRotOffset = new Vector3(-90f, 0f, 0f);
    public Vector3 maxRotOffset = new Vector3(90f, 0f, 0f);

    public Vector3 scaleOffset = new Vector3();
    public Vector3 minScaleOffset = new Vector3();
    public Vector3 maxScaleOffset = new Vector3();

    public void Init(GameObject _groupParent)
    {
        groupParentObj = _groupParent;
        scatterObjects = new List<GameObject>();
    }

    public void Clean()
    {
        DestroyImmediate(groupParentObj, true);
        foreach (var item in scatterObjects)
        {
            DestroyImmediate(item, true);
        }
    }

    public void AddScatterObject(GameObject _go, Vector3 _pos, Vector3 _scale, Vector3 _normal, bool _useNormal)
    {
        GeoPainterPoint myNewPoint = new GeoPainterPoint();
        myNewPoint.go = _go;
        myNewPoint.pos = _pos;
        myNewPoint.scale = _scale;
        myNewPoint.normal = _normal;
        myNewPoint.useNormal = _useNormal;
        myPointsList.Add(myNewPoint);
    }

    public void RemoveScatterObject()
    {
    }
}