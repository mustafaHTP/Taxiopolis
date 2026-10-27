using System.Collections.Generic;

namespace Framework.CustomEventBus
{
    /// <summary>
    /// Event bus for managing and dispatching events.
    /// </summary>
    public static class EventBus<T> where T : IEvent
    {
        private readonly static HashSet<IEventBinding<T>> _eventBindings = new();

        public static void Register(IEventBinding<T> eventBinding)
        {
            _eventBindings.Add(eventBinding);
        }

        public static void Unregister(IEventBinding<T> eventBinding)
        {
            _eventBindings.Remove(eventBinding);
        }

        public static void Fire(T eventData)
        {
            foreach (var binding in _eventBindings)
            {
                binding.OnEvent?.Invoke(eventData);
                binding.OnEventNoArgs?.Invoke();
            }
        }

        public static void Clear()
        {
            _eventBindings.Clear();
        }
    }
}
