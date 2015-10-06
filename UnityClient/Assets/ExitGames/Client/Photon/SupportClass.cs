// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.SupportClass
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// Contains several (more or less) useful static methods, mostly used for debugging.
  /// 
  /// </summary>
  public class SupportClass
  {
    protected internal static SupportClass.IntegerMillisecondsDelegate IntegerMilliseconds = (SupportClass.IntegerMillisecondsDelegate) (() => Environment.TickCount);

    public static uint CalculateCrc(byte[] buffer, int length)
    {
      uint num1 = uint.MaxValue;
      uint num2 = 3988292384U;
      for (int index1 = 0; index1 < length; ++index1)
      {
        byte num3 = buffer[index1];
        num1 ^= (uint) num3;
        for (int index2 = 0; index2 < 8; ++index2)
        {
          if (((int) num1 & 1) != 0)
            num1 = num1 >> 1 ^ num2;
          else
            num1 >>= 1;
        }
      }
      return num1;
    }

    public static List<MethodInfo> GetMethods(Type type, Type attribute)
    {
      List<MethodInfo> list = new List<MethodInfo>();
      if (type == null)
        return list;
      foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
      {
        if (attribute == null || methodInfo.IsDefined(attribute, false))
          list.Add(methodInfo);
      }
      return list;
    }

    /// <summary>
    /// Gets the local machine's "milliseconds since start" value (precision is described in remarks).
    /// 
    /// </summary>
    /// 
    /// <remarks>
    /// This method uses Environment.TickCount (cheap but with only 16ms precision).
    ///             PhotonPeer.LocalMsTimestampDelegate is available to set the delegate (unless already Connected).
    /// 
    /// </remarks>
    /// 
    /// <returns>
    /// Fraction of the current time in Milliseconds (this is not a proper datetime timestamp).
    /// </returns>
    public static int GetTickCount()
    {
      return SupportClass.IntegerMilliseconds();
    }

    /// <summary>
    /// Creates a background thread that calls the passed function in 100ms intervals, as long as that returns true.
    /// 
    /// </summary>
    /// <param name="myThread"/>
    public static void CallInBackground(Func<bool> myThread)
    {
      SupportClass.CallInBackground(myThread, 100);
    }

    /// <summary>
    /// Creates a background thread that calls the passed function in 100ms intervals, as long as that returns true.
    /// 
    /// </summary>
    /// <param name="myThread"/><param name="millisecondsInterval">Milliseconds to sleep between calls of myThread.</param>
    public static void CallInBackground(Func<bool> myThread, int millisecondsInterval)
    {
      new Thread((ThreadStart) (() =>
      {
        while (myThread())
          Thread.Sleep(millisecondsInterval);
      }))
      {
        IsBackground = true
      }.Start();
    }

    /// <summary>
    /// Writes the exception's stack trace to the received stream.
    /// 
    /// </summary>
    /// <param name="throwable">Exception to obtain information from.</param><param name="stream">Output sream used to write to.</param>
    public static void WriteStackTrace(Exception throwable, TextWriter stream)
    {
      if (stream != null)
      {
        stream.WriteLine(throwable.ToString());
        stream.WriteLine(throwable.StackTrace);
        stream.Flush();
      }
      else
      {
        Debug.WriteLine(throwable.ToString());
        Debug.WriteLine(throwable.StackTrace);
      }
    }

    /// <summary>
    /// Writes the exception's stack trace to the received stream. Writes to: System.Diagnostics.Debug.
    /// 
    /// </summary>
    /// <param name="throwable">Exception to obtain information from.</param>
    public static void WriteStackTrace(Exception throwable)
    {
      SupportClass.WriteStackTrace(throwable, (TextWriter) null);
    }

    /// <summary>
    /// This method returns a string, representing the content of the given IDictionary.
    ///             Returns "null" if parameter is null.
    /// 
    /// </summary>
    /// <param name="dictionary">IDictionary to return as string.
    ///             </param>
    /// <returns>
    /// The string representation of keys and values in IDictionary.
    /// 
    /// </returns>
    public static string DictionaryToString(IDictionary dictionary)
    {
      return SupportClass.DictionaryToString(dictionary, true);
    }

    /// <summary>
    /// This method returns a string, representing the content of the given IDictionary.
    ///             Returns "null" if parameter is null.
    /// 
    /// </summary>
    /// <param name="dictionary">IDictionary to return as string.</param><param name="includeTypes"/>
    public static string DictionaryToString(IDictionary dictionary, bool includeTypes)
    {
      if (dictionary == null)
        return "null";
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("{");
      foreach (object index in (IEnumerable) dictionary.Keys)
      {
        if (stringBuilder.Length > 1)
          stringBuilder.Append(", ");
        Type type;
        string str;
        if (dictionary[index] == null)
        {
          type = typeof (object);
          str = "null";
        }
        else
        {
          type = dictionary[index].GetType();
          str = dictionary[index].ToString();
        }
        if (typeof (IDictionary) == type || typeof (Hashtable) == type)
          str = SupportClass.DictionaryToString((IDictionary) dictionary[index]);
        if (typeof (string[]) == type)
          str = string.Format("{{{0}}}", (object) string.Join(",", (string[]) dictionary[index]));
        if (includeTypes)
          stringBuilder.AppendFormat("({0}){1}=({2}){3}", (object) index.GetType().Name, index, (object) type.Name, (object) str);
        else
          stringBuilder.AppendFormat("{0}={1}", index, (object) str);
      }
      stringBuilder.Append("}");
      return stringBuilder.ToString();
    }

    [Obsolete("Use DictionaryToString() instead.")]
    public static string HashtableToString(Hashtable hash)
    {
      return SupportClass.DictionaryToString((IDictionary) hash);
    }

    /// <summary>
    /// Inserts the number's value into the byte array, using Big-Endian order (a.k.a. Network-byte-order).
    /// 
    /// </summary>
    /// <param name="buffer">Byte array to write into.</param><param name="index">Index of first position to write to.</param><param name="number">Number to write.</param>
    [Obsolete("Use Protocol.Serialize() instead.")]
    public static void NumberToByteArray(byte[] buffer, int index, short number)
    {
      Protocol.Serialize(number, buffer, ref index);
    }

    /// <summary>
    /// Inserts the number's value into the byte array, using Big-Endian order (a.k.a. Network-byte-order).
    /// 
    /// </summary>
    /// <param name="buffer">Byte array to write into.</param><param name="index">Index of first position to write to.</param><param name="number">Number to write.</param>
    [Obsolete("Use Protocol.Serialize() instead.")]
    public static void NumberToByteArray(byte[] buffer, int index, int number)
    {
      Protocol.Serialize(number, buffer, ref index);
    }

    /// <summary>
    /// Converts a byte-array to string (useful as debugging output).
    ///             Uses BitConverter.ToString(list) internally after a null-check of list.
    /// 
    /// </summary>
    /// <param name="list">Byte-array to convert to string.</param>
    /// <returns>
    /// List of bytes as string.
    /// 
    /// </returns>
    public static string ByteArrayToString(byte[] list)
    {
      if (list == null)
        return string.Empty;
      return BitConverter.ToString(list);
    }

    public delegate int IntegerMillisecondsDelegate();

    /// <summary>
    /// Class to wrap static access to the random.Next() call in a thread safe manner.
    /// 
    /// </summary>
    public class ThreadSafeRandom
    {
      private static readonly Random _r = new Random();

      public static int Next()
      {
        lock (SupportClass.ThreadSafeRandom._r)
          return SupportClass.ThreadSafeRandom._r.Next();
      }
    }
  }
}
