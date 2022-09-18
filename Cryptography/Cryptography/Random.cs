using System.Security.Cryptography;

namespace Cryptography
{
    /// <summary>
    /// Class for cryptographic random functions
    /// </summary>
    public static class Random
    {
        /// <summary>
        /// Represents the maximum value that an unsigned byte can have.
        /// </summary>
        public const short __MAX_UBYTE_VALUE__ = 256;
        /// <summary>
        /// Represents the maximum value that an unsigned 16-bit integer can have.
        /// </summary>
        public const int __MAX_USHORT_VALUE__ = 65536;
        /// <summary>
        /// Represents the maximum value that an unsigned 32-bit integer can have.
        /// </summary>
        public const long __MAX_UINT_VALUE__ = 4294967296;
        /// <summary>
        /// Generates a strong cryptographic random number of a byte format (between 0 and 255).
        /// </summary>
        /// <param name="min">The minimum integer value that this random value can have</param>
        /// <param name="max">The maximum exclusive integer value that this random value can have</param>
        /// <param name="exceptNum">A representation of an array of integer values that should not take the random value</param>
        /// <returns>A random byte value</returns>
        /// <exception cref="CryptographyException"></exception>
        public static byte RandomGeneratedNumber(int min = 0, int max = __MAX_UBYTE_VALUE__, params int[] exceptNum)
        {
            if (min < 0 || max > __MAX_UBYTE_VALUE__ || min >= max)
            {
                throw new CryptographyException("Incoherent parameters for min or max.");
            }

            bool isExcept = false;
            int result = min - 1;
            while (result < min || result >= max || isExcept)
            {
                isExcept = false;
                byte[] randNumBuffer = new byte[1];
                RandomNumberGenerator.Create().GetBytes(randNumBuffer);
                result = randNumBuffer[0];
                foreach (int except in exceptNum)
                {
                    if (result == except)
                    {
                        isExcept = true;
                        break;
                    }
                }
            }
            return (byte)result;
        }

        /// <summary>
        /// Fill an full array of strong cryptographic random unsigned 16-bits integer values (between 0 and 65536).
        /// </summary>
        /// <param name="tableToFill">The reference array to fill with random values</param>
        /// <param name="min">The minimum integer value that this random value can have</param>
        /// <param name="max">The maximum exclusive integer value that this random value can have</param>
        /// <param name="exceptNum">A representation of an array of integer values that should not take the random values</param>
        /// <exception cref="CryptographyException"></exception>
        public static void RandomGeneratedNumberDb(ref ushort[] tableToFill, int min = 0, int max = __MAX_USHORT_VALUE__, params int[] exceptNum)
        {
            for (int db = 0; db < tableToFill.Length; db++)
            {
                tableToFill[db] = RandomGeneratedNumberDb(min, max, exceptNum);
            }
        }

        /// <summary>
        /// Generates a strong cryptographic random number of a unsigned 16-bits integer format (between 0 and 65536).
        /// </summary>
        /// <param name="min">The minimum integer value that this random value can have</param>
        /// <param name="max">The maximum exclusive integer value that this random value can have</param>
        /// <param name="exceptNum">A representation of an array of integer values that should not take the random value</param>
        /// <returns>A random unsigned 16-bits integer value</returns>
        /// <exception cref="CryptographyException"></exception>
        public static ushort RandomGeneratedNumberDb(int min = 0, int max = __MAX_USHORT_VALUE__, params int[] exceptNum)
        {
            if (min < 0 || max > __MAX_USHORT_VALUE__ || min >= max)
            {
                throw new CryptographyException("Incoherent parameters for min or max.");
            }

            bool isExcept = false;
            int result = min - 1;
            while (result < min || result >= max || isExcept)
            {
                isExcept = false;
                byte[] randBytesBuffer = new byte[2];
                RandomNumberGenerator.Create().GetBytes(randBytesBuffer);
                result = (randBytesBuffer[0] << 8) + randBytesBuffer[1];
                foreach (int except in exceptNum)
                {
                    if (result == except)
                    {
                        isExcept = true;
                        break;
                    }
                }
            }
            return (ushort)result;
        }

        /// <summary>
        /// Fill an full array of strong cryptographic random unsigned 32-bits integer values (between 0 and 4294967296).
        /// </summary>
        /// <param name="tableToFill">The reference array to fill with random values</param>
        /// <param name="min">The minimum integer value that this random value can have</param>
        /// <param name="max">The maximum exclusive integer value that this random value can have</param>
        /// <param name="exceptNum">A representation of an array of 64-bits integer values that should not take the random values</param>
        /// <exception cref="CryptographyException"></exception>
        public static void RandomGeneratedNumberQb(ref uint[] tableToFill, long min = 0, long max = __MAX_UINT_VALUE__, params long[] exceptNum)
        {
            for (int qb = 0; qb < tableToFill.Length; qb++)
            {
                tableToFill[qb] = RandomGeneratedNumberQb(min, max, exceptNum);
            }
        }

        /// <summary>
        /// Generates a strong cryptographic random number of a unsigned 32-bits integer format (between 0 and 4294967296).
        /// </summary>
        /// <param name="min">The minimum integer value that this random value can have</param>
        /// <param name="max">The maximum exclusive integer value that this random value can have</param>
        /// <param name="exceptNum">A representation of an array of 64-bits integer values that should not take the random value</param>
        /// <returns>A random unsigned 32-bits integer value</returns>
        /// <exception cref="CryptographyException"></exception>
        public static uint RandomGeneratedNumberQb(long min = 0, long max = __MAX_UINT_VALUE__, params long[] exceptNum)
        {
            if (min < 0 || max > __MAX_UINT_VALUE__ || min >= max)
            {
                throw new CryptographyException("Incoherent parameters for min or max.");
            }

            bool isExcept = false;
            long result = min - 1;
            while (result < min || result >= max || isExcept)
            {
                isExcept = false;
                byte[] randBytesBuffer = new byte[4];
                RandomNumberGenerator.Create().GetBytes(randBytesBuffer);
                result = ((long)randBytesBuffer[0] << 24) + ((long)randBytesBuffer[1] << 16) + ((long)randBytesBuffer[2] << 8) + randBytesBuffer[3];
                foreach (long except in exceptNum)
                {
                    if (result == except)
                    {
                        isExcept = true;
                        break;
                    }
                }
            }
            return (uint)result;
        }
    }
}