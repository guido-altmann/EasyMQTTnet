using System;

namespace EasyMQTTnet
{
    public interface IBus
    {
        void Publish<T>(T message);

        void SubScribe<T>(Action<T> onMessage);
    }
}