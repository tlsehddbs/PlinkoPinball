using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlinkoPinball.Core;

namespace PlinkoPinball.UI
{
    /// <summary>
    /// TimeManager 이벤트를 구독해 Timerbar를 갱신
    /// </summary>    
    public sealed class TimerBar : MonoBehaviour
    {
        [Header("Bar UI")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;

        [Header("Behavior")]
        [Tooltip("라운드 시작 시간(기준값). StartRound 시점에 갱신되도록 나중에 개선 가능.")]
        [SerializeField] private float referenceStartSeconds = 60f;

        [Tooltip("위험 구간(0~1. ex) 0.2(20%)이하일 때 임박 상태 표시 등")]
        [Range(0f, 1f)]
        [SerializeField] private float criticalTimeThresholdNormalized = 0.2f;
        private bool _isCriticalTime;

        private GameManager _gameManager;


        private void OnEnable()
        {
            _gameManager = GameManager.Instance;

            if (_gameManager == null || _gameManager.Time == null)
            {
                return;
            }

            _gameManager.Time.OnTimeChanged += HandleTimeChanged;

            // 초기 갱신
            HandleTimeChanged(_gameManager.Time.RemainingSeconds);
        }

        private void OnDisable()
        {
            if (_gameManager != null && _gameManager.Time != null)
                _gameManager.Time.OnTimeChanged -= HandleTimeChanged;
        }

        private void HandleTimeChanged(float remainingSeconds)
        {
            // fill 계산
            float denom = Mathf.Max(0.001f, referenceStartSeconds);
            float normalized = Mathf.Clamp01(remainingSeconds / denom);

            if (fillImage != null)
                fillImage.fillAmount = normalized;

            //임박 구간 연출은 추후에 (훅만 달아두고)
            // normalized 는 추후에 사운드/진동/펄스 같은 연출을 붙일 때 유용하게 사용할 수 있음
            // 


            bool nowCritical = normalized <= criticalTimeThresholdNormalized;

            if (nowCritical != _isCriticalTime)
            {
                _isCriticalTime = nowCritical;

                if (_isCriticalTime)
                    OnCriticalTimeEntered();    // 진입 시 1회만 호출(호출 스팸 방지)
                else
                    OnCriticalTimeExited();
            }
        }

        /// <summary>
        /// 시간이 임박했을 때 호출
        /// - 펄스 이펙트, 사운드, 화명 효과 등
        /// </summary>
        private void OnCriticalTimeEntered()
        {
            //TODO: 연출 연결
        }

        /// <summary>
        /// 시간 임박 상태에서 벗어났을 때(시간 추가 등의 상황) 호출
        /// </summary>
        private void OnCriticalTimeExited()
        {
            //TODO: 연출 해제
        }
    }
}
