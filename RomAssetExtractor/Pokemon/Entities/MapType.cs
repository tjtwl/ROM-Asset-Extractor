namespace RomAssetExtractor.Pokemon.Entities
{
    // Source: map_types.h from pokefirered
    public enum MapType
    {
        None = 0,
        Town = 1,
        City = 2, // Not used by any map. RSE use this map type to distinguish Town and City. FRLG make no distinction
        Route = 3,
        Underground = 4,
        Underwater = 5, // Not used by any map.
        OceanRoute = 6, // Not used by any map.
        Unknown = 7, // Not used by any map.
        Indoor = 8,
        SecretBase = 9, // Not used by any map.
    }
}
