# Code Talker Examples Readme

## You Will Learn

- The fundamental concepts of Code Talker Network communication, including what packets and listeners are
- How to create custom packets to structure data for network transmission
- How to register listeners to handle incoming packets
- How to send packets across the network to other clients

## Making a Packet

**What is a packet?**  
A packet is a structured data message that you want to send across the network. It contains specific information (like a string message, an integer, etc.) and is serialized into a format suitable for transmission.

**How to create a packet?**  

- Define a class that inherits from `PacketBase`.  
- This class contains properties representing the data you want to send.  
- You must add `[JsonProperty]` to your properties to ensure they are properly serialized across the network. To keep it simple, make your properties `public` with get and set accessors, allowing Newtonsoft.Json to handle serialization automatically. Alternatively, you can mark a constructor with `[JsonConstructor]` to control deserialization, or use any serialization method that ensures your class can be converted to and from JSON.
- Make sure to override `PacketSourceGUID` to identify the source of the packet. It is _strongly_ recommended to use the GUID of your plugin for this field.

**Example:**  
In the provided code, `ExamplePacket` is a simple packet with a `Payload` string. When you instantiate it, you pass the payload message, and it becomes ready to send.

---

## Registering a Listener

**What is a listener?**  
A listener is a method that waits for incoming packets of a specific type. When such a packet arrives, the listener is triggered, allowing you to process the data.

**How to set up a listener?**  

- Use `CodeTalkerNetwork.RegisterListener<T>()`, where `T` is your packet type.  
- Provide a callback method that takes a `PacketHeader` and your packet type as parameters.

**Example:**  
In the plugin's `Awake()` method, the code registers a listener for `ExamplePacket`:

```cs
CodeTalkerNetwork.RegisterListener<ExamplePacket>(ReceiveExamplePacket);
```

This means whenever an `ExamplePacket` arrives, the `ReceiveExamplePacket` method is called.

---

## Sending a Packet

**How to send a packet?**  

- Instantiate your packet class with the data you want to send.  
- Use `CodeTalkerNetwork.SendNetworkPacket()` to transmit it.

**Example:**  
The `SendPacketTest` method creates a new `ExamplePacket` with a payload and sends it:

```cs
CodeTalkerNetwork.SendNetworkPacket(new ExamplePacket(payload));
```

---

## Summary in Simple Terms

- **Define what data you want to send** by creating a custom packet class.
- **Register a listener** that will react when your packets are received.
- **Send packets** by creating an instance of your packet class and calling the send method.
