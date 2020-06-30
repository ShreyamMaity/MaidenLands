// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeoPainter : MonoBehaviour
{
    [HideInInspector]
    public List<GeoPainterGroup> painterGroups = new List<GeoPainterGroup>();
    [HideInInspector]
    public GeoPainterGroup currentGroup = null;

    string[] objectSelectionMethod = new string[2];

    [HideInInspector]
    public int bibSortIndex = 0;
    [HideInInspector]
    public int soloObjectIndex = 0;
    [HideInInspector]
    public int paintLayer = 0;

    void OnEnable()
    {
        objectSelectionMethod[0] = "Random";
        objectSelectionMethod[1] = "Solo";
    }

    public GeoPainterGroup AddGeoPaintGroup(int atIndex = -1)
    {
        GameObject groupObject = new GameObject();
        groupObject.AddComponent<GeoPainterGroup>();
        GeoPainterGroup group = groupObject.GetComponent<GeoPainterGroup>();

        groupObject.name = "Don't Delete in scene";
        // init the group
        group.Init(groupObject);

        if (atIndex == -1)
            painterGroups.Add(group);
        else
            painterGroups[atIndex] = group;

        return group;
    }

    public void RemoveGeoPaintGroup(GeoPainterGroup group)
    {
        if (painterGroups.Contains(group))
        {
            group.Clean();
            painterGroups.Remove(group);
        }
    }

}
