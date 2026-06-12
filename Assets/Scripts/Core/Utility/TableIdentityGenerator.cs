using System.Text;
using UnityEngine;

namespace PlinkoPinball.Core.Utility
{
    /// <summary>
    /// Trigger / Switch의 ID를 일관된 규칙으로 생성하는 유틸리티.
    /// </summary>
    // 인스펙터 수동 입력 부담을 줄이고 오타를 방지하기 위함. 
    // 에디터 보조용으로 사용하며 실제 사용은 OnValidate에서 수행
    public static class TableIdentityGenerator
    {
        /// <summary>
        /// GameObject의 이름을 ID에 안전하게 넣기 위해 slug 문자열로 변환합니다.
        /// </summary>
        /// <param name="rawName">원본 이름을 넣습니다.</param>
        /// <returns>소문자 기반 slug 문자열을 반환합니다.</returns>
        public static string ToSlug(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                return "noname";
            }

            var sb = new StringBuilder(rawName.Length);
            bool previousWasSeparator = false;

            for (int i = 0; i < rawName.Length; i++)
            {
                char c = rawName[i];

                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(char.ToLowerInvariant(c));
                    previousWasSeparator = false;
                }
                else
                {
                    if (previousWasSeparator)
                    {
                        continue;
                    }

                    sb.Append('-');
                    previousWasSeparator = true;
                }
            }

            string result = sb.ToString().Trim('-');
            return string.IsNullOrEmpty(result) ? "noname" : result;
        }

        /// <summary>
        /// Trigger용 event Id를 생성합니다.
        /// </summary>
        /// <param name="prefix">event의 종류를 넣습니다.</param>
        /// <param name="target">대상 Transform을 넣습니다.</param>
        /// <returns>생성된 eventId를 반환합니다.</returns>
        public static string CreateEventId(string prefix, Transform target)
        {
            string safePrefix = string.IsNullOrWhiteSpace(prefix) ? "event" : ToSlug(prefix);
            string safeName = target != null ? ToSlug(target.name) : "noname";

            return $"{safePrefix}.{safeName}";
        }

        /// <summary>
        /// 파생 event Id를 생성합니다.
        /// </summary>
        /// <param name="baseEventId">기본 eventId를 넣습니다.</param>
        /// <param name="suffix">추가되는 식별자 이름을 넣습니다.</param>
        /// <returns>suffix가 결합된 eventId를 반환합니다.</returns>
        public static string CreateDerivedEventId(string baseEventId, string suffix)
        {
            string safeBase = string.IsNullOrWhiteSpace(baseEventId) ? "event.noname" : baseEventId.Trim();
            string safeSuffix = string.IsNullOrWhiteSpace(suffix) ? "event" : ToSlug(suffix);

            return $"{safeBase}.{safeSuffix}";
        }

        /// <summary>
        /// Switch Id를 생성합니다.
        /// </summary>
        /// <param name="target">ID를 생성할 대상 Transform을 넣습니다.</param>
        /// <returns>생성된 switchId를 반환합니다.</returns>
        public static string CreateSwitchId(Transform target)
        {
            return target != null ? ToSlug(target.name) : "switch";
        }

        /// <summary>
        /// 부모 Transform 이름을 기반으로 groupId를 생성합니다. 부모가 없을 경우 빈 문자열을 반환합니다.
        /// </summary>
        /// <param name="target">기준이 되는 Transform을 넣습니다.</param>
        /// <returns>부모에 기반한 groupId 또는 빈 문자열을 반환합니다.</returns>
        public static string CreateParentBasedGroupId(Transform target)
        {
            if (target == null || target.parent == null)
            {
                return string.Empty;
            }

            return ToSlug(target.parent.name);
        }
    }
}
