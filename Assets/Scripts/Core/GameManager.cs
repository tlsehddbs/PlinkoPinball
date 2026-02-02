using UnityEngine;
using PlinkoPinball.Services;

namespace PlinkoPinball.Core
{
    /// <summary>
    /// 게임 전체의 최상위 관리자
    /// 
    /// - 게임 상태를 단일 진실(SSOT)로 관리
    /// - 주요 서비스(Timer/Score/Economy/Event 등)을 생성/초기화하고 구동
    /// - 상태 전환(라운드 시작/종료/일시정지)를 중앙에서 통제
    /// 
    /// 단,
    /// - 모든 로직을 여기에 몰아넣지 않는다
    /// - GameManager는 흐름만 잡고, 규칙/계산은 서비스로 내려보낸다
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Round Defaults")]
        [Tooltip("라운드 시작 시 기본으로 제공되는 시간")]
        [SerializeField] private float startTimeSeconds = 60f;

        [Tooltip("TimeChanged 이벤트를 n 초 간격으로만 통지하려면 설정(0일 경우 매 변경시 통지)")]
        [SerializeField] private float timeChangedNotifyIntervalSeconds = 0.1f;

        public GameState State { get; private set; } = GameState.Boot;

        public Timer Time { get; private set; }

        public event System.Action<GameState, GameState> OnStateChanged;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);

                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Bootstrap();
        }

        private void Bootstrap()
        {
            // Service 생성
            Time = new Timer(notifyIntervalSeconds: timeChangedNotifyIntervalSeconds);
            Time.OnTimeOver += HandleTimeOver;

            SetState(GameState.MainMenu);
        }

        private void Update()
        {
            // InRun 상태일 때만 타이머가 작동하도록 함.
            if (State == GameState.InRound)
            {
                Time.Tick(UnityEngine.Time.deltaTime);
            }
        }

        public void StartRound()
        {
            if (State == GameState.InRound) return;

            SetState(GameState.InRound);
            Time.Start(startTimeSeconds);

            Debug.Log($"[GameManager] Round Start - {startTimeSeconds:0.##}s");
        }

        public void EndRound()
        {
            if (State != GameState.InRound && State != GameState.Paused) return;

            Time.Stop();
            SetState(GameState.RoundEnded);

            Debug.Log("[GameManager] Round End");
        }

        public void SetPaused(bool paused)
        {
            if (paused)
            {
                if (State != GameState.InRound) return;

                SetState(GameState.Paused);
                Time.SetPaused(true);
            }
            else
            {
                if (State != GameState.Paused) return;

                SetState(GameState.InRound);
                Time.SetPaused(false);
            }
        }

        /// <summary>
        /// 타이머가 0이 되면 호출되는 콜백
        /// </summary>
        private void HandleTimeOver()
        {
            //TODO: 정산/기록 저장/결과 등으로 연결
            Debug.Log("[GameManager] Time Over");
            EndRound();
        }


        private void SetState(GameState newState)
        {
            if (State == newState) return;
            
            var prev = State;
            State = newState;

            // NOTE:
            // 상태 변경은 게임 흐름의 핵심
            // UI/오디오/연출은 이 이벤트를 구독해서 결합도를 낮춤
            OnStateChanged?.Invoke(prev, newState);
        }

#if UNITY_EDITOR
        // 개발 편의: 에디터에서 바로 런 시작 테스트
        [ContextMenu("DEV/Start Run")]
        private void DevStartRun() => StartRound();
#endif
    }
}
