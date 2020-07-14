using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;

namespace EasyMQTTnet
{
    public class MessageBus : IBus, IDisposable
    {
        private IMqttClient mqttClient;
        private readonly string server;
        private readonly int port;
        private Dictionary<string, Action<object>> registeredMessageHandlers = new Dictionary<string, Action<object>>();

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
                var obj = JsonConvert.DeserializeObject(payload);

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

                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await mqttClient.ConnectAsync(options, CancellationToken.None); // Since 3.0.5 with CancellationToken
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            });

            try
            {
                await mqttClient.ConnectAsync(options, CancellationToken.None); // Since 3.0.5 with CancellationToken
            }
            catch
            {
                Console.WriteLine("### CONNECTING FAILED ###");
            }
        }

        public void Publish<T>(T message)
        {
            if (!mqttClient.IsConnected) return;

            var type = message.GetType();
            var topic = $"{type.Assembly.ManifestModule.Name}/{type.Namespace}/{type.Name}";
            var payload = JsonConvert.SerializeObject(message);

             mqttClient.PublishAsync(topic, payload).GetAwaiter().GetResult();
        }

        public void SubScribe<T>(Action<T> onMessage)
        {
            if (!mqttClient.IsConnected) return;

            var type = typeof(T);

            var topic = $"{type.Assembly.ManifestModule.Name}/{type.Namespace}/{type.Name}";
            mqttClient.SubscribeAsync(new MqttTopicFilter() { Topic = topic }).GetAwaiter().GetResult();

            registeredMessageHandlers.Add(topic, o => onMessage((T)o));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                mqttClient?.Dispose();
        }
       
    }
}
