using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Gameplay.Modules;

namespace PlinkoPinball.Gameplay.Core
{
    public static class ModuleRootLookupCache
    {
        private static readonly Dictionary<Transform, ModuleRoot> Cache = new Dictionary<Transform, ModuleRoot>(128);

        public static ModuleRoot GetOrFind(Transform source)
        {
            if (source == null)
                return null;

            if (Cache.TryGetValue(source, out var cached))
            {
                if (cached != null)
                    return cached;

                Cache.Remove(source);
            }

            var found = source.GetComponentInParent<ModuleRoot>();
            Cache[source] = found;
            return found;
        }

        public static void Clear()
        {
            Cache.Clear();
        }
    }
}