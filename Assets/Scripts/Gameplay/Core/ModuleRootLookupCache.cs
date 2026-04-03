using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Gameplay.Modules;

namespace PlinkoPinball.Gameplay.Core
{
    public static class ModuleRootLookupCache
    {
        private static readonly Dictionary<Transform, ModuleRoot> Cache = new Dictionary<Transform, ModuleRoot>(128);

        /// <summary>
        /// 주어진 source Transform이 속한 ModuleRoot를 반환합니다. 캐시에 없을 경우 부모 체인을 타고 탐색한 뒤 결과를 저장합니다.
        /// </summary>
        /// <param name="source">TableEvent.Source로 전달된 Transform입니다.</param>
        /// <returns>ModuleRoot를 반환합니다. 없을 경우 null을 return합니다.</returns>
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
            
            // 캐싱시 null값도 포함하여 탐색 비용을 줄입니다.
            Cache[source] = found;

            return found;
        }

        /// <summary>
        /// 특정한 source Transform에 대한 캐시를 제거합니다.
        /// </summary>
        /// <param name="source">제거할 source Transform</param>
        public static void Invalidate(Transform source)
        {
            if (source == null)
                return;

            Cache.Remove(source);
        }

        /// <summary>
        /// 전체 캐시를 비웁니다. 모듈 재장착, 리롤, 테이블 재구성 직후 호출하도록 합니다.
        /// </summary>
        public static void Clear()
        {
            Cache.Clear();
        }

        /// <summary>
        /// [개발용] 캐시된 항목의 수를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public static int GetCacheCount()
        {
            return Cache.Count;
        }
    }
}