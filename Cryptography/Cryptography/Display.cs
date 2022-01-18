using System;

namespace Cryptography
{
    /// <summary>
    /// Enum for console message state display
    /// </summary>
    public enum MessageState
    {
        /// <summary>
        /// The success message state (color green)
        /// </summary>
        Success,
        /// <summary>
        /// The normal message state (current color when called)
        /// </summary>
        Normal,
        /// <summary>
        /// The information message state (color cyan)
        /// </summary>
        Info,
        /// <summary>
        /// The warning message state (color yellow)
        /// </summary>
        Warning,
        /// <summary>
        /// The failure message state (color red)
        /// </summary>
        Failure
    }

    /// <summary>
    /// Class for console displaying functions
    /// </summary>
    public static class Display
    {
        /// <summary>
        /// Prints a defined string message line on console with its corresponding state color
        /// </summary>
        /// <param name="message">The message to print on console</param>
        /// <param name="msgSta">The message state to show</param>
        /// <param name="haveNewLine">If false, no new line character will be added to the end of the message</param>
        /// <param name="resetColors">If true, puts console into its default foreground and background colors</param>
        public static void PrintMessage(string message, MessageState msgSta = MessageState.Normal, bool haveNewLine = true, bool resetColors = false)
        {
            ConsoleColor colorBefore = Console.ForegroundColor;
            switch (msgSta)
            {
                case MessageState.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    if (haveNewLine)
                        Console.WriteLine(message);
                    else
                        Console.Write(message);
                    if (resetColors)
                        Console.ResetColor();
                    else
                        Console.ForegroundColor = colorBefore;
                    break;
                case MessageState.Normal:
                    if (haveNewLine)
                        Console.WriteLine(message);
                    else
                        Console.Write(message);
                    if (resetColors)
                        Console.ResetColor();
                    else
                        Console.ForegroundColor = colorBefore;
                    break;
                case MessageState.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    if (haveNewLine)
                        Console.WriteLine(message);
                    else
                        Console.Write(message);
                    if (resetColors)
                        Console.ResetColor();
                    else
                        Console.ForegroundColor = colorBefore;
                    break;
                case MessageState.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (haveNewLine)
                        Console.WriteLine(message);
                    else
                        Console.Write(message);
                    if (resetColors)
                        Console.ResetColor();
                    else
                        Console.ForegroundColor = colorBefore;
                    break;
                case MessageState.Failure:
                    Console.ForegroundColor = ConsoleColor.Red;
                    if (haveNewLine)
                        Console.WriteLine(message);
                    else
                        Console.Write(message);
                    if (resetColors)
                        Console.ResetColor();
                    else
                        Console.ForegroundColor = colorBefore;
                    break;
                default:
                    break;
            }
        }
    }
}
