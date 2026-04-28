using System.Collections.Generic;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 지정된 영역 내에서 플링코 볼을 랜덤 생성
    /// </summary>
    public sealed class PlinkoBallSpawner : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private PlinkoBallActor ballPrefab;

        [Header("Spawn Area")]
        [SerializeField] private Transform spawnAnchor;
        [SerializeField] private Vector3 areaSize = new Vector3(6f, 0f, 0f);
        [SerializeField, Min(0f)] private float minSpawnDistance = 0.25f;
        [SerializeField, Min(1)] private int maxPositionAttempts = 20;

        [Header("Spawn Rotation")]
        [SerializeField] private bool useSpawnerRotation = true;

        private readonly List<Vector3> _recentSpawnPositions = new();
        [SerializeField, Min(0)] private int RecentPositionLimit = 32;

        /// <summary>
        /// 새 볼을 생성하고 런 컨트롤러에 바인딩
        /// </summary>
        public PlinkoBallActor SpawnBall(PlinkoRunController runController)
        {
            if (ballPrefab == null)
            {
                return null;
            }

            Vector3 position = FindSpawnPosition();
            Quaternion rotation = useSpawnerRotation ? transform.rotation : Quaternion.identity;

            PlinkoBallActor actor = Instantiate(ballPrefab, position, rotation);
            actor.Initialize(runController);

            RegisterSpawnPosition(position);

            return actor;
        }

        private Vector3 FindSpawnPosition()
        {
            Vector3 fallback = transform.TransformPoint(spawnAnchor.position);

            for (int i = 0; i < maxPositionAttempts; i++)
            {
                Vector3 candidate = GetRandomPointInArea();

                if (IsFarEnoughFromRecentSpawns(candidate))
                {
                    return candidate;
                }
            }

            return fallback;
        }

        private Vector3 GetRandomPointInArea()
        {
            Vector3 localOffset = spawnAnchor.position + new Vector3(
                Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f),
                0f
            );

            return transform.TransformPoint(localOffset);
        }

        private bool IsFarEnoughFromRecentSpawns(Vector3 candidate)
        {
            float minSqrDistance = minSpawnDistance * minSpawnDistance;

            for (int i = 0; i < _recentSpawnPositions.Count; i++)
            {
                if ((candidate - _recentSpawnPositions[i]).sqrMagnitude < minSqrDistance)
                {
                    return false;
                }
            }

            return true;
        }

        private void RegisterSpawnPosition(Vector3 position)
        {
            _recentSpawnPositions.Add(position);

            if (_recentSpawnPositions.Count > RecentPositionLimit)
            {
                _recentSpawnPositions.RemoveAt(0);
            }
        }
    }
}
