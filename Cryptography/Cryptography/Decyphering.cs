using System;
using System.IO;

namespace Cryptography
{
    /// <summary>
    /// Class that contains functions for decyphering
    /// </summary>
    public static class Decyphering
    {
        /// <summary>
        /// Reads a file containing a SJML type cyphered message.
        /// </summary>
        /// <param name="filename">The path to the file to read</param>
        /// <param name="cypheredMessage">An unsigned 32-bits integer array containing the bytes stream</param>
        /// <exception cref="CryptographyException"></exception>
        public static void ReadCypherIntoFile(string filename, out uint[] cypheredMessage)
        {
            cypheredMessage = new uint[2];
            //Reading cypher message for userinfos...
            try
            {
                using FileStream fs = new FileStream(filename, FileMode.Open);
                using BinaryReader binrd = new BinaryReader(fs);
                try
                {
                    for (int db = 0; db < fs.Length / 4; db++)
                    {
                        if (db == cypheredMessage.Length)
                            Common.ExtendTable(ref cypheredMessage);
                        cypheredMessage[db] = binrd.ReadUInt32();
                    }
                }
                catch
                {
                    binrd.Close();
                    fs.Close();
                    throw new CryptographyException("Cypher message reading failed!");
                }
            }
            catch
            {
                throw new CryptographyException("Cypher message reading failed!");
            }
        }

        /// <summary>
        /// Makes the first steps of decyphering a SJML cypher type message (the opposite operations of <c>Encryption.ClosingCyphering()</c> function).
        /// </summary>
        /// <param name="filename">The path to the file containing the cypher message to read</param>
        /// <param name="cypherKey">The 32-bit unsigned integer array containing the retrived cypher key</param>
        /// <param name="decypheredMessage">The 32-bit unsigned integer array that contains the first decyphered message without the key in</param>
        public static void OpeningDecyphering(string filename, out uint[] cypherKey, out uint[] decypheredMessage)
        {
            ReadCypherIntoFile(filename, out uint[] cypheredMessage);
            CleanUnshiftMessage(cypheredMessage, out uint[] decypheredFirst);
            GetCypherKey(decypheredFirst, out cypherKey, out decypheredFirst);
            Common.XORPassIntoMessage(cypherKey, ref decypheredFirst);
            GetChecksum(decypheredFirst, out decypheredMessage);
        }

        /// <summary>
        /// Cleans and unshifts a SJML cypher type message.
        /// </summary>
        /// <param name="tableInput">The array that contains the cyphered message to decypher</param>
        /// <param name="tableOutput">The array that contains the first decyphered message</param>
        static void CleanUnshiftMessage(uint[] tableInput, out uint[] tableOutput)
        {
            byte shift = (byte)(tableInput[0] >> 16);
            uint type = tableInput[1];

            if (type % 3 == 0)
                Common.CphrMode = CypherMode.x1;
            else if ((type - 1) % 3 == 0)
                Common.CphrMode = CypherMode.x2;
            else if ((type - 2) % 3 == 0)
                Common.CphrMode = CypherMode.x3;

            tableOutput = new uint[tableInput.Length - 2];
            try
            {
                for (int db = 0; db < tableOutput.Length; db++)
                    tableOutput[db] = tableInput[db + 2];
                uint[] messageShifted = new uint[tableOutput.Length];
                for (int db = 0; db < tableOutput.Length; db++)
                    messageShifted[(db + shift) % tableOutput.Length] = tableOutput[db];
                tableOutput = messageShifted;
            }
            catch
            {
                ExceptionCaught();
            }
        }

        /// <summary>
        /// Gets the cypher key from a SJML cyphered message type.
        /// </summary>
        /// <param name="tableInput">The cleaned and unshifted cyphered message to get the cypher key</param>
        /// <param name="cypherKey">The array containing the retrived cypher key</param>
        /// <param name="tableOutput">The array containing the cyphered message without the cypher key</param>
        static void GetCypherKey(uint[] tableInput, out uint[] cypherKey, out uint[] tableOutput)
        {
            cypherKey = new uint[Common.KeyNbrUInt];
            tableOutput = new uint[tableInput.Length - cypherKey.Length];
            try
            {
                for (int db = 0; db < cypherKey.Length; db++)
                    cypherKey[db] = tableInput[db];
                for (int db = 0; db < tableOutput.Length; db++)
                    tableOutput[db] = tableInput[db + cypherKey.Length];
            }
            catch
            {
                ExceptionCaught();
            }
        }

        /// <summary>
        /// Take out the checksum of an SJML cyphered message and calculates it.
        /// </summary>
        /// <param name="cypheredMessage">The cyphered message to take out and calculate the checksum</param>
        /// <param name="outputMessage">The cyphered message without the checksum</param>
        /// <exception cref="CryptographyException"></exception>
        public static void GetChecksum(uint[] cypheredMessage, out uint[] outputMessage)
        {
            uint calculatedChecksum = default;
            outputMessage = new uint[cypheredMessage.Length - 1];
            for (int qb = 0; qb < cypheredMessage.Length; qb++)
            {
                if (qb + 1 == cypheredMessage.Length)
                {
                    calculatedChecksum -= cypheredMessage[qb];
                    break;
                }
                outputMessage[qb] = cypheredMessage[qb];
                calculatedChecksum += cypheredMessage[qb];
            }

            if (calculatedChecksum != 0)
                ExceptionCaught();
        }

        /// <summary>
        /// Strip all the scrambled message characters and return an string array contaning the original infos.
        /// </summary>
        /// <param name="cypher"></param>
        /// <returns>An array of string that contain the original informations</returns>
        public static string[] StripOutAndSplit(uint[] cypher)
        {
            string knownStr = default;
            for (int qb = 0; qb < cypher.Length; qb++)
            {
                try
                {
                    knownStr += Convert.ToChar(cypher[qb]);
                }
                catch (OverflowException)
                {
                    continue;
                }
            }
            if (knownStr == null)
                ExceptionCaught();
            return knownStr.Split('%', StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Sort an array filled with the decyphered recognized infos with their type.
        /// </summary>
        /// <param name="partsInfo"></param>
        /// <returns>A tuple countaining <code>(username, computername, datetime, PID)</code> in this order</returns>
        public static (string, string, DateTime, string) ShortingUserInfos(string[] partsInfo)
        {
            string username = null;
            string computername = null;
            DateTime datetime = default;
            string UID = null;

            foreach (string part in partsInfo)
            {
                if (part.Contains("USER:"))
                    username = part[5..];
                else if (part.Contains("COMP:"))
                    computername = part[5..];
                else if (part.Contains("UID:"))
                    UID = part[4..];
                else
                {
                    try
                    {
                        datetime = DateTime.Parse(part);
                    }
                    catch (FormatException)
                    {
                        continue;
                    }
                }
            }

            return (username, computername, datetime, UID);
        }

        static void ExceptionCaught()
        {
            throw new CryptographyException("Decyphering failed! Cypher was corrupted.");
        }
    }
}
