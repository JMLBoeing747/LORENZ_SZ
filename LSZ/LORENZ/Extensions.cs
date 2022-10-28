using Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading;

namespace LORENZ
{
    public static class Extensions
    {
        private static string NomFichierChiffrement { get; set; }
        public static string GetNomFichierChiffrement()
        {
            return NomFichierChiffrement ?? "";
        }
        public static string DeniedChars => "#%&{}<>*?$!'\":@+`|=";

        public static void Configuration()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Write("Bienvenue " + Parametres.PseudoName);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
        }

        public static void AfficherMarqueurFin()
        {
            ConsoleColor colorForeBef = Console.ForegroundColor;
            ConsoleColor colorBackBef = Console.BackgroundColor;
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("=== FIN ===");
            Console.ForegroundColor = colorForeBef;
            Console.BackgroundColor = colorBackBef;
            Console.WriteLine();
        }

        public static void AfficherTitre(string titre,
                                         ConsoleColor backTitle,
                                         ConsoleColor foreTitle = ConsoleColor.White,
                                         int length = 35,
                                         bool capitals = true)
        {
            ConsoleColor colorForeBef = Console.ForegroundColor;
            ConsoleColor colorBackBef = Console.BackgroundColor;
            Console.BackgroundColor = backTitle;
            Console.ForegroundColor = foreTitle;
            if (capitals)
            {
                titre = titre.ToUpper();
            }

            int spaceCount = (length - titre.Length) / 2;
            if (spaceCount > 0)
            {
                titre = new string(' ', spaceCount) + titre + new string(' ', spaceCount);
                int diff = length - titre.Length;
                titre = diff > 0 ? new string(' ', diff) + titre : titre;
            }
            Console.WriteLine($"\n{titre}\n");
            Console.ResetColor();
            Console.ForegroundColor = colorForeBef;
            Console.BackgroundColor = colorBackBef;
        }

        public static bool EcrireChiffrementLong(string msgChiffre, string cipherFileName = "")
        {
            if (cipherFileName == "")
            {
                return false;
            }

            NomFichierChiffrement = cipherFileName;

            if (Parametres.CipherFileDirectory == null)
            {
                SetCipherFileDirectory(true);
            }

            if (!Directory.Exists(Parametres.CipherFileDirectory))
            {
                Directory.CreateDirectory(Parametres.CipherFileDirectory);
            }

            try
            {
                File.WriteAllText(Parametres.CipherFileDirectory + NomFichierChiffrement, msgChiffre);
                return true;
            }
            catch (ArgumentException)
            {
                Display.PrintMessage("Chemin d'accès invalide.\n"
                                     + "Retirez tout caractère interdit "
                                     + DeniedChars, MessageState.Failure);
            }
            catch (IOException)
            {
                Display.PrintMessage("Chemin d'accès invalide.\n"
                                     + "Retirez tout caractère interdit "
                                     + DeniedChars, MessageState.Failure);
            }
            catch (Exception)
            {
                Display.PrintMessage("Chemin d'accès invalide.", MessageState.Failure);
            }

            return false;
        }

        public static bool SetCipherFileDirectory(bool cancelDenied = false)
        {
            if (cancelDenied)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }

            AfficherTitre("Répertoire des chiffrements", ConsoleColor.DarkYellow, ConsoleColor.Black);
            Console.WriteLine("Le répertoire des fichiers de chiffrement permet à LORENZ de localiser les fichiers de chiffrement.");
            Console.WriteLine("Ces fichiers se créent lorsqu'un message chiffré devient trop long pour être retranscrit sur le");
            Console.WriteLine("terminal. Lorsque viendra le temps de déchiffrer de tels fichiers, il faudra alors les insérer dans");
            Console.WriteLine("ce répertoire afin d'amorcer leur déchiffrement.");
            Console.WriteLine("\nSpécifiez le chemin d'accès absolu au répertoire des fichiers de chiffrement :");
            if (!cancelDenied)
            {
                Display.PrintMessage("Pour annuler, appuyez sur ESC ou sur ENTRÉE sans rien écrire", MessageState.Warning);
            }

            if (Parametres.CipherFileDirectory != null)
            {
                Console.WriteLine("\nRépertoire actuel : " + Parametres.CipherFileDirectory);
            }

            while (true)
            {
                Console.Write(">>> ");
                string dirPath = SpecialPrint();
                if (dirPath is null or "")
                {
                    if (!cancelDenied)
                    {
                        return false;
                    }
                    else
                    {
                        Display.PrintMessage("Vous devez spécifier un chemin d'accès valide.", MessageState.Failure);
                        continue;
                    }
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
                    Display.PrintMessage("Répertoire spécifié : " + Parametres.CipherFileDirectory, MessageState.Success);
                    Parametres.EcrireGeneralParamsFile();
                    break;
                }
                catch (ArgumentNullException)
                {
                    Display.PrintMessage("Aucun chemin d'accès spécifié.", MessageState.Failure);
                }
                catch (ArgumentException)
                {
                    Display.PrintMessage("Chemin d'accès invalide.\n"
                                         + "Utilisez '\\' au lieu de '/' pour les séparateurs,\n"
                                         + "ou retirez tout caractère interdit "
                                         + DeniedChars, MessageState.Failure);
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

        public static string SpecialPrint(char endChar = '\r',
                                          uint maxLength = 0,
                                          bool hideEndChar = true,
                                          bool stripEndChar = true,
                                          ConsoleKey escapeKey = ConsoleKey.Escape,
                                          bool addNewLine = true,
                                          bool includeCtrl = false)
        {
            string writeLine = "";
            char pressChar;
            int topTop = Console.CursorTop;
            int beginBegin = Console.CursorLeft;
            Stack<int> lastLeft = new();
            do
            {
                int beginTop = Console.CursorTop;
                int beginLeft = Console.CursorLeft;
                ConsoleKeyInfo keyPress = Console.ReadKey();
                pressChar = keyPress.KeyChar;
                if (keyPress.Key == escapeKey)
                {
                    Console.Write("\xFF");
                    return null;
                }
                else if (keyPress.Key == ConsoleKey.Backspace)
                {
                    if (Console.CursorLeft < beginBegin)
                    {
                        Console.CursorLeft++;
                        continue;
                    }

                    Console.Write(' ');
                    Console.CursorLeft--;
                    if (Console.CursorLeft == beginLeft && Console.CursorTop > topTop)
                    {
                        Console.CursorTop--;
                        int charsInLine = lastLeft.Count > 1 ? lastLeft.Pop() - lastLeft.Peek() : lastLeft.Pop();
                        int curEndLn = charsInLine % Console.WindowWidth;
                        Console.CursorLeft = curEndLn;
                        if (writeLine[^1] != '\n')
                        {
                            Console.CursorLeft--;
                            Console.Write(' ');
                            Console.CursorLeft--;
                        }
                        writeLine = writeLine[..^1];
                        continue;
                    }

                    if (writeLine.Length > 0)
                    {
                        writeLine = writeLine[..^1];
                    }
                    continue;
                }
                else if (maxLength == 0 || writeLine.Length < maxLength)
                {
                    if ((keyPress.Key == ConsoleKey.Enter && endChar != '\r')
                         || (Console.CursorLeft == Console.WindowWidth - 1))
                    {
                        int totalCharsWritten = Console.CursorLeft < Console.WindowWidth - 1 ?
                                                writeLine.Length - lastLeft.Count : writeLine.Length + 1 - lastLeft.Count;
                        lastLeft.Push(totalCharsWritten);
                        Console.CursorTop++;

                        if (Console.CursorLeft < Console.WindowWidth - 1)
                        {
                            pressChar = '\n';
                        }
                        else
                        {
                            Console.CursorLeft = 0;
                        }
                    }
                    else if (!includeCtrl && pressChar != '\0')
                    {
                        if (pressChar != endChar && pressChar is > '\0' and < ' ')
                        {
                            if (Console.CursorLeft == beginLeft + 1)
                            {
                                Console.CursorLeft--;
                                Console.Write(' ');
                                Console.CursorLeft--;
                            }
                            else if (pressChar == '\x0A' && Console.CursorTop > topTop)
                            {
                                Console.CursorTop--;
                                Console.CursorLeft = beginLeft;
                            }
                            else if (pressChar == '\x09')
                            {
                                int newSpaces = Console.CursorLeft - beginLeft;
                                writeLine += new string(' ', newSpaces);
                            }

                            continue;
                        }
                    }
                    else if (pressChar == '\0')
                    {
                        continue;
                    }
                }
                else
                {
                    Console.CursorTop = beginTop;
                    if (Console.CursorLeft > beginLeft)
                    {
                        int forwardChars = Console.CursorLeft - beginLeft;
                        Console.CursorLeft = beginLeft;
                        Console.Write(new string(' ', forwardChars));
                        Console.CursorLeft = beginLeft;
                    }
                    
                    continue;
                }

                writeLine += pressChar;
            } while (pressChar != endChar);

            if (stripEndChar)
            {
                writeLine = writeLine[..^1];
            }
            
            if (hideEndChar && !string.IsNullOrWhiteSpace(endChar.ToString()))
            {
                Console.CursorLeft--;
                Console.Write(' ');
                Console.CursorLeft--;
            }

            if (addNewLine)
            {
                Console.WriteLine();
            }

            return writeLine;
        }

        public static void Music()
        {
            Display.PrintMessage("Playing...");

            int noteDur = 100;
            int sleepDur = 255;

            for (int times = 0; times < 4; times++)
            {
                Console.Beep(370, noteDur);
                Thread.Sleep(sleepDur);
                Console.Beep(294, noteDur);
                Thread.Sleep(sleepDur);
                Console.Beep(370, noteDur);
                Thread.Sleep(sleepDur);
            }
            Thread.Sleep(sleepDur);
            for (int times = 0; times < 4; times++)
            {
                Console.Beep(330, noteDur);
                Thread.Sleep(sleepDur);
                Console.Beep(277, noteDur);
                Thread.Sleep(sleepDur);
                if (times < 3)
                {
                    Console.Beep(330, noteDur);
                    Thread.Sleep(sleepDur);
                }
            }
        }
    }
}

