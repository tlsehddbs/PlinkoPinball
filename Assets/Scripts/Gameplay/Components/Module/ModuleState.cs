using System;
using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Gameplay.Components.Reactions;

namespace PlinkoPinball.Gameplay.Modules
{
    /// <summary>
    /// 모듈의 런타임 상태를 관리하는 컴포넌트.
    ///
    /// 책임:
    /// - 하위 SwitchState 수집/캐싱
    /// - switchId / groupId 기반 조회 제공
    /// - 그룹별 ON 개수 / 완료 여부 계산
    /// - 상태 변경 시 dirty 마킹
    /// - 모듈 상태 리셋
    ///
    /// 비책임:
    /// - 점수 계산
    /// - TableEvent 발행
    /// - Trigger 제어
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ModuleState : MonoBehaviour
    {
        [Header("Collection")]
        [Tooltip("Awake에서 하위 SwitchState를 자동 수집한다.")]
        [SerializeField] private bool collectOnAwake = true;

        public int SwitchCount => _switches.Count;

        /// <summary>
        /// 모듈 상태가 변경되었음을 알린다.
        /// 주로 디버그/UI/확장 포인트용.
        /// </summary>
        public event Action<ModuleState> StateChanged;

        private readonly List<SwitchState> _switches = new List<SwitchState>(16);
        private readonly Dictionary<string, List<SwitchState>> _groups = new Dictionary<string, List<SwitchState>>(8);
        private readonly Dictionary<string, SwitchState> _switchById = new Dictionary<string, SwitchState>(16);

        // 필요 시 계산 캐시 무효화를 위해 사용
        private bool _isDirty = true;

        private void Awake()
        {
            if (collectOnAwake)
                RebuildCache();
        }

        private void OnEnable()
        {
            SubscribeSwitchEvents();
        }

        private void OnDisable()
        {
            UnsubscribeSwitchEvents();
        }

        /// <summary>
        /// 하위 SwitchState를 다시 수집하고 캐시를 재구성한다.
        /// 모듈 동적 재조립 시에만 호출하는 것을 권장한다.
        /// </summary>
        public void RebuildCache()
        {
            UnsubscribeSwitchEvents();

            _switches.Clear();
            _groups.Clear();
            _switchById.Clear();

            var found = GetComponentsInChildren<SwitchState>(true);
            if (found != null)
            {
                for (int i = 0; i < found.Length; i++)
                {
                    var sw = found[i];
                    if (sw == null)
                        continue;

                    _switches.Add(sw);

                    if (!string.IsNullOrWhiteSpace(sw.SwitchId) && !_switchById.ContainsKey(sw.SwitchId))
                        _switchById.Add(sw.SwitchId, sw);

                    if (!string.IsNullOrWhiteSpace(sw.GroupId))
                    {
                        if (!_groups.TryGetValue(sw.GroupId, out var list))
                        {
                            list = new List<SwitchState>(4);
                            _groups.Add(sw.GroupId, list);
                        }

                        list.Add(sw);
                    }
                }
            }

            SubscribeSwitchEvents();
            MarkDirty();
        }

        /// <summary>
        /// switchId로 개별 스위치를 찾는다.
        /// </summary>
        public SwitchState FindSwitch(string switchId)
        {
            if (string.IsNullOrWhiteSpace(switchId))
                return null;

            _switchById.TryGetValue(switchId, out var sw);
            return sw;
        }

        /// <summary>
        /// 특정 그룹의 전체 스위치 개수를 반환한다.
        /// </summary>
        public int GetTotalCount(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return 0;

            return _groups.TryGetValue(groupId, out var list) ? list.Count : 0;
        }

        /// <summary>
        /// 특정 그룹의 ON 상태 스위치 개수를 반환한다.
        /// </summary>
        public int GetOnCount(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return 0;

            if (!_groups.TryGetValue(groupId, out var list) || list == null)
                return 0;

            int count = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var sw = list[i];
                if (sw != null && sw.IsOn)
                    count++;
            }
            // Debug.Log($"모듈 내 활성화 된 스위치의 개수 : {count} / 총 스위치의 개수 : {list.Count}");
            return count;
        }

        /// <summary>
        /// 그룹 완료 여부를 판단한다.
        /// </summary>
        /// <param name="groupId">조회할 그룹 ID</param>
        /// <param name="requireAllOn">true면 전부 ON이어야 완료, false면 하나라도 ON이면 완료</param>
        public bool IsGroupComplete(string groupId, bool requireAllOn = true)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return false;

            if (!_groups.TryGetValue(groupId, out var list) || list == null || list.Count == 0)
                return false;

            if (requireAllOn)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var sw = list[i];
                    if (sw == null || !sw.IsOn)
                        return false;
                }

                return true;
            }

            for (int i = 0; i < list.Count; i++)
            {
                var sw = list[i];
                if (sw != null && sw.IsOn)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 모든 하위 스위치를 초기 상태로 복원한다.
        /// </summary>
        public void ResetAllSwitches()
        {
            for (int i = 0; i < _switches.Count; i++)
            {
                if (_switches[i] != null)
                    _switches[i].ResetState();
            }

            MarkDirty();
        }

        /// <summary>
        /// 현재 상태가 변경되었음을 마킹한다.
        /// 캐시 계산 최적화가 필요해질 때 사용할 수 있다.
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
            StateChanged?.Invoke(this);
        }

        /// <summary>
        /// dirty 상태를 해제한다.
        /// 외부에서 계산 캐시를 확정했을 때 사용할 수 있다.
        /// </summary>
        public void ClearDirty()
        {
            _isDirty = false;
        }

        /// <summary>
        /// 상태가 마지막 계산 이후 변경되었는지 반환한다.
        /// </summary>
        public bool IsDirty()
        {
            return _isDirty;
        }

        private void SubscribeSwitchEvents()
        {
            for (int i = 0; i < _switches.Count; i++)
            {
                if (_switches[i] != null)
                    _switches[i].StateChanged += OnSwitchStateChanged;
            }
        }

        private void UnsubscribeSwitchEvents()
        {
            for (int i = 0; i < _switches.Count; i++)
            {
                if (_switches[i] != null)
                    _switches[i].StateChanged -= OnSwitchStateChanged;
            }
        }

        private void OnSwitchStateChanged(SwitchState switchState, bool isOn)
        {
            MarkDirty();
        }
    }
}