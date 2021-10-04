using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Events.Runtime;
using UnityEngine;

namespace ScreenSystem.Runtime
{
    public static class ScreenManager
    {
        public static bool Initialized { get; private set; }
        public static ScreenSettings Settings { get; private set; }
        
        public static bool IsPortrait { get; private set; }

        private static readonly Dictionary<Type, GameScreen> OpenScreens = new();
        private static readonly Dictionary<PopupPriority, PopupCanvasController> PopupControllers = new();

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
                var newController = GameObject.Instantiate(Settings.popupCanvasControllerPrefab);
                newController.Initialize(priorityValue);
                PopupControllers.Add(priorityValue, newController);
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

            var template = await _screenProvider.GetScreen<T>(GetScreenId(typeof(T)));
            screen = GameObject.Instantiate(template);
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
            var template = await _screenProvider.GetPopup<T>(GetPopupId(typeof(T)));
            var spawnedPopup = GameObject.Instantiate(template, PopupControllers[template.priority].popupContainer);
            spawnedPopup.PopupCanvasController = PopupControllers[template.priority];
            EventManager.TriggerEvent(new PopupSpawnedEvent(spawnedPopup, PopupControllers[template.priority]));
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

        private static void UpdateScreenSorting()
        {
            var blockHighPriorityPopups = false;
            var blockMediumPriorityPopups = false;
            foreach (var (_, gameScreen) in OpenScreens)
            {
                if (gameScreen.blockHighPriorityPopups)
                {
                    blockHighPriorityPopups = true;
                }

                if (gameScreen.blockMediumPriorityPopups)
                {
                    blockMediumPriorityPopups = true;
                }
            }
            
            PopupControllers[PopupPriority.High].gameObject.SetActive(!blockHighPriorityPopups);
            PopupControllers[PopupPriority.Medium].gameObject.SetActive(!blockMediumPriorityPopups);
        }
    }
}
