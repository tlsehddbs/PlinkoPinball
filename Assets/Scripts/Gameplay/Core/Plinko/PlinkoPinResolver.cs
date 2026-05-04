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
        }

        private void Start()
        {
            if (runtime == null)
            {
                runtime = GetComponentInParent<PlinkoPinRuntime>();
            }


            if (boardContext == null)
            {
                boardContext = FindAnyObjectByType<PlinkoBoardContext>();
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
            Debug.Log($"[PlinkoPinCollisionHandler] Pin={runtime.PinId}, base={baseReward}, flat={modifier.FlatCurrencyBonus}, mul={modifier.HitMultiplier}, bounce={modifier.ExtraBounceReward}", this);
#endif

            boardContext.RewardAccumulator.RegisterPinHit(baseReward, runtime.ExportState());

            Vector3 hitPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;

            for (int i = 0; i < _reactions.Length; i++)
            {
                _reactions[i].OnPinHit(collision.rigidbody, hitPoint);
            }
        }
    }
}