﻿// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.OperationResponse
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System.Collections;
using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// Contains the server's response for an operation called by this peer.
  ///             The indexer of this class actually provides access to the Parameters Dictionary.
  /// 
  /// </summary>
  /// 
  /// <remarks>
  /// The OperationCode defines the type of operation called on Photon and in turn also the Parameters that
  ///             are set in the request. Those are provided as Dictionary with byte-keys.
  ///             There are pre-defined constants for various codes defined in the LoadBalancing application.
  ///             Check: OperationCode, ParameterCode, etc.
  /// 
  /// <para/>
  /// 
  ///             An operation's request is summarized by the ReturnCode: a short typed code for "Ok" or
  ///             some different result. The code's meaning is specific per operation. An optional DebugMessage can be
  ///             provided to simplify debugging.
  /// 
  /// <para/>
  /// 
  ///             Each call of an operation gets an ID, called the "invocID". This can be matched to the IDs
  ///             returned with any operation calls. This way, an application could track if a certain OpRaiseEvent
  ///             call was successful.
  /// 
  /// </remarks>
  public class OperationResponse
  {
    /// <summary>
    /// The code for the operation called initially (by this peer).
    /// </summary>
    /// 
    /// <remarks>
    /// Use enums or constants to be able to handle those codes, like OperationCode does.
    /// </remarks>
    public byte OperationCode;
    /// <summary>
    /// A code that "summarizes" the operation's success or failure. Specific per operation. 0 usually means "ok".
    /// </summary>
    public short ReturnCode;
    /// <summary>
    /// An optional string sent by the server to provide readable feedback in error-cases. Might be null.
    /// </summary>
    public string DebugMessage;
    /// <summary>
    /// A Dictionary of values returned by an operation, using byte-typed keys per value.
    /// </summary>
    public Dictionary<byte, object> Parameters;

    /// <summary>
    /// Alternative access to the Parameters, which wraps up a TryGetValue() call on the Parameters Dictionary.
    /// 
    /// </summary>
    /// <param name="parameterCode">The byte-code of a returned value.</param>
    /// <returns>
    /// The value returned by the server, or null if the key does not exist in Parameters.
    /// </returns>
    public object this[byte parameterCode]
    {
      get
      {
        object obj;
        this.Parameters.TryGetValue(parameterCode, out obj);
        return obj;
      }
      set
      {
        this.Parameters[parameterCode] = value;
      }
    }

    /// <summary>
    /// ToString() override.
    /// </summary>
    /// 
    /// <returns>
    /// Relatively short output of OpCode and returnCode.
    /// </returns>
    public override string ToString()
    {
      return string.Format("OperationResponse {0}: ReturnCode: {1}.", (object) this.OperationCode, (object) this.ReturnCode);
    }

    /// <summary>
    /// Extensive output of operation results.
    /// </summary>
    /// 
    /// <returns>
    /// To be used in debug situations only, as it returns a string for each value.
    /// </returns>
    public string ToStringFull()
    {
      return string.Format("OperationResponse {0}: ReturnCode: {1} ({3}). Parameters: {2}", (object) this.OperationCode, (object) this.ReturnCode, (object) SupportClass.DictionaryToString((IDictionary) this.Parameters), (object) this.DebugMessage);
    }
  }
}
