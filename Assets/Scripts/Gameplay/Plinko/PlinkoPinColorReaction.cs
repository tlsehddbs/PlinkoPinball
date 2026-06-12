using UnityEngine;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Components.Plinko
{
    /// <summary>
    /// Plinko Pin Runtime 상태에 따라 Renderer 색상을 갱신
    /// Normal / Boosted / Error 상태를 시각적으로 구분
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlinkoPinColorReaction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private PlinkoPinRuntime pinRuntime;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color boostedColor = new(0.35f, 0.9f, 1f, 1f);
        [SerializeField] private Color errorColor = new(1f, 0.15f, 0.1f, 1f);

        [Header("Material Property")]
        [SerializeField] private string colorProperty = "_BaseColor";

        private MaterialPropertyBlock _propertyBlock;

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>();
            }

            if (pinRuntime == null)
            {
                pinRuntime = GetComponentInParent<PlinkoPinRuntime>();
            }

            _propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            Refresh();
        }

        /// <summary>
        /// 현재 Runtime 상태를 기준으로 색상을 즉시 갱신
        /// BoardStateApplier가 Snapshot을 적용한 뒤 호출
        /// </summary>
        public void Refresh()
        {
            if (targetRenderer == null)
            {
                return;
            }

            Color color = ResolveColor();

            targetRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(colorProperty, color);
            targetRenderer.SetPropertyBlock(_propertyBlock);
        }

        private Color ResolveColor()
        {
            if (pinRuntime != null)
            {
                PlinkoPinModifierData data = pinRuntime.ExportState();
                
                return ResolvePinColor(data.StateKind);
            }

            return normalColor;
        }

        private Color ResolvePinColor(PlinkoPinStateKind stateKind)
        {
            switch (stateKind)
            {
                case PlinkoPinStateKind.Error:
                    return errorColor;

                case PlinkoPinStateKind.Boosted:
                    return boostedColor;

                default:
                    return normalColor;
            }
        }
    }
}