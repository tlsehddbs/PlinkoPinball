using System;
using System.Collections.Generic;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 핀볼에서 전달된 런 스냅샷을 실제 핀/슬롯 적용 결과로 해석
    /// </summary>
    public static class PlinkoAppliedSnapshotBuilder
    {
        public static PlinkoBoardAppliedSnapshot Build(PlinkoBoardDefinition boardDefinition, in PlinkoRunSnapshot runSnapshot)
        {
            var rng = new Random(runSnapshot.BoardSeed);

            var pinStates = CreateDefaultPinStates(boardDefinition);
            var slotStates = CreateDefaultSlotStates(boardDefinition);

            ApplyTokens(runSnapshot.Tokens, pinStates, slotStates, rng);

            return new PlinkoBoardAppliedSnapshot(
                runSnapshot.StartBalls,
                runSnapshot.GlobalCurrencyPerPinHit,
                pinStates,
                slotStates);
        }

        private static List<PlinkoPinStateSnapshot> CreateDefaultPinStates(PlinkoBoardDefinition boardDefinition)
        {
            var result = new List<PlinkoPinStateSnapshot>(boardDefinition.Pins.Count);

            for (int i = 0; i < boardDefinition.Pins.Count; i++)
            {
                result.Add(new PlinkoPinStateSnapshot(
                    boardDefinition.Pins[i].PinId,
                    0,
                    1f,
                    0));
            }

            return result;
        }

        private static List<PlinkoSlotStateSnapshot> CreateDefaultSlotStates(PlinkoBoardDefinition boardDefinition)
        {
            var result = new List<PlinkoSlotStateSnapshot>(boardDefinition.Slots.Count);

            for (int i = 0; i < boardDefinition.Slots.Count; i++)
            {
                result.Add(new PlinkoSlotStateSnapshot(
                    boardDefinition.Slots[i].SlotId,
                    0,
                    1f));
            }

            return result;
        }

        private static void ApplyTokens(IReadOnlyList<PlinkoBonusToken> tokens, List<PlinkoPinStateSnapshot> pinStates, List<PlinkoSlotStateSnapshot> slotStates, Random rng)
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

        private static void ApplyRandomPinFlatCurrency(List<PlinkoPinStateSnapshot> pinStates, int amount, Random rng)
        {
            if (pinStates.Count == 0)
            {
                return;
            }

            int index = rng.Next(0, pinStates.Count);
            PlinkoPinStateSnapshot state = pinStates[index];

            pinStates[index] = new PlinkoPinStateSnapshot(
                state.PinId,
                state.FlatCurrencyBonus + amount,
                state.HitMultiplier,
                state.ExtraBounceReward);
        }

        private static void ApplyRandomPinHitMultiplier(List<PlinkoPinStateSnapshot> pinStates, int amount, Random rng)
        {
            if (pinStates.Count == 0)
            {
                return;
            }

            int index = rng.Next(0, pinStates.Count);
            PlinkoPinStateSnapshot state = pinStates[index];

            pinStates[index] = new PlinkoPinStateSnapshot(
                state.PinId,
                state.FlatCurrencyBonus,
                state.HitMultiplier + amount,
                state.ExtraBounceReward);
        }

        private static void ApplyRandomPinExtraBounceReward(List<PlinkoPinStateSnapshot> pinStates, int amount, Random rng)
        {
            if (pinStates.Count == 0)
            {
                return;
            }

            int index = rng.Next(0, pinStates.Count);
            PlinkoPinStateSnapshot state = pinStates[index];

            pinStates[index] = new PlinkoPinStateSnapshot(
                state.PinId,
                state.FlatCurrencyBonus,
                state.HitMultiplier,
                state.ExtraBounceReward + amount);
        }

        private static void ApplyRandomSlotFlatCurrency(List<PlinkoSlotStateSnapshot> slotStates, int amount, Random rng)
        {
            if (slotStates.Count == 0)
            {
                return;
            }

            int index = rng.Next(0, slotStates.Count);
            PlinkoSlotStateSnapshot state = slotStates[index];

            slotStates[index] = new PlinkoSlotStateSnapshot(
                state.SlotId,
                state.FlatCurrencyBonus + amount,
                state.JackpotMultiplier);
        }

        private static void ApplyRandomSlotJackpotMultiplier(List<PlinkoSlotStateSnapshot> slotStates, int amount, Random rng)
        {
            if (slotStates.Count == 0)
            {
                return;
            }

            int index = rng.Next(0, slotStates.Count);
            PlinkoSlotStateSnapshot state = slotStates[index];

            slotStates[index] = new PlinkoSlotStateSnapshot(
                state.SlotId,
                state.FlatCurrencyBonus,
                state.JackpotMultiplier + amount);
        }
    }
}