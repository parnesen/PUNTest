// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.DictionaryEntryEnumerator
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System;
using System.Collections;
using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
  public class DictionaryEntryEnumerator : IEnumerator<DictionaryEntry>, IDisposable, IEnumerator
  {
    private IDictionaryEnumerator enumerator;

    object IEnumerator.Current
    {
      get
      {
        return (object) (DictionaryEntry) this.enumerator.Current;
      }
    }

    public DictionaryEntry Current
    {
      get
      {
        return (DictionaryEntry) this.enumerator.Current;
      }
    }

    public object Key
    {
      get
      {
        return this.enumerator.Key;
      }
    }

    public object Value
    {
      get
      {
        return this.enumerator.Value;
      }
    }

    public DictionaryEntry Entry
    {
      get
      {
        return this.enumerator.Entry;
      }
    }

    public DictionaryEntryEnumerator(IDictionaryEnumerator original)
    {
      this.enumerator = original;
    }

    public bool MoveNext()
    {
      return this.enumerator.MoveNext();
    }

    public void Reset()
    {
      this.enumerator.Reset();
    }

    public void Dispose()
    {
      this.enumerator = (IDictionaryEnumerator) null;
    }
  }
}
