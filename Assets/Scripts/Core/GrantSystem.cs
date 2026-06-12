using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Modules;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Core.Grant
{
    /// <summary>
    /// Pinball Phase 중 발생한 TableEvent 및 Module completion event를 해석하여
    /// PlinkoRunSnapshotBuilder에 Plinko 입력 데이터를 누적한다.
    ///
    /// 주의:
    /// - Plinko Board를 직접 수정하지 않는다.
    /// - Pinball 중 실제 Currency를 지급하지 않는다.
    /// - PinBonus / SlotBonus / PinValueBonus / SlotValueBonus는 업그레이드 시스템에서 처리한다.
    /// </summary>
    public sealed class GrantSystem : MonoBehaviour
    {
        [Header("Snapshot")]
        [SerializeField] private PlinkoRunSnapshotBuilder snapshotBuilder;

        [Header("Table Event Grants")]
        [SerializeField] private PlinkoGrantDefinition[] grantDefinitions;

        [Header("Module Grants")]
        [SerializeField] private ModuleRoot[] moduleRoots;
        [SerializeField] private ModulePlinkoGrantDefinition[] moduleGrantDefinitions;

        [Header("Debug")]
        [SerializeField] private bool logGrants = false;

        private readonly HashSet<string> _oncePerRoundGrantedKeys = new();

        public void ConfigureSnapshotBuilder(PlinkoRunSnapshotBuilder builder)
        {
            if (builder != null)
            {
                snapshotBuilder = builder;
            }
        }

        private void OnEnable()
        {
            ResolveSnapshotBuilder();
            TableEventBus.OnEvent += OnTableEvent;
            SubscribeModules();
        }

        private void OnDisable()
        {
            TableEventBus.OnEvent -= OnTableEvent;
            UnsubscribeModules();
            _oncePerRoundGrantedKeys.Clear();
        }

        private void OnTableEvent(TableEvent tableEvent)
        {
            if (ResolveSnapshotBuilder() == null || grantDefinitions == null)
            {
                return;
            }

            for (int i = 0; i < grantDefinitions.Length; i++)
            {
                PlinkoGrantDefinition definition = grantDefinitions[i];
                if (definition == null)
                {
                    continue;
                }

                if (!definition.Matches(in tableEvent))
                {
                    continue;
                }

                if (!PassesChance(definition.Chance))
                {
                    continue;
                }

                ApplyRewards(definition.Rewards, $"tableEvent:{tableEvent.eventId}");
            }
        }

        private void SubscribeModules()
        {
            if (moduleRoots == null)
            {
                return;
            }

            for (int i = 0; i < moduleRoots.Length; i++)
            {
                if (moduleRoots[i] == null)
                {
                    continue;
                }

                moduleRoots[i].RewardStateChanged += OnModuleRewardStateChanged;
            }
        }

        private void UnsubscribeModules()
        {
            if (moduleRoots == null)
            {
                return;
            }

            for (int i = 0; i < moduleRoots.Length; i++)
            {
                if (moduleRoots[i] == null)
                {
                    continue;
                }

                moduleRoots[i].RewardStateChanged -= OnModuleRewardStateChanged;
            }
        }

        private void OnModuleRewardStateChanged(string moduleId, string groupId, ModuleRewardState state)
        {
            if (ResolveSnapshotBuilder() == null || moduleGrantDefinitions == null)
            {
                return;
            }

            for (int i = 0; i < moduleGrantDefinitions.Length; i++)
            {
                ModulePlinkoGrantDefinition definition = moduleGrantDefinitions[i];
                if (definition == null)
                {
                    continue;
                }

                if (!definition.Matches(moduleId, groupId, state))
                {
                    continue;
                }

                string onceKey = $"{moduleId}:{groupId}:{state}:{definition.name}";
                if (definition.OncePerRound && _oncePerRoundGrantedKeys.Contains(onceKey))
                {
                    continue;
                }

                if (!PassesChance(definition.Chance))
                {
                    continue;
                }

                ApplyRewards(definition.Rewards, $"module:{moduleId}:{groupId}:{state}");

                if (definition.OncePerRound)
                {
                    _oncePerRoundGrantedKeys.Add(onceKey);
                }
            }
        }

        private bool PassesChance(float chance)
        {
            if (chance >= 1f)
            {
                return true;
            }

            if (chance <= 0f)
            {
                return false;
            }

            return Random.value <= chance;
        }

        private void ApplyRewards(IReadOnlyList<PlinkoGrantRewardEntry> rewards, string reason)
        {
            if (rewards == null)
            {
                return;
            }

            for (int i = 0; i < rewards.Count; i++)
            {
                PlinkoGrantRewardEntry reward = rewards[i];
                if (reward == null)
                {
                    continue;
                }

                ApplyReward(reward, reason);
            }
        }

        private void ApplyReward(PlinkoGrantRewardEntry reward, string reason)
        {
            if (reward.GrantStartBalls && reward.StartBallAmount > 0)
            {
                snapshotBuilder.AddStartBalls(reward.StartBallAmount);

                if (logGrants)
                {
                    Debug.Log($"[PinballPlinkoGrantSystem] +StartBall {reward.StartBallAmount} / {reason}", this);
                }
            }

            if (!reward.GrantToken)
            {
                return;
            }

            ApplyTokenReward(reward.TokenType, reward.TokenAmount, reward.TokenStackCount, reason);
        }

        private void ApplyTokenReward(PlinkoBonusTokenType tokenType, int amount, int stackCount, string reason)
        {
            switch (tokenType)
            {
                case PlinkoBonusTokenType.GlobalPinMultiplier:
                    snapshotBuilder.AddGlobalPinMultiplier(amount);
                    if (logGrants)
                    {
                        Debug.Log(
                            $"[PinballPlinkoGrantSystem] +GlobalPinMultiplier {amount} / {reason}",
                            this);
                    }
                    break;

                case PlinkoBonusTokenType.GlobalSlotMultiplier:
                    snapshotBuilder.AddGlobalSlotMultiplier(amount);
                    if (logGrants)
                    {
                        Debug.Log(
                            $"[PinballPlinkoGrantSystem] +GlobalSlotMultiplier {amount} / {reason}",
                            this);
                    }
                    break;

                case PlinkoBonusTokenType.ErrorPinRateReduction:
                    snapshotBuilder.AddErrorPinRateReduction(amount * 0.01f);
                    if (logGrants)
                    {
                        Debug.Log(
                            $"[PinballPlinkoGrantSystem] +ErrorPinRateReduction {amount * 0.01f:0.###} / {reason}",
                            this);
                    }
                    break;

                case PlinkoBonusTokenType.ErrorSlotRateReduction:
                    snapshotBuilder.AddErrorSlotRateReduction(amount * 0.01f);
                    if (logGrants)
                    {
                        Debug.Log(
                            $"[PinballPlinkoGrantSystem] +ErrorSlotRateReduction {amount * 0.01f:0.###} / {reason}",
                            this);
                    }
                    break;

                default:
                    snapshotBuilder.AddToken(tokenType, amount, stackCount);
                    if (logGrants)
                    {
                        Debug.Log(
                            $"[PinballPlinkoGrantSystem] +Token {tokenType} amount={amount}, stack={stackCount} / {reason}",
                            this);
                    }
                    break;
            }
        }

        private PlinkoRunSnapshotBuilder ResolveSnapshotBuilder()
        {
            if (snapshotBuilder != null)
            {
                return snapshotBuilder;
            }

            snapshotBuilder = PlinkoRunSnapshotBuilder.ResolveFor(this);
            return snapshotBuilder;
        }
    }
}
