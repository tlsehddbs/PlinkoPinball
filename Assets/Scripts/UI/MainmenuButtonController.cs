using UnityEngine;
using UnityEngine.SceneManagement;
using PlinkoPinball.Core.Flow;

namespace PlinkoPinball.UI.MainMenu
{
    public sealed class MainMenuButtonController : MonoBehaviour
    {
        [SerializeField] private string fallbackStartSceneName = "MainframeOSScene";

        public void StartGame()
        {
            if (GameSceneManager.Instance != null)
            {
                GameSceneManager.Instance.LoadMainTable();
                return;
            }

            SceneManager.LoadScene(fallbackStartSceneName);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
