using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core;
using PlinkoPinball.Core.Flow;
using PlinkoPinball.Core.Grant;
using PlinkoPinball.Core.Rewards;
using PlinkoPinball.Gameplay;
using PlinkoPinball.Gameplay.Core.Compression;
using PlinkoPinball.Gameplay.Core.Flow;
using PlinkoPinball.Gameplay.Components.Plinko;
using PlinkoPinball.Gameplay.Core.Plinko;
using PlinkoPinball.Gameplay.Modules;
using PlinkoPinball.Gameplay.Upgrade;
using PlinkoPinball.Pinball;
using PlinkoPinball.UI;

namespace PlinkoPinball.UI.Mainframe
{
    [DisallowMultipleComponent]
    public sealed class MainframeRuntimeCoordinator : MonoBehaviour
    {
        private const string PinballProcessLabel = "PINBALL.EXE";
        private const string PlinkoProcessLabel = "PLINKO.EXE";
        private const string UpgradeProcessLabel = "UPGRADE.EXE";

        [Header("Runtime Prefabs")]
        [SerializeField] private GameObject plinkoBoardRootPrefab;
        [SerializeField] private GameObject plinkoRuntimeSystemsPrefab;
        [SerializeField] private CompressionSettings compressionSettings;

        [Header("Render Textures")]
        [SerializeField] private RenderTexture pinballRenderTexture;
        [SerializeField] private RenderTexture plinkoRenderTexture;

        [Header("Runtime Placement")]
        [SerializeField] private Vector3 pinballRuntimePosition = Vector3.zero;
        [SerializeField] private Vector3 plinkoRuntimePosition = new Vector3(-30f, 0f, 0f);

        [Header("Cameras")]
        [SerializeField] private Vector3 pinballCameraPosition = new Vector3(-0.04f, 3.32f, -16.02f);
        [SerializeField] private float pinballCameraOrthographicSize = 8f;
        [SerializeField] private float plinkoCameraOrthographicSize = 7f;

        [Header("Window Flow")]
        [SerializeField] private bool openWindowOnPhaseChange = true;
        [SerializeField] private bool openPinballWindowOnInitialRound;
        [SerializeField] private bool resetPinballWhenWindowCloses = true;

        private MainframeProcessManager processManager;
        private GameManager gameManager;
        private GameObject runtimeRoot;
        private GameObject pinballRuntimeRoot;
        private GameObject plinkoBoardRoot;
        private GameObject plinkoRuntimeRoot;
        private Camera plinkoRenderCamera;
        private PlinkoSceneBootstrap plinkoBootstrap;
        private PlinkoBoardRuntimeGenerator plinkoRuntimeGenerator;
        private PlinkoBoardStateApplier plinkoStateApplier;
        private bool runtimeReady;
        private bool plinkoRuntimeReady;
        private bool plinkoBoardGenerated;
        private bool subscribed;
        private bool initialPinballRoundStarted;

        public void Initialize(
            MainframeProcessManager manager,
            GameObject boardRootPrefab,
            GameObject runtimeSystemsPrefab,
            CompressionSettings compressionSettingsAsset,
            RenderTexture pinballTexture,
            RenderTexture plinkoTexture)
        {
            processManager = manager;
            plinkoBoardRootPrefab = boardRootPrefab;
            plinkoRuntimeSystemsPrefab = runtimeSystemsPrefab;
            compressionSettings = compressionSettingsAsset;
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
            RebindStatusPresenters();

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

            runtimeRoot = new GameObject("MainframeRuntimeRoot");
            runtimeRoot.transform.SetParent(transform, false);

            pinballRuntimeRoot = ResolveWorldPinballRuntimeRoot();
            if (pinballRuntimeRoot == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[{nameof(MainframeRuntimeCoordinator)}] Missing world pinball runtime. Place a Pinball object with {nameof(BallSpawner)} in the scene.", this);
#endif
                return;
            }

            EnsurePinballGlobalSystems();
            ConfigurePinballPlinkoBonusWiring();
            ConfigurePinballRenderCamera();

            runtimeReady = true;
        }

        private GameObject ResolveWorldPinballRuntimeRoot()
        {
            BallSpawner[] spawners = FindObjectsByType<BallSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < spawners.Length; i++)
            {
                BallSpawner spawner = spawners[i];
                if (spawner == null)
                {
                    continue;
                }

                Transform root = spawner.transform.root;
                if (root == transform || root == runtimeRoot.transform)
                {
                    continue;
                }

                return root.gameObject;
            }

            return null;
        }

        private void ConfigurePinballRenderCamera()
        {
            Camera existingCamera = pinballRuntimeRoot.GetComponentInChildren<Camera>(true);
            if (existingCamera != null)
            {
                existingCamera.targetTexture = pinballRenderTexture;
                existingCamera.enabled = true;
                return;
            }

            CreateRenderCamera(
                "PinballRenderCamera",
                pinballRuntimeRoot.transform.position + pinballCameraPosition,
                pinballCameraOrthographicSize,
                pinballRenderTexture);
        }

        private Camera CreateRenderCamera(string cameraName, Vector3 position, float orthographicSize, RenderTexture targetTexture)
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
            return camera;
        }

        private bool EnsurePlinkoRuntime()
        {
            if (plinkoRuntimeReady)
            {
                return true;
            }

            if (plinkoBoardRootPrefab == null || plinkoRuntimeSystemsPrefab == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[{nameof(MainframeRuntimeCoordinator)}] Missing plinko runtime prefab references.", this);
#endif
                return false;
            }

            if (runtimeRoot == null)
            {
                runtimeRoot = new GameObject("MainframeRuntimeRoot");
                runtimeRoot.transform.SetParent(transform, false);
            }

            plinkoBoardRoot = Instantiate(plinkoBoardRootPrefab, plinkoRuntimePosition, Quaternion.identity, runtimeRoot.transform);
            plinkoBoardRoot.name = plinkoBoardRootPrefab.name;

            plinkoRuntimeRoot = Instantiate(plinkoRuntimeSystemsPrefab, plinkoRuntimePosition, Quaternion.identity, runtimeRoot.transform);
            plinkoRuntimeRoot.name = plinkoRuntimeSystemsPrefab.name;

            plinkoRenderCamera = CreateRenderCamera(
                "PlinkoRenderCamera",
                plinkoRuntimePosition + new Vector3(0f, 0f, -15f),
                plinkoCameraOrthographicSize,
                plinkoRenderTexture);

            ConfigurePlinkoRuntime();
            plinkoRuntimeReady = true;
            SetPlinkoRuntimeActive(false);
            return true;
        }

        private void ConfigurePlinkoRuntime()
        {
            PlinkoBoardLayoutRoot layoutRoot = plinkoBoardRoot.GetComponent<PlinkoBoardLayoutRoot>();
            Transform pinRoot = FindChildByName(plinkoBoardRoot.transform, "PinRoot");
            Transform slotRoot = FindChildByName(plinkoBoardRoot.transform, "SlotRoot");

            plinkoRuntimeGenerator = plinkoRuntimeRoot.GetComponent<PlinkoBoardRuntimeGenerator>();
            if (plinkoRuntimeGenerator != null)
            {
                plinkoRuntimeGenerator.ConfigureRuntimeRoots(layoutRoot, pinRoot, slotRoot);
            }

            PlinkoBallSpawner ballSpawner = plinkoRuntimeRoot.GetComponent<PlinkoBallSpawner>();
            if (ballSpawner != null && layoutRoot != null)
            {
                ballSpawner.ConfigureSpawnAnchor(layoutRoot.BallSpawnAnchor);
            }

            plinkoStateApplier = plinkoRuntimeRoot.GetComponent<PlinkoBoardStateApplier>();
            PlinkoRunController runController = plinkoRuntimeRoot.GetComponent<PlinkoRunController>();

            plinkoBootstrap = plinkoRuntimeRoot.GetComponent<PlinkoSceneBootstrap>();
            if (plinkoBootstrap == null)
            {
                plinkoBootstrap = plinkoRuntimeRoot.AddComponent<PlinkoSceneBootstrap>();
            }

            plinkoBootstrap.Configure(
                plinkoRuntimeGenerator,
                plinkoStateApplier,
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
            if (label == PlinkoProcessLabel)
            {
                if (isVisible)
                {
                    EnsurePlinkoRuntime();
                    SetPlinkoRuntimeActive(true);
                    EnsurePlinkoBoardGenerated();
                }
                else
                {
                    SetPlinkoRuntimeActive(false);
                }

                return;
            }

            if (label == PinballProcessLabel && !isVisible)
            {
                ResetPinballRuntimeForWindowClose();
                return;
            }

            if (label == PinballProcessLabel && isVisible)
            {
                HandlePinballProcessOpened();
                return;
            }

            if (!isVisible || initialPinballRoundStarted)
            {
                return;
            }

            if (label == PinballProcessLabel && gameManager != null && gameManager.Phase == GamePhase.Pinball)
            {
                StartInitialPinballRound();
            }
        }

        private void HandlePinballProcessOpened()
        {
            if (gameManager == null)
            {
                return;
            }

            if (gameManager.Phase == GamePhase.Plinko)
            {
                initialPinballRoundStarted = true;
                gameManager.CompletePlinkoAndReturnToPinball();
                return;
            }

            if (!initialPinballRoundStarted && gameManager.Phase == GamePhase.Pinball)
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

        private void ResetPinballRuntimeForWindowClose()
        {
            if (!resetPinballWhenWindowCloses || pinballRuntimeRoot == null)
            {
                return;
            }

            PlungerLauncher[] launchers = pinballRuntimeRoot.GetComponentsInChildren<PlungerLauncher>(true);
            for (int i = 0; i < launchers.Length; i++)
            {
                launchers[i]?.ClearBall();
            }

            PinballFlipper[] flippers = pinballRuntimeRoot.GetComponentsInChildren<PinballFlipper>(true);
            for (int i = 0; i < flippers.Length; i++)
            {
                flippers[i]?.ResetToRest();
            }

            ModuleRoot[] moduleRoots = pinballRuntimeRoot.GetComponentsInChildren<ModuleRoot>(true);
            for (int i = 0; i < moduleRoots.Length; i++)
            {
                moduleRoots[i]?.ResetModuleState();
            }

            BallSpawner[] spawners = pinballRuntimeRoot.GetComponentsInChildren<BallSpawner>(true);
            for (int i = 0; i < spawners.Length; i++)
            {
                spawners[i]?.Spawn();
            }
        }

        private void RegisterPinballRuntime()
        {
            EnsurePinballGlobalSystems();
            ConfigurePinballPlinkoBonusWiring();

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

        private void EnsurePinballGlobalSystems()
        {
            if (pinballRuntimeRoot == null)
            {
                return;
            }

            Transform systemsRoot = ResolvePinballSystemsRoot();
            PlinkoRunSnapshotBuilder snapshotBuilder = GetOrCreatePinballSystem<PlinkoRunSnapshotBuilder>(systemsRoot, "PlinkoRunSnapshotBuilder");

            ScoreSystem scoreSystem = GetOrCreatePinballSystem<ScoreSystem>(systemsRoot, "ScoreSystem");
            if (scoreSystem == null)
            {
                return;
            }

            CompressionSystem compressionSystem = GetOrCreatePinballSystem<CompressionSystem>(systemsRoot, "CompressionSystem");
            if (compressionSystem != null)
            {
                compressionSystem.ConfigureSettings(compressionSettings);
                compressionSystem.ConfigureSnapshotBuilder(snapshotBuilder);
            }

            PinballToPlinkoTransitionController transitionController = GetOrCreatePinballSystem<PinballToPlinkoTransitionController>(systemsRoot, "PinballToPlinkoTransitionController");
            if (transitionController != null)
            {
                transitionController.ConfigureSnapshotBuilder(snapshotBuilder);
            }
        }

        private Transform ResolvePinballSystemsRoot()
        {
            Transform existing = pinballRuntimeRoot.transform.Find("MainframePinballSystems");
            if (existing != null)
            {
                return existing;
            }

            GameObject systemsObject = new GameObject("MainframePinballSystems");
            systemsObject.transform.SetParent(pinballRuntimeRoot.transform, false);
            systemsObject.transform.localPosition = Vector3.zero;
            systemsObject.transform.localRotation = Quaternion.identity;
            systemsObject.transform.localScale = Vector3.one;
            return systemsObject.transform;
        }

        private T GetOrCreatePinballSystem<T>(Transform systemsRoot, string objectName) where T : Component
        {
            T existing = pinballRuntimeRoot.GetComponentInChildren<T>(true);
            if (existing != null)
            {
                return existing;
            }

            Transform child = systemsRoot.Find(objectName);
            GameObject systemObject;
            if (child != null)
            {
                systemObject = child.gameObject;
            }
            else
            {
                systemObject = new GameObject(objectName);
                systemObject.transform.SetParent(systemsRoot, false);
                systemObject.transform.localPosition = Vector3.zero;
                systemObject.transform.localRotation = Quaternion.identity;
                systemObject.transform.localScale = Vector3.one;
            }

            return systemObject.GetComponent<T>() ?? systemObject.AddComponent<T>();
        }

        private static void RebindStatusPresenters()
        {
            TopBarPresenter[] presenters = FindObjectsByType<TopBarPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < presenters.Length; i++)
            {
                presenters[i]?.Rebind();
            }
        }

        private void ConfigurePinballPlinkoBonusWiring()
        {
            if (pinballRuntimeRoot == null)
            {
                return;
            }

            PlinkoRunSnapshotBuilder snapshotBuilder = pinballRuntimeRoot.GetComponentInChildren<PlinkoRunSnapshotBuilder>(true);
            if (snapshotBuilder == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[{nameof(MainframeRuntimeCoordinator)}] Missing PlinkoRunSnapshotBuilder in pinball runtime.", this);
#endif
                return;
            }

            PinballToPlinkoTransitionController[] transitionControllers = pinballRuntimeRoot.GetComponentsInChildren<PinballToPlinkoTransitionController>(true);
            for (int i = 0; i < transitionControllers.Length; i++)
            {
                transitionControllers[i].ConfigureSnapshotBuilder(snapshotBuilder);
            }

            GrantSystem[] grantSystems = pinballRuntimeRoot.GetComponentsInChildren<GrantSystem>(true);
            for (int i = 0; i < grantSystems.Length; i++)
            {
                grantSystems[i].ConfigureSnapshotBuilder(snapshotBuilder);
            }

            PinballRewardSystem[] rewardSystems = pinballRuntimeRoot.GetComponentsInChildren<PinballRewardSystem>(true);
            for (int i = 0; i < rewardSystems.Length; i++)
            {
                rewardSystems[i].ConfigureSnapshotBuilder(snapshotBuilder);
            }

            CompressionSystem[] compressionSystems = pinballRuntimeRoot.GetComponentsInChildren<CompressionSystem>(true);
            for (int i = 0; i < compressionSystems.Length; i++)
            {
                compressionSystems[i].ConfigureSnapshotBuilder(snapshotBuilder);
            }
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
                    EnsurePlinkoRuntime();
                    SetPlinkoRuntimeActive(true);
                    EnsurePlinkoBoardGenerated();
                    BeginPlinkoRun();
                    break;
            }
        }

        private void BeginPlinkoRun()
        {
            EnsurePlinkoRuntime();

            if (plinkoBootstrap == null)
            {
                return;
            }

            if (plinkoBootstrap.BeginFromPendingContext())
            {
                plinkoBoardGenerated = true;
            }
        }

        private void EnsurePlinkoBoardGenerated()
        {
            if (plinkoBoardGenerated)
            {
                return;
            }

            if (plinkoRuntimeGenerator == null || plinkoStateApplier == null)
            {
                return;
            }

            plinkoRuntimeGenerator.GenerateBoard(out PlinkoPinRuntime[] generatedPins, out PlinkoSlotRuntime[] generatedSlots);
            plinkoStateApplier.RegisterRuntimeObjects(generatedPins, generatedSlots);
            plinkoStateApplier.ResetBoardState();
            plinkoBoardGenerated = true;
        }

        private void SetPlinkoRuntimeActive(bool active)
        {
            if (plinkoBoardRoot != null)
            {
                plinkoBoardRoot.SetActive(active);
            }

            if (plinkoRuntimeRoot != null)
            {
                plinkoRuntimeRoot.SetActive(active);
            }

            if (plinkoRenderCamera != null)
            {
                plinkoRenderCamera.enabled = active;
            }
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
