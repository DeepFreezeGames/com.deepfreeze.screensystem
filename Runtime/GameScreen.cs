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

        public virtual void Close()
        {
            EventManager.TriggerEvent(new ScreenClosedEvent(this));
            
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
        }
    }
}