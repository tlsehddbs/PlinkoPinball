using UnityEngine;
using UnityEngine.SceneManagement;
using PlinkoPinball.Gameplay.Core.Plinko;
using PlinkoPinball.Core;

namespace PlinkoPinball.Gameplay.Core.Flow
{
    /// <summary>
    /// 핀볼 라운드 종료 시 플링코 씬으로 전환
    /// </summary>
    public sealed class PinballToPlinkoTransitionController : MonoBehaviour
    {
        [SerializeField] private PlinkoRunSnapshotBuilder plinkoSnapshotBuilder;
        [SerializeField] private string plinkoSceneName = "PlinkoScene";
        [SerializeField] private bool loadSceneOnTransition;

        /// <summary>
        /// 플링코 씬으로 전환
        /// </summary>
        /// <param name="roundIndex">현재 라운드 인덱스</param>
        /// <param name="pinballScore">현재 핀볼 점수</param>
        public void TransitionToPlinko(int roundIndex, int pinballScore)
        {
            if (plinkoSnapshotBuilder == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("[PinballToPlinkoTransitionController] Missing snapshot builder.", this);
#endif
                return;
            }

            if (PhaseHandoffService.Instance == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("[PinballToPlinkoTransitionController] Missing handoff service.", this);
#endif
                return;
            }

            PlinkoRunSnapshot snapshot = plinkoSnapshotBuilder.BuildSnapshot();

            var context = new PlinkoPhaseHandoffContext(roundIndex, pinballScore, snapshot);

            PhaseHandoffService.Instance.SetPlinkoContext(context);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[PinballToPlinkoTransitionController] Transitioning to Plinko phase | Round={roundIndex} | Score={pinballScore}", this);
#endif

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPhase(GamePhase.Plinko);
            }

            if (loadSceneOnTransition)
            {
                SceneManager.LoadScene(plinkoSceneName);
            }
        }
    }
}
