// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.Hashtable
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System.Collections;
using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// This is a substitute for the Hashtable class, missing in: Win8RT and Windows Phone. It uses a Dictionary&lt;object,object&gt; as base.
  /// 
  /// </summary>
  /// 
  /// <remarks>
  /// Please be aware that this class might act differently than the Hashtable equivalent.
  ///             As far as Photon is concerned, the substitution is sufficiently precise.
  /// 
  /// </remarks>
  public class Hashtable : Dictionary<object, object>
  {
    private DictionaryEntryEnumerator enumerator;

    public new object this[object key]
    {
      get
      {
        object obj = (object) null;
        this.TryGetValue(key, out obj);
        return obj;
      }
      set
      {
        this[key] = value;
      }
    }

    public Hashtable()
    {
    }

    public Hashtable(int x)
      : base(x)
    {
    }

    public IEnumerator<DictionaryEntry> GetEnumerator()
    {
      return (IEnumerator<DictionaryEntry>) new DictionaryEntryEnumerator(this.GetEnumerator());
    }

    public override string ToString()
    {
      List<string> list = new List<string>();
      foreach (object index in this.Keys)
      {
        if (index == null || this[index] == null)
          list.Add((string) index + (object) "=" + (string) this[index]);
        else
          list.Add("(" + (object) index.GetType() + ")" + (string) index + "=(" + (string) (object) this[index].GetType() + ")" + (string) this[index]);
      }
      return string.Join(", ", list.ToArray());
    }

    /// <summary>
    /// Creates a shallow copy of the Hashtable.
    /// 
    /// </summary>
    /// 
    /// <remarks>
    /// A shallow copy of a collection copies only the elements of the collection, whether they are
    ///             reference types or value types, but it does not copy the objects that the references refer
    ///             to. The references in the new collection point to the same objects that the references in
    ///             the original collection point to.
    /// 
    /// </remarks>
    /// 
    /// <returns>
    /// Shallow copy of the Hashtable.
    /// </returns>
    public object Clone()
    {
      return (object) new Dictionary<object, object>((IDictionary<object, object>) this);
    }
  }
}
