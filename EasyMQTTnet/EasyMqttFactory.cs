using System;

namespace EasyMQTTnet
{
    public static class EasyMqttFactory
    {
        public static IBus CreateBus(string connectionString)
        {
            return new MessageBus();
        }
    }
}
