using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    public enum PlinkoPinStateKind
    {
        Normal,
        Boosted,
        Error
    }

    public enum PlinkoSlotStateKind
    {
        Normal,
        Boosted,
        Error
    }


    [Serializable]
    public readonly struct PlinkoPinStateSnapshot
    {
        public readonly string PinId;
        public readonly PlinkoPinStateKind StateKind;
        public readonly int ValueBonus;
        public readonly float Multiplier;

        public PlinkoPinStateSnapshot(string pinId, PlinkoPinStateKind stateKind, int valueBonus, float multiplier)
        {
            PinId = pinId;
            StateKind = stateKind;
            ValueBonus = Mathf.Max(0, valueBonus);
            Multiplier = Mathf.Max(0f, multiplier);
        }
    }

    [Serializable]
    public readonly struct PlinkoSlotStateSnapshot
    {
        public readonly string SlotId;
        public readonly PlinkoSlotStateKind StateKind;
        public readonly int ValueBonus;
        public readonly float Multiplier;

        public PlinkoSlotStateSnapshot(string slotId, PlinkoSlotStateKind stateKind, int valueBonus, float multiplier)
        {
            SlotId = slotId;
            StateKind = stateKind;
            ValueBonus = Mathf.Max(0, valueBonus);
            Multiplier = Mathf.Max(0f, multiplier);
        }
    }

    /// <summary>
    /// 플링코 시작 직전에 확정된 보드 적용 결과
    /// </summary>
    public readonly struct PlinkoBoardAppliedSnapshot
    {
        public readonly int StartBalls;
        public readonly float GlobalPinMultiplier;
        public readonly float GlobalSlotMultiplier;
        public readonly IReadOnlyList<PlinkoPinStateSnapshot> PinStates;
        public readonly IReadOnlyList<PlinkoSlotStateSnapshot> SlotStates;

        public PlinkoBoardAppliedSnapshot(int startBalls, float globalPinMultiplier, float globalSlotMultiplier, IReadOnlyList<PlinkoPinStateSnapshot> pinStates, IReadOnlyList<PlinkoSlotStateSnapshot> slotStates)
        {
            StartBalls = Mathf.Max(0, startBalls);
            GlobalPinMultiplier = Mathf.Max(0f, globalPinMultiplier);
            GlobalSlotMultiplier = Mathf.Max(0f, globalSlotMultiplier);
            PinStates = pinStates;
            SlotStates = slotStates;
        }
    }
}