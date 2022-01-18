using System;

namespace Cryptography
{
    /// <summary>
    /// Represents errors that occurs during cryptography operations
    /// </summary>
    [Serializable]
    public class CryptographyException : Exception
    {
        /// <summary>
        /// Initialize a new instance of the CryptographyException class
        /// </summary>
        public CryptographyException() { }
        /// <summary>
        /// Initialize a new instance of the CryptographyException class
        /// </summary>
        /// <param name="message"><inheritdoc/></param>
        public CryptographyException(string message) : base(message) { }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="message"><inheritdoc/></param>
        /// <param name="inner"><inheritdoc/></param>
        public CryptographyException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="info"><inheritdoc/></param>
        /// <param name="context"><inheritdoc/></param>
        protected CryptographyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
