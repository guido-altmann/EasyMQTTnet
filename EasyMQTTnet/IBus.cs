using System;

namespace EasyMQTTnet
{
    /// <summary>
    /// Provides a simple Publish/Subscribe API for a message bus.
    /// </summary>
    public interface IBus
    {
        /// <summary>
        /// Publishes the specified message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        bool Publish<T>(T message);

        /// <summary>
        /// Subscribes to a stream of messages that match a .NET type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onMessage">The on message.</param>
        void Subscribe<T>(Action<T> onMessage);
    }
}