#Folders

**PUNTest** is the Unity 5.1 project into which PUN has been installed. See ClientConnection.cs under Assets. It's the standard chat app from the Photon Tutorials.

**ChatClient** is a console client that connects to the server using tcp. There's a working .exe in the build/debug directory. 

**ChatServer** is the visual studio solution that builds the chat server. The compliled code has already been moved into the ChatServer directory of the included photon server.

**Photon-OnPremise-Server-SDK_v3-4-30-9719** is the photon SDK, configured to host the ChatServer via TCP and WebSockets

#Instructions
1. Launch the included photon server
2. Launch one of the console chat clients
1. Load MainScene from the root of 'Assets', launch the scene and watch the Console output. 
You'll see that the client does not connect. If you enable the TCP code instead, it connects, and you can see
chat messages arriving from the standalone clients when you type into a standalone client. 

```
        //peer = new PhotonPeer(this, ConnectionProtocol.Tcp);
        //peer.Connect("127.0.0.1:4530", "ChatServer");

        peer = new PhotonPeer(this, ConnectionProtocol.WebSocket);
        peer.Connect("46.52.15.26:9090", "ChatServer");
```
