using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 보드에서 드랍되는 단일 볼의 런타임 상태를 관리
    /// </summary>
    public sealed class PlinkoBallActor : MonoBehaviour
    {
        private PlinkoRunController _runController;
        private bool _isResolved;

        /// <summary>
        /// 볼을 현재 플링코 런에 바인딩
        /// </summary>
        public void Initialize(PlinkoRunController runController)
        {
            _runController = runController;
            _isResolved = false;
        }

        /// <summary>
        /// 볼을 해결 상태로 전환
        /// 슬롯 도착, 타임아웃, 보드 이탈 등 모든 종료 경로는 이 메서드로 모음
        /// </summary>
        public void Resolve()
        {
            if (_isResolved)
            {
                return;
            }

            _isResolved = true;

            _runController?.OnBallResolved(this);

            Destroy(gameObject);
        }
    }
}