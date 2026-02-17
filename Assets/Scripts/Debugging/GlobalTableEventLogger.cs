using UnityEngine;
using PlinkoPinball.Core.TableEvents;

namespace PlinkoPinball.Debugging
{
    /// <summary>
    /// TableEventBus 전체 이벤트를 콘솔에 출력(개발용)
    /// </summary>
    // MainTable Scene에 1개로 제한
    public sealed class GlobalTableEventLogger : MonoBehaviour
    {
        [SerializeField] private bool logEnabled = true;

        private void OnEnable()
        {
            TableEventBus.OnEvent += Handle;
        }

        private void DisEnable()
        {
            TableEventBus.OnEvent -= Handle;
        }

        private void Handle(TableEvent e)
        {
            if (!logEnabled) return;
            Debug.Log($"[TableEvent] {e}");
        }
    }
}
