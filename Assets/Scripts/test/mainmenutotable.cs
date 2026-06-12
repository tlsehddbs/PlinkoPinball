using UnityEngine;
using UnityEngine.SceneManagement;
using PlinkoPinball.Core.Flow;

public class mainmenutotable : MonoBehaviour
{
    [SerializeField] private string fallbackSceneName = "MainframeOSScene";

    public void Change()
    {
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadMainTable();
            return;
        }

        SceneManager.LoadScene(fallbackSceneName);
    }
}
