using UnityEngine;
using PlinkoPinball.Gameplay.Core.Plinko;
using PlinkoPinball.Core.TableEvents;

// namespace PlinkoPinball.Gameplay.Core.Modules
// {
//     /// <summary>
//     /// 모듈 상태 변화에 따라 플링코 토큰 보상 지급
//     /// 모듈은 상태를 제공, 지급 로직은 시스템이 담당
//     /// </summary>
//     public sealed class ModuleTokenRewardSystem : MonoBehaviour
//     {
//         [SerializeField] private PlinkoRunSnapshotBuilder snapshotBuilder;
//         [SerializeField] private ModuleRoot[] moduleRoots;
//         [SerializeField] private ModuleTokenRewardDefinition[] rewardDefinitions;

//         private void OnEnable()
//         {
//             if (moduleRoots == null)
//             {
//                 return;
//             }

//             for (int i = 0; i < moduleRoots.Length; i++)
//             {
//                 if (moduleRoots[i] != null)
//                 {
//                     moduleRoots[i].RewardStateChanged += OnModuleRewardStateChanged;
//                 }
//             }
//         }

//         private void OnDisable()
//         {
//             if (moduleRoots == null)
//             {
//                 return;
//             }

//             for (int i = 0; i < moduleRoots.Length; i++)
//             {
//                 if (moduleRoots[i] != null)
//                 {
//                     moduleRoots[i].RewardStateChanged -= OnModuleRewardStateChanged;
//                 }
//             }
//         }

//         private void OnModuleRewardStateChanged(string moduleId, ModuleRewardState state)
//         {
//             if (snapshotBuilder == null || rewardDefinitions == null)
//             {
//                 return;
//             }

//             for (int i = 0; i < rewardDefinitions.Length; i++)
//             {
//                 ModuleTokenRewardDefinition definition = rewardDefinitions[i];

//                 if (definition == null)
//                 {
//                     continue;
//                 }

//                 if (definition.ModuleId != moduleId || definition.RequiredState != state)
//                 {
//                     continue;
//                 }

//                 ApplyRewards(definition);
//             }
//         }

//         private void ApplyRewards(ModuleTokenRewardDefinition definition)
//         {
//             var rewards = definition.Rewards;

//             if (rewards == null)
//             {
//                 return;
//             }

//             for (int i = 0; i < rewards.Length; i++)
//             {
//                 ModuleTokenRewardDefinition.RewardEntry reward = rewards[i];

//                 if (reward.GrantStartBalls)
//                 {
//                     snapshotBuilder.AddStartBalls(reward.StartBallAmount);
//                 }

//                 if (!reward.GrantToken)
//                 {
//                     continue;
//                 }

//                 ApplyTokenReward(reward.TokenType, reward.TokenAmount, reward.TokenStackCount);
//             }
//         }

//         private void ApplyTokenReward(PlinkoBonusTokenType tokenType, int amount, int stackCount)
//         {
//             switch (tokenType)
//             {
//                 case PlinkoBonusTokenType.GlobalPinMultiplier:
//                     snapshotBuilder.AddGlobalPinMultiplier(amount);
//                     break;

//                 case PlinkoBonusTokenType.GlobalSlotMultiplier:
//                     snapshotBuilder.AddGlobalSlotMultiplier(amount);
//                     break;

//                 case PlinkoBonusTokenType.ErrorPinRateReduction:
//                     snapshotBuilder.AddErrorPinRateReduction(amount * 0.01f);
//                     break;

//                 case PlinkoBonusTokenType.ErrorSlotRateReduction:
//                     snapshotBuilder.AddErrorSlotRateReduction(amount * 0.01f);
//                     break;

//                 default:
//                     snapshotBuilder.AddToken(tokenType, amount, stackCount);
//                     break;
//             }
//         }
//     }
// }


namespace PlinkoPinball.Core.Rewards
{
    public sealed class PinballRewardSystem : MonoBehaviour
    {
        [SerializeField] private PinballRewardDefinition[] rewardDefinitions;
        [SerializeField] private PlinkoRunSnapshotBuilder snapshotBuilder;

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
        }

        private void OnDisable()
        {
            TableEventBus.OnEvent -= OnTableEvent;
        }

        private void OnTableEvent(TableEvent e)
        {
            if (rewardDefinitions == null)
            {
                return;
            }

            for (int i = 0; i < rewardDefinitions.Length; i++)
            {
                var def = rewardDefinitions[i];
                if (def == null || !def.Matches(in e))
                {
                    continue;
                }

                ApplyRewards(def.Rewards);
            }
        }

        private void ApplyRewards(PinballRewardEntry[] rewards)
        {
            if (rewards == null || ResolveSnapshotBuilder() == null)
            {
                return;
            }

            for (int i = 0; i < rewards.Length; i++)
            {
                ApplyReward(rewards[i]);
            }
        }

        private void ApplyReward(PinballRewardEntry reward)
        {
            switch (reward.RewardType)
            {
                case PinballRewardType.PlinkoStartBall:
                    snapshotBuilder.AddStartBalls(Mathf.RoundToInt(reward.Amount));
                    break;

                case PinballRewardType.CompressionGain:
                    // CompressionSystem으로 넘기는 게 더 좋음.
                    // 직접 AddStartBall 금지.
                    break;

                // case PinballRewardType.PinBonus:
                //     snapshotBuilder.AddPinBonus(Mathf.RoundToInt(reward.Amount));
                //     break;

                // case PinballRewardType.SlotBonus:
                //     snapshotBuilder.AddSlotBonus(Mathf.RoundToInt(reward.Amount));
                //     break;

                case PinballRewardType.GlobalPinMultiplier:
                    snapshotBuilder.AddGlobalPinMultiplier(reward.Amount);
                    break;

                case PinballRewardType.GlobalSlotMultiplier:
                    snapshotBuilder.AddGlobalSlotMultiplier(reward.Amount);
                    break;

                // case PinballRewardType.PinValueBonus:
                //     snapshotBuilder.AddPinValueBonus(reward.Amount);
                //     break;

                // case PinballRewardType.SlotValueBonus:
                //     snapshotBuilder.AddSlotValueBonus(reward.Amount);
                //     break;

                case PinballRewardType.ErrorPinRateReduction:
                    snapshotBuilder.AddErrorPinRateReduction(reward.Amount);
                    break;

                case PinballRewardType.ErrorSlotRateReduction:
                    snapshotBuilder.AddErrorSlotRateReduction(reward.Amount);
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
