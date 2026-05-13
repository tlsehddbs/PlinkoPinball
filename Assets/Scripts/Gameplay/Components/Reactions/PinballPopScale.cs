using System.Collections;
using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Core;

namespace PlinkoPinball.Gameplay.Components.Reactions
{
    public sealed class PinballPopScale : MonoBehaviour, ITableEventReaction
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Scale")]
        [Min(1f)]
        [SerializeField] private float popScale = 1.15f;

        [Min(0.01f)]
        [SerializeField] private float growDuration = 0.05f;

        [Min(0.01f)]
        [SerializeField] private float shrinkDuration = 0.08f;

        private Vector3 _baseScale;
        private Coroutine _routine;

        private void Awake()
        {
            if (target == null)
            {
                target = transform;
            }

            _baseScale = target.localScale;
        }

        public void OnTableEvent(in TableEvent e)
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(PopRoutine());
        }

        private IEnumerator PopRoutine()
        {
            Vector3 peak = _baseScale * popScale;

            float t = 0f;
            while (t < growDuration)
            {
                t += Time.deltaTime;
                target.localScale = Vector3.Lerp(_baseScale, peak, t / growDuration);
                yield return null;
            }

            t = 0f;
            while (t < shrinkDuration)
            {
                t += Time.deltaTime;
                target.localScale = Vector3.Lerp(peak, _baseScale, t / shrinkDuration);
                yield return null;
            }

            target.localScale = _baseScale;
            _routine = null;
        }
    }
}