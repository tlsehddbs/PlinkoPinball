using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core;
using PlinkoPinball.Core.Flow;
using PlinkoPinball.Gameplay.Core.Flow;
using PlinkoPinball.Gameplay.Core.Plinko;
using PlinkoPinball.Gameplay.Upgrade;

namespace PlinkoPinball.UI.Mainframe
{
    [DisallowMultipleComponent]
    public sealed class MainframeRuntimeCoordinator : MonoBehaviour
    {
        private const string PinballProcessLabel = "PINBALL.EXE";
        private const string PlinkoProcessLabel = "PLINKO.EXE";
        private const string UpgradeProcessLabel = "UPGRADE.EXE";

        [Header("Runtime Prefabs")]
        [SerializeField] private GameObject pinballRuntimePrefab;
        [SerializeField] private GameObject plinkoBoardRootPrefab;
        [SerializeField] private GameObject plinkoRuntimeSystemsPrefab;

        [Header("Render Textures")]
        [SerializeField] private RenderTexture pinballRenderTexture;
        [SerializeField] private RenderTexture plinkoRenderTexture;

        [Header("Runtime Placement")]
        [SerializeField] private Vector3 pinballRuntimePosition = Vector3.zero;
        [SerializeField] private Vector3 plinkoRuntimePosition = new Vector3(24f, 0f, 0f);

        [Header("Cameras")]
        [SerializeField] private Vector3 pinballCameraPosition = new Vector3(-0.04f, 3.32f, -16.02f);
        [SerializeField] private float pinballCameraOrthographicSize = 8f;
        [SerializeField] private float plinkoCameraOrthographicSize = 7f;

        [Header("Window Flow")]
        [SerializeField] private bool openWindowOnPhaseChange = true;
        [SerializeField] private bool openPinballWindowOnInitialRound;

        private MainframeProcessManager processManager;
        private GameManager gameManager;
        private GameObject runtimeRoot;
        private GameObject pinballRuntimeRoot;
        private GameObject plinkoBoardRoot;
        private GameObject plinkoRuntimeRoot;
        private PlinkoSceneBootstrap plinkoBootstrap;
        private bool runtimeReady;
        private bool subscribed;
        private bool initialPinballRoundStarted;

        public void Initialize(
            MainframeProcessManager manager,
            GameObject pinballPrefab,
            GameObject boardRootPrefab,
            GameObject runtimeSystemsPrefab,
            RenderTexture pinballTexture,
            RenderTexture plinkoTexture)
        {
            processManager = manager;
            pinballRuntimePrefab = pinballPrefab;
            plinkoBoardRootPrefab = boardRootPrefab;
            plinkoRuntimeSystemsPrefab = runtimeSystemsPrefab;
            pinballRenderTexture = pinballTexture;
            plinkoRenderTexture = plinkoTexture;

            if (Application.isPlaying)
            {
                EnsureStarted();
            }
        }

        private void Update()
        {
            if (!Application.isPlaying || runtimeReady)
            {
                return;
            }

            EnsureStarted();
        }

        private void OnDestroy()
        {
            if (subscribed && gameManager != null)
            {
                gameManager.OnPhaseChanged -= HandlePhaseChanged;
            }

            if (processManager != null)
            {
                processManager.ProcessVisibilityChanged -= HandleProcessVisibilityChanged;
            }
        }

        private void EnsureStarted()
        {
            gameManager = EnsureGameManager();
            if (gameManager == null)
            {
                return;
            }

            EnsureRuntimeObjects();
            if (!runtimeReady)
            {
                return;
            }

            SubscribeToPhaseChanges();
            SubscribeToProcessChanges();
            RegisterPinballRuntime();

            if (!initialPinballRoundStarted && gameManager.Phase != GamePhase.Plinko)
            {
                gameManager.SetPhase(GamePhase.Pinball);

                if (openPinballWindowOnInitialRound || processManager == null)
                {
                    processManager?.TryOpenByLabel(PinballProcessLabel);
                    StartInitialPinballRound();
                }
                else
                {
                    processManager?.HideByLabel(PinballProcessLabel);
                }
            }
            else
            {
                HandlePhaseChanged(GamePhase.None, gameManager.Phase);
            }
        }

        private static GameManager EnsureGameManager()
        {
            if (GameManager.Instance != null)
            {
                return GameManager.Instance;
            }

            GameObject gameManagerObject = new GameObject("GameManager");
            return gameManagerObject.AddComponent<GameManager>();
        }

        private void EnsureRuntimeObjects()
        {
            if (runtimeReady)
            {
                return;
            }

            if (pinballRuntimePrefab == null || plinkoBoardRootPrefab == null || plinkoRuntimeSystemsPrefab == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[{nameof(MainframeRuntimeCoordinator)}] Missing runtime prefab references.", this);
#endif
                return;
            }

            runtimeRoot = new GameObject("MainframeRuntimeRoot");
            runtimeRoot.transform.SetParent(transform, false);

            pinballRuntimeRoot = Instantiate(pinballRuntimePrefab, pinballRuntimePosition, Quaternion.identity, runtimeRoot.transform);
            pinballRuntimeRoot.name = pinballRuntimePrefab.name;

            plinkoBoardRoot = Instantiate(plinkoBoardRootPrefab, plinkoRuntimePosition, Quaternion.identity, runtimeRoot.transform);
            plinkoBoardRoot.name = plinkoBoardRootPrefab.name;

            plinkoRuntimeRoot = Instantiate(plinkoRuntimeSystemsPrefab, plinkoRuntimePosition, Quaternion.identity, runtimeRoot.transform);
            plinkoRuntimeRoot.name = plinkoRuntimeSystemsPrefab.name;

            CreateRenderCamera(
                "PinballRenderCamera",
                pinballCameraPosition,
                pinballCameraOrthographicSize,
                pinballRenderTexture);

            CreateRenderCamera(
                "PlinkoRenderCamera",
                plinkoRuntimePosition + new Vector3(0f, 0f, -15f),
                plinkoCameraOrthographicSize,
                plinkoRenderTexture);

            ConfigurePlinkoRuntime();
            runtimeReady = true;
        }

        private void CreateRenderCamera(string cameraName, Vector3 position, float orthographicSize, RenderTexture targetTexture)
        {
            GameObject cameraObject = new GameObject(cameraName, typeof(Camera));
            cameraObject.transform.SetParent(runtimeRoot.transform, false);
            cameraObject.transform.position = position;

            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.orthographicSize = orthographicSize;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.targetTexture = targetTexture;
            camera.depth = -10f;
        }

        private void ConfigurePlinkoRuntime()
        {
            PlinkoBoardLayoutRoot layoutRoot = plinkoBoardRoot.GetComponent<PlinkoBoardLayoutRoot>();
            Transform pinRoot = FindChildByName(plinkoBoardRoot.transform, "PinRoot");
            Transform slotRoot = FindChildByName(plinkoBoardRoot.transform, "SlotRoot");

            PlinkoBoardRuntimeGenerator runtimeGenerator = plinkoRuntimeRoot.GetComponent<PlinkoBoardRuntimeGenerator>();
            if (runtimeGenerator != null)
            {
                runtimeGenerator.ConfigureRuntimeRoots(layoutRoot, pinRoot, slotRoot);
            }

            PlinkoBallSpawner ballSpawner = plinkoRuntimeRoot.GetComponent<PlinkoBallSpawner>();
            if (ballSpawner != null && layoutRoot != null)
            {
                ballSpawner.ConfigureSpawnAnchor(layoutRoot.BallSpawnAnchor);
            }

            PlinkoBoardStateApplier stateApplier = plinkoRuntimeRoot.GetComponent<PlinkoBoardStateApplier>();
            PlinkoRunController runController = plinkoRuntimeRoot.GetComponent<PlinkoRunController>();

            plinkoBootstrap = plinkoRuntimeRoot.GetComponent<PlinkoSceneBootstrap>();
            if (plinkoBootstrap == null)
            {
                plinkoBootstrap = plinkoRuntimeRoot.AddComponent<PlinkoSceneBootstrap>();
            }

            plinkoBootstrap.Configure(
                runtimeGenerator,
                stateApplier,
                runController,
                UpgradeEffectResolver.Instance,
                false);
        }

        private void SubscribeToPhaseChanges()
        {
            if (subscribed)
            {
                return;
            }

            gameManager.OnPhaseChanged += HandlePhaseChanged;
            subscribed = true;
        }

        private void SubscribeToProcessChanges()
        {
            if (processManager == null)
            {
                return;
            }

            processManager.ProcessVisibilityChanged -= HandleProcessVisibilityChanged;
            processManager.ProcessVisibilityChanged += HandleProcessVisibilityChanged;
        }

        private void HandleProcessVisibilityChanged(string label, bool isVisible)
        {
            if (!isVisible || initialPinballRoundStarted)
            {
                return;
            }

            if (label == PinballProcessLabel && gameManager != null && gameManager.Phase == GamePhase.Pinball)
            {
                StartInitialPinballRound();
            }
        }

        private void StartInitialPinballRound()
        {
            if (initialPinballRoundStarted || gameManager == null)
            {
                return;
            }

            initialPinballRoundStarted = true;
            gameManager.StartPinballRound();
        }

        private void RegisterPinballRuntime()
        {
            ScoreSystem scoreSystem = pinballRuntimeRoot.GetComponentInChildren<ScoreSystem>(true);
            PinballToPlinkoTransitionController transitionController = pinballRuntimeRoot.GetComponentInChildren<PinballToPlinkoTransitionController>(true);

            MonoBehaviour[] behaviours = pinballRuntimeRoot.GetComponentsInChildren<MonoBehaviour>(true);
            List<IRoundResettable> roundResettables = new List<IRoundResettable>(behaviours.Length);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IRoundResettable resettable)
                {
                    roundResettables.Add(resettable);
                }
            }

            gameManager.RegisterPinballScene(scoreSystem, transitionController, roundResettables);
        }

        private void HandlePhaseChanged(GamePhase previous, GamePhase next)
        {
            if (!openWindowOnPhaseChange || processManager == null)
            {
                if (next == GamePhase.Plinko)
                {
                    BeginPlinkoRun();
                }

                return;
            }

            switch (next)
            {
                case GamePhase.Pinball:
                    processManager.HideByLabel(PlinkoProcessLabel);
                    processManager.HideByLabel(UpgradeProcessLabel);
                    if (openPinballWindowOnInitialRound || previous == GamePhase.Plinko)
                    {
                        processManager.TryOpenByLabel(PinballProcessLabel);
                    }
                    break;

                case GamePhase.Plinko:
                    processManager.HideByLabel(PinballProcessLabel);
                    processManager.TryOpenByLabel(PlinkoProcessLabel);
                    BeginPlinkoRun();
                    break;
            }
        }

        private void BeginPlinkoRun()
        {
            if (plinkoBootstrap == null)
            {
                return;
            }

            plinkoBootstrap.BeginFromPendingContext();
        }

        private static Transform FindChildByName(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null && children[i].name == childName)
                {
                    return children[i];
                }
            }

            return null;
        }
    }
}
