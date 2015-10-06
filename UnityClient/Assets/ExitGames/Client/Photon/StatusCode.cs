// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.StatusCode
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System;

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// Enumeration of situations that change the peers internal status.
  ///             Used in calls to OnStatusChanged to inform your application of various situations that might happen.
  /// 
  /// </summary>
  /// 
  /// <remarks>
  /// Most of these codes are referenced somewhere else in the documentation when they are relevant to methods.
  /// 
  /// </remarks>
  public enum StatusCode
  {
    SecurityExceptionOnConnect = 1022,
    ExceptionOnConnect = 1023,
    Connect = 1024,
    Disconnect = 1025,
    Exception = 1026,
    QueueOutgoingReliableWarning = 1027,
    QueueOutgoingUnreliableWarning = 1029,
    SendError = 1030,
    QueueOutgoingAcksWarning = 1031,
    QueueIncomingReliableWarning = 1033,
    QueueIncomingUnreliableWarning = 1035,
    QueueSentWarning = 1037,
    ExceptionOnReceive = 1039,
    [Obsolete("Replaced by ExceptionOnReceive")] InternalReceiveException = 1039,
    TimeoutDisconnect = 1040,
    DisconnectByServer = 1041,
    DisconnectByServerUserLimit = 1042,
    DisconnectByServerLogic = 1043,
    [Obsolete("TCP routing was removed after becoming obsolete.")] TcpRouterResponseOk = 1044,
    [Obsolete("TCP routing was removed after becoming obsolete.")] TcpRouterResponseNodeIdUnknown = 1045,
    [Obsolete("TCP routing was removed after becoming obsolete.")] TcpRouterResponseEndpointUnknown = 1046,
    [Obsolete("TCP routing was removed after becoming obsolete.")] TcpRouterResponseNodeNotReady = 1047,
    EncryptionEstablished = 1048,
    EncryptionFailedToEstablish = 1049,
  }
}
