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


        [Header("Debug")]
        [SerializeField] private bool debugDrawSpawnPoints = true;
        [SerializeField] private float debugSphereRadius = 0.05f;

        private readonly List<Vector3> _debugCandidates = new();
        private readonly List<bool> _debugResults = new(); // true = success, false = rejected
        private Vector3 _debugFinalPosition;
        private bool _debugUsedFallback;
        

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
            _debugCandidates.Clear();
            _debugResults.Clear();
            _debugUsedFallback = false;

            Vector3 fallback = GetRandomPointInArea();

            for (int i = 0; i < maxPositionAttempts; i++)
            {
                Vector3 candidate = GetRandomPointInArea();

                bool valid = IsFarEnoughFromRecentSpawns(candidate);

                _debugCandidates.Add(candidate);
                _debugResults.Add(valid);

                if (valid)
                {
                    _debugFinalPosition = candidate;
                    return candidate;
                }

                fallback = candidate;
            }

            _debugUsedFallback = true;
            _debugFinalPosition = fallback;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_debugUsedFallback)
            {
                Debug.LogWarning("[PlinkoBallSpawner] Fallback used - spawn area too dense", this);
            }
#endif
            return fallback;
        }

        private Vector3 GetRandomPointInArea()
        {
            Vector3 center = spawnAnchor != null ? spawnAnchor.position : transform.position;

            return center + transform.TransformVector(new Vector3(
                Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f),
                0f
            ));
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

#if UNITY_EDITOR
        // Unity Scene Window 에서 작동함
        private void OnDrawGizmos()
        {
            if (!debugDrawSpawnPoints)
            {
                return;
            }

            // Spawn Area
            var areaCenterOffset = spawnAnchor.position;
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(areaCenterOffset, areaSize);

            Gizmos.matrix = Matrix4x4.identity;

            // 후보 위치들
            for (int i = 0; i < _debugCandidates.Count; i++)
            {
                Gizmos.color = _debugResults[i] ? Color.green : Color.red;
                Gizmos.DrawSphere(_debugCandidates[i], debugSphereRadius);
            }

            // 최종 선택 위치
            Gizmos.color = _debugUsedFallback ? Color.magenta : Color.yellow;
            Gizmos.DrawWireSphere(_debugFinalPosition, debugSphereRadius * 2f);
        }
#endif
    }
}
