using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Modules
{
    /// <summary>
    /// 모듈 프리팹 최상위에 위치하는 설정/룰 컴포넌트.
    ///
    /// 책임:
    /// - 모듈 ID 보유
    /// - 기본 점수 배수 제공
    /// - 그룹 완료 시 추가 점수 배수 규칙 해석
    /// - ModuleState를 통해 현재 Switch 그룹 상태 조회
    /// - 그룹별 완료 상태 변화를 감지하여 보상 시스템에 알림
    ///
    /// 주의:
    /// - Trigger를 직접 알지 않는다.
    /// - Plinko SnapshotBuilder를 직접 수정하지 않는다.
    /// - Currency를 직접 지급하지 않는다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ModuleState))]
    public sealed class ModuleRoot : MonoBehaviour
    {
        [Serializable]
        private sealed class SwitchGroupRule
        {
            [Tooltip("ModuleState 내 SwitchState.groupId와 일치해야 한다.")]
            public string groupId = "group";

            [Tooltip("true면 그룹 내 모든 스위치가 ON이어야 완료 상태로 본다.")]
            public bool requireAllOn = true;

            [Tooltip("그룹 완료 시 추가되는 점수 배수.")]
            [Min(0f)]
            public float completedMultiplierBonus = 0f;

            [Tooltip("true면 이 그룹 완료 상태 변화가 ModuleRewardState 이벤트를 발생시킨다.")]
            public bool emitRewardState = true;
        }

        [Header("Identity")]
        [Tooltip("디버그/데이터 식별용 모듈 ID. 비워두면 Awake에서 GameObject 이름을 사용한다.")]
        [SerializeField] private string moduleId = "module";

        [Header("Score")]
        [Tooltip("모듈 기본 배수. 일반적으로 1.0을 사용한다.")]
        [Min(0f)]
        [SerializeField] private float baseMultiplier = 1f;

        [Header("Switch Group Rules")]
        [SerializeField] private SwitchGroupRule[] groupRules = Array.Empty<SwitchGroupRule>();

        private readonly Dictionary<string, bool> _groupCompletionStates = new();

        public string ModuleId => moduleId;

        public ModuleState State { get; private set; }

        /// <summary>
        /// 모듈 그룹의 보상 상태 변화가 발생했을 때 알림.
        /// parameter:
        /// - moduleId
        /// - groupId
        /// - rewardState
        /// </summary>
        public event Action<string, string, ModuleRewardState> RewardStateChanged;

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(moduleId) || moduleId == "module")
            {
                moduleId = gameObject.name;
            }

            State = GetComponent<ModuleState>();
            InitializeGroupCompletionStates();
        }

        private void OnEnable()
        {
            if (State != null)
            {
                State.StateChanged += OnModuleStateChanged;
            }

            RefreshRewardStates();
        }

        private void OnDisable()
        {
            if (State != null)
            {
                State.StateChanged -= OnModuleStateChanged;
            }
        }

        /// <summary>
        /// 현재 모듈의 최종 점수 배수를 계산한다.
        /// ScoreSystem에서 호출하는 것을 의도한다.
        /// 이 메서드는 보상 이벤트를 발생시키지 않는다.
        /// </summary>
        public float GetScoreMultiplier()
        {
            float result = baseMultiplier;

            if (State == null || groupRules == null || groupRules.Length == 0)
            {
                return result;
            }

            for (int i = 0; i < groupRules.Length; i++)
            {
                SwitchGroupRule rule = groupRules[i];
                if (!IsValidRule(rule))
                {
                    continue;
                }

                if (State.IsGroupComplete(rule.groupId, rule.requireAllOn))
                {
                    result += rule.completedMultiplierBonus;
                }
            }

            return result;
        }

        /// <summary>
        /// 모든 reward emit 대상 그룹의 완료 상태를 평가하고,
        /// 이전 상태와 달라졌을 경우 RewardStateChanged를 발생시킨다.
        /// SwitchState 변경 이후 또는 ModuleState 갱신 이후 호출한다.
        /// </summary>
        public void RefreshRewardStates()
        {
            if (State == null || groupRules == null || groupRules.Length == 0)
            {
                return;
            }

            for (int i = 0; i < groupRules.Length; i++)
            {
                SwitchGroupRule rule = groupRules[i];
                if (!IsValidRule(rule) || !rule.emitRewardState)
                {
                    continue;
                }

                bool completedNow = State.IsGroupComplete(rule.groupId, rule.requireAllOn);
                EvaluateRewardState(rule.groupId, completedNow);
            }
        }

        /// <summary>
        /// 특정 그룹의 완료 상태를 평가하고, 상태 변화가 있을 경우 알린다.
        /// </summary>
        public void EvaluateRewardState(string groupId, bool isCompletedNow)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                return;
            }

            _groupCompletionStates.TryGetValue(groupId, out bool wasCompleted);

            if (!wasCompleted && isCompletedNow)
            {
                RewardStateChanged?.Invoke(moduleId, groupId, ModuleRewardState.Completed);
            }
            else if (wasCompleted && !isCompletedNow)
            {
                RewardStateChanged?.Invoke(moduleId, groupId, ModuleRewardState.Deactivated);
            }

            _groupCompletionStates[groupId] = isCompletedNow;
        }

        /// <summary>
        /// 특정 그룹의 ON 개수를 반환한다.
        /// </summary>
        public int GetOnCount(string groupId)
        {
            return State != null ? State.GetOnCount(groupId) : 0;
        }

        /// <summary>
        /// 특정 그룹의 총 개수를 반환한다.
        /// </summary>
        public int GetTotalCount(string groupId)
        {
            return State != null ? State.GetTotalCount(groupId) : 0;
        }

        /// <summary>
        /// 특정 그룹 완료 여부를 반환한다.
        /// </summary>
        public bool IsGroupComplete(string groupId, bool requireAllOn = true)
        {
            return State != null && State.IsGroupComplete(groupId, requireAllOn);
        }

        /// <summary>
        /// 모듈 상태 초기화.
        /// Pinball Round 시작 또는 Reboot 시 호출한다.
        /// </summary>
        public void ResetModuleState()
        {
            if (State != null)
            {
                State.ResetAllSwitches();
            }

            InitializeGroupCompletionStates();
        }

        private void InitializeGroupCompletionStates()
        {
            _groupCompletionStates.Clear();

            if (groupRules == null)
            {
                return;
            }

            for (int i = 0; i < groupRules.Length; i++)
            {
                SwitchGroupRule rule = groupRules[i];
                if (!IsValidRule(rule))
                {
                    continue;
                }

                bool completed = State != null && State.IsGroupComplete(rule.groupId, rule.requireAllOn);
                _groupCompletionStates[rule.groupId] = completed;
            }
        }

        private static bool IsValidRule(SwitchGroupRule rule)
        {
            return rule != null && !string.IsNullOrWhiteSpace(rule.groupId);
        }

        private void OnModuleStateChanged(ModuleState moduleState)
        {
            RefreshRewardStates();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(moduleId))
            {
                moduleId = gameObject.name;
            }

            if (baseMultiplier < 0f)
            {
                baseMultiplier = 0f;
            }
        }
#endif
    }
}
