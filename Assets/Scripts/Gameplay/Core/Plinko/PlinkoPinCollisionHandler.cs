using UnityEngine;
using PlinkoPinball.Gameplay.Components.Plinko;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 핀 충돌 시 보상 누적을 요청
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class PlinkoPinCollisionHandler : MonoBehaviour
    {
        [SerializeField, Min(0)] private int baseReward = 1;
        [SerializeField] private PlinkoPinRuntime runtime;
        [SerializeField] private PlinkoBoardContext boardContext;

        private void Start()
        {
            if (boardContext == null)
            {
                boardContext = GetComponentInParent<PlinkoBoardContext>();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (boardContext == null || runtime == null)
            {
                return;
            }

            if (collision.gameObject.GetComponent<PlinkoBallActor>() == null)
            {
                return;
            }

            PlinkoPinModifierData modifier = runtime.ExportState();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                $"[PlinkoPinCollisionHandler] Pin={runtime.PinId}, base={baseReward}, flat={modifier.FlatCurrencyBonus}, mul={modifier.HitMultiplier}, bounce={modifier.ExtraBounceReward}",
                this);
#endif

            boardContext.RewardAccumulator.RegisterPinHit(baseReward, runtime.ExportState());
        }
    }
}