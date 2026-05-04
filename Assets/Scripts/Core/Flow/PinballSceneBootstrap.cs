using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Gameplay.Core.Flow;

namespace PlinkoPinball.Core.Flow
{
    /// <summary>
    /// 핀볼 씬 초기화를 담당합니다.
    /// </summary>
    public sealed class PinballSceneBootstrap : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private PinballToPlinkoTransitionController transitionController;

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            if (scoreSystem == null)
            {
                scoreSystem = FindFirstObjectByType<ScoreSystem>();
            }

            if (transitionController == null)
            {
                transitionController = FindFirstObjectByType<PinballToPlinkoTransitionController>();
            }

            MonoBehaviour[] sceneBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            List<IRoundResettable> roundResettables = new List<IRoundResettable>();

            for (int i = 0; i < sceneBehaviours.Length; i++)
            {
                if (sceneBehaviours[i] is IRoundResettable resettable)
                {
                    roundResettables.Add(resettable);
                }
            }

            GameManager.Instance.RegisterPinballScene(
                scoreSystem,
                transitionController,
                roundResettables);

            //GameManager.Instance.InputRouter?.ApplyPhase(GamePhase.Pinball);
            GameManager.Instance.StartPinballRound();
        }
    }
}