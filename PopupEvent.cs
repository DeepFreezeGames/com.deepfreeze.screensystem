using System;
using DeepFreeze.Events;

namespace DeepFreeze.ScreenSystem
{
    /// <summary>
    /// The base class for <see cref="Popup"/> events
    /// </summary>
    public abstract class PopupEvent : IEvent
    {
        public Popup Popup { get; protected set; }
        public Type PopupType { get; }

        protected PopupEvent(Popup popup)
        {
            Popup = popup;
            PopupType = popup.GetType();
        }
    }

    /// <summary>
    /// Called by the <see cref="ScreenManager"/> when a popup is spawned
    /// </summary>
    public class PopupSpawnedEvent : PopupEvent
    {
        public PopupSpawnedEvent(Popup popup) : base(popup)
        {
        }
    }
    
    /// <summary>
    /// Called by the <see cref="Popup"/> when the popup is activated
    /// </summary>
    public class PopupOpenedEvent : PopupEvent
    {
        public PopupOpenedEvent(Popup popup) :base(popup)
        {
        }
    }

    /// <summary>
    /// Called by the <see cref="Popup"/> when it has been closed
    /// </summary>
    public class PopupClosedEvent : PopupEvent
    {
        public PopupClosedEvent(Popup popup) : base(popup)
        {
        }
    }
}