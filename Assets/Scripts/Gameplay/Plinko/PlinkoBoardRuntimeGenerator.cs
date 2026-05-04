using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Gameplay.Components.Plinko;
using UnityEditor;
using Unity.Mathematics;


namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 보드 정의를 기준으로 런타임 핀과 슬롯을 생성
    /// <remarks>
    /// 핀/슬롯의 개수, ID, 로컬 좌표는 모두 <see cref="PlinkoBoardDefinition"/>을 단일 기준 데이터로 사용합니다.
    /// </remarks>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlinkoBoardRuntimeGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlinkoBoardLayoutRoot layoutRoot;
        [SerializeField] private Transform pinRoot;
        [SerializeField] private Transform slotRoot;
        [SerializeField] private PlinkoBoardDefinition boardDefinition;

        [Header("Prefabs")]
        [SerializeField] private GameObject pinPrefab;
        [SerializeField] private GameObject slotPrefab;

        /// <summary>
        /// 보드 정의를 기준으로 핀과 슬롯을 생성
        /// </summary>
        /// <param name="generatedPins">생성된 핀 런타임 배열</param>
        /// <param name="generatedSlots">생성된 슬롯 런타임 배열</param>
        public void GenerateBoard(out PlinkoPinRuntime[] generatedPins, out PlinkoSlotRuntime[] generatedSlots)
        {
            generatedPins = System.Array.Empty<PlinkoPinRuntime>();
            generatedSlots = System.Array.Empty<PlinkoSlotRuntime>();

            if (!ValidateRuntimeReferences())
            {
                return;
            }

            ClearChildren(pinRoot);
            ClearChildren(slotRoot);

            List<PlinkoPinRuntime> pinList = new List<PlinkoPinRuntime>(boardDefinition.Pins.Count);
            List<PlinkoSlotRuntime> slotList = new List<PlinkoSlotRuntime>(boardDefinition.Slots.Count);

            Vector3 boardCenter = layoutRoot.BoardCenterAnchor.position;

            for (int i = 0; i < boardDefinition.Pins.Count; i++)
            {
                PlinkoPinDefinition pinDefinition = boardDefinition.Pins[i];
                Vector2 local = pinDefinition.LocalPosition;

                Vector3 worldPosition = new Vector3(
                    boardCenter.x + local.x,
                    boardCenter.y + local.y,
                    boardCenter.z);

                GameObject instance = Instantiate(pinPrefab, worldPosition, quaternion.identity, pinRoot);
                instance.name = pinDefinition.PinId;

                PlinkoPinRuntime runtime = instance.GetComponent<PlinkoPinRuntime>();
                if (runtime != null)
                {
                    runtime.SetPinIdForAuthoring(pinDefinition.PinId);
                    pinList.Add(runtime);
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                else
                {
                    Debug.LogWarning(
                        $"[{nameof(PlinkoBoardRuntimeGenerator)}] Pin prefab is missing {nameof(PlinkoPinRuntime)}.",
                        this);
                }
#endif
            }

            for (int i = 0; i < boardDefinition.Slots.Count; i++)
            {
                PlinkoSlotDefinition slotDefinition = boardDefinition.Slots[i];
                Vector2 local = slotDefinition.LocalPosition;

                Vector3 worldPosition = new Vector3(
                    boardCenter.x + local.x,
                    boardCenter.y + local.y,
                    boardCenter.z);

                GameObject instance = Instantiate(slotPrefab, worldPosition, Quaternion.identity, slotRoot);
                instance.name = slotDefinition.SlotId;

                PlinkoSlotRuntime runtime = instance.GetComponent<PlinkoSlotRuntime>();
                if (runtime != null)
                {
                    runtime.SetSlotIdForAuthoring(slotDefinition.SlotId);
                    runtime.SetBaseRewardForAuthoring(slotDefinition.BaseReward);
                    slotList.Add(runtime);
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                else
                {
                    Debug.LogWarning(
                        $"[{nameof(PlinkoBoardRuntimeGenerator)}] Slot prefab is missing {nameof(PlinkoSlotRuntime)}.",
                        this);
                }
#endif
            }

            generatedPins = pinList.ToArray();
            generatedSlots = slotList.ToArray();
        }

        /// <summary>
        /// 보드 정의의 메타데이터를 기준으로 핀/슬롯 정의를 다시 생성
        /// <remarks>
        /// 긴 줄은 Columns 개수, 짧은 줄은 Columns - 1 개수로 생성
        /// </remarks>
        /// </summary>
        [ContextMenu("Rebuild Definition Layout")]
        public void RebuildDefinitionLayout()
        {
            if (boardDefinition == null || layoutRoot == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning(
                    $"[{nameof(PlinkoBoardRuntimeGenerator)}] Missing board definition or layout root.",
                    this);
#endif
                return;
            }

            List<PlinkoPinDefinition> pinDefinitions = new List<PlinkoPinDefinition>(EstimatePinCount(boardDefinition.Rows, boardDefinition.Columns, boardDefinition.StaggerOddRows));

            List<PlinkoSlotDefinition> slotDefinitions = new List<PlinkoSlotDefinition>(boardDefinition.SlotCount);

            int longRowCount = Mathf.Max(2, boardDefinition.Columns);
            int shortRowCount = Mathf.Max(1, boardDefinition.Columns - 1);

            float longRowWidth = (longRowCount - 1) * boardDefinition.HorizontalSpacing;
            float centerOffsetY = (boardDefinition.Rows - 1) * boardDefinition.VerticalSpacing * 0.5f;

            for (int row = 0; row < boardDefinition.Rows; row++)
            {
                bool isShortRow = boardDefinition.StaggerOddRows && (row % 2 == 1);
                int rowPinCount = isShortRow ? shortRowCount : longRowCount;

                float rowWidth = (rowPinCount - 1) * boardDefinition.HorizontalSpacing;
                float rowCenterOffsetX = rowWidth * 0.5f;

                for (int col = 0; col < rowPinCount; col++)
                {
                    float x = (col * boardDefinition.HorizontalSpacing) - rowCenterOffsetX;
                    float y = centerOffsetY - (row * boardDefinition.VerticalSpacing) + boardDefinition.PinAreaYOffset;

                    string pinId = $"pin.r{row:00}.c{col:00}";
                    pinDefinitions.Add(new PlinkoPinDefinition(pinId, row, col, new Vector2(x, y)));
                }
            }

            float slotTotalWidth = (boardDefinition.SlotCount - 1) * boardDefinition.SlotSpacing;
            float slotCenterOffsetX = slotTotalWidth * 0.5f;
            float slotY = -centerOffsetY - (boardDefinition.Rows * boardDefinition.VerticalSpacing) + boardDefinition.SlotAreaYOffset;

            for (int i = 0; i < boardDefinition.SlotCount; i++)
            {
                float x = (i * boardDefinition.SlotSpacing) - slotCenterOffsetX;
                string slotId = $"slot.{i:00}";

                slotDefinitions.Add(new PlinkoSlotDefinition(
                    slotId,
                    i,
                    new Vector2(x, slotY),
                    5));
            }

            boardDefinition.ApplyAuthoringLayout(
                boardDefinition.Rows,
                boardDefinition.Columns,
                boardDefinition.SlotCount,
                boardDefinition.StaggerOddRows,
                boardDefinition.StaggerRatio,
                boardDefinition.PinAreaYOffset,
                boardDefinition.SlotAreaYOffset,
                pinDefinitions,
                slotDefinitions);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(boardDefinition);
            }
#endif
        }

        private bool ValidateRuntimeReferences()
        {
            bool valid =
                layoutRoot != null &&
                pinRoot != null &&
                slotRoot != null &&
                boardDefinition != null &&
                pinPrefab != null &&
                slotPrefab != null;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!valid)
            {
                Debug.LogWarning(
                    $"[{nameof(PlinkoBoardRuntimeGenerator)}] Missing required references.",
                    this);
            }
#endif

            if (valid && layoutRoot.BoardCenterAnchor == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning(
                    $"[{nameof(PlinkoBoardRuntimeGenerator)}] Layout root is missing BoardCenterAnchor.",
                    this);
#endif
                return false;
            }

            return valid;
        }

        private int EstimatePinCount(int rows, int columns, bool staggerOddRows)
        {
            int longRowCount = Mathf.Max(2, columns);
            int shortRowCount = Mathf.Max(1, columns - 1);

            int total = 0;
            for (int row = 0; row < rows; row++)
            {
                bool isShortRow = staggerOddRows && (row % 2 == 1);
                total += isShortRow ? shortRowCount : longRowCount;
            }

            return total;
        }

        private void ClearChildren(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(root.GetChild(i).gameObject);
                    continue;
                }
#endif
                Destroy(root.GetChild(i).gameObject);
            }
        }
    }
}