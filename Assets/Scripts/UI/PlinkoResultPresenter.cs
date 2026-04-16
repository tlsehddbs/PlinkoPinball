using TMPro;
using UnityEngine;

namespace PlinkoPinball.UI.Presenters
{
    /// <summary>
    /// 플링코 런 결과를 표시하는 테스트용 프레젠터
    /// </summary>
    public sealed class PlinkoResultPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text earnedCurrencyText;
        [SerializeField] private TMP_Text pinHitsText;
        [SerializeField] private TMP_Text ballsResolvedText;

        /// <summary>
        /// 결과 패널을 갱신하고 표시
        /// </summary>
        public void Show(long earnedCurrency, int pinHits, int ballsResolved)
        {
            if (earnedCurrencyText != null)
            {
                earnedCurrencyText.text = earnedCurrency.ToString();
            }

            if (pinHitsText != null)
            {
                pinHitsText.text = pinHits.ToString();
            }

            if (ballsResolvedText != null)
            {
                ballsResolvedText.text = ballsResolved.ToString();
            }

            if (root != null)
            {
                root.SetActive(true);
            }
        }
    }
}