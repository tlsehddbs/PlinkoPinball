using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlinkoPinball.Core.Flow
{
    /// <summary>
    /// 주요 씬 전환을 담당합니다.
    /// </summary>
    public sealed class GameSceneManager : MonoBehaviour
    {
        public static GameSceneManager Instance { get; private set; }


        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string mainTableSceneName = "MainTable";
        [SerializeField] private string plinkoSceneName = "PlinkoPhase";

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

        public void LoadMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void LoadMainTable()
        {
            SceneManager.LoadScene(mainTableSceneName);
            GameManager.Instance.SetPhase(GamePhase.Pinball);
        }

        public void LoadPlinko()
        {
            SceneManager.LoadScene(plinkoSceneName);
            GameManager.Instance.SetPhase(GamePhase.Plinko);
        }
    }
}