using System;
using System.Collections.Concurrent;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// Object used to configure and retrieve mappings
    /// </summary>
    /// <remarks>
    /// A single used as a storage, and configuration for all the mappings</remarks>
    public sealed class Mapper
    {
        private static Mapper _instance;
        private readonly ConcurrentDictionary<Type, IMap> _maps;
        private readonly MapperConfiguration _config;

        private Mapper()
        {
            _maps = new ConcurrentDictionary<Type, IMap>();
            _config = new MapperConfiguration();
        }

        private static Mapper Instance => _instance ?? (_instance = new Mapper());

        /// <summary>
        /// The mapper configuration
        /// </summary>
        public static MapperConfiguration Configuration => Instance._config;

        /// <summary>
        /// Retrieves the a map for a type
        /// </summary>
        /// <typeparam name="T">The type the map is for</typeparam>
        /// <remarks>If no map is set creates one automatically base on the conventions set in the configuration</remarks>
        public static Map<T> Map<T>() where T : class, new()
        {
            if (!Instance._maps.TryGetValue(typeof(T), out var map))
            {
                map = new Map<T>();
                Instance._maps.TryAdd(typeof(T), map);
            }
            return map as Map<T>;
        }

        /// <summary>
        /// Manually sets a map for a type
        /// </summary>
        /// <typeparam name="T">The type the map is for</typeparam>
        /// <param name="map">The map to use</param>
        /// <remarks>If a map already exists the new one will replace it</remarks>
        public static void SetMap<T>(Map<T> map) where T : class, new()
        {
            Instance._maps.AddOrUpdate(typeof(T), map, (type, map1) => map);
        }

        /// <summary>
        /// Populates the maps stored in the mapper for the types specified
        /// </summary>
        /// <param name="types">The types to map</param>
        public static void PreMap(params Type[] types)
        {
            var mapType = typeof(Map<>);
            foreach (var type in types)
            {
                if (!Instance._maps.ContainsKey(type))
                {
                    var map = Activator.CreateInstance(mapType.MakeGenericType(type)) as IMap;
                    Instance._maps.TryAdd(type, map);
                }
            }
        }

        /// <summary>
        /// Removes all the current mappings stored
        /// </summary>
        public static void ClearMaps()
        {
            Instance._maps.Clear();
        }
    }
}
