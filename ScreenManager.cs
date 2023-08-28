#define DEEPFREEZE_SCREENSYSTEM

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeepFreeze.Events;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeepFreeze.ScreenSystem
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
        private static Task _nextSortingUpdate;

        #region SCREEN/POPUP IDs
        private static string ScreenPrefix => IsPortrait ? Settings.screenPrefixPort : Settings.screenPrefixLand;
        private static string PopupPrefix => IsPortrait ? Settings.popupPrefixPort : Settings.popupPrefixLand;
        
        private static string GetScreenId(Type type)
        {
            return string.IsNullOrEmpty(Settings.constantScreenPrefix)
                ? $"{ScreenPrefix}{type.Name}{Settings.screenSuffix}"
                : $"{Settings.constantScreenPrefix}{ScreenPrefix}{type.Name}{Settings.screenSuffix}";
        }

        private static string GetScreenId(string screenName)
        {
            return string.IsNullOrEmpty(Settings.constantScreenPrefix)
                ? $"{ScreenPrefix}{screenName}{Settings.screenSuffix}"
                : $"{Settings.constantScreenPrefix}{ScreenPrefix}{screenName}{Settings.screenSuffix}";
        }

        private static string GetPopupId(Type type)
        {
            return string.IsNullOrEmpty(Settings.constantPopupPrefix)
                ? $"{PopupPrefix}{type.Name}{Settings.popupSuffix}"
                : $"{Settings.constantPopupPrefix}{PopupPrefix}{type.Name}{Settings.popupSuffix}";
        }

        private static string GetPopupId(string popupName)
        {
            return string.IsNullOrEmpty(Settings.constantPopupPrefix)
                ? $"{PopupPrefix}{popupName}{Settings.popupSuffix}"
                : $"{Settings.constantPopupPrefix}{PopupPrefix}{popupName}{Settings.popupSuffix}";
        }
        #endregion

        public static async void Initialize(ScreenSettings settings, IScreenProvider screenProvider)
        {
            if (Initialized)
            {
                Debug.LogError($"Trying to initialize {nameof(ScreenManager)} but it is already initialized");
                return;
            }
            
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

        public static async Task<GameScreen> ShowScreen(Type screenType)
        {
            if (!screenType.IsAssignableFrom(typeof(GameScreen)))
            {
                Debug.LogError($"Trying to show screen by type but the type given is not an implementation of type: {nameof(GameScreen)}");
                return null;
            }

            if (IsScreenOpen(screenType, out var screen))
            {
                return screen;
            }

            screen = await _screenProvider.GetScreen<GameScreen>(GetScreenId(screenType));
            Object.DontDestroyOnLoad(screen.gameObject);
            OpenScreens.Add(screenType, screen);

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

        public static bool IsScreenOpen(Type screenType, out GameScreen screen)
        {
            if (!screenType.IsAssignableFrom(typeof(GameScreen)))
            {
                Debug.LogError($"Trying to show screen by type but the type given is not an implementation of type: {nameof(GameScreen)}");
                screen = null;
                return false;
            }
            
            return OpenScreens.TryGetValue(screenType, out screen);
        }

        public static void CloseScreen<T>() where T : GameScreen
        {
            if (IsScreenOpen<T>(out var screen))
            {
                screen.Close();
            }
        }

        public static void CloseScreen(Type screenType)
        {
            if (!IsScreenOpen(screenType, out var screen))
            {
                screen.Close();
            }
        }

        public static void CloseAllScreens()
        {
            foreach (var openScreen in OpenScreens)
            {
                openScreen.Value.Close();
            }
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
            canvasController.AddPopup(spawnedPopup);
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

        public static void CloseAllPopups()
        {
            foreach (var popupController in PopupControllers)
            {
                foreach (var popup in popupController.Value.Popups)
                {
                    popup.Close();
                }
            }
        }
        #endregion

        /// <summary>
        /// A wrapper method to prevent multiple sorting updates from firing
        /// </summary>
        private static void UpdateScreenSorting()
        {
            if (_nextSortingUpdate != null)
            {
                return;
            }

            _nextSortingUpdate = InternalUpdateSorting();
        }

        private static async Task InternalUpdateSorting()
        {
            await Task.Yield();

            if (Settings.logSortingUpdates)
            {
                Debug.Log("Updating screen sorting");
            }
            
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

            if (Settings.logSortingUpdates)
            {
                Debug.Log("Sorting updated");
            }
            
            _nextSortingUpdate = null;
        }
    }
}
