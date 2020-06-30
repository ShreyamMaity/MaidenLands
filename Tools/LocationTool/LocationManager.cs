using System.Collections.Generic;
using UnityEngine;


namespace Location_Tool
{
    public static class LocationManager
    {
        public static int maxLocationCount = 30;

        public static Dictionary<string, Location> locations = new Dictionary<string, Location>();

        /// <summary>
        /// Add the location this is main repo of locations,
        /// </summary>
        /// <param name="key"></param>
        /// <param name="location"></param>
        public static void Add(string key, Location location)
        {
            if(locations.ContainsKey(key))
            { return; }

            locations[key] = location;
        }

        public static void Remove(string key)
        {
            if (locations.ContainsKey(key))
            { locations.Remove(key); return; }

        }

        /// <summary>
        /// Get the location by name.
        /// </summary>
        /// <param name="locationName"></param>
        /// <returns></returns>
        public static Location GetLocation(string locationName)
        {
            if (locations.ContainsKey(locationName))
                return locations[locationName];

            return null;
        }

        /// <summary>
        /// Returns a random unoccupied location of category
        /// </summary>
        /// <returns></returns>
        public static Location GetRandUnoccupied(LocationCategory t)
        {
            List<Location> rLocations = new List<Location>();

            // Get all locations of a particular type
            foreach (var item in locations.Values)
            {
                if (item.category == t && !item.isOccupied) rLocations.Add(item); // Add if type matches
            }

            // meaning no location is available
            if (rLocations.Count == 0)
                return null;

            Location rLocation = rLocations[UnityEngine.Random.Range(0, rLocations.Count)];
            return rLocation;
        }

    }

}
