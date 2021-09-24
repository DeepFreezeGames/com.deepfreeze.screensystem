using System;
using UnityEngine;

namespace ScreenSystem.Runtime
{
    [Serializable]
    public class ScreenSettings
    {
        public string constantScreenPrefix;
        public string constantPopupPrefix;
        
        [Space]
        public string screenPrefixPort = "Screen_Port_";
        public string screenPrefixLand = "Screen_Land_";
        public string popupPrefixPort = "Popup_Port_";
        public string popupPrefixLand = "Popup_Land_";

        [Space]
        public PopupCanvasController popupCanvasControllerPrefab;
    }
}