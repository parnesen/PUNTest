// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.GpType
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// The gp type.
  /// 
  /// </summary>
  internal enum GpType : byte
  {
    Unknown = (byte) 0,
    Null = (byte) 42,
    Dictionary = (byte) 68,
    StringArray = (byte) 97,
    Byte = (byte) 98,
    Custom = (byte) 99,
    Double = (byte) 100,
    EventData = (byte) 101,
    Float = (byte) 102,
    Hashtable = (byte) 104,
    Integer = (byte) 105,
    Short = (byte) 107,
    Long = (byte) 108,
    IntegerArray = (byte) 110,
    Boolean = (byte) 111,
    OperationResponse = (byte) 112,
    OperationRequest = (byte) 113,
    String = (byte) 115,
    ByteArray = (byte) 120,
    Array = (byte) 121,
    ObjectArray = (byte) 122,
  }
}
