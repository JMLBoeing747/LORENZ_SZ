using System;
using System.IO;

namespace Cryptography
{
    /// <summary>
    /// Class that contains functions for cyphering
    /// </summary>
    public static class Encryption
    {
        /// <summary>
        /// Creates a full new scrambled message from the original with random characters to later be encyphered.
        /// </summary>
        /// <param name="message">The original message</param>
        /// <returns>A string containing the new scrambled message</returns>
        /// <exception cref="CryptographyException"></exception>
        public static uint[] CreateScrambledMessage(string message)
        {
            int nbrRandQBytes = new System.Random().Next(80, 120);

            uint[] randQBytes = new uint[nbrRandQBytes];
            Random.RandomGeneratedNumberQb(ref randQBytes);

            if (message.Length > 135)
            {
                throw new CryptographyException("Too long message to cypher");
            }

            uint[] messageFormated = ToUIntArray("%" + message + "%");
            int randomIndexMessage = Random.RandomGeneratedNumber(0, randQBytes.Length);

            (int, uint[])[] strToInsert = new (int, uint[])[1]
            {
                (randomIndexMessage, messageFormated)
            };
            return MultipleTableInsert(randQBytes, strToInsert);
        }

        /// <summary>
        /// Creates a full new scrambled message from environement parameters random characters to later be encyphered.
        /// </summary>
        /// <param name="username">The environement username parameter to encypher</param>
        /// <param name="computername">The environment computername to encypher</param>
        /// <param name="UID">The PID code to insert, useful when generating a product key</param>
        /// <param name="nbrMaxUIntCypher">The maximum length of the returned scrambled message array</param>
        /// <returns>An unsigned 32-bits integer array containing the new scrambled message</returns>
        public static uint[] CreateScrambledMessage(string username, string computername, string UID = null, int nbrMaxUIntCypher = default)
        {
            uint[] usernameFormated = ToUIntArray("%" + "USER:" + username + "%");
            uint[] computernameFormated = ToUIntArray("%" + "COMP:" + computername + "%");
            uint[] dateTimeFormated = ToUIntArray("%" + DateTime.UtcNow + "%");
            uint[] UIDFormated = null;
            if (UID != null)
            {
                UIDFormated = ToUIntArray("%" + "UID:" + UID + "%");
            }

            int nbrRandQBytes;
            if (nbrMaxUIntCypher == default)
            {
                nbrRandQBytes = new System.Random().Next(80, 120);
            }
            else
            {
                nbrRandQBytes = nbrMaxUIntCypher - Common.MinUIntMandatoryParamsLength - usernameFormated.Length - computernameFormated.Length - dateTimeFormated.Length - UID.Length;
            }

            uint[] randQBytes = new uint[nbrRandQBytes];
            Random.RandomGeneratedNumberQb(ref randQBytes);

            int randomIndexUsername = Random.RandomGeneratedNumber(0, randQBytes.Length);
            int randomIndexComputername = Random.RandomGeneratedNumber(0, randQBytes.Length, randomIndexUsername);
            int randomIndexDateTime = Random.RandomGeneratedNumber(0, randQBytes.Length, randomIndexUsername, randomIndexComputername);
            int randomIndexPID = -1;
            if (UID != null)
            {
                randomIndexPID = Random.RandomGeneratedNumber(0, randQBytes.Length, randomIndexUsername, randomIndexComputername, randomIndexDateTime);
            }

            (int, uint[])[] strToInsert;
            if (randomIndexPID == -1)
            {
                strToInsert = new (int, uint[])[3]
                {
                (randomIndexUsername, usernameFormated),
                (randomIndexComputername, computernameFormated),
                (randomIndexDateTime, dateTimeFormated)
                };
            }
            else
            {
                strToInsert = new (int, uint[])[4]
                {
                (randomIndexUsername, usernameFormated),
                (randomIndexComputername, computernameFormated),
                (randomIndexDateTime, dateTimeFormated),
                (randomIndexPID, UIDFormated)
                };
            }


            return MultipleTableInsert(randQBytes, strToInsert);
        }

        /// <summary>
        /// Converts a string to its equivalent representation in unsigned 32-bits integer array.
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <returns></returns>
        public static uint[] ToUIntArray(string s)
        {
            uint[] tableOfUInt = new uint[s.Length];
            for (int ch = 0; ch < s.Length; ch++)
            {
                tableOfUInt[ch] = Convert.ToUInt32(s[ch]);
            }

            return tableOfUInt;
        }

        /// <summary>
        /// Inserts multiples unsigned 32-bit integer arrays from an input to form a single one array.
        /// </summary>
        /// <param name="tableInput">The base unsigned 32-bit integer array</param>
        /// <param name="tablesAndIndexToInsert">The arrays to insert into the base array, defined by the <c>tableInput</c> parameter</param>
        /// <returns></returns>
        public static uint[] MultipleTableInsert(uint[] tableInput, params (int, uint[])[] tablesAndIndexToInsert)
        {
            uint[] tableOfIndex(int index)
            {
                foreach ((int, uint[]) item in tablesAndIndexToInsert)
                {
                    if (index == item.Item1)
                    {
                        return item.Item2;
                    }
                }

                return null;
            }

            uint[] uIntTableBuffer = new uint[1];
            int shift = 0;
            for (int qb = 0; qb < tableInput.Length; qb++)
            {
                uint[] tableFromIndex = tableOfIndex(qb);
                if (tableFromIndex != null)
                {
                    for (int qb2 = 0; qb2 < tableFromIndex.Length; qb2++)
                    {
                        if (qb + shift + qb2 == uIntTableBuffer.Length)
                        {
                            Common.ExtendTable(ref uIntTableBuffer);
                        }

                        uIntTableBuffer[qb + shift + qb2] = tableFromIndex[qb2];
                    }
                    shift += tableFromIndex.Length;
                }
                if (qb + shift == uIntTableBuffer.Length)
                {
                    Common.ExtendTable(ref uIntTableBuffer);
                }

                uIntTableBuffer[qb + shift] = tableInput[qb];
            }
            return uIntTableBuffer;
        }

        /// <summary>
        /// Makes the last steps of cyphering a SJML cypher type message.
        /// </summary>
        /// <param name="keyQBytes">The 32-bit unsigned integer array containing the cypher key</param>
        /// <param name="messageQBytes">The 32-bit unsigned integer array that contains the cyphered message to close</param>
        public static void ClosingCyphering(uint[] keyQBytes, ref uint[] messageQBytes)
        {
            GenerateCheckSum(ref messageQBytes);
            Common.XORPassIntoMessage(keyQBytes, ref messageQBytes);
            FinalizeCypherMessage(keyQBytes, ref messageQBytes);
        }

        /// <summary>
        /// Generates the checksum for the cyphered message and includes it in.
        /// </summary>
        /// <param name="messageQBytes">The cyphered message to calculate the checksum and to insert it in</param>
        public static void GenerateCheckSum(ref uint[] messageQBytes)
        {
            uint checkSum = default;
            foreach (uint qb in messageQBytes)
            {
                checkSum += qb;
            }

            uint[] bufferMessage = new uint[messageQBytes.Length + 1];
            for (int i = 0; i < messageQBytes.Length; i++)
            {
                bufferMessage[i] = messageQBytes[i];
            }

            bufferMessage[messageQBytes.Length] = checkSum;
            messageQBytes = bufferMessage;
        }

        /// <summary>
        /// Generates a full valid cyphered message with shift, checksum and type number intergrated.
        /// </summary>
        /// <param name="keyQBytes">The unsigned 32-bit integer array that represent the cypher key used to cypher the message</param>
        /// <param name="messageQBytes">The cyphered message to finalize</param>
        public static void FinalizeCypherMessage(uint[] keyQBytes, ref uint[] messageQBytes)
        {
            uint[] bufferMessage = new uint[keyQBytes.Length + messageQBytes.Length];
            for (int qb = 0; qb < keyQBytes.Length; qb++)
            {
                bufferMessage[qb] = keyQBytes[qb];
            }

            for (int qb = keyQBytes.Length; qb < bufferMessage.Length; qb++)
            {
                bufferMessage[qb] = messageQBytes[qb - keyQBytes.Length];
            }

            messageQBytes = bufferMessage;

            byte shift = Random.RandomGeneratedNumber(max: (int)(0.75 * messageQBytes.Length));
            uint type = default;
            switch (Common.CphrMode)
            {
                case CypherMode.x1:
                    type = (uint)(Random.RandomGeneratedNumberQb(1) % (Random.__MAX_UINT_VALUE__ / 3) * 3);
                    break;
                case CypherMode.x2:
                    type = (uint)(Random.RandomGeneratedNumberQb(1) % (Random.__MAX_UINT_VALUE__ / 3) * 3 + 1);
                    break;
                case CypherMode.x3:
                    type = (uint)(Random.RandomGeneratedNumberQb(1) % (Random.__MAX_UINT_VALUE__ / 3) * 3 + 2);
                    break;
                default:
                    break;
            }

            bufferMessage = new uint[messageQBytes.Length];
            for (int qb = 0; qb < messageQBytes.Length; qb++)
            {
                bufferMessage[(qb - shift + messageQBytes.Length) % messageQBytes.Length] = messageQBytes[qb];
            }

            messageQBytes = bufferMessage;
            bufferMessage = new uint[messageQBytes.Length + 2];
            bufferMessage[0] = (uint)((shift << 16) + Random.RandomGeneratedNumberDb(1));
            bufferMessage[1] = type;
            for (int qb = 2; qb < bufferMessage.Length; qb++)
            {
                bufferMessage[qb] = messageQBytes[qb - 2];
            }

            messageQBytes = bufferMessage;
        }

        /// <summary>
        /// Writes the cyphered message from an 32-bit unsigned integer array into a specified file.
        /// </summary>
        /// <param name="cypherMessage">The array containing the cyphered message to write</param>
        /// <param name="filename">The path to the file where to write the cyphered message</param>
        public static void WriteCypherIntoFile(uint[] cypherMessage, string filename)
        {
            using BinaryWriter binwr = new BinaryWriter(new FileStream(filename, FileMode.Create));
            for (int i = 0; i < cypherMessage.Length; i++)
            {
                binwr.Write(cypherMessage[i]);
            }
        }
    }
}