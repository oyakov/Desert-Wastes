using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Wastelands.Core.Services
{
    public interface IEventBus
    {
        IDisposable Subscribe<TEvent>(Action<TEvent> handler);
        void Publish<TEvent>(TEvent @event);
    }

    /// <summary>
    /// Lightweight synchronous event bus. Ensures deterministic ordering
    /// by invoking subscribers in subscription order and processing
    /// nested publishes using a queue.
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private readonly ConcurrentDictionary<Type, SubscriptionList> _subscriptions = new();

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var list = _subscriptions.GetOrAdd(typeof(TEvent), _ => new SubscriptionList());
            return list.Add(handler);
        }

        public void Publish<TEvent>(TEvent @event)
        {
            if (!_subscriptions.TryGetValue(typeof(TEvent), out var list))
            {
                return;
            }

            list.Publish(@event);
        }

        private sealed class SubscriptionList
        {
            private readonly object _gate = new();
            private readonly List<Delegate> _delegates = new();
            private readonly Queue<object> _queue = new();
            private bool _isPublishing;

            public IDisposable Add(Delegate handler)
            {
                lock (_gate)
                {
                    _delegates.Add(handler);
                }

                return new Subscription(this, handler);
            }

            public void Publish(object payload)
            {
                lock (_gate)
                {
                    _queue.Enqueue(payload);
                    if (_isPublishing)
                    {
                        return;
                    }

                    _isPublishing = true;
                }

                try
                {
                    while (true)
                    {
                        object? next;
                        lock (_gate)
                        {
                            if (_queue.Count == 0)
                            {
                                _isPublishing = false;
                                return;
                            }

                            next = _queue.Dequeue();
                        }

                        Delegate[] handlers;
                        lock (_gate)
                        {
                            handlers = _delegates.ToArray();
                        }

                        foreach (var handler in handlers)
                        {
                            handler.DynamicInvoke(next);
                        }
                    }
                }
                finally
                {
                    lock (_gate)
                    {
                        _isPublishing = false;
                    }
                }
            }

            public void Remove(Delegate handler)
            {
                lock (_gate)
                {
                    _delegates.Remove(handler);
                }
            }

            private sealed class Subscription : IDisposable
            {
                private readonly SubscriptionList _owner;
                private readonly Delegate _handler;
                private bool _disposed;

                public Subscription(SubscriptionList owner, Delegate handler)
                {
                    _owner = owner;
                    _handler = handler;
                }

                public void Dispose()
                {
                    if (_disposed)
                    {
                        return;
                    }

                    _disposed = true;
                    _owner.Remove(_handler);
                }
            }
        }
    }
}
