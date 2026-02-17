using UnityEngine;
using UnityEngine.InputSystem;
using PlinkoPinball.Core;
using PlinkoPinball.Input;
using PlinkoPinball.Gameplay;

namespace PlinkoPinball.InputRuntime
{
    /// <summary>
    /// Input System의 액션을 게임 로직(GameManager)에 연결하는 라우터
    /// 
    /// - 바인딩/디바이스는 InputActions 에셋에서 관리
    /// - 코드는 "어떤 액션이 발생했는지"만 처리(플랫폼 별 독립적)
    /// - 모바일 UI 버튼도 같은 액션을 호출하도록 확장성을 겸비
    /// </summary>
    public sealed class InputRouter : MonoBehaviour
    {
        [Header("Enable Input")]
        [SerializeField] private bool enableGameplayActions = true;

        private PlatformControls _controls;
        private GameManager _gameManager;

        // 이벤트 핸들러를 필드로 분리(구독 해제를 가능하도록)
        private System.Action<InputAction.CallbackContext> _onStart;
        private System.Action<InputAction.CallbackContext> _onPause;
        private System.Action<InputAction.CallbackContext> _onRestart;

        [SerializeField] private PlinkoPinball.Gameplay.PlungerLauncher launcher;
        private System.Action<InputAction.CallbackContext> _onLaunchPerformed;
        private System.Action<InputAction.CallbackContext> _onLaunchCanceled;


        // 추후 Awake로 변경 예정
        private void Awake()
        {
            _gameManager = GameManager.Instance;

            // Generated input actions class
            _controls = new PlatformControls();

            _onStart = ctx => HandleStartRound();
            _onPause = ctx => HandlePause();
            _onRestart = ctx => HandleRestart();

            _onLaunchPerformed = ctx => { if (launcher != null) launcher.BeginCharge(); };
            _onLaunchCanceled = ctx => { if (launcher != null) launcher.Release(); };
        }

        private void OnEnable()
        {
            if (!enableGameplayActions) return;

            // 이벤트 구독 
            _controls.GamePlay.StartRound.performed += _onStart;
            _controls.GamePlay.Pause.performed += _onPause;
            _controls.GamePlay.Restart.performed += _onRestart;

            _controls.GamePlay.Launch.performed += _onLaunchPerformed;
            _controls.GamePlay.Launch.canceled += _onLaunchCanceled;

            _controls.GamePlay.Enable();
        }

        private void OnDisable()
        {
            if (_controls == null) return;

            // 이벤트 구독 해제
            _controls.GamePlay.StartRound.performed -= _onStart;
            _controls.GamePlay.Pause.performed -= _onPause;
            _controls.GamePlay.Restart.performed -= _onRestart;

            _controls.GamePlay.Launch.performed -= _onLaunchPerformed;
            _controls.GamePlay.Launch.canceled -= _onLaunchCanceled;

            _controls.GamePlay.Disable();
        }


        private void HandleStartRound()
        {
            Debug.Log($"[Input] StartRound performed. GameState={_gameManager?.State}");
            if (_gameManager == null) return;

            // MainMenu / RoundEnded 상태에서 시작 가능하도록
            if (_gameManager.State == GameState.MainMenu || _gameManager.State == GameState.RoundEnded)
                _gameManager.StartRound();
        }

        private void HandlePause()
        {
            if (_gameManager == null) return;

            if (_gameManager.State == GameState.InRound) _gameManager.SetPaused(true);
            else if (_gameManager.State == GameState.Paused) _gameManager.SetPaused(false);
        }

        private void HandleRestart()
        {
            if (_gameManager == null) return;

            if (_gameManager.State == GameState.RoundEnded)
                _gameManager.StartRound();
        }
    }
}

