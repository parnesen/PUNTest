// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.CustomType
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System;

namespace ExitGames.Client.Photon
{
  internal class CustomType
  {
    public readonly byte Code;
    public readonly Type Type;
    public readonly SerializeMethod SerializeFunction;
    public readonly DeserializeMethod DeserializeFunction;
    public readonly SerializeStreamMethod SerializeStreamFunction;
    public readonly DeserializeStreamMethod DeserializeStreamFunction;

    public CustomType(Type type, byte code, SerializeMethod serializeFunction, DeserializeMethod deserializeFunction)
    {
      this.Type = type;
      this.Code = code;
      this.SerializeFunction = serializeFunction;
      this.DeserializeFunction = deserializeFunction;
    }

    public CustomType(Type type, byte code, SerializeStreamMethod serializeFunction, DeserializeStreamMethod deserializeFunction)
    {
      this.Type = type;
      this.Code = code;
      this.SerializeStreamFunction = serializeFunction;
      this.DeserializeStreamFunction = deserializeFunction;
    }
  }
}
