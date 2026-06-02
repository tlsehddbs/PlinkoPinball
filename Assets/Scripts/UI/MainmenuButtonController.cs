using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlinkoPinball.UI.MainMenu
{
    public sealed class MainMenuButtonController : MonoBehaviour
    {

        public void StartGame()
        {
            SceneManager.LoadScene("PinballScene");
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