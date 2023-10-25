using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFreeze.ScreenSystem
{
    [Serializable]
    public partial class ScreenSettings
    {
        public List<ScreenProfile> profiles = new();

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