using System;
using Events.Runtime;
using UnityEngine;
using UnityEngine.Events;

namespace ScreenSystem.Runtime
{
    public abstract class GameScreen : MonoBehaviour
    {
        [Serializable]
        public enum CloseMethods
        {
            Hide = 0,
            Destroy = 1
        }
        
        public bool hideable = true;
        public bool hidesOtherScreens = true;
        public CloseMethods closeMethod;
        public bool blockHighPriorityPopups;
        public bool blockMediumPriorityPopups;

        public void OnEnable()
        {
            if (ScreenManager.Settings.logPopupShown)
            {
                Debug.Log($"Screen shown - {GetType().Name}");
            }
        }

        public virtual void Close()
        {
            switch (closeMethod)
            {
                case CloseMethods.Hide:
                    gameObject.SetActive(false);
                    break;
                case CloseMethods.Destroy:
                    Destroy(gameObject);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (ScreenManager.Settings.logScreenClosed)
            {
                Debug.Log($"Screen closed - {GetType().Name}");
            }

            EventManager.TriggerEvent(new ScreenClosedEvent(this));
        }
    }
}