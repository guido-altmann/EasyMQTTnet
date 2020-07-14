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

        [TestMethod()]
        public void MessageBusTest()
        {
            var target = EasyMqttFactory.CreateBus("server=localhost");
            Assert.IsNotNull(target);
        }

        [TestMethod()]
        public void PublishTest()
        {
            var target = EasyMqttFactory.CreateBus("localhost");
            var message = new MyMessage() {Text = "Hello Message!"};
            target.Publish(message);
        }

        [TestMethod()]
        public void SubscribeTest()
        {
            var messageReceived = false;
            var target = EasyMqttFactory.CreateBus("localhost");
            target.Subscribe<MyMessage>(msg =>
            {
                messageReceived = true;
                Console.WriteLine("Received Message: " + msg.Text);
            });
            var message = new MyMessage() { Text = "Hello Message!" };
            target.Publish(message);
            Task.Delay(200).GetAwaiter().GetResult();
            Assert.IsTrue(messageReceived);
        }
    }
}