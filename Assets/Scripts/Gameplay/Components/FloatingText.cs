using System.Collections;
using System;
using TMPro;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Components
{
    [DisallowMultipleComponent]
    public sealed class FloatingTextView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Motion")]
        [SerializeField, Min(0.01f)] private float lifetime = 0.65f;
        [SerializeField] private Vector3 worldOffset = new(0f, 0.75f, 0f);

        private Camera _mainCamera;
        private Coroutine _playRoutine;
        private Action<FloatingTextView> _releaseRequested;

        private void Awake()
        {
            if (label == null)
            {
                label = GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_mainCamera == null)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(transform.position - _mainCamera.transform.position);
        }

        public void Play(string text, Vector3 worldPosition)
        {
            Play(text, worldPosition, null);
        }

        public void Play(string text, Vector3 worldPosition, Action<FloatingTextView> releaseRequested)
        {
            _releaseRequested = releaseRequested;
            transform.position = worldPosition;

            if (label != null)
            {
                label.text = text;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
            }

            _playRoutine = StartCoroutine(PlayRoutine(worldPosition));
        }

        private IEnumerator PlayRoutine(Vector3 startPosition)
        {
            Vector3 endPosition = startPosition + worldOffset;
            float elapsed = 0f;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / lifetime);

                transform.position = Vector3.Lerp(startPosition, endPosition, t);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f - t;
                }

                yield return null;
            }

            _playRoutine = null;

            if (_releaseRequested != null)
            {
                _releaseRequested.Invoke(this);
                yield break;
            }

            Destroy(gameObject);
        }
    }
}
