using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class LocationMenu
{
    [MenuItem("LocationTool_v1.0/CreateLocation")]
    static void CreateLocation(MenuCommand menuCommand)
    {
        GameObject location = new GameObject("Location");
        location.AddComponent<Location_Tool.Location>();

        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(location, menuCommand.context as GameObject);

        location.GetComponent<Location_Tool.Location>().category = Location_Tool.LocationCategory.None;

        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(location, "Create " + location.name);
        Selection.activeObject = location;
    }
}
