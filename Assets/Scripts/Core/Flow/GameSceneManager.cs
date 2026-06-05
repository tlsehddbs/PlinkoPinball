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
        [SerializeField] private string mainTableSceneName = "MainframeOSScene";
        [SerializeField] private string plinkoSceneName = "PlinkoScene";
        [SerializeField] private bool useMainframeSingleSceneFlow = true;

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
            if (useMainframeSingleSceneFlow)
            {
                GameManager.Instance.SetPhase(GamePhase.Plinko);
                return;
            }

            SceneManager.LoadScene(plinkoSceneName);
            GameManager.Instance.SetPhase(GamePhase.Plinko);
        }
    }
}
