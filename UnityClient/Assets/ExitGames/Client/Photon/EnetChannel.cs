// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.EnetChannel
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
  internal class EnetChannel
  {
    internal byte ChannelNumber;
    internal Dictionary<int, NCommand> incomingReliableCommandsList;
    internal Dictionary<int, NCommand> incomingUnreliableCommandsList;
    internal Queue<NCommand> outgoingReliableCommandsList;
    internal Queue<NCommand> outgoingUnreliableCommandsList;
    internal int incomingReliableSequenceNumber;
    internal int incomingUnreliableSequenceNumber;
    internal int outgoingReliableSequenceNumber;
    internal int outgoingUnreliableSequenceNumber;

    public EnetChannel(byte channelNumber, int commandBufferSize)
    {
      this.ChannelNumber = channelNumber;
      this.incomingReliableCommandsList = new Dictionary<int, NCommand>(commandBufferSize);
      this.incomingUnreliableCommandsList = new Dictionary<int, NCommand>(commandBufferSize);
      this.outgoingReliableCommandsList = new Queue<NCommand>(commandBufferSize);
      this.outgoingUnreliableCommandsList = new Queue<NCommand>(commandBufferSize);
    }

    public bool ContainsUnreliableSequenceNumber(int unreliableSequenceNumber)
    {
      return this.incomingUnreliableCommandsList.ContainsKey(unreliableSequenceNumber);
    }

    public NCommand FetchUnreliableSequenceNumber(int unreliableSequenceNumber)
    {
      return this.incomingUnreliableCommandsList[unreliableSequenceNumber];
    }

    public bool ContainsReliableSequenceNumber(int reliableSequenceNumber)
    {
      return this.incomingReliableCommandsList.ContainsKey(reliableSequenceNumber);
    }

    public NCommand FetchReliableSequenceNumber(int reliableSequenceNumber)
    {
      return this.incomingReliableCommandsList[reliableSequenceNumber];
    }

    public void clearAll()
    {
      lock (this)
      {
        this.incomingReliableCommandsList.Clear();
        this.incomingUnreliableCommandsList.Clear();
        this.outgoingReliableCommandsList.Clear();
        this.outgoingUnreliableCommandsList.Clear();
      }
    }
  }
}
