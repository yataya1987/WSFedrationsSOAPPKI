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
using libaxolotl.groups.state;
using libaxolotl.state;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static libaxolotl.state.StorageProtos;

namespace libaxolotl.groups
{
    /**
     * A durable representation of a set of SenderKeyStates for a specific
     * SenderKeyName.
     *
     * @author
     */
    public class SenderKeyRecord
    {
        private static readonly int MAX_STATES = 5;

        private LinkedList<SenderKeyState> senderKeyStates = new LinkedList<SenderKeyState>();

        public SenderKeyRecord() { }

        public SenderKeyRecord(byte[] serialized)
        {
            SenderKeyRecordStructure senderKeyRecordStructure = SenderKeyRecordStructure.ParseFrom(serialized);

            foreach (StorageProtos.SenderKeyStateStructure structure in senderKeyRecordStructure.SenderKeyStatesList)
            {
                this.senderKeyStates.AddFirst(new SenderKeyState(structure));
            }
        }

        public bool isEmpty()
        {
            return senderKeyStates.Count == 0;
        }

        public SenderKeyState getSenderKeyState()
        {
            if (!isEmpty())
            {
                return senderKeyStates.First.Value;
            }
            else
            {
                throw new InvalidKeyIdException("No key state in record!");
            }
        }

        public SenderKeyState getSenderKeyState(uint keyId)
        {
            foreach (SenderKeyState state in senderKeyStates)
            {
                if (state.getKeyId() == keyId)
                {
                    return state;
                }
            }

            throw new InvalidKeyIdException("No keys for: " + keyId);
        }

        public void addSenderKeyState(uint id, uint iteration, byte[] chainKey, ECPublicKey signatureKey)
        {
            senderKeyStates.AddFirst(new SenderKeyState(id, iteration, chainKey, signatureKey));

            if (senderKeyStates.Count > MAX_STATES)
            {
                senderKeyStates.RemoveLast();
            }
        }

        public void setSenderKeyState(uint id, uint iteration, byte[] chainKey, ECKeyPair signatureKey)
        {
            senderKeyStates.Clear();
            senderKeyStates.AddFirst(new SenderKeyState(id, iteration, chainKey, signatureKey));
        }

        public byte[] serialize()
        {
            SenderKeyRecordStructure.Builder recordStructure = SenderKeyRecordStructure.CreateBuilder();

            foreach (SenderKeyState senderKeyState in senderKeyStates)
            {
                recordStructure.AddSenderKeyStates(senderKeyState.getStructure());
            }

            return recordStructure.Build().ToByteArray();
        }
    }
}
