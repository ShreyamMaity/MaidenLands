using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FoilageSystem;


[System.Serializable]
class GeoPaint
{
    GeoPainter painter;

    string appTitle = "Geo Painter Tools";

    bool isPainting = false;
    string isPaintingButtonText = "";

    Event currentEvent = null;

    List<GameObject> myObjToInstArray = new List<GameObject>();
    List<GameObject> objectsToScatter = new List<GameObject>();

    public void OnEnable()
    {
        painter = GameObject.FindGameObjectWithTag("SystemObject").GetComponent<GeoPainter>();

        if (!isPainting) isPaintingButtonText = "Start Painting";
        else isPaintingButtonText = "Stop Painting";
    }

    public void OnSceneGUI(SceneView sv)
    {
        currentEvent = Event.current;
        DrawHandles(sv);
        HandleInput(sv);
    }

    void HandleInput(SceneView sv)
    {
        var ctrlID = GUIUtility.GetControlID(appTitle.GetHashCode(), FocusType.Passive);
        switch (currentEvent.type)
        {
            case EventType.MouseDrag:
                if (currentEvent.control)
                {
                    Paint(sv);
                    currentEvent.Use();
                }
                else if (currentEvent.shift)
                {
                    RemovePaint(sv);
                    currentEvent.Use();
                }
                break;

            case EventType.MouseUp:
                if (currentEvent.control)
                {
                    Paint(sv);
                    myObjToInstArray = new List<GameObject>();
                    currentEvent.Use();
                }
                else if (currentEvent.shift)
                {
                    RemovePaint(sv);
                    currentEvent.Use();
                }
                break;

            case EventType.Layout:
                HandleUtility.AddDefaultControl(ctrlID);
                break;

            case EventType.MouseMove:
                HandleUtility.Repaint();
                break;
        }
    }


    bool geoPaintUIFoldPanel = false;
    bool scatterObjectsPanel = false;

    public void UpdateUI(EditorWindow window)
    {
        if (painter == null) OnEnable();

        GUILayout.BeginVertical();

        geoPaintUIFoldPanel = EditorUtils.Foldout("Geo & Foilge Painter", geoPaintUIFoldPanel, 5, 0);

        if (geoPaintUIFoldPanel)
        {
            // if painter group is deleted from scene, then set 
            // to true
            for (int i = 0; i < painter.painterGroups.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15);

                var item = painter.painterGroups[i];

                // if the user deletes this geo group from scene,
                if(item == null) {
                    painter.painterGroups.RemoveAt(i);
                    window.Repaint();
                    return;
                }

                item.groupParentObj.name = GUILayout.TextField(item.groupParentObj.name);

                if (GUILayout.Button("Select")) { painter.currentGroup = item; }
                if (GUILayout.Button("Remove")) { painter.RemoveGeoPaintGroup(item); break; }

                GUILayout.Space(15);
                GUILayout.EndHorizontal();
            }

            scatterObjectsPanel = EditorUtils.Foldout("Scatter Objects", scatterObjectsPanel, 15, 15) && !isPainting;

            if (scatterObjectsPanel)
            {
                if (painter.currentGroup & painter.currentGroup != null)
                {
                    for (int i = 0; i < painter.currentGroup.ScatterObjects.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);

                        painter.currentGroup.ScatterObjects[i] = (GameObject)EditorGUILayout.ObjectField(painter.currentGroup.ScatterObjects[i], typeof(GameObject), false);
                        if (GUILayout.Button("Remove")) { painter.currentGroup.ScatterObjects.RemoveAt(i); break; }

                        GUILayout.Space(15);
                        GUILayout.EndHorizontal();
                    }
                }
            }

            OptionsPanel();
            SettingsPanel();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);

            GUILayout.BeginVertical();

            if (!isPainting)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Group")) { AddNewGroup(); }
                if (GUILayout.Button("Add Scatter Mesh")) { AddNewScatterObject(); }
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button(isPaintingButtonText))
            {
                myObjToInstArray.Clear();
                // clear out the objects to scatter
                // objectsToScatter.Clear();

                if(painter.currentGroup == null)
                {
                    Debug.Log("no foilage group selected");
                    return;
                }

                if (painter.currentGroup.useFoilageGroups)
                {
                    objectsToScatter = FoilageEditor.Instance.foilageGroupsEditor.groups.GetFoilageGroups();
                }
                else
                {
                    objectsToScatter = painter.currentGroup.ScatterObjects;
                }

                isPainting = !isPainting;
                if (isPainting) { isPaintingButtonText = "Stop Painting"; }
                else { isPaintingButtonText = "Start Painting"; scatterObjectsPanel = true; }
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();

            GUILayout.Space(15);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    bool optionsFoldPanel = false;
    void OptionsPanel()
    {
        optionsFoldPanel = EditorUtils.Foldout("Paint Options", optionsFoldPanel, 15, 0) && !isPainting;

        if (optionsFoldPanel)
        {
            if (painter.currentGroup != null)
            {
                GUILayout.Space(10);

                // position offsets
                EditorUtils.BoldLabel("Position Offset", 30, 15);
                painter.currentGroup.posOffset = EditorUtils.VectorField(painter.currentGroup.posOffset, "Offset Pos    ", 30, 15);
                painter.currentGroup.minPosOffset = EditorUtils.VectorField(painter.currentGroup.minPosOffset, "Random Min", 30, 15);
                painter.currentGroup.maxPosOffset = EditorUtils.VectorField(painter.currentGroup.maxPosOffset, "Random Max", 30, 15);

                GUILayout.Space(10);

                // rotation offsets
                EditorUtils.BoldLabel("Rotation Offset", 30, 15);
                painter.currentGroup.rotOffset = EditorUtils.VectorField(painter.currentGroup.rotOffset, "Offset Angle", 30, 15);
                painter.currentGroup.minRotOffset = EditorUtils.VectorField(painter.currentGroup.minRotOffset, "Random Min", 30, 15);
                painter.currentGroup.maxRotOffset = EditorUtils.VectorField(painter.currentGroup.maxRotOffset, "Random Max", 30, 15);

                GUILayout.Space(10);

                // scale offsets
                EditorUtils.BoldLabel("Scale Offset", 30, 15);
                painter.currentGroup.scaleOffset = EditorUtils.VectorField(painter.currentGroup.scaleOffset, "Offset Scale", 30, 15);
                painter.currentGroup.minScaleOffset = EditorUtils.VectorField(painter.currentGroup.minScaleOffset, "Random Min", 30, 15);
                painter.currentGroup.maxScaleOffset = EditorUtils.VectorField(painter.currentGroup.maxScaleOffset, "Random Max", 30, 15);

                GUILayout.Space(10);
            }
        }
    }

    bool settingsFoldPanel = false;
    void SettingsPanel()
    {
        settingsFoldPanel = EditorUtils.Foldout("Paint Settings", settingsFoldPanel, 15, 0);

        if (settingsFoldPanel && painter.currentGroup != null)
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Min Slope", GUILayout.MinWidth(100));
            painter.currentGroup.minSlopeVal = EditorGUILayout.FloatField(painter.currentGroup.minSlopeVal, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Max Slope", GUILayout.MinWidth(100));
            painter.currentGroup.maxSlopeVal = EditorGUILayout.FloatField(painter.currentGroup.maxSlopeVal, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Min Altitude", GUILayout.MinWidth(100));
            painter.currentGroup.minAltitudeVal = EditorGUILayout.FloatField(painter.currentGroup.minAltitudeVal, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal(); 

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Max Altitude", GUILayout.MinWidth(100));
            painter.currentGroup.maxAltitudeVal = EditorGUILayout.FloatField(painter.currentGroup.maxAltitudeVal, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("DistanceRadius", GUILayout.MinWidth(100));
            painter.currentGroup.distanceRadius = EditorGUILayout.FloatField(painter.currentGroup.distanceRadius, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Spray Radius", GUILayout.MinWidth(100));
            painter.currentGroup.sprayRadius = EditorGUILayout.FloatField(painter.currentGroup.sprayRadius, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Delete Radius", GUILayout.MinWidth(100));
            painter.currentGroup.deleteRadius = EditorGUILayout.FloatField(painter.currentGroup.deleteRadius, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Seed", GUILayout.MinWidth(100));
            painter.currentGroup.rndSeed = EditorGUILayout.IntField(painter.currentGroup.rndSeed, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Paint Layer", GUILayout.MinWidth(100));
            painter.paintLayer = EditorGUILayout.LayerField(painter.paintLayer, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Use Normal", GUILayout.MinWidth(150));
            painter.currentGroup.useNormal = EditorGUILayout.Toggle(painter.currentGroup.useNormal, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Uniform Scale", GUILayout.MinWidth(150));
            painter.currentGroup.scaleUniform = EditorGUILayout.Toggle(painter.currentGroup.scaleUniform, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Use Foilage Groups", GUILayout.MinWidth(150));
            painter.currentGroup.useFoilageGroups = EditorGUILayout.Toggle(painter.currentGroup.useFoilageGroups, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }
    }

    void DrawHandles(SceneView sv)
    {
        if (painter.currentGroup == null) return;
        if (!isPainting) return;

        RaycastHit hit;
        Ray ray = GetRay(sv);

        var layerMask = 1 << painter.paintLayer;

        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
        {

            if (painter.currentGroup.sprayRadius != 0)
            {
                Handles.color = Color.green;
                Handles.CircleHandleCap(1, hit.point, Quaternion.LookRotation(hit.normal), painter.currentGroup.sprayRadius, EventType.Repaint);
            }
            if (painter.currentGroup.distanceRadius != 0)
            {
                Handles.color = Color.blue;
                Handles.CircleHandleCap(0, hit.point, Quaternion.LookRotation(hit.normal), painter.currentGroup.distanceRadius, EventType.Repaint);
            }
            if (painter.currentGroup.deleteRadius != 0)
            {
                Handles.color = Color.red;
                Handles.CircleHandleCap(0, hit.point, Quaternion.LookRotation(hit.normal), painter.currentGroup.deleteRadius, EventType.Repaint);
            }
        }

        HandleUtility.Repaint();
    }

    void AddNewGroup()
    {
        painter.currentGroup = painter.AddGeoPaintGroup();
    }

    void AddNewScatterObject()
    {
        painter.currentGroup.ScatterObjects.Add(null);
    }

    void RandomizeSolo(GeoPainterPoint element)
    {
        Transform obj = element.go.transform;
        var myRot = Quaternion.identity;
        if (element.useNormal)
        {
            //myRot = Quaternion.LookRotation(hit.normal);
            myRot = Quaternion.FromToRotation(obj.up, element.normal) * obj.rotation;
        }
        obj.position = element.pos;
        obj.rotation = myRot;
        obj.localScale = element.scale;

        // Position
        var tmpPosX = painter.currentGroup.posOffset.x + Random.Range(painter.currentGroup.minPosOffset.x, painter.currentGroup.maxPosOffset.x);
        var tmpPosY = painter.currentGroup.posOffset.y + Random.Range(painter.currentGroup.minPosOffset.y, painter.currentGroup.maxPosOffset.y);
        var tmpPosZ = painter.currentGroup.posOffset.z + Random.Range(painter.currentGroup.minPosOffset.z, painter.currentGroup.maxPosOffset.z);
        obj.Translate(tmpPosX, tmpPosY, tmpPosZ);

        // Rotation
        var tmpRotX = painter.currentGroup.rotOffset.x + Random.Range(painter.currentGroup.minRotOffset.x, painter.currentGroup.maxRotOffset.x);
        var tmpRotY = painter.currentGroup.rotOffset.y + Random.Range(painter.currentGroup.minRotOffset.y, painter.currentGroup.maxRotOffset.y);
        var tmpRotZ = painter.currentGroup.rotOffset.z + Random.Range(painter.currentGroup.minRotOffset.z, painter.currentGroup.maxRotOffset.z);
        obj.Rotate(tmpRotX, tmpRotY, tmpRotZ);

        // Scale
        var tmpSclX = painter.currentGroup.scaleOffset.x + Random.Range(painter.currentGroup.minScaleOffset.x, painter.currentGroup.maxScaleOffset.x);
        var tmpSclY = painter.currentGroup.scaleOffset.y + Random.Range(painter.currentGroup.minScaleOffset.y, painter.currentGroup.maxScaleOffset.y);
        var tmpSclZ = painter.currentGroup.scaleOffset.z + Random.Range(painter.currentGroup.minScaleOffset.z, painter.currentGroup.maxScaleOffset.z);

        if (!painter.currentGroup.scaleUniform)
        {
            obj.localScale += new Vector3(tmpSclX, tmpSclY, tmpSclZ);
        }
        else
        {
            obj.localScale += new Vector3(tmpSclX, tmpSclX, tmpSclX);
        }
    }

    void Paint(SceneView sv)
    {
        if (!isPainting) return;

        if (painter.currentGroup == null) return;

        if (objectsToScatter.Count == 0) return;

        RaycastHit hitInfo = new RaycastHit();
        // var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Ray ray = GetRay(sv);
        var layerMask = 1 << painter.paintLayer;

        //Spray
        if (Physics.Raycast(ray.origin, ray.direction, out hitInfo, Mathf.Infinity, layerMask))
        {
            GameObject newObj = null;
            Quaternion myRot;
            float dist = Mathf.Infinity;
            GameObject objToInst = null;
            //Spray
            if (painter.currentGroup.sprayRadius > 0)
            {
                var randomCircle = Random.insideUnitCircle * painter.currentGroup.sprayRadius;
                var rayDirection = (hitInfo.point + new Vector3(randomCircle.x, 0, randomCircle.y)) - ray.origin;
                RaycastHit newHit;
                if (Physics.Raycast(ray.origin, rayDirection, out newHit, Mathf.Infinity, layerMask))
                {
                    hitInfo = newHit;
                }
            }

            //Check Distance
            if (painter.currentGroup.transform.childCount != 0)
            {
                foreach (var obj in myObjToInstArray)//currentGroup.transform)
                {
                    var tempDist = Vector3.Distance(hitInfo.point, obj.transform.position);
                    if (tempDist < dist)
                        dist = tempDist;
                }
            }

            // See if we are inside of a mask zone.
            // Find all game objects with tag paintMask
            GameObject[] objectsArray;
            var objectMask = false;
            objectsArray = GameObject.FindGameObjectsWithTag("paintMask");
            // Iterate through all objects and check for isInside
            foreach (GameObject go in objectsArray)
            {
                if (go.GetComponent<Collider>())
                {
                    if (CheckIsInside(go.GetComponent<Collider>(), hitInfo.point))
                    {
                        objectMask = true;
                        // EditorUtility.DisplayDialog("Hit a prefab painting mask!", "It looks like we hit an object with the tag paintMask", "Continue", "You have to continue");
                    }
                }
            }

            float angle;
            angle = Vector3.Angle(hitInfo.normal, new Vector3(0, 1, 0));

            float hitHeight;
            hitHeight = hitInfo.point.y;

            // JO Added check for slop angle and altitude
            if (dist >= painter.currentGroup.distanceRadius && angle <= painter.currentGroup.maxSlopeVal && angle >= painter.currentGroup.minSlopeVal && hitHeight >= painter.currentGroup.minAltitudeVal && hitHeight <= painter.currentGroup.maxAltitudeVal && !objectMask)
            {
                //Biblio Method
                if (painter.bibSortIndex == 0)
                {
                    // Random
                    // JO Here is where you would pick an item based on the random probability of each
                    var myRandom = Random.Range(0, objectsToScatter.Count);
                    objToInst = objectsToScatter[myRandom];
                }
                if (painter.bibSortIndex == 1)
                {
                    objToInst = objectsToScatter[painter.soloObjectIndex];
                }

                // Check is we're using normal placement
                myRot = Quaternion.identity;
                if (painter.currentGroup.useNormal)
                {
                    myRot = Quaternion.FromToRotation(objToInst.transform.up, hitInfo.normal) * objToInst.transform.rotation;
                }

                //Create the Object
                // if using foilage groups
                if (painter.currentGroup.useFoilageGroups)
                {
                    newObj = Object.Instantiate(objToInst);
                    newObj.transform.position = hitInfo.point;

                    GrassPatch instPatch = newObj.GetComponent<GrassPatch>();
                    GrassPatch originalPatch = objToInst.GetComponent<GrassPatch>();

                    for (int i = 0; i < originalPatch.grassPlanes.Count; i++)
                    {
                        GrassPlane originalPlane = originalPatch.grassPlanes[i]; // original patch
                        GrassPlane instPlane = instPatch.grassPlanes[i]; // instantiated patch

                        instPlane.transform = Object.Instantiate(originalPlane.transform.gameObject).transform;
                        instPlane.transform.parent = newObj.transform;

                        // make position its relative position is same as original
                        Vector3 worldPos = newObj.transform.TransformPoint(originalPlane.transform.localPosition);
                        Vector3 localPos = newObj.transform.InverseTransformPoint(worldPos);
                        instPlane.transform.localPosition = localPos;

                        // make it rotate according to ground normal
                        // instPlane.transform.rotation = myRot;
                        RaycastHit hit = new RaycastHit();
                        if (Physics.Raycast(instPlane.transform.position, Vector3.down, out hit, LayerMask.NameToLayer("Ground")))
                        {
                            //Quaternion rot = Quaternion.FromToRotation(instPlane.transform.up, hit.normal) * instPlane.transform.rotation;
                            //instPlane.transform.rotation = rot;
                            // instPlane.transform.eulerAngles = hit.normal;
                            //instPlane.transform.position = hit.point;
                        }

                        instPlane.groundVerts = originalPlane.groundVerts;
                    }

                    myObjToInstArray.Add(newObj);
                }
                else
                {
                    newObj = Object.Instantiate(objToInst); // PrefabUtility.InstantiatePrefab(objToInst);
                    newObj.transform.position = hitInfo.point;
                    newObj.transform.rotation = myRot;
                    myObjToInstArray.Add(newObj);
                }

                // Update Points Array
                painter.currentGroup.AddScatterObject(newObj, hitInfo.point, newObj.transform.localScale, hitInfo.normal, painter.currentGroup.useNormal);

                // Update Position Pivot
                if (painter.currentGroup.transform.childCount == 0)
                {
                    painter.currentGroup.transform.position = newObj.transform.position;
                    newObj.transform.parent = painter.currentGroup.transform;
                }
                else
                {
                    newObj.transform.parent = painter.currentGroup.transform;
                }
                RandomizeSolo(painter.currentGroup.myPointsList[painter.currentGroup.myPointsList.Count - 1]);

                // if using foilage groups
                if (painter.currentGroup.useFoilageGroups)
                {
                    GrassPatch patch = newObj.GetComponent<GrassPatch>();
                    patch.Place();
                }
            }
        }
    }

    void RemovePaint(SceneView sv)
    {
        if (painter.currentGroup == null) return;

        RaycastHit hit;
        var ray = GetRay(sv);

        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
        {
            /*
			//currentGroup
			if(hit.transform.IsChildOf(currentGroup.transform))
			{
				DestroyImmediate(hit.collider.gameObject);
			}
			*/
            for (int i = 0; i < painter.currentGroup.myPointsList.Count; i++)
            {
                var element = painter.currentGroup.myPointsList[i];
                if (Vector3.Distance(hit.point, element.go.transform.position) <= painter.currentGroup.deleteRadius)
                {
                    Object.DestroyImmediate(element.go);
                    painter.currentGroup.myPointsList.RemoveAt(i);
                }
            }
        }
    }

    bool CheckIsInside(Collider maskingObject, Vector3 point)
    {
        Vector3 center;
        Vector3 direction;

        RaycastHit hitInfo = new RaycastHit();
        Ray ray;

        bool hit;

        // Use collider bounds to get the center of the collider. May be inaccurate
        // for some colliders (i.e. MeshCollider with a 'plane' mesh)
        center = maskingObject.bounds.center;

        // Cast a ray from point to center
        direction = center - point;
        ray = new Ray(point, direction);
        hit = maskingObject.Raycast(ray, out hitInfo, direction.magnitude);
        //hit = Physics.Raycast(point, direction, Mathf.Infinity);

        // If we hit the collider, point is outside. So we return !hit
        //Debug.Log("Collision is: " + hit);
        return !hit;
    }

    Ray GetRay(SceneView sv)
    {
        Vector3 mousePos = Event.current.mousePosition;
        mousePos.y = sv.camera.pixelHeight - mousePos.y;
        Ray ray = sv.camera.ScreenPointToRay(mousePos);
        return ray;
    }
}


[System.Serializable]
public class FoilageGroupsEditor
{
    public FoilageGroup groups;
    bool mainfoldOut = false;

    public void OnEnable()
    {
        groups = GameObject.FindGameObjectWithTag("SystemObject").GetComponent<FoilageGroup>();
    }

    public void UpdateUI()
    {
        mainfoldOut = EditorUtils.Foldout("Foilage Groups", mainfoldOut, 5, 15);

        if (mainfoldOut)
        {
            for (int j = 0; j < groups.groupObjects.Count; j++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(18);

                EditorUtils.Label(j.ToString(), 0, 0);

                groups.groupObjects[j] = (GameObject)EditorGUILayout.ObjectField(groups.groupObjects[j], typeof(GameObject), true);

                // if the user drags an object from the scene and if it;s not a GrassPatch
                if(groups.groupObjects[j] != null && !groups.groupObjects[j].GetComponent<GrassPatch>())
                {
                    groups.groupObjects[j] = null;
                }
                else
                {
                    groups.AddEntry(j);
                }

                if (EditorUtils.Button("Remove", 0, 0)) { groups.Remove(j); groups.selectedIndex = -1; return; }
                if (EditorUtils.Button("Select", 0, 0)) { groups.Select(j); }

                GUILayout.Space(15);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5f);
            FoilageRules r = null;

            if(groups.selectedIndex != -1 && groups.foilageRules.ContainsKey(groups.selectedIndex))
            {
                r = groups.foilageRules[groups.selectedIndex];
            }

            if (r != null)
            {
                DrawScatterSettingsUI(r);
                DrawRulesUI(r);
            }

            GUILayout.Space(5f);
            GUILayout.BeginHorizontal();

            if (EditorUtils.Button("Add Foilage group", 20, 0))
            {
                groups.Add();
            }

            if (EditorUtils.Button("Clear", 0, 15))
            {
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5f);
        }
    }


    bool scatterUIFoldout = false;
    void DrawScatterSettingsUI(FoilageRules r)
    {
        scatterUIFoldout = EditorUtils.Foldout("Scatter Settings", scatterUIFoldout, 15, 15);
        if (scatterUIFoldout)
        {
            GUILayout.Space(10);

            // position offsets
            EditorUtils.BoldLabel("Position Offset", 30, 15);
            r.posOffset = EditorUtils.VectorField(r.posOffset, "Offset Pos    ", 30, 15);
            r.minPosOffset = EditorUtils.VectorField(r.minPosOffset, "Random Min", 30, 15);
            r.maxPosOffset = EditorUtils.VectorField(r.maxPosOffset, "Random Max", 30, 15);

            GUILayout.Space(10);

            // rotation offsets
            EditorUtils.BoldLabel("Rotation Offset", 30, 15);
            r.rotOffset = EditorUtils.VectorField(r.rotOffset, "Offset Angle", 30, 15);
            r.minRotOffset = EditorUtils.VectorField(r.minRotOffset, "Random Min", 30, 15);
            r.maxRotOffset = EditorUtils.VectorField(r.maxRotOffset, "Random Max", 30, 15);

            GUILayout.Space(10);

            // scale offsets
            EditorUtils.BoldLabel("Scale Offset", 30, 15);

            GUILayout.BeginHorizontal();
            EditorUtils.Label("Random Min ", 30, 0);
            r.minScaleOffset = EditorUtils.Float(ref r.minScaleOffset, "", 30, 15, minWidth: 50);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorUtils.Label("Random Max", 30, 0);
            r.maxScaleOffset = EditorUtils.Float(ref r.maxScaleOffset, "", 30, 15, minWidth: 50);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }
    }

    bool rulesUIFoldout = false;
    void DrawRulesUI(FoilageRules r)
    {
        rulesUIFoldout = EditorUtils.Foldout("Properties", rulesUIFoldout, 15, 15);
        if (rulesUIFoldout)
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Min Slope", GUILayout.MinWidth(100));
            r.minSlopeVal = EditorGUILayout.FloatField(r.minSlopeVal, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Max Slope", GUILayout.MinWidth(100));
            r.maxSlopeVal = EditorGUILayout.FloatField(r.maxSlopeVal, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Min Altitude", GUILayout.MinWidth(100));
            r.minAltitudeVal = EditorGUILayout.FloatField(r.minAltitudeVal, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Max Altitude", GUILayout.MinWidth(100));
            r.maxAltitudeVal = EditorGUILayout.FloatField(r.maxAltitudeVal, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("Use Normal", GUILayout.MinWidth(150));
            r.useNormal = EditorGUILayout.Toggle(r.useNormal, GUILayout.MinWidth(50));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }
    }
}


public class Texturing
{
    // timer for timing method durations
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    Terrain myTerrain;

    public Texture2D splatA;
    public Texture2D splatB;

    public void OnEnable()
    {
    }

    bool mainfoldOut = false;
    public void UpdateUI()
    {
        mainfoldOut = EditorUtils.Foldout("Texturing", mainfoldOut, 5, 15);
        if (mainfoldOut)
        {
            MainPanelUI();
        }
    }

    void MainPanelUI()
    {
        GUILayout.Space(5);

        splatA = EditorUtils.Texture2dField("First Splatmap", ref splatA, 20f, 15f);
        EditorGUILayout.Separator();
        splatB = EditorUtils.Texture2dField("Second Splatmap", ref splatB, 20f, 15f);
        EditorGUILayout.Separator();

        if (EditorUtils.Button("Applay Splatmap", 20, 15))
        {
            Debug.Log("Applay splatmap");
            ApplySplatMap();
        }
    }

    public void OnSceneGUI(SceneView sv)
    {
    }

    void ApplySplatMap()
    {
        if (!myTerrain) myTerrain = Terrain.activeTerrain;
        if (myTerrain == null) { Debug.LogError("No terrain selected"); return; }

        TerrainData terrainData = myTerrain.terrainData;
        if (terrainData == null) { Debug.LogError("Failed getting terrain data"); return; }

        if (terrainData.alphamapLayers < 4)
        {
            EditorUtility.DisplayDialog("Missing Terrain Textures", "Please set up at least 4 textures in the terrain painter dialog", "OK");
            return;
        }

        if (splatB != null && terrainData.alphamapLayers < 8)
        {
            EditorUtility.DisplayDialog("Missing Terrain Textures", "Please set up at least 8 textures in the terrain painter dialog", "OK");
            return;
        }

        int width = splatA.width;
        bool usingTwoMaps = false;

        if(splatB != null)
        {
            if (splatA.width != splatB.width && splatA.height != splatB.height)
            {
                Debug.LogError("Both splatmaps must have same resolution (" + splatA.width + " != " + splatB.width + ")");
                return;
            }
            usingTwoMaps = true;
        }

        terrainData.alphamapResolution = width;

        float[,,] splatmapData = terrainData.GetAlphamaps(0, 0, width, width);
        Color[] splatmapColors = splatA.GetPixels();
        Color[] splatmapColors_b = null;

        if (usingTwoMaps) splatmapColors_b = splatB.GetPixels();

        Color col_a = Color.clear;
        Color col_b = Color.clear;

        for (int y = 0; y < width; y++)
        {
            // update progress bar every now and then
            if (y % 10 == 0)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Applying splatmap", "calculating...", Mathf.InverseLerp(0.0f, width, y)))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
            }
            //

            for (int x = 0; x < width; x++)
            {
                float sum;
                col_a = splatmapColors[x * width + y];

                if (usingTwoMaps)
                {
                    col_b = splatmapColors_b[y * width + x];
                    sum = col_a.r + col_a.g + col_a.b + col_b.r + col_b.g + col_b.b;
                }
                else
                {
                    sum = col_a.r + col_a.g + col_a.b;
                }

                if (sum > 1.0f)
                {
                    // no final channel, and scale down
                    splatmapData[x, y, 0] = col_a.r / sum;
                    splatmapData[x, y, 1] = col_a.g / sum;
                    splatmapData[x, y, 2] = col_a.b / sum;
                    splatmapData[x, y, 3] = 1f - sum;

                    if (usingTwoMaps)
                    {
                        splatmapData[x, y, 4] = col_b.r / sum;
                        splatmapData[x, y, 5] = col_b.g / sum;
                        splatmapData[x, y, 6] = col_b.b / sum;
                        splatmapData[x, y, 7] = 1f - sum;
                    }
                }
                else
                {
                    // derive final channel from white
                    splatmapData[x, y, 0] = col_a.r;
                    splatmapData[x, y, 1] = col_a.g;
                    splatmapData[x, y, 2] = col_a.b;
                    splatmapData[x, y, 3] = 1f - sum;

                    if (usingTwoMaps)
                    {
                        splatmapData[x, y, 4] = col_b.r;
                        splatmapData[x, y, 5] = col_b.g;
                        splatmapData[x, y, 6] = col_b.b;
                        splatmapData[x, y, 7] = 1f - sum;
                    }
                }
            }
        }

        EditorUtility.ClearProgressBar();
        terrainData.SetAlphamaps(0, 0, splatmapData);
        Debug.Log("Splatmaps applied " + GetTimerTime());
    }

    void StartTimer()
    {
        stopwatch.Reset();
        stopwatch.Start();
    }

    string GetTimerTime()
    {
        stopwatch.Stop();
        return " (" + stopwatch.Elapsed.Milliseconds + "ms)";
    }
}


[System.Serializable]
public class FoilageArea
{
    PointsGenerator generator = null;

    public void OnEnable()
    {
        generator = GameObject.FindGameObjectWithTag("SystemObject").GetComponent<PointsGenerator>();
        if (generator == null) Debug.Log("Generator not found in scene");
    }

    public void UpdateUI()
    {
        MainPanel_UI();
    }

    public void OnSceneGUI(SceneView sv)
    {
        OnDrawGizmos();
    }

    bool mainfoldOut = false;
    void MainPanel_UI()
    {
        if( generator == null) { return; }

        mainfoldOut = EditorUtils.Foldout("Foilage Area", mainfoldOut, 5, 15);
        if (mainfoldOut)
        {
            GUILayout.BeginVertical();

            FoilageDistributionUI();
            GUILayout.Space(5f);

            GUILayout.BeginHorizontal();
            EditorUtils.Label("Point Radius", 15, 0, minWidth: 100f);
            generator.pointRadius = EditorUtils.Float(ref generator.pointRadius, "", 15f, 15f, minWidth: 100);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorUtils.Label("Debug", 15, 0, minWidth: 100f);
            generator.debugMode = EditorUtils.Toggle(ref generator.debugMode, "", 0, 0);
            GUILayout.EndHorizontal();

            if (EditorUtils.Button("Generate Points", 15f, 15f)) { GeneratePoints(); SceneView.RepaintAll(); };

            if (EditorUtils.Button("Clear Points Array", 15f, 15f)) { generator.ClearPoints(); SceneView.RepaintAll(); };

            if (EditorUtils.Button("Distribute", 15f, 15f))
            {
                generator.Distribute(FoilageEditor.Instance.foilageGroupsEditor.groups);
                generator.debugMode = false;
                SceneView.RepaintAll();
            };

            GUILayout.EndVertical();
        }
    }

    // foilage distribution settings panel
    bool foilageDistributionPanel = false;
    void FoilageDistributionUI()
    {
        GUILayout.BeginHorizontal();
        foilageDistributionPanel = EditorUtils.Foldout("Distribution Settings", foilageDistributionPanel, 15, 15);
        GUILayout.EndHorizontal();

        if (foilageDistributionPanel)
        {
            GUILayout.Space(10);
            GUILayout.Space(10);
        }
    }

    /// <summary>
    /// distributes vegetation is a given location
    /// </summary>
    void GeneratePoints()
    {
        if(Selection.activeGameObject == null)
        {
            Debug.Log("select a location in scene first");
            return;
        }

        if (Selection.activeGameObject.GetComponent<Location_Tool.Location>())
            generator.GeneratePoints(Selection.activeGameObject.GetComponent<Location_Tool.Location>());
        else
            Debug.Log("Selected scene object is not a location");
    }

    /// <summary>
    /// Just for debugging if need be, before actually instancing some prefab.
    /// </summary>
    void OnDrawGizmos()
    {
        if (generator == null || !generator.debugMode) 
            return;

        if (generator.location == null)
        { return; }

        if (generator.location.boundaries.Count >= 4 && generator.Points != null)
        {
            Handles.DrawWireCube(generator.location.GetCentre(), generator.location.GetRectSize());
            Handles.color = Color.blue;
            foreach (Vector3 point in generator.Points)
                Handles.DrawWireCube(point, new Vector3(0.1f, 0.1f, 0.1f));

            // this is just to vis the grid created by "Poison Disc Sampler"
            //foreach (var item in sampler.Grid)
            //{
            //    Vector3 finalPos = prefab_refObj.transform.TransformPoint(new Vector3(item.x,0,item.y));
            //    Gizmos.DrawWireCube(finalPos, new Vector3(0.5f,0.5f,0.5f));
            //}
        }
    }

}


[System.Serializable]
public class GrassAndGroundCover
{
    FoilageSystem.GrassAndGroundCover grassAndGroundCover = new FoilageSystem.GrassAndGroundCover();

    public void OnEnable()
    {
    }

    bool mainfoldOut = false;
    public void UpdateUI()
    {
        mainfoldOut = EditorUtils.Foldout("Grass And Ground Cover", mainfoldOut, 5, 15);
        if (mainfoldOut)
        {
            DrawMainUI();
        }
    }

    public void OnSceneGUI(SceneView sv)
    {

    }

    void DrawMainUI()
    {
        GUILayout.Space(5);

        EditorUtils.Texture2dField("Grass Map", ref grassAndGroundCover.grassMap, 15f, 15f);
        EditorUtils.Texture2dField("Bush Map", ref grassAndGroundCover.treeMap, 15f, 15f);

        GUILayout.BeginHorizontal();
        EditorUtils.Label("Grass Density", 15, 0);
        GUILayout.FlexibleSpace();
        grassAndGroundCover.grassDensity = EditorGUILayout.Slider(grassAndGroundCover.grassDensity, 0.01f, 3f);
        GUILayout.Space(15);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorUtils.Label("Grass Clumping", 15, 0);
        GUILayout.FlexibleSpace();
        grassAndGroundCover.grassClumping = EditorGUILayout.Slider(grassAndGroundCover.grassClumping, 0f, 1f);
        GUILayout.Space(15);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorUtils.Label("Bush/Detail Density", 15, 0);
        GUILayout.FlexibleSpace();
        grassAndGroundCover.bushDensity = EditorGUILayout.Slider(grassAndGroundCover.bushDensity, 0.01f, 2f);
        GUILayout.Space(15);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        if (EditorUtils.Button("Simulate", 15, 0))
        {
            grassAndGroundCover.Generate();
        }

        GUILayout.Space(5);
    }
}


[System.Serializable]
public class FoilageEditor : EditorWindow
{
    SceneView sceneView;
    static FoilageEditor instance = null;
    public static FoilageEditor Instance {
        get
        {
            return instance;
        }
    }

    // textures
    Texture2D headerSectionTexture;
    Texture2D uiSectionTexture;

    // rect
    Rect headerSection;
    Rect uiSection;

    // ****************************************** Classes ******************************************
    GeoPaint geoPaint = new GeoPaint();
    Texturing texturing = new Texturing();
    GrassAndGroundCover grassAndGroundCover = new GrassAndGroundCover();
    FoilageArea foilageArea = new FoilageArea();
    public FoilageGroupsEditor foilageGroupsEditor = new FoilageGroupsEditor();

    void CreateInstance()
    {
        if (instance == null)
            instance = this;
    }

    [MenuItem("FoilageEditor_v1.0/FoilageEditor")]
    static void OpwnWindow()
    {
        FoilageEditor window = (FoilageEditor)GetWindow<FoilageEditor>();
        // width, height
        window.minSize = new Vector2(350, 600);
        window.Show();
    }

    void OnEnable()
    {
        CreateInstance();

        LoadTextures();

        geoPaint.OnEnable();
        texturing.OnEnable();
        foilageArea.OnEnable();
        grassAndGroundCover.OnEnable();
        foilageGroupsEditor.OnEnable();

        SceneView.duringSceneGui += this.OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnValidate()
    {
        CreateInstance();

        LoadTextures();

        geoPaint.OnEnable();
        texturing.OnEnable();
        foilageArea.OnEnable();
        grassAndGroundCover.OnEnable();
        foilageGroupsEditor.OnEnable();
    }

    void LoadTextures()
    {
        // header section texture
        headerSectionTexture = new Texture2D(1, 1);
        headerSectionTexture.SetPixel(0, 0, Color.clear);
        headerSectionTexture.Apply();

        // ui section texture
        uiSectionTexture = new Texture2D(1, 1);
        uiSectionTexture.SetPixel(0, 0, Color.grey);
        uiSectionTexture.Apply();
    }

    void OnGUI()
    {
        DrawHeaders();
        GUILayout.BeginArea(uiSection);

        GUILayout.Space(10f);
        foilageGroupsEditor.UpdateUI();
        GUILayout.Space(10f);

        geoPaint.UpdateUI(this);
        GUILayout.Space(10f);

        foilageArea.UpdateUI();
        GUILayout.Space(10f);

        grassAndGroundCover.UpdateUI();
        GUILayout.Space(10f);

        texturing.UpdateUI();
        GUILayout.Space(10f);

        GUILayout.EndArea();
    }

    void OnSceneGUI(SceneView sv)
    {
        geoPaint.OnSceneGUI(sv);
        foilageArea.OnSceneGUI(sv);
        grassAndGroundCover.OnSceneGUI(sv);
        texturing.OnSceneGUI(sv);
    }

    void DrawHeaders()
    {
        // header section rect
        headerSection.x = 0;
        headerSection.y = 0;
        headerSection.width = Screen.width;
        headerSection.height = 120f;

        // ui settings rect
        uiSection.x = 0;
        uiSection.y = 120f;
        uiSection.width = Screen.width;
        uiSection.height = Screen.height - 120f;

        if (headerSectionTexture == null || uiSectionTexture == null)
        {
            OnEnable();
        }

        GUI.DrawTexture(headerSection, headerSectionTexture);
        GUI.DrawTexture(uiSection, uiSectionTexture);
    }
}
