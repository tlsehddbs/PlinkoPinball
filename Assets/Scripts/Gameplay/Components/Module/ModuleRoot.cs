using System;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Modules
{
    /// <summary>
    /// 모듈 프리팹 최상위에 위치하는 설정/룰 컴포넌트.
    ///
    /// 책임:
    /// - 모듈 ID 보유
    /// - 기본 점수 배수 제공
    /// - 그룹 완료 시 추가 배수 규칙 해석
    /// - ModuleState를 통해 현재 상태를 조회
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
        }

        // 단발성 보너스 점수를 주는 규칙을 효환가능하게 제작하여야 함

        [Header("Identity")]
        [Tooltip("디버그/데이터 식별용 모듈 ID.")]
        [SerializeField] private string moduleId = "module";

        [Header("Score")]
        [Tooltip("모듈 기본 배수. 일반적으로 1.0을 사용한다.")]
        [Min(0f)]
        [SerializeField] private float baseMultiplier = 1f;

        [Header("Switch Group Rules")]
        [SerializeField] private SwitchGroupRule[] groupRules = Array.Empty<SwitchGroupRule>();

        public string ModuleId => moduleId;

        public ModuleState State { get; private set; }


        private void Awake()
        {
            // 모듈의 ID 를 프리팹(하이어라키에 올라간 이름)으로 초기화
            moduleId = gameObject.name;

            State = GetComponent<ModuleState>();
        }

        /// <summary>
        /// 현재 모듈의 최종 점수 배수를 계산한다.
        /// </summary>
        public float GetScoreMultiplier()
        {
            float result = baseMultiplier;

            if (State == null || groupRules == null || groupRules.Length == 0)
                return result;

            for (int i = 0; i < groupRules.Length; i++)
            {
                var rule = groupRules[i];
                if (rule == null || string.IsNullOrWhiteSpace(rule.groupId))
                    continue;

                if (State.IsGroupComplete(rule.groupId, rule.requireAllOn))
                    result += rule.completedMultiplierBonus;

                // Debug.Log(State.GetOnCount(rule.groupId));
            }

            return result;
        }

        // 아래의 Get 함수들은 외부에서 사용될 때를 가정하여 제작한 부분임. ex) ScoreSystem

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
        /// 모듈 상태를 초기화한다.
        /// </summary>
        public void ResetModuleState()
        {
            if (State != null)
                State.ResetAllSwitches();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(moduleId))
                moduleId = gameObject.name;

            if (baseMultiplier < 0f)
                baseMultiplier = 0f;
        }
#endif
    }
}