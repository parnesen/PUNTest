using Photon.SocketServer;

public class ChatServer : ApplicationBase
{
    protected override PeerBase CreatePeer(InitRequest initRequest)
    {
        return new ChatPeer(initRequest.Protocol, initRequest.PhotonPeer);
    }

    protected override void Setup()
    {
    }

    protected override void TearDown()
    {
    }
}