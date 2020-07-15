using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EasyMQTTnet.Tests
{
    [TestClass()]
    public class MessageBusTests
    {
        class MyMessage
        {
            public string Text { get; set; }
        }

        class MyNextMessage
        {
            public string Text { get; set; }
        }

        private const string CONNECTIONSTRING = "server=test.mosquitto.org, port=1883";

        [TestMethod()]
        public void MessageBusTest()
        {
            var target = EasyMqttFactory.CreateBus(CONNECTIONSTRING);
            Assert.IsNotNull(target);
            Assert.IsTrue(target.IsConnected, "Connection to MQTT broker fails.");
        }

        [TestMethod()]
        public void PublishTest()
        {
            var target = EasyMqttFactory.CreateBus(CONNECTIONSTRING);
            Assert.IsTrue(target.IsConnected, "Connection to MQTT broker fails.");

            var message = new MyMessage() {Text = "Hello Message!"};
            var actual = target.Publish(message);
            Assert.IsTrue(actual, "Publishing message fails.");
        }

        [TestMethod()]
        public void SubscribeTest()
        {
            var firstMessageReceived = false;
            var secondMessageReceived = false;

            var target = EasyMqttFactory.CreateBus(CONNECTIONSTRING);
            Assert.IsTrue(target.IsConnected, "Connection to MQTT broker fails.");

            target.Subscribe<MyMessage>(msg =>
            {
                firstMessageReceived = true;
                Console.WriteLine("Received Message: " + msg.Text);
            });
            target.Subscribe<MyNextMessage>(msg =>
            {
                secondMessageReceived = true;
                Console.WriteLine("Received Message: " + msg.Text);
            });

            var message = new MyMessage() { Text = "Hello first Message!" };
            var message2 = new MyNextMessage() { Text = "Hello second Message!" };
            target.Publish(message);
            target.Publish(message2);

            Task.Delay(500).GetAwaiter().GetResult();

            Assert.IsTrue(firstMessageReceived, "First message not reveived.");
            Assert.IsTrue(secondMessageReceived, "Second message not reveived.");
        }
    }
}