using System;
using Events.Runtime;

namespace ScreenSystem.Runtime
{
    /// <summary>
    /// The base class for <see cref="Popup"/> events
    /// </summary>
    public abstract class PopupEvent : IEvent
    {
        public Type[] DispatchAs { get; protected set; }
        public Popup Popup { get; protected set; }
        public PopupCanvasController PopupCanvasController { get; protected set; }
        public Type PopupType { get; }

        protected PopupEvent(Popup popup)
        {
            DispatchAs = new[] { typeof(PopupEvent) };
            Popup = popup;
            PopupType = popup.GetType();
        }
    }

    /// <summary>
    /// Called by the <see cref="ScreenManager"/> when a popup is spawned
    /// </summary>
    public class PopupSpawnedEvent : PopupEvent
    {
        public PopupCanvasController PopupCanvasController { get; protected set; }
        
        public PopupSpawnedEvent(Popup popup, PopupCanvasController popupCanvasController) : base(popup)
        {
            DispatchAs = new[] { typeof(PopupSpawnedEvent) };
        }
    }
    
    /// <summary>
    /// Called by the <see cref="Popup"/> when the popup is activated
    /// </summary>
    public class PopupOpenedEvent : IEvent
    {
        public Type[] DispatchAs { get; protected set; }
        public Popup Popup { get; protected set; }

        public PopupOpenedEvent(Popup popup)
        {
            DispatchAs = new[] { typeof(PopupOpenedEvent) };
        }
    }

    /// <summary>
    /// Called by the <see cref="Popup"/> when it has been closed
    /// </summary>
    public class PopupClosedEvent : PopupEvent
    {
        public PopupClosedEvent(Popup popup) : base(popup)
        {
            DispatchAs = new[] { typeof(PopupClosedEvent) };
        }
    }
}