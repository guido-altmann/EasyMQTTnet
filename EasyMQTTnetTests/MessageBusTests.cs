using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;

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
        //private const string CONNECTIONSTRING = "server=localhost, port=1883";

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
            var receivedEvents = new List<string>();

            var target = EasyMqttFactory.CreateBus(CONNECTIONSTRING);
            Assert.IsTrue(target.IsConnected, "Connection to MQTT broker fails.");

            var statsUpdatedEvent = new ManualResetEvent(false);

            target.Subscribe<MyMessage>(msg =>
            {
                receivedEvents.Add(msg.Text);
                statsUpdatedEvent.Set();
                Console.WriteLine("Received Message: " + msg.Text);
            });
            target.Subscribe<MyNextMessage>(msg =>
            {
                receivedEvents.Add(msg.Text);
                statsUpdatedEvent.Set();
                Console.WriteLine("Received Message: " + msg.Text);
            });

            var message = new MyMessage() { Text = "Hello first Message!" };
            var message2 = new MyNextMessage() { Text = "Hello second Message!" };
            target.Publish(message);
            statsUpdatedEvent.WaitOne(5000, false);
            statsUpdatedEvent.Reset();

            target.Publish(message2);
            statsUpdatedEvent.WaitOne(5000, false);

            Assert.AreEqual(2, receivedEvents.Count);
            Assert.AreEqual("Hello first Message!", receivedEvents[0]);
            Assert.AreEqual("Hello second Message!", receivedEvents[1]);
        }
    }
}