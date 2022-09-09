using Cryptography;
using System;
using System.IO;
using System.Security;

namespace LORENZ
{
    public static class Extensions
    {
        private static string NomFichierChiffrement { get; set; }
        public static string GetNomFichierChiffrement()
        {
            return NomFichierChiffrement ?? "";
        }
        
        public static void Configuration()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("Bienvenue " + Parametres.PseudoName);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        public static void EcrireChiffrementLong(string msgChiffre)
        {
            NomFichierChiffrement = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
            
            if (Parametres.CipherFileDirectory == null)
            {
                InitCipherFileDirectory();
            }

            if (!Directory.Exists(Parametres.CipherFileDirectory))
            {
                Directory.CreateDirectory(Parametres.CipherFileDirectory);
            }
            
            File.WriteAllText(Parametres.CipherFileDirectory + NomFichierChiffrement, msgChiffre);
        }

        private static void InitCipherFileDirectory()
        {
            while (true)
            {
                Display.PrintMessage("Spécifiez le chemin d'accès absolu au répertoire des fichiers de chiffrement :\n", MessageState.Warning);
                Console.Write(">>> ");
                string dirPath = Console.ReadLine();
                try
                {
                    DirectoryInfo dinfo = new DirectoryInfo(dirPath);
                    if (!dinfo.ToString().EndsWith('\\'))
                    {
                        Parametres.CipherFileDirectory = dinfo.FullName + "\\";
                    }
                    else
                    {
                        Parametres.CipherFileDirectory = dinfo.FullName;
                    }
                    Display.PrintMessage("Répertoire spécifié : " + dinfo.FullName);
                    Parametres.WriteGeneralParamsFile();
                    break;
                }
                catch (ArgumentNullException)
                {
                    Display.PrintMessage("Aucun chemin d'accès spécifié.", MessageState.Failure);
                }
                catch (ArgumentException)
                {
                    Display.PrintMessage("Chemin d'accès invalide.\n" +
                        "Utilisez '\\' au lieu de '/' pour les séparateurs,\n" +
                        "ou retirez tout caractère interdit #%&{}<>*?$!'\":@+`|=", MessageState.Failure);
                }
                catch (SecurityException)
                {
                    Display.PrintMessage("L'accès au répertoire est refusé.", MessageState.Failure);
                }
                catch (Exception)
                {
                    Display.PrintMessage("Chemin d'accès invalide.", MessageState.Failure);
                }
            }
        }
    }
}
