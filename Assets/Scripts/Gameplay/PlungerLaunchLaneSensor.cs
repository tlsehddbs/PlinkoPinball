using UnityEngine;

namespace PlinkoPinball.Gameplay
{
    /// <summary>
    /// 런치 레인 내부에 공이 존재하는지 감지하는 센서
    /// - 공이 레인 내부에 있을 경우에만 발사를 허용(중복 방지)
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class LaunchLaneSensor : MonoBehaviour
    {
        [SerializeField] private PlungerLauncher launcher;

        private void Reset()
        {
            // Trigger 사용할 것
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (launcher == null) return;

            if (other.attachedRigidbody != null)
            {
                // 공인지 판별은 tag 로
                launcher.SetBall(other.attachedRigidbody);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (launcher == null) return;

            if (other.attachedRigidbody != null && launcher.HasBall(other.attachedRigidbody))
                launcher.ClearBall();
        }
    }
}
