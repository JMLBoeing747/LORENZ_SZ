using Cryptography;
using System;

namespace LORENZ
{
    public enum ErrorCode
    {
        E0x00,
        E0x11,
        E0x12,
        E0x20,
        E0xFFF,
    }

    [Serializable]
    public class LORENZException : Exception
    {
        public ErrorCode Err { get; set; }
        public LORENZException(ErrorCode err, bool haveMessageWithKey = true)
        {
            Err = err;
            if (haveMessageWithKey)
            {
                switch (err)
                {
                    case ErrorCode.E0x00:
                        Display.PrintMessage("PROCESS TERMINATED...EXIT PROGRAM.", MessageState.Failure);
                        break;
                    case ErrorCode.E0x11:
                        Display.PrintMessage("ACCESS DENIED! Vous n'avez pas les droits d'accès à ce programme!\nERROR CODE 0x11", MessageState.Failure);
                        break;
                    case ErrorCode.E0x12:
                        Display.PrintMessage("ACCESS DENIED! Vous n'avez pas les droits d'accès à ce programme!\nERROR CODE 0x12", MessageState.Failure);
                        break;
                    case ErrorCode.E0x20:
                        Display.PrintMessage("ACCESS DENIED! Vous n'avez pas les droits d'accès à ce programme!\nERROR CODE 0x20", MessageState.Failure);
                        break;
                    default:
                        break;
                }
                Console.ReadKey(true);
            }
        }
        public LORENZException(string message) : base(message) { }
        public LORENZException(string message, Exception inner) : base(message, inner) { }
        protected LORENZException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
