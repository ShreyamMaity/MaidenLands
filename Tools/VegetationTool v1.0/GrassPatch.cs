using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// these vertices in a grassplane can be modified to better
/// fit the grassplane to terrain
/// </summary>
[System.Serializable]
public class GroundVert
{
    public Vector3 vertex = Vector3.zero;
    public List<Vector3> influencedVerts = new List<Vector3>();

    public bool showInfluencedVerts = true;
    public string showInfluencedButtonText = "Show";

    public GroundVert(Vector3 vert)
    {
        showInfluencedButtonText = "Show";
        vertex = vert;
    }

    public void ClearInfluenced()
    {
        influencedVerts.Clear();
    }
}


/// <summary>
/// an individual plane of grass
/// </summary>
[System.Serializable]
public class GrassPlane
{
    public Transform transform;
    public Vector3[] originalVerts = new Vector3[0];
    public List<GroundVert> groundVerts = new List<GroundVert>();

    public bool isSelected = false;
    bool isSaved = false;

    public GroundVert AddGroundVert(Vector3 vert)
    {
        GroundVert groundVert = new GroundVert(vert);
        groundVerts.Add(groundVert);
        return groundVert;
    }

    public void RemoveGroundVert(GroundVert groundVert)
    {
        if (groundVerts.Contains(groundVert))
            groundVerts.Remove(groundVert);
    }

    public void AdjustToGround()
    {
        LayerMask mask = LayerMask.NameToLayer("Ground");
        if (!isSaved) { SaveOriginal(); isSaved = true; }

        Vector3[] vertices = transform.GetComponent<MeshFilter>().sharedMesh.vertices;
        float distance = float.MinValue;

        RaycastHit hitInfo = new RaycastHit();
        foreach (var item in groundVerts)
        {
            Vector3 groundVert = item.vertex;

            Vector3 from = transform.TransformPoint(item.influencedVerts[0]);
            Vector3 to = (from - transform.TransformPoint(item.vertex)).normalized * 10f;

            // if vertex is above ground
            if (Physics.Raycast(from, -to, out hitInfo, mask))
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (vertices[i] == groundVert)
                    {
                        // first note the vector difference between current position of this
                        // vertex and hitPoint
                        if (hitInfo.transform.name != "Terrain") { Debug.Log("not terrain"); return; }
                        distance = Vector3.Distance(transform.TransformPoint(vertices[i]), hitInfo.point);
                        vertices[i] = transform.InverseTransformPoint(hitInfo.point);
                    }
                }

                //apply this difference to the verts that will be
                //effected by this groundVert
                foreach (var effectedVert in item.influencedVerts)
                {
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        if(vertices[i] == effectedVert)
                        {
                            Vector3 p2 = transform.TransformPoint(vertices[i]) + (-((to.normalized) * distance));
                            vertices[i] = transform.InverseTransformPoint(p2);
                        }
                    }
                }
            }
        }

        Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;
        Mesh newmesh = new Mesh();
        newmesh.vertices = mesh.vertices;
        newmesh.triangles = mesh.triangles;
        newmesh.uv = mesh.uv;
        newmesh.normals = mesh.normals;
        newmesh.tangents = mesh.tangents;

        Color[] colors = mesh.colors;
        for (int i = 0; i < mesh.colors.Length; ++i)
            colors[i].a = colors[i].grayscale;
        newmesh.colors = mesh.colors;

        transform.GetComponent<MeshFilter>().sharedMesh = newmesh;
        transform.GetComponent<MeshFilter>().sharedMesh.vertices = vertices;
        isSaved = false;
    }

    public void SaveOriginal()
    {
        originalVerts = transform.GetComponent<MeshFilter>().sharedMesh.vertices;
    }

    public void UndoChanges()
    {
        transform.GetComponent<MeshFilter>().sharedMesh.vertices = originalVerts;
    }

    /// <summary>
    /// returns a vector perpendicular to plane
    /// </summary>
    public Vector3 GetParallel()
    {
        List<Triangle> triangles = Geometry.UnityMeshToTriangles(transform.GetComponent<Mesh>(), transform);
        Triangle tri = triangles[0];

        var sideA = tri.b - tri.a;
        var sideB = tri.c - tri.a;

        var perp = Vector3.Cross(sideA, sideB);
        var perpLength = perp.magnitude;
        perp /= perpLength;
        var parallel = Vector3.Cross(perp, Vector3.right);

        return parallel;
    }

    public void AutoDetectVerts()
    {
        groundVerts.Clear();

        var originalEuler = transform.eulerAngles;
        transform.eulerAngles = new Vector3(originalEuler.x, 0, 0);

        Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;

        // determine ground verts
        foreach (var item in mesh.vertices)
        {
            Vector3 vertex = transform.TransformPoint(item);
            if (vertex.y < 0.15f)
                AddGroundVert(item);
        }

        // determine effected vertices of each ground vert
        foreach (var item in groundVerts)
        {
            foreach (var vert in mesh.vertices)
            {
                // meaning its a ground vert
                if (transform.TransformPoint(vert).y < 0.15f) continue;

                // create a line segment
                List<Triangle> triangles = Geometry.UnityMeshToTriangles(mesh, transform);
                Triangle tri = triangles[0];

                var sideA = tri.b - tri.a;
                var sideB = tri.c - tri.a;

                var perp = Vector3.Cross(sideA, sideB);
                var perpLength = perp.magnitude;
                perp /= perpLength;
                var parallel = Vector3.Cross(perp, Vector3.right);

                Vector3 p1 = transform.TransformPoint(item.vertex);
                Vector3 p2 = p1 + (parallel * 5f);

                if (Utils.Utils.IsColinear(p1, p2, transform.TransformPoint(vert)))
                {
                    item.influencedVerts.Add(vert);
                }
            }
        }

        transform.eulerAngles = originalEuler;

    }
}


public class GrassPatch : MonoBehaviour
{
    public GameObject parent = null;
    public List<GrassPlane> grassPlanes = new List<GrassPlane>();

    public GrassPatchUtils utils = new GrassPatchUtils();

    public void AddGrassPlane()
    {
        GrassPlane newPlane = new GrassPlane();
        grassPlanes.Add(newPlane);
    }

    public void RemoveGrassPlane(int index)
    {
        grassPlanes.RemoveAt(index);
    }


    public void AutoDetectVerts()
    {
        foreach(var item in grassPlanes)
        {
            Debug.Log(item);
            item.AutoDetectVerts();
        }
    }

    //combine the grass patches together
    //into one mesh
    public void CombinePatches()
    {
        MeshFilter[] meshFilters = new MeshFilter[grassPlanes.Count];//  GetComponentsInChildren<MeshFilter>();

        int i = 0;
        foreach (var item in grassPlanes)
        {
            meshFilters[i] = item.transform.GetComponent<MeshFilter>();
            i++;
        }

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }

        if (!transform.GetComponent<MeshFilter>()) transform.gameObject.AddComponent<MeshFilter>();
        if (!transform.GetComponent<MeshRenderer>()) transform.gameObject.AddComponent<MeshRenderer>();
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
        transform.gameObject.SetActive(true);
    }

    public void Place()
    {
        foreach (var item in grassPlanes)
        {
            item.AdjustToGround();
        }
    }
}


[System.Serializable]
public class GrassPatchUtils
{
    public List<GameObject> objects = new List<GameObject>();

    public GrassPatchUtils()
    {
    }

    public void Combine() { }
}