#define DEEPFREEZE_SCREENSYSTEM

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Events.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScreenSystem.Runtime
{
    public static class ScreenManager
    {
        public static bool Initialized { get; private set; }
        public static ScreenSettings Settings { get; private set; }
        
        public static bool IsPortrait { get; private set; }

        private static readonly Dictionary<Type, GameScreen> OpenScreens = new();
        private static readonly Dictionary<PopupPriority, PopupCanvasController> PopupControllers = new();
        
        public static bool BlockHighPriorityPopups { get; private set; }
        public static bool BlockMediumPriorityPopups { get; private set; }

        private static IScreenProvider _screenProvider;

        #region SCREEN/POPUP IDs
        private static string ScreenPrefix => IsPortrait ? Settings.screenPrefixPort : Settings.screenPrefixLand;
        private static string PopupPrefix => IsPortrait ? Settings.popupPrefixPort : Settings.popupPrefixLand;
        
        private static string GetScreenId(Type type)
        {
            return string.IsNullOrEmpty(Settings.constantScreenPrefix)
                ? $"{ScreenPrefix}{type.Name}"
                : $"{Settings.constantScreenPrefix}{ScreenPrefix}{type.Name}";
        }

        private static string GetPopupId(Type type)
        {
            return string.IsNullOrEmpty(Settings.constantPopupPrefix)
                ? $"{PopupPrefix}{type.Name}"
                : $"{Settings.constantPopupPrefix}{PopupPrefix}{type.Name}";
        }
        #endregion

        public static async void Initialize(ScreenSettings settings, IScreenProvider screenProvider)
        {
            Settings = settings;
            
            IsPortrait = Screen.height > Screen.width;
            
            Debug.Log($"Starting screen provider: {screenProvider.GetType().Name}");
            _screenProvider = screenProvider;
            await UniTask.WaitUntil(() => _screenProvider.Initialized);

            SpawnPopupContainers();
            BindEvents();

            Initialized = true;
        }

        private static void SpawnPopupContainers()
        {
            PopupControllers.Clear();

            foreach (var priority in Enum.GetValues(typeof(PopupPriority)))
            {
                var priorityValue = (PopupPriority)priority;
                var newController = Object.Instantiate(Settings.popupCanvasControllerPrefab);
                newController.Initialize(priorityValue);
                PopupControllers.Add(priorityValue, newController);
                Object.DontDestroyOnLoad(newController.gameObject);
                newController.gameObject.SetActive(false);
            }
        }

        private static void BindEvents()
        {
            EventManager.SubscribeEventListener<ScreenClosedEvent>(OnScreenClosed);
            EventManager.SubscribeEventListener<PopupClosedEvent>(OnPopupClosed);
        }

        private static void OnScreenClosed(ScreenClosedEvent screenClosedEvent)
        {
            OpenScreens.Remove(screenClosedEvent.ScreenType);
            UpdateScreenSorting();
        }

        #region SCREENS
        public static async Task<T> ShowScreen<T>() where T : GameScreen
        {
            Debug.Log($"Showing screen: ({GetScreenId(typeof(T))})");
            if (IsScreenOpen<T>(out var screen))
            {
                return screen;
            }

            screen = await _screenProvider.GetScreen<T>(GetScreenId(typeof(T)));
            Object.DontDestroyOnLoad(screen.gameObject);
            OpenScreens.Add(typeof(T), screen);

            EventManager.TriggerEvent(new ScreenOpenedEvent(screen));
            UpdateScreenSorting();
            
            return screen;
        }

        public static bool IsScreenOpen<T>(out T screen) where T : GameScreen
        {
            if (OpenScreens.ContainsKey(typeof(T)))
            {
                screen = (T)OpenScreens[typeof(T)];
                return true;
            }

            screen = null;
            return false;
        }

        public static void CloseScreen<T>() where T : GameScreen
        {
            if (!IsScreenOpen<T>(out var screen))
            {
                return;
            }
            
            screen.Close();
        }
        #endregion

        #region POPUPS
        public static async Task<T> ShowPopup<T>() where T : Popup
        {
            var spawnedPopup = await _screenProvider.GetPopup<T>(GetPopupId(typeof(T)));
            var spawnedPopupTransform = spawnedPopup.transform;
            spawnedPopupTransform.SetParent(PopupControllers[spawnedPopup.priority].popupContainer);
            spawnedPopupTransform.localPosition = Vector3.zero;
            spawnedPopupTransform.localRotation = Quaternion.identity;
            spawnedPopupTransform.localScale = Vector3.one;
            var canvasController = PopupControllers[spawnedPopup.priority];
            spawnedPopup.PopupCanvasController = canvasController;
            EventManager.TriggerEvent(new PopupSpawnedEvent(spawnedPopup));
            UpdateScreenSorting();
            return spawnedPopup;
        }

        public static bool IsPopupShown<T>(out T popup) where T : Popup
        {
            foreach (var popupController in PopupControllers)
            {
                foreach (var canvasPopup in popupController.Value.Popups)
                {
                    if (canvasPopup is T popupAsType)
                    {
                        popup = popupAsType;
                        return true;
                    }
                }
            }
            
            popup = null;
            return false;
        }

        private static void OnPopupClosed(PopupClosedEvent popupClosedEvent)
        {
            UpdateScreenSorting();
        }
        #endregion

        private static async void UpdateScreenSorting()
        {
            await Task.Yield();
            
            BlockHighPriorityPopups = false;
            BlockMediumPriorityPopups = false;
            foreach (var (_, gameScreen) in OpenScreens)
            {
                if (gameScreen.blockHighPriorityPopups)
                {
                    BlockHighPriorityPopups = true;
                }

                if (gameScreen.blockMediumPriorityPopups)
                {
                    BlockMediumPriorityPopups = true;
                }
            }

            Debug.Log($"{(BlockHighPriorityPopups ? "Hide" : "Show")} high priority");
            PopupControllers[PopupPriority.High].UpdatePopupSorting(!BlockHighPriorityPopups);
            Debug.Log($"{(!BlockMediumPriorityPopups && !PopupControllers[PopupPriority.High].gameObject.activeInHierarchy ? "Show" : "Hide")} medium priority");
            PopupControllers[PopupPriority.Medium].UpdatePopupSorting(!BlockMediumPriorityPopups && !PopupControllers[PopupPriority.High].gameObject.activeInHierarchy);
        }
    }
}
