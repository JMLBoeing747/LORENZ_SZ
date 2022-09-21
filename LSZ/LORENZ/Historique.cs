using Cryptography;
using System;
using System.Collections.Generic;
using System.IO;

namespace LORENZ
{
    public enum PrivacyState
    {
        NotDefined = 0,
        Public = 1,
        Private = -1
    };

    public static class Historique
    {
        public static string FichierHistorique => $@"{Parametres.ParamsDirectory}/HISTORY.LZI";
        public static List<(DateTime, string, string, PrivacyState)> ListeHistorique { get; private set; } = new List<(DateTime, string, string, PrivacyState)>();


        public static void AfficherHistorique()
        {
            while (true)
            {
                Console.Clear();
                int headerSwitch = 0;
                for (int hEntry = ListeHistorique.Count - 1; hEntry >= 0; hEntry--)
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
                                    Console.WriteLine("---------- Hier -----------");
                                    headerSwitch = 2;
                                }
                                else if (dateEntry.Day == DateTime.Now.Day && headerSwitch < 1)
                                {
                                    Console.WriteLine("------- Aujourd'hui -------");
                                    headerSwitch = 1;
                                }
                                else if (dateEntry.Day < DateTime.Now.Day - 1)
                                {
                                    Console.WriteLine("------ Cette semaine ------");
                                    headerSwitch = 3;
                                }
                            }
                            else if (dateEntry.Day < DateTime.Now.Day - 7)
                            {
                                Console.WriteLine("------- Ce mois-ci --------");
                                headerSwitch = 4;
                            }
                        }
                        else if (dateEntry.Month < DateTime.Now.Month)
                        {
                            Console.WriteLine("------- Cette année -------");
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

                    /* paddingLeftMax : Nombre d'espaces minimum pour aligner les entrée en synchronisation avec
                     *                  l'augmentation de l'index
                     * realEntry :      Index réel de l'entrée à afficher sur l'écran
                     * padLeftStr :     String contenant les espaces ' ' qui alignent les entrées de l'historique
                     */
                    int paddingLeftMax = ListeHistorique.Count.ToString().Length - 1;
                    int realEntry = ListeHistorique.Count - hEntry;
                    string padLeftStr = new(' ', 1 + paddingLeftMax - realEntry.ToString().Length);

                    int lineLenMax = Console.WindowWidth - "[x]:dd-MM-yyyy HH:mm:ss : ".Length + paddingLeftMax - 13;
                    if (excerpt.Length > lineLenMax)
                    {
                        excerpt = excerpt[..lineLenMax] + "...";
                    }

                    Console.WriteLine($"[{realEntry}]:" + padLeftStr + " " + dtStr + " : " + excerpt);
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nPour accéder au contenu complet d'un de ces éléments, " +
                                  "inscrivez le numéro d'index situé à gauche");
                Console.WriteLine("qui les identifie et appuyez sur ENTRÉE.");
                Console.WriteLine("Appuyez sur Backspace pour effacer.");
                Console.WriteLine("\nPour retourner, appuyer sur ESC.");
                Console.Write(">> ");
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
                        if (numeroStr.Length != 0)
                        {
                            break;
                        }
                        else
                        {
                            Console.SetCursorPosition(curLeftInitial, curTopInitial);
                        }
                    }
                    else
                    {
                        Console.SetCursorPosition(curLeftInitial, curTopInitial);
                        Console.Write(' ');
                        Console.SetCursorPosition(curLeftInitial, curTopInitial);
                    }
                }

                // Afficher entrée
                if (numeroStr[0] == 'D' && numeroStr.Length > 1)
                {
                    int numeroDel = int.Parse(numeroStr[1..]);
                    int realNumero = ListeHistorique.Count - numeroDel;
                    RetirerHistorique(realNumero);
                }
                else if (numeroStr[0] != 'D')
                {
                    int numeroInt = int.Parse(numeroStr);
                    int realNumero = ListeHistorique.Count - numeroInt;
                    AfficherEntree(realNumero);
                }
                else
                {
                    Display.PrintMessage("Aucune entrée à supprimer !", MessageState.Failure);
                    Console.ReadKey(true);
                }
            }
        }

        public static void AfficherEntree(int index)
        {
            if (index < ListeHistorique.Count && index >= 0)
            {
                Console.Clear();
                string dateOfDeciphering = ListeHistorique[index].Item1.ToString("dddd d MMMM yyyy");
                string hourOfDeciphering = ListeHistorique[index].Item1.ToString("H:m:ss");
                string historicMsg = ListeHistorique[index].Item2;
                string msgAuthor = ListeHistorique[index].Item3 == "" ? "Inconnu" : ListeHistorique[index].Item3;
                PrivacyState privSta = ListeHistorique[index].Item4;

                int invIndex = ListeHistorique.Count - index;
                string headerMarker = new('=', 50);
                Console.WriteLine(headerMarker);
                Console.WriteLine("Message historique #" + invIndex);
                Console.WriteLine("Déchiffré le " + dateOfDeciphering + " à " + hourOfDeciphering);
                Console.WriteLine("Auteur : " + msgAuthor);
                Console.Write("Niveau de confidentialité : ");
                switch (privSta)
                {
                    case PrivacyState.NotDefined:
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Inconnu");
                        break;
                    case PrivacyState.Public:
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Public");
                        break;
                    case PrivacyState.Private:
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Privé");
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERROR");
                        break;
                }
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Cyan;

                Console.WriteLine(headerMarker);
                Console.WriteLine("\n" + historicMsg);
                Extensions.AfficherMarqueurFin();
                Console.WriteLine("\n[C]: Ajouter à une catégorie");
                Console.WriteLine("[Del]: Supprimer ce message");
                Console.WriteLine("\nAppuyez sur n'importe quelle autre touche pour quitter...");
                ConsoleKeyInfo saisie = Console.ReadKey(true);
                switch (saisie.Key)
                {
                    case ConsoleKey.C:
                        // Ajouter catégorie
                        break;
                    case ConsoleKey.Delete:
                        RetirerHistorique(index);
                        Display.PrintMessage("Message supprimé", MessageState.Warning);
                        Console.ReadKey(true);
                        return;
                    default:
                        break;
                }
            }
            else
            {
                Display.PrintMessage("Index invalide !", MessageState.Failure);
                Console.ReadKey(true);
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
                    string[] itemEntries = itemLine.Split('\0');
                    string itemEntry1 = "", itemEntry2 = "", itemEntry3 = "";
                    if (itemEntries.Length >= 2)
                    {
                        itemEntry1 = itemEntries[0];
                        itemEntry2 = itemEntries[1];
                    }

                    if (itemEntries.Length >= 3)
                    {
                        itemEntry3 = itemEntries[2];
                    }

                    PrivacyState privSta = PrivacyState.NotDefined;
                    if (itemEntries.Length >= 4)
                    {
                        privSta = itemEntries[3] switch
                        {
                            "1" => PrivacyState.Public,
                            "-1" => PrivacyState.Private,
                            _ => PrivacyState.NotDefined,
                        };
                    }


                    (DateTime, string, string, PrivacyState) tupleEntry = new();
                    DateTime dtEntry = DateTime.Parse(itemEntry1);
                    tupleEntry.Item1 = dtEntry;
                    tupleEntry.Item2 = itemEntry2;
                    tupleEntry.Item3 = itemEntry3;
                    tupleEntry.Item4 = privSta;
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
            foreach ((DateTime, string, string, PrivacyState) item in ListeHistorique)
            {
                allHistoryStr += item.Item1.ToUniversalTime().ToString("u") + "\0" + item.Item2 + "\0" + item.Item3 + "\0" + (int)item.Item4 + "\0\0";
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

        public static void AjouterHistorique(string nouvEntree, DateTime dateEntree, string auteur, PrivacyState pState = PrivacyState.NotDefined)
        {
            LireFichierHistorique();
            auteur = auteur == "" ? "Inconnu" : auteur;
            (DateTime, string, string, PrivacyState) nouvEntreeTuple = (dateEntree, nouvEntree, auteur, pState);
            ListeHistorique.Add(nouvEntreeTuple);
            EcrireHistorique();
        }

        public static void RetirerHistorique(int indexEntree)
        {
            if (indexEntree < ListeHistorique.Count && indexEntree >= 0)
            {
                ListeHistorique.RemoveAt(indexEntree);
                EcrireHistorique();
            }
            else
            {
                Display.PrintMessage("Index invalide !", MessageState.Failure);
                Console.ReadKey(true);
            }
        }
    }
}
