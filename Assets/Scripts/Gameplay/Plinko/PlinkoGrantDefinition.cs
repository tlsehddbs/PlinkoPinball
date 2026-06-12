using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Modules;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    [CreateAssetMenu(menuName = "PlinkoPinball/Plinko Grants/Table Event Grant Definition")]
    public sealed class PlinkoGrantDefinition : ScriptableObject
    {
        [Header("Event Filter")]
        [SerializeField] private bool useEventTypeFilter = true;
        [SerializeField] private TableEventType requiredEventType = TableEventType.Hit;

        [SerializeField] private string requiredEventId = "";

        [Tooltip("비워두면 tag 조건을 사용하지 않는다.")]
        [SerializeField] private string requiredTag = "";

        [Header("Grant")]
        [Range(0f, 1f)]
        [SerializeField] private float chance = 1f;

        [SerializeField] private List<PlinkoGrantRewardEntry> rewards = new();

        public float Chance => chance;
        public IReadOnlyList<PlinkoGrantRewardEntry> Rewards => rewards;

        public bool Matches(in TableEvent tableEvent)
        {
            if (useEventTypeFilter && tableEvent.eventType != requiredEventType)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(requiredEventId) && tableEvent.eventId != requiredEventId)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(requiredTag) && !tableEvent.HasTag(requiredTag))
            {
                return false;
            }

            return true;
        }
    }

    [CreateAssetMenu(menuName = "PlinkoPinball/Plinko Grants/Module Grant Definition")]
    public sealed class ModulePlinkoGrantDefinition : ScriptableObject
    {
        [Header("Module Filter")]
        [Tooltip("비워두면 모든 모듈 ID를 허용한다.")]
        [SerializeField] private string requiredModuleId = "";

        [Tooltip("비워두면 모든 groupId를 허용한다.")]
        [SerializeField] private string requiredGroupId = "";

        [SerializeField] private ModuleRewardState requiredState = ModuleRewardState.Completed;

        [Header("Grant Policy")]
        [SerializeField] private bool oncePerRound = false;

        [Range(0f, 1f)]
        [SerializeField] private float chance = 1f;

        [Header("Rewards")]
        [SerializeField] private List<PlinkoGrantRewardEntry> rewards = new();

        public bool OncePerRound => oncePerRound;
        public float Chance => chance;
        public IReadOnlyList<PlinkoGrantRewardEntry> Rewards => rewards;

        public bool Matches(string moduleId, string groupId, ModuleRewardState state)
        {
            if (state != requiredState)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(requiredModuleId) && requiredModuleId != moduleId)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(requiredGroupId) && requiredGroupId != groupId)
            {
                return false;
            }

            return true;
        }
    }
}
