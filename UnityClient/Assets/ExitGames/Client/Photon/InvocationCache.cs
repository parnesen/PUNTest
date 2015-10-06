// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.InvocationCache
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System;
using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
  internal class InvocationCache
  {
    private readonly LinkedList<InvocationCache.CachedOperation> cache = new LinkedList<InvocationCache.CachedOperation>();
    private int nextInvocationId = 1;

    public int NextInvocationId
    {
      get
      {
        return this.nextInvocationId;
      }
    }

    public int Count
    {
      get
      {
        return this.cache.Count;
      }
    }

    public void Reset()
    {
      lock (this.cache)
      {
        this.nextInvocationId = 1;
        this.cache.Clear();
      }
    }

    public void Invoke(int invocationId, Action action)
    {
      lock (this.cache)
      {
        if (invocationId < this.nextInvocationId)
          return;
        if (invocationId == this.nextInvocationId)
        {
          ++this.nextInvocationId;
          action();
          if (this.cache.Count <= 0)
            return;
          LinkedListNode<InvocationCache.CachedOperation> local_0 = this.cache.First;
          while (local_0 != null && local_0.Value.InvocationId == this.nextInvocationId)
          {
            ++this.nextInvocationId;
            local_0.Value.Action();
            local_0 = local_0.Next;
            this.cache.RemoveFirst();
          }
        }
        else
        {
          InvocationCache.CachedOperation local_1 = new InvocationCache.CachedOperation()
          {
            InvocationId = invocationId,
            Action = action
          };
          if (this.cache.Count == 0)
          {
            this.cache.AddLast(local_1);
          }
          else
          {
            for (LinkedListNode<InvocationCache.CachedOperation> local_2 = this.cache.First; local_2 != null; local_2 = local_2.Next)
            {
              if (local_2.Value.InvocationId > invocationId)
              {
                this.cache.AddBefore(local_2, local_1);
                return;
              }
            }
            this.cache.AddLast(local_1);
          }
        }
      }
    }

    private class CachedOperation
    {
      public int InvocationId { get; set; }

      public Action Action { get; set; }
    }
  }
}
