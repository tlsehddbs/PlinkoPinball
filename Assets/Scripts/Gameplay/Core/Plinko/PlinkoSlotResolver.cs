using UnityEngine;
using PlinkoPinball.Gameplay.Components.Plinko;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 슬롯 진입 시 슬롯 보상을 정산하고 볼을 종료
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class PlinkoSlotResolver : MonoBehaviour
    {
        [SerializeField] private PlinkoSlotRuntime runtime;
        [SerializeField] private PlinkoBoardContext boardContext;

        private void Start()
        {
            if (boardContext == null)
            {
                boardContext = GetComponentInParent<PlinkoBoardContext>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (boardContext == null || runtime == null)
            {
                return;
            }

            PlinkoBallActor ball = other.GetComponent<PlinkoBallActor>();
            if (ball == null)
            {
                return;
            }

            PlinkoSlotModifierData modifier = runtime.ExportState();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                $"[PlinkoSlotResolver] Slot={runtime.SlotId}, base={runtime.BaseReward}, flat={modifier.FlatCurrencyBonus}, jackpot={modifier.JackpotMultiplier}",
                this);
#endif

            boardContext.RewardAccumulator.RegisterSlotReward(runtime.BaseReward, runtime.ExportState());
            ball.Resolve();
        }
    }
}