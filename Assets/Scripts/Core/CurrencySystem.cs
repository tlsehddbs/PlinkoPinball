using UnityEngine;

namespace PlinkoPinball.Core
{
    /// <summary>
    /// 메타 재화 관리
    /// </summary>
    public sealed class CurrencySystem : MonoBehaviour
    {
        public static CurrencySystem Instance { get; private set; }
        
        [SerializeField, Min(0)] private long currentCurrency;

        public long CurrentCurrency => currentCurrency;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

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
            Debug.Log($"[CurrencySystem] get amount={amount}");
        }
    }
}