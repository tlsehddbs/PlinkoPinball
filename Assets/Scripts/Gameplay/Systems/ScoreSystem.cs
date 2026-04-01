using System;
using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Core;
using PlinkoPinball.Gameplay.Modules;

namespace PlinkoPinball.Gameplay.Systems
{
    /// <summary>
    /// TableEvent를 점수로 변환하는 Global System (TableEventBus 구독)
    /// 점수에 대한 규칙, 계산을 이곳에서 처리
    /// 
    /// 이벤트, 태그 기반으로 점수를 계산할 예정(확장에 유연하게 대응)
    /// Trigger의 baseValue 에 multiplier를 적용하여 점수를 누적함
    /// 
    /// 추후 eventId 기반 오버라이드/룰 테이블(SO)로 확장 예정
    /// Contextbonus, upgrade 등 적용 예정
    /// </summary>
    public sealed class ScoreSystem : MonoBehaviour
    {
        [Header("Tag")]
        [Tooltip("이 태그가 있는 이벤트만 점수에 반영. 비어있을 시 태그 필터를 사용하지 않음.")]
        [SerializeField] private string scoreTag = "score";

        [Header("Multiplier")]
        [Min(0.1f)][SerializeField] private float baseMultiplier = 1.0f;   // 전역 기본 배수(업그레이드 관련 부분도 이 것에 영향을 받도록 구상중)
        [Min(1.0f)][SerializeField] private float maxMultiplier = 20.0f;

        [Header("Debug")]
        [Tooltip("개발 중 이벤트별 점수 계산 로그를 출력한다.")]
        [SerializeField] private bool enableScoreDebugLog = true;

        public int CurrentScore { get; private set; }
        public int BestScore { get; private set; }
        public float Multiplier { get; private set; }

        public event Action<int> OnScoreChanged;
        public event Action<int> OnBestScoreChanged;
        public event Action<float> OnMultiplierChanged;

        private const string BestScorePrefsKey = "PlinkoPinball_BestScore_v1";


        private void Awake()
        {
            BestScore = PlayerPrefs.GetInt(BestScorePrefsKey, 0);
            Multiplier = Mathf.Clamp(baseMultiplier, 0.1f, maxMultiplier);
        }

        private void OnEnable()
        {
            TableEventBus.OnEvent += HandleTableEvent;
        }

        private void OnDisable()
        {
            TableEventBus.OnEvent -= HandleTableEvent;
        }

        /// <summary>
        /// 라운드 시작 시 점수를 초기화합니다.
        /// </summary>
        public void ResetScore()
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        /// <summary>
        /// 배수를 설정합니다. (업그레이드 등에서 사용할 예정)
        /// </summary>
        public void SetMultiplier(float value)
        {
            float clamped = Mathf.Clamp(value, 0.1f, maxMultiplier);
            if (Mathf.Abs(clamped - Multiplier) < 0.0001f) return;

            Multiplier = clamped;
            OnMultiplierChanged?.Invoke(Multiplier);
        }

        /// <summary>
        /// 배수 증감을 적용합니다. (예 : 램프 성공 시 +0.2)
        /// </summary>
        public void AddMultiplier(float value)
        {
            SetMultiplier(Multiplier + value);
        }

        /// <summary>
        /// 라운드 종료 시 최고기록 갱신 판별을 하고 저장합니다.
        /// </summary>
        public void CommitBestScoreIfNeeded()
        {
            if (CurrentScore <= BestScore) return;

            BestScore = CurrentScore;
            PlayerPrefs.SetInt(BestScorePrefsKey, BestScore);
            PlayerPrefs.Save();
            OnBestScoreChanged?.Invoke(BestScore);
        }



        private void HandleTableEvent(TableEvent e)
        {
            if (!ShouldScore(e))
                return;

            // ModuleRoot가 있는 오브젝트를 찾고 캐싱한다. (없으면 찾고, 있으면 가져옴)
            ModuleRoot module = ModuleRootLookupCache.GetOrFind(e.source);

            // TODO: 이 부분에서 모듈별로 어떤 트리거에 배율을 적용할 것인지를 자동적으로 정해야 할 것 같음.
            float moduleMultiplier = module != null ? module.GetScoreMultiplier() : 1f;

            // 실제 점수를 계산하는 로직
            int add = CalculateScoreResult(e.baseValue, Multiplier, moduleMultiplier);
            if (add <= 0)
                return;

            CurrentScore += add;
            OnScoreChanged?.Invoke(CurrentScore);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (enableScoreDebugLog)
            {
                string sourceName = e.source != null ? e.source.name : "null";
                string moduleName = module != null ? module.ModuleId : "None";

                Debug.Log(
                    $"[ScoreSystem] +{add} " +
                    $"(base={e.baseValue}, globalMul={Multiplier:0.00}, moduleMul={moduleMultiplier:0.00}) " +
                    $"from {e.eventId} | source={sourceName} | module={moduleName}");
                Debug.Log($"[ScoreSystem]CurrentScore: {CurrentScore}");
            }
#endif
        }

        /// <summary>
        /// 해당 이벤트가 점수 반영 대상인지 판별합니다.
        /// </summary>
        /// <returns></returns>
        private bool ShouldScore(TableEvent e)
        {
            if (!string.IsNullOrEmpty(scoreTag) && !e.HasTag(scoreTag))
                return false;

            if (e.baseValue <= 0)
                return false;
            
            //Debug.Log("점수 반영 대상입니다.");
            return true;
        }

        /// <summary>
        /// 최종 점수를 계산합니다.
        /// </summary>
        private int CalculateScoreResult(int baseValue, float globalMultiplier, float moduleMultiplier)
        {
            float result = baseValue * globalMultiplier * moduleMultiplier;
            
            return Mathf.RoundToInt(result);
        }
    }
}
