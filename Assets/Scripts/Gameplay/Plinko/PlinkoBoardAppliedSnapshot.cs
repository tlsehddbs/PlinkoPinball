using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    [Serializable]
    public struct PlinkoPinStateSnapshot
    {
        [SerializeField] private string pinId;
        [SerializeField] private int flatCurrencyBonus;
        [SerializeField] private float hitMultiplier;
        [SerializeField] private int extraBounceReward;

        public string PinId => pinId;
        public int FlatCurrencyBonus => flatCurrencyBonus;
        public float HitMultiplier => hitMultiplier;
        public int ExtraBounceReward => extraBounceReward;

        public PlinkoPinStateSnapshot(string pinId, int flatCurrencyBonus, float hitMultiplier, int extraBounceReward)
        {
            this.pinId = pinId;
            this.flatCurrencyBonus = flatCurrencyBonus;
            this.hitMultiplier = hitMultiplier;
            this.extraBounceReward = extraBounceReward;
        }
    }

    [Serializable]
    public struct PlinkoSlotStateSnapshot
    {
        [SerializeField] private string slotId;
        [SerializeField] private int flatCurrencyBonus;
        [SerializeField] private float jackpotMultiplier;

        public string SlotId => slotId;
        public int FlatCurrencyBonus => flatCurrencyBonus;
        public float JackpotMultiplier => jackpotMultiplier;

        public PlinkoSlotStateSnapshot(string slotId, int flatCurrencyBonus, float jackpotMultiplier)
        {
            this.slotId = slotId;
            this.flatCurrencyBonus = flatCurrencyBonus;
            this.jackpotMultiplier = jackpotMultiplier;
        }
    }

    /// <summary>
    /// 플링코 시작 직전에 확정된 보드 적용 결과
    /// </summary>
    public readonly struct PlinkoBoardAppliedSnapshot
    {
        public readonly int StartBalls;
        public readonly int GlobalCurrencyPerPinHit;
        public readonly IReadOnlyList<PlinkoPinStateSnapshot> PinStates;
        public readonly IReadOnlyList<PlinkoSlotStateSnapshot> SlotStates;

        public PlinkoBoardAppliedSnapshot(int startBalls, int globalCurrencyPerPinHit, IReadOnlyList<PlinkoPinStateSnapshot> pinStates, IReadOnlyList<PlinkoSlotStateSnapshot> slotStates)
        {
            StartBalls = startBalls;
            GlobalCurrencyPerPinHit = globalCurrencyPerPinHit;
            PinStates = pinStates;
            SlotStates = slotStates;
        }
    }
}