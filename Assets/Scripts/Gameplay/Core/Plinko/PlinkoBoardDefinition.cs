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
    }

    [Serializable]
    public struct PlinkoSlotDefinition
    {
        [SerializeField] private string slotId;
        [SerializeField, Min(0)] private int baseReward;

        public string SlotId => slotId;
        public int BaseReward => baseReward;
    }

    /// <summary>
    /// 플링코 보드의 고정 핀/슬롯 배치를 정의
    /// </summary>
    [CreateAssetMenu(
        fileName = "PlinkoBoardDefinition",
        menuName = "PlinkoPinball/Plinko/Board Definition")]
    public sealed class PlinkoBoardDefinition : ScriptableObject
    {
        [SerializeField] private List<PlinkoPinDefinition> pins = new List<PlinkoPinDefinition>();
        [SerializeField] private List<PlinkoSlotDefinition> slots = new List<PlinkoSlotDefinition>();

        public IReadOnlyList<PlinkoPinDefinition> Pins => pins;
        public IReadOnlyList<PlinkoSlotDefinition> Slots => slots;
    }
}