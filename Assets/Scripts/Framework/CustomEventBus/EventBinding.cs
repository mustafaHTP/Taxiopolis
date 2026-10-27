using System;

namespace Framework.CustomEventBus
{
    public class EventBinding<T> : IEventBinding<T> where T : IEvent
    {
        public Action<T> OnEvent { get; set; }
        public Action OnEventNoArgs { get; set; }

        public EventBinding(Action<T> onEvent)
        {
            OnEvent = onEvent;
        }

        public EventBinding(Action onEventNoArgs)
        {
            OnEventNoArgs = onEventNoArgs;
        }

        public void AddListener(Action<T> onEvent)
        {
            OnEvent += onEvent;
        }

        public void AddListener(Action onEventNoArgs)
        {
            OnEventNoArgs += onEventNoArgs;
        }

        public void RemoveListener(Action<T> onEvent)
        {
            OnEvent -= onEvent;
        }

        public void RemoveListener(Action onEventNoArgs)
        {
            OnEventNoArgs -= onEventNoArgs;
        }
    }
}
