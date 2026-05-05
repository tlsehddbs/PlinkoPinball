using UnityEngine;
using PlinkoPinball.Core.TableEvents;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 수신한 TableEvent를 해석하여 플링코 입력 데이터를 SnapshotBuilder에 누적
    /// </summary>
    public sealed class PinballPlinkoTokenGrantSystem : MonoBehaviour
    {
        [SerializeField] private PlinkoRunSnapshotBuilder snapshotBuilder;
        [SerializeField] private PlinkoTokenGrantDefinition[] grantDefinitions;

        private void OnEnable()
        {
            TableEventBus.OnEvent += OnTableEvent;
        }

        private void OnDisable()
        {
            TableEventBus.OnEvent -= OnTableEvent;
        }

        /// <summary>
        /// 수신한 TableEvent를 기준으로 정의된 플링코 보상을 적용
        /// </summary>
        /// <param name="tableEvent">수신한 테이블 이벤트</param>
        private void OnTableEvent(TableEvent tableEvent)
        {
            if (snapshotBuilder == null || grantDefinitions == null)
            {
                return;
            }

            for (int i = 0; i < grantDefinitions.Length; i++)
            {
                PlinkoTokenGrantDefinition definition = grantDefinitions[i];
                if (definition == null)
                {
                    continue;
                }

                if (!definition.Matches(in tableEvent))
                {
                    continue;
                }

                if (definition.Chance < 1f && Random.value > definition.Chance)
                {
                    continue;
                }

                ApplyRewards(definition);
            }
        }

        /// <summary>
        /// 정의된 보상을 SnapshotBuilder에 누적
        /// </summary>
        /// <param name="definition">적용할 보상 정의</param>
        private void ApplyRewards(PlinkoTokenGrantDefinition definition)
        {
            var rewards = definition.Rewards;
            if (rewards == null)
            {
                return;
            }

            for (int i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];

                if (reward.GrantStartBalls)
                {
                    snapshotBuilder.AddStartBalls(reward.StartBallAmount);
                }

                if (!reward.GrantToken)
                {
                    continue;
                }

                ApplyTokenReward(reward.TokenType, reward.TokenAmount, reward.TokenStackCount);
            }
        }

        private void ApplyTokenReward(PlinkoBonusTokenType tokenType, int amount, int stackCount)
        {
            switch (tokenType)
            {
                case PlinkoBonusTokenType.GlobalPinMultiplier:
                    snapshotBuilder.AddGlobalPinMultiplier(amount);
                    break;

                case PlinkoBonusTokenType.GlobalSlotMultiplier:
                    snapshotBuilder.AddGlobalSlotMultiplier(amount);
                    break;

                case PlinkoBonusTokenType.ErrorPinRateReduction:
                    snapshotBuilder.AddErrorPinRateReduction(amount * 0.01f);
                    break;

                case PlinkoBonusTokenType.ErrorSlotRateReduction:
                    snapshotBuilder.AddErrorSlotRateReduction(amount * 0.01f);
                    break;

                default:
                    snapshotBuilder.AddToken(tokenType, amount, stackCount);
                    break;
            }
        }
    }
}