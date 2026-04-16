using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Gameplay.Components.Plinko;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 보드를 정식으로 생성하는 Authoring 컴포넌트
    /// 핀/슬롯 프리팹 생성, ID 부여, 레이아웃 앵커 갱신, 보드 데이터 동기화를 담당
    /// remarks:
    /// 이 컴포넌트는 에디터 작성 도구로 사용
    /// </remarks>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlinkoBoardAuthoring : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlinkoBoardLayoutRoot layoutRoot;
        [SerializeField] private Transform pinRoot;
        [SerializeField] private Transform slotRoot;
        [SerializeField] private PlinkoBoardDefinition boardDefinition;

        [Header("Prefabs")]
        [SerializeField] private GameObject pinPrefab;
        [SerializeField] private GameObject slotPrefab;

        [Header("Pin Layout")]
        [SerializeField, Min(1)] private int rows = 8;
        [SerializeField, Min(1)] private int columns = 13;
        [SerializeField, Min(0.1f)] private float horizontalSpacing = 1f;
        [SerializeField, Min(0.1f)] private float verticalSpacing = 1.2f;
        [SerializeField] private bool staggerOddRows = true;
        [SerializeField, Min(0f)] private float staggerOffsetRatio = 0.5f;
        [SerializeField] private float[] customRowOffsets;

        [Header("Slot Layout")]
        [SerializeField] private bool generateSlots = true;
        [SerializeField, Min(1)] private int slotCount = 10;
        [SerializeField, Min(0.1f)] private float slotSpacing = 1.2f;
        [SerializeField] private int[] slotBaseRewards;

        [Header("Board Bounds")]
        [SerializeField, Min(0f)] private float sidePadding = 0.6f;
        [SerializeField, Min(0f)] private float topPadding = 0.8f;
        [SerializeField, Min(0f)] private float bottomPadding = 0.8f;
        [SerializeField, Min(0f)] private float spawnYOffset = 0.5f;
        [SerializeField, Min(0f)] private float slotStripYOffset = 0.0f;

        [Header("Pin Area Offset")]
        [SerializeField] private float pinAreaYOffset = -1.0f;
        [SerializeField] private float slotAreaYOffset = -0.6f;

        /// <summary>
        /// 현재 설정을 기준으로 핀과 슬롯을 다시 생성
        /// </summary>
        [ContextMenu("Rebuild Board")]
        public void RebuildBoard()
        {
            if (pinRoot == null || pinPrefab == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[{nameof(PlinkoBoardAuthoring)}] Pin root or pin prefab is missing.", this);
#endif
                return;
            }

            ClearChildren(pinRoot);

            if (slotRoot != null)
            {
                ClearChildren(slotRoot);
            }

            List<PlinkoPinDefinition> generatedPins = GeneratePins();
            List<PlinkoSlotDefinition> generatedSlots = generateSlots ? GenerateSlots() : new List<PlinkoSlotDefinition>();

            UpdateLayoutAnchors(generatedPins, generatedSlots);
            SyncBoardDefinition(generatedPins, generatedSlots);
        }

        /// <summary>
        /// 현재 루트 하위의 핀과 슬롯을 제거
        /// </summary>
        [ContextMenu("Clear Board")]
        public void ClearBoard()
        {
            if (pinRoot != null)
            {
                ClearChildren(pinRoot);
            }

            if (slotRoot != null)
            {
                ClearChildren(slotRoot);
            }

            if (boardDefinition != null)
            {
                boardDefinition.ApplyAuthoringLayout(null, null);
                MarkDirty(boardDefinition);
            }
        }

        private List<PlinkoPinDefinition> GeneratePins()
        {
            var generatedPins = new List<PlinkoPinDefinition>(rows * columns);

            float baseWidth = (columns - 1) * horizontalSpacing;
            float centerOffsetX = baseWidth * 0.5f;

            float totalHeight = (rows - 1) * verticalSpacing;
            float centerOffsetY = totalHeight * 0.5f;

            for (int row = 0; row < rows; row++)
            {
                float rowOffset = GetRowOffset(row);

                for (int col = 0; col < columns; col++)
                {
                    float x = (col * horizontalSpacing) - centerOffsetX + rowOffset;
                    float y = centerOffsetY - (row * verticalSpacing) + pinAreaYOffset;

                    Vector3 localPosition = new Vector3(x, y, 0f);

                    GameObject instance = InstantiateForAuthoring(pinPrefab, pinRoot);
                    instance.name = $"Pin_{row:00}_{col:00}";
                    instance.transform.localPosition = localPosition;
                    //instance.transform.localRotation = Quaternion.identity;

                    string pinId = $"pin.r{row:00}.c{col:00}";
                    PlinkoPinRuntime runtime = instance.GetComponent<PlinkoPinRuntime>();
                    if (runtime != null)
                    {
                        runtime.SetPinIdForAuthoring(pinId);
                    }

                    generatedPins.Add(new PlinkoPinDefinition(pinId, new Vector2(localPosition.x, localPosition.y)));
                }
            }

            return generatedPins;
        }

        private List<PlinkoSlotDefinition> GenerateSlots()
        {
            var generatedSlots = new List<PlinkoSlotDefinition>(slotCount);

            if (slotRoot == null || slotPrefab == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[{nameof(PlinkoBoardAuthoring)}] Slot root or slot prefab is missing.", this);
#endif
                return generatedSlots;
            }

            float totalWidth = (slotCount - 1) * slotSpacing;
            float centerOffsetX = totalWidth * 0.5f;

            float totalHeight = (rows - 1) * verticalSpacing;
            float centerOffsetY = totalHeight * 0.5f;

            float y = centerOffsetY - (rows * verticalSpacing) + slotAreaYOffset;

            for (int i = 0; i < slotCount; i++)
            {
                float x = (i * slotSpacing) - centerOffsetX;
                Vector3 localPosition = new Vector3(x, y, 0f);

                GameObject instance = InstantiateForAuthoring(slotPrefab, slotRoot);
                instance.name = $"Slot_{i:00}";
                instance.transform.localPosition = localPosition;
                instance.transform.localRotation = Quaternion.identity;

                string slotId = $"slot.{i:00}";
                int baseReward = GetSlotBaseReward(i);

                PlinkoSlotRuntime runtime = instance.GetComponent<PlinkoSlotRuntime>();
                if (runtime != null)
                {
                    runtime.SetSlotIdForAuthoring(slotId);
                    runtime.SetBaseRewardForAuthoring(baseReward);
                }

                generatedSlots.Add(new PlinkoSlotDefinition(slotId, baseReward));
            }

            return generatedSlots;
        }

        private void UpdateLayoutAnchors(IReadOnlyList<PlinkoPinDefinition> generatedPins, IReadOnlyList<PlinkoSlotDefinition> generatedSlots)
        {
            if (layoutRoot == null || generatedPins == null || generatedPins.Count == 0)
            {
                return;
            }

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float minY = float.MaxValue;

            for (int i = 0; i < generatedPins.Count; i++)
            {
                Vector2 position = generatedPins[i].LocalPosition;
                minX = Mathf.Min(minX, position.x);
                maxX = Mathf.Max(maxX, position.x);
                maxY = Mathf.Max(maxY, position.y);
                minY = Mathf.Min(minY, position.y);
            }

            if (generatedSlots != null && generatedSlots.Count > 0 && slotRoot != null)
            {
                for (int i = 0; i < slotRoot.childCount; i++)
                {
                    Vector3 slotPosition = slotRoot.GetChild(i).localPosition;
                    minX = Mathf.Min(minX, slotPosition.x);
                    maxX = Mathf.Max(maxX, slotPosition.x);
                    minY = Mathf.Min(minY, slotPosition.y);
                }
            }

            layoutRoot.ApplyAuthoringBounds(
                minX - sidePadding,
                maxX + sidePadding,
                maxY + topPadding,
                minY - bottomPadding,
                spawnYOffset,
                slotStripYOffset);
        }

        private void SyncBoardDefinition(IReadOnlyList<PlinkoPinDefinition> generatedPins, IReadOnlyList<PlinkoSlotDefinition> generatedSlots)
        {
            if (boardDefinition == null)
            {
                return;
            }

            boardDefinition.ApplyAuthoringLayout(generatedPins, generatedSlots);
            MarkDirty(boardDefinition);
        }

        private float GetRowOffset(int row)
        {
            if (customRowOffsets != null && row < customRowOffsets.Length)
            {
                return customRowOffsets[row];
            }

            if (staggerOddRows && (row % 2 == 1))
            {
                return horizontalSpacing * staggerOffsetRatio;
            }

            return 0f;
        }

        private int GetSlotBaseReward(int index)
        {
            if (slotBaseRewards != null && index < slotBaseRewards.Length)
            {
                return Mathf.Max(0, slotBaseRewards[index]);
            }

            return 5;
        }

        private GameObject InstantiateForAuthoring(GameObject prefab, Transform parent)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
                if (instance != null)
                {
                    return instance;
                }
            }
#endif
            return Instantiate(prefab, parent);
        }

        private void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                SafeDestroy(root.GetChild(i).gameObject);
            }
        }

        private void SafeDestroy(GameObject target)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(target);
                return;
            }
#endif
            Destroy(target);
        }

        private void MarkDirty(Object target)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && target != null)
            {
                EditorUtility.SetDirty(target);
            }
#endif
        }
    }
}