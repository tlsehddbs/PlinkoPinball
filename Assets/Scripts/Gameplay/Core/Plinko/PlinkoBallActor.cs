using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 볼의 생명주기를 관리
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlinkoBallActor : MonoBehaviour
    {
        private PlinkoRunController _runController;
        private bool _resolved;

        /// <summary>
        /// 현재 볼을 런 컨트롤러에 바인딩합니다.
        /// </summary>
        public void Initialize(PlinkoRunController runController)
        {
            _runController = runController;
        }

        /// <summary>
        /// 이 볼을 해결 완료 상태로 처리하고 제거
        /// </summary>
        public void Resolve()
        {
            if (_resolved)
            {
                return;
            }

            _resolved = true;

            if (_runController != null)
            {
                _runController.OnBallResolved(this);
            }

            Destroy(gameObject);
        }
    }
}