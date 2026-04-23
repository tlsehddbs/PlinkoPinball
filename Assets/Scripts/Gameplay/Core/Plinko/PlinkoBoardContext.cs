using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 보드 하위 오브젝트가 공통 런타임 참조에 접근하기 위한 컨텍스트
    /// </summary>
    public sealed class PlinkoBoardContext : MonoBehaviour
    {
        [SerializeField] private PlinkoRewardAccumulator rewardAccumulator;

        public PlinkoRewardAccumulator RewardAccumulator => rewardAccumulator;
    }
}