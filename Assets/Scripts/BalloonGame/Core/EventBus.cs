using System;
using System.Collections.Generic;

/// <summary>
/// Lightweight generic event bus using struct payloads.
/// Subscribe in <c>OnEnable</c> and unsubscribe in <c>OnDisable</c>.
/// </summary>
public static class EventBus
{
    private static class Binding<T> where T : struct
    {
        private static event Action<T> Event;
        private static readonly HashSet<Action<T>> Subscribers = new();

        public static void Subscribe(Action<T> handler)
        {
            if (handler == null || !Subscribers.Add(handler))
            {
                return;
            }

            Event += handler;
        }

        public static void Unsubscribe(Action<T> handler)
        {
            if (handler == null || !Subscribers.Remove(handler))
            {
                return;
            }

            Event -= handler;
        }

        public static void Publish(T message)
        {
            Event?.Invoke(message);
        }
    }

    public static void Subscribe<T>(Action<T> handler) where T : struct
    {
        Binding<T>.Subscribe(handler);
    }

    public static void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        Binding<T>.Unsubscribe(handler);
    }

    public static void Publish<T>(T message) where T : struct
    {
        Binding<T>.Publish(message);
    }
}
