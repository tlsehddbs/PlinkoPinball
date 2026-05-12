using UnityEngine;
using System;
using System.Collections.Generic;

using PlinkoPinball.Services;
using PlinkoPinball.Core.Flow;
using PlinkoPinball.Gameplay.Core.Flow;
using PlinkoPinball.InputRuntime;

namespace PlinkoPinball.Core
{
    /// <summary>
    /// 게임 전체 전역 상태와 상위 흐름을 관리합니다.
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Round Defaults")]
        [SerializeField] private float startTimeSeconds = 20f;
        [SerializeField] private float timeChangedNotifyIntervalSeconds = 0.1f;

        [Header("Global Services")]
        [SerializeField] private GameSceneManager gameSceneManager;
        [SerializeField] private GameSessionState sessionState;
        [SerializeField] private InputRouter inputRouter;

        public GamePhase Phase { get; private set; } = GamePhase.None;
        public TimeManager Time { get; private set; }
        public GameSessionState SessionState => sessionState;
        public InputRouter InputRouter => inputRouter;

        public event Action<GamePhase, GamePhase> OnPhaseChanged;

        private readonly List<IRoundResettable> _roundResettables = new List<IRoundResettable>();

        private ScoreSystem _scoreSystem;
        private PinballToPlinkoTransitionController _transitionController;
        private bool _roundEnded;
        private bool _isPaused;
        

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Time = new TimeManager(notifyIntervalSeconds: timeChangedNotifyIntervalSeconds);
            Time.OnTimeOver += HandleTimeOver;

            // 현재 씬에 맞는 페이즈로 자동 설정되게 하는 거는 어떨까?
            //SetPhase(GamePhase.MainMenu);
        }

        private void Update()
        {
            if (Phase == GamePhase.Pinball && !_isPaused)
            {
                Time.Tick(UnityEngine.Time.deltaTime);
            }
        }

        public void SetPhase(GamePhase newPhase)
        {
            if (Phase == newPhase)
            {
                return;
            }

            GamePhase previous = Phase;
            Phase = newPhase;
            OnPhaseChanged?.Invoke(previous, newPhase);

            inputRouter.ApplyPhase(newPhase);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[GameManager] Phase => {newPhase}", this);
#endif
        }

        public void StartNewSession()
        {
            sessionState?.ResetSession();
            gameSceneManager?.LoadMainTable();
        }

        public void ReturnToMainMenu()
        {
            gameSceneManager?.LoadMainMenu();
        }

        public void RegisterPinballScene(ScoreSystem scoreSystem, PinballToPlinkoTransitionController transitionController, IEnumerable<IRoundResettable> roundResettables)
        {
            _scoreSystem = scoreSystem;
            _transitionController = transitionController;

            _roundResettables.Clear();

            if (roundResettables != null)
            {
                foreach (IRoundResettable resettable in roundResettables)
                {
                    if (resettable != null)
                    {
                        _roundResettables.Add(resettable);
                    }
                }
            }
        }

        public void StartPinballRound()
        {
            Debug.Log("[GameManager] StartPinballRound 호출됨.");
            _roundEnded = false;
            _isPaused = false;
            SetPhase(GamePhase.Pinball);

            for (int i = 0; i < _roundResettables.Count; i++)
            {
                _roundResettables[i].ResetForRound();
            }

            Time.Start(startTimeSeconds);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[GameManager] Round {sessionState?.CurrentRoundIndex ?? 1} started.", this);
#endif
        }

        // TODO: Pause 될 떼 물리 tick 도 멈추게 변경하는것도?
        public void SetPaused(bool paused)
        {
            if (Phase != GamePhase.Pinball)
            {
                return;
            }

            _isPaused = paused;
            Time.SetPaused(paused);
        }

        public bool IsPaused => _isPaused;

        public void EndPinballRoundAndTransitionToPlinko()
        {
            if (_roundEnded)
            {
                return;
            }

            _roundEnded = true;
            _isPaused = false;
            Time.Stop();

            int roundIndex = sessionState != null ? sessionState.CurrentRoundIndex : 1;
            int currentScore = _scoreSystem != null ? _scoreSystem.CurrentScore : 0;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[GameManager] Ending round {roundIndex}. Score={currentScore}", this);
#endif

            _transitionController?.TransitionToPlinko(roundIndex, currentScore);
        }

        public void CompletePlinkoAndReturnToPinball()
        {
            sessionState?.AdvanceRound();
            gameSceneManager?.LoadMainTable();
            _roundEnded = false;
        }

        private void HandleTimeOver()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[GameManager] Time Over", this);
#endif
            EndPinballRoundAndTransitionToPlinko();
        }
    }
}