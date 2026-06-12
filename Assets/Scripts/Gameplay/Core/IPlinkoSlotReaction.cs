using UnityEngine;
using PlinkoPinball.Gameplay.Components.Plinko;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 슬롯에 공이 도착해 해결될 때 실행되는 로컬 반응 인터페이스
    /// </summary>
    public interface IPlinkoSlotReaction
    {
        void OnSlotResolved(PlinkoBallActor ball, Vector3 hitPoint, PlinkoSlotRuntime runtime, in PlinkoSlotModifierData modifier, int finalReward);
    }
}
