using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlinkoPinball.UI
{
    [DisallowMultipleComponent]
    public sealed class TopBarView : MonoBehaviour
    {
        [Header("Power")]
        [SerializeField] private TMP_Text powerLabelText;
        [SerializeField] private TMP_Text powerValueText;
        [SerializeField] private Image powerFill;

        [Header("Throughput")]
        [SerializeField] private TMP_Text throughputValueText;

        [Header("Compression")]
        [SerializeField] private TMP_Text compressionValueText;
        [SerializeField] private Image compressionFill;

        [Header("Start Ball")]
        [SerializeField] private TMP_Text startBallValueText;

        [Header("Credits")]
        [SerializeField] private TMP_Text creditsValueText;

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
        public void SetSystemStatus(string status)
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
                powerFill.fillAmount = 1f;
            }
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

            if (compressionValueText != null)
            {
                compressionValueText.text = $"{current} / {threshold}";
            }

            if (compressionFill != null)
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
    }
}