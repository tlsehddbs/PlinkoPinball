using UnityEngine;
using System.Collections.Generic;
using PlinkoPinball.Gameplay.Core.Plinko;
using PlinkoPinball.Gameplay.Upgrade;

namespace PlinkoPinball.Gameplay.Components.Plinko
{
    [DisallowMultipleComponent]
    public sealed class PlinkoPinFloatingTextReaction : MonoBehaviour, IPlinkoPinReaction
    {
        [Header("Prefab")]
        [SerializeField] private FloatingTextView floatingTextPrefab;

        [Header("References")]
        [SerializeField] private PlinkoPinRuntime runtime;
        [SerializeField] private UpgradeEffectResolver upgradeEffectResolver;

        [Header("Text")]
        [SerializeField] private string normalText = "+1";
        [SerializeField] private string bonusFormat = "+{0}";
        [SerializeField] private string multiplierFormat = "x{0:0.##}";
        [SerializeField] private string bonusMultiplierFormat = "+{0} x{1:0.##}";

        [Header("Position")]
        [SerializeField] private bool useHitPoint = true;
        [SerializeField] private Vector3 spawnOffset = new(0f, 0.35f, 0f);

        [Header("Spam Control")]
        [SerializeField, Min(0f)] private float cooldown = 0.03f;

        [Header("Pooling")]
        [SerializeField, Min(0)] private int prewarmCount;
        [SerializeField, Min(1)] private int maxPoolSize = 24;
        [SerializeField] private Transform poolRoot;

        private float _lastShownTime = -999f;
        private readonly Queue<FloatingTextView> _pool = new();


        private void Awake()
        {
            if (runtime == null)
            {
                runtime = GetComponentInParent<PlinkoPinRuntime>();
            }

            EnsurePoolRoot();
            PrewarmPool();
        }

        public void OnPinHit(Rigidbody ball, Vector3 hitPoint)
        {
            if (floatingTextPrefab == null || runtime == null)
            {
                return;
            }

            if (Time.time < _lastShownTime + cooldown)
            {
                return;
            }

            PlinkoPinModifierData data = runtime.ExportState();

            if (data.StateKind == PlinkoPinStateKind.Error)
            {
                return;
            }

            string displayText = BuildText(data);

            _lastShownTime = Time.time;

            Vector3 spawnPosition = useHitPoint ? hitPoint : transform.position;
            spawnPosition += spawnOffset;

            FloatingTextView view = GetFromPool(spawnPosition);
            view.Play(displayText, spawnPosition, ReleaseToPool);
        }

        private void PrewarmPool()
        {
            if (floatingTextPrefab == null || prewarmCount <= 0)
            {
                return;
            }

            for (int i = _pool.Count; i < prewarmCount; i++)
            {
                FloatingTextView view = CreatePooledText();
                view.gameObject.SetActive(false);
                _pool.Enqueue(view);
            }
        }

        private FloatingTextView GetFromPool(Vector3 position)
        {
            EnsurePoolRoot();

            FloatingTextView view = _pool.Count > 0 ? _pool.Dequeue() : CreatePooledText();
            view.transform.SetParent(transform, true);
            view.transform.SetPositionAndRotation(position, Quaternion.identity);
            view.gameObject.SetActive(true);
            return view;
        }

        private FloatingTextView CreatePooledText()
        {
            FloatingTextView view = Instantiate(floatingTextPrefab, poolRoot);
            view.name = floatingTextPrefab.name;
            return view;
        }

        private void ReleaseToPool(FloatingTextView view)
        {
            if (view == null)
            {
                return;
            }

            if (_pool.Count >= maxPoolSize)
            {
                Destroy(view.gameObject);
                return;
            }

            EnsurePoolRoot();
            view.transform.SetParent(poolRoot, false);
            view.gameObject.SetActive(false);
            _pool.Enqueue(view);
        }

        private void EnsurePoolRoot()
        {
            if (poolRoot != null)
            {
                return;
            }

            GameObject root = new GameObject("PlinkoFloatingTextPool");
            root.transform.SetParent(transform, false);
            poolRoot = root.transform;
        }

        private string BuildText(in PlinkoPinModifierData data)
        {
            bool hasBonus = data.ValueBonus > 0;
            bool hasMultiplier = !Mathf.Approximately(data.Multiplier, 1f);
            int baseValue = GetBaseValue();

            if (hasBonus && hasMultiplier)
            {
                return string.Format(bonusMultiplierFormat, baseValue + data.ValueBonus, data.Multiplier);
            }

            if (hasBonus)
            {
                return string.Format(bonusFormat, baseValue + data.ValueBonus);
            }

            if (hasMultiplier)
            {
                return string.Format(multiplierFormat, data.Multiplier);
            }

            return baseValue > 0 ? $"+{baseValue}" : normalText;
        }

        private int GetBaseValue()
        {
            ResolveUpgradeEffectResolver();
            return upgradeEffectResolver != null ? upgradeEffectResolver.GetBasePinValue() : 1;
        }

        private void ResolveUpgradeEffectResolver()
        {
            if (upgradeEffectResolver != null)
            {
                return;
            }

            upgradeEffectResolver = UpgradeEffectResolver.Instance;
            if (upgradeEffectResolver != null)
            {
                return;
            }

            upgradeEffectResolver = FindFirstObjectByType<UpgradeEffectResolver>();
        }
    }
}
