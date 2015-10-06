// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.CmdLogReceivedAck
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

namespace ExitGames.Client.Photon
{
  internal class CmdLogReceivedAck : CmdLogItem
  {
    public int ReceivedSentTime;

    public CmdLogReceivedAck(NCommand command, int timeInt, int rtt, int variance)
    {
      this.TimeInt = timeInt;
      this.Channel = (int) command.commandChannelID;
      this.SequenceNumber = command.ackReceivedReliableSequenceNumber;
      this.Rtt = rtt;
      this.Variance = variance;
      this.ReceivedSentTime = command.ackReceivedSentTime;
    }

    public override string ToString()
    {
      return string.Format("ACK  NOW: {0,5}  CH: {1,3} SQ: {2,4} RTT: {3,4} VAR: {4,3}  Sent: {5,5} Diff: {6,4}", (object) this.TimeInt, (object) this.Channel, (object) this.SequenceNumber, (object) this.Rtt, (object) this.Variance, (object) this.ReceivedSentTime, (object) (this.TimeInt - this.ReceivedSentTime));
    }
  }
}
