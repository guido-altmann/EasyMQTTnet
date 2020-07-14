namespace EasyMQTTnet
{
    /// <summary>
    /// 
    /// </summary>
    public static class EasyMqttFactory
    {
        /// <summary>
        /// Creates the messaging bus.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public static IBus CreateBus(string connectionString)
        {
            return new MessageBus();
        }
    }
}
