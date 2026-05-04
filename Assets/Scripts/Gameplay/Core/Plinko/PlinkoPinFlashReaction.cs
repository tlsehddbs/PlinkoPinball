using System.Collections;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Components.Plinko
{
    /// <summary>
    /// 플링코 핀 충돌 시 Renderer 색상을 순간적으로 밝게 점멸시키는 로컬 반응
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlinkoPinFlashReaction : MonoBehaviour, IPlinkoPinReaction
    {
        [Header("Target")]
        [SerializeField] private Renderer targetRenderer;

        [Header("Flash")]
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField, Min(0.01f)] private float duration = 0.08f;

        private MaterialPropertyBlock _propertyBlock;
        private Color _baseColor;
        private Coroutine _routine;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>();
            }

            _propertyBlock = new MaterialPropertyBlock();

            if (targetRenderer != null && targetRenderer.sharedMaterial != null)
            {
                if (targetRenderer.sharedMaterial.HasProperty(BaseColorId))
                {
                    _baseColor = targetRenderer.sharedMaterial.GetColor(BaseColorId);
                }
                else if (targetRenderer.sharedMaterial.HasProperty(ColorId))
                {
                    _baseColor = targetRenderer.sharedMaterial.GetColor(ColorId);
                }
                else
                {
                    _baseColor = Color.white;
                }
            }
        }


        public void OnPinHit(Rigidbody ball, Vector3 hitPoint)
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

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetColor(Color.Lerp(flashColor, _baseColor, t));
                yield return null;
            }

            SetColor(_baseColor);
            _routine = null;
        }

        private void SetColor(Color color)
        {
            targetRenderer.GetPropertyBlock(_propertyBlock);

            if (targetRenderer.sharedMaterial != null && targetRenderer.sharedMaterial.HasProperty(BaseColorId))
            {
                _propertyBlock.SetColor(BaseColorId, color);
            }
            else
            {
                _propertyBlock.SetColor(ColorId, color);
            }

            targetRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}