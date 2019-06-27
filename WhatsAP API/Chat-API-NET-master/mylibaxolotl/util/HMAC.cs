﻿/** 
 * Copyright (C) 2015 smndtrl
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

// http://stackoverflow.com/questions/12185122/calculating-hmacsha256-using-c-sharp-to-match-payment-provider-example

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace libaxolotl.util
{
    public class Sign
    {
        public static byte[] sha256sum(byte[] key, byte[] message)
        {
            try {
                using (var hmac = new HMACSHA256(key))
                {
                    var result = hmac.ComputeHash(message);
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Assertion Error", e);
            }
        }
    }

    public class Sha256
    {
        public static byte[] Sign(byte[] key, byte[] message)
        {
            try{
                using (var hmac = new HMACSHA256(key))
                {
                    var result = hmac.ComputeHash(message);
                    return result;
                }
            }
            catch (Exception e){
                throw new InvalidOperationException("Assertion Error", e);
            }
        }

        public static bool Verify(byte[] key, byte[] message, byte[] signature)
        {
            bool err = false;
            // Initialize the keyed hash object. 
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                byte[] computedHash = hmac.ComputeHash(message);

                for (int i = 0; i < signature.Length; i++){
                    if (computedHash[i] != signature[i]){
                        err = true;
                    }
                }
            }
            if (err){
                Console.WriteLine("Hash values differ! Signed file has been tampered with!");
                return false;
            }
            else{
                Console.WriteLine("Hash values agree -- no tampering occurred.");
                return true;
            }

        }

    }

    public class Encrypt
    {
        public static byte[] aesCbcPkcs5(byte[] message, byte[] key, byte[] iv)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (AesManaged cryptor = new AesManaged())
                {
                    cryptor.Mode = CipherMode.CBC;
                    cryptor.Padding = PaddingMode.PKCS7;
                    cryptor.KeySize = 128;
                    cryptor.BlockSize = 128;

                    using (CryptoStream cs = new CryptoStream(ms, cryptor.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                    {
                        cs.Write(message, 0, message.Length);
                    }
                    byte[] encryptedContent = ms.ToArray();

                    //Create new byte array that should contain both unencrypted iv and encrypted data
                    byte[] result = new byte[iv.Length + encryptedContent.Length];

                    //copy our 2 array into one
                    System.Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                    System.Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

                    return result;
                }
            }
        }
        public static byte[] aesCtr(byte[] message, byte[] key, uint counter)
        {
            byte[] iv = new byte[16];
            ByteUtil.intToByteArray(iv, 0, (int)counter);

            using (MemoryStream ms = new MemoryStream())
            {
                using (AesManaged cryptor = new AesManaged())
                {
                    cryptor.Mode = CipherMode.CBC;
                    cryptor.Padding = PaddingMode.PKCS7;
                    cryptor.KeySize = 128;
                    cryptor.BlockSize = 128;

                    using (CryptoStream cs = new CryptoStream(ms, cryptor.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                    {
                        cs.Write(message, 0, message.Length);
                    }
                    byte[] encryptedContent = ms.ToArray();

                    //Create new byte array that should contain both unencrypted iv and encrypted data
                    byte[] result = new byte[iv.Length + encryptedContent.Length];

                    //copy our 2 array into one
                    System.Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                    System.Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

                    return result;
                }
            }
        }
    }

    public class Decrypt
    {
        public static byte[] aesCbcPkcs5(byte[] encryptedContent, byte[] key, byte[] iv)
        {
        /*    byte[] iv = new byte[16]; //initial vector is 16 bytes
            byte[] encryptedContent = new byte[secret.Length - 16]; //the rest should be encryptedcontent

            //Copy data to byte array
            System.Buffer.BlockCopy(secret, 0, iv, 0, iv.Length);
            System.Buffer.BlockCopy(secret, iv.Length, encryptedContent, 0, encryptedContent.Length);
          */

            using (MemoryStream ms = new MemoryStream())
            {
                using (AesManaged cryptor = new AesManaged())
                {
                    cryptor.Mode = CipherMode.CBC;
                    cryptor.Padding = PaddingMode.PKCS7;
                    cryptor.KeySize = 128;
                    cryptor.BlockSize = 128;

                    using (CryptoStream cs = new CryptoStream(ms, cryptor.CreateDecryptor(key, iv), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedContent, 0, encryptedContent.Length);

                    }
                    return ms.ToArray();
                }
            }
        }
        public static byte[] aesCtr(byte[] encryptedContent, byte[] key, uint counter)
        {
            byte[] iv = new byte[16];
            ByteUtil.intToByteArray(iv, 0, (int)counter);

            using (MemoryStream ms = new MemoryStream())
            {
                using (AesManaged cryptor = new AesManaged())
                {
                    cryptor.Mode = CipherMode.CBC;
                    cryptor.Padding = PaddingMode.PKCS7;
                    cryptor.KeySize = 128;
                    cryptor.BlockSize = 128;

                    using (CryptoStream cs = new CryptoStream(ms, cryptor.CreateDecryptor(key, iv), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedContent, 0, encryptedContent.Length);

                    }
                    return ms.ToArray();
                }
            }
        }

    }

    public static class CryptoHelper
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
