using System.Collections;
using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Core;

namespace PlinkoPinball.Gameplay.Components.Reactions
{
    public sealed class PinballFlashReaction : MonoBehaviour, ITableEventReaction
    {
        [Header("Renderer")]
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private string colorProperty = "_BaseColor";

        [Header("Flash")]
        [SerializeField] private Color flashColor = Color.white;

        [Min(0.01f)]
        [SerializeField] private float duration = 0.08f;

        private MaterialPropertyBlock _block;
        private Color _baseColor = Color.white;
        private Coroutine _routine;

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>();
            }

            _block = new MaterialPropertyBlock();

            if (targetRenderer != null && targetRenderer.sharedMaterial != null)
            {
                if (targetRenderer.sharedMaterial.HasProperty(colorProperty))
                {
                    _baseColor = targetRenderer.sharedMaterial.GetColor(colorProperty);
                }
            }
        }

        public void OnTableEvent(in TableEvent e)
        {
            if (targetRenderer == null)
            {
                return;
            }

            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            SetColor(flashColor);
            yield return new WaitForSeconds(duration);
            SetColor(_baseColor);
            _routine = null;
        }

        private void SetColor(Color color)
        {
            targetRenderer.GetPropertyBlock(_block);
            _block.SetColor(colorProperty, color);
            targetRenderer.SetPropertyBlock(_block);
        }
    }
}