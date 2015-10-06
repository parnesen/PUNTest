// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.SerializeMethod
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// Type of serialization methods to add custom type support.
  ///             Use PhotonPeer.ReisterType() to register new types with serialization and deserialization methods.
  /// 
  /// </summary>
  /// <param name="customObject">The method will get objects passed that were registered with it in RegisterType().</param>
  /// <returns>
  /// Return a byte[] that resembles the object passed in. The framework will surround it with length and type info, so don't include it.
  /// </returns>
  public delegate byte[] SerializeMethod(object customObject);
}
