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

using libaxolotl.ecc;
using libaxolotl.state;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace libaxolotl.util
{
    /**
     * Helper class for generating keys of different types.
     *
     * @author Moxie Marlinspike
     */
    public class KeyHelper
    {

        private KeyHelper() { }

        /**
         * Generate an identity key pair.  Clients should only do this once,
         * at install time.
         *
         * @return the generated IdentityKeyPair.
         */
        public static IdentityKeyPair generateIdentityKeyPair()
        {
            ECKeyPair keyPair = Curve.generateKeyPair();
            IdentityKey publicKey = new IdentityKey(keyPair.getPublicKey());
            return new IdentityKeyPair(publicKey, keyPair.getPrivateKey());
        }

        /**
         * Generate a registration ID.  Clients should only do this once,
         * at install time.
         *
         * @param extendedRange By default (false), the generated registration
         *                      ID is sized to require the minimal possible protobuf
         *                      encoding overhead. Specify true if the caller needs
         *                      the full range of MAX_INT at the cost of slightly
         *                      higher encoding overhead.
         * @return the generated registration ID.
         */
        public static uint generateRegistrationId(bool extendedRange)
        {
            //try
            //{
                //SecureRandom secureRandom = SecureRandom.getInstance("SHA1PRNG");
                if (extendedRange) return getRandomSequence(uint.MaxValue - 1) + 1;
                else return getRandomSequence(16380) + 1;
            /*}
            catch (NoSuchAlgorithmException e)
            {
                throw new AssertionError(e);
            }*/
        }

        public static uint getRandomSequence(uint maxValue, uint minValue= 0)
        {            
            RandomNumberGenerator rng = new RNGCryptoServiceProvider();
            byte[] _uint32Buffer = new byte[4];

            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException("minValue");

            if (minValue == maxValue) return minValue;
             Int64 diff = maxValue - minValue;

            while (true)
            {
                rng.GetBytes(_uint32Buffer);
                UInt32 rand = BitConverter.ToUInt32(_uint32Buffer, 0);

                Int64 max = (1 + (Int64)UInt32.MaxValue);
                Int64 remainder = max % diff;
                if (rand < max - remainder)
                {
                    return (UInt32)(minValue + (rand % diff));
                }
            }
        }

        /**
         * Generate a list of PreKeys.  Clients should do this at install time, and
         * subsequently any time the list of PreKeys stored on the server runs low.
         * <p>
         * PreKey IDs are shorts, so they will eventually be repeated.  Clients should
         * store PreKeys in a circular buffer, so that they are repeated as infrequently
         * as possible.
         *
         * @param start The starting PreKey ID, inclusive.
         * @param count The number of PreKeys to generate.
         * @return the list of generated PreKeyRecords.
         */
        public static IList<PreKeyRecord> generatePreKeys(uint start, uint count)
        {
            IList<PreKeyRecord> results = new List<PreKeyRecord>();

            start--;

            for (uint i = 0; i < count; i++)
            {
                results.Add(new PreKeyRecord(((start + i) % (Medium.MAX_VALUE - 1)) + 1, Curve.generateKeyPair()));
            }

            return results;
        }

        /**
         * Generate the last resort PreKey.  Clients should do this only once, at install
         * time, and durably store it for the length of the install.
         *
         * @return the generated last resort PreKeyRecord.
         */
        public static PreKeyRecord generateLastResortPreKey()
        {
            ECKeyPair keyPair = Curve.generateKeyPair();
            return new PreKeyRecord(Medium.MAX_VALUE, keyPair);
        }

        /**
         * Generate a signed PreKey
         *
         * @param identityKeyPair The local client's identity key pair.
         * @param signedPreKeyId The PreKey id to assign the generated signed PreKey
         *
         * @return the generated signed PreKey
         * @throws InvalidKeyException when the provided identity key is invalid
         */
        public static SignedPreKeyRecord generateSignedPreKey(IdentityKeyPair identityKeyPair, uint signedPreKeyId)
        {
            ECKeyPair keyPair = Curve.generateKeyPair();
            byte[] signature = Curve.calculateSignature(identityKeyPair.getPrivateKey(), keyPair.getPublicKey().serialize());

            return new SignedPreKeyRecord(signedPreKeyId, getTime(), keyPair, signature);
        }


        public static ECKeyPair generateSenderSigningKey()
        {
            return Curve.generateKeyPair();
        }

        public static byte[] generateSenderKey()
        {
            RandomNumberGenerator rng = new RNGCryptoServiceProvider();
            byte[] key = new byte[32];
            rng.GetBytes(key);
            return key;

        }

        public static uint generateSenderKeyId()
        {
            RandomNumberGenerator rng = new RNGCryptoServiceProvider();
            byte[] random = new byte[4];
            rng.GetBytes(random);
            return BitConverter.ToUInt32(random, 0);
        }

        public static ulong getTime()
        {
            return (ulong)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}
