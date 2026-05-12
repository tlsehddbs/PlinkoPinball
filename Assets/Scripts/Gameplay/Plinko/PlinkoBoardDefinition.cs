using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    [Serializable]
    public struct PlinkoPinDefinition
    {
        [SerializeField] private string pinId;
        [SerializeField] private int row;
        [SerializeField] private int column;
        [SerializeField] private Vector2 localPosition;

        public string PinId => pinId;
        public int Row => row;
        public int Column => column;
        public Vector2 LocalPosition => localPosition;

        public PlinkoPinDefinition(string pinId, int row, int column, Vector2 localPosition)
        {
            this.pinId = pinId;
            this.row = row;
            this.column = column;
            this.localPosition = localPosition;
        }
    }

    [Serializable]
    public struct PlinkoSlotDefinition
    {
        [SerializeField] private string slotId;
        [SerializeField] private int column;
        [SerializeField] private Vector2 localPosition;
        [SerializeField, Min(0)] private int baseReward;

        public string SlotId => slotId;
        public int Column => column;
        public Vector2 LocalPosition => localPosition;
        public int BaseReward => baseReward;

        public PlinkoSlotDefinition(string slotId, int column, Vector2 localPosition, int baseReward)
        {
            this.slotId = slotId;
            this.column = column;
            this.localPosition = localPosition;
            this.baseReward = baseReward;
        }
    }

    /// <summary>
    /// 플링코 보드의 고정 핀/슬롯 배치를 정의
    /// </summary>
    [CreateAssetMenu(fileName = "PlinkoBoardDefinition", menuName = "PlinkoPinball/Plinko/Board Definition")]
    public sealed class PlinkoBoardDefinition : ScriptableObject
    {
        [Header("Board Layout")]
        [SerializeField, Min(1)] private int rows = 8;
        [SerializeField, Min(2)] private int columns = 5;
        [SerializeField, Min(1)] private int slotCount = 8;
        [SerializeField] private bool staggerOddRows = true;
        [SerializeField, Min(0f)] private float staggerRatio = 0.5f;
        [SerializeField] private float pinAreaYOffset = -1.0f;
        [SerializeField] private float slotAreaYOffset = 0.0f;

        [Header("Spacing")]
        [SerializeField, Min(0.01f)] private float horizontalSpacing = 1.0f;
        [SerializeField, Min(0.01f)] private float verticalSpacing = 1.2f;
        [SerializeField, Min(0.01f)] private float slotSpacing = 1.2f;

        [Header("Elements")]
        [SerializeField] private List<PlinkoPinDefinition> pins = new List<PlinkoPinDefinition>();
        [SerializeField] private List<PlinkoSlotDefinition> slots = new List<PlinkoSlotDefinition>();

        public int Rows => rows;
        public int Columns => columns;
        public int SlotCount => slotCount;
        public bool StaggerOddRows => staggerOddRows;
        public float StaggerRatio => staggerRatio;
        public float PinAreaYOffset => pinAreaYOffset;
        public float SlotAreaYOffset => slotAreaYOffset;
        public float HorizontalSpacing => horizontalSpacing;
        public float VerticalSpacing => verticalSpacing;
        public float SlotSpacing => slotSpacing;

        public IReadOnlyList<PlinkoPinDefinition> Pins => pins;
        public IReadOnlyList<PlinkoSlotDefinition> Slots => slots;

        /// <summary>
        /// 현재 레이아웃 메타데이터를 기준으로 핀/슬롯 정의를 재생성
        /// remarks:
        /// 긴 줄은 Columns 개수, 짧은 줄은 Columns - 1 개수로 생성
        /// </remarks>
        /// </summary>
        [ContextMenu("Rebuild Layout Definitions")]
        public void RebuildLayoutDefinitions()
        {
            pins.Clear();
            slots.Clear();

            int longRowCount = Mathf.Max(2, columns);
            int shortRowCount = Mathf.Max(1, columns - 1);

            float longRowWidth = (longRowCount - 1) * horizontalSpacing;
            float longRowCenterOffsetX = longRowWidth * 0.5f;

            float totalHeight = (rows - 1) * verticalSpacing;
            float centerOffsetY = totalHeight * 0.5f;

            for (int row = 0; row < rows; row++)
            {
                bool isShortRow = staggerOddRows && (row % 2 == 1);
                int rowPinCount = isShortRow ? shortRowCount : longRowCount;

                float rowWidth = (rowPinCount - 1) * horizontalSpacing;
                float rowCenterOffsetX = rowWidth * 0.5f;

                for (int col = 0; col < rowPinCount; col++)
                {
                    float x = (col * horizontalSpacing) - rowCenterOffsetX;
                    float y = centerOffsetY - (row * verticalSpacing) + pinAreaYOffset;

                    string pinId = $"pin.r{row:00}.c{col:00}";
                    pins.Add(new PlinkoPinDefinition(pinId, row, col, new Vector2(x, y)));
                }
            }

            float slotTotalWidth = (slotCount - 1) * slotSpacing;
            float slotCenterOffsetX = slotTotalWidth * 0.5f;

            for (int i = 0; i < slotCount; i++)
            {
                float x = (i * slotSpacing) - slotCenterOffsetX;
                float y = -centerOffsetY - verticalSpacing + slotAreaYOffset;

                string slotId = $"slot.{i:00}";
                slots.Add(new PlinkoSlotDefinition(slotId, i, new Vector2(x, y), 5));
            }
        }

        /// <summary>
        /// Authoring 단계에서 보드 메타데이터와 핀/슬롯 데이터를 함께 갱신
        /// </summary>
        public void ApplyAuthoringLayout(int newRows, int newColumns, int newSlotCount, bool newStaggerOddRows, float newStaggerRatio, float newPinAreaYOffset, float newSlotAreaYOffset, IReadOnlyList<PlinkoPinDefinition> newPins, IReadOnlyList<PlinkoSlotDefinition> newSlots)
        {
            rows = Mathf.Max(1, newRows);
            columns = Mathf.Max(2, newColumns);
            slotCount = Mathf.Max(1, newSlotCount);
            staggerOddRows = newStaggerOddRows;
            staggerRatio = Mathf.Max(0f, newStaggerRatio);
            pinAreaYOffset = newPinAreaYOffset;
            slotAreaYOffset = newSlotAreaYOffset;

            pins.Clear();
            slots.Clear();

            if (newPins != null)
            {
                for (int i = 0; i < newPins.Count; i++)
                {
                    pins.Add(newPins[i]);
                }
            }

            if (newSlots != null)
            {
                for (int i = 0; i < newSlots.Count; i++)
                {
                    slots.Add(newSlots[i]);
                }
            }
        }
    }
}