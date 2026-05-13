using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Core;

namespace PlinkoPinball.Gameplay.Components.Reactions
{
    /// <summary>
    /// Hit 이벤트 시 공에게 임펄스를 가해 “범퍼 같은 손맛”을 만든다.
    /// - Trigger(감지)와 분리되어 있으므로, 범퍼/슬링샷/핀 등 어디든 조합 가능.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PinballHitImpulse : MonoBehaviour, ITableEventReaction
    {
        [Header("Filter")]
        [SerializeField] private bool onlyOnHitType = true;

        [Header("Impulse")]
        [Tooltip("임펄스 강도. 값은 물리 튜닝으로 조절.")]
        [Min(0f)]
        [SerializeField] private float impulseStrength = 6f;

        [Tooltip("수직(Up) 방향 보너스. 공이 너무 평평하게만 움직일 때 도움됨.")]
        [SerializeField] private float upBias = 0f;

        public void OnTableEvent(in TableEvent e)
        {
            if (onlyOnHitType && e.eventType != TableEventType.Hit)
            {
                return;
            }

            // Trigger에서 주입해 둔 ball Rigidbody를 사용
            var ballRb = e.ball;
            if (ballRb == null) 
            {
                return;
            }

            // 방향: 범퍼 중심에서 공 위치 방향으로 밀어냄
            Vector3 dir = (ballRb.worldCenterOfMass - transform.position).normalized;
            if (dir.sqrMagnitude < 0.0001f)
            {
                dir = transform.up;     // 완전히 겹쳤을 경우를 대비
            }
                
            if (upBias != 0f)
            {
                dir = (dir + Vector3.up * upBias).normalized;
            }
                
            ballRb.AddForce(dir * impulseStrength, ForceMode.Impulse);
        }
    }
}
