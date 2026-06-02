using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlinkoPinball.UI
{
    [DisallowMultipleComponent]
    public sealed class TopBarView : MonoBehaviour
    {
        [Header("Power")]
        [SerializeField] private GameObject powerRoot;
        [SerializeField] private TMP_Text powerLabelText;
        [SerializeField] private TMP_Text powerValueText;
        [SerializeField] private Image powerFill;

        [Header("Throughput")]
        [SerializeField] private GameObject throughputRoot;
        [SerializeField] private TMP_Text throughputValueText;

        [Header("Compression")]
        [SerializeField] private GameObject compressionRoot;
        [SerializeField] private TMP_Text compressionValueText;
        [SerializeField] private Image compressionFill;
        [SerializeField, Min(0f)] private float compressionFillSmoothTime = 0.12f;

        [Header("Start Ball")]
        [SerializeField] private GameObject startBallRoot;
        [SerializeField] private TMP_Text startBallValueText;

        [Header("Credits")]
        [SerializeField] private GameObject creditsRoot;
        [SerializeField] private TMP_Text creditsValueText;

        private float _compressionFillTarget;
        private float _compressionFillVelocity;

        private void Update()
        {
            UpdateCompressionFill();
        }

        /// <summary>
        /// Pinball Phase용 Power 상태를 표시
        /// </summary>
        public void SetPower(float current, float max)
        {
            float normalized = max > 0f ? Mathf.Clamp01(current / max) : 0f;

            if (powerLabelText != null)
            {
                powerLabelText.text = "POWER";
            }

            if (powerValueText != null)
            {
                powerValueText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }

            if (powerFill != null)
            {
                powerFill.fillAmount = normalized;
            }
        }

        /// <summary>
        /// Plinko Phase 등에서 시스템 상태 메시지를 표시
        /// </summary>
        public void SetSystemStatus(string status, float fillAmount = 1f)
        {
            if (powerLabelText != null)
            {
                powerLabelText.text = "SYSTEM";
            }

            if (powerValueText != null)
            {
                powerValueText.text = status;
            }

            if (powerFill != null)
            {
                powerFill.fillAmount = Mathf.Clamp01(fillAmount);
            }
        }

        public void SetPowerVisible(bool visible)
        {
            SetSectionVisible(powerRoot, visible, powerLabelText, powerValueText, powerFill);
        }

        public void SetThroughputVisible(bool visible)
        {
            SetSectionVisible(throughputRoot, visible, throughputValueText);
        }

        public void SetCompressionVisible(bool visible)
        {
            SetSectionVisible(compressionRoot, visible, compressionValueText, compressionFill);
        }

        public void SetStartBallVisible(bool visible)
        {
            SetSectionVisible(startBallRoot, visible, startBallValueText);
        }

        public void SetCreditsVisible(bool visible)
        {
            SetSectionVisible(creditsRoot, visible, creditsValueText);
        }

        public void SetThroughput(long value)
        {
            if (throughputValueText != null)
            {
                throughputValueText.text = value.ToString("N0");
            }
        }

        public void SetCompression(int current, int threshold)
        {
            float normalized = threshold > 0 ? Mathf.Clamp01((float)current / threshold) : 0f;
            _compressionFillTarget = normalized;

            if (compressionValueText != null)
            {
                compressionValueText.text = $"{current} / {threshold}";
            }

            if (compressionFill != null && compressionFillSmoothTime <= 0f)
            {
                compressionFill.fillAmount = normalized;
            }
        }

        public void SetStartBallCount(int count)
        {
            if (startBallValueText != null)
            {
                startBallValueText.text = count.ToString("N0");
            }
        }

        public void SetCredits(long value)
        {
            if (creditsValueText != null)
            {
                creditsValueText.text = value.ToString("N0");
            }
        }

        private static void SetSectionVisible(GameObject root, bool visible, params Component[] fallbackComponents)
        {
            if (root != null)
            {
                root.SetActive(visible);
                return;
            }

            if (fallbackComponents == null)
            {
                return;
            }

            for (int i = 0; i < fallbackComponents.Length; i++)
            {
                if (fallbackComponents[i] != null)
                {
                    fallbackComponents[i].gameObject.SetActive(visible);
                }
            }
        }

        private void UpdateCompressionFill()
        {
            if (compressionFill == null || !compressionFill.gameObject.activeInHierarchy)
            {
                return;
            }

            if (compressionFillSmoothTime <= 0f)
            {
                compressionFill.fillAmount = _compressionFillTarget;
                return;
            }

            compressionFill.fillAmount = Mathf.SmoothDamp(
                compressionFill.fillAmount,
                _compressionFillTarget,
                ref _compressionFillVelocity,
                compressionFillSmoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime);
        }
    }
}
