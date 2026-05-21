using System.Linq;
using UnityEngine;
using PlinkoPinball.Core.TableEvents;

namespace PlinkoPinball.UI
{
    /// <summary>
    /// TableEventBus를 구독해 Pinball 이벤트를 로그 UI로 전달한다.
    /// 게임 규칙을 변경하지 않는 표시 전용 Presenter다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PinballEventLogPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PinballEventLogView logView;

        [Header("Filter")]
        [SerializeField] private bool showScoreEvents = true;
        [SerializeField] private bool showCompressionEvents = true;
        [SerializeField] private bool showChipsetEvents = true;
        [SerializeField] private bool showTimeEvents = true;

        private void OnEnable()
        {
            TableEventBus.OnEvent += HandleTableEvent;
        }

        private void OnDisable()
        {
            TableEventBus.OnEvent -= HandleTableEvent;
        }

        private void HandleTableEvent(TableEvent tableEvent)
        {
            if (logView == null)
            {
                return;
            }

            if (!ShouldShow(tableEvent))
            {
                return;
            }

            string message = FormatMessage(tableEvent);
            logView.AddLog(message);
        }

        private bool ShouldShow(TableEvent tableEvent)
        {
            if (tableEvent.tags == null)
            {
                return false;
            }

            Debug.LogWarning("보여줄 로그가 있는뎁쇼?");

            if (showScoreEvents && tableEvent.HasTag("score"))
            {
                return true;
            }

            if (showCompressionEvents && tableEvent.HasTag("compression"))
            {
                return true;
            }

            //TODO: 아래 부분은 기능이 다듬어 진 이후에 실 작동 테스트 및 실전 적용시 주석 해제하여 사용할 것
            // if (showChipsetEvents && tableEvent.HasTag("chipset"))
            //     return true;

            // if (showTimeEvents && (tableEvent.HasTag("timeBonus") || tableEvent.HasTag("timePenalty")))
            //     return true;

            return false;
        }

        private static string FormatMessage(TableEvent tableEvent)
        {
            string sourceName = tableEvent.source != null ? tableEvent.source.name : "Unknown";

            if (tableEvent.tags != null && tableEvent.tags.Contains("compression"))
            {
                return $"[CMP] {sourceName}  +Compression";
            }

            // if (tableEvent.tags != null && tableEvent.tags.Contains("chipset"))
            // {
            //     return $"[CHIP] {sourceName}  Chipset Signal";
            // }

            // if (tableEvent.tags != null && tableEvent.tags.Contains("timeBonus"))
            // {
            //     return $"[PWR] {sourceName}  +Power";
            // }

            // if (tableEvent.tags != null && tableEvent.tags.Contains("timePenalty"))
            // {
            //     return $"[PWR] {sourceName}  -Power";
            // }

            if (tableEvent.tags != null && tableEvent.tags.Contains("score"))
            {
                return $"[THR] {sourceName}  +{tableEvent.baseValue}";
            }

            return $"[EVT] {sourceName}";
        }
    }
}