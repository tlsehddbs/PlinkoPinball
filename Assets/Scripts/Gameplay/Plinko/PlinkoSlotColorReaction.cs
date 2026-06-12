using System.Collections;
using UnityEngine;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Components.Plinko
{
    /// <summary>
    /// Plinko Slot Runtime 상태에 따라 Renderer 색상을 갱신하고, 슬롯 도착 시 선택적으로 하이라이트합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlinkoSlotColorReaction : MonoBehaviour, IPlinkoSlotReaction
    {
        [Header("References")]
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private PlinkoSlotRuntime slotRuntime;

        [Header("State Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color boostedColor = new(0.35f, 0.9f, 1f, 1f);
        [SerializeField] private Color errorColor = new(1f, 0.15f, 0.1f, 1f);

        [Header("Resolve Flash")]
        [SerializeField] private bool flashOnResolve = true;
        [SerializeField] private Color resolveFlashColor = new(1f, 0.95f, 0.35f, 1f);
        [SerializeField, Min(0.01f)] private float flashDuration = 0.08f;

        [Header("Material Property")]
        [SerializeField] private string colorProperty = "_BaseColor";

        private MaterialPropertyBlock _propertyBlock;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>();
            }

            if (slotRuntime == null)
            {
                slotRuntime = GetComponentInParent<PlinkoSlotRuntime>();
            }

            _propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            SetColor(ResolveColor());
        }

        public void OnSlotResolved(
            PlinkoBallActor ball,
            Vector3 hitPoint,
            PlinkoSlotRuntime runtime,
            in PlinkoSlotModifierData modifier,
            int finalReward)
        {
            if (!flashOnResolve || targetRenderer == null)
            {
                return;
            }

            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }

            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            SetColor(resolveFlashColor);
            yield return new WaitForSeconds(flashDuration);
            Refresh();
            _flashRoutine = null;
        }

        private Color ResolveColor()
        {
            if (slotRuntime != null)
            {
                PlinkoSlotModifierData data = slotRuntime.ExportState();
                return ResolveSlotColor(data.StateKind);
            }

            return normalColor;
        }

        private Color ResolveSlotColor(PlinkoSlotStateKind stateKind)
        {
            switch (stateKind)
            {
                case PlinkoSlotStateKind.Error:
                    return errorColor;

                case PlinkoSlotStateKind.Boosted:
                    return boostedColor;

                default:
                    return normalColor;
            }
        }

        private void SetColor(Color color)
        {
            if (targetRenderer == null)
            {
                return;
            }

            targetRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(colorProperty, color);
            targetRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
