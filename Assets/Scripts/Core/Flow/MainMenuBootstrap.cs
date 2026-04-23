using UnityEngine;
using PlinkoPinball.Core;

namespace PlinkoPinball.Gameplay.Core.Flow
{
    /// <summary>
    /// 메인 메뉴 씬 초기화를 담당합니다.
    /// </summary>
    public sealed class MainMenuBootstrap : MonoBehaviour
    {
        private void Start()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.SetPhase(GamePhase.MainMenu);
            //GameManager.Instance.InputRouter?.BindPinballTargets(null);
            GameManager.Instance.InputRouter?.ApplyPhase(GamePhase.MainMenu);
        }
    }
}