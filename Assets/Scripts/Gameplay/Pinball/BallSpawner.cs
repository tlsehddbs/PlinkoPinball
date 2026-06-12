using UnityEngine;

namespace PlinkoPinball.Gameplay
{
    public sealed class BallSpawner : MonoBehaviour
    {
        [Header("Ball")]
        [SerializeField] private Rigidbody ballPrefab;
        [SerializeField] private Transform spawnPoint;

        public Rigidbody CurrentBall { get; private set; }

        public Rigidbody Spawn()
        {
            if (ballPrefab == null || spawnPoint == null)
            {
                Debug.LogError("[BallSpawner] Prefab or SpawnPoint is null");
                return null;
            }

            if (CurrentBall != null)
            {
                Destroy(CurrentBall.gameObject);
            }

            CurrentBall = Instantiate(ballPrefab, spawnPoint.position, spawnPoint.rotation);
            return CurrentBall;
        }

        public void ResetBallToSpawn(Rigidbody ball)
        {
            if (ball == null || spawnPoint == null)
            {
                return;
            }

            CurrentBall = ball;
            ball.linearVelocity = Vector3.zero;
            ball.angularVelocity = Vector3.zero;
            ball.position = spawnPoint.position;
            ball.rotation = spawnPoint.rotation;
            ball.Sleep();
            ball.WakeUp();
        }

        public void DespawnCurrent()
        {
            if (CurrentBall == null)
            {
                return;
            }

            Destroy(CurrentBall.gameObject);
            CurrentBall = null;
        }
    }
}
