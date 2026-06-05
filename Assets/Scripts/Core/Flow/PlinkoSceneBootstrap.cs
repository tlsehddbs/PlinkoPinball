using UnityEngine;
using PlinkoPinball.Gameplay.Components.Plinko;
using PlinkoPinball.Gameplay.Core.Plinko;
using PlinkoPinball.Gameplay.Core.Flow;
using PlinkoPinball.Gameplay.Upgrade;

namespace PlinkoPinball.Core.Flow
{
    public sealed class PlinkoSceneBootstrap : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField] private PlinkoBoardRuntimeGenerator boardRuntimeGenerator;
        [SerializeField] private PlinkoBoardStateApplier boardStateApplier;
        [SerializeField] private PlinkoRunController runController;
        [SerializeField] private UpgradeEffectResolver upgradeEffectResolver;
        [SerializeField] private bool startOnSceneLoad = true;

        [Header("Debug")]
        [SerializeField] private bool useDebugSnapshotInEditor;
        [SerializeField] private PlinkoDebugSnapshotProvider debugSnapshotProvider;

        private void Start()
        {
            if (!startOnSceneLoad)
            {
                return;
            }

            BeginFromPendingContext();
        }

        public void Configure(
            PlinkoBoardRuntimeGenerator runtimeGenerator,
            PlinkoBoardStateApplier stateApplier,
            PlinkoRunController controller,
            UpgradeEffectResolver resolver,
            bool autoStart)
        {
            boardRuntimeGenerator = runtimeGenerator;
            boardStateApplier = stateApplier;
            runController = controller;
            upgradeEffectResolver = resolver;
            startOnSceneLoad = autoStart;
        }

        public bool BeginFromPendingContext()
        {
            if (GameManager.Instance == null)
            {
                return false;
            }

            ResolveUpgradeEffectResolver();

            GameManager.Instance.SetPhase(GamePhase.Plinko);
            //GameManager.Instance.InputRouter?.BindPinballTargets(null);
            //GameManager.Instance.InputRouter?.ApplyPhase(GamePhase.Plinko);

            if (!TryGetContext(out PlinkoPhaseHandoffContext context))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[{nameof(PlinkoSceneBootstrap)}] No valid handoff context found.", this);
#endif
                return false;
            }

            if (boardRuntimeGenerator == null || boardStateApplier == null || runController == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[{nameof(PlinkoSceneBootstrap)}] Missing scene references.", this);
#endif
                return false;
            }

            boardRuntimeGenerator.GenerateBoard(out PlinkoPinRuntime[] generatedPins, out PlinkoSlotRuntime[] generatedSlots);

            boardStateApplier.RegisterRuntimeObjects(generatedPins, generatedSlots);

            PlinkoBoardAppliedSnapshot appliedSnapshot = PlinkoAppliedSnapshotBuilder.Build(
                    generatedPins,
                    generatedSlots,
                    context.RunSnapshot,
                    upgradeEffectResolver);

            boardStateApplier.ResetBoardState();
            boardStateApplier.ApplySnapshot(appliedSnapshot);
            Debug.Log("[PlinkoBoardStateApplier] ApplySnapshot completed. Refreshing visuals.", this);

            runController.BeginRun(context.RoundIndex, context.PinballScore, appliedSnapshot.StartBalls);
            return true;
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

        private void ResolveUpgradeEffectResolver()
        {
            if (upgradeEffectResolver != null)
            {
                return;
            }

            upgradeEffectResolver = UpgradeEffectResolver.Instance;
            if (upgradeEffectResolver != null)
            {
                return;
            }

            upgradeEffectResolver = FindFirstObjectByType<UpgradeEffectResolver>();
        }
    }
}
