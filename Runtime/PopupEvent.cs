using System;
using Events.Runtime;

namespace ScreenSystem.Runtime
{
    public abstract class PopupEvent : IEvent
    {
        public Type[] DispatchAs { get; protected set; }
        public Popup Popup { get; protected set; }

        protected PopupEvent(Popup popup)
        {
            DispatchAs = new[] { typeof(PopupEvent) };
            Popup = popup;
        }
    }

    public class PopupOpenedEvent : PopupEvent
    {
        public PopupOpenedEvent(Popup popup) : base(popup)
        {
            DispatchAs = new[] { typeof(PopupOpenedEvent) };
        }
    }

    public class PopupClosedEvent : PopupEvent
    {
        public PopupClosedEvent(Popup popup) : base(popup)
        {
            DispatchAs = new[] { typeof(PopupClosedEvent) };
        }
    }
}