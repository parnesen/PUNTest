// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.DebugLevel
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// Level / amount of DebugReturn callbacks. Each debug level includes output for lower ones: OFF, ERROR, WARNING, INFO, ALL.
  /// 
  /// </summary>
  public enum DebugLevel : byte
  {
    OFF = (byte) 0,
    ERROR = (byte) 1,
    WARNING = (byte) 2,
    INFO = (byte) 3,
    ALL = (byte) 5,
  }
}
