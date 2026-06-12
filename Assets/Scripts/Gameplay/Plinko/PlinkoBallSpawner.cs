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

        [Header("Pooling")]
        [SerializeField, Min(0)] private int prewarmCount = 16;
        [SerializeField, Min(1)] private int maxPoolSize = 128;
        [SerializeField] private Transform poolRoot;

        private readonly List<Vector3> _recentSpawnPositions = new();
        private readonly Queue<PlinkoBallActor> _pool = new();
        [SerializeField, Min(0)] private int RecentPositionLimit = 32;


        [Header("Debug")]
        [SerializeField] private bool debugDrawSpawnPoints = true;
        [SerializeField] private float debugSphereRadius = 0.05f;

        private readonly List<Vector3> _debugCandidates = new();
        private readonly List<bool> _debugResults = new(); // true = success, false = rejected
        private Vector3 _debugFinalPosition;
        private bool _debugUsedFallback;

        private void Awake()
        {
            EnsurePoolRoot();
            PrewarmPool();
        }

        public void ConfigureSpawnAnchor(Transform anchor)
        {
            spawnAnchor = anchor;
        }
        

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

            PlinkoBallActor actor = GetFromPool(position, rotation);
            Rigidbody body = actor.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.AddForce(Vector3.down * 3, ForceMode.Impulse);
            }

            actor.Initialize(runController, ReleaseToPool);

            RegisterSpawnPosition(position);

            return actor;
        }

        private void PrewarmPool()
        {
            if (ballPrefab == null || prewarmCount <= 0)
            {
                return;
            }

            for (int i = _pool.Count; i < prewarmCount; i++)
            {
                PlinkoBallActor actor = CreatePooledBall();
                actor.gameObject.SetActive(false);
                _pool.Enqueue(actor);
            }
        }

        private PlinkoBallActor GetFromPool(Vector3 position, Quaternion rotation)
        {
            EnsurePoolRoot();

            PlinkoBallActor actor = _pool.Count > 0 ? _pool.Dequeue() : CreatePooledBall();
            Transform actorTransform = actor.transform;
            actorTransform.SetParent(transform, true);
            actorTransform.SetPositionAndRotation(position, rotation);
            actor.gameObject.SetActive(true);
            return actor;
        }

        private PlinkoBallActor CreatePooledBall()
        {
            PlinkoBallActor actor = Instantiate(ballPrefab, poolRoot);
            actor.name = ballPrefab.name;
            return actor;
        }

        private void ReleaseToPool(PlinkoBallActor actor)
        {
            if (actor == null)
            {
                return;
            }

            Rigidbody body = actor.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            if (_pool.Count >= maxPoolSize)
            {
                Destroy(actor.gameObject);
                return;
            }

            EnsurePoolRoot();
            actor.transform.SetParent(poolRoot, false);
            actor.gameObject.SetActive(false);
            _pool.Enqueue(actor);
        }

        private void EnsurePoolRoot()
        {
            if (poolRoot != null)
            {
                return;
            }

            GameObject root = new GameObject("PlinkoBallPool");
            root.transform.SetParent(transform, false);
            poolRoot = root.transform;
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

            if (spawnAnchor == null)
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
