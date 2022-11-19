using Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using TreeRegister;

namespace LORENZKeygen
{
    /// <summary>
    /// Class for key generation functions
    /// </summary>
    public static class Keygen
    {
        private static string ProductKeyFile { get => "PRDCTKEY.LKI"; }
        private static string UIDRegister { get => "UIDREG.TXT"; }

        public static void CreateMatrix(ref uint[] keyQBytes, int opCode)
        {
            int matrixLength = (int)Math.Cbrt(Common.KeyNbrUInt * 4);

            //Divide keyQBytes into bytes for keyBytes
            Common.UIntToByteArray(keyQBytes, out byte[] keyBytes);

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
                case -2:
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, true);
                    break;
                case -3:
                    Common.RegularSwitchInto2DMatrix(ref theByteMatrix, false);
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix, false);
                    Common.XORForeach2DMatrix(ref theByteMatrix, false);
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, false, true);
                    break;
                case 11:
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix);
                    Common.RegularSwitchInto2DMatrix(ref theByteMatrix, false);
                    break;
                case 12:
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix, false);
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix);
                    break;
                case 13:
                    Common.RegularSwitchInto2DMatrix(ref theByteMatrix);
                    Common.RegularSwitchInto2DMatrix(ref theByteMatrix);
                    Common.XORForeach2DMatrix(ref theByteMatrix, false);
                    break;
                case 14:
                    Common.XORForeach2DMatrix(ref theByteMatrix, false);
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, false);
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, false, true);
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix);
                    Common.XORForeach2DMatrix(ref theByteMatrix);
                    break;
                default:
                    return;
            }

            //Create keyBytes from moved matrix
            counter = 0;
            for (int deep = 0; deep < deepLength; deep++)
                for (int line = 0; line < lineLength; line++)
                    for (int column = 0; column < columnLength; column++)
                    {
                        keyBytes[counter] = theByteMatrix[deep, line, column];
                        counter++;
                    }

            //Create keyQBytes from keyBytes
            Common.ByteToUIntArray(keyBytes, out keyQBytes);
        }

        public static void GeneratingKey((string, string) userInfos)
        {
            string TheUID = GenerateUID(6);
            Common.CphrMode = CypherMode.x2;
            uint[] cypherMessage = Encryption.CreateScrambledMessage(userInfos.Item1, userInfos.Item2, TheUID);
            uint[] keyQBytesArray = new uint[Common.KeyNbrUInt];

            Cryptography.Random.RandomGeneratedNumberQb(ref keyQBytesArray);
            CreateMatrix(ref keyQBytesArray, 11);
            Common.NotOperationToKey(ref keyQBytesArray);
            Common.XORPassIntoMessage(keyQBytesArray, ref cypherMessage);

            CreateMatrix(ref keyQBytesArray, 12);
            Common.XORPassIntoMessage(keyQBytesArray, ref cypherMessage);

            Common.NotOperationToKey(ref keyQBytesArray);
            CreateMatrix(ref keyQBytesArray, 13);
            Common.ReverseKey(ref keyQBytesArray);
            Common.XORPassIntoMessage(keyQBytesArray, ref cypherMessage);

            CreateMatrix(ref keyQBytesArray, 14);
            Common.NotOperationToKey(ref keyQBytesArray);
            Common.ReverseKey(ref keyQBytesArray);
            Common.XORPassIntoMessage(keyQBytesArray, ref cypherMessage);

            Encryption.ClosingCyphering(keyQBytesArray, ref cypherMessage);
            Encryption.WriteCypherIntoFile(cypherMessage, ProductKeyFile);
        }

        private static string GenerateUID(int nbrChar)
        {
            int maxIndexRank = 3;
            
            //Reading UID Register...
            List<string> listOfUIDs = Generation.ReadTreeRegister(UIDRegister);

            string futureUID;
            while (true)
            {
                futureUID = default;
                for (int i = 0; i < nbrChar; i++)
                {
                    int rand = Cryptography.Random.RandomGeneratedNumber() / 2;
                    if (rand >= 48 && rand <= 57)
                        futureUID += Convert.ToChar(rand);
                    else if (rand >= 65 && rand <= 90)
                        futureUID += Convert.ToChar(rand);
                    else if (rand >= 97 && rand <= 122)
                        futureUID += Convert.ToChar(rand);
                    else
                        i--;
                }

                if (listOfUIDs.Contains(futureUID))
                    continue;
                else
                    break;
            }
            listOfUIDs.Add(futureUID);
            listOfUIDs.Sort();

            //Values into tree data structure before writing...
            List<string> linesToWrite = Generation.GenerateTreeRegister(listOfUIDs, maxIndexRank);

            //Writing...
            File.WriteAllLines(UIDRegister, linesToWrite, System.Text.Encoding.UTF8);
            return futureUID;
        }
    }
}