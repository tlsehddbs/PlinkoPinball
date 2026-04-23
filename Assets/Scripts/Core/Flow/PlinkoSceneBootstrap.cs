using UnityEngine;
using PlinkoPinball.Core;
using PlinkoPinball.Gameplay.Components.Plinko;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Core.Flow
{
    public sealed class PlinkoSceneBootstrap : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField] private PlinkoBoardRuntimeGenerator boardRuntimeGenerator;
        [SerializeField] private PlinkoBoardStateApplier boardStateApplier;
        [SerializeField] private PlinkoRunController runController;

        [Header("Debug")]
        [SerializeField] private bool useDebugSnapshotInEditor;
        [SerializeField] private PlinkoDebugSnapshotProvider debugSnapshotProvider;

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.SetPhase(GamePhase.Plinko);
            //GameManager.Instance.InputRouter?.BindPinballTargets(null);
            //GameManager.Instance.InputRouter?.ApplyPhase(GamePhase.Plinko);

            if (!TryGetContext(out PlinkoPhaseHandoffContext context))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[{nameof(PlinkoSceneBootstrap)}] No valid handoff context found.", this);
#endif
                return;
            }

            if (boardRuntimeGenerator == null || boardStateApplier == null || runController == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[{nameof(PlinkoSceneBootstrap)}] Missing scene references.", this);
#endif
                return;
            }

            boardRuntimeGenerator.GenerateBoard(
                out PlinkoPinRuntime[] generatedPins,
                out PlinkoSlotRuntime[] generatedSlots);

            boardStateApplier.RegisterRuntimeObjects(generatedPins, generatedSlots);

            PlinkoBoardAppliedSnapshot appliedSnapshot =
                PlinkoAppliedSnapshotBuilder.Build(
                    generatedPins,
                    generatedSlots,
                    context.RunSnapshot);

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

            if (PhaseHandoffService.Instance != null && PhaseHandoffService.Instance.TryConsumePlinkoContext(out context))
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