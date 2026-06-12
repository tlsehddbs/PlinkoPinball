using UnityEngine;

namespace PlinkoPinball.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public sealed class PinballResetTrigger : MonoBehaviour
    {
        [SerializeField] private BallSpawner ballSpawner;
        [SerializeField] private bool requirePinballBallTuner = true;

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            if (trigger != null)
            {
                trigger.isTrigger = true;
            }
        }

        private void Awake()
        {
            if (ballSpawner == null)
            {
                ballSpawner = FindFirstObjectByType<BallSpawner>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody ball = other.attachedRigidbody;
            if (ball == null)
            {
                return;
            }

            if (requirePinballBallTuner && ball.GetComponent<PinballBallPhysicsTuner>() == null)
            {
                return;
            }

            if (ballSpawner == null)
            {
                ballSpawner = FindFirstObjectByType<BallSpawner>();
            }

            ballSpawner?.ResetBallToSpawn(ball);
        }
    }
}
