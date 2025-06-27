# CodeTalker

**CodeTalker** is a lightweight C# networking library designed to simplify sending and receiving custom packets in Atlyss. It allows you to create your own packet types and register callbacks that are automatically invoked when matching packets are received.

**Special Thanks**: ButteredLilly for initial inspiration for the delivery method, thanks!

## Features

- Define custom packet classes by inheriting from `PacketBase`.
- Easily register listeners for specific packet types.
- Automatically deserialize incoming messages and dispatch them to the appropriate listener.
- Packets are serialized into Steam lobby chat messages, avoiding the need for new RPCs that could disrupt multiplayer or clutter the in-game chat.

## How it works

- Create your packet classes by inheriting from `PacketBase`.
- Register listeners for your packet types using `CodeTalkerNetwork.RegisterListener<T>()`.
- Send packets using `CodeTalkerNetwork.SendNetworkPacket()`.
- Incoming messages are deserialized and routed automatically to the registered listeners.
- `PacketHeader` information is included with each payload, providing useful data such as the sender's identity and whether the sender is the host.

---

For a full example, see the [example folder](https://github.com/RobynLlama/CodeTalker/tree/main/src/CodeTalker/Examples) in the repo.

---
