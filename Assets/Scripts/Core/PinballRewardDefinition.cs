using UnityEngine;
using PlinkoPinball.Core.TableEvents;

namespace PlinkoPinball.Core.Rewards
{
    [CreateAssetMenu(menuName = "PlinkoPinball/Rewards/Pinball Reward Definition")]
    public sealed class PinballRewardDefinition : ScriptableObject
    {
        [Header("Event Filter")]
        [SerializeField] private string requiredEventId;
        [SerializeField] private TableEventType requiredEventType;
        [SerializeField] private bool useEventTypeFilter = true;
        [SerializeField] private string requiredTag;

        [Header("Rewards")]
        [SerializeField] private PinballRewardEntry[] rewards;

        public bool Matches(in TableEvent e)
        {
            if (useEventTypeFilter && e.eventType != requiredEventType)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(requiredEventId) && e.eventId != requiredEventId)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(requiredTag) && !e.HasTag(requiredTag))
            {
                return false;
            }

            return true;
        }

        public PinballRewardEntry[] Rewards => rewards;
    }
}