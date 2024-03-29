﻿using System;
using DeepFreeze.Events;

namespace DeepFreeze.ScreenSystem
{
    public abstract class ScreenEvent : IEvent
    {
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
        }
    }

    public class ScreenClosedEvent : ScreenEvent
    {
        public ScreenClosedEvent(GameScreen screen) : base(screen)
        {
        }
    }
}