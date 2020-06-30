 using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class HitObjectInfo
{
    public Transform transform;
    public Vector3 hitPoint;
    public GrassPlane plane = null;

    public HitObjectInfo()
    {
        transform = null;
    }
}


[CustomEditor(typeof(GrassPatch))]
public class GrassPatchEditior : Editor
{
    Vector3 currentSelectedVertex = new Vector3();
    int currentlySelectedVertexIndex = -1;
    GroundVert currentSelectedGroundVert = new GroundVert(Vector3.zero);
    Vector3 currentHitNormal = new Vector3();
    List<Vector3> effectedVerts = new List<Vector3>();
    HitObjectInfo currentSelectedObject = new HitObjectInfo();
    Mesh mesh;
    GrassPatch grassPatch;


    void OnEnable()
    {
        grassPatch = (GrassPatch)target;
        currentSelectedObject = new HitObjectInfo();
        currentSelectedGroundVert = new GroundVert(Vector3.zero);
        Refresh();
    }

    void OnSceneGUI()
    {
        Event current = Event.current;
        HandleInput(current);
        DrawHandles();
        // AdjustToGround();
    }

    public override void OnInspectorGUI()
    {
        DrawUI();
        DrawInspectorUI();
        DrawUtilsUI();
    }


    void HandleInput(Event e)
    {
        if (e.type == EventType.MouseDown && e.button == 0 && e.modifiers == EventModifiers.Control)
        {
            currentSelectedObject = ObjectUnderMouse();
        }

        if (e.type == EventType.MouseDown && e.button == 0 && e.modifiers == EventModifiers.Shift)
        {
            if (currentSelectedObject.transform != null) SelectNearestVertex();
        }

        // to prevent from deselection //
        if (grassPatch == null) { return; }
        if (Selection.activeGameObject != grassPatch.transform.gameObject)
            Selection.activeGameObject = grassPatch.transform.gameObject;
    }

    void DrawInspectorUI()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorUtils.HorizontalLine(Color.blue);
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        if (GUILayout.Button("Add GrassMesh")) { grassPatch.AddGrassPlane(); }
        if (GUILayout.Button("Auto Detect Verts")){ AutoDetectVerts(); SceneView.RepaintAll(); }
        if (GUILayout.Button("Test Adjustment")) { AdjustToGround(); }
        if (GUILayout.Button("Combine Patches")) { grassPatch.CombinePatches(); }
        if (GUILayout.Button("Save Original")) { SaveOriginal(); }
        if (GUILayout.Button("Undo Changes")) { UndoChanges(); }

        EditorGUILayout.EndVertical();
    }

    void DrawUI()
    {
        for (int i = 0; i < grassPatch.grassPlanes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            grassPatch.grassPlanes[i].transform = (Transform)EditorGUILayout.ObjectField(grassPatch.grassPlanes[i].transform, typeof(Transform), true);

            if (GUILayout.Button("Select"))
            {
                currentSelectedObject.transform = grassPatch.grassPlanes[i].transform;
                currentSelectedObject.plane = grassPatch.grassPlanes[i];
                currentSelectedVertex = Vector3.zero;
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Remove"))
            {
                grassPatch.RemoveGrassPlane(i);
                currentSelectedObject = new HitObjectInfo();
            }

            EditorGUILayout.EndHorizontal();

        }

        if (currentSelectedObject == null || currentSelectedObject.transform == null) return;
        GrassPlane p = currentSelectedObject.plane;


        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorUtils.HorizontalLine(Color.blue);
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        //if (GUILayout.Button("Add Ground Vert"))
        //{
        //    currentSelectedGroundVert = p.AddGroundVert(currentSelectedVertex);
        //}

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        foreach (var groundVert in p.groundVerts)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(groundVert.vertex.ToString());

            if (GUILayout.Button("Select"))
            {
                currentSelectedVertex = groundVert.vertex;
                currentSelectedGroundVert = groundVert;
                SceneView.RepaintAll();
            };

            //if (GUILayout.Button("Add")) { groundVert.influencedVerts.Add(currentSelectedVertex); SceneView.RepaintAll(); }

            //if (GUILayout.Button("Clear")) { groundVert.ClearInfluenced(); SceneView.RepaintAll(); }

            if (GUILayout.Button("Remove")) { p.RemoveGroundVert(groundVert); SceneView.RepaintAll(); break; }

            EditorGUILayout.EndHorizontal();
        }
    }

    bool utilsFoldout = false;
    void DrawUtilsUI()
    {
        GUILayout.Space(10f);
        utilsFoldout = EditorGUILayout.Foldout(utilsFoldout, "Utilities");
        if (utilsFoldout)
        {
            List<GameObject> items = grassPatch.utils.objects;

            for (int i = 0; i < items.Count; i++)
            {
                GUILayout.BeginHorizontal();
                items[i] = (GameObject)EditorGUILayout.ObjectField(items[i], typeof(GameObject), true);
                if (GUILayout.Button(" Remove ")) { items.RemoveAt(i); break; }
                GUILayout.EndHorizontal();
            }

            // auto add a new entry
            if (items.Count > 0 && items[items.Count - 1] != null)
            {
                items.Add(null);
            }
            else if(items.Count == 0) { items.Add(null); }

            if (GUILayout.Button("Combine")) { grassPatch.utils.Combine(); }

            grassPatch.utils.objects = items;
        }
    }

    HitObjectInfo ObjectUnderMouse()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit = new RaycastHit();
        HitObjectInfo objectInfo = new HitObjectInfo();

        if (Physics.Raycast(ray, out hit))
        {
            objectInfo.transform = hit.transform;
            objectInfo.hitPoint = hit.point;
        }

        return objectInfo;
    }

    void SelectNearestVertex()
    {
        float minDistance = float.MaxValue;
        if (currentSelectedObject.transform != null)
        {
            Mesh mesh = currentSelectedObject.transform.GetComponent<MeshFilter>().sharedMesh;
            if (!mesh) { return; }

            Vector3 hitPos = Vector3.zero;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                hitPos = hit.point;
                currentHitNormal = hit.normal;
            }

            int i = 0;
            foreach (var vert in mesh.vertices)
            {
                float distance = Vector3.Distance(currentSelectedObject.transform.InverseTransformPoint(hitPos), vert);
                if(distance < minDistance)
                {
                    currentSelectedVertex = vert;
                    currentlySelectedVertexIndex = i;
                    minDistance = distance;
                    i++;
                }
            }
        }
    }

    // vertex editor for currentSelectedObject
    Vector3 handleSize = new Vector3(0.05f, 0.05f, 0.05f);
    Vector3 vert = new Vector3();
    void DrawHandles()
    {
        if (currentSelectedObject == null || currentSelectedObject.transform == null) return;

        Transform t = currentSelectedObject.transform;
        mesh = t.GetComponent<MeshFilter>().sharedMesh;
        if (!mesh) { return; }

        Handles.color = Color.yellow;
        Handles.DrawWireCube(t.TransformPoint(currentSelectedVertex), handleSize);

        Handles.color = Color.red;
        if (currentSelectedGroundVert.vertex != currentSelectedVertex) return;
        foreach (var item in currentSelectedGroundVert.influencedVerts)
        {
            Handles.color = Color.cyan;
            Handles.DrawWireCube(t.TransformPoint(item), handleSize);
            SceneView.RepaintAll();
        }

        // test
        // calculate and draw face normal
        List<Triangle> triangles = Geometry.UnityMeshToTriangles(mesh, t);
        Triangle tri = triangles[0];

        var sideA = tri.b - tri.a;
        var sideB = tri.c - tri.a;

        var perp = Vector3.Cross(sideA, sideB);
        var perpLength = perp.magnitude;
        perp /= perpLength;
        var parallel = Vector3.Cross(perp, Vector3.right);

        Debug.DrawRay(tri.GetCentre(), perp * 10f);

        Vector3 pos = t.TransformPoint(grassPatch.grassPlanes[0].groundVerts[0].vertex);
        Debug.DrawRay(pos, (parallel) * 10f);
    }

    void AutoDetectVerts()
    {
        effectedVerts.Clear();
        grassPatch.AutoDetectVerts();
    }

    Vector3 GetRaycastDirection()
    {
        Vector3 dir = Vector3.Cross(Vector3.right, currentHitNormal);
        dir.Normalize();
        return dir;
    }

    void AdjustToGround()
    {
        foreach (var item in grassPatch.grassPlanes)
        {
            item.AdjustToGround();
        }
    }

    void SaveOriginal()
    {
        foreach (var item in grassPatch.grassPlanes)
        {
            item.SaveOriginal();
        }
    }

    void UndoChanges()
    {
        foreach (var item in grassPatch.grassPlanes)
        {
            item.UndoChanges();
        }
    }

    void Refresh()
    {
    }
}
