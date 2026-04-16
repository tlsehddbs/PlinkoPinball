using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Gameplay.Components.Plinko;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 적용 결과 스냅샷을 실제 핀/슬롯 인스턴스에 반영
    /// </summary>
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

        /// <summary>
        /// 모든 핀/슬롯 상태를 기본값으로 되돌림
        /// </summary>
        public void ResetBoardState()
        {
            for (int i = 0; i < pinRuntimes.Length; i++)
            {
                if (pinRuntimes[i] != null)
                {
                    pinRuntimes[i].ResetRuntimeState();
                }
            }

            for (int i = 0; i < slotRuntimes.Length; i++)
            {
                if (slotRuntimes[i] != null)
                {
                    slotRuntimes[i].ResetRuntimeState();
                }
            }
        }

        /// <summary>
        /// 적용 결과 스냅샷을 핀/슬롯 인스턴스에 주입
        /// </summary>
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
        }

        private void BuildLookup()
        {
            _pinLookup = new Dictionary<string, PlinkoPinRuntime>(pinRuntimes.Length);
            _slotLookup = new Dictionary<string, PlinkoSlotRuntime>(slotRuntimes.Length);

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
}