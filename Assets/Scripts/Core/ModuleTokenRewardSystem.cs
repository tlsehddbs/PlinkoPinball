using UnityEngine;
using PlinkoPinball.Gameplay.Core.Plinko;
using PlinkoPinball.Gameplay.Modules;

namespace PlinkoPinball.Gameplay.Core.Modules
{
    /// <summary>
    /// 모듈 상태 변화에 따라 플링코 토큰 보상 지급
    /// 모듈은 상태를 제공, 지급 로직은 시스템이 담당
    /// </summary>
    public sealed class ModuleTokenRewardSystem : MonoBehaviour
    {
        [SerializeField] private PlinkoRunSnapshotBuilder snapshotBuilder;
        [SerializeField] private ModuleRoot[] moduleRoots;
        [SerializeField] private ModuleTokenRewardDefinition[] rewardDefinitions;

        private void OnEnable()
        {
            if (moduleRoots == null)
            {
                return;
            }

            for (int i = 0; i < moduleRoots.Length; i++)
            {
                if (moduleRoots[i] != null)
                {
                    moduleRoots[i].RewardStateChanged += OnModuleRewardStateChanged;
                }
            }
        }

        private void OnDisable()
        {
            if (moduleRoots == null)
            {
                return;
            }

            for (int i = 0; i < moduleRoots.Length; i++)
            {
                if (moduleRoots[i] != null)
                {
                    moduleRoots[i].RewardStateChanged -= OnModuleRewardStateChanged;
                }
            }
        }

        private void OnModuleRewardStateChanged(string moduleId, ModuleRewardState state)
        {
            if (snapshotBuilder == null || rewardDefinitions == null)
            {
                return;
            }

            for (int i = 0; i < rewardDefinitions.Length; i++)
            {
                ModuleTokenRewardDefinition definition = rewardDefinitions[i];
                if (definition == null)
                {
                    continue;
                }

                if (definition.ModuleId != moduleId || definition.RequiredState != state)
                {
                    continue;
                }

                ApplyRewards(definition);
            }
        }

        private void ApplyRewards(ModuleTokenRewardDefinition definition)
        {
            var rewards = definition.Rewards;
            if (rewards == null)
            {
                return;
            }

            for (int i = 0; i < rewards.Length; i++)
            {
                var reward = rewards[i];

                if (reward.GrantStartBalls)
                {
                    snapshotBuilder.AddStartBalls(reward.StartBallAmount);
                }

                if (reward.GrantGlobalCurrencyPerPinHit)
                {
                    snapshotBuilder.AddGlobalCurrencyPerPinHit(reward.GlobalCurrencyPerPinHitAmount);
                }

                if (reward.GrantToken)
                {
                    snapshotBuilder.AddToken(
                        reward.TokenType,
                        reward.TokenAmount,
                        reward.TokenStackCount);
                }
            }
        }
    }
}