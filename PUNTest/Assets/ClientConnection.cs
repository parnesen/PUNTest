using System;
using UnityEngine;
using System.Collections;
using System.Security.Policy;
using ExitGames.Client.Photon;


public class ClientConnection : MonoBehaviour
{

    private readonly MyChatClient client = new MyChatClient();

	// Use this for initialization
	void Start () {
        Debug.Log("ClientConnection starting");
        Application.runInBackground = true;
        client.Connect();
	}
	
	// Update is called once per frame
	void Update () {
        if (client.peer != null)
        {
            client.peer.Service();  // make sure to call this regularly! it limits effort internally, so calling often is ok!
        }
	}
}


class MyChatClient : IPhotonPeerListener
{

    public bool connected;

    public PhotonPeer peer;

    public void Connect()
    {
        connected = false;
        //peer = new PhotonPeer(this, ConnectionProtocol.Tcp);
        //peer.Connect("127.0.0.1:4530", "ChatServer");

        peer = new PhotonPeer(this, ConnectionProtocol.WebSocket);
        peer.Connect("46.52.15.26:9090", "ChatServer");
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log(level + ": " + message);
    }

    public void OnEvent(EventData eventData)
    {
        Debug.Log("Event: " + eventData.Code);
        if (eventData.Code == 1)
        {
            Debug.Log("Chat: " + eventData.Parameters[1]);
        }
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        Debug.Log("Response: " + operationResponse.OperationCode);
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        Debug.Log("Status: " + statusCode);
        if (statusCode == StatusCode.Connect)
        {
            connected = true;
        }
    }
}

