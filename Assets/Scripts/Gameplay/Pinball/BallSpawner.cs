using UnityEngine;

namespace PlinkoPinball.Gameplay
{
    /// <summary>
    /// 공 생성/재생성 담당
    /// </summary>
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