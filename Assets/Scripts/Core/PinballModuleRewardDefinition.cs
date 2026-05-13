using UnityEngine;
using PlinkoPinball.Gameplay.Modules;

namespace PlinkoPinball.Core.Rewards
{
    [CreateAssetMenu(
        menuName = "PlinkoPinball/Rewards/Pinball Module Reward Definition")]
    public sealed class PinballModuleRewardDefinition : ScriptableObject
    {
        [Header("Module Filter")]
        [SerializeField] private string moduleId;
        [SerializeField] private string groupId = "group";
        [SerializeField] private ModuleRewardState requiredState = ModuleRewardState.Completed;

        [Header("Grant Policy")]
        [SerializeField] private bool oncePerRound = false;

        [Header("Rewards")]
        [SerializeField] private PinballRewardEntry[] rewards;

        public string ModuleId => moduleId;
        public string GroupId => groupId;
        public ModuleRewardState RequiredState => requiredState;
        public bool OncePerRound => oncePerRound;
        public PinballRewardEntry[] Rewards => rewards;

        public bool Matches(string targetModuleId, string targetGroupId, ModuleRewardState state)
        {
            if (!string.IsNullOrWhiteSpace(moduleId) && moduleId != targetModuleId)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(groupId) && groupId != targetGroupId)
            {
                return false;
            }

            return state == requiredState;
        }
    }
}