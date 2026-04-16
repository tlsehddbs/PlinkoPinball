using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    [Serializable]
    public struct PlinkoPinDefinition
    {
        [SerializeField] private string pinId;
        [SerializeField] private Vector2 localPosition;

        public string PinId => pinId;
        public Vector2 LocalPosition => localPosition;

        public PlinkoPinDefinition(string pinId, Vector2 localPosition)
        {
            this.pinId = pinId;
            this.localPosition = localPosition;
        }
    }

    [Serializable]
    public struct PlinkoSlotDefinition
    {
        [SerializeField] private string slotId;
        [SerializeField, Min(0)] private int baseReward;

        public string SlotId => slotId;
        public int BaseReward => baseReward;

        public PlinkoSlotDefinition(string slotId, int baseReward)
        {
            this.slotId = slotId;
            this.baseReward = baseReward;
        }
    }

    /// <summary>
    /// 플링코 보드의 고정 핀/슬롯 배치를 정의
    /// </summary>
    [CreateAssetMenu(fileName = "PlinkoBoardDefinition", menuName = "PlinkoPinball/Plinko/Board Definition")]
    public sealed class PlinkoBoardDefinition : ScriptableObject
    {
        [SerializeField] private List<PlinkoPinDefinition> pins = new List<PlinkoPinDefinition>();
        [SerializeField] private List<PlinkoSlotDefinition> slots = new List<PlinkoSlotDefinition>();

        public IReadOnlyList<PlinkoPinDefinition> Pins => pins;
        public IReadOnlyList<PlinkoSlotDefinition> Slots => slots;


        /// <summary>
        /// Authoring 단계에서 핀/슬롯 데이터를 통째로 교체
        /// remarks:
        /// 보드 자동 생성기에서 호출하는 전용 API
        /// </remarks>
        /// </summary>
        /// <param name="newPins">새 핀 정의 목록</param>
        /// <param name="newSlots">새 슬롯 정의 목록</param>
        public void ApplyAuthoringLayout(IReadOnlyList<PlinkoPinDefinition> newPins, IReadOnlyList<PlinkoSlotDefinition> newSlots)
        {
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