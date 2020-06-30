using System.Collections.Generic;
using UnityEngine;


namespace Location_Tool
{

    [System.Serializable]
    public class List_Add : Utils.Action
    {
        public Point point;
        Location location;

        public List_Add(Point dest, Location loc)
        {
            point = dest;
            location = loc;
        }

        public override void Undo()
        {
            point.isSelected = false;
            location.RemovePoint(point);
        }

        public override void Redo()
        {
            point.isSelected = true;
            // location.AddBoundaryPoint(point);
        }
    }

    [System.Serializable]
    public class Point
    {
        public Vector3 position = new Vector3();
        public bool isSelected = false;
        public bool isOccupied = false;

        public DestinationCategory type = DestinationCategory.Position;
        public string destName = "____________________";

        // the local position of this destination relative the gameobject it is attached to
        public Vector3 localPosition = Vector3.zero;
        public Vector3 lastPosition = Vector3.zero;
        public Vector3 eulerAngles = new Vector3(0, 90f, 0);

        public Point(Vector3 pos)
        {
            position = pos;
        }

        public void UpdatePosition(Transform refTrans)
        {
            position = refTrans.TransformPoint(localPosition);
        }

        public Vector3 GetGroundNormal()
        {
            // get the direction of normal vector from the ground, 
            // 
            Ray ray = new Ray(position, Vector3.down);
            RaycastHit hitInfo;
            Vector3 groundNormal = Vector3.up;

            if (Physics.Raycast(ray, out hitInfo, 0.5f))
            {
                groundNormal = hitInfo.normal;
            }
            return groundNormal;
        }

        public Vector2 Position2D { get { return new Vector2(position.x, position.z); } }

        public bool IsOccupied() { return false; }
    }


    [System.Serializable]
    [ExecuteInEditMode]
    public class Location : MonoBehaviour
    {
        // the category of location
        [HideInInspector]
        public LocationCategory category;

        // boundaries of this location
        [HideInInspector]
        public List<Point> boundaries = new List<Point>();

        // destinations in this location
        [HideInInspector]
        public List<Point> destinations = new List<Point>();

        public bool isOccupied = false;

        public Color boundryLineColor = Color.red;
        public Color boundryMarkerColor = Color.blue;

        public void OnValidate()
        {
            Refresh();
        }

        public void OnDestroy()
        {
            foreach (var item in gameObject.GetComponentsInChildren<Location>())
            {
                LocationManager.Remove(item.name);
            }
        }

        public void Refresh()
        {
            foreach (var item in gameObject.GetComponentsInChildren<Location>())
            {
                LocationManager.Add(item.name, item);
            }
        }

        public Point AddBoundaryPoint(Vector3 position)
        {
            // create the new destination at position.
            Point newPoint = new Point(position);
            boundaries.Add(newPoint);
            // create an action as well for undo redo operations.
            List_Add newActn = new List_Add(newPoint, this);
            Utils.ActionManager.Add(newActn);
            return newPoint;
        }

        public Point AddDestinationPoint(Vector3 position)
        {
            // create the new destination at position.
            Point newPoint = new Point(position);
            destinations.Add(newPoint);
            // create an action as well for undo redo operations.
            List_Add newActn = new List_Add(newPoint, this);
            Utils.ActionManager.Add(newActn);
            return newPoint;
        }

        public void RemovePoint(Point point)
        {
            if (boundaries.Contains(point)) boundaries.Remove(point);
            if (destinations.Contains(point)) destinations.Remove(point);
        }

        /// <summary>
        /// returns a random unoccupied sublocation
        /// </summary>
        public Location GetSubLocation(LocationCategory c)
        {
            List<Location> sublocations = new List<Location>();
            foreach (var item in gameObject.GetComponentsInChildren<Location>())
            {
                if(!item.isOccupied && item.category == c) sublocations.Add(item);
            }
            return sublocations[UnityEngine.Random.Range(0, sublocations.Count)];
        }

        /// <summary>
        /// Returns the location of type Destination in this Location, if any, else
        /// returns null
        /// </summary>
        /// <returns></returns>
        public Location GetDestination()
        {
            Location[] ls = GetComponentsInChildren<Location>();
            foreach (var item in ls)
            {
                if (item.transform.parent == transform)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// Returns a destination specified by its name, only if
        /// destination is not occupied.
        /// Use GetDestination() without params to get Destination in this location,
        /// if any.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Point GetDestination(string name)
        {
            foreach (var item in destinations)
            {
                if (item.destName == name && !item.isOccupied) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns a random destination specified by its type( Position, SpawnPoint etc )
        /// <param name="d"></param>
        /// <returns></returns>
        public Point GetRandomDestination(DestinationCategory d)
        {
            List<Point> subDestinations = new List<Point>();
            foreach (var item in destinations)
            {
                if (item.type == d && !item.isOccupied) subDestinations.Add(item);
            }

            // meaning no destination available
            if (subDestinations.Count == 0)
                return null;

            return subDestinations[UnityEngine.Random.Range(0, subDestinations.Count)];
        }

        public void UpdateTransform()
        {
            UpdateDestinationPositions(transform);

            // then update sublocations
            foreach (var location in GetComponentsInChildren<Location>())
            {
                foreach (var item in location.boundaries)
                {
                    item.UpdatePosition(location.transform);
                    item.localPosition = location.transform.InverseTransformPoint(item.position);
                }
                foreach (var item in location.destinations)
                {
                    item.UpdatePosition(location.transform);
                    item.localPosition = location.transform.InverseTransformPoint(item.position);
                }
            }
        }

        public void UpdateLocalTransforms()
        {
            // then update sublocations
            foreach (var location in GetComponentsInChildren<Location>())
            {
                foreach (var item in location.boundaries)
                {
                    item.localPosition = location.transform.InverseTransformPoint(item.position);
                }
                foreach (var item in location.destinations)
                {
                    item.localPosition = location.transform.InverseTransformPoint(item.position);
                }
            }
        }

        public void UpdateDestinationPositions(Transform refTrans)
        {
            foreach(var item in boundaries)
            {
                item.UpdatePosition(refTrans);
            }
        }

        /// <summary>
        /// Returns the 2d positions ignoring the Y(height)-component.
        /// </summary>
        public Vector2[] Bounderies2d
        {
            get
            {
                Vector2[] positions = new Vector2[boundaries.Count];
                int i = 0;
                foreach (Point dest in boundaries) { positions[i] = dest.Position2D; i++; }
                return positions;
            }
        }

        public Vector3[] Bounderies
        {
            get
            {
                Vector3[] positions = new Vector3[boundaries.Count];
                int i = 0;
                foreach (Point dest in boundaries) { positions[i] = dest.position; i++; }
                return positions;
            }
        }

        /// <summary>
        /// The heighest Y_coordinate value in this location.
        /// </summary>
        /// <returns></returns>
        public float HeighestPoint()
        {
            float heighestPoint = LocationConstants.pointDistanceFromGround;
            foreach (Vector3 position in Bounderies)
            {
                if (position.y > heighestPoint)
                    heighestPoint = position.y;
            }
            return heighestPoint;
        }

        public Vector3 GetMinBounds()
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            // min
            foreach (Point dest in boundaries)
            {
                Vector3 pos = dest.position;
                min.x = pos.x < min.x ? pos.x : min.x;
                min.y = pos.y < min.y ? pos.y : min.y;
                min.z = pos.z < min.z ? pos.z : min.z;
            }
            return min;
        }

        public Vector3 GetMaxBounds()
        {
            Vector3 max = new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue);

            foreach (Point dest in boundaries)
            {
                Vector3 pos = dest.position;

                max.x = Mathf.Max(pos.x, max.x);
                max.y = Mathf.Max(pos.y, max.y);
                max.z = Mathf.Max(pos.z, max.z);
            }
            return max;
        }

        /// <summary>
        /// the minimum bounding box surrounding the location points. 
        /// </summary>
        /// <returns></returns>
        public Vector3 GetRectSize()
        {
            return new Vector3(GetMaxBounds().x - GetMinBounds().x, 0.5f, GetMaxBounds().z - GetMinBounds().z);
        }

        /// <summary>
        /// The centre point of this location.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetCentre()
        {
            if(boundaries.Count < 2)
            {
                // Debug.Log("Cannot calculate center destination count is < then 2");
                return transform.position;
            }

            Vector2[] vertices = new Vector2[boundaries.Count];
            vertices = Bounderies2d;

            Vector2 centroid = Utils.GeomUtils2d.Compute2DPolygonCentroid(vertices, vertices.Length);
            return new Vector3(centroid.x, LocationConstants.pointDistanceFromGround, centroid.y);
        }

    }
}