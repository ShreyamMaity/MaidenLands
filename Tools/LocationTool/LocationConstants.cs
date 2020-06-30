using UnityEngine;


namespace Location_Tool
{

    [System.Serializable]
    public static class LocationConstants
    {
        public static Vector3 INVALID_DESTINATION = new Vector3(-1, -1, -1);
        public static float pointDistanceFromGround = 0.2f;

        public static bool drawDestinations = true;
        public static bool drawLocations = true;

        public static Color destMarkerColor = Color.magenta;

        public static float markerRadius = 0.2f;
    }

    [System.Serializable]
    public enum LocationCategory
    {
        None,
        Town,
        Travern,
        Woods,
        Inn,
        PointOfInterest,
        Bakery,
        Home,
        Area,
        WoodPiles,
        RedZone,
        RainShelter,
        SittingPlace,
        Camp,
        Farm,
        Mill,
        Garden,
    }

    public enum DestinationCategory
    {
        Position,
        SpawnPoint,
    }

    public enum GoTo
    {
        FixedLocation_And_FixedDestination,
        FixedLocation_And_RandomDestination,
        RandomLocation_And_FixedDestination,
        RandomLocation_And_RandomDestination,
        RandomLocationInLocation_And_RandomDestination,
    }

}
