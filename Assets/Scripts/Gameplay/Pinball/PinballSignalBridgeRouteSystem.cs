using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core.TableEvents;

namespace PlinkoPinball.Gameplay.Pinball.Routes
{
    public sealed class PinballSignalBridgeRouteSystem : MonoBehaviour
    {
        private enum RouteSide
        {
            None = 0,
            Memory = 1,
            Processing = 2
        }

        private struct ActiveRoute
        {
            public RouteSide side;
            public float startedAt;
        }

        [Header("Input Event IDs")]
        [SerializeField] private string memoryEntryEventId = TableEventIds.PassMemoryEntry;
        [SerializeField] private string memoryExitEventId = TableEventIds.PassMemoryExit;

        [SerializeField] private string processingEntryEventId = TableEventIds.PassProcessingEntry;
        [SerializeField] private string processingExitEventId = TableEventIds.PassProcessingExit;

        [Header("Completed Event IDs")]
        [SerializeField] private string memoryCompletedEventId = TableEventIds.RouteMemoryCompleted;
        [SerializeField] private string processingCompletedEventId = TableEventIds.RouteProcessingCompleted;

        [Header("Timing")]
        [Min(0.1f)]
        [SerializeField] private float maxRouteDurationSeconds = 3.0f;

        [Header("Completed Tags")]
        [SerializeField] private string[] memoryCompletedTags =
        {
            TableEventTags.PlinkoReward,
            TableEventTags.PinBonus,
            TableEventTags.Memory,
            TableEventTags.Route
        };

        [SerializeField] private string[] processingCompletedTags =
        {
            TableEventTags.PlinkoReward,
            TableEventTags.SlotBonus,
            TableEventTags.Processing,
            TableEventTags.Route
        };

        [Header("Debug")]
        [SerializeField] private bool logRouteEvents = false;

        private readonly Dictionary<Rigidbody, ActiveRoute> _activeRoutes = new();
        private static readonly List<Rigidbody> ExpiredBuffer = new();

        private void OnEnable()
        {
            TableEventBus.OnEvent += OnTableEvent;
        }

        private void OnDisable()
        {
            TableEventBus.OnEvent -= OnTableEvent;
            _activeRoutes.Clear();
        }

        private void Update()
        {
            CleanupExpiredRoutes();
        }

        private void OnTableEvent(TableEvent e)
        {
            if (e.eventType != TableEventType.Pass || e.ball == null)
            {
                return;
            }

            if (e.eventId == memoryEntryEventId)
            {
                BeginRoute(e.ball, RouteSide.Memory);
                return;
            }

            if (e.eventId == processingEntryEventId)
            {
                BeginRoute(e.ball, RouteSide.Processing);
                return;
            }

            if (e.eventId == memoryExitEventId)
            {
                TryCompleteRoute(in e, RouteSide.Memory);
                return;
            }

            if (e.eventId == processingExitEventId)
            {
                TryCompleteRoute(in e, RouteSide.Processing);
            }
        }

        private void BeginRoute(Rigidbody ball, RouteSide side)
        {
            _activeRoutes[ball] = new ActiveRoute
            {
                side = side,
                startedAt = Time.time
            };

            if (logRouteEvents)
            {
                Debug.Log($"[SignalBridgeRouteSystem] Begin {side} route. ball={ball.name}", this);
            }
        }

        private void TryCompleteRoute(in TableEvent exitEvent, RouteSide expectedSide)
        {
            Rigidbody ball = exitEvent.ball;

            if (!_activeRoutes.TryGetValue(ball, out ActiveRoute active))
            {
                return;
            }

            float elapsed = Time.time - active.startedAt;

            if (active.side != expectedSide || elapsed > maxRouteDurationSeconds)
            {
                _activeRoutes.Remove(ball);
                return;
            }

            _activeRoutes.Remove(ball);
            PublishCompletedRoute(in exitEvent, expectedSide, elapsed);
        }

        private void PublishCompletedRoute(in TableEvent exitEvent, RouteSide side, float elapsed)
        {
            string eventId = side == RouteSide.Memory ? memoryCompletedEventId : processingCompletedEventId;

            string[] tags = side == RouteSide.Memory ? memoryCompletedTags : processingCompletedTags;

            var completedEvent = new TableEvent
            {
                eventId = eventId,
                eventType = TableEventType.Custom,
                baseValue = 0,
                tags = tags,
                source = exitEvent.source,
                position = exitEvent.position,
                time = Time.time,
                ball = exitEvent.ball
            };

            if (logRouteEvents)
            {
                Debug.Log($"[SignalBridgeRouteSystem] Completed {side} route in {elapsed:0.00}s", this);
            }

            TableEventBus.Publish(in completedEvent);
        }

        private void CleanupExpiredRoutes()
        {
            if (_activeRoutes.Count == 0)
            {
                return;
            }

            ExpiredBuffer.Clear();

            foreach (KeyValuePair<Rigidbody, ActiveRoute> pair in _activeRoutes)
            {
                if (pair.Key == null || Time.time - pair.Value.startedAt > maxRouteDurationSeconds)
                {
                    ExpiredBuffer.Add(pair.Key);
                }
            }

            for (int i = 0; i < ExpiredBuffer.Count; i++)
            {
                _activeRoutes.Remove(ExpiredBuffer[i]);
            }
        }
    }
}