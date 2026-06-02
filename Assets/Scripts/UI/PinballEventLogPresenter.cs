using UnityEngine;
using PlinkoPinball.Core.TableEvents;

namespace PlinkoPinball.UI
{
    /// <summary>
    /// TableEventBus를 구독해 Pinball 이벤트를 메인프레임 시스템 로그 UI로 전달한다.
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

        [Header("Format")]
        [SerializeField] private bool showTimestamp = true;
        [SerializeField] private bool showSource = true;

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

            if (showCompressionEvents && tableEvent.HasTag("compression"))
            {
                return true;
            }

            if (showScoreEvents && tableEvent.HasTag("score"))
            {
                return true;
            }

            if (showChipsetEvents && tableEvent.HasTag("chipset"))
            {
                return true;
            }

            if (showTimeEvents && (tableEvent.HasTag("timeBonus") || tableEvent.HasTag("timePenalty")))
            {
                return true;
            }

            return false;
        }

        private string FormatMessage(TableEvent tableEvent)
        {
            string sourceName = GetSourceName(tableEvent);
            string sourceCode = NormalizeSourceName(sourceName);
            string timestamp = showTimestamp ? $"[{Time.time:0000.00}] " : string.Empty;

            if (tableEvent.HasTag("compression"))
            {
                return BuildLine(timestamp, "CMP", sourceCode, "PACKET +1");
            }

            if (tableEvent.HasTag("chipset"))
            {
                return BuildLine(timestamp, "CHIP", sourceCode, "SIGNAL LATCHED");
            }

            if (tableEvent.HasTag("timeBonus"))
            {
                return BuildLine(timestamp, "PWR", sourceCode, $"+{tableEvent.baseValue:0.#}% RESERVE");
            }

            if (tableEvent.HasTag("timePenalty"))
            {
                return BuildLine(timestamp, "PWR", sourceCode, $"-{Mathf.Abs(tableEvent.baseValue):0.#}% RESERVE");
            }

            if (tableEvent.HasTag("score"))
            {
                return BuildLine(timestamp, "THR", sourceCode, $"SIGNAL +{tableEvent.baseValue:0}");
            }

            return BuildLine(timestamp, "EVT", sourceCode, "SIGNAL DETECTED");
        }

        private string BuildLine(string timestamp, string channel, string source, string message)
        {
            if (!showSource)
            {
                return $"{timestamp}{channel,-4} {message}";
            }

            return $"{timestamp}{channel,-4} {source,-14} {message}";
        }

        private static string GetSourceName(TableEvent tableEvent)
        {
            if (tableEvent.source == null)
            {
                return "UNKNOWN";
            }

            return tableEvent.source.name;
        }

        private static string NormalizeSourceName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return "UNKNOWN";
            }

            string value = raw.ToUpperInvariant();

            value = value.Replace("(CLONE)", string.Empty);
            value = value.Replace("TESTBUMPER", "CAP");
            value = value.Replace("BUMPER", "CAP");
            value = value.Replace("CAPACITOR", "CAP");
            value = value.Replace("DROPTARGET", "MOSFET");
            value = value.Replace("TARGET", "MOSFET");
            value = value.Replace("SLING", "COIL");
            value = value.Replace("ORBIT", "BUS");
            value = value.Replace("DATABUS", "BUS");
            value = value.Replace("RAMP", "BRIDGE");
            value = value.Replace("SPINNER", "FAN");
            value = value.Replace("COLLIDER", "NODE");

            value = value.Replace("__", "_");
            value = value.Trim('_', ' ');

            if (value.Length > 14)
            {
                value = value.Substring(0, 14);
            }

            return value;
        }
    }
}