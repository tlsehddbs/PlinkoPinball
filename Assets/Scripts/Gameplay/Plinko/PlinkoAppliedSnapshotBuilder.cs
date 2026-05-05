using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlinkoPinball.Gameplay.Components.Plinko;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    public static class PlinkoAppliedSnapshotBuilder
    {
        //TODO: 임시로 여기에 구현, 추후 별개 설정 패널을 만들어 통합 관리 예정
        private const float BaseErrorPinRate = 0.5f;
        private const float BaseErrorSlotRate = 0.25f;


        public static PlinkoBoardAppliedSnapshot Build(PlinkoPinRuntime[] pinRuntimes, PlinkoSlotRuntime[] slotRuntimes, in PlinkoRunSnapshot runSnapshot)
        {
            var rng = new System.Random(runSnapshot.BoardSeed);

            List<PlinkoPinStateSnapshot> pinStates = CreateDefaultPinStates(pinRuntimes);
            List<PlinkoSlotStateSnapshot> slotStates = CreateDefaultSlotStates(slotRuntimes);

            ApplyErrorStates(pinStates, slotStates, runSnapshot.ErrorPinRateReduction, runSnapshot.ErrorSlotRateReduction, rng);
            ApplyTokens(runSnapshot.Tokens, pinStates, slotStates, rng);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogAppliedState(runSnapshot, pinStates, slotStates);
#endif

            return new PlinkoBoardAppliedSnapshot(
                runSnapshot.StartBalls,
                runSnapshot.GlobalPinMultiplier,
                runSnapshot.GlobalSlotMultiplier,
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

                result.Add(new PlinkoPinStateSnapshot(pinRuntimes[i].PinId, PlinkoPinStateKind.Normal, 0, 1f));
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

                result.Add(new PlinkoSlotStateSnapshot(slotRuntimes[i].SlotId, PlinkoSlotStateKind.Normal, 0, 1f));
            }

            return result;
        }

        private static void ApplyTokens(IReadOnlyList<PlinkoBonusToken> tokens, List<PlinkoPinStateSnapshot> pinStates, List<PlinkoSlotStateSnapshot> slotStates, System.Random rng)
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
                        case PlinkoBonusTokenType.RandomPinValueBonus:
                            ApplyRandomPinValueBonus(pinStates, token.Amount, rng);
                            break;

                        case PlinkoBonusTokenType.RandomPinMultiplier:
                            ApplyRandomPinMultiplier(pinStates, token.Amount, rng);
                            break;

                        case PlinkoBonusTokenType.RandomSlotValueBonus:
                            ApplyRandomSlotValueBonus(slotStates, token.Amount, rng);
                            break;

                        case PlinkoBonusTokenType.RandomSlotMultiplier:
                            ApplyRandomSlotMultiplier(slotStates, token.Amount, rng);
                            break;
                    }
                }
            }
        }

        private static void ApplyRandomPinValueBonus(List<PlinkoPinStateSnapshot> pinStates, int amount, System.Random rng)
        {
            if (pinStates.Count == 0)
            {
                return;
            }

            int index = PickRandomNonErrorPinIndex(pinStates, rng);
            if (index < 0)
            {
                return;
            }

            PlinkoPinStateSnapshot state = pinStates[index];

            pinStates[index] = new PlinkoPinStateSnapshot(state.PinId, PlinkoPinStateKind.Boosted, state.ValueBonus + amount, state.Multiplier);
        }

        private static void ApplyRandomPinMultiplier(List<PlinkoPinStateSnapshot> pinStates, int amount, System.Random rng)
        {
            if (pinStates.Count == 0)
            {
                return;
            }

            int index = PickRandomNonErrorPinIndex(pinStates, rng);
            if (index < 0)
            {
                return;
            }

            PlinkoPinStateSnapshot state = pinStates[index];

            pinStates[index] = new PlinkoPinStateSnapshot(state.PinId, PlinkoPinStateKind.Boosted, state.ValueBonus, state.Multiplier + amount);
        }

        private static void ApplyRandomSlotValueBonus(List<PlinkoSlotStateSnapshot> slotStates, int amount, System.Random rng)
        {
            if (slotStates.Count == 0)
            {
                return;
            }

            int index = PickRandomNonErrorSlotIndex(slotStates, rng);
            if (index < 0)
            {
                return;
            }

            PlinkoSlotStateSnapshot state = slotStates[index];

            slotStates[index] = new PlinkoSlotStateSnapshot(state.SlotId, PlinkoSlotStateKind.Boosted, state.ValueBonus + amount, state.Multiplier);
        }

        private static void ApplyRandomSlotMultiplier(List<PlinkoSlotStateSnapshot> slotStates, int amount, System.Random rng)
        {
            if (slotStates.Count == 0)
            {
                return;
            }
            
            int index = PickRandomNonErrorSlotIndex(slotStates, rng);
            if (index < 0)
            {
                return;
            }
            PlinkoSlotStateSnapshot state = slotStates[index];

            slotStates[index] = new PlinkoSlotStateSnapshot(state.SlotId, PlinkoSlotStateKind.Boosted, state.ValueBonus, state.Multiplier + amount);
        }



        private static void ApplyErrorStates(List<PlinkoPinStateSnapshot> pinStates, List<PlinkoSlotStateSnapshot> slotStates, float errorPinRateReduction, float errorSlotRateReduction, System.Random rng)
        {
            float finalPinErrorRate = Mathf.Clamp01(BaseErrorPinRate - errorPinRateReduction);
            float finalSlotErrorRate = Mathf.Clamp01(BaseErrorSlotRate - errorSlotRateReduction);

            ApplyRandomErrorPins(pinStates, finalPinErrorRate, rng);
            ApplyRandomErrorSlots(slotStates, finalSlotErrorRate, rng);
        }


        private static void ApplyRandomErrorPins(List<PlinkoPinStateSnapshot> pinStates, float errorRate, System.Random rng)
        {
            if (pinStates == null || pinStates.Count == 0 || errorRate <= 0f)
            {
                return;
            }

            for (int i = 0; i < pinStates.Count; i++)
            {
                if (rng.NextDouble() > errorRate)
                {
                    continue;
                }

                PlinkoPinStateSnapshot state = pinStates[i];

                pinStates[i] = new PlinkoPinStateSnapshot(state.PinId, PlinkoPinStateKind.Error, 0, 1f);
            }
        }

        private static void ApplyRandomErrorSlots(List<PlinkoSlotStateSnapshot> slotStates, float errorRate, System.Random rng)
        {
            if (slotStates == null || slotStates.Count == 0 || errorRate <= 0f)
            {
                return;
            }

            for (int i = 0; i < slotStates.Count; i++)
            {
                if (rng.NextDouble() > errorRate)
                {
                    continue;
                }

                PlinkoSlotStateSnapshot state = slotStates[i];

                slotStates[i] = new PlinkoSlotStateSnapshot(state.SlotId, PlinkoSlotStateKind.Error, 0, 1f);
            }
        }


        private static int PickRandomNonErrorPinIndex(List<PlinkoPinStateSnapshot> pinStates, System.Random rng)
        {
            if (pinStates == null || pinStates.Count == 0)
            {
                return -1;
            }

            List<int> candidates = new List<int>();

            for (int i = 0; i < pinStates.Count; i++)
            {
                if (pinStates[i].StateKind != PlinkoPinStateKind.Error)
                {
                    candidates.Add(i);
                }
            }

            if (candidates.Count == 0)
            {
                return -1;
            }

            return candidates[rng.Next(0, candidates.Count)];
        }

        private static int PickRandomNonErrorSlotIndex(List<PlinkoSlotStateSnapshot> slotStates, System.Random rng)
        {
            if (slotStates == null || slotStates.Count == 0)
            {
                return -1;
            }

            List<int> candidates = new List<int>();

            for (int i = 0; i < slotStates.Count; i++)
            {
                if (slotStates[i].StateKind != PlinkoSlotStateKind.Error)
                {
                    candidates.Add(i);
                }
            }

            if (candidates.Count == 0)
            {
                return -1;
            }

            return candidates[rng.Next(0, candidates.Count)];
        }



#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static void LogAppliedState(in PlinkoRunSnapshot runSnapshot, List<PlinkoPinStateSnapshot> pinStates, List<PlinkoSlotStateSnapshot> slotStates)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[PlinkoAppliedSnapshotBuilder] Applied Snapshot");
            sb.AppendLine($"  Seed = {runSnapshot.BoardSeed}");
            sb.AppendLine($"  StartBalls = {runSnapshot.StartBalls}");
            sb.AppendLine($"  GlobalPinMultiplier = {runSnapshot.GlobalPinMultiplier}");
            sb.AppendLine($"  GlobalSlotMultiplier = {runSnapshot.GlobalSlotMultiplier}");
            sb.AppendLine($"  ErrorPinRateReduction = {runSnapshot.ErrorPinRateReduction}");
            sb.AppendLine($"  ErrorSlotRateReduction = {runSnapshot.ErrorSlotRateReduction}");

            for (int i = 0; i < pinStates.Count; i++)
            {
                PlinkoPinStateSnapshot pin = pinStates[i];

                if (pin.StateKind == PlinkoPinStateKind.Normal && pin.ValueBonus == 0 && Mathf.Approximately(pin.Multiplier, 1f))
                {
                    continue;
                }

                sb.AppendLine($"  Pin {pin.PinId} => state={pin.StateKind}, valueBonus={pin.ValueBonus}, multiplier={pin.Multiplier}");
            }

            for (int i = 0; i < slotStates.Count; i++)
            {
                PlinkoSlotStateSnapshot slot = slotStates[i];

                if (slot.StateKind == PlinkoSlotStateKind.Normal && slot.ValueBonus == 0 && Mathf.Approximately(slot.Multiplier, 1f))
                {
                    continue;
                }

                sb.AppendLine($"  Slot {slot.SlotId} => state={slot.StateKind}, valueBonus={slot.ValueBonus}, multiplier={slot.Multiplier}");
            }

            Debug.Log(sb.ToString());
        }
#endif
    }
}