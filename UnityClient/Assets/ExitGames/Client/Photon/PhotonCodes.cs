// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.PhotonCodes
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

namespace ExitGames.Client.Photon
{
  internal static class PhotonCodes
  {
    /// <summary>
    /// Param code. Used in internal op: InitEncryption.
    /// </summary>
    internal static byte ClientKey = (byte) 1;
    /// <summary>
    /// Encryption-Mode code. Used in internal op: InitEncryption.
    /// </summary>
    internal static byte ModeKey = (byte) 2;
    /// <summary>
    /// Param code. Used in internal op: InitEncryption.
    /// </summary>
    internal static byte ServerKey = (byte) 1;
    /// <summary>
    /// Code of internal op: InitEncryption.
    /// </summary>
    internal static byte InitEncryption = (byte) 0;
    /// <summary>
    /// TODO: Code of internal op: Ping (used in PUN binary websockets).
    /// </summary>
    internal static byte Ping = (byte) 1;
    /// <summary>
    /// Result code for any (internal) operation.
    /// </summary>
    public const byte Ok = (byte) 0;
  }
}
