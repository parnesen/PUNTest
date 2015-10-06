// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.CmdLogSentReliable
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

namespace ExitGames.Client.Photon
{
  internal class CmdLogSentReliable : CmdLogItem
  {
    public int Resend;
    public int RoundtripTimeout;
    public int Timeout;
    public bool TriggeredTimeout;

    public CmdLogSentReliable(NCommand command, int timeInt, int rtt, int variance, bool triggeredTimeout = false)
    {
      this.TimeInt = timeInt;
      this.Channel = (int) command.commandChannelID;
      this.SequenceNumber = command.reliableSequenceNumber;
      this.Rtt = rtt;
      this.Variance = variance;
      this.Resend = (int) command.commandSentCount;
      this.RoundtripTimeout = command.roundTripTimeout;
      this.Timeout = command.timeoutTime;
      this.TriggeredTimeout = triggeredTimeout;
    }

    public override string ToString()
    {
      return string.Format("SND  NOW: {0,5}  CH: {1,3} SQ: {2,4} RTT: {3,4} VAR: {4,3}  Resend#: {5,2} ResendIn: {7} Timeout: {6,5} {8}", (object) this.TimeInt, (object) this.Channel, (object) this.SequenceNumber, (object) this.Rtt, (object) this.Variance, (object) this.Resend, (object) this.Timeout, (object) this.RoundtripTimeout, this.TriggeredTimeout ? (object) "< TIMEOUT" : (object) "");
    }
  }
}
