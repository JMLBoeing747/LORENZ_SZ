using Cryptography;
using System;

namespace LORENZ
{
    public static class Cryptographie
    {
        public static void ChiffrerFichier(uint[] message, string filename)
        {
            Common.CphrMode = CypherMode.x3;
            uint[] keyQBytesArray = new uint[Common.KeyNbrUInt];
            uint[] cypherMessage = new uint[message.Length];
            message.CopyTo(cypherMessage, 0);

            Cryptography.Random.RandomGeneratedNumberQb(ref keyQBytesArray);
            CreateMatrix(ref keyQBytesArray, 21);
            Common.NotOperationToKey(ref keyQBytesArray);
            Common.XORPassIntoMessage(keyQBytesArray, ref cypherMessage);

            CreateMatrix(ref keyQBytesArray, 22);
            Common.NotOperationToKey(ref keyQBytesArray);
            Common.ReverseKey(ref keyQBytesArray);
            Common.XORPassIntoMessage(keyQBytesArray, ref cypherMessage);

            CreateMatrix(ref keyQBytesArray, 23);
            Common.XORPassIntoMessage(keyQBytesArray, ref cypherMessage);

            CreateMatrix(ref keyQBytesArray, 24);
            Common.ReverseKey(ref keyQBytesArray);
            Common.XORPassIntoMessage(keyQBytesArray, ref cypherMessage);
            Common.NotOperationToKey(ref keyQBytesArray);

            CreateMatrix(ref keyQBytesArray, 25);
            Encryption.ClosingCyphering(keyQBytesArray, ref cypherMessage);
            Encryption.WriteCypherIntoFile(cypherMessage, filename);
        }

        public static void DechiffrerUserinfo()
        {
            uint[] cypheredMessageOnly = DechiffrerFichier(Parametres.UserlogFile);
            Display.PrintMessage(".", MessageState.Info, false);
            //Strip out unknown characters, associate and verifying infos...
            (string, string, DateTime, string) userInfos = Decyphering.ShortingUserInfos(Decyphering.StripOutAndSplit(cypheredMessageOnly));
            Display.PrintMessage(".", MessageState.Info, false);
            if (userInfos.Item3 < DateTime.UtcNow)
            {
                Parametres.LID = userInfos.Item4;
                return;
            }
            else
                throw new LORENZException(ErrorCode.E0x12);
        }

        public static uint[] DechiffrerFichier(string filename)
        {
            Parametres.FichierEnAnalyse = filename;
            try
            {
                //Decyphering...
                Decyphering.OpeningDecyphering(filename, out uint[] keyQBytes, out uint[] decypheredMessage);
                CreateMatrix(ref keyQBytes, -25);
                Common.NotOperationToKey(ref keyQBytes);
                Common.XORPassIntoMessage(keyQBytes, ref decypheredMessage);
                Common.ReverseKey(ref keyQBytes);

                CreateMatrix(ref keyQBytes, -24);
                Common.XORPassIntoMessage(keyQBytes, ref decypheredMessage);

                CreateMatrix(ref keyQBytes, -23);
                Common.XORPassIntoMessage(keyQBytes, ref decypheredMessage);
                Common.ReverseKey(ref keyQBytes);
                Common.NotOperationToKey(ref keyQBytes);

                CreateMatrix(ref keyQBytes, -22);
                Common.XORPassIntoMessage(keyQBytes, ref decypheredMessage);
                return decypheredMessage;
            }
            catch (CryptographyException)
            {
                if (Parametres.FichierEnAnalyse == Parametres.LastAccessFile)
                    throw new LORENZException(ErrorCode.E0x11, false);
                else if (Parametres.FichierEnAnalyse == Parametres.UserlogFile)
                    throw new LORENZException(ErrorCode.E0x12, false);
                else if (Parametres.FichierEnAnalyse == Parametres.ProductKeyFile)
                    throw new LORENZException(ErrorCode.E0x20, false);
                else
                    throw new LORENZException(ErrorCode.E0xFFF, false);
            }
        }

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
                case -12:
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix);
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix, false);
                    break;
                case -13:
                    Common.XORForeach2DMatrix(ref theByteMatrix);
                    Common.RegularSwitchInto2DMatrix(ref theByteMatrix, false);
                    Common.RegularSwitchInto2DMatrix(ref theByteMatrix, false);
                    break;
                case -14:
                    Common.XORForeach2DMatrix(ref theByteMatrix, false);
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix);
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, true, true);
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, true);
                    Common.XORForeach2DMatrix(ref theByteMatrix);
                    break;
                case 21:
                    Common.XORForeach2DMatrix(ref theByteMatrix, false);
                    Common.XORForeach2DMatrix(ref theByteMatrix, false);
                    break;
                case 22:
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix);
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, onlyFirst2DMatrix: true);
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix, false);
                    Common.RegularSwitchInto2DMatrix(ref theByteMatrix);
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, false, true);
                    break;
                case -22:
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, onlyFirst2DMatrix: true);
                    Common.RegularSwitchInto2DMatrix(ref theByteMatrix, false);
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix, false);
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, false, true);
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix);
                    break;
                case 23:
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, false, true);
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, false);
                    Common.RegularSwitchInto2DMatrix(ref theByteMatrix, false);
                    break;
                case -23:
                    Common.RegularSwitchInto2DMatrix(ref theByteMatrix);
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix);
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, onlyFirst2DMatrix: true);
                    break;
                case 24:
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix, false);
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix);
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix);
                    break;
                case -24:
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, false);
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix);
                    Common.RegularMirrorInto2DMatrix(ref theByteMatrix, false);
                    break;
                case 25:
                    Common.XORForeach2DMatrix(ref theByteMatrix, false);
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix);
                    Common.XORForeach2DMatrix(ref theByteMatrix, false);
                    break;
                case -25:
                    Common.XORForeach2DMatrix(ref theByteMatrix);
                    Common.Turn90Foreach2DMatrix(ref theByteMatrix, false);
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
    }
}
