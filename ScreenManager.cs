#define DEEPFREEZE_SCREENSYSTEM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeepFreeze.Events;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace DeepFreeze.ScreenSystem
{
    public static class ScreenManager
    {
        public static bool Initialized { get; private set; }
        public static ScreenSettings Settings { get; private set; }
        public static ScreenProfile Profile { get; private set; }
        
        public static bool IsPortrait { get; private set; }

        private static readonly Dictionary<Type, GameScreen> OpenScreens = new();
        private static readonly Dictionary<PopupPriority, PopupCanvasController> PopupControllers = new();
        
        public static bool BlockHighPriorityPopups { get; private set; }
        public static bool BlockMediumPriorityPopups { get; private set; }

        private static IScreenProvider _screenProvider;
        private static Task _nextSortingUpdate;

        #region SCREEN/POPUP IDs
        private static string ScreenPrefix => IsPortrait ? Profile.screenPrefixPort : Profile.screenPrefixLand;
        private static string PopupPrefix => IsPortrait ? Profile.popupPrefixPort : Profile.popupPrefixLand;
        
        private static string GetScreenId(Type type)
        {
            return string.IsNullOrEmpty(Profile.constantScreenPrefix)
                ? $"{ScreenPrefix}{type.Name}{Profile.screenSuffix}"
                : $"{Profile.constantScreenPrefix}{ScreenPrefix}{type.Name}{Profile.screenSuffix}";
        }

        private static string GetScreenId(string screenName)
        {
            return string.IsNullOrEmpty(Profile.constantScreenPrefix)
                ? $"{ScreenPrefix}{screenName}{Profile.screenSuffix}"
                : $"{Profile.constantScreenPrefix}{ScreenPrefix}{screenName}{Profile.screenSuffix}";
        }

        private static string GetPopupId(Type type)
        {
            return string.IsNullOrEmpty(Profile.constantPopupPrefix)
                ? $"{PopupPrefix}{type.Name}{Profile.popupSuffix}"
                : $"{Profile.constantPopupPrefix}{PopupPrefix}{type.Name}{Profile.popupSuffix}";
        }

        private static string GetPopupId(string popupName)
        {
            return string.IsNullOrEmpty(Profile.constantPopupPrefix)
                ? $"{PopupPrefix}{popupName}{Profile.popupSuffix}"
                : $"{Profile.constantPopupPrefix}{PopupPrefix}{popupName}{Profile.popupSuffix}";
        }
        #endregion

        public static async void Initialize(ScreenSettings settings, IScreenProvider screenProvider)
        {
            Initialize(settings, settings.profiles.FirstOrDefault(), screenProvider);
        }

        public static async void Initialize(ScreenSettings settings, ScreenProfile profile, IScreenProvider screenProvider)
        {
            if (Initialized)
            {
                Debug.LogError($"Trying to initialize {nameof(ScreenManager)} but it is already initialized");
                return;
            }
            
            Settings = settings;
            Profile = profile ?? Settings.profiles.FirstOrDefault();
            if (Profile == null)
            {
                Debug.LogError("There are no screen profiles in the given settings. Falling back to default profile but your UI may not work as expected");
                Profile = new ScreenProfile();
            }
            
            IsPortrait = Screen.height > Screen.width;
            
            Debug.Log($"Starting screen provider: {screenProvider.GetType().Name}");
            _screenProvider = screenProvider;
            await UniTask.WaitUntil(() => _screenProvider.Initialized);

            await SpawnPopupContainers();
            BindEvents();

            Initialized = true;
        }

        public static void SetProfile(int profileIndex)
        {
            if (Settings == null)
            {
                Debug.LogError("Trying to set UI profile but the ScreenManager is not yet initialized");
                return;
            }

            var profile = Settings.profiles.ElementAt(profileIndex);
            if (profile == null)
            {
                Debug.LogError($"Trying to set UI profile to the profile at index {profileIndex.ToString()} but there is no profile at that index in the given settings");
                if (Profile == null)
                {
                    Debug.Log("Falling back to default UI profile");
                    profile = Settings.profiles.FirstOrDefault();
                }
            }
            
            SetProfile(profile);
        }

        public static void SetProfile(string profileId)
        {
            if (Settings == null)
            {
                Debug.LogError("Trying to set UI profile but the ScreenManager is not yet initialized");
                return;
            }

            var profile = Settings.profiles.FirstOrDefault(p => p.id.Equals(profileId, StringComparison.InvariantCultureIgnoreCase));
            if (profile == null)
            {
                Debug.LogError($"Trying to set the UI profile to {profileId} but no profile with that ID could be found in the current settings");
                if (Profile == null)
                {
                    Debug.Log("Falling back to default UI profile");
                    profile = Settings.profiles.FirstOrDefault();
                }
            }
            
            SetProfile(profile);
        }

        public static void SetProfile(ScreenProfile profile)
        {
            if (Profile == profile)
            {
                Debug.LogWarning("Trying to set profile to existing profile");
                return;
            }
            
            Debug.Log($"Setting UI profile to {profile.id}");
            Profile = profile;
        }

        private static async Task SpawnPopupContainers()
        {
            PopupControllers.Clear();

            foreach (var priority in Enum.GetValues(typeof(PopupPriority)))
            {
                var priorityValue = (PopupPriority)priority;
                var newControllerObj = await Addressables.InstantiateAsync(Profile.popupCanvasControllerPrefab);
                var newController = newControllerObj.GetComponent<PopupCanvasController>();
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
            if (!typeof(GameScreen).IsAssignableFrom(screenType))
            {
                Debug.LogError($"Trying to show screen by type {screenType} but the type given is not an implementation of type: {nameof(GameScreen)}");
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
            if (!typeof(GameScreen).IsAssignableFrom(screenType))
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

        public static async Task CloseAllScreens()
        {
            foreach (var openScreen in OpenScreens)
            {
                Object.Destroy(openScreen.Value.gameObject);
            }
            
            OpenScreens.Clear();
            await InternalUpdateSorting();
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

        public static async Task<Popup> ShowPopup(Type popupType)
        {
            if (!typeof(Popup).IsAssignableFrom(popupType))
            {
                Debug.LogError($"Trying to show popup of type {popupType} but the type is not an implementation of Popup");
                return null;
            }
            
            var spawnedPopup = await _screenProvider.GetPopup<Popup>(GetPopupId(popupType));
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

        public static bool IsPopupShown(Type popupType, out Popup popup)
        {
            if (!typeof(Popup).IsAssignableFrom(popupType))
            {
                Debug.LogError($"Trying to show popup of type {popupType} but the type is not an implementation of Popup");
                popup = null;
                return false;
            }
            
            foreach (var popupController in PopupControllers)
            {
                foreach (var canvasPopup in popupController.Value.Popups)
                {
                    if (canvasPopup.GetType() == popupType)
                    {
                        popup = canvasPopup;
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

        public static async Task CloseAllPopups()
        {
            foreach (var popupController in PopupControllers)
            {
                foreach (var popup in popupController.Value.Popups)
                {
                    Object.Destroy(popup.gameObject);
                }
                
                popupController.Value.Popups.Clear();
            }
            
            await InternalUpdateSorting();
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
