using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Location_Tool
{

    [CustomEditor(typeof(Location))]
    [CanEditMultipleObjects]
    public class LocationEditor : Editor
    {
        Point currentlySelectedPoint;
        Location location;
        bool needsUpdateTransform = true;

        void OnEnable()
        {
            location = (Location)target;
            Refresh();
            currentlySelectedPoint = new Point(LocationConstants.INVALID_DESTINATION);
        }

        void OnSceneGUI()
        {
            Event current = Event.current;
            HandleInput(current);
            DrawDestinations();
            UpdateHandles(current);
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            DrawBasicSettings();
            DrawDestinationsInspector();
            DrawBottomButtons();

            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();

            //serializedObject.Update();
            //serializedObject.ApplyModifiedProperties();
        }

        public void HandleInput(Event e)
        {
            // create new destination point
            if (e.type == EventType.MouseDown && e.button == 0 && e.modifiers == EventModifiers.Control)
            {
                Vector3 mousePos = GetMousePosition();
                bool pointUnderMouse = false;

                // first check if any destination is under mouse.
                // if any destination is under mouse then it is selected in IsAnyDestinationUnderMouse function.
                pointUnderMouse = PointUnderMouse(location, mousePos);

                if (!pointUnderMouse) // if no destination is under mouse then create a new destination.
                {
                    currentlySelectedPoint = location.AddBoundaryPoint(mousePos);
                    // note its local position relative to this game object.
                    Vector3 localPos = location.transform.InverseTransformPoint(currentlySelectedPoint.position);
                    currentlySelectedPoint.localPosition = localPos;
                    currentlySelectedPoint.isSelected = true;
                }
            }
            else if (e.type == EventType.MouseDown && e.button == 0 && e.modifiers == EventModifiers.Shift)
            {
                Vector3 mousePos = GetMousePosition();
                bool pointUnderMouse = false;

                // first check if any destination is under mouse.
                // if any destination is under mouse then it is selected in IsAnyDestinationUnderMouse function.
                pointUnderMouse = PointUnderMouse(location, mousePos);

                if (!pointUnderMouse) // if no destination is under mouse then create a new destination.
                {
                    currentlySelectedPoint = location.AddDestinationPoint(mousePos);
                    // note its local position relative to this game object.
                    Vector3 localPos = location.transform.InverseTransformPoint(currentlySelectedPoint.position);
                    currentlySelectedPoint.localPosition = localPos;
                    currentlySelectedPoint.isSelected = true;
                }
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                Reset_Y_Position();
            }

            else if (e.type == EventType.ScrollWheel && e.modifiers == EventModifiers.Control)
            {
                if (e.delta.y > 0)
                    Utils.ActionManager.Undo();

                else if (e.delta.y < 0)
                    Utils.ActionManager.Redo();
            }

            // to prevent from deselection //
            if (location == null) { return; }
            if (Selection.activeGameObject != location.transform.gameObject)
                Selection.activeGameObject = location.transform.gameObject;
        }

        Vector3 GetMousePosition()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit = new RaycastHit();

            Vector3 mousePos;

            // calculate waypoint position from here
            // especially important is the y-position (height) of waypoint from
            // surface below it.
            if (Physics.Raycast(ray, out hit))
            {
                mousePos = hit.point;
            }
            else
            {
                float drawPlaneHeight = 0;
                float dstToDrawPlane = (drawPlaneHeight - ray.origin.y) / ray.direction.y;
                mousePos = ray.GetPoint(dstToDrawPlane);
            }
            mousePos.y += LocationConstants.pointDistanceFromGround;
            return mousePos;
        }

        void Reset_Y_Position()
        {
            RaycastHit hitInfo;
            foreach (var item in location.boundaries)
            {
                if (Physics.Raycast(item.position, -Vector3.up, out hitInfo)) // create a ray from destinaion position in -Y direction.
                    item.position.y = hitInfo.point.y + LocationConstants.pointDistanceFromGround;
                else
                    item.position.y = 0.0f + LocationConstants.pointDistanceFromGround;
            }
            foreach (var item in location.destinations)
            {
                if (Physics.Raycast(item.position, -Vector3.up, out hitInfo)) // create a ray from destinaion position in -Y direction.
                    item.position.y = hitInfo.point.y + LocationConstants.pointDistanceFromGround;
                else
                    item.position.y = 0.0f + LocationConstants.pointDistanceFromGround;
            }
        }

        bool PointUnderMouse(Location loc, Vector3 mousePos)
        {
            foreach (var item in location.boundaries)
            {
                float distance = Vector3.Distance(mousePos, item.position);
                if (distance < LocationConstants.markerRadius + 0.05f)
                {
                    SelectPoint(item);
                    return true;
                }
            }
            foreach (var item in location.destinations)
            {
                float distance = Vector3.Distance(mousePos, item.position);
                if (distance < LocationConstants.markerRadius + 0.05f)
                {
                    SelectPoint(item);
                    return true;
                }
            }
            return false;
        }

        void SelectPoint(Point destination)
        {
            currentlySelectedPoint.isSelected = false; // deselect previous
            currentlySelectedPoint = destination;
            currentlySelectedPoint.isSelected = true;
        }

        void DrawDestinations()
        {
            foreach (var item in LocationManager.locations.Keys)
            {
                Location obj = LocationManager.locations[item];

                if (obj == null) { LocationManager.locations.Remove(item); return; }

                DrawPoints(
                    obj.boundaries,
                    obj.destinations,
                    obj.boundryMarkerColor,
                    obj.boundryLineColor,
                    obj.name,
                    obj.GetCentre()
                    );
            }
        }

        void DrawPoints(
            List<Point> boundaries,
            List<Point> destinations,
            Color pointColor,
            Color lineColour,
            string name,
            Vector3 center)
        {

            Handles.Label(center, name);

            // draw destinations
            foreach (var item in destinations)
            {
                Handles.color = LocationConstants.destMarkerColor;

                if (item == currentlySelectedPoint)
                    Handles.color = Color.yellow;
                else
                    Handles.color = LocationConstants.destMarkerColor;

                Handles.DrawSolidDisc(item.position, item.GetGroundNormal(), LocationConstants.markerRadius);
            }

            // draw boundaries
            foreach (var item in boundaries)
            {
                if (item == currentlySelectedPoint)
                    Handles.color = Color.yellow;
                else
                    Handles.color = pointColor;

                Handles.DrawSolidDisc(item.position, item.GetGroundNormal(), LocationConstants.markerRadius);
            }

            // Lines connecting the Destination points.

            Handles.color = lineColour;
            int index = 0;

            foreach (var destination in boundaries)
            {
                index++;
                if (index > boundaries.Count - 1)
                {
                    index = 0;
                }

                Vector3 nextDest = boundaries[index].position;
                Handles.DrawDottedLine(destination.position, nextDest, 6f);
            }
        }

        void UpdateHandles(Event e)
        {
            if (currentlySelectedPoint.isSelected == false)
                return;

            currentlySelectedPoint.position = Handles.PositionHandle(currentlySelectedPoint.position, Quaternion.identity);

            if (currentlySelectedPoint.lastPosition != currentlySelectedPoint.position)
            {
                needsUpdateTransform = true;
            }

            if (needsUpdateTransform && e.type == EventType.MouseUp)
            { location.UpdateLocalTransforms(); currentlySelectedPoint.lastPosition = currentlySelectedPoint.position; needsUpdateTransform = false; }
        }

        #region INSPECTOR DRAWING SECTION

        void DrawBasicSettings()
        {
            location.category = (LocationCategory)EditorGUILayout.EnumPopup("LocationCategory", location.category);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Point Distance From Ground");
            LocationConstants.pointDistanceFromGround = EditorGUILayout.FloatField(LocationConstants.pointDistanceFromGround);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Point Radius");
            LocationConstants.markerRadius = EditorGUILayout.Slider(LocationConstants.markerRadius, 0.1f, 1f);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Draw Destinations");
            LocationConstants.drawDestinations = EditorGUILayout.Toggle(LocationConstants.drawDestinations);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Destination Color");
            location.boundryMarkerColor = EditorGUILayout.ColorField(location.boundryMarkerColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Lines Color");
            location.boundryLineColor = EditorGUILayout.ColorField(location.boundryLineColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        void DrawDestinationsInspector()
        {
            if (currentlySelectedPoint.isSelected && location.destinations.Count > 0)
            {
                // label section
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label("Current Dest Name");
                currentlySelectedPoint.destName = EditorGUILayout.TextField(currentlySelectedPoint.destName);

                EditorGUILayout.EndHorizontal();

                if (currentlySelectedPoint.destName.Length > 20)
                {
                    currentlySelectedPoint.destName = currentlySelectedPoint.destName.Remove(20);
                }
                // 

                // destination type
                currentlySelectedPoint.type = (DestinationCategory)EditorGUILayout.EnumPopup("DestinationCategory", currentlySelectedPoint.type);
                currentlySelectedPoint.eulerAngles = EditorGUILayout.Vector3Field("AssignedRotation", currentlySelectedPoint.eulerAngles);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            // draw destination label along with its position and a select button
            foreach (Point destination in location.destinations)
            {
                Vector3 position = destination.position;

                EditorGUILayout.BeginHorizontal();

                GUILayout.Label(destination.destName);
                GUILayout.Label(position.ToString());

                if (GUILayout.Button("Select"))
                {
                    SelectPoint(destination);
                }

                EditorGUILayout.EndHorizontal();
            }
            //
        }

        void DrawBottomButtons()
        {

            if (GUILayout.Button("Refresh Location"))
            {
                // empty the dict.
                Refresh();
            }

            if (GUILayout.Button("Update Transforms"))
            {
                location.UpdateLocalTransforms();
            }

            if (GUILayout.Button("Delete Selected Point"))
            {
                // ** TODO ** //
                // Remove the corresponding entry from action manager queue //
                currentlySelectedPoint.isSelected = false;
                location.RemovePoint(currentlySelectedPoint);
            }

            if (GUILayout.Button("Clear Destinations"))
            {
                currentlySelectedPoint.isSelected = false;
                location.destinations = new List<Point>();
            }

            if (GUILayout.Button("Clear Boundaries"))
            {
                currentlySelectedPoint.isSelected = false;
                location.boundaries = new List<Point>();
            }
        }

        #endregion

        void Refresh()
        {
            LocationManager.locations = new Dictionary<string, Location>();
            location.Refresh();
            location.UpdateTransform();
        }

    }
}