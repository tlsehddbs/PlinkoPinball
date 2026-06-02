using System;
using UnityEngine;

namespace PlinkoPinball.Core
{
    /// <summary>
    /// 재화 관리 시스템
    /// </summary>
    public sealed class CurrencySystem : MonoBehaviour
    {
        public static CurrencySystem Instance { get; private set; }

        /// <summary>
        /// 재화 변경 시 호출
        /// </summary>
        public event Action<long> OnCurrencyChanged;

        [SerializeField, Min(0)]
        private long currentCurrency;

        /// <summary>
        /// 현재 보유 재화
        /// </summary>
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

            Debug.Log($"[CurrencySystem] Add Currency amount={amount} total={currentCurrency}");

            OnCurrencyChanged?.Invoke(currentCurrency);
        }

        /// <summary>
        /// 재화 소비
        /// </summary>
        public bool SpendCurrency(long amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            if (currentCurrency < amount)
            {
                Debug.LogWarning($"[CurrencySystem] Not enough currency need={amount} current={currentCurrency}");

                return false;
            }

            currentCurrency -= amount;

            Debug.Log($"[CurrencySystem] Spend Currency amount={amount} remain={currentCurrency}");

            OnCurrencyChanged?.Invoke(currentCurrency);

            return true;
        }

        /// <summary>
        /// 구매 가능 여부 확인
        /// </summary>
        public bool CanAfford(long amount)
        {
            return currentCurrency >= amount;
        }

#if UNITY_EDITOR
        [ContextMenu("Add 100 Currency")]
        private void DebugAddCurrency()
        {
            AddCurrency(100);
        }
#endif
    }
}