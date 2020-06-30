using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Triangle
{
    public Vector3 a = new Vector3();
    public Vector3 b = new Vector3();
    public Vector3 c = new Vector3();

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        a = v1;
        b = v2;
        c = v3;
    }

    public Vector3 GetCentre()
    {
        Vector3 centrePos = new Vector3();

        // direct method
        float x = (this.a.x + this.b.x + this.c.x) / 3;
        float y = (this.a.y + this.b.y + this.c.y) / 3;
        float z = (this.a.z + this.b.z + this.c.z) / 3;

        centrePos = new Vector3(x, y, z);

        return centrePos;
    }
}

public static class Geometry
{
    public static List<Triangle> UnityMeshToTriangles(Mesh m, Transform t)
    {
        Vector3[] vertices = m.vertices;
        int[] indices = m.triangles;
        List<Triangle> triangles = new List<Triangle>();

        int nextStop = 3;
        for (int i = 0; i < m.triangles.Length+2; i++)
        {
            if (i > nextStop)
            {
                int indice1 = indices[nextStop - 3];
                int indice2 = indices[nextStop - 2];
                int indice3 = indices[nextStop - 1];

                // create a new triangle
                Vector3 v1 = t.TransformPoint( vertices[indice1] );
                Vector3 v2 = t.TransformPoint( vertices[indice2] );
                Vector3 v3 = t.TransformPoint( vertices[indice3] );

                triangles.Add(new Triangle(v1, v2, v3));

                nextStop += 3;
                continue;
            }
        }

        // Debug.Log(triangles.Count);
        return triangles;
    } 
}
