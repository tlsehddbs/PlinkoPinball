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
        [Header("References")]
        [SerializeField] private PlinkoSlotRuntime runtime;
        [SerializeField] private PlinkoBoardContext boardContext;

        private Collider _triggerCollider;

        private void Reset()
        {
            _triggerCollider = GetComponent<Collider>();
            _triggerCollider.isTrigger = true;

            runtime = GetComponentInParent<PlinkoSlotRuntime>();
            boardContext = GetComponentInParent<PlinkoBoardContext>();
        }

        private void Awake()
        {
            _triggerCollider = GetComponent<Collider>();
            _triggerCollider.isTrigger = true;

            if (runtime == null)
            {
                runtime = GetComponentInParent<PlinkoSlotRuntime>();
            }

            if (boardContext == null)
            {
                boardContext = FindAnyObjectByType<PlinkoBoardContext>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (runtime == null || boardContext == null)
            {
                DebugLogMissingReference();
                return;
            }

            PlinkoBallActor ball = other.GetComponentInParent<PlinkoBallActor>();
            if (ball == null)
            {
                return;
            }

            ResolveSlot(ball);
        }

        private void ResolveSlot(PlinkoBallActor ball)
        {
            PlinkoSlotModifierData modifier = runtime.ExportState();
            int finalReward = modifier.CalculateReward(runtime.BaseReward);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                $"[PlinkoSlotResolver] Slot={runtime.SlotId}, base={runtime.BaseReward}, flat={modifier.FlatCurrencyBonus}, multiplier={modifier.JackpotMultiplier}, final={finalReward}",
                this);
#endif

            if (boardContext.RewardAccumulator != null)
            {
                boardContext.RewardAccumulator.RegisterSlotReward(runtime.BaseReward, modifier);
            }

            ball.Resolve();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void DebugLogMissingReference()
        {
            Debug.LogWarning(
                $"[{nameof(PlinkoSlotResolver)}] Missing reference. Runtime={runtime != null}, BoardContext={boardContext != null}",
                this);
        }
    }
}