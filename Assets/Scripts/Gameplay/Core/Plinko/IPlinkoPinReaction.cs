using UnityEngine;

namespace PlinkoPinball.Gameplay.Components.Plinko
{
    /// <summary>
    /// 플링코 핀 충돌 시 같은 핀 오브젝트에서 실행되는 로컬 반응 인터페이스
    /// </summary>
    public interface IPlinkoPinReaction
    {
        /// <summary>
        /// 볼이 핀에 충돌했을 때 호출
        /// </summary>
        /// <param name="ball">충돌한 볼의 Rigidbody</param>
        /// <param name="hitPoint">충돌 위치</param>
        void OnPinHit(Rigidbody ball, Vector3 hitPoint);
    }
}