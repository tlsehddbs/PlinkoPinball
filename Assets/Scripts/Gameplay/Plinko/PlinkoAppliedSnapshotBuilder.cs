using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlinkoPinball.Gameplay.Components.Plinko;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    public static class PlinkoAppliedSnapshotBuilder
    {
        public static PlinkoBoardAppliedSnapshot Build(PlinkoPinRuntime[] pinRuntimes, PlinkoSlotRuntime[] slotRuntimes, in PlinkoRunSnapshot runSnapshot)
        {
            var rng = new System.Random(runSnapshot.BoardSeed);

            List<PlinkoPinStateSnapshot> pinStates = CreateDefaultPinStates(pinRuntimes);
            List<PlinkoSlotStateSnapshot> slotStates = CreateDefaultSlotStates(slotRuntimes);

            ApplyTokens(runSnapshot.Tokens, pinStates, slotStates, rng);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogAppliedState(runSnapshot, pinStates, slotStates);
#endif

            return new PlinkoBoardAppliedSnapshot(
                runSnapshot.StartBalls,
                runSnapshot.GlobalCurrencyPerPinHit,
                pinStates,
                slotStates);
        }

        private static List<PlinkoPinStateSnapshot> CreateDefaultPinStates(PlinkoPinRuntime[] pinRuntimes)
        {
            var result = new List<PlinkoPinStateSnapshot>(pinRuntimes != null ? pinRuntimes.Length : 0);

            if (pinRuntimes == null)
            {
                return result;
            }

            for (int i = 0; i < pinRuntimes.Length; i++)
            {
                if (pinRuntimes[i] == null || string.IsNullOrWhiteSpace(pinRuntimes[i].PinId))
                {
                    continue;
                }

                result.Add(new PlinkoPinStateSnapshot(
                    pinRuntimes[i].PinId,
                    0,
                    1f,
                    0));
            }

            return result;
        }

        private static List<PlinkoSlotStateSnapshot> CreateDefaultSlotStates(PlinkoSlotRuntime[] slotRuntimes)
        {
            var result = new List<PlinkoSlotStateSnapshot>(slotRuntimes != null ? slotRuntimes.Length : 0);

            if (slotRuntimes == null)
            {
                return result;
            }

            for (int i = 0; i < slotRuntimes.Length; i++)
            {
                if (slotRuntimes[i] == null || string.IsNullOrWhiteSpace(slotRuntimes[i].SlotId))
                {
                    continue;
                }

                result.Add(new PlinkoSlotStateSnapshot(
                    slotRuntimes[i].SlotId,
                    0,
                    1f));
            }

            return result;
        }

        private static void ApplyTokens(
            IReadOnlyList<PlinkoBonusToken> tokens,
            List<PlinkoPinStateSnapshot> pinStates,
            List<PlinkoSlotStateSnapshot> slotStates,
            System.Random rng)
        {
            if (tokens == null)
            {
                return;
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                PlinkoBonusToken token = tokens[i];

                for (int stack = 0; stack < token.StackCount; stack++)
                {
                    switch (token.TokenType)
                    {
                        case PlinkoBonusTokenType.RandomPinFlatCurrencyBonus:
                            ApplyRandomPinFlatCurrency(pinStates, token.Amount, rng);
                            break;

                        case PlinkoBonusTokenType.RandomPinHitMultiplier:
                            ApplyRandomPinHitMultiplier(pinStates, token.Amount, rng);
                            break;

                        case PlinkoBonusTokenType.RandomPinExtraBounceReward:
                            ApplyRandomPinExtraBounceReward(pinStates, token.Amount, rng);
                            break;

                        case PlinkoBonusTokenType.RandomSlotFlatCurrencyBonus:
                            ApplyRandomSlotFlatCurrency(slotStates, token.Amount, rng);
                            break;

                        case PlinkoBonusTokenType.RandomSlotJackpotMultiplier:
                            ApplyRandomSlotJackpotMultiplier(slotStates, token.Amount, rng);
                            break;
                    }
                }
            }
        }

        private static void ApplyRandomPinFlatCurrency(List<PlinkoPinStateSnapshot> pinStates, int amount, System.Random rng)
        {
            if (pinStates.Count == 0) return;

            int index = rng.Next(0, pinStates.Count);
            PlinkoPinStateSnapshot state = pinStates[index];

            pinStates[index] = new PlinkoPinStateSnapshot(
                state.PinId,
                state.FlatCurrencyBonus + amount,
                state.HitMultiplier,
                state.ExtraBounceReward);
        }

        private static void ApplyRandomPinHitMultiplier(List<PlinkoPinStateSnapshot> pinStates, int amount, System.Random rng)
        {
            if (pinStates.Count == 0) return;

            int index = rng.Next(0, pinStates.Count);
            PlinkoPinStateSnapshot state = pinStates[index];

            pinStates[index] = new PlinkoPinStateSnapshot(
                state.PinId,
                state.FlatCurrencyBonus,
                state.HitMultiplier + amount,
                state.ExtraBounceReward);
        }

        private static void ApplyRandomPinExtraBounceReward(List<PlinkoPinStateSnapshot> pinStates, int amount, System.Random rng)
        {
            if (pinStates.Count == 0) return;

            int index = rng.Next(0, pinStates.Count);
            PlinkoPinStateSnapshot state = pinStates[index];

            pinStates[index] = new PlinkoPinStateSnapshot(
                state.PinId,
                state.FlatCurrencyBonus,
                state.HitMultiplier,
                state.ExtraBounceReward + amount);
        }

        private static void ApplyRandomSlotFlatCurrency(List<PlinkoSlotStateSnapshot> slotStates, int amount, System.Random rng)
        {
            if (slotStates.Count == 0) return;

            int index = rng.Next(0, slotStates.Count);
            PlinkoSlotStateSnapshot state = slotStates[index];

            slotStates[index] = new PlinkoSlotStateSnapshot(
                state.SlotId,
                state.FlatCurrencyBonus + amount,
                state.JackpotMultiplier);
        }

        private static void ApplyRandomSlotJackpotMultiplier(List<PlinkoSlotStateSnapshot> slotStates, int amount, System.Random rng)
        {
            if (slotStates.Count == 0) return;

            int index = rng.Next(0, slotStates.Count);
            PlinkoSlotStateSnapshot state = slotStates[index];

            slotStates[index] = new PlinkoSlotStateSnapshot(
                state.SlotId,
                state.FlatCurrencyBonus,
                state.JackpotMultiplier + amount);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static void LogAppliedState(
            in PlinkoRunSnapshot runSnapshot,
            List<PlinkoPinStateSnapshot> pinStates,
            List<PlinkoSlotStateSnapshot> slotStates)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[PlinkoAppliedSnapshotBuilder] Applied Snapshot");
            sb.AppendLine($"  Seed = {runSnapshot.BoardSeed}");
            sb.AppendLine($"  StartBalls = {runSnapshot.StartBalls}");
            sb.AppendLine($"  GlobalCurrencyPerPinHit = {runSnapshot.GlobalCurrencyPerPinHit}");

            for (int i = 0; i < pinStates.Count; i++)
            {
                PlinkoPinStateSnapshot pin = pinStates[i];
                if (pin.FlatCurrencyBonus == 0 &&
                    Mathf.Approximately(pin.HitMultiplier, 1f) &&
                    pin.ExtraBounceReward == 0)
                {
                    continue;
                }

                sb.AppendLine($"  Pin {pin.PinId} => flat={pin.FlatCurrencyBonus}, mul={pin.HitMultiplier}, bounce={pin.ExtraBounceReward}");
            }

            for (int i = 0; i < slotStates.Count; i++)
            {
                PlinkoSlotStateSnapshot slot = slotStates[i];
                if (slot.FlatCurrencyBonus == 0 &&
                    Mathf.Approximately(slot.JackpotMultiplier, 1f))
                {
                    continue;
                }

                sb.AppendLine($"  Slot {slot.SlotId} => flat={slot.FlatCurrencyBonus}, jackpot={slot.JackpotMultiplier}");
            }

            Debug.Log(sb.ToString());
        }
#endif
    }
}