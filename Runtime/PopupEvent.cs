using System;
using Events.Runtime;

namespace ScreenSystem.Runtime
{
    /// <summary>
    /// The base class for <see cref="Popup"/> events
    /// </summary>
    public abstract class PopupEvent : IEvent
    {
        public Type DispatchAs { get; protected set; }
        public Popup Popup { get; protected set; }
        public Type PopupType { get; }
        public PopupCanvasController PopupCanvasController { get; protected set; }

        protected PopupEvent(Popup popup, PopupCanvasController popupCanvasController)
        {
            DispatchAs = typeof(PopupEvent);
            Popup = popup;
            PopupCanvasController = popupCanvasController;
            PopupType = popup.GetType();
        }
    }

    /// <summary>
    /// Called by the <see cref="ScreenManager"/> when a popup is spawned
    /// </summary>
    public class PopupSpawnedEvent : PopupEvent
    {
        public PopupSpawnedEvent(Popup popup, PopupCanvasController popupCanvasController) : base(popup, popupCanvasController)
        {
            DispatchAs = typeof(PopupSpawnedEvent);
        }
    }
    
    /// <summary>
    /// Called by the <see cref="Popup"/> when the popup is activated
    /// </summary>
    public class PopupOpenedEvent : IEvent
    {
        public Type DispatchAs { get; protected set; }
        public Popup Popup { get; protected set; }

        public PopupOpenedEvent(Popup popup)
        {
            DispatchAs = typeof(PopupOpenedEvent);
        }
    }

    /// <summary>
    /// Called by the <see cref="Popup"/> when it has been closed
    /// </summary>
    public class PopupClosedEvent : PopupEvent
    {
        public PopupClosedEvent(Popup popup, PopupCanvasController popupCanvasController) : base(popup, popupCanvasController)
        {
            DispatchAs = typeof(PopupClosedEvent);
        }
    }
}