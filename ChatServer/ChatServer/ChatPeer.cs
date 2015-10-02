﻿using System;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;

public class ChatPeer : PeerBase
{
    private static readonly object syncRoot = new object();

    public ChatPeer(IRpcProtocol protocol, IPhotonPeer unmanagedPeer)
        : base(protocol, unmanagedPeer)
    {
        lock (syncRoot)
        {
            BroadcastMessage += this.OnBroadcastMessage;
        }
    }

    private static event Action<ChatPeer, EventData, SendParameters> BroadcastMessage;

    protected override void OnDisconnect(DisconnectReason disconnectCode, string reasonDetail)
    {
        lock (syncRoot)
        {
            BroadcastMessage -= this.OnBroadcastMessage;
        }
    }

    protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
    {
        var @event = new EventData(1) { Parameters = operationRequest.Parameters };
        lock (syncRoot)
        {
            BroadcastMessage(this, @event, sendParameters);
        }

        var response = new OperationResponse(operationRequest.OperationCode);
        this.SendOperationResponse(response, sendParameters);
    }

    private void OnBroadcastMessage(ChatPeer peer, EventData @event, SendParameters sendParameters)
    {
        if (peer != this)
        {
            this.SendEvent(@event, sendParameters);
        }
    }
}