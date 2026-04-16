using UnityEngine;
using PlinkoPinball.Gameplay.Core.Flow;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 씬 진입 시 전달된 스냅샷을 해석하고 런을 시작
    /// </summary>
    public sealed class PlinkoSceneBootstrapper : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField] private PlinkoBoardDefinition boardDefinition;
        [SerializeField] private PlinkoBoardStateApplier boardStateApplier;
        [SerializeField] private PlinkoRunController runController;

        [Header("Debug")]
        [SerializeField] private bool useDebugSnapshotInEditor;
        [SerializeField] private PlinkoDebugSnapshotProvider debugSnapshotProvider;

        private void Start()
        {
            if (!TryGetContext(out PlinkoPhaseHandoffContext context))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[{nameof(PlinkoSceneBootstrapper)}] No handoff context found.", this);
#endif
                return;
            }

            if (boardDefinition == null || boardStateApplier == null || runController == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[{nameof(PlinkoSceneBootstrapper)}] Missing required reference.", this);
#endif
                return;
            }

            PlinkoBoardAppliedSnapshot appliedSnapshot =
                PlinkoAppliedSnapshotBuilder.Build(boardDefinition, context.RunSnapshot);

            boardStateApplier.ResetBoardState();
            boardStateApplier.ApplySnapshot(appliedSnapshot);

            runController.BeginRun(
                context.RoundIndex,
                context.PinballScore,
                appliedSnapshot.StartBalls,
                appliedSnapshot.GlobalCurrencyPerPinHit);
        }

        private bool TryGetContext(out PlinkoPhaseHandoffContext context)
        {
            context = default;

            if (PhaseHandoffService.Instance != null &&
                PhaseHandoffService.Instance.TryConsumePlinkoContext(out context))
            {
                return true;
            }

#if UNITY_EDITOR
            if (useDebugSnapshotInEditor && debugSnapshotProvider != null)
            {
                context = debugSnapshotProvider.BuildDebugContext();
                return true;
            }
#endif

            return false;
        }
    }
}