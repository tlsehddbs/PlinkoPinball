using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Flow
{
    /// <summary>
    /// 현재 플레이 세션 전반에서 유지해야 하는 상태를 보관합니다.
    /// </summary>
    public sealed class GameSessionState : MonoBehaviour
    {
        public static GameSessionState Instance { get; private set; }
        
        [SerializeField, Min(1)] private int currentRoundIndex = 1;

        public int CurrentRoundIndex => currentRoundIndex;


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
        }

        public void SetRoundIndex(int roundIndex)
        {
            currentRoundIndex = Mathf.Max(1, roundIndex);
        }

        public void AdvanceRound()
        {
            currentRoundIndex += 1;
        }
    }
}