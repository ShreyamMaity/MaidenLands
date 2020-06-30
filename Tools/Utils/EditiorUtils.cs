using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public static class EditorUtils
{
    static GUIStyle horizontalLine = null;

    static void InitHorizontalLine()
    {
        // custom GUI line
        horizontalLine = new GUIStyle();
        horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
        horizontalLine.margin = new RectOffset(0, 0, 4, 4);
        horizontalLine.fixedHeight = 2f;
    }

    public static void HorizontalLine(Color color)
    {
        if (horizontalLine == null) InitHorizontalLine();

        var c = GUI.color;
        GUI.color = color;
        GUILayout.Box(GUIContent.none, horizontalLine);
        GUI.color = c;
    }



    public static bool Button(string text, float spaceStart, float spaceEnd)
    {
        bool val = false;

        GUILayout.BeginHorizontal();
        GUILayout.Space(spaceStart);

        if (GUILayout.Button(text)) { val = true; }
        else val = false;

        GUILayout.Space(spaceEnd);
        GUILayout.EndHorizontal();

        return val;
    }

    public static bool Toggle(ref bool val, string text, float spaceStart, float spaceEnd)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(spaceStart);

        val = GUILayout.Toggle(val, text);

        GUILayout.Space(spaceEnd);
        GUILayout.EndHorizontal();

        return val;
    }

    public static float Float(ref float val, string text, float spaceStart, float spaceEnd, float minWidth = -1)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(spaceStart);

        if (minWidth == -1)
            val = EditorGUILayout.FloatField(val, text);
        else
            val = EditorGUILayout.FloatField(val, GUILayout.MinWidth(minWidth));

        GUILayout.Space(spaceEnd);
        GUILayout.EndHorizontal();

        return val;
    }

    public static bool Foldout(string text, bool foldout, float spaceStart, float spaceEnd, bool vertical = false)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(spaceStart);

        foldout = EditorGUILayout.Foldout(foldout, text);

        GUILayout.Space(spaceEnd);
        GUILayout.EndHorizontal();

        return foldout;
    }

    public static void Label(string text, float spaceStart, float spaceEnd, float maxWidth = -1, float minWidth = -1)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(spaceStart);

        if(minWidth == -1 && maxWidth == -1) GUILayout.Label(text);
        if (maxWidth != -1) GUILayout.Label(text, GUILayout.MaxWidth(maxWidth));
        if (minWidth != -1) GUILayout.Label(text, GUILayout.MinWidth(minWidth));

        GUILayout.Space(spaceEnd);
        GUILayout.EndHorizontal();
    }

    public static string Text(float spaceStart, float spaceEnd, float maxWidth = -1, float minWidth = -1)
    {
        string text = "";

        GUILayout.BeginHorizontal();
        GUILayout.Space(spaceStart);

        if (minWidth == -1 && maxWidth == -1) GUILayout.TextField("");
        if (maxWidth != -1)
        {
            text = GUILayout.TextField("", GUILayout.MaxWidth(maxWidth));
        }
        if(minWidth != -1)
        {
            text = GUILayout.TextField("", GUILayout.MinWidth(minWidth));
        }

        GUILayout.Space(spaceEnd);
        GUILayout.EndHorizontal();

        return text;
    }

    public static void BoldLabel(string text, float spaceStart, float spaceEnd)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(spaceStart);

        GUILayout.Label(text, EditorStyles.boldLabel);

        GUILayout.Space(spaceEnd);
        GUILayout.EndHorizontal();
    }

    public static Vector3 VectorField(Vector3 vector, string label, float spaceStart, float spaceEnd)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.Label(label);
        vector = EditorGUILayout.Vector3Field("", vector);
        GUILayout.Space(15);
        GUILayout.EndHorizontal();

        return vector;
    }

    public static Texture2D Texture2dField(string label, ref Texture2D texture, float startSpace, float endSpace)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(startSpace);
        if(label != "")
            GUILayout.Label(label);
        texture = (Texture2D)EditorGUILayout.ObjectField("", texture, typeof(Texture2D), false);
        GUILayout.Space(endSpace);
        GUILayout.EndHorizontal();

        return texture;
    }

    public static GameObject GameObjectField(string label, ref GameObject gameObject, float startSpace, float endSpace, float minWidth = -1, float maxWidth = -1)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(startSpace);
        GUILayout.Label(label);

        if (minWidth != -1)
            gameObject = (GameObject)EditorGUILayout.ObjectField("", gameObject, typeof(GameObject), true, GUILayout.MinWidth(minWidth));

        else if (maxWidth != -1)
            gameObject = (GameObject)EditorGUILayout.ObjectField("", gameObject, typeof(GameObject), true, GUILayout.MaxWidth(maxWidth));

        else gameObject = (GameObject)EditorGUILayout.ObjectField("", gameObject, typeof(GameObject), true);

        GUILayout.Space(endSpace);
        GUILayout.EndHorizontal();

        return gameObject;
    }

}
