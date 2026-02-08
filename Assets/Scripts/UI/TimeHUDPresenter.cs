using UnityEngine;
using TMPro;
using PlinkoPinball.Core;

namespace PlinkoPinball.UI
{
    /// <summary>
    /// Time HUD 표시 전용 프리젠터
    /// 
    /// - Timer.OnTimeChanged 이벤트로 UI 갱신
    /// 
    /// ! UI 비용 감소
    /// ! Game 로직과 UI 결합도 최소
    /// ! 모바일에서도 안정적인 프레임 유지에 유리하도록
    /// </summary>
    public class TimeHUDPresenter : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text timeText;

        // GameManager null 방지를 위해 OnEnable 시점에 다시 확보
        private GameManager _gameManager;

        private void Awake()
        {
            if (timeText == null)
                Debug.Log("[TimeHUDPresenter] timeText is not assigned");
        }

        //TODO: GameManager의 이벤트를 구독할 때 활성화? 순서에 따른 문제로 Start에 임시로 만들어 둠. 추후에 씬이 확장될 때 OnEnable 함수로 변경할 것.
        private void Start()
        {
            _gameManager = GameManager.Instance;
            if (_gameManager == null)
            {
                Debug.Log("[TimeHUDPresenter] GameManager.Instance is null");
                return;
            }

            // Time 이벤트 구독
            _gameManager.Time.OnTimeChanged += HandleTimeChanged;

            // 구독 후 1회 즉시 반영(씬 진입 시 Time 표시 정상을 위함)
            HandleTimeChanged(_gameManager.Time.RemainingSeconds);
        }

        // private void OnEnable()
        // {
        //     _gameManager = GameManager.Instance;
        //     if (_gameManager == null)
        //     {
        //         Debug.Log("[TimeHUDPresenter] GameManager.Instance is null");
        //         return;
        //     }

        //     // Time 이벤트 구독
        //     _gameManager.Time.OnTimeChanged += HandleTimeChanged;

        //     // 구독 후 1회 즉시 반영(씬 진입 시 Time 표시 정상을 위함)
        //     HandleTimeChanged(_gameManager.Time.RemainingSeconds);
        // }

        private void OnDisable()
        {
            if (_gameManager == null) return;
            if (_gameManager.Time != null)
                _gameManager.Time.OnTimeChanged -= HandleTimeChanged;
        }

        private void HandleTimeChanged(float remainingSeconds)
        {
            if (timeText == null) return;

            // 0.1초 단위로 갱신 (notifyIntervalSeconds=0.1 과 동일)
            timeText.text = $"Time {remainingSeconds:0.0}s";
        }
    }
}