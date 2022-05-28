using System;

namespace LORENZ
{
    public static class Extensions
    {
        public static void Configuration()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("Bienvenue " + Parametres.PseudoName);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}
