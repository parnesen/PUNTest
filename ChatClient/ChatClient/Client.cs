using System;
using System.Collections.Generic;
using System.Text;

using ExitGames.Client.Photon;

public class ChatClient : IPhotonPeerListener
{
    private bool connected;

    public static void Main()
    {
        var client = new ChatClient();
        var peer = new PhotonPeer(client, ConnectionProtocol.Tcp);

        // connect
        client.connected = false;
        peer.Connect("127.0.0.1:4530", "ChatServer");
        while (!client.connected)
        {
            peer.Service();
        }

        var buffer = new StringBuilder();
        while (true)
        {
            peer.Service();

            // read input
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key != ConsoleKey.Enter)
                {
                    // store input
                    buffer.Append(key.KeyChar);
                }
                else
                {
                    // send to server
                    var parameters = new Dictionary<byte, object> { { 1, buffer.ToString() } };
                    peer.OpCustom(1, parameters, true);
                    buffer.Length = 0;
                }
            }
        }
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        Console.WriteLine(level + ": " + message);
    }

    public void OnEvent(EventData eventData)
    {
        Console.WriteLine("Event: " + eventData.Code);
        if (eventData.Code == 1)
        {
            Console.WriteLine("Chat: " + eventData.Parameters[1]);
        }
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        Console.WriteLine("Response: " + operationResponse.OperationCode);
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        if (statusCode == StatusCode.Connect)
        {
            this.connected = true;
        }
        else
        {
            Console.WriteLine("Status: " + statusCode);
        }
    }
}