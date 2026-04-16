using UnityEngine;

namespace PlinkoPinball.Gameplay.Upgrades
{
    /// <summary>
    /// 업그레이드 그래프의 단일 노드 정의 데이터
    /// ScriptableObject로 관리하여 밸런싱과 그래프 배치를 데이터화
    /// </summary>
    [CreateAssetMenu(fileName = "UpgradeNodeData", menuName = "PlinkoPinball/Upgrades/Upgrade Node")]
    public sealed class UpgradeNodeData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "upgrade.none";
        [SerializeField] private string displayName = "New Upgrade";
        [SerializeField] [TextArea(2, 4)] private string description = "Description";
        [SerializeField] private Sprite icon;

        [Header("Graph")]
        [SerializeField] private string[] prerequisiteIds;
        [SerializeField] private Vector2 graphPosition = Vector2.zero;

        [Header("Cost")]
        [SerializeField] private int cost = 1;

        [Header("Effect")]
        [SerializeField] private UpgradeStatKey statKey = UpgradeStatKey.None;
        [SerializeField] private float additiveValue = 0f;

        [Header("Flags")]
        [SerializeField] private bool isRootNode;
        [SerializeField] private bool isRepeatable;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public string[] PrerequisiteIds => prerequisiteIds;
        public Vector2 GraphPosition => graphPosition;
        public int Cost => cost;
        public UpgradeStatKey StatKey => statKey;
        public float AdditiveValue => additiveValue;
        public bool IsRootNode => isRootNode;
        public bool IsRepeatable => isRepeatable;
    }
}