using UnityEngine;
using PlinkoPinball.Core.TableEvents;

namespace PlinkoPinball.Gameplay.Components.Reactions
{
    /// <summary>
    /// Hit 이벤트 시 공에게 임펄스를 가해 “범퍼 같은 손맛”을 만든다.
    /// - Trigger(감지)와 분리되어 있으므로, 범퍼/슬링샷/핀 등 어디든 조합 가능.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ImpulseOnHit : MonoBehaviour, ITableEventReaction
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
                return;

            // source(트리거 오브젝트) 기준으로 주변의 Rigidbody(공)를 찾아 임펄스 적용.
            // 현재 이벤트 구조에는 ball 참조가 없으므로, 가장 단순/안전한 방식으로 구현:
            // - 실제 공 Rigidbody는 충돌에서만 확실히 알 수 있으므로,
            //   추후 개선 버전에서는 TableEvent에 ball Transform/Rigidbody를 포함시키는 것이 좋다.
            //
            // MVP 단계에서는 “범퍼 오브젝트 주변의 Rigidbody”를 찾아 적용하거나,
            // Trigger가 ball 정보를 포함하도록 확장(권장)하면 된다.

            // ---- MVP 간단 구현(권장 개선: TableEvent에 ballRb 포함) ----
            // 여기서는 e.position 근처에서 Rigidbody를 검색해 가장 가까운 것을 공으로 간주한다.
            const float searchRadius = 0.25f;
            var hits = Physics.OverlapSphere(e.position, searchRadius);

            Rigidbody ballRb = null;
            float best = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                var rb = hits[i].attachedRigidbody;
                if (rb == null) continue;

                float d = (rb.worldCenterOfMass - e.position).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    ballRb = rb;
                }
            }

            if (ballRb == null) return;

            // 방향: 범퍼 중심 -> 공 위치 방향으로 밀어냄
            Vector3 dir = (ballRb.worldCenterOfMass - transform.position).normalized;
            if (upBias != 0f) dir = (dir + Vector3.up * upBias).normalized;

            ballRb.AddForce(dir * impulseStrength, ForceMode.Impulse);
        }
    }
}
