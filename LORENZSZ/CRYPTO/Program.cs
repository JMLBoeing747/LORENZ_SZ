using Cryptography;
using System;

namespace CRYPTO
{
    class Program
    {
        static string CryptoVersion { get => "1.0.0"; }
        static string UserinfoTextFile { get => "USERINFO.TXT"; }

        static int WarningMessageBegin()
        {
            Display.PrintMessage("WELCOME TO \"CRYPTO ENCRYPTOR\"!\n", MessageState.Warning);

            Display.PrintMessage("WARNING! THIS PROGRAM WILL GENERATE A CYPHER TEXT CONTAINING THE COMPUTER NAME", MessageState.Warning);
            Display.PrintMessage("AND USERNAME OF THIS COMPUTER. AT THE FIRST TIME, BE SURE THAT THESE INFOS DO NOT", MessageState.Warning);
            Display.PrintMessage("CONTAIN SPECIAL CHARACTERS TO AVOID ERRORS IN CYPHERING.\n", MessageState.Warning);

            Display.PrintMessage($"THE INFORMATIONS TO BE ENCRYPTED ARE:\n{Environment.UserName,-20} (as username)\n{Environment.MachineName,-20} (as computername)\n", MessageState.Info);

            Display.PrintMessage($"AFTER EXECUTING \"CRYPTO ENCRYPTOR\", A NEW FILE, \"{UserinfoTextFile}\", WILL BE GENERATED", MessageState.Warning);
            Display.PrintMessage("AND WILL CONTAIN THE INFOS JUST MENTIONNED PREVIOUSLY. DESPITE THESE INFOS ARE ENCRYPTED", MessageState.Warning);
            Display.PrintMessage("FOR PRIVACY AND SECURITY, WE NEED YOUR CONSENT TO PROCEED WITH THIS OPERATION.\n", MessageState.Warning);

            Display.PrintMessage("IF YOU DECIDE TO CONTINUE BY GIVING THIS FILE, YOU CONSENT TO LET THE LORENZ DISTRIBUTOR", MessageState.Warning);
            Display.PrintMessage("TO USE THESE INFORMATIONS FOR SECURITY AND INTEGRITY PURPOSES, OR, IN OTHER WORDS, TO", MessageState.Warning);
            Display.PrintMessage("GENERATE A UNIQUE PRODUCT KEY FOR YOUR ORDERED LORENZ CYPHER APP.", MessageState.Warning);
            Display.PrintMessage("THEREOF, THE LORENZ DISTRIBUTOR WILL NOT SHARE THESE INFORMATIONS WITHOUT YOUR CONSENT", MessageState.Warning);
            Display.PrintMessage($"AND WILL DESTROY YOUR \"{UserinfoTextFile}\" AND YOUR PRODUCT KEY FILE WITHIN THE 5 MINUTES", MessageState.Warning);
            Display.PrintMessage("FOLLOWING THE RECEPTION OF THE KEY.\n", MessageState.Warning);

            Display.PrintMessage("IF YOU DO NOT AGREE, PLEASE PRESS THE \"ESCAPE\" KEY.\n", MessageState.Warning);

            Display.PrintMessage("THANK YOU AND ENJOY :D", MessageState.Info);
            Console.WriteLine("Press ESCAPE to abort or any key to continue...");
            ConsoleKeyInfo saisie = Console.ReadKey(true);
            Console.Clear();
            if (saisie.Key == ConsoleKey.Escape)
                return -1;
            else
                return 0;
        }

        static void CreateMatrix(ref uint[] keyQBytes, int opCode)
        {
            int matrixLength = (int)Math.Cbrt(Common.KeyNbrUInt * 4);

            //Divide keyQBytes into bytes for keyBytes
            byte[] keyBytes = new byte[Common.KeyNbrUInt * 4];
            for (int qb = 0; qb < keyQBytes.Length; qb++)
            {
                keyBytes[4 * qb] = (byte)(keyQBytes[qb] >> 24);
                keyBytes[4 * qb + 1] = (byte)(keyQBytes[qb] >> 16);
                keyBytes[4 * qb + 2] = (byte)(keyQBytes[qb] >> 8);
                keyBytes[4 * qb + 3] = (byte)(keyQBytes[qb]);
            }

            byte[,,] theByteMatrix = new byte[matrixLength, matrixLength, matrixLength];
            int deepLength = theByteMatrix.GetLength(0);
            int lineLength = theByteMatrix.GetLength(1);
            int columnLength = theByteMatrix.GetLength(2);

            //Create matrix from byteArray
            int counter = 0;
            for (int deep = 0; deep < deepLength; deep++)
                for (int line = 0; line < lineLength; line++)
                    for (int column = 0; column < columnLength; column++)
                    {
                        theByteMatrix[deep, line, column] = keyBytes[counter];
                        counter++;
                    }

            switch (opCode)
            {
                case 1:
                    Common.RegularSwitchInto2DMatrix(ref theByteMatrix);
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix);
                    break;
                case 2:
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, false);
                    break;
                case 3:
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, onlyFirst2DMatrix: true);
                    Common.XORForeach2DMatrix(ref theByteMatrix);
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix, false);
                    Common.RegularSwitchInto2DMatrix(ref theByteMatrix);
                    break;
                default:
                    return;
            }

            counter = 0;
            for (int deep = 0; deep < deepLength; deep++)
                for (int line = 0; line < lineLength; line++)
                    for (int column = 0; column < columnLength; column++)
                    {
                        keyBytes[counter] = theByteMatrix[deep, line, column];
                        counter++;
                    }

            //Create keyQBytes from keyBytes
            for (int db = 0; db < keyQBytes.Length; db++)
            {
                keyQBytes[db] = (uint)(keyBytes[4 * db] << 24);
                keyQBytes[db] += (uint)(keyBytes[4 * db + 1] << 16);
                keyQBytes[db] += (uint)(keyBytes[4 * db + 2] << 8);
                keyQBytes[db] += (uint)(keyBytes[4 * db + 3]);
            }
        }

        static void ChiffrerLeMessage(uint[] message)
        {
            //Show warning message...
            if (WarningMessageBegin() == -1)
                return;

            Common.CphrMode = CypherMode.x1;
            uint[] keyQBytesArray = new uint[Common.KeyNbrUInt];
            uint[] cypherMessage = new uint[message.Length];
            message.CopyTo(cypherMessage, 0);

            Cryptography.Random.RandomGeneratedNumberQb(ref keyQBytesArray);
            CreateMatrix(ref keyQBytesArray, 1);
            Common.NotOperationToKey(ref keyQBytesArray);
            Common.XORPassIntoMessage(keyQBytesArray, ref cypherMessage);
            CreateMatrix(ref keyQBytesArray, 2);
            Common.NotOperationToKey(ref keyQBytesArray);
            Common.ReverseKey(ref keyQBytesArray);
            Common.XORPassIntoMessage(keyQBytesArray, ref cypherMessage);

            CreateMatrix(ref keyQBytesArray, 3);
            Encryption.ClosingCyphering(keyQBytesArray, ref cypherMessage);
            Encryption.WriteCypherIntoFile(cypherMessage, UserinfoTextFile);
        }

        public static void Main()
        {
            Console.Title = "CRYPTO ENCRYPTOR " + CryptoVersion;
            ChiffrerLeMessage(Encryption.CreateScrambledMessage(Environment.UserName, Environment.MachineName));
        }
    }
}
