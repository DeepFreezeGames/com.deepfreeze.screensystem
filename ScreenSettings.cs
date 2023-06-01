using System;
using UnityEngine;

namespace DeepFreeze.ScreenSystem
{
    [Serializable]
    public partial class ScreenSettings
    {
        public string constantScreenPrefix;
        public string constantPopupPrefix;
        
        [Space]
        public string screenPrefixPort = "Screen_Port_";
        public string screenPrefixLand = "Screen_Land_";
        public string screenSuffix = "";
        public string popupPrefixPort = "Popup_Port_";
        public string popupPrefixLand = "Popup_Land_";
        public string popupSuffix = "";

        [Space]
        public PopupCanvasController popupCanvasControllerPrefab;

        [Space] 
        public bool logScreenSpawned;
        public bool logScreenShown;
        public bool logScreenClosed;
        public bool logPopupSpawned;
        public bool logPopupShown;
        public bool logPopupClosed;
        public bool logSortingUpdates;
    }
}