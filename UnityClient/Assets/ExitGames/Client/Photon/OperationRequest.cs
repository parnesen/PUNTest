// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.OperationRequest
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// Container for an Operation request, which is a code and parameters.
  /// 
  /// </summary>
  /// 
  /// <remarks>
  /// On the lowest level, Photon only allows byte-typed keys for operation parameters.
  ///             The values of each such parameter can be any serializable datatype: byte, int, hashtable and many more.
  /// 
  /// </remarks>
  public class OperationRequest
  {
    /// <summary>
    /// Byte-typed code for an operation - the short identifier for the server's method to call.
    /// </summary>
    public byte OperationCode;
    /// <summary>
    /// The parameters of the operation - each identified by a byte-typed code in Photon.
    /// </summary>
    public Dictionary<byte, object> Parameters;
  }
}
