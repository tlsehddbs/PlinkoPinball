using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Core;

namespace PlinkoPinball.Debugging
{
    /// <summary>
    /// 로컬 옵저버 호출이 정상적인지 확인하는 반응자(개발용)
    /// 특정 오브젝트에 붙이면 그 오브젝트 이벤트만 로그 생성
    /// </summary>
    public class LocalEventLoggerReaction : MonoBehaviour, ITableEventReaction
    {
        [SerializeField] private bool logEnabled = true;

        public void OnTableEvent(in TableEvent e)
        {
            if (!logEnabled) return;
            // Debug.Log($"[LocalReaction:{name}] {e}");
        }
    }
}

