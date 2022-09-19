﻿using Cryptography;
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

        public static void AfficherMarqueurFin()
        {
            ConsoleColor colorForeBef = Console.ForegroundColor;
            ConsoleColor colorBackBef = Console.BackgroundColor;
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("=== FIN ===");
            Console.ForegroundColor = colorForeBef;
            Console.BackgroundColor = colorBackBef;
        }

        public static void EcrireChiffrementLong(string msgChiffre)
        {
            NomFichierChiffrement = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";

            if (Parametres.CipherFileDirectory == null)
            {
                SetCipherFileDirectory(true);
            }

            if (!Directory.Exists(Parametres.CipherFileDirectory))
            {
                Directory.CreateDirectory(Parametres.CipherFileDirectory);
            }

            File.WriteAllText(Parametres.CipherFileDirectory + NomFichierChiffrement, msgChiffre);
        }

        public static bool SetCipherFileDirectory(bool cancelDenied = false)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            while (true)
            {
                Console.WriteLine("Spécifiez le chemin d'accès absolu au répertoire des fichiers de chiffrement :");
                if (!cancelDenied)
                {
                    Console.WriteLine("Appuyez sur ENTRÉE sans rien écrire pour annuler.", MessageState.Warning);
                }

                Console.WriteLine();

                if (Parametres.CipherFileDirectory != null)
                {
                    Console.WriteLine("Répertoire actuel : " + Parametres.CipherFileDirectory);
                }

                Console.Write(">>> ");
                string dirPath = Console.ReadLine();
                if (dirPath == "")
                {
                    return false;
                }

                try
                {
                    DirectoryInfo dinfo = new(dirPath);
                    if (!dinfo.ToString().EndsWith('\\'))
                    {
                        Parametres.CipherFileDirectory = dinfo.FullName + "\\";
                    }
                    else
                    {
                        Parametres.CipherFileDirectory = dinfo.FullName;
                    }
                    Display.PrintMessage("Répertoire spécifié : " + Parametres.CipherFileDirectory);
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

            return true;
        }
    }
}
