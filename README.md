
# EasyMQTTnet
## A .NET API for Message Queuing with MQTT

![Build](https://github.com/guido-altmann/EasyMQTTnet/workflows/Build/badge.svg)

(Experimental State)

**Goals:**

1. To make working with Message Queuing on .NET as easy as possible.

To connect to a MQTT broker...

    var bus = EasyMqttFactory.CreateBus("server=localhost");

To publish a message...

    bus.Publish(message);

To subscribe to a message...

    bus.Subscribe<MyMessage>(msg => Console.WriteLine(msg.Text));


The goal of EasyMQTTnet is to provide a library that makes working with Message Queuing in .NET as simple as possible. In order to do this, it has to take an opinionated view of how you should use EasyMQTTnet with .NET. It trades flexibility for simplicity by enforcing some simple conventions. These include:

* Messages should be represented by .NET types. 
* Messages should be routed by their .NET type.

This means that messages are defined by .NET classes. Each distinct message type that you want to send is represented by a class. The class should be public with a default constructor and public read/write properties. You would not normally implement any functionality in a message, but treat it as a simple data container or Data Transfer Object (DTO). Here is a simple message:

    public class MyMessage
    {
        public string Text { get; set; }
    }

EasyMQTTnet routes messages by their type. When you publish a message, EasyMQTTnet examines its type and gives it a routing key based on the type name, namespace and assembly. On the consuming side, subscribers subscribe to a type. After subscribing to a type, messages of that type get routed to the subscriber.

By default, EasyMQTTnet serializes .NET types as JSON using the Newtonsoft.Json library. This has the advantage that messages are human readable, so you can use tools such as the EasyMQTTnet management application to debug message problems.
