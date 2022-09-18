using System;

namespace Cryptography
{
    /// <summary>
    /// Specifies the supported SJML cyphering mode.
    /// </summary>
    public enum CypherMode
    {
        /// <summary>
        /// 512-bits SJML cyphermode
        /// </summary>
        x1,
        /// <summary>
        /// 1728-bits SJML cyphermode
        /// </summary>
        x2,
        /// <summary>
        /// 4096-bits SJML cyphermode
        /// </summary>
        x3
    }

    /// <summary>
    /// Class that contains common functions and properties for encryption and decryption
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// The SJML cyphermode to use
        /// </summary>
        /// <remarks>Only useful when using Encryption functions</remarks>
        public static CypherMode CphrMode { get; set; }
        /// <summary>
        /// The number of bits the cypher key will contain
        /// </summary>
        public static int KeyNbrBits
        {
            get
            {
                return CphrMode switch
                {
                    CypherMode.x1 => 512,
                    CypherMode.x2 => 1728,
                    CypherMode.x3 => 4096,
                    _ => 0,
                };
            }
        }
        /// <summary>
        /// The length of the unsigned 32-bits integer array corresponding to the cypher key values
        /// </summary>
        public static int KeyNbrUInt
        {
            get
            {
                double matrixLength = Math.Cbrt(KeyNbrBits / 32 * 4);
                return matrixLength == (int)matrixLength ? KeyNbrBits / 32 : throw new CryptographyException("INVALID BIT COUNT");
            }
        }
        /// <summary>
        /// The minimum length of an unsigned 32-bit array representing the mandatory elements of a SJML cypher (the key length + 3)
        /// </summary>
        public static int MinUIntMandatoryParamsLength { get => KeyNbrUInt + 3; }

        /// <summary>
        /// Makes an XOR operation through the cyphered message with the cypher key.
        /// </summary>
        /// <param name="keyQBytes">The cypher key</param>
        /// <param name="messageQBytes">The cyphered message</param>
        public static void XORPassIntoMessage(uint[] keyQBytes, ref uint[] messageQBytes)
        {
            for (int i = 0; i < messageQBytes.Length; i++)
            {
                for (int j = 0; j < keyQBytes.Length; j++)
                {
                    if (i >= messageQBytes.Length)
                        break;
                    messageQBytes[i] = messageQBytes[i] ^ keyQBytes[j];
                    if (j + 1 < keyQBytes.Length)
                        i++;
                }
            }
        }

        /// <summary>
        /// Converts an unsigned 32-bit integer into byte array.
        /// </summary>
        /// <param name="uintArray">The unsigned 32-bit integer array to convert</param>
        /// <param name="byteArray">The converted byte array</param>
        public static void UIntToByteArray(uint[] uintArray, out byte[] byteArray)
        {
            byteArray = new byte[uintArray.Length * 4];
            for (int qb = 0; qb < uintArray.Length; qb++)
            {
                byteArray[4 * qb] = (byte)(uintArray[qb] >> 24);
                byteArray[4 * qb + 1] = (byte)(uintArray[qb] >> 16);
                byteArray[4 * qb + 2] = (byte)(uintArray[qb] >> 8);
                byteArray[4 * qb + 3] = (byte)(uintArray[qb]);
            }
        }

        /// <summary>
        /// Converts a byte array into unsigned 32-bit integer array.
        /// </summary>
        /// <param name="byteArray">The byte array to convert</param>
        /// <param name="uintArray">The converted unsigned 32-bit integer array</param>
        public static void ByteToUIntArray(byte[] byteArray, out uint[] uintArray)
        {
            uintArray = new uint[byteArray.Length / 4];
            for (int db = 0; db < uintArray.Length; db++)
            {
                uintArray[db] = (uint)(byteArray[4 * db] << 24);
                uintArray[db] += (uint)(byteArray[4 * db + 1] << 16);
                uintArray[db] += (uint)(byteArray[4 * db + 2] << 8);
                uintArray[db] += byteArray[4 * db + 3];
            }
        }

        /// <summary>
        /// Makes NOT operation to an unsigned 32-bit integer array representing the cypher key.
        /// </summary>
        /// <param name="keyQBytes">The unsigned 32-bit integer array to perform the NOT operation in</param>
        public static void NotOperationToKey(ref uint[] keyQBytes)
        {
            for (int qb = 0; qb < keyQBytes.Length; qb++)
                keyQBytes[qb] = ~keyQBytes[qb];
        }

        /// <summary>
        /// Reverses an unsigned 32-bit integer array representing the cypher key.
        /// </summary>
        /// <param name="keyQBytes">The unsigned 32-bit integer array to reverse</param>
        public static void ReverseKey(ref uint[] keyQBytes)
        {
            uint[] bufferBytes = new uint[keyQBytes.Length];

            for (int i = 0; i < keyQBytes.Length; i++)
            {
                uint buffer = default;
                for (int bit = 0; bit < 32; bit++)
                {
                    if (keyQBytes[i] - Math.Pow(2, 32 - bit) >= 0)
                        keyQBytes[i] = (uint)(keyQBytes[i] - Math.Pow(2, 32 - bit));
                    buffer += (uint)(keyQBytes[i] >> 31 - bit << bit);
                }
                bufferBytes[keyQBytes.Length - 1 - i] = buffer;
            }
            keyQBytes = bufferBytes;
        }

        /// <summary>
        /// Performs a switch between each 2D matrix in the 3D matrix
        /// </summary>
        /// <param name="byteMatrix">The 3D matrix to perform transformation in</param>
        /// <param name="frontToBack">Specifies the operation direction. <para>If <c>true</c>, the XOR operation will go from front 2D matrix to back.</para><para>If <c>false</c>, it will go from back to front.</para></param>
        public static void RegularSwitchInto2DMatrix(ref byte[,,] byteMatrix, bool frontToBack = true)
        {
            int deepLength = byteMatrix.GetLength(0);
            int lineLength = byteMatrix.GetLength(1);
            int columnLength = byteMatrix.GetLength(2);
            byte[,] bufferMatrix2D = new byte[lineLength, columnLength];
            if (frontToBack)
                for (int d = 0; d < deepLength - 1; d++)
                {
                    for (int l = 0; l < lineLength; l++)
                        for (int c = 0; c < columnLength; c++)
                            bufferMatrix2D[l, c] = byteMatrix[d + 1, l, c];

                    for (int l = 0; l < lineLength; l++)
                        for (int c = 0; c < columnLength; c++)
                            byteMatrix[d + 1, l, c] = byteMatrix[d, l, c];

                    for (int l = 0; l < lineLength; l++)
                        for (int c = 0; c < columnLength; c++)
                            byteMatrix[d, l, c] = bufferMatrix2D[l, c];
                }
            else
                for (int d = deepLength - 1; d > 0; d--)
                {
                    for (int l = 0; l < lineLength; l++)
                        for (int c = 0; c < columnLength; c++)
                            bufferMatrix2D[l, c] = byteMatrix[d - 1, l, c];

                    for (int l = 0; l < lineLength; l++)
                        for (int c = 0; c < columnLength; c++)
                            byteMatrix[d - 1, l, c] = byteMatrix[d, l, c];

                    for (int l = 0; l < lineLength; l++)
                        for (int c = 0; c < columnLength; c++)
                            byteMatrix[d, l, c] = bufferMatrix2D[l, c];
                }
        }

        /// <summary>
        /// Mirrors each 2D matrix in the 3D matrix.
        /// </summary>
        /// <param name="byteMatrix">The 3D matrix to perform transformation in</param>
        /// <param name="horizontalMirror">Specifies the mirror orientation. <para>If <c>true</c>, mirror will be performed horizontally.</para> <para>If <c>false</c>, mirror will be performed vertically.</para></param>
        public static void RegularMirrorInto2DMatrix(ref byte[,,] byteMatrix, bool horizontalMirror = true)
        {
            int deepLength = byteMatrix.GetLength(0);
            int lineLength = byteMatrix.GetLength(1);
            int columnLength = byteMatrix.GetLength(2);
            if (horizontalMirror)
                for (int d = 0; d < deepLength; d++)
                    for (int l = 0; l < lineLength; l++)
                    {
                        byte[] line = new byte[columnLength];
                        for (int c = 0; c < columnLength; c++)
                            line[columnLength - 1 - c] = byteMatrix[d, l, c];
                        for (int c = 0; c < columnLength; c++)
                            byteMatrix[d, l, c] = line[c];
                    }
            else
                for (int d = 0; d < deepLength; d++)
                    for (int c = 0; c < columnLength; c++)
                    {
                        byte[] column = new byte[lineLength];
                        for (int l = 0; l < lineLength; l++)
                            column[lineLength - 1 - l] = byteMatrix[d, l, c];
                        for (int l = 0; l < lineLength; l++)
                            byteMatrix[d, l, c] = column[l];
                    }
        }

        /// <summary>
        /// Rotates 90 degrees each 2D matrix in the 3D matrix.
        /// </summary>
        /// <param name="byteMatrix">The 3D matrix to perform transformation in</param>
        /// <param name="antiClockwise">Specifies whether rotation is anticlockwise or clockwise.</param>
        /// <param name="onlyFirst2DMatrix">Specifies whether the operation have to be performed on the first front 2D matrix only.</param>
        public static void Turn90Foreach2DMatrix(ref byte[,,] byteMatrix, bool antiClockwise = true, bool onlyFirst2DMatrix = false)
        {
            int deepLength = onlyFirst2DMatrix ? 1 : byteMatrix.GetLength(0);
            int lineLength = byteMatrix.GetLength(1);
            int columnLength = byteMatrix.GetLength(2);
            for (int d = 0; d < deepLength; d++)
            {
                for (int dr = d; dr < deepLength; dr++)
                {
                    for (int crownOrder = 0; crownOrder < lineLength / 2; crownOrder++)
                    {
                        int c = crownOrder;
                        int l = crownOrder;
                        byte bufferElement = byteMatrix[dr, l, c];
                        int crownElementNbr = (lineLength - (crownOrder + 1) * 2) * 4 + 4;
                        if (antiClockwise)
                            for (int crownElementIndex = 0; crownElementIndex < crownElementNbr - 1; crownElementIndex++)
                            {
                                if (c + 1 < columnLength - crownOrder && l == crownOrder)
                                {
                                    byteMatrix[dr, l, c] = byteMatrix[dr, l, c + 1];
                                    c++;
                                }
                                else if (l + 1 < lineLength - crownOrder && c == columnLength - crownOrder - 1)
                                {
                                    byteMatrix[dr, l, c] = byteMatrix[dr, l + 1, c];
                                    l++;
                                }
                                else if (c - 1 >= crownOrder && l == lineLength - crownOrder - 1)
                                {
                                    byteMatrix[dr, l, c] = byteMatrix[dr, l, c - 1];
                                    c--;
                                }
                                else if (l - 1 >= crownOrder && c == crownOrder)
                                {
                                    byteMatrix[dr, l, c] = byteMatrix[dr, l - 1, c];
                                    l--;
                                }
                            }
                        else
                            for (int crownElementIndex = 0; crownElementIndex < crownElementNbr - 1; crownElementIndex++)
                            {
                                if (l + 1 < lineLength - crownOrder && c == crownOrder)
                                {
                                    byteMatrix[dr, l, c] = byteMatrix[dr, l + 1, c];
                                    l++;
                                }
                                else if (c + 1 < columnLength - crownOrder && l == lineLength - crownOrder - 1)
                                {
                                    byteMatrix[dr, l, c] = byteMatrix[dr, l, c + 1];
                                    c++;
                                }
                                else if (l - 1 >= crownOrder && c == columnLength - crownOrder - 1)
                                {
                                    byteMatrix[dr, l, c] = byteMatrix[dr, l - 1, c];
                                    l--;
                                }
                                else if (c - 1 >= crownOrder && l == crownOrder)
                                {
                                    byteMatrix[dr, l, c] = byteMatrix[dr, l, c - 1];
                                    c--;
                                }
                            }
                        byteMatrix[dr, l, c] = bufferElement;
                    }
                }
            }
        }

        /// <summary>
        /// Makes an XOR operation for each 2D matrix in the 3D matrix.
        /// </summary>
        /// <param name="byteMatrix">The 3D matrix to perform transformation in</param>
        /// <param name="frontToBack">Specifies the XOR operation direction. <para>If <c>true</c>, the XOR operation will go from front 2D matrix to back.</para><para>If <c>false</c>, it will go from back to front.</para></param>
        public static void XORForeach2DMatrix(ref byte[,,] byteMatrix, bool frontToBack = true)
        {
            int deepLength = byteMatrix.GetLength(0);
            int lineLength = byteMatrix.GetLength(1);
            int columnLength = byteMatrix.GetLength(2);
            if (frontToBack)
                for (int d = 0; d < deepLength - 1; d++)
                    for (int l = 0; l < lineLength; l++)
                        for (int c = 0; c < columnLength; c++)
                            byteMatrix[d, l, c] = (byte)(byteMatrix[d, l, c] ^ byteMatrix[d + 1, l, c]);
            else
                for (int d = deepLength - 1; d > 0; d--)
                    for (int l = 0; l < lineLength; l++)
                        for (int c = 0; c < columnLength; c++)
                            byteMatrix[d - 1, l, c] = (byte)(byteMatrix[d, l, c] ^ byteMatrix[d - 1, l, c]);
        }

        /// <summary>
        /// Extends an unsigned 32-bit integer array by adding one location
        /// </summary>
        /// <param name="table">The array to extend</param>
        public static void ExtendTable(ref uint[] table)
        {
            uint[] bufferTb = new uint[table.Length + 1];
            for (int i = 0; i < table.Length; i++)
                bufferTb[i] = table[i];
            table = bufferTb;
        }
    }
}
