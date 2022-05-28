namespace LORENZKeygen
{
    /// <summary>
    /// Represents errors that occurs during key generation
    /// </summary>
    [System.Serializable]
    public class KeygenException : System.Exception
    {
        public KeygenException() { }
        public KeygenException(string message) : base(message) { }
        public KeygenException(string message, System.Exception inner) : base(message, inner) { }
        protected KeygenException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}