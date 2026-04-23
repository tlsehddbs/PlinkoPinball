using UnityEngine;
using UnityEngine.InputSystem;
using PlinkoPinball.Core;
using PlinkoPinball.Input;
using PlinkoPinball.Gameplay.Core.Flow;
using PlinkoPinball.Gameplay;

namespace PlinkoPinball.InputRuntime
{
    /// <summary>
    /// Input System 액션을 현재 phase에 맞는 게임 로직으로 라우팅합니다.
    /// </summary>
    public sealed class InputRouter : MonoBehaviour
    {
        public static InputRouter Instance { get; private set; }

        [Header("Enable Input")]
        [SerializeField] private bool enableGameplayActions = true;

        private PlatformControls _controls;
        private GameManager _gameManager;

        private System.Action<InputAction.CallbackContext> _onStart;
        private System.Action<InputAction.CallbackContext> _onPause;
        private System.Action<InputAction.CallbackContext> _onRestart;
        private System.Action<InputAction.CallbackContext> _onLaunchPerformed;
        private System.Action<InputAction.CallbackContext> _onLaunchCanceled;

        public event System.Action OnLaunchPressed;
        public event System.Action OnLaunchReleased;

        private System.Action<InputAction.CallbackContext> _onReturnToPinball;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            //_gameManager = GameManager.Instance;
            _controls = new PlatformControls();

            _onStart = _ => HandleStartRound();
            _onPause = _ => HandlePause();
            _onRestart = _ => HandleRestart();

            _onLaunchPerformed = _ => HandleLaunchPressed();
            _onLaunchCanceled = _ => HandleLaunchReleased();

            _onReturnToPinball = _ => { if (_gameManager.Phase == GamePhase.Plinko) _gameManager.CompletePlinkoAndReturnToPinball(); };
        }

        private void Start()
        {
            _gameManager = GameManager.Instance;
        }

        private void OnEnable()
        {
            if (!enableGameplayActions)
            {
                return;
            }

            _controls.GamePlay.StartRound.performed += _onStart;
            _controls.GamePlay.Pause.performed += _onPause;
            _controls.GamePlay.Restart.performed += _onRestart;
            _controls.GamePlay.Launch.performed += _onLaunchPerformed;
            _controls.GamePlay.Launch.canceled += _onLaunchCanceled;

            _controls.Plinko.Return.performed += _onReturnToPinball;

            _controls.UI.Enable();
            _controls.GamePlay.Enable();
            _controls.Plinko.Enable();
        }

        private void OnDisable()
        {
            if (_controls == null)
            {
                return;
            }

            _controls.GamePlay.StartRound.performed -= _onStart;
            _controls.GamePlay.Pause.performed -= _onPause;
            _controls.GamePlay.Restart.performed -= _onRestart;
            _controls.GamePlay.Launch.performed -= _onLaunchPerformed;
            _controls.GamePlay.Launch.canceled -= _onLaunchCanceled;

            _controls.Plinko.Return.performed -= _onReturnToPinball;

            _controls.UI.Disable();
            _controls.GamePlay.Disable();
            _controls.Plinko.Disable();
        }

        public void ApplyPhase(GamePhase phase)
        {
            if (_controls == null)
            {
                return;
            }
            // _controls.UI.Disable();
            // _controls.GamePlay.Disable();
            // _controls.Plinko.Disable();

            switch (phase)
            {
                case GamePhase.MainMenu:
                    _controls.UI.Enable();
                    break;

                case GamePhase.Pinball:
                    _controls.GamePlay.Enable();
                    Debug.Log($"[InputRouter] 현재 Phase={_gameManager.Phase}");
                    break;

                case GamePhase.Plinko:
                    _controls.Plinko.Enable();
                    Debug.Log($"[InputRouter] 현재 Phase={_gameManager.Phase}");
                    break;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[InputRouter] ApplyPhase => {phase}", this);
#endif
        }

        // TODO: 이 부분은 제거하는 걸로 (별도 실행 버튼을 둘 이유가 없음)
        private void HandleStartRound()
        {
            if (_gameManager == null)
            {
                return;
            }

            if (_gameManager.Phase == GamePhase.MainMenu)
            {
                _gameManager.StartNewSession();
            }
        }

        private void HandlePause()
        {
            if (_gameManager == null)
            {
                return;
            }

            if (_gameManager.Phase == GamePhase.Pinball)
            {
                _gameManager.SetPaused(!_gameManager.IsPaused);
            }
        }

        private void HandleRestart()
        {
            if (_gameManager == null)
            {
                return;
            }

            if (_gameManager.Phase == GamePhase.MainMenu)
            {
                _gameManager.StartNewSession();
                return;
            }

            if (_gameManager.Phase == GamePhase.Pinball)
            {
                _gameManager.StartPinballRound();
            }
        }

        private void HandleLaunchPressed()
        {
            Debug.Log($"[InputRouter] HandleLaunchPressed 호출됨. {_gameManager} / {_gameManager.Phase} / {_gameManager.IsPaused}");
            if (_gameManager == null || _gameManager.Phase != GamePhase.Pinball || _gameManager.IsPaused)
            {
                return;
            }
            OnLaunchPressed?.Invoke();
        }

        private void HandleLaunchReleased()
        {
            if (_gameManager == null || _gameManager.Phase != GamePhase.Pinball || _gameManager.IsPaused)
            {
                return;
            }
            OnLaunchReleased?.Invoke();
        }
    }
}