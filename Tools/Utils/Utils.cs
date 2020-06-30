using UnityEngine;
using System.Collections.Generic;


namespace Utils
{
    public static class GeomUtils2d
    {
        public static int Orientation(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float val = (p2.y - p1.y) * (p3.x - p2.x) - 
                        (p2.x - p1.x) * (p3.y - p2.y);
            if (val == 0) return 0; // collinear
            return (val > 0) ? 1 : 2; // clockwise or counterclockwise
        }

        public static bool OnSegment(Vector2 p, Vector2 r, Vector2 q)
        {
            if (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) &&
                q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y))
                return true;

            return false;
        }

        public static bool IsIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
        {
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);

            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            if ((o1 != o2) && (o3 != o4)) return true;

            // special case when
            // p1, q1, p2 are collinear and p2 lies on p1q1

            if (o1 == 0 && OnSegment(p1, p2, p2)) return true;

            // special case when
            // p1, p1, q2 are collinear and q2 lies on p1q1

            if (o2 == 0 && OnSegment(p1, p2, q2)) return true;

            // special case when
            // p2, q2, p1 are collinear and p1 lies on p2q2

            if (o3 == 0 && OnSegment(p2, q2, p1)) return true;

            // special case when
            // p2, q2, q1 are collinear and q1 lies on p2q2

            if (o4 == 0 && OnSegment(p2, q2, q1)) return true;

            return false;
        }

        public static bool PointInsidePolygon(Vector2[] polygon, int n, Vector2 p)
        {
            if (n < 3) return false; // vertext count must be > 3

            int count = 0; int i = 0;

            do
            {
                int next = (i + 1) % n;

                if (IsIntersect(polygon[i], polygon[next], p, new Vector2(float.MaxValue, p.y)))
                {
                    if (Orientation(polygon[i], polygon[next], p) == 0)
                    {
                        return OnSegment(polygon[i], polygon[next], p);
                    }

                    count++;
                }
                i = next;
            } while (i != 0);

            // Return true if count is odd, false otherwise.
            return count % 2 == 1;
        }

        public static Vector2 Compute2DPolygonCentroid(Vector2[] vertices, int vertexCount)
        {
            Vector2 centroid = new Vector2(0, 0);

            double signedArea = 0.0f;
            float x0 = 0.0f; // Current vertex X
            float y0 = 0.0f; // Current vertex Y
            float x1 = 0.0f; // Next vertex X
            float y1 = 0.0f; // Next vertex Y
            float a = 0.0f;  // Partial signed area

            // For all vertices
            int i = 0;
            for (i = 0; i < vertexCount - 1; ++i)
            {
                x0 = vertices[i].x;
                y0 = vertices[i].y;
                x1 = vertices[(i + 1) % vertexCount].x;
                y1 = vertices[(i + 1) % vertexCount].y;
                a = x0 * y1 - x1 * y0;
                signedArea += a;
                centroid.x += (x0 + x1) * a;
                centroid.y += (y0 + y1) * a;
            }

            // Do last vertex separately to avoid performing an expensive
            // modulus operation in each iteration.
            x0 = vertices[i].x;
            y0 = vertices[i].y;
            x1 = vertices[0].x;
            y1 = vertices[0].y;
            a = x0 * y1 - x1 * y0;
            signedArea += a;
            centroid.x += (x0 + x1) * a;
            centroid.y += (y0 + y1) * a;

            signedArea *= 0.5;
            centroid.x /= (float)(6.0 * signedArea);
            centroid.y /= (float)(6.0 * signedArea);

            return centroid;
        }
    }


    public static class Utils
    {
        public static bool IsEqual(double x, double y)
        {
            if (Mathf.Abs((float)x - (float)y) < Mathf.Exp(1) - 12)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Static function to test Point inside circle
        /// returns false if point is on the circle.
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="radius"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool PointInsideCircle(Vector3 centre, float radius, Vector3 point)
        {
            // distance between circle centre and the point to test
            float distance = Vector3.Distance(centre, point);

            // if point is inside the circle
            if (distance < radius)
                return true;

            // if point is outside the circle
            else if (distance > radius)
                return false;

            // if point is on the circle
            return false;
        }

        /// <summary>
        /// Returns a Direction Vector from angle in degrees.
        /// </summary>
        /// <param name="angleInDegrees"></param>
        /// <param name="angleIsGlobal"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal, Transform t = null)
        {
            if (!angleIsGlobal && t != null)
            {
                angleInDegrees += t.localEulerAngles.y;
            }
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }


        public static bool IsColinear(Vector3 start, Vector3 end, Vector3 inBetween)
        {
            return Vector3.Cross(end - start, inBetween - start) == Vector3.zero;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool OnLine(Vector3 start, Vector3 end, Vector3 inBetween)
        {
            return inBetween.x <= end.x && inBetween.x >= start.x
              && inBetween.y <= end.y && inBetween.y >= start.y
              && inBetween.z <= end.z && inBetween.z >= start.z
              && Vector3.Cross(end - start, inBetween - start) == Vector3.zero;
        }

    }
}