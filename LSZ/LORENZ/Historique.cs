using Cryptography;
using System;
using System.Collections.Generic;
using System.IO;

namespace LORENZ
{
    public static class Historique
    {
        public static string FichierHistorique => $@"{Parametres.ParamsDirectory}/HISTORY.LZI";
        public static List<(DateTime, string)> ListeHistorique { get; private set; } = new List<(DateTime, string)>();


        public static void AfficherHistorique()
        {
            Console.Clear();
            int headerSwitch = 0;
            for (int hEntry = 0; hEntry < ListeHistorique.Count; hEntry++)
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.ForegroundColor = ConsoleColor.White;

                DateTime dateEntry = ListeHistorique[hEntry].Item1;
                if (dateEntry.Year == DateTime.Now.Year && headerSwitch < 5)
                {
                    if (dateEntry.Month == DateTime.Now.Month && headerSwitch < 4)
                    {
                        if (dateEntry.Day >= DateTime.Now.Day - 7 && headerSwitch < 3)
                        {
                            if (dateEntry.Day == DateTime.Now.Day - 1 && headerSwitch < 2)
                            {
                                Console.WriteLine("-------- Hier ---------");
                                headerSwitch = 2;
                            }
                            else if (dateEntry.Day == DateTime.Now.Day && headerSwitch < 1)
                            {
                                Console.WriteLine("----- Aujourd'hui -----");
                                headerSwitch = 1;
                            }
                            else if (dateEntry.Day < DateTime.Now.Day - 1)
                            {
                                Console.WriteLine("---- Cette semaine ----");
                                headerSwitch = 3;
                            }
                        }
                        else if (dateEntry.Day < DateTime.Now.Day - 7)
                        {
                            Console.WriteLine("----- Ce mois-ci ------");
                            headerSwitch = 4;
                        }
                    }
                    else if (dateEntry.Month < DateTime.Now.Month)
                    {
                        Console.WriteLine("----- Cette année -----");
                        headerSwitch = 5;
                    }
                }
                else if (dateEntry.Year < DateTime.Now.Year)
                {
                    Console.WriteLine("--- Il y a longtemps ---");
                    headerSwitch = 6;
                }

                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Cyan;

                string dtStr = ListeHistorique[hEntry].Item1.ToString("G");
                string excerpt = ListeHistorique[hEntry].Item2.Replace('\n', ' ');

                int lineLenMax = Console.WindowWidth - "[x]:dd-MM-yyyy HH:mm:ss : ".Length - 13;
                if (excerpt.Length > lineLenMax)
                {
                    excerpt = excerpt[..lineLenMax] + "...";
                }

                Console.WriteLine($"[{hEntry + 1}]:" + dtStr + " : " + excerpt);
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nPour accéder au contenu complet d'un de ces éléments, tapez le numéro d'index situé à gauche");
            Console.WriteLine("qui les identifie et appuyez sur ENTRÉE.");
            Console.WriteLine("Appuyez sur Backspace pour effacer.");
            Console.WriteLine("\nPour retourner, appuyer sur ESC.");
            Console.ForegroundColor = ConsoleColor.Cyan;

            string numeroStr = "";
            while (true)
            {
                int curTopInitial = Console.CursorTop;
                int curLeftInitial = Console.CursorLeft;

                ConsoleKeyInfo numero = Console.ReadKey();
                if (numero.Key == ConsoleKey.Escape)
                {
                    return;
                }

                if (numero.Key is >= ConsoleKey.D0 and <= ConsoleKey.D9)
                {
                    numeroStr += ((int)numero.Key - 48).ToString();
                }
                else if (numero.Key is >= ConsoleKey.NumPad0 and <= ConsoleKey.NumPad9)
                {
                    numeroStr += ((int)numero.Key - 96).ToString();
                }
                else if (numero.Key == ConsoleKey.Delete && numeroStr.Length == 0)
                {
                    numeroStr += 'D';
                    Console.Write("Delete : ");
                }
                else if (numero.Key == ConsoleKey.Backspace && curLeftInitial > 0)
                {
                    if (numeroStr != "D")
                    {
                        Console.SetCursorPosition(curLeftInitial - 1, curTopInitial);
                        Console.Write(' ');
                        Console.SetCursorPosition(curLeftInitial - 1, curTopInitial);
                    }
                    else
                    {
                        Console.SetCursorPosition(curLeftInitial - 9, curTopInitial);
                        Console.Write("        ");
                        Console.SetCursorPosition(curLeftInitial - 9, curTopInitial);
                    }

                    numeroStr = numeroStr[..(numeroStr.Length - 1)];
                }
                else if (numero.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else
                {
                    Console.SetCursorPosition(curLeftInitial, curTopInitial);
                    Console.Write(' ');
                    Console.SetCursorPosition(curLeftInitial, curTopInitial);
                }
            }



        }

        public static bool LireFichierHistorique()
        {
            try
            {
                if (!File.Exists(FichierHistorique))
                {
                    return false;
                }

                Decyphering.OpeningDecyphering(FichierHistorique, out uint[] cipherKey, out uint[] value);
                Common.XORPassIntoMessage(cipherKey, ref value);
                Common.ReverseKey(ref cipherKey);
                Common.NotOperationToKey(ref cipherKey);
                Common.XORPassIntoMessage(cipherKey, ref value);

                string historyStr = "";
                foreach (uint bItem in value)
                {
                    historyStr += (char)bItem;
                }

                ListeHistorique.Clear();
                string[] historyLines = historyStr.Split("\0\0", StringSplitOptions.RemoveEmptyEntries);
                foreach (string itemLine in historyLines)
                {
                    string itemEntry1 = itemLine[..itemLine.IndexOf('\0')];
                    string itemEntry2 = itemLine[(itemLine.IndexOf('\0') + 1)..];
                    (DateTime, string) tupleEntry = new();
                    DateTime dtEntry = DateTime.Parse(itemEntry1);
                    tupleEntry.Item1 = dtEntry;
                    tupleEntry.Item2 = itemEntry2;
                    ListeHistorique.Add(tupleEntry);
                }
            }
            catch (CryptographyException)
            {
                Console.WriteLine("Un problème est survenu lors de la lecture du fichier de l'historique.");
                Console.WriteLine("Il est possible que le fichier ait été supprimé, déplacé ou corrompu.");
            }

            return true;
        }

        public static void EcrireHistorique()
        {
            Common.CphrMode = CypherMode.x1;
            string allHistoryStr = "";
            foreach ((DateTime, string) item in ListeHistorique)
            {
                allHistoryStr += item.Item1.ToString("u") + "\0" + item.Item2 + "\0\0";
            }

            uint[] allHistoryUInt = Encryption.ToUIntArray(allHistoryStr);
            uint[] cipherKey = new uint[Common.KeyNbrUInt];
            Cryptography.Random.RandomGeneratedNumberQb(ref cipherKey);
            Common.XORPassIntoMessage(cipherKey, ref allHistoryUInt);
            Common.NotOperationToKey(ref cipherKey);
            Common.ReverseKey(ref cipherKey);
            Common.XORPassIntoMessage(cipherKey, ref allHistoryUInt);
            Encryption.ClosingCyphering(cipherKey, ref allHistoryUInt);
            Encryption.WriteCypherIntoFile(allHistoryUInt, FichierHistorique);
        }

        public static void AjouterHistorique(string nouvEntree, DateTime dateEntree)
        {
            LireFichierHistorique();
            (DateTime, string) nouvEntreeTuple = (dateEntree, nouvEntree);
            ListeHistorique.Add(nouvEntreeTuple);
            EcrireHistorique();
        }
    }
}
