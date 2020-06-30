using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(Sense.FOV))]
public class FOVEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

    void OnSceneGUI()
    {
        Sense.FOV fov = (Sense.FOV)target;

        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.viewRadius);

        Vector3 viewAngleA = Utils.Utils.DirFromAngle(fov.viewAngle / 2, false, fov.transform);
        Vector3 viewAngleB = Utils.Utils.DirFromAngle(-fov.viewAngle / 2, false, fov.transform);

        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);

        Handles.color = Color.red;
        foreach (Transform target in fov.VisibleTargets)
        {
            Handles.DrawLine(fov.transform.position, target.position);
        }

    }
}
