using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlinkoPinball.Gameplay.Components.Plinko;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    public sealed class PlinkoBoardStateApplier : MonoBehaviour
    {
        [SerializeField] private PlinkoPinRuntime[] pinRuntimes;
        [SerializeField] private PlinkoSlotRuntime[] slotRuntimes;

        private Dictionary<string, PlinkoPinRuntime> _pinLookup;
        private Dictionary<string, PlinkoSlotRuntime> _slotLookup;

        private void Awake()
        {
            BuildLookup();
        }

        public void RegisterRuntimeObjects(PlinkoPinRuntime[] pins, PlinkoSlotRuntime[] slots)
        {
            pinRuntimes = pins ?? System.Array.Empty<PlinkoPinRuntime>();
            slotRuntimes = slots ?? System.Array.Empty<PlinkoSlotRuntime>();
            BuildLookup();
        }

        public void ResetBoardState()
        {
            for (int i = 0; i < pinRuntimes.Length; i++)
            {
                pinRuntimes[i]?.ResetRuntimeState();
            }

            for (int i = 0; i < slotRuntimes.Length; i++)
            {
                slotRuntimes[i]?.ResetRuntimeState();
            }
        }

        public void ApplySnapshot(in PlinkoBoardAppliedSnapshot snapshot)
        {
            for (int i = 0; i < snapshot.PinStates.Count; i++)
            {
                PlinkoPinStateSnapshot pinState = snapshot.PinStates[i];
                if (_pinLookup.TryGetValue(pinState.PinId, out PlinkoPinRuntime runtime))
                {
                    runtime.ApplyState(pinState.FlatCurrencyBonus, pinState.HitMultiplier, pinState.ExtraBounceReward);
                }
            }

            for (int i = 0; i < snapshot.SlotStates.Count; i++)
            {
                PlinkoSlotStateSnapshot slotState = snapshot.SlotStates[i];
                if (_slotLookup.TryGetValue(slotState.SlotId, out PlinkoSlotRuntime runtime))
                {
                    runtime.ApplyState(slotState.FlatCurrencyBonus, slotState.JackpotMultiplier);
                }
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogAppliedRuntimeState();
#endif
        }

        private void BuildLookup()
        {
            _pinLookup = new Dictionary<string, PlinkoPinRuntime>(pinRuntimes != null ? pinRuntimes.Length : 0);
            _slotLookup = new Dictionary<string, PlinkoSlotRuntime>(slotRuntimes != null ? slotRuntimes.Length : 0);

            if (pinRuntimes != null)
            {
                for (int i = 0; i < pinRuntimes.Length; i++)
                {
                    if (pinRuntimes[i] == null || string.IsNullOrWhiteSpace(pinRuntimes[i].PinId))
                    {
                        continue;
                    }

                    if (!_pinLookup.ContainsKey(pinRuntimes[i].PinId))
                    {
                        _pinLookup.Add(pinRuntimes[i].PinId, pinRuntimes[i]);
                    }
                }
            }

            if (slotRuntimes != null)
            {
                for (int i = 0; i < slotRuntimes.Length; i++)
                {
                    if (slotRuntimes[i] == null || string.IsNullOrWhiteSpace(slotRuntimes[i].SlotId))
                    {
                        continue;
                    }

                    if (!_slotLookup.ContainsKey(slotRuntimes[i].SlotId))
                    {
                        _slotLookup.Add(slotRuntimes[i].SlotId, slotRuntimes[i]);
                    }
                }
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void LogAppliedRuntimeState()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[PlinkoBoardStateApplier] Runtime State");

            if (pinRuntimes != null)
            {
                for (int i = 0; i < pinRuntimes.Length; i++)
                {
                    if (pinRuntimes[i] == null)
                    {
                        continue;
                    }

                    PlinkoPinModifierData data = pinRuntimes[i].ExportState();
                    if (data.FlatCurrencyBonus == 0 &&
                        Mathf.Approximately(data.HitMultiplier, 1f) &&
                        data.ExtraBounceReward == 0)
                    {
                        continue;
                    }

                    sb.AppendLine($"  Pin {pinRuntimes[i].PinId} => flat={data.FlatCurrencyBonus}, mul={data.HitMultiplier}, bounce={data.ExtraBounceReward}");
                }
            }

            if (slotRuntimes != null)
            {
                for (int i = 0; i < slotRuntimes.Length; i++)
                {
                    if (slotRuntimes[i] == null)
                    {
                        continue;
                    }

                    PlinkoSlotModifierData data = slotRuntimes[i].ExportState();
                    if (data.FlatCurrencyBonus == 0 && Mathf.Approximately(data.JackpotMultiplier, 1f))
                    {
                        continue;
                    }

                    sb.AppendLine($"  Slot {slotRuntimes[i].SlotId} => flat={data.FlatCurrencyBonus}, jackpot={data.JackpotMultiplier}");
                }
            }

            Debug.Log(sb.ToString(), this);
        }
#endif
    }
}