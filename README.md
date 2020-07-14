#### A .NET API for Message Queueing with MQTT


Goals:

1. To make working with RabbitMQ on .NET as easy as possible.

To connect to a RabbitMQ broker...

    var bus = RabbitHutch.CreateBus("host=localhost");

To publish a message...

    bus.Publish(message);

To subscribe to a message...

    bus.Subscribe<MyMessage>("my_subscription_id", msg => Console.WriteLine(msg.Text));

