using System.Collections.Generic;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Upgrades
{
    /// <summary>
    /// 업그레이드 노드 정의를 보관하는 데이터베이스
    /// </summary>
    [CreateAssetMenu(fileName = "UpgradeDatabase", menuName = "PlinkoPinball/Upgrades/Upgrade Database")]
    public sealed class UpgradeDatabase : ScriptableObject
    {
        [SerializeField] private UpgradeNodeData[] nodes;

        private Dictionary<string, UpgradeNodeData> _lookup;

        public IReadOnlyList<UpgradeNodeData> Nodes => nodes;

        private void OnEnable()
        {
            RebuildLookup();
        }

        /// <summary>
        /// 지정한 ID의 노드를 찾는다
        /// </summary>
        public bool TryGetNode(string id, out UpgradeNodeData node)
        {
            if (_lookup == null)
            {
                RebuildLookup();
            }

            return _lookup.TryGetValue(id, out node);
        }

        private void RebuildLookup()
        {
            _lookup = new Dictionary<string, UpgradeNodeData>(nodes != null ? nodes.Length : 0);

            if (nodes == null)
            {
                return;
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node == null || string.IsNullOrWhiteSpace(node.Id))
                {
                    continue;
                }

                _lookup[node.Id] = node;
            }
        }
    }
}