using System;
using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Core;

namespace PlinkoPinball.Gameplay.Components.Reactions
{
    /// <summary>
    /// Hit 이벤트를 입력으로 받아 ON/OFF 상태를 유지하는 스위치 컴포넌트.
    ///
    /// 책임:
    /// - 로컬 Hit 이벤트를 입력으로 받는다.
    /// - 스위치 상태(IsOn)를 변경한다.
    /// - 상태 변경 시 StateChanged 이벤트를 발생시킨다.
    ///
    /// 비책임:
    /// - 점수 계산
    /// - TableEvent 재발행
    /// - 그룹 완료 판정
    /// - 전역 시스템 직접 호출
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SwitchState : MonoBehaviour, ITableEventReaction
    {
        [Header("Identity")]
        [Tooltip("모듈 내부에서 이 스위치를 식별하는 고유 ID.")]
        [SerializeField] private string switchId = "switch";

        [Tooltip("모듈 내부 그룹 판정에 사용하는 그룹 ID. 비어 있으면 그룹 미사용.")]
        [SerializeField] private string groupId = "";

        [Header("Initial State")]
        [Tooltip("라운드 시작 또는 Reset 시 복원할 초기 상태.")]
        [SerializeField] private bool initialOn = false;

        [Header("Input Filter")]
        [Tooltip("비어 있지 않으면 eventId가 정확히 일치할 때만 반응한다.")]
        [SerializeField] private string requiredEventIdExact = "";

        [Tooltip("비어 있지 않으면 eventId가 이 prefix로 시작할 때만 반응한다. exact보다 우선순위가 낮다.")]
        [SerializeField] private string requiredEventIdPrefix = "hit.";

        [Tooltip("비어 있지 않으면 해당 태그가 포함된 이벤트에만 반응한다.")]
        [SerializeField] private string requiredTag = "";

        [Header("Behavior")]
        [Tooltip("true면 한 번 켜진 뒤 다시 OFF로 돌아가지 않는다. (체크형/래치형 스위치)")]
        [SerializeField] private bool latchOnOnly = false;

        [Tooltip("상태 변경 후 이 시간 동안 추가 입력을 무시한다.")]
        [Min(0f)]
        [SerializeField] private float cooldownSeconds = 0.05f;

        /// <summary>
        /// 스위치 식별자.
        /// </summary>
        public string SwitchId => switchId;

        /// <summary>
        /// 그룹 식별자. 비어 있으면 그룹 미사용.
        /// </summary>
        public string GroupId => groupId;

        /// <summary>
        /// 현재 ON/OFF 상태.
        /// </summary>
        public bool IsOn { get; private set; }

        /// <summary>
        /// 상태가 변경되었을 때 호출된다.
        /// 첫 번째 인자는 자신, 두 번째 인자는 변경 후 상태이다.
        /// </summary>
        public event Action<SwitchState, bool> StateChanged;

        private float _nextAllowedTime;

        private void Awake()
        {
            IsOn = initialOn;
            _nextAllowedTime = 0f;
        }

        /// <summary>
        /// Trigger가 전달한 로컬 TableEvent를 받아 스위치 상태를 처리한다.
        /// </summary>
        /// <param name="e">입력 이벤트</param>
        public void OnTableEvent(in TableEvent e)
        {
            if (!isActiveAndEnabled)
                return;

            // 현재 설계에서는 Hit 입력만 스위치에 반응시킨다.
            if (e.eventType != TableEventType.Hit)
                return;

            if (!PassesEventFilter(in e))
                return;

            if (cooldownSeconds > 0f && Time.time < _nextAllowedTime)
                return;

            _nextAllowedTime = Time.time + cooldownSeconds;

            if (latchOnOnly)
            {
                // 이미 ON이면 재통지하지 않음
                if (!IsOn)
                    SetStateInternal(true, notify: true);

                return;
            }

            SetStateInternal(!IsOn, notify: true);
        }

        /// <summary>
        /// 초기 상태로 복원한다.
        /// </summary>
        public void ResetState()
        {
            SetStateInternal(initialOn, notify: true);
            _nextAllowedTime = 0f;
        }

        /// <summary>
        /// 외부에서 스위치 상태를 강제로 설정한다.
        /// </summary>
        /// <param name="value">설정할 상태</param>
        /// <param name="notify">true면 StateChanged를 호출한다</param>
        public void SetState(bool value, bool notify = true)
        {
            SetStateInternal(value, notify);
        }

        /// <summary>
        /// 외부에서 현재 상태를 토글한다.
        /// </summary>
        /// <param name="notify">true면 StateChanged를 호출한다</param>
        public void Toggle(bool notify = true)
        {
            SetStateInternal(!IsOn, notify);
        }

        /// <summary>
        /// 현재 이벤트가 이 스위치의 입력 조건을 만족하는지 판정한다.
        /// </summary>
        private bool PassesEventFilter(in TableEvent e)
        {
            // exact match 우선
            if (!string.IsNullOrEmpty(requiredEventIdExact))
            {
                if (!string.Equals(e.eventId, requiredEventIdExact, StringComparison.Ordinal))
                    return false;
            }
            else if (!string.IsNullOrEmpty(requiredEventIdPrefix))
            {
                if (string.IsNullOrEmpty(e.eventId) ||
                    !e.eventId.StartsWith(requiredEventIdPrefix, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(requiredTag))
            {
                if (e.tags == null || e.tags.Length == 0)
                    return false;

                for (int i = 0; i < e.tags.Length; i++)
                {
                    if (string.Equals(e.tags[i], requiredTag, StringComparison.Ordinal))
                        return true;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 내부 상태를 변경한다.
        /// </summary>
        /// <param name="value">새 상태</param>
        /// <param name="notify">true면 상태 변경 이벤트를 발생시킨다</param>
        private void SetStateInternal(bool value, bool notify)
        {
            if (IsOn == value)
                return;

            IsOn = value;

            if (notify)
                StateChanged?.Invoke(this, IsOn);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(switchId))
                switchId = gameObject.name;
        }
#endif
    }
}