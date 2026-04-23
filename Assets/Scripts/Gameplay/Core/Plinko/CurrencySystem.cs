using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Meta
{
    /// <summary>
    /// 메타 재화 관리
    /// </summary>
    public sealed class CurrencySystem : MonoBehaviour
    {
        [SerializeField, Min(0)] private long currentCurrency;

        public long CurrentCurrency => currentCurrency;

        /// <summary>
        /// 재화 추가
        /// </summary>
        public void AddCurrency(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentCurrency += amount;
        }
    }
}