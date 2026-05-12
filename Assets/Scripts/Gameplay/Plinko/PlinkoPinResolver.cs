using UnityEngine;
using PlinkoPinball.Gameplay.Components.Plinko;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 핀 충돌 시 보상 누적을 요청
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class PlinkoPinResolver : MonoBehaviour
    {
        [SerializeField, Min(0)] private int baseReward = 1;
        [SerializeField] private PlinkoPinRuntime runtime;
        [SerializeField] private PlinkoBoardContext boardContext;

        private IPlinkoPinReaction[] _reactions;


        private void Awake()
        {
            _reactions = GetComponents<IPlinkoPinReaction>();
            
            if (runtime == null)
            {
                runtime = GetComponentInParent<PlinkoPinRuntime>();
            }


            if (boardContext == null)
            {
                boardContext = FindAnyObjectByType<PlinkoBoardContext>();
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (boardContext == null || runtime == null)
            {
                return;
            }

            if (other.gameObject.GetComponentInParent<PlinkoBallActor>() == null)
            {
                return;
            }

            PlinkoPinModifierData modifier = runtime.ExportState();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            int previewReward = PlinkoRewardCalculator.CalculatePinHitReward(baseReward, modifier);
            
            Debug.Log($"[PlinkoPinResolver] Pin={runtime.PinId}, state={modifier.StateKind}, base={baseReward}, valueBonus={modifier.ValueBonus}, multiplier={modifier.Multiplier}, reward={previewReward}", this);
#endif

            if (boardContext.RewardAccumulator != null)
            {
                boardContext.RewardAccumulator.RegisterPinHit(baseReward, modifier);
            }

            Vector3 hitPoint = other.contactCount > 0 ? other.GetContact(0).point : transform.position;

            for (int i = 0; i < _reactions.Length; i++)
            {
                _reactions[i].OnPinHit(other.rigidbody, hitPoint);
            }
        }
    }
}