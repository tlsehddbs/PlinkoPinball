using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlinkoPinball.UI.MainMenu
{
    [DisallowMultipleComponent]
    public sealed class MainMenuSelectionArrow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private TMP_Text arrowText;
        private TMP_FontAsset font;
        private Color color = Color.white;
        private int hoverDepth;
        private bool selected;
        private bool createdArrowText;

        public void Initialize(Color arrowColor, TMP_FontAsset fontAsset)
        {
            color = arrowColor;
            font = fontAsset;
            EnsureArrow();
            Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hoverDepth++;
            EventSystem.current?.SetSelectedGameObject(gameObject);
            Refresh();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hoverDepth = Mathf.Max(0, hoverDepth - 1);
            Refresh();
        }

        public void OnSelect(BaseEventData eventData)
        {
            selected = true;
            Refresh();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selected = false;
            Refresh();
        }

        private void EnsureArrow()
        {
            if (arrowText != null)
            {
                return;
            }

            Transform existing = transform.Find("SelectionArrow");
            if (existing != null)
            {
                arrowText = existing.GetComponent<TMP_Text>();
            }

            if (arrowText == null)
            {
                GameObject arrowObject = new GameObject("SelectionArrow", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                arrowObject.layer = 5;
                arrowObject.transform.SetParent(transform, false);
                arrowText = arrowObject.GetComponent<TMP_Text>();
                createdArrowText = true;
            }

            if (!createdArrowText)
            {
                return;
            }

            RectTransform rect = arrowText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(38f, 0f);
            rect.sizeDelta = new Vector2(36f, 0f);
            arrowText.text = ">";
            arrowText.fontSize = 24f;
            arrowText.alignment = TextAlignmentOptions.MidlineLeft;
            arrowText.textWrappingMode = TextWrappingModes.NoWrap;
            arrowText.color = color;
            arrowText.raycastTarget = false;

            if (font != null)
            {
                arrowText.font = font;
            }
        }

        private void Refresh()
        {
            EnsureArrow();
            if (createdArrowText)
            {
                arrowText.color = color;
            }
            arrowText.gameObject.SetActive(selected || hoverDepth > 0);
        }
    }
}
