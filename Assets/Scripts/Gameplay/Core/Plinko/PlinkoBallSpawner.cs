using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 페이즈 내 볼을 생성
    /// </summary>
    public sealed class PlinkoBallSpawner : MonoBehaviour
    {
        [SerializeField] private PlinkoBallActor ballPrefab;
        [SerializeField] private Transform spawnPoint;

        /// <summary>
        /// 새 볼을 생성하고 런 컨트롤러에 바인딩
        /// </summary>
        public PlinkoBallActor SpawnBall(PlinkoRunController runController)
        {
            if (ballPrefab == null || spawnPoint == null)
            {
                return null;
            }

            PlinkoBallActor actor = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
            actor.Initialize(runController);
            return actor;
        }
    }
}