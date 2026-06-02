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
        private IPlinkoSlotReaction[] _reactions;

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

            _reactions = CollectReactions();

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
            int finalReward = PlinkoRewardCalculator.CalculateSlotReward(runtime.BaseReward, modifier);
            Vector3 hitPoint = ball != null ? ball.transform.position : transform.position;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[PlinkoSlotResolver] Slot={runtime.SlotId}, state={modifier.StateKind}, base={runtime.BaseReward}, valueBonus={modifier.ValueBonus}, multiplier={modifier.Multiplier}, final={finalReward}", this);
#endif

            if (boardContext.RewardAccumulator != null)
            {
                boardContext.RewardAccumulator.RegisterSlotReward(ball, runtime.BaseReward, modifier);
            }

            NotifyReactions(ball, hitPoint, in modifier, finalReward);

            ball.Resolve();
        }

        private void NotifyReactions(PlinkoBallActor ball, Vector3 hitPoint, in PlinkoSlotModifierData modifier, int finalReward)
        {
            if (_reactions == null)
            {
                return;
            }

            for (int i = 0; i < _reactions.Length; i++)
            {
                _reactions[i]?.OnSlotResolved(ball, hitPoint, runtime, in modifier, finalReward);
            }
        }

        private IPlinkoSlotReaction[] CollectReactions()
        {
            IPlinkoSlotReaction[] reactions = GetComponents<IPlinkoSlotReaction>();
            if (reactions != null && reactions.Length > 0)
            {
                return reactions;
            }

            if (runtime != null)
            {
                reactions = runtime.GetComponents<IPlinkoSlotReaction>();
                if (reactions != null && reactions.Length > 0)
                {
                    return reactions;
                }

                reactions = runtime.GetComponentsInChildren<IPlinkoSlotReaction>(true);
                if (reactions != null && reactions.Length > 0)
                {
                    return reactions;
                }
            }

            return System.Array.Empty<IPlinkoSlotReaction>();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void DebugLogMissingReference()
        {
            Debug.LogWarning($"[{nameof(PlinkoSlotResolver)}] Missing reference. Runtime={runtime != null}, BoardContext={boardContext != null}", this);
        }
    }
}
