using UnityEngine;

namespace PlinkoPinball.Gameplay.Utility
{
    public static class BallRigidbodyUnility
    {
        // Collider로부터 공의 rb를 찾음
        public static bool TryGetBallRigidbody(Collider other, out Rigidbody ball)
        {
            ball = other.attachedRigidbody;
            if (ball == null) return false;

            //TODO: ball tag/layer/component 등도 여기서 확인

            return true;
        }
    }
}
