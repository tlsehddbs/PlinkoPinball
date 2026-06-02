using UnityEngine;
using PlinkoPinball.InputRuntime;


namespace PlinkoPinball.Gameplay
{
    /// <summary>
    /// 차지 - 릴리즈 플런저 런처(임펄스/속도 방식)
    /// 
    /// - 누르고 있는 동안 충전
    /// - 떼는 순간 공에게 속도 변화를 줘 발사
    /// - 런처 레인에 공이 있을때만 발사 가능
    /// 
    /// ForceMode.ChangeVelocity를 사용
    /// -> 튜닝에 용이
    /// -> 빠른 개발을 위한 선택(추후 실제 물리 효과를 적용할 수도 있음)
    /// </summary>
    public sealed class PlungerLauncher : MonoBehaviour
    {
        [Header("Direction")]
        [Tooltip("발사 방향 기준 트랜스폼. 비워둘 경우 오브젝트의 forward를 사용")]
        [SerializeField] private Transform launchDirection;

        [Header("Charge")]
        [Tooltip("풀 차지까지 걸리는 시간(초)")]
        [SerializeField] private float maxChargeSeconds = 1.0f;

        [Tooltip("차지 최소/최대 속도 변화")]
        [SerializeField] private float minVelocityChange = 6f;
        [SerializeField] private float maxVelocityChange = 18f;

        [Tooltip("릴리즈 직후 재차지/발사 방지룔 쿨다운(초)")]
        [SerializeField] private float releaseCooldown = 0.1f;

        [Header("Tuning")]
        [Tooltip("발사 시 기존 속도의 발사방향 성분을 제거하여 이상 튕김을 방지")]
        [SerializeField] private bool clearForwardVelocityDeforeLaunch = true;

        private Rigidbody _ballRb;

        private float _charge01;
        private bool _isCharging;
        private float _cooldownTimer;

        public bool IsCharging => _isCharging;
        public float Charge01 => _charge01;


        private void OnEnable()
        {
            if (InputRouter.Instance != null)
            {
                InputRouter.Instance.OnLaunchPressed += BeginCharge;
                InputRouter.Instance.OnLaunchReleased += Release;
            }
        }

        private void OnDisable()
        {
            if (InputRouter.Instance != null)
            {
                InputRouter.Instance.OnLaunchPressed -= BeginCharge;
                InputRouter.Instance.OnLaunchReleased -= Release;
            }
        }

        private void Update()
        {
            if (_cooldownTimer > 0f) 
            {
                _cooldownTimer -= Time.deltaTime;
            }

            if (!_isCharging) 
            {
                return;
            }

            // charge01을 maxchargeSeconds 동안 1까지 증가
            float chargeRate = (maxChargeSeconds <= 0f) ? 1f : (Time.deltaTime / maxChargeSeconds);
            _charge01 = Mathf.Clamp01(_charge01 + chargeRate);
        }

        /// <summary>
        /// 런치 레인 센서가 공을 감지했을 때 호출
        /// </summary>
        public void SetBall(Rigidbody ballRb)
        {
            _ballRb = ballRb;
        }

        public bool HasBall(Rigidbody rb) => _ballRb == rb;

        public void ClearBall()
        {
            _ballRb = null;
            _isCharging = false;
            _charge01 = 0f;
        }

        /// <summary>
        /// 입력 press 시 호출
        /// </summary>
        public void BeginCharge()
        {
            //Debug.Log("begin launcher");
            if (_cooldownTimer > 0f) 
            {
                return;
            }

            if (_ballRb == null) 
            {
                return;     // 레인에 공이 없으면 차지 x
            }

            _isCharging = true;
            _charge01 = 0f;
        }

        public void Release()
        {
            //Debug.Log("Release launcher");
            if (!_isCharging) 
            {
                return;
            }

            _isCharging = false;

            if (_ballRb == null) 
            {
                return;
            }

            float dv = Mathf.Lerp(minVelocityChange, maxVelocityChange, _charge01);
            Vector3 dir = (launchDirection != null) ? launchDirection.forward : transform.forward;
            dir.Normalize();

            if (clearForwardVelocityDeforeLaunch)
            {
                // 발사 방향으로 이미 속도가 있는 경우(레인 안에서 튕김 등), forward 성분만 제거해 발사 결과를 안정화
                float forwardSpeed = Vector3.Dot(_ballRb.linearVelocity, dir);
                if (forwardSpeed > 0f)
                {
                    _ballRb.linearVelocity -= dir * forwardSpeed;
                }
            }

            // 속도 변화를 직접 적용 -> 질량과 무관하게 일정한 발사 느낌을 줌
            _ballRb.AddForce(dir * dv, ForceMode.VelocityChange);

            _cooldownTimer = releaseCooldown;
            _charge01 = 0f;
        }
    }
}
