// Decompiled with JetBrains decompiler
// Type: Photon.SocketServer.Security.DiffieHellmanCryptoProvider
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using Photon.SocketServer.Numeric;
using System;
using System.Security.Cryptography;

namespace Photon.SocketServer.Security
{
  internal class DiffieHellmanCryptoProvider : IDisposable
  {
    private static readonly BigInteger primeRoot = new BigInteger((long) OakleyGroups.Generator);
    private readonly BigInteger prime;
    private readonly BigInteger secret;
    private readonly BigInteger publicKey;
    private Rijndael crypto;
    private byte[] sharedKey;

    public bool IsInitialized
    {
      get
      {
        return this.crypto != null;
      }
    }

    /// <summary>
    /// Gets the public key that can be used by another DiffieHellmanCryptoProvider object
    ///             to generate a shared secret agreement.
    /// 
    /// </summary>
    public byte[] PublicKey
    {
      get
      {
        return this.publicKey.GetBytes();
      }
    }

    /// <summary>
    /// Gets the shared key that is used by the current instance for cryptographic operations.
    /// 
    /// </summary>
    public byte[] SharedKey
    {
      get
      {
        return this.sharedKey;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Security.DiffieHellmanCryptoProvider"/> class.
    /// 
    /// </summary>
    public DiffieHellmanCryptoProvider()
    {
      this.prime = new BigInteger(OakleyGroups.OakleyPrime768);
      this.secret = this.GenerateRandomSecret(160);
      this.publicKey = this.CalculatePublicKey();
    }

    /// <summary>
    /// Derives the shared key is generated from the secret agreement between two parties,
    ///             given a byte array that contains the second party's public key.
    /// 
    /// </summary>
    /// <param name="otherPartyPublicKey">The second party's public key.
    ///             </param>
    public void DeriveSharedKey(byte[] otherPartyPublicKey)
    {
      this.sharedKey = this.CalculateSharedKey(new BigInteger(otherPartyPublicKey)).GetBytes();
      byte[] hash;
      using (SHA256 shA256 = (SHA256) new SHA256Managed())
        hash = shA256.ComputeHash(this.SharedKey);
      this.crypto = (Rijndael) new RijndaelManaged();
      this.crypto.Key = hash;
      this.crypto.IV = new byte[16];
      this.crypto.Padding = PaddingMode.PKCS7;
    }

    public byte[] Encrypt(byte[] data)
    {
      return this.Encrypt(data, 0, data.Length);
    }

    public byte[] Encrypt(byte[] data, int offset, int count)
    {
      using (ICryptoTransform encryptor = this.crypto.CreateEncryptor())
        return encryptor.TransformFinalBlock(data, offset, count);
    }

    public byte[] Decrypt(byte[] data)
    {
      return this.Decrypt(data, 0, data.Length);
    }

    public byte[] Decrypt(byte[] data, int offset, int count)
    {
      using (ICryptoTransform decryptor = this.crypto.CreateDecryptor())
        return decryptor.TransformFinalBlock(data, offset, count);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected void Dispose(bool disposing)
    {
      if (!disposing)
        ;
    }

    private BigInteger CalculatePublicKey()
    {
      return DiffieHellmanCryptoProvider.primeRoot.ModPow(this.secret, this.prime);
    }

    private BigInteger CalculateSharedKey(BigInteger otherPartyPublicKey)
    {
      return otherPartyPublicKey.ModPow(this.secret, this.prime);
    }

    private BigInteger GenerateRandomSecret(int secretLength)
    {
      BigInteger bigInteger;
      do
      {
        bigInteger = BigInteger.GenerateRandom(secretLength);
      }
      while (bigInteger >= this.prime - (BigInteger) 1 || bigInteger == (BigInteger) 0);
      return bigInteger;
    }
  }
}
