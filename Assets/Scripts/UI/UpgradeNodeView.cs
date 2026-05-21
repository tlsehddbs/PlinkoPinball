using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlinkoPinball.UI.Upgrades
{
    /// <summary>
    /// 업그레이드 맵 위에 배치되는 단일 노드 View다.
    /// 실제 구매 판정은 외부 Presenter/System에서 처리한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UpgradeNodeView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text costText;

        public Button Button => button;

        public void SetView(string title, int level, int maxLevel, int cost)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (levelText != null)
            {
                levelText.text = $"Lv {level}/{maxLevel}";
            }

            if (costText != null)
            {
                costText.text = cost.ToString();
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }
}