using UnityEngine;
using System.Collections.Generic;
using PlinkoPinball.Core;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Core.Flow
{
    [System.Serializable]
    public struct UpgradeLevelState
    {
        public string UpgradeId;
        public int Level;

        public UpgradeLevelState(string upgradeId, int level)
        {
            UpgradeId = upgradeId;
            Level = Mathf.Max(0, level);
        }
    }

    /// <summary>
    /// 현재 플레이 세션 전반에서 유지해야 하는 상태를 보관합니다.
    /// </summary>
    public sealed class GameSessionState : MonoBehaviour
    {
        public static GameSessionState Instance { get; private set; }
        
        [SerializeField, Min(1)] private int currentRoundIndex = 1;
        [SerializeField, Min(0)] private long playerCurrency;

        [Header("Last Round")]
        [SerializeField, Min(0)] private int lastPinballScore;
        [SerializeField, Min(0)] private long lastPlinkoEarnedCurrency;
        [SerializeField, Min(0)] private int lastPlinkoPinHits;
        [SerializeField, Min(0)] private int lastPlinkoBallsResolved;

        [Header("Session Totals")]
        [SerializeField, Min(0)] private int totalPinballScore;
        [SerializeField, Min(0)] private long totalPlinkoEarnedCurrency;
        [SerializeField, Min(0)] private int totalPlinkoPinHits;
        [SerializeField, Min(0)] private int totalPlinkoBallsResolved;

        [Header("Flow")]
        [SerializeField] private GamePhase currentPhase = GamePhase.None;

        [Header("Upgrades")]
        [SerializeField] private List<UpgradeLevelState> upgradeLevels = new List<UpgradeLevelState>();

        public int CurrentRoundIndex => currentRoundIndex;
        public long PlayerCurrency => playerCurrency;
        public int LastPinballScore => lastPinballScore;
        public long LastPlinkoEarnedCurrency => lastPlinkoEarnedCurrency;
        public int LastPlinkoPinHits => lastPlinkoPinHits;
        public int LastPlinkoBallsResolved => lastPlinkoBallsResolved;
        public int TotalPinballScore => totalPinballScore;
        public long TotalPlinkoEarnedCurrency => totalPlinkoEarnedCurrency;
        public int TotalPlinkoPinHits => totalPlinkoPinHits;
        public int TotalPlinkoBallsResolved => totalPlinkoBallsResolved;
        public GamePhase CurrentPhase => currentPhase;

        public event System.Action OnSessionStateChanged;

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

        public void ResetSession()
        {
            currentRoundIndex = 1;
            lastPinballScore = 0;
            lastPlinkoEarnedCurrency = 0;
            lastPlinkoPinHits = 0;
            lastPlinkoBallsResolved = 0;
            totalPinballScore = 0;
            totalPlinkoEarnedCurrency = 0;
            totalPlinkoPinHits = 0;
            totalPlinkoBallsResolved = 0;
            currentPhase = GamePhase.None;
            upgradeLevels.Clear();

            OnSessionStateChanged?.Invoke();
        }

        public void SetRoundIndex(int roundIndex)
        {
            currentRoundIndex = Mathf.Max(1, roundIndex);
            OnSessionStateChanged?.Invoke();
        }

        public void AdvanceRound()
        {
            currentRoundIndex += 1;
            OnSessionStateChanged?.Invoke();
        }

        public void SetPhase(GamePhase phase)
        {
            if (currentPhase == phase)
            {
                return;
            }

            currentPhase = phase;
            OnSessionStateChanged?.Invoke();
        }

        public void SetPlayerCurrency(long currency)
        {
            long sanitizedCurrency = currency < 0 ? 0 : currency;
            if (playerCurrency == sanitizedCurrency)
            {
                return;
            }

            playerCurrency = sanitizedCurrency;
            OnSessionStateChanged?.Invoke();
        }

        public void RecordPinballRoundResult(int roundIndex, int pinballScore)
        {
            currentRoundIndex = Mathf.Max(1, roundIndex);
            lastPinballScore = Mathf.Max(0, pinballScore);
            totalPinballScore += lastPinballScore;
            OnSessionStateChanged?.Invoke();
        }

        public void RecordPlinkoRunResult(int roundIndex, int pinballScore, in PlinkoRunResult result)
        {
            currentRoundIndex = Mathf.Max(1, roundIndex);
            lastPinballScore = Mathf.Max(0, pinballScore);
            lastPlinkoEarnedCurrency = result.EarnedCurrency < 0 ? 0 : result.EarnedCurrency;
            lastPlinkoPinHits = Mathf.Max(0, result.TotalPinHits);
            lastPlinkoBallsResolved = Mathf.Max(0, result.BallsResolved);

            totalPlinkoEarnedCurrency += lastPlinkoEarnedCurrency;
            totalPlinkoPinHits += lastPlinkoPinHits;
            totalPlinkoBallsResolved += lastPlinkoBallsResolved;

            OnSessionStateChanged?.Invoke();
        }

        public int GetUpgradeLevel(string upgradeId)
        {
            if (string.IsNullOrWhiteSpace(upgradeId))
            {
                return 0;
            }

            for (int i = 0; i < upgradeLevels.Count; i++)
            {
                if (upgradeLevels[i].UpgradeId == upgradeId)
                {
                    return Mathf.Max(0, upgradeLevels[i].Level);
                }
            }

            return 0;
        }

        public void SetUpgradeLevel(string upgradeId, int level)
        {
            if (string.IsNullOrWhiteSpace(upgradeId))
            {
                return;
            }

            int sanitizedLevel = Mathf.Max(0, level);

            for (int i = 0; i < upgradeLevels.Count; i++)
            {
                if (upgradeLevels[i].UpgradeId != upgradeId)
                {
                    continue;
                }

                if (upgradeLevels[i].Level == sanitizedLevel)
                {
                    return;
                }

                upgradeLevels[i] = new UpgradeLevelState(upgradeId, sanitizedLevel);
                OnSessionStateChanged?.Invoke();
                return;
            }

            upgradeLevels.Add(new UpgradeLevelState(upgradeId, sanitizedLevel));
            OnSessionStateChanged?.Invoke();
        }
    }
}
