using System.Collections;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Components.Plinko
{
    /// <summary>
    /// 플링코 핀이 충돌 시 순간적으로 커졌다가 원래 크기로 돌아오는 로컬 피드백
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlinkoPinPopScaleReaction : MonoBehaviour, IPlinkoPinReaction
    {
        [Header("Target")]
        [SerializeField] private Transform scaleTarget;

        [Header("Pop")]
        [SerializeField, Min(1f)] private float popScale = 1.22f;
        [SerializeField, Min(0.01f)] private float growDuration = 0.035f;
        [SerializeField, Min(0.01f)] private float shrinkDuration = 0.08f;

        [Header("Options")]
        [SerializeField] private bool restartOnRepeatedHit = true;

        private Vector3 _baseScale;
        private Coroutine _routine;

        private void Awake()
        {
            if (scaleTarget == null)
            {
                scaleTarget = transform;
            }

            _baseScale = scaleTarget.localScale;
        }

        /// <inheritdoc />
        public void OnPinHit(Rigidbody ball, Vector3 hitPoint)
        {
            if (_routine != null)
            {
                if (!restartOnRepeatedHit)
                {
                    return;
                }

                StopCoroutine(_routine);
                scaleTarget.localScale = _baseScale;
            }

            _routine = StartCoroutine(PopRoutine());
        }

        private IEnumerator PopRoutine()
        {
            Vector3 poppedScale = _baseScale * popScale;

            float elapsed = 0f;

            while (elapsed < growDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / growDuration);
                scaleTarget.localScale = Vector3.LerpUnclamped(_baseScale, poppedScale, EaseOutBack(t));
                yield return null;
            }

            elapsed = 0f;

            while (elapsed < shrinkDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / shrinkDuration);
                scaleTarget.localScale = Vector3.LerpUnclamped(poppedScale, _baseScale, EaseOutCubic(t));
                yield return null;
            }

            scaleTarget.localScale = _baseScale;
            _routine = null;
        }

        private static float EaseOutCubic(float t)
        {
            t = 1f - t;
            return 1f - t * t * t;
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnValidate()
        {
            popScale = Mathf.Max(1f, popScale);
            growDuration = Mathf.Max(0.01f, growDuration);
            shrinkDuration = Mathf.Max(0.01f, shrinkDuration);
        }
#endif
    }
}