using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlinkoPinball.Gameplay.Components.Plinko;
using System;

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
            if (pinRuntimes != null)
            {
                for (int i = 0; i < pinRuntimes.Length; i++)
                {
                    pinRuntimes[i]?.ResetRuntimeState();
                }
            }

            if (slotRuntimes != null)
            {
                for (int i = 0; i < slotRuntimes.Length; i++)
                {
                    slotRuntimes[i]?.ResetRuntimeState();
                }
            }
        }

        public void ApplySnapshot(in PlinkoBoardAppliedSnapshot snapshot)
        {
            ResetBoardState();

            if (snapshot.PinStates != null)
            {
                for (int i = 0; i < snapshot.PinStates.Count; i++)
                {
                    PlinkoPinStateSnapshot pinState = snapshot.PinStates[i];

                    if (_pinLookup.TryGetValue(pinState.PinId, out PlinkoPinRuntime runtime))
                    {
                        float finalMultiplier = pinState.Multiplier * Mathf.Max(0f, snapshot.GlobalPinMultiplier);
                        runtime.ApplyState(pinState.StateKind, pinState.ValueBonus, finalMultiplier);
                    }
                }
            }

            if (snapshot.SlotStates != null)
            {
                for (int i = 0; i < snapshot.SlotStates.Count; i++)
                {
                    PlinkoSlotStateSnapshot slotState = snapshot.SlotStates[i];

                    if (_slotLookup.TryGetValue(slotState.SlotId, out PlinkoSlotRuntime runtime))
                    {
                        float finalMultiplier = slotState.Multiplier * Mathf.Max(0f, snapshot.GlobalSlotMultiplier);
                        runtime.ApplyState(slotState.StateKind, slotState.ValueBonus, finalMultiplier);
                    }
                }
            }

            RefreshVisualReactions();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogAppliedRuntimeState();
#endif
        }

        private void RefreshVisualReactions()
        {
            PlinkoPinColorReaction[] reactions = FindObjectsByType<PlinkoPinColorReaction>(FindObjectsSortMode.None);

            for (int i = 0; i < reactions.Length; i++)
            {
                if (reactions[i] != null)
                {
                    reactions[i].Refresh();
                }
            }

            PlinkoSlotColorReaction[] slotReactions = FindObjectsByType<PlinkoSlotColorReaction>(FindObjectsSortMode.None);

            for (int i = 0; i < slotReactions.Length; i++)
            {
                if (slotReactions[i] != null)
                {
                    slotReactions[i].Refresh();
                }
            }
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

                    if (data.StateKind == PlinkoPinStateKind.Normal && data.ValueBonus == 0 && Mathf.Approximately(data.Multiplier, 1f))
                    {
                        continue;
                    }
                    sb.AppendLine($"  Pin {pinRuntimes[i].PinId} => state={data.StateKind}, valueBonus={data.ValueBonus}, multiplier={data.Multiplier}");
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
                    if (data.StateKind == PlinkoSlotStateKind.Normal && data.ValueBonus == 0 && Mathf.Approximately(data.Multiplier, 1f))
                    {
                        continue;
                    }

                    sb.AppendLine($"  Slot {slotRuntimes[i].SlotId} => state={data.StateKind}, valueBonus={data.ValueBonus}, multiplier={data.Multiplier}");
                }
            }

            Debug.Log(sb.ToString(), this);
        }
#endif
    }
}
