using System;
using UnityEngine;
using PlinkoPinball.Gameplay.Core.Flow;

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
        [SerializeField] private GameSessionState sessionState;

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

            ResolveSessionState();
            SyncSessionState();
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

            SyncSessionState();
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

            SyncSessionState();
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

        public void SetCurrency(long amount)
        {
            currentCurrency = amount < 0 ? 0 : amount;
            SyncSessionState();
            OnCurrencyChanged?.Invoke(currentCurrency);
        }

        private void SyncSessionState()
        {
            ResolveSessionState();
            sessionState?.SetPlayerCurrency(currentCurrency);
        }

        private void ResolveSessionState()
        {
            if (sessionState != null)
            {
                return;
            }

            sessionState = GameSessionState.Instance;
            if (sessionState == null)
            {
                sessionState = FindFirstObjectByType<GameSessionState>();
            }
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
