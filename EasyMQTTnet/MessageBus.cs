using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;
using Newtonsoft.Json;

namespace EasyMQTTnet
{
    /// <summary>
    /// Provides the (MQTT) message bus.
    /// </summary>
    /// <seealso cref="EasyMQTTnet.IBus" />
    /// <seealso cref="System.IDisposable" />
    public class MessageBus : IBus, IDisposable
    {
        private IMqttClient mqttClient;
        private readonly string server;
        private readonly int port;
        private readonly Dictionary<string, Action<object>> registeredMessageHandlers = new Dictionary<string, Action<object>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBus"/> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="port">The port.</param>
        public MessageBus(string server = "localhost", int port = 1883)
        {
            this.server = server;
            this.port = port;
            InitMqtt().GetAwaiter().GetResult();
        }

        private async Task InitMqtt()
        {
            var factory = new MqttFactory();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(server, port) // Port is optional
                .Build();
            mqttClient = factory.CreateMqttClient();

#pragma warning disable CA1303 // Do not pass literals as localized parameters

            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
#if DEBUG
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();
#endif
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                var typeInfo = e.ApplicationMessage.Topic.Split('/');
                var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
                var filePath = fi.DirectoryName ?? "";
                var assembly = Assembly.LoadFrom(Path.Combine(filePath, typeInfo[0]));
                var type = assembly.GetType($"{typeInfo[1]}+{typeInfo[2]}");
                Debug.WriteLine($"Deserialize type: {type}");
                var obj = JsonConvert.DeserializeObject(payload, type);

                if (registeredMessageHandlers.ContainsKey(e.ApplicationMessage.Topic))
                    registeredMessageHandlers[e.ApplicationMessage.Topic].Invoke(obj);

            });

            mqttClient.UseConnectedHandler(e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");
            });

            mqttClient.UseDisconnectedHandler(async e =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");

                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

                try
                {
                    await mqttClient.ConnectAsync(options, CancellationToken.None).ConfigureAwait(false); // Since 3.0.5 with CancellationToken
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            });

            try
            {
                await mqttClient.ConnectAsync(options, CancellationToken.None).ConfigureAwait(false); // Since 3.0.5 with CancellationToken
            }
            catch
            {
                Console.WriteLine("### CONNECTING FAILED ###");
            }
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        }

        /// <inheritdoc />
        public bool IsConnected => mqttClient.IsConnected;

        /// <inheritdoc />
        public bool Publish<T>(T message)
        {
            if (!mqttClient.IsConnected) return false;

            var type = message.GetType();
            var topic = GetRoutingKey(type);
            var payload = JsonConvert.SerializeObject(message);

            var result = mqttClient.PublishAsync(topic, payload).GetAwaiter().GetResult();

            return result.ReasonCode == MqttClientPublishReasonCode.Success;
        }

        /// <inheritdoc />
        public void Subscribe<T>(Action<T> onMessage)
        {
            if (!mqttClient.IsConnected) return;

            var type = typeof(T);
            var topic = GetRoutingKey(type);
            var result = mqttClient.SubscribeAsync(new MqttTopicFilter() { Topic = topic }).GetAwaiter().GetResult();

            if (result.Items.Count > 0)
                registeredMessageHandlers.Add(topic, o => onMessage((T)o));
        }

        private string GetRoutingKey(Type type)
        {
#if DEBUG
            Console.WriteLine($"Declaring type full name: {type.DeclaringType?.FullName}");
            Console.WriteLine($"Type full name: {type.FullName}");
#endif
            return $"{type.Assembly.ManifestModule.Name}/{type.DeclaringType?.FullName}/{type.Name}";
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                mqttClient?.Dispose();
        }
       
    }
}
