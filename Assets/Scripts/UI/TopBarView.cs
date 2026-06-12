using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlinkoPinball.UI
{
    /// <summary>
    /// 상단 또는 사이드 HUD에 표시되는 핵심 런 상태 정보를 출력한다.
    /// Power Reserve, Throughput, Compression, Start Ball Buffer, Credits 표시를 담당한다.
    /// 
    /// 이 View는 시스템 상태를 직접 계산하지 않고, 외부 시스템에서 전달된 값을
    /// 터미널 스타일 UI 텍스트로 변환해 표시한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TopBarView : MonoBehaviour
    {
        [Header("ASCII Progress Style")]
        [SerializeField, Min(4)] private int asciiBarLength = 16;
        [SerializeField] private char filledChar = '#';
        [SerializeField] private char emptyChar = '-';

        [Header("Power Color Threshold")]
        [SerializeField, Range(0f, 1f)] private float powerWarningThreshold = 0.25f;
        [SerializeField, Range(0f, 1f)] private float powerCriticalThreshold = 0.1f;
        [SerializeField] private Color powerNormalColor = Color.white;
        [SerializeField] private Color powerWarningColor = new Color(1f, 0.72f, 0.25f);
        [SerializeField] private Color powerCriticalColor = new Color(1f, 0.25f, 0.25f);
        [SerializeField] private bool forceTextColor;
        [SerializeField] private Color forcedTextColor = Color.black;

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

        private readonly StringBuilder _barBuilder = new StringBuilder(32);

        private const string MonospaceOpenTag = "<mspace=0.6em>";
        private const string MonospaceCloseTag = "</mspace>";

        public void SetTextColor(Color color, bool forceDynamicColors = true)
        {
            forcedTextColor = color;
            forceTextColor = forceDynamicColors;

            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null)
                {
                    texts[i].color = color;
                }
            }
        }

        private void Update()
        {
            UpdateCompressionFill();
        }

        /// <summary>
        /// Pinball Phase의 Power Reserve 상태를 표시한다.
        /// Power는 시간 자원에 해당하므로 current / max 대신 퍼센트 중심으로 출력한다.
        /// </summary>
        /// <param name="current">현재 남은 Power Reserve 값.</param>
        /// <param name="max">최대 Power Reserve 값.</param>
        public void SetPower(float current, float max)
        {
            float normalized = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            int percent = Mathf.RoundToInt(normalized * 100f);

            if (powerLabelText != null)
            {
                powerLabelText.text = "POWER";
                powerLabelText.color = GetTextColor(GetPowerColor(normalized));
            }

            if (powerValueText != null)
            {
                string bar = BuildAsciiBar(normalized);
                powerValueText.text = $"{MonospaceOpenTag}{bar} {percent}%{MonospaceCloseTag}";
                powerValueText.color = GetTextColor(GetPowerColor(normalized));
            }

            if (powerFill != null)
            {
                powerFill.fillAmount = normalized;
                powerFill.color = GetPowerColor(normalized);
            }
        }

        /// <summary>
        /// Plinko Phase 또는 비-Pinball Phase에서 시스템 상태 메시지를 표시한다.
        /// 예: CHARGING, COMPUTING, SNAPSHOT READY.
        /// </summary>
        /// <param name="status">표시할 시스템 상태 문자열.</param>
        /// <param name="fillAmount">상태 진행률. 0~1 범위로 클램프된다.</param>
        public void SetSystemStatus(string status, float fillAmount = 1f)
        {
            float normalized = Mathf.Clamp01(fillAmount);

            if (powerLabelText != null)
            {
                powerLabelText.text = "SYSTEM";
                powerLabelText.color = GetTextColor(powerNormalColor);
            }

            if (powerValueText != null)
            {
                string bar = BuildAsciiBar(normalized);
                powerValueText.text = $"{MonospaceOpenTag}{bar} {status}{MonospaceCloseTag}";
                powerValueText.color = GetTextColor(powerNormalColor);
            }

            if (powerFill != null)
            {
                powerFill.fillAmount = normalized;
                powerFill.color = powerNormalColor;
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

        /// <summary>
        /// Throughput은 핀볼 런 성능 지표이므로 ProgressBar가 아닌 숫자 출력으로 표시한다.
        /// </summary>
        /// <param name="value">현재 Throughput 값.</param>
        public void SetThroughput(long value)
        {
            if (throughputValueText != null)
            {
                throughputValueText.text = $"{MonospaceOpenTag}{value:N0}{MonospaceCloseTag}";
            }
        }

        /// <summary>
        /// Packet Compression 진행도를 ASCII ProgressBar와 current / threshold 형식으로 표시한다.
        /// </summary>
        /// <param name="current">현재 Compression Progress.</param>
        /// <param name="threshold">Plinko Start Ball 1개 획득에 필요한 Threshold.</param>
        public void SetCompression(int current, int threshold)
        {
            float normalized = threshold > 0 ? Mathf.Clamp01((float)current / threshold) : 0f;
            int percent = Mathf.RoundToInt(normalized * 100f);

            _compressionFillTarget = normalized;

            if (compressionValueText != null)
            {
                string bar = BuildAsciiBar(normalized);
                compressionValueText.text = $"{MonospaceOpenTag}{bar} {percent}%{MonospaceCloseTag}";
            }

            if (compressionFill != null && compressionFillSmoothTime <= 0f)
            {
                compressionFill.fillAmount = normalized;
            }
        }

        /// <summary>
        /// Plinko Phase 시작 시 사용할 Start Ball Buffer 수를 표시한다.
        /// </summary>
        /// <param name="count">누적된 Plinko Start Ball 수.</param>
        public void SetStartBallCount(int count)
        {
            if (startBallValueText != null)
            {
                startBallValueText.text = $"{MonospaceOpenTag}{count:N0}{MonospaceCloseTag}";
            }
        }

        /// <summary>
        /// 현재 보유 Currency/Credits 값을 표시한다.
        /// </summary>
        /// <param name="value">현재 Currency 값.</param>
        public void SetCredits(long value)
        {
            if (creditsValueText != null)
            {
                creditsValueText.text = $"{MonospaceOpenTag}{value:N0}{MonospaceCloseTag}";
            }
        }

        private string BuildAsciiBar(float normalized)
        {
            normalized = Mathf.Clamp01(normalized);

            int filledCount = Mathf.RoundToInt(normalized * asciiBarLength);
            filledCount = Mathf.Clamp(filledCount, 0, asciiBarLength);

            _barBuilder.Clear();
            _barBuilder.Append('[');

            for (int i = 0; i < asciiBarLength; i++)
            {
                _barBuilder.Append(i < filledCount ? filledChar : emptyChar);
            }

            _barBuilder.Append(']');
            return _barBuilder.ToString();
        }

        private Color GetPowerColor(float normalized)
        {
            if (normalized <= powerCriticalThreshold)
            {
                return powerCriticalColor;
            }

            if (normalized <= powerWarningThreshold)
            {
                return powerWarningColor;
            }

            return powerNormalColor;
        }

        private Color GetTextColor(Color fallback)
        {
            return forceTextColor ? forcedTextColor : fallback;
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
