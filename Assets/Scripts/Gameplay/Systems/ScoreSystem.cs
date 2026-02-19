using System;
using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using Unity.VisualScripting;

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
        [Min(0.1f)][SerializeField] private float baseMultiplier = 1.0f;   // 기본 배수
        [Min(1.0f)][SerializeField] private float maxMultiplier = 20.0f;

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
        /// 라운드 시작 시 점수 초기화.
        /// </summary>
        public void ResetScore()
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        /// <summary>
        /// 배수 설정(업그레이드 등에서 사용할 예정)
        /// </summary>
        public void SetMultiplier(float value)
        {
            float clamped = Mathf.Clamp(value, 0.1f, maxMultiplier);
            if (Mathf.Abs(clamped - Multiplier) < 0.0001f) return;

            Multiplier = clamped;
            OnMultiplierChanged?.Invoke(Multiplier);
        }

        /// <summary>
        /// 배수 증감 (예 : 램프 성공 시 +0.2)
        /// </summary>
        public void AddMultiplier(float value)
        {
            SetMultiplier(Multiplier + value);
        }

        /// <summary>
        /// 라운드 종료 시 최고기록 갱신 판별 및 저장
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
            // score Tag가 없으면 점수 처리하지 않음
            if (!e.HasTag(scoreTag)) return;

            // baseValue가 0 이하이면 점수 없음
            if (e.baseValue <= 0) return;

            // 점수 계산
            int add = Mathf.RoundToInt(e.baseValue * Multiplier);
            if (add <= 0) return;

            CurrentScore += add;
            OnScoreChanged?.Invoke(CurrentScore);

            Debug.Log($"[ScoreSystem] +{add} (base={e.baseValue}, mul={Multiplier:0.00}) from {e.eventId}");
        }
    }
}
