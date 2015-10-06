// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.Protocol
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// Provides tools for the Exit Games Protocol
  /// 
  /// </summary>
  public class Protocol
  {
    internal static readonly Dictionary<Type, CustomType> TypeDict = new Dictionary<Type, CustomType>();
    internal static readonly Dictionary<byte, CustomType> CodeDict = new Dictionary<byte, CustomType>();
    private static readonly byte[] memShort = new byte[2];
    private static readonly long[] memLongBlock = new long[1];
    private static readonly byte[] memLongBlockBytes = new byte[8];
    private static readonly float[] memFloatBlock = new float[1];
    private static readonly byte[] memFloatBlockBytes = new byte[4];
    private static readonly double[] memDoubleBlock = new double[1];
    private static readonly byte[] memDoubleBlockBytes = new byte[8];
    private static readonly byte[] memInteger = new byte[4];
    private static readonly byte[] memLong = new byte[8];
    private static readonly byte[] memFloat = new byte[4];
    private static readonly byte[] memDeserialize = new byte[4];
    private static readonly byte[] memDouble = new byte[8];
    public const string protocolType = "GpBinaryV16";

    internal static bool TryRegisterType(Type type, byte typeCode, SerializeMethod serializeFunction, DeserializeMethod deserializeFunction)
    {
      if (Protocol.CodeDict.ContainsKey(typeCode) || Protocol.TypeDict.ContainsKey(type))
        return false;
      CustomType customType = new CustomType(type, typeCode, serializeFunction, deserializeFunction);
      Protocol.CodeDict.Add(typeCode, customType);
      Protocol.TypeDict.Add(type, customType);
      return true;
    }

    internal static bool TryRegisterType(Type type, byte typeCode, SerializeStreamMethod serializeFunction, DeserializeStreamMethod deserializeFunction)
    {
      if (Protocol.CodeDict.ContainsKey(typeCode) || Protocol.TypeDict.ContainsKey(type))
        return false;
      CustomType customType = new CustomType(type, typeCode, serializeFunction, deserializeFunction);
      Protocol.CodeDict.Add(typeCode, customType);
      Protocol.TypeDict.Add(type, customType);
      return true;
    }

    private static bool SerializeCustom(MemoryStream dout, object serObject)
    {
      CustomType customType;
      if (!Protocol.TypeDict.TryGetValue(serObject.GetType(), out customType))
        return false;
      if (customType.SerializeStreamFunction == null)
      {
        byte[] buffer = customType.SerializeFunction(serObject);
        dout.WriteByte((byte) 99);
        dout.WriteByte(customType.Code);
        Protocol.SerializeShort(dout, (short) buffer.Length, false);
        dout.Write(buffer, 0, buffer.Length);
        return true;
      }
      dout.WriteByte((byte) 99);
      dout.WriteByte(customType.Code);
      long position1 = dout.Position;
      dout.Position = dout.Position + 2L;
      short serObject1 = customType.SerializeStreamFunction(dout, serObject);
      long position2 = dout.Position;
      dout.Position = position1;
      Protocol.SerializeShort(dout, serObject1, false);
      dout.Position = dout.Position + (long) serObject1;
      if (dout.Position != position2)
        throw new Exception("Serialization failed. Stream position corrupted. Should be " + (object) position2 + " is now: " + (string) (object) dout.Position + " serializedLength: " + (string) (object) serObject1);
      return true;
    }

    private static object DeserializeCustom(MemoryStream din, byte customTypeCode)
    {
      short length = Protocol.DeserializeShort(din);
      CustomType customType;
      if (!Protocol.CodeDict.TryGetValue(customTypeCode, out customType))
        return (object) null;
      if (customType.DeserializeStreamFunction == null)
      {
        byte[] numArray = new byte[(int) length];
        din.Read(numArray, 0, (int) length);
        return customType.DeserializeFunction(numArray);
      }
      long position = din.Position;
      object obj = customType.DeserializeStreamFunction(din, length);
      if ((int) (din.Position - position) != (int) length)
        din.Position = position + (long) length;
      return obj;
    }

    /// <summary>
    /// Serialize creates a byte-array from the given object and returns it.
    /// 
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <returns>
    /// The serialized byte-array
    /// </returns>
    public static byte[] Serialize(object obj)
    {
      MemoryStream dout = new MemoryStream(64);
      Protocol.Serialize(dout, obj, true);
      return dout.ToArray();
    }

    /// <summary>
    /// Deserialize returns an object reassembled from the given byte-array.
    /// 
    /// </summary>
    /// <param name="serializedData">The byte-array to be Deserialized</param>
    /// <returns>
    /// The Deserialized object
    /// </returns>
    public static object Deserialize(byte[] serializedData)
    {
      MemoryStream din = new MemoryStream(serializedData);
      return Protocol.Deserialize(din, (byte) din.ReadByte());
    }

    internal static object DeserializeMessage(MemoryStream stream)
    {
      return Protocol.Deserialize(stream, (byte) stream.ReadByte());
    }

    internal static byte[] DeserializeRawMessage(MemoryStream stream)
    {
      return (byte[]) Protocol.Deserialize(stream, (byte) stream.ReadByte());
    }

    internal static void SerializeMessage(MemoryStream ms, object msg)
    {
      Protocol.Serialize(ms, msg, true);
    }

    private static Type GetTypeOfCode(byte typeCode)
    {
      byte num = typeCode;
      if ((uint) num <= 42U)
      {
        if ((int) num == 0 || (int) num == 42)
          return typeof (object);
      }
      else
      {
        switch (num)
        {
          case (byte) 68:
            return typeof (IDictionary);
          case (byte) 97:
            return typeof (string[]);
          case (byte) 98:
            return typeof (byte);
          case (byte) 99:
            return typeof (CustomType);
          case (byte) 100:
            return typeof (double);
          case (byte) 101:
            return typeof (EventData);
          case (byte) 102:
            return typeof (float);
          case (byte) 104:
            return typeof (Hashtable);
          case (byte) 105:
            return typeof (int);
          case (byte) 107:
            return typeof (short);
          case (byte) 108:
            return typeof (long);
          case (byte) 110:
            return typeof (int[]);
          case (byte) 111:
            return typeof (bool);
          case (byte) 112:
            return typeof (OperationResponse);
          case (byte) 113:
            return typeof (OperationRequest);
          case (byte) 115:
            return typeof (string);
          case (byte) 120:
            return typeof (byte[]);
          case (byte) 121:
            return typeof (Array);
          case (byte) 122:
            return typeof (object[]);
        }
      }
      Debug.WriteLine("missing type: " + (object) typeCode);
      throw new Exception("deserialize(): " + (object) typeCode);
    }

    private static GpType GetCodeOfType(Type type)
    {
      switch (Type.GetTypeCode(type))
      {
        case TypeCode.Boolean:
          return GpType.Boolean;
        case TypeCode.Byte:
          return GpType.Byte;
        case TypeCode.Int16:
          return GpType.Short;
        case TypeCode.Int32:
          return GpType.Integer;
        case TypeCode.Int64:
          return GpType.Long;
        case TypeCode.Single:
          return GpType.Float;
        case TypeCode.Double:
          return GpType.Double;
        case TypeCode.String:
          return GpType.String;
        default:
          if (type.IsArray)
            return type == typeof (byte[]) ? GpType.ByteArray : GpType.Array;
          if (type == typeof (Hashtable))
            return GpType.Hashtable;
          if (type.IsGenericType && typeof (Dictionary<,>) == type.GetGenericTypeDefinition())
            return GpType.Dictionary;
          if (type == typeof (EventData))
            return GpType.EventData;
          if (type == typeof (OperationRequest))
            return GpType.OperationRequest;
          return type == typeof (OperationResponse) ? GpType.OperationResponse : GpType.Unknown;
      }
    }

    private static Array CreateArrayByType(byte arrayType, short length)
    {
      return Array.CreateInstance(Protocol.GetTypeOfCode(arrayType), (int) length);
    }

    internal static void SerializeOperationRequest(MemoryStream memStream, OperationRequest serObject, bool setType)
    {
      Protocol.SerializeOperationRequest(memStream, serObject.OperationCode, serObject.Parameters, setType);
    }

    internal static void SerializeOperationRequest(MemoryStream memStream, byte operationCode, Dictionary<byte, object> parameters, bool setType)
    {
      if (setType)
        memStream.WriteByte((byte) 113);
      memStream.WriteByte(operationCode);
      Protocol.SerializeParameterTable(memStream, parameters);
    }

    internal static OperationRequest DeserializeOperationRequest(MemoryStream din)
    {
      return new OperationRequest()
      {
        OperationCode = Protocol.DeserializeByte(din),
        Parameters = Protocol.DeserializeParameterTable(din)
      };
    }

    internal static void SerializeOperationResponse(MemoryStream memStream, OperationResponse serObject, bool setType)
    {
      if (setType)
        memStream.WriteByte((byte) 112);
      memStream.WriteByte(serObject.OperationCode);
      Protocol.SerializeShort(memStream, serObject.ReturnCode, false);
      if (string.IsNullOrEmpty(serObject.DebugMessage))
        memStream.WriteByte((byte) 42);
      else
        Protocol.SerializeString(memStream, serObject.DebugMessage, false);
      Protocol.SerializeParameterTable(memStream, serObject.Parameters);
    }

    internal static OperationResponse DeserializeOperationResponse(MemoryStream memoryStream)
    {
      return new OperationResponse()
      {
        OperationCode = Protocol.DeserializeByte(memoryStream),
        ReturnCode = Protocol.DeserializeShort(memoryStream),
        DebugMessage = Protocol.Deserialize(memoryStream, Protocol.DeserializeByte(memoryStream)) as string,
        Parameters = Protocol.DeserializeParameterTable(memoryStream)
      };
    }

    internal static void SerializeEventData(MemoryStream memStream, EventData serObject, bool setType)
    {
      if (setType)
        memStream.WriteByte((byte) 101);
      memStream.WriteByte(serObject.Code);
      Protocol.SerializeParameterTable(memStream, serObject.Parameters);
    }

    internal static EventData DeserializeEventData(MemoryStream din)
    {
      return new EventData()
      {
        Code = Protocol.DeserializeByte(din),
        Parameters = Protocol.DeserializeParameterTable(din)
      };
    }

    private static void SerializeParameterTable(MemoryStream memStream, Dictionary<byte, object> parameters)
    {
      if (parameters == null || parameters.Count == 0)
      {
        Protocol.SerializeShort(memStream, (short) 0, false);
      }
      else
      {
        Protocol.SerializeShort(memStream, (short) parameters.Count, false);
        foreach (KeyValuePair<byte, object> keyValuePair in parameters)
        {
          memStream.WriteByte(keyValuePair.Key);
          Protocol.Serialize(memStream, keyValuePair.Value, true);
        }
      }
    }

    private static Dictionary<byte, object> DeserializeParameterTable(MemoryStream memoryStream)
    {
      short num = Protocol.DeserializeShort(memoryStream);
      Dictionary<byte, object> dictionary = new Dictionary<byte, object>((int) num);
      for (int index1 = 0; index1 < (int) num; ++index1)
      {
        byte index2 = (byte) memoryStream.ReadByte();
        object obj = Protocol.Deserialize(memoryStream, (byte) memoryStream.ReadByte());
        dictionary[index2] = obj;
      }
      return dictionary;
    }

    /// <summary>
    /// Calls the correct serialization method for the passed object.
    /// 
    /// </summary>
    private static void Serialize(MemoryStream dout, object serObject, bool setType)
    {
      if (serObject == null)
      {
        if (!setType)
          return;
        dout.WriteByte((byte) 42);
      }
      else
      {
        switch (Protocol.GetCodeOfType(serObject.GetType()))
        {
          case GpType.Dictionary:
            Protocol.SerializeDictionary(dout, (IDictionary) serObject, setType);
            break;
          case GpType.Byte:
            Protocol.SerializeByte(dout, (byte) serObject, setType);
            break;
          case GpType.Double:
            Protocol.SerializeDouble(dout, (double) serObject, setType);
            break;
          case GpType.EventData:
            Protocol.SerializeEventData(dout, (EventData) serObject, setType);
            break;
          case GpType.Float:
            Protocol.SerializeFloat(dout, (float) serObject, setType);
            break;
          case GpType.Hashtable:
            Protocol.SerializeHashTable(dout, (Hashtable) serObject, setType);
            break;
          case GpType.Integer:
            Protocol.SerializeInteger(dout, (int) serObject, setType);
            break;
          case GpType.Short:
            Protocol.SerializeShort(dout, (short) serObject, setType);
            break;
          case GpType.Long:
            Protocol.SerializeLong(dout, (long) serObject, setType);
            break;
          case GpType.Boolean:
            Protocol.SerializeBoolean(dout, (bool) serObject, setType);
            break;
          case GpType.OperationResponse:
            Protocol.SerializeOperationResponse(dout, (OperationResponse) serObject, setType);
            break;
          case GpType.OperationRequest:
            Protocol.SerializeOperationRequest(dout, (OperationRequest) serObject, setType);
            break;
          case GpType.String:
            Protocol.SerializeString(dout, (string) serObject, setType);
            break;
          case GpType.ByteArray:
            Protocol.SerializeByteArray(dout, (byte[]) serObject, setType);
            break;
          case GpType.Array:
            if (serObject is int[])
            {
              Protocol.SerializeIntArrayOptimized(dout, (int[]) serObject, setType);
              break;
            }
            if (serObject.GetType().GetElementType() == typeof (object))
            {
              Protocol.SerializeObjectArray(dout, serObject as object[], setType);
              break;
            }
            Protocol.SerializeArray(dout, (Array) serObject, setType);
            break;
          default:
            if (Protocol.SerializeCustom(dout, serObject))
              break;
            throw new Exception("cannot serialize(): " + (object) serObject.GetType());
        }
      }
    }

    private static void SerializeByte(MemoryStream dout, byte serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 98);
      dout.WriteByte(serObject);
    }

    private static void SerializeBoolean(MemoryStream dout, bool serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 111);
      dout.WriteByte(serObject ? (byte) 1 : (byte) 0);
    }

    private static void SerializeShort(MemoryStream dout, short serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 107);
      lock (Protocol.memShort)
      {
        byte[] local_0 = Protocol.memShort;
        local_0[0] = (byte) ((uint) serObject >> 8);
        local_0[1] = (byte) serObject;
        dout.Write(local_0, 0, 2);
      }
    }

    /// <summary>
    /// Serializes a short typed value into a byte-array (target) starting at the also given targetOffset.
    ///             The altered offset is known to the caller, because it is given via a referenced parameter.
    /// 
    /// </summary>
    /// <param name="value">The short value to be serialized</param><param name="target">The byte-array to serialize the short to</param><param name="targetOffset">The offset in the byte-array</param>
    public static void Serialize(short value, byte[] target, ref int targetOffset)
    {
      target[targetOffset++] = (byte) ((uint) value >> 8);
      target[targetOffset++] = (byte) value;
    }

    private static void SerializeInteger(MemoryStream dout, int serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 105);
      lock (Protocol.memInteger)
      {
        byte[] local_0 = Protocol.memInteger;
        local_0[0] = (byte) (serObject >> 24);
        local_0[1] = (byte) (serObject >> 16);
        local_0[2] = (byte) (serObject >> 8);
        local_0[3] = (byte) serObject;
        dout.Write(local_0, 0, 4);
      }
    }

    /// <summary>
    /// Serializes an int typed value into a byte-array (target) starting at the also given targetOffset.
    ///             The altered offset is known to the caller, because it is given via a referenced parameter.
    /// 
    /// </summary>
    /// <param name="value">The int value to be serialized</param><param name="target">The byte-array to serialize the short to</param><param name="targetOffset">The offset in the byte-array</param>
    public static void Serialize(int value, byte[] target, ref int targetOffset)
    {
      target[targetOffset++] = (byte) (value >> 24);
      target[targetOffset++] = (byte) (value >> 16);
      target[targetOffset++] = (byte) (value >> 8);
      target[targetOffset++] = (byte) value;
    }

    private static void SerializeLong(MemoryStream dout, long serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 108);
      lock (Protocol.memLongBlock)
      {
        Protocol.memLongBlock[0] = serObject;
        Buffer.BlockCopy((Array) Protocol.memLongBlock, 0, (Array) Protocol.memLongBlockBytes, 0, 8);
        byte[] local_0 = Protocol.memLongBlockBytes;
        if (BitConverter.IsLittleEndian)
        {
          byte local_1 = local_0[0];
          byte local_2 = local_0[1];
          byte local_3 = local_0[2];
          byte local_4 = local_0[3];
          local_0[0] = local_0[7];
          local_0[1] = local_0[6];
          local_0[2] = local_0[5];
          local_0[3] = local_0[4];
          local_0[4] = local_4;
          local_0[5] = local_3;
          local_0[6] = local_2;
          local_0[7] = local_1;
        }
        dout.Write(local_0, 0, 8);
      }
    }

    private static void SerializeFloat(MemoryStream dout, float serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 102);
      lock (Protocol.memFloatBlockBytes)
      {
        Protocol.memFloatBlock[0] = serObject;
        Buffer.BlockCopy((Array) Protocol.memFloatBlock, 0, (Array) Protocol.memFloatBlockBytes, 0, 4);
        if (BitConverter.IsLittleEndian)
        {
          byte local_0 = Protocol.memFloatBlockBytes[0];
          byte local_1 = Protocol.memFloatBlockBytes[1];
          Protocol.memFloatBlockBytes[0] = Protocol.memFloatBlockBytes[3];
          Protocol.memFloatBlockBytes[1] = Protocol.memFloatBlockBytes[2];
          Protocol.memFloatBlockBytes[2] = local_1;
          Protocol.memFloatBlockBytes[3] = local_0;
        }
        dout.Write(Protocol.memFloatBlockBytes, 0, 4);
      }
    }

    /// <summary>
    /// Serializes an float typed value into a byte-array (target) starting at the also given targetOffset.
    ///             The altered offset is known to the caller, because it is given via a referenced parameter.
    /// 
    /// </summary>
    /// <param name="value">The float value to be serialized</param><param name="target">The byte-array to serialize the short to</param><param name="targetOffset">The offset in the byte-array</param>
    public static void Serialize(float value, byte[] target, ref int targetOffset)
    {
      lock (Protocol.memFloatBlock)
      {
        Protocol.memFloatBlock[0] = value;
        Buffer.BlockCopy((Array) Protocol.memFloatBlock, 0, (Array) target, targetOffset, 4);
      }
      if (BitConverter.IsLittleEndian)
      {
        byte num1 = target[targetOffset];
        byte num2 = target[targetOffset + 1];
        target[targetOffset] = target[targetOffset + 3];
        target[targetOffset + 1] = target[targetOffset + 2];
        target[targetOffset + 2] = num2;
        target[targetOffset + 3] = num1;
      }
      targetOffset += 4;
    }

    private static void SerializeDouble(MemoryStream dout, double serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 100);
      lock (Protocol.memDoubleBlockBytes)
      {
        Protocol.memDoubleBlock[0] = serObject;
        Buffer.BlockCopy((Array) Protocol.memDoubleBlock, 0, (Array) Protocol.memDoubleBlockBytes, 0, 8);
        byte[] local_0 = Protocol.memDoubleBlockBytes;
        if (BitConverter.IsLittleEndian)
        {
          byte local_1 = local_0[0];
          byte local_2 = local_0[1];
          byte local_3 = local_0[2];
          byte local_4 = local_0[3];
          local_0[0] = local_0[7];
          local_0[1] = local_0[6];
          local_0[2] = local_0[5];
          local_0[3] = local_0[4];
          local_0[4] = local_4;
          local_0[5] = local_3;
          local_0[6] = local_2;
          local_0[7] = local_1;
        }
        dout.Write(local_0, 0, 8);
      }
    }

    private static void SerializeString(MemoryStream dout, string serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 115);
      byte[] bytes = Encoding.UTF8.GetBytes(serObject);
      if (bytes.Length > (int) short.MaxValue)
        throw new NotSupportedException("Strings that exceed a UTF8-encoded byte-length of 32767 (short.MaxValue) are not supported. Yours is: " + (object) bytes.Length);
      Protocol.SerializeShort(dout, (short) bytes.Length, false);
      dout.Write(bytes, 0, bytes.Length);
    }

    private static void SerializeArray(MemoryStream dout, Array serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 121);
      if (serObject.Length > (int) short.MaxValue)
        throw new NotSupportedException("String[] that exceed 32767 (short.MaxValue) entries are not supported. Yours is: " + (object) serObject.Length);
      Protocol.SerializeShort(dout, (short) serObject.Length, false);
      Type elementType = serObject.GetType().GetElementType();
      GpType codeOfType = Protocol.GetCodeOfType(elementType);
      if (codeOfType != GpType.Unknown)
      {
        dout.WriteByte((byte) codeOfType);
        if (codeOfType == GpType.Dictionary)
        {
          bool setKeyType;
          bool setValueType;
          Protocol.SerializeDictionaryHeader(dout, (object) serObject, out setKeyType, out setValueType);
          for (int index = 0; index < serObject.Length; ++index)
          {
            object dict = serObject.GetValue(index);
            Protocol.SerializeDictionaryElements(dout, dict, setKeyType, setValueType);
          }
        }
        else
        {
          for (int index = 0; index < serObject.Length; ++index)
          {
            object serObject1 = serObject.GetValue(index);
            Protocol.Serialize(dout, serObject1, false);
          }
        }
      }
      else
      {
        CustomType customType;
        if (!Protocol.TypeDict.TryGetValue(elementType, out customType))
          throw new NotSupportedException("cannot serialize array of type " + (object) elementType);
        dout.WriteByte((byte) 99);
        dout.WriteByte(customType.Code);
        for (int index = 0; index < serObject.Length; ++index)
        {
          object customObject = serObject.GetValue(index);
          if (customType.SerializeStreamFunction == null)
          {
            byte[] buffer = customType.SerializeFunction(customObject);
            Protocol.SerializeShort(dout, (short) buffer.Length, false);
            dout.Write(buffer, 0, buffer.Length);
          }
          else
          {
            long position1 = dout.Position;
            dout.Position = dout.Position + 2L;
            short serObject1 = customType.SerializeStreamFunction(dout, customObject);
            long position2 = dout.Position;
            dout.Position = position1;
            Protocol.SerializeShort(dout, serObject1, false);
            dout.Position = dout.Position + (long) serObject1;
            if (dout.Position != position2)
              throw new Exception("Serialization failed. Stream position corrupted. Should be " + (object) position2 + " is now: " + (string) (object) dout.Position + " serializedLength: " + (string) (object) serObject1);
          }
        }
      }
    }

    private static void SerializeByteArray(MemoryStream dout, byte[] serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 120);
      Protocol.SerializeInteger(dout, serObject.Length, false);
      dout.Write(serObject, 0, serObject.Length);
    }

    private static void SerializeIntArrayOptimized(MemoryStream inWriter, int[] serObject, bool setType)
    {
      if (setType)
        inWriter.WriteByte((byte) 121);
      Protocol.SerializeShort(inWriter, (short) serObject.Length, false);
      inWriter.WriteByte((byte) 105);
      byte[] buffer = new byte[serObject.Length * 4];
      int num1 = 0;
      for (int index1 = 0; index1 < serObject.Length; ++index1)
      {
        byte[] numArray1 = buffer;
        int index2 = num1;
        int num2 = 1;
        int num3 = index2 + num2;
        int num4 = (int) (byte) (serObject[index1] >> 24);
        numArray1[index2] = (byte) num4;
        byte[] numArray2 = buffer;
        int index3 = num3;
        int num5 = 1;
        int num6 = index3 + num5;
        int num7 = (int) (byte) (serObject[index1] >> 16);
        numArray2[index3] = (byte) num7;
        byte[] numArray3 = buffer;
        int index4 = num6;
        int num8 = 1;
        int num9 = index4 + num8;
        int num10 = (int) (byte) (serObject[index1] >> 8);
        numArray3[index4] = (byte) num10;
        byte[] numArray4 = buffer;
        int index5 = num9;
        int num11 = 1;
        num1 = index5 + num11;
        int num12 = (int) (byte) serObject[index1];
        numArray4[index5] = (byte) num12;
      }
      inWriter.Write(buffer, 0, buffer.Length);
    }

    private static void SerializeStringArray(MemoryStream dout, string[] serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 97);
      Protocol.SerializeShort(dout, (short) serObject.Length, false);
      for (int index = 0; index < serObject.Length; ++index)
        Protocol.SerializeString(dout, serObject[index], false);
    }

    private static void SerializeObjectArray(MemoryStream dout, object[] objects, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 122);
      Protocol.SerializeShort(dout, (short) objects.Length, false);
      for (int index = 0; index < objects.Length; ++index)
      {
        object serObject = objects[index];
        Protocol.Serialize(dout, serObject, true);
      }
    }

    private static void SerializeHashTable(MemoryStream dout, Hashtable serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 104);
      Protocol.SerializeShort(dout, (short) serObject.Count, false);
      foreach (DictionaryEntry dictionaryEntry in serObject)
      {
        Protocol.Serialize(dout, dictionaryEntry.Key, true);
        Protocol.Serialize(dout, dictionaryEntry.Value, true);
      }
    }

    private static void SerializeDictionary(MemoryStream dout, IDictionary serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 68);
      bool setKeyType;
      bool setValueType;
      Protocol.SerializeDictionaryHeader(dout, (object) serObject, out setKeyType, out setValueType);
      Protocol.SerializeDictionaryElements(dout, (object) serObject, setKeyType, setValueType);
    }

    private static void SerializeDictionaryHeader(MemoryStream writer, Type dictType)
    {
      bool setKeyType;
      bool setValueType;
      Protocol.SerializeDictionaryHeader(writer, (object) dictType, out setKeyType, out setValueType);
    }

    private static void SerializeDictionaryHeader(MemoryStream writer, object dict, out bool setKeyType, out bool setValueType)
    {
      Type[] genericArguments = dict.GetType().GetGenericArguments();
      setKeyType = genericArguments[0] == typeof (object);
      setValueType = genericArguments[1] == typeof (object);
      if (setKeyType)
      {
        writer.WriteByte((byte) 0);
      }
      else
      {
        GpType codeOfType = Protocol.GetCodeOfType(genericArguments[0]);
        if (codeOfType == GpType.Unknown || codeOfType == GpType.Dictionary)
          throw new Exception("Unexpected - cannot serialize Dictionary with key type: " + (object) genericArguments[0]);
        writer.WriteByte((byte) codeOfType);
      }
      if (setValueType)
      {
        writer.WriteByte((byte) 0);
      }
      else
      {
        GpType codeOfType = Protocol.GetCodeOfType(genericArguments[1]);
        if (codeOfType == GpType.Unknown)
          throw new Exception("Unexpected - cannot serialize Dictionary with value type: " + (object) genericArguments[0]);
        writer.WriteByte((byte) codeOfType);
        if (codeOfType == GpType.Dictionary)
          Protocol.SerializeDictionaryHeader(writer, genericArguments[1]);
      }
    }

    private static void SerializeDictionaryElements(MemoryStream writer, object dict, bool setKeyType, bool setValueType)
    {
      IDictionary dictionary = (IDictionary) dict;
      Protocol.SerializeShort(writer, (short) dictionary.Count, false);
      foreach (DictionaryEntry dictionaryEntry in dictionary)
      {
        if (!setValueType && dictionaryEntry.Value == null)
          throw new Exception("Can't serialize null in Dictionary with specific value-type.");
        if (!setKeyType && dictionaryEntry.Key == null)
          throw new Exception("Can't serialize null in Dictionary with specific key-type.");
        Protocol.Serialize(writer, dictionaryEntry.Key, setKeyType);
        Protocol.Serialize(writer, dictionaryEntry.Value, setValueType);
      }
    }

    private static object Deserialize(MemoryStream din, byte type)
    {
      byte num = type;
      if ((uint) num <= 42U)
      {
        if ((int) num == 0 || (int) num == 42)
          return (object) null;
      }
      else
      {
        switch (num)
        {
          case (byte) 68:
            return (object) Protocol.DeserializeDictionary(din);
          case (byte) 97:
            return (object) Protocol.DeserializeStringArray(din);
          case (byte) 98:
            return (object) Protocol.DeserializeByte(din);
          case (byte) 99:
            byte customTypeCode = (byte) din.ReadByte();
            return Protocol.DeserializeCustom(din, customTypeCode);
          case (byte) 100:
            return (object) Protocol.DeserializeDouble(din);
          case (byte) 101:
            return (object) Protocol.DeserializeEventData(din);
          case (byte) 102:
            return (object) Protocol.DeserializeFloat(din);
          case (byte) 104:
            return (object) Protocol.DeserializeHashTable(din);
          case (byte) 105:
            return (object) Protocol.DeserializeInteger(din);
          case (byte) 107:
            return (object) Protocol.DeserializeShort(din);
          case (byte) 108:
            return (object) Protocol.DeserializeLong(din);
          case (byte) 110:
            return (object) Protocol.DeserializeIntArray(din);
          case (byte) 111:
            return (object) (bool) (Protocol.DeserializeBoolean(din) ? 1 : 0);
          case (byte) 112:
            return (object) Protocol.DeserializeOperationResponse(din);
          case (byte) 113:
            return (object) Protocol.DeserializeOperationRequest(din);
          case (byte) 115:
            return (object) Protocol.DeserializeString(din);
          case (byte) 120:
            return (object) Protocol.DeserializeByteArray(din);
          case (byte) 121:
            return (object) Protocol.DeserializeArray(din);
          case (byte) 122:
            return (object) Protocol.DeserializeObjectArray(din);
        }
      }
      Debug.WriteLine("missing type: " + (object) type);
      throw new Exception("deserialize(): " + (object) type);
    }

    private static byte DeserializeByte(MemoryStream din)
    {
      return (byte) din.ReadByte();
    }

    private static bool DeserializeBoolean(MemoryStream din)
    {
      return din.ReadByte() != 0;
    }

    private static short DeserializeShort(MemoryStream din)
    {
      lock (Protocol.memShort)
      {
        byte[] local_0 = Protocol.memShort;
        din.Read(local_0, 0, 2);
        return (short) ((int) local_0[0] << 8 | (int) local_0[1]);
      }
    }

    /// <summary>
    /// Deserialize fills the given short typed value with the given byte-array (source) starting at the also given offset.
    ///             The result is placed in a variable (value). There is no need to return a value because the parameter value is given by reference.
    ///             The altered offset is this way also known to the caller.
    /// 
    /// </summary>
    /// <param name="value">The short value to deserialized into</param><param name="source">The byte-array to deserialize from</param><param name="offset">The offset in the byte-array</param>
    public static void Deserialize(out short value, byte[] source, ref int offset)
    {
      value = (short) ((int) source[offset++] << 8 | (int) source[offset++]);
    }

    /// <summary>
    /// DeserializeInteger returns an Integer typed value from the given Memorystream.
    /// 
    /// </summary>
    private static int DeserializeInteger(MemoryStream din)
    {
      lock (Protocol.memInteger)
      {
        byte[] local_0 = Protocol.memInteger;
        din.Read(local_0, 0, 4);
        return (int) local_0[0] << 24 | (int) local_0[1] << 16 | (int) local_0[2] << 8 | (int) local_0[3];
      }
    }

    /// <summary>
    /// Deserialize fills the given int typed value with the given byte-array (source) starting at the also given offset.
    ///             The result is placed in a variable (value). There is no need to return a value because the parameter value is given by reference.
    ///             The altered offset is this way also known to the caller.
    /// 
    /// </summary>
    /// <param name="value">The int value to deserialize into</param><param name="source">The byte-array to deserialize from</param><param name="offset">The offset in the byte-array</param>
    public static void Deserialize(out int value, byte[] source, ref int offset)
    {
      value = (int) source[offset++] << 24 | (int) source[offset++] << 16 | (int) source[offset++] << 8 | (int) source[offset++];
    }

    private static long DeserializeLong(MemoryStream din)
    {
      lock (Protocol.memLong)
      {
        byte[] local_0 = Protocol.memLong;
        din.Read(local_0, 0, 8);
        if (BitConverter.IsLittleEndian)
          return (long) local_0[0] << 56 | (long) local_0[1] << 48 | (long) local_0[2] << 40 | (long) local_0[3] << 32 | (long) local_0[4] << 24 | (long) local_0[5] << 16 | (long) local_0[6] << 8 | (long) local_0[7];
        return BitConverter.ToInt64(local_0, 0);
      }
    }

    private static float DeserializeFloat(MemoryStream din)
    {
      lock (Protocol.memFloat)
      {
        byte[] local_0 = Protocol.memFloat;
        din.Read(local_0, 0, 4);
        if (BitConverter.IsLittleEndian)
        {
          byte local_1 = local_0[0];
          byte local_2 = local_0[1];
          local_0[0] = local_0[3];
          local_0[1] = local_0[2];
          local_0[2] = local_2;
          local_0[3] = local_1;
        }
        return BitConverter.ToSingle(local_0, 0);
      }
    }

    /// <summary>
    /// Deserialize fills the given float typed value with the given byte-array (source) starting at the also given offset.
    ///             The result is placed in a variable (value). There is no need to return a value because the parameter value is given by reference.
    ///             The altered offset is this way also known to the caller.
    /// 
    /// </summary>
    /// <param name="value">The float value to deserialize</param><param name="source">The byte-array to deserialize from</param><param name="offset">The offset in the byte-array</param>
    public static void Deserialize(out float value, byte[] source, ref int offset)
    {
      if (BitConverter.IsLittleEndian)
      {
        lock (Protocol.memDeserialize)
        {
          byte[] local_0 = Protocol.memDeserialize;
          local_0[3] = source[offset++];
          local_0[2] = source[offset++];
          local_0[1] = source[offset++];
          local_0[0] = source[offset++];
          value = BitConverter.ToSingle(local_0, 0);
        }
      }
      else
      {
        value = BitConverter.ToSingle(source, offset);
        offset += 4;
      }
    }

    private static double DeserializeDouble(MemoryStream din)
    {
      lock (Protocol.memDouble)
      {
        byte[] local_0 = Protocol.memDouble;
        din.Read(local_0, 0, 8);
        if (BitConverter.IsLittleEndian)
        {
          byte local_1 = local_0[0];
          byte local_2 = local_0[1];
          byte local_3 = local_0[2];
          byte local_4 = local_0[3];
          local_0[0] = local_0[7];
          local_0[1] = local_0[6];
          local_0[2] = local_0[5];
          local_0[3] = local_0[4];
          local_0[4] = local_4;
          local_0[5] = local_3;
          local_0[6] = local_2;
          local_0[7] = local_1;
        }
        return BitConverter.ToDouble(local_0, 0);
      }
    }

    private static string DeserializeString(MemoryStream din)
    {
      short num = Protocol.DeserializeShort(din);
      if ((int) num == 0)
        return "";
      byte[] numArray = new byte[(int) num];
      din.Read(numArray, 0, numArray.Length);
      return Encoding.UTF8.GetString(numArray, 0, numArray.Length);
    }

    private static Array DeserializeArray(MemoryStream din)
    {
      short num1 = Protocol.DeserializeShort(din);
      byte num2 = (byte) din.ReadByte();
      Array array1;
      switch (num2)
      {
        case (byte) 121:
          Array array2 = Protocol.DeserializeArray(din);
          array1 = Array.CreateInstance(array2.GetType(), (int) num1);
          array1.SetValue((object) array2, 0);
          for (short index = (short) 1; (int) index < (int) num1; ++index)
          {
            Array array3 = Protocol.DeserializeArray(din);
            array1.SetValue((object) array3, (int) index);
          }
          break;
        case (byte) 120:
          array1 = Array.CreateInstance(typeof (byte[]), (int) num1);
          for (short index = (short) 0; (int) index < (int) num1; ++index)
          {
            Array array3 = (Array) Protocol.DeserializeByteArray(din);
            array1.SetValue((object) array3, (int) index);
          }
          break;
        case (byte) 99:
          byte key = (byte) din.ReadByte();
          CustomType customType;
          if (!Protocol.CodeDict.TryGetValue(key, out customType))
            throw new Exception("Cannot find deserializer for custom type: " + (object) key);
          array1 = Array.CreateInstance(customType.Type, (int) num1);
          for (int index = 0; index < (int) num1; ++index)
          {
            short length = Protocol.DeserializeShort(din);
            if (customType.DeserializeStreamFunction == null)
            {
              byte[] numArray = new byte[(int) length];
              din.Read(numArray, 0, (int) length);
              array1.SetValue(customType.DeserializeFunction(numArray), index);
            }
            else
              array1.SetValue(customType.DeserializeStreamFunction(din, length), index);
          }
          break;
        case (byte) 68:
          Array arrayResult = (Array) null;
          Protocol.DeserializeDictionaryArray(din, num1, out arrayResult);
          return arrayResult;
        default:
          array1 = Protocol.CreateArrayByType(num2, num1);
          for (short index = (short) 0; (int) index < (int) num1; ++index)
            array1.SetValue(Protocol.Deserialize(din, num2), (int) index);
          break;
      }
      return array1;
    }

    private static byte[] DeserializeByteArray(MemoryStream din)
    {
      int count = Protocol.DeserializeInteger(din);
      byte[] buffer = new byte[count];
      din.Read(buffer, 0, count);
      return buffer;
    }

    private static int[] DeserializeIntArray(MemoryStream din)
    {
      int length = Protocol.DeserializeInteger(din);
      int[] numArray = new int[length];
      for (int index = 0; index < length; ++index)
        numArray[index] = Protocol.DeserializeInteger(din);
      return numArray;
    }

    private static string[] DeserializeStringArray(MemoryStream din)
    {
      int length = (int) Protocol.DeserializeShort(din);
      string[] strArray = new string[length];
      for (int index = 0; index < length; ++index)
        strArray[index] = Protocol.DeserializeString(din);
      return strArray;
    }

    private static object[] DeserializeObjectArray(MemoryStream din)
    {
      short num = Protocol.DeserializeShort(din);
      object[] objArray = new object[(int) num];
      for (int index = 0; index < (int) num; ++index)
      {
        byte type = (byte) din.ReadByte();
        objArray[index] = Protocol.Deserialize(din, type);
      }
      return objArray;
    }

    private static Hashtable DeserializeHashTable(MemoryStream din)
    {
      int x = (int) Protocol.DeserializeShort(din);
      Hashtable hashtable = new Hashtable(x);
      for (int index1 = 0; index1 < x; ++index1)
      {
        object index2 = Protocol.Deserialize(din, (byte) din.ReadByte());
        object obj = Protocol.Deserialize(din, (byte) din.ReadByte());
        hashtable[index2] = obj;
      }
      return hashtable;
    }

    private static IDictionary DeserializeDictionary(MemoryStream din)
    {
      byte typeCode1 = (byte) din.ReadByte();
      byte typeCode2 = (byte) din.ReadByte();
      int num = (int) Protocol.DeserializeShort(din);
      bool flag1 = (int) typeCode1 == 0 || (int) typeCode1 == 42;
      bool flag2 = (int) typeCode2 == 0 || (int) typeCode2 == 42;
      IDictionary dictionary = Activator.CreateInstance(typeof (Dictionary<,>).MakeGenericType(Protocol.GetTypeOfCode(typeCode1), Protocol.GetTypeOfCode(typeCode2))) as IDictionary;
      for (int index = 0; index < num; ++index)
      {
        object key = Protocol.Deserialize(din, flag1 ? (byte) din.ReadByte() : typeCode1);
        object obj = Protocol.Deserialize(din, flag2 ? (byte) din.ReadByte() : typeCode2);
        dictionary.Add(key, obj);
      }
      return dictionary;
    }

    private static bool DeserializeDictionaryArray(MemoryStream din, short size, out Array arrayResult)
    {
      byte keyTypeCode;
      byte valTypeCode;
      Type type1 = Protocol.DeserializeDictionaryType(din, out keyTypeCode, out valTypeCode);
      arrayResult = Array.CreateInstance(type1, (int) size);
      for (short index1 = (short) 0; (int) index1 < (int) size; ++index1)
      {
        IDictionary dictionary = Activator.CreateInstance(type1) as IDictionary;
        if (dictionary == null)
          return false;
        short num = Protocol.DeserializeShort(din);
        for (int index2 = 0; index2 < (int) num; ++index2)
        {
          object key;
          if ((int) keyTypeCode != 0)
          {
            key = Protocol.Deserialize(din, keyTypeCode);
          }
          else
          {
            byte type2 = (byte) din.ReadByte();
            key = Protocol.Deserialize(din, type2);
          }
          object obj;
          if ((int) valTypeCode != 0)
          {
            obj = Protocol.Deserialize(din, valTypeCode);
          }
          else
          {
            byte type2 = (byte) din.ReadByte();
            obj = Protocol.Deserialize(din, type2);
          }
          dictionary.Add(key, obj);
        }
        arrayResult.SetValue((object) dictionary, (int) index1);
      }
      return true;
    }

    private static Type DeserializeDictionaryType(MemoryStream reader, out byte keyTypeCode, out byte valTypeCode)
    {
      keyTypeCode = (byte) reader.ReadByte();
      valTypeCode = (byte) reader.ReadByte();
      GpType gpType1 = (GpType) keyTypeCode;
      GpType gpType2 = (GpType) valTypeCode;
      return typeof (Dictionary<,>).MakeGenericType(gpType1 != GpType.Unknown ? Protocol.GetTypeOfCode(keyTypeCode) : typeof (object), gpType2 != GpType.Unknown ? Protocol.GetTypeOfCode(valTypeCode) : typeof (object));
    }
  }
}
