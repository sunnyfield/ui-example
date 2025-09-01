using System;
using Game.Scripts.DataModel;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.LowLevel;
using UnityEngine.UIElements;
using Game.Scripts.UISystem;
using Debug = UnityEngine.Debug;

namespace Game.Scripts
{
    public static class GameRoot
    {
        private enum GamePhase : byte
        {
            Start = 0,
            ConfigLoaded = 1,
            UILoaded = 2,
            Ready = 3,
            ConfigFailed = 4,
            UIFailed = 5,
            Failed = 6
        }
        
        public struct GameConfigData
        {
            public NativeArray<int> DifficultySizes;
        }

        private static GamePhase s_phase;
        private static UIDocument s_uiView;
        
        private static GameConfigData s_configData;
        private static PersistentPlayerData s_playerData;
        
        private static NativeQueue<UIUpdateCommand> s_uiUpdateQueue;
        private static NativeQueue<UIUpdateCommand> s_uiActionQueue;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Initialize()
        {
            s_uiUpdateQueue = new NativeQueue<UIUpdateCommand>(Allocator.Persistent);
            s_uiActionQueue = new NativeQueue<UIUpdateCommand>(Allocator.Persistent);
            
            InitializeGameContentAsync();
            
            Application.quitting += OnApplicationQuitting;
            
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            InsertGameUpdate(ref playerLoop);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeUI()
        {
            var gameUIObject = GameObject.Find("GameUI");
            
            if (gameUIObject != null)
            {
                s_uiView = gameUIObject.GetComponent<UIDocument>();
                UpdateInitializationPhaseAndRegisterOnReady();
            }
            else
            {
                s_phase = (s_phase != GamePhase.ConfigFailed) ? GamePhase.UIFailed : GamePhase.Failed;
            }
        }
        
        private static void OnApplicationQuitting()
        {
            Cleanup();
        }
        
        private static void Cleanup()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            RemoveGameUpdate(ref playerLoop);
            PlayerLoop.SetPlayerLoop(playerLoop);
            
            if (s_configData.DifficultySizes.IsCreated)
            {
                s_configData.DifficultySizes.Dispose();
            }
            
            if (s_uiUpdateQueue.IsCreated) s_uiUpdateQueue.Dispose();
            if (s_uiActionQueue.IsCreated) s_uiActionQueue.Dispose();
            
            UIManager.Cleanup();
            AddressableAssetManager.Cleanup();
            
            Application.quitting -= OnApplicationQuitting;
        }
        
        private static void UpdateInitializationPhaseAndRegisterOnReady()
        {
            int hasConfig = (s_configData.DifficultySizes.IsCreated) ? 1 : 0;
            int hasUI = (s_uiView != null) ? 1 : 0;
            
            byte newPhase = (byte)(hasConfig + hasUI * 2);
            
            if (newPhase <= 3)
            {
                s_phase = (GamePhase)newPhase;
            }
            
            if (s_phase == GamePhase.Ready)
            {
                VisualElement uiViewRootVisualElement = s_uiView.rootVisualElement;
                
                UIManager.RegisterElements(uiViewRootVisualElement, s_configData.DifficultySizes);
                UIEventScheduler.ScheduleIconChange(UIElementId.PlayerScoreIcon, AddressableAssetManager.s_coinIcon);
                UIEventScheduler.ScheduleVisibilityChange(UIElementId.PuzzlePictureView, true);
                UIEventScheduler.ScheduleVisibilityChange(UIElementId.PuzzleStartView, true);
                UIEventScheduler.ScheduleScoreUpdate(s_playerData.Score);
            }
        }
        
        private static ushort s_frameCounter;
        private static readonly System.Random s_randomGenerator = new ();
        
        private static void GameLoop()
        {
            s_frameCounter++;
            
            if (s_frameCounter % 500 == 0) // score changes once per 500 frames
            {
                int newScore = s_randomGenerator.Next(0, 2000);
                s_playerData.Score = newScore;
                
                UIEventScheduler.ScheduleScoreUpdate(newScore);
                UIEventScheduler.ScheduleIconChange(UIElementId.PlayButtonIcon,
                    s_playerData.Score < 1000 ? AddressableAssetManager.s_adsIcon : AddressableAssetManager.s_coinIcon);
            }
            
            ProcessUIActions();
            
            ProcessUIUpdates();
        }
        
        private static void ProcessUIActions()
        {
            int processedCount = 0;
            while (s_uiActionQueue.TryDequeue(out UIUpdateCommand action) && processedCount < 16)
            {
                ProcessUserAction(action);
                processedCount++;
            }
        }
        
        private static void ProcessUIUpdates()
        {
            int processedCount = 0;
            while (s_uiUpdateQueue.TryDequeue(out UIUpdateCommand command) && processedCount < 64)
            {
                UIManager.ProcessUIUpdate(command);
                processedCount++;
            }
        }
        
        private static void ProcessUserAction(UIUpdateCommand action)
        {
            switch (action.EventType)
            {
                case UIEventType.PlayButtonClicked:
                    UIEventScheduler.ScheduleVisibilityChange(UIElementId.PuzzleStartView, false);
                    Debug.Log(
                        $"With difficulty selected: index {UIManager.s_selectedDifficultyIndex.ToString()}, size {s_configData.DifficultySizes[UIManager.s_selectedDifficultyIndex].ToString()}");
                    break;
            }
        }
        
        public static void EnqueueUIUpdate(UIUpdateCommand command) => s_uiUpdateQueue.Enqueue(command);

        public static void EnqueueUIAction(UIUpdateCommand action) => s_uiActionQueue.Enqueue(action);

        private static void InsertGameUpdate(ref PlayerLoopSystem playerLoop)
        {
            var updateSystem = new PlayerLoopSystem
            {
                type = typeof(GameRoot),
                updateDelegate = GameLoop
            };
            
            var existingSystems = playerLoop.subSystemList[5].subSystemList ?? Array.Empty<PlayerLoopSystem>(); //5 == Update
            var newSystems = new PlayerLoopSystem[existingSystems.Length + 1];
            Array.Copy(existingSystems, newSystems, existingSystems.Length);
            newSystems[existingSystems.Length] = updateSystem;

            playerLoop.subSystemList[5].subSystemList = newSystems;
        }

        private static void RemoveGameUpdate(ref PlayerLoopSystem playerLoop)
        {
                var existingSystems = playerLoop.subSystemList[5].subSystemList;
                if (existingSystems == null) return;

                for (int j = 0; j < existingSystems.Length; j++)
                {
                    if (existingSystems[j].type != typeof(GameRoot)) continue;

                    var newSystems = new PlayerLoopSystem[existingSystems.Length - 1];
                    Array.Copy(existingSystems, 0, newSystems, 0, j);
                    Array.Copy(existingSystems, j + 1, newSystems, j, existingSystems.Length - j - 1);

                    playerLoop.subSystemList[5].subSystemList = newSystems;
                    return;
                }
        }

        private static async void InitializeGameContentAsync()
        {
            try
            {
                var configHandle = Addressables.LoadAssetAsync<ScriptableObject>("config");
                var configSO = await configHandle.Task;
                
                if (configSO is GameConfig config)
                {
                    s_configData = new GameConfigData
                    {
                        DifficultySizes = new NativeArray<int>(config.DifficultySizes, Allocator.Persistent),
                    };
                    
                    s_playerData = new PersistentPlayerData { Score = config.InitialPlayerScore };
                    
                    await AddressableAssetManager.PreloadAssets();
                }
                Addressables.Release(configHandle);
                UpdateInitializationPhaseAndRegisterOnReady();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load config: {e.Message}");
                s_phase = (s_uiView != null) ? GamePhase.ConfigFailed : GamePhase.Failed;
                s_configData = default;
            }
        }
    }
}