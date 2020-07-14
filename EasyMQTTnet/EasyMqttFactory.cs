using System.Text.RegularExpressions;

namespace EasyMQTTnet
{
    /// <summary>
    /// Factory to create the core APIs.
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
            var server = "localhost";
            var port = 1883;

            var pattern = @"(?<key>[^=;,]+)=(?<val>[^;,]+(,\d+)?)";
            var options = RegexOptions.Multiline;

            foreach (Match m in Regex.Matches(connectionString, pattern, options))
            {
                switch (m.Groups["key"].Value)
                {
                    case "server":
                        server = m.Groups["val"].Value;
                        break;
                    case "port":
                        _ = int.TryParse(m.Groups["val"].Value, out port);
                        break;
                }
            }

            return new MessageBus(server, port);
        }
    }
}
