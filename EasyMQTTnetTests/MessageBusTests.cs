using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyMQTTnet;
using System;
using System.Collections.Generic;
using System.Text;
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
            var message = new MyMessage() {Text = "Hallo Message!"};
            target.Publish(message);
        }

        [TestMethod()]
        public void SubScribeTest()
        {
            var target = EasyMqttFactory.CreateBus("localhost");
            target.SubScribe<MyMessage>(msg => Console.WriteLine("Received Message: " + msg.Text));
            var message = new MyMessage() { Text = "Hallo Message!" };
            target.Publish(message);
            Task.Delay(2000).GetAwaiter().GetResult();
        }
    }
}