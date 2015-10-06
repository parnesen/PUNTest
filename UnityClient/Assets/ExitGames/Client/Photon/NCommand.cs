// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.NCommand
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System;

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// Internal class for "commands" - the package in which operations are sent.
  /// </summary>
  internal class NCommand : IComparable<NCommand>
  {
    internal byte reservedByte = (byte) 4;
    internal const int FLAG_RELIABLE = 1;
    internal const int FLAG_UNSEQUENCED = 2;
    internal const byte FV_UNRELIABLE = (byte) 0;
    internal const byte FV_RELIABLE = (byte) 1;
    internal const byte FV_UNRELIBALE_UNSEQUENCED = (byte) 2;
    internal const byte CT_NONE = (byte) 0;
    internal const byte CT_ACK = (byte) 1;
    internal const byte CT_CONNECT = (byte) 2;
    internal const byte CT_VERIFYCONNECT = (byte) 3;
    internal const byte CT_DISCONNECT = (byte) 4;
    internal const byte CT_PING = (byte) 5;
    internal const byte CT_SENDRELIABLE = (byte) 6;
    internal const byte CT_SENDUNRELIABLE = (byte) 7;
    internal const byte CT_SENDFRAGMENT = (byte) 8;
    internal const byte CT_EG_SERVERTIME = (byte) 12;
    internal const int HEADER_UDP_PACK_LENGTH = 12;
    internal const int CmdSizeMinimum = 12;
    internal const int CmdSizeAck = 20;
    internal const int CmdSizeConnect = 44;
    internal const int CmdSizeVerifyConnect = 44;
    internal const int CmdSizeDisconnect = 12;
    internal const int CmdSizePing = 12;
    internal const int CmdSizeReliableHeader = 12;
    internal const int CmdSizeUnreliableHeader = 16;
    internal const int CmdSizeFragmentHeader = 32;
    internal const int CmdSizeMaxHeader = 36;
    internal byte commandFlags;
    internal byte commandType;
    internal byte commandChannelID;
    internal int reliableSequenceNumber;
    internal int unreliableSequenceNumber;
    internal int unsequencedGroupNumber;
    internal int startSequenceNumber;
    internal int fragmentCount;
    internal int fragmentNumber;
    internal int totalLength;
    internal int fragmentOffset;
    internal int fragmentsRemaining;
    internal byte[] Payload;
    internal int commandSentTime;
    internal byte commandSentCount;
    internal int roundTripTimeout;
    internal int timeoutTime;
    internal int ackReceivedReliableSequenceNumber;
    internal int ackReceivedSentTime;
    private byte[] completeCommand;
    internal int Size;

    /// <summary>
    /// this variant does only create outgoing commands and increments . incoming ones are created from a DataInputStream
    /// </summary>
    internal NCommand(EnetPeer peer, byte commandType, byte[] payload, byte channel)
    {
      this.commandType = commandType;
      this.commandFlags = (byte) 1;
      this.commandChannelID = channel;
      this.Payload = payload;
      this.Size = 12;
      switch (this.commandType)
      {
        case (byte) 1:
          this.Size = 20;
          this.commandFlags = (byte) 0;
          break;
        case (byte) 2:
          this.Size = 44;
          this.Payload = new byte[32];
          this.Payload[0] = (byte) 0;
          this.Payload[1] = (byte) 0;
          int targetOffset = 2;
          Protocol.Serialize((short) peer.mtu, this.Payload, ref targetOffset);
          this.Payload[4] = (byte) 0;
          this.Payload[5] = (byte) 0;
          this.Payload[6] = (byte) sbyte.MinValue;
          this.Payload[7] = (byte) 0;
          this.Payload[11] = peer.ChannelCount;
          this.Payload[15] = (byte) 0;
          this.Payload[19] = (byte) 0;
          this.Payload[22] = (byte) 19;
          this.Payload[23] = (byte) 136;
          this.Payload[27] = (byte) 2;
          this.Payload[31] = (byte) 2;
          break;
        case (byte) 4:
          this.Size = 12;
          if (peer.peerConnectionState == PeerBase.ConnectionStateValue.Connected)
            break;
          this.commandFlags = (byte) 2;
          if (peer.peerConnectionState == PeerBase.ConnectionStateValue.Zombie)
            this.reservedByte = (byte) 2;
          break;
        case (byte) 6:
          this.Size = 12 + payload.Length;
          break;
        case (byte) 7:
          this.Size = 16 + payload.Length;
          this.commandFlags = (byte) 0;
          break;
        case (byte) 8:
          this.Size = 32 + payload.Length;
          break;
      }
    }

    /// <summary>
    /// reads the command values (commandHeader and command-values) from incoming bytestream and populates the incoming command*
    /// </summary>
    internal NCommand(EnetPeer peer, byte[] inBuff, ref int readingOffset)
    {
      this.commandType = inBuff[readingOffset++];
      this.commandChannelID = inBuff[readingOffset++];
      this.commandFlags = inBuff[readingOffset++];
      this.reservedByte = inBuff[readingOffset++];
      Protocol.Deserialize(out this.Size, inBuff, ref readingOffset);
      Protocol.Deserialize(out this.reliableSequenceNumber, inBuff, ref readingOffset);
      peer.bytesIn += (long) this.Size;
      switch (this.commandType)
      {
        case (byte) 1:
          Protocol.Deserialize(out this.ackReceivedReliableSequenceNumber, inBuff, ref readingOffset);
          Protocol.Deserialize(out this.ackReceivedSentTime, inBuff, ref readingOffset);
          break;
        case (byte) 3:
          short num;
          Protocol.Deserialize(out num, inBuff, ref readingOffset);
          readingOffset += 30;
          if ((int) peer.peerID == -1)
          {
            peer.peerID = num;
            break;
          }
          break;
        case (byte) 6:
          this.Payload = new byte[this.Size - 12];
          break;
        case (byte) 7:
          Protocol.Deserialize(out this.unreliableSequenceNumber, inBuff, ref readingOffset);
          this.Payload = new byte[this.Size - 16];
          break;
        case (byte) 8:
          Protocol.Deserialize(out this.startSequenceNumber, inBuff, ref readingOffset);
          Protocol.Deserialize(out this.fragmentCount, inBuff, ref readingOffset);
          Protocol.Deserialize(out this.fragmentNumber, inBuff, ref readingOffset);
          Protocol.Deserialize(out this.totalLength, inBuff, ref readingOffset);
          Protocol.Deserialize(out this.fragmentOffset, inBuff, ref readingOffset);
          this.Payload = new byte[this.Size - 32];
          this.fragmentsRemaining = this.fragmentCount;
          break;
      }
      if (this.Payload == null)
        return;
      Buffer.BlockCopy((Array) inBuff, readingOffset, (Array) this.Payload, 0, this.Payload.Length);
      readingOffset += this.Payload.Length;
    }

    internal static NCommand CreateAck(EnetPeer peer, NCommand commandToAck, int sentTime)
    {
      byte[] numArray = new byte[8];
      int targetOffset = 0;
      Protocol.Serialize(commandToAck.reliableSequenceNumber, numArray, ref targetOffset);
      Protocol.Serialize(sentTime, numArray, ref targetOffset);
      return new NCommand(peer, (byte) 1, numArray, commandToAck.commandChannelID)
      {
        ackReceivedReliableSequenceNumber = commandToAck.reliableSequenceNumber,
        ackReceivedSentTime = sentTime
      };
    }

    internal byte[] Serialize()
    {
      if (this.completeCommand != null)
        return this.completeCommand;
      int count = this.Payload == null ? 0 : this.Payload.Length;
      int dstOffset = 12;
      if ((int) this.commandType == 7)
        dstOffset = 16;
      else if ((int) this.commandType == 8)
        dstOffset = 32;
      this.completeCommand = new byte[dstOffset + count];
      this.completeCommand[0] = this.commandType;
      this.completeCommand[1] = this.commandChannelID;
      this.completeCommand[2] = this.commandFlags;
      this.completeCommand[3] = this.reservedByte;
      int targetOffset = 4;
      Protocol.Serialize(this.completeCommand.Length, this.completeCommand, ref targetOffset);
      Protocol.Serialize(this.reliableSequenceNumber, this.completeCommand, ref targetOffset);
      if ((int) this.commandType == 7)
      {
        targetOffset = 12;
        Protocol.Serialize(this.unreliableSequenceNumber, this.completeCommand, ref targetOffset);
      }
      else if ((int) this.commandType == 8)
      {
        targetOffset = 12;
        Protocol.Serialize(this.startSequenceNumber, this.completeCommand, ref targetOffset);
        Protocol.Serialize(this.fragmentCount, this.completeCommand, ref targetOffset);
        Protocol.Serialize(this.fragmentNumber, this.completeCommand, ref targetOffset);
        Protocol.Serialize(this.totalLength, this.completeCommand, ref targetOffset);
        Protocol.Serialize(this.fragmentOffset, this.completeCommand, ref targetOffset);
      }
      if (count > 0)
        Buffer.BlockCopy((Array) this.Payload, 0, (Array) this.completeCommand, dstOffset, count);
      this.Payload = (byte[]) null;
      return this.completeCommand;
    }

    public int CompareTo(NCommand other)
    {
      if (((int) this.commandFlags & 1) != 0)
        return this.reliableSequenceNumber - other.reliableSequenceNumber;
      return this.unreliableSequenceNumber - other.unreliableSequenceNumber;
    }

    public override string ToString()
    {
      if ((int) this.commandType == 1)
        return string.Format("CMD({1} ack for c#:{0} s#/time {2}/{3})", (object) this.commandChannelID, (object) this.commandType, (object) this.ackReceivedReliableSequenceNumber, (object) this.ackReceivedSentTime);
      return string.Format("CMD({1} c#:{0} r/u: {2}/{3} st/r#/rt:{4}/{5}/{6})", (object) this.commandChannelID, (object) this.commandType, (object) this.reliableSequenceNumber, (object) this.unreliableSequenceNumber, (object) this.commandSentTime, (object) this.commandSentCount, (object) this.timeoutTime);
    }
  }
}
