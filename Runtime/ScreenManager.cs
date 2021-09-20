using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ScreenSystem.Runtime
{
    public static class ScreenManager
    {
        public static bool Initialized { get; private set; }
        public static ScreenSettings Settings { get; private set; }
        
        public static bool IsPortrait { get; private set; }

        private static Dictionary<Type, GameScreen> _openScreens = new();

        #region SCREEN/POPUP IDs
        private static string ScreenPrefix => IsPortrait ? Settings.ScreenPrefixPort : Settings.ScreenPrefixLand;
        private static string PopupPrefix => IsPortrait ? Settings.PopupPrefixPort : Settings.PopupPrefixLand;
        
        private static string GetScreenId(Type type)
        {
            return string.IsNullOrEmpty(Settings.ConstantScreenPrefix)
                ? $"{ScreenPrefix}{type.Name}"
                : $"{Settings.ConstantScreenPrefix}{ScreenPrefix}{type.Name}";
        }

        private static string GetPopupId(Type type)
        {
            return string.IsNullOrEmpty(Settings.ConstantPopupPrefix)
                ? $"{ScreenPrefix}{type.Name}"
                : $"{Settings.ConstantPopupPrefix}{ScreenPrefix}{type.Name}";
        }
        #endregion

        public static void Initialize(ScreenSettings settings)
        {
            Settings = settings;


            Initialized = true;
        }

        public static async Task<T> ShowScreen<T>() where T : GameScreen
        {
            if (IsScreenOpen<T>(out var screen))
            {
                return screen;
            }
            
            //var template =


            return default;
        }

        public static bool IsScreenOpen<T>(out T screen) where T : GameScreen
        {
            if (_openScreens.ContainsKey(typeof(T)))
            {
                screen = (T)_openScreens[typeof(T)];
                return true;
            }

            screen = default;
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
    }
}
