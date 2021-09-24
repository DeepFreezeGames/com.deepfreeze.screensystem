using System;
using Events.Runtime;

namespace ScreenSystem.Runtime
{
    public abstract class ScreenEvent : IEvent
    {
        public Type[] DispatchAs { get; protected set; }
        public GameScreen Screen { get; }
        public Type ScreenType { get; }

        protected ScreenEvent(GameScreen screen)
        {
            Screen = screen;
            ScreenType = screen.GetType();
        }
    }

    public class ScreenOpenedEvent : ScreenEvent
    {
        public ScreenOpenedEvent(GameScreen screen) : base(screen)
        {
            DispatchAs = new[] { typeof(ScreenOpenedEvent) };
        }
    }

    public class ScreenClosedEvent : ScreenEvent
    {
        public ScreenClosedEvent(GameScreen screen) : base(screen)
        {
            DispatchAs = new[] { typeof(ScreenClosedEvent) };
        }
    }
}