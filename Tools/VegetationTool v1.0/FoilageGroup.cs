using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class FoilageRules
{
    // foilage properties
    public int rndSeed = 1;

    public float minSlopeVal = 0;
    public float maxSlopeVal = 90;

    public float minAltitudeVal = -10000;
    public float maxAltitudeVal = 10000;

    public bool useNormal = true;

    // ************************************************* //
    // scatter settings
    public Vector3 posOffset = new Vector3();
    public Vector3 minPosOffset = new Vector3();
    public Vector3 maxPosOffset = new Vector3();

    public Vector3 rotOffset = new Vector3(0f, 90f, 0f);
    public Vector3 minRotOffset = new Vector3(0f, -90f, 0f);
    public Vector3 maxRotOffset = new Vector3(0f, 90f, 0f);

    public float minScaleOffset = -0.25f;
    public float maxScaleOffset = 0.25f;
}


[System.Serializable]
public class ObjectAndIndex<T>
{
    public T obj;
    public int index = -1;

    public ObjectAndIndex(T _obj, int _index)
    {
        obj = _obj;
        index = _index;
    }
}


public class FoilageGroup : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> groupObjects = new List<GameObject>();
    public Dictionary<int, FoilageRules> foilageRules = new Dictionary<int, FoilageRules>();

    // public List<ObjectAndIndex<FoilageRules>> _foilageRules = new List<ObjectAndIndex<FoilageRules>>();

    [HideInInspector]
    public int selectedIndex = -1;

    public void Init()
    {
        groupObjects = new List<GameObject>();
        foilageRules = new Dictionary<int, FoilageRules>();
        selectedIndex = -1;
    }

    public List<GameObject> GetFoilageGroups()
    {
        return groupObjects;
    }

    public void Add()
    {
        groupObjects.Add(null);
    }

    public void AddEntry(int key)
    {
        if (!foilageRules.ContainsKey(key) && groupObjects[key] != null)
        {
            foilageRules.Add(key, new FoilageRules());
            Debug.Log("key added " + key);
        }
    }

    public void Remove(int key)
    {
        if (foilageRules.ContainsKey(key))
            foilageRules.Remove(key);

        groupObjects.RemoveAt(key);
        Debug.Log("key removed " + key);
    }

    public void Select(int key)
    {
        if(foilageRules.ContainsKey(key) && groupObjects[key] != null)
            selectedIndex = key;
    }

    public void Randomize(GameObject obj, FoilageRules r)
    {
        // Position
        var tmpPosX = r.posOffset.x + UnityEngine.Random.Range(r.minPosOffset.x, r.maxPosOffset.x);
        var tmpPosY = r.posOffset.y + UnityEngine.Random.Range(r.minPosOffset.y, r.maxPosOffset.y);
        var tmpPosZ = r.posOffset.z + UnityEngine.Random.Range(r.minPosOffset.z, r.maxPosOffset.z);
        obj.transform.Translate(tmpPosX, tmpPosY, tmpPosZ);

        // Rotation
        var tmpRotX = r.rotOffset.x + UnityEngine.Random.Range(r.minRotOffset.x, r.maxRotOffset.x);
        var tmpRotY = r.rotOffset.y + UnityEngine.Random.Range(r.minRotOffset.y, r.maxRotOffset.y);
        var tmpRotZ = r.rotOffset.z + UnityEngine.Random.Range(r.minRotOffset.z, r.maxRotOffset.z);
        obj.transform.Rotate(tmpRotX, tmpRotY, tmpRotZ);

        // Scale
        var scaleOffset = UnityEngine.Random.Range(r.minScaleOffset, r.maxScaleOffset);
        obj.transform.localScale += new Vector3(scaleOffset, scaleOffset, scaleOffset);
    }
}
