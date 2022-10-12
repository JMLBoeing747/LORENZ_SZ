using Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

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
        public static List<(DateTime, string, string, PrivacyState)> ListeHistorique { get; private set; } = new();

        public static void AfficherHistorique()
        {
            int msgHistoryMaxLen = 0;
            for (int m = 0; m < ListeHistorique.Count; m++)
            {
                if (ListeHistorique[m].Item2.Length > msgHistoryMaxLen)
                {
                    msgHistoryMaxLen = ListeHistorique[m].Item2.Length;
                }
            }

            while (true)
            {
                Console.Clear();
                int headerSwitch = 0;
                for (int hEntry = ListeHistorique.Count - 1; hEntry >= 0; hEntry--)
                {
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                    Console.ForegroundColor = ConsoleColor.White;

                    DateTime dateEntry = ListeHistorique[hEntry].Item1;
                    if (headerSwitch < 1000)
                    {
                        if (dateEntry.Year == DateTime.Now.Year && headerSwitch < 100)
                        {
                            if (dateEntry.Month == DateTime.Now.Month && headerSwitch < 100)
                            {
                                if (dateEntry.Day >= DateTime.Now.Day - 7 && headerSwitch < 10)
                                {
                                    if (dateEntry.Day == DateTime.Now.Day && headerSwitch < 1)
                                    {
                                        Console.WriteLine("           Aujourd'hui           ");
                                        headerSwitch = 1;
                                    }
                                    else if (dateEntry.Day == DateTime.Now.Day - 1 && headerSwitch < 2)
                                    {
                                        Console.WriteLine("              Hier               ");
                                        headerSwitch = 2;
                                    }
                                    else if (dateEntry.Day == DateTime.Now.Day - 2 && headerSwitch < 3)
                                    {
                                        Console.WriteLine("           Avant-hier            ");
                                        headerSwitch = 3;
                                    }
                                    else if (dateEntry.Day < DateTime.Now.Day - 2)
                                    {
                                        Console.WriteLine("          Cette semaine          ");
                                        headerSwitch = 10;
                                    }
                                }
                                else if (dateEntry.Month == DateTime.Now.Month)
                                {
                                    Console.WriteLine("           Ce mois-ci            ");
                                    headerSwitch = 100;
                                }
                            }
                            else if (dateEntry.Month == DateTime.Now.Month - 1 && headerSwitch < 20)
                            {
                                Console.WriteLine("         Le mois dernier         ");
                                headerSwitch = 20;
                            }
                            else if (dateEntry.Month < DateTime.Now.Month - 1)
                            {
                                Console.WriteLine("           Cette année           ");
                                headerSwitch = 100;
                            }
                        }
                        else if (dateEntry.Year == DateTime.Now.Year - 1 && headerSwitch < 200)
                        {
                            Console.WriteLine("         L'année dernière        ");
                            headerSwitch = 200;
                        }
                        else
                        {
                            Console.WriteLine("         Il y a longtemps        ");
                            headerSwitch = 1000;
                        }
                    }

                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Cyan;

                    string dtStr = ListeHistorique[hEntry].Item1.ToString("G");
                    string excerpt = ListeHistorique[hEntry].Item2.Replace('\n', ' ');
                    int excerptLen = ListeHistorique[hEntry].Item2.Length;

                    /* indexPaddingMax :  Nombre d'espaces minimum pour aligner les entrées en synchronisation avec
                     *                   l'augmentation de l'index
                     * lenPaddingMax :    Nombre d'espaces minimum pour aligner les entrées selon la plus grande longueur
                     *                   de message sauvegardé dans l'historique
                     * realEntry :        Index réel de l'entrée à afficher sur l'écran
                     * indexPadStr :      String contenant les espaces ' ' qui alignent les entrées de l'historique
                     *                   selon l'index du message
                     * lenPadStr :        String contenant les espaces ' ' qui alignent les entrées de l'historique
                     *                   selon la longueur du message
                     */
                    int indexPaddingMax = ListeHistorique.Count.ToString().Length;
                    int lenPaddingMax = msgHistoryMaxLen.ToString().Length;
                    int realEntry = ListeHistorique.Count - hEntry;
                    string indexPadStr = new(' ', indexPaddingMax - realEntry.ToString().Length);
                    string lenPadStr = new(' ', lenPaddingMax - excerptLen.ToString().Length);

                    int lineLenMax = Console.WindowWidth
                                     - "[x]: dd-MM-yyyy HH:mm:ss | long. : ".Length
                                     - " | ".Length
                                     - indexPaddingMax
                                     - lenPaddingMax
                                     - "...".Length
                                     - 10;

                    if (excerpt.Length > lineLenMax)
                    {
                        excerpt = excerpt[..lineLenMax] + "...";
                    }

                    Console.WriteLine($"[{realEntry}]: "
                                      + indexPadStr
                                      + dtStr
                                      + " | long. : "
                                      + excerptLen
                                      + lenPadStr
                                      + " | "
                                      + excerpt);
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nPour accéder au contenu complet d'un de ces éléments, " +
                                  "inscrivez le numéro d'index situé à gauche");
                Console.WriteLine("qui les identifie et appuyez sur ENTRÉE.");
                Console.WriteLine("Vous pouvez utiliser Backspace pour corriger.");
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
                    else if (numero.Key == ConsoleKey.Backspace && curLeftInitial > 3)
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
                string hourOfDeciphering = ListeHistorique[index].Item1.ToString("H' h 'mm");
                string historicMsg = ListeHistorique[index].Item2;
                string msgAuthor = ListeHistorique[index].Item3 == "" ? "Inconnu" : ListeHistorique[index].Item3;
                PrivacyState privSta = ListeHistorique[index].Item4;

                string modelMax = "Déchiffré le dimanche 31 décembre 2000 à 11 h 59 ";
                int invIndex = ListeHistorique.Count - index;
                string headerMarker = new('=', modelMax.Length);
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
                Console.WriteLine("Longueur : " + historicMsg.Length + " charactères");
                Console.WriteLine(headerMarker);

                Console.Write("\n" + historicMsg);
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
                Cryptographie.CreateMatrix(ref cipherKey, -23);
                Common.XORPassIntoMessage(cipherKey, ref value);
                Cryptographie.CreateMatrix(ref cipherKey, -24);
                Common.ReverseKey(ref cipherKey);
                Common.XORPassIntoMessage(cipherKey, ref value);
                Common.NotOperationToKey(ref cipherKey);
                Common.XORPassIntoMessage(cipherKey, ref value);

                string historyStr = "";
                foreach (uint bItem in value)
                {
                    historyStr += (char)(bItem & 0xFF);
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
            for (int i = 0; i < allHistoryUInt.Length; i++)
            {
                byte[] filling = new byte[3];
                RandomNumberGenerator.Create().GetBytes(filling);
                uint filling3Bytes = 0;
                for (int b = 0; b < filling.Length; b++)
                {
                    filling3Bytes += (uint)filling[b] << (8 * b);
                }

                allHistoryUInt[i] += filling3Bytes << 8;
            }

            uint[] cipherKey = new uint[Common.KeyNbrUInt];
            Cryptography.Random.RandomGeneratedNumberQb(ref cipherKey);
            Common.XORPassIntoMessage(cipherKey, ref allHistoryUInt);
            Common.NotOperationToKey(ref cipherKey);
            Common.XORPassIntoMessage(cipherKey, ref allHistoryUInt);
            Common.ReverseKey(ref cipherKey);
            Cryptographie.CreateMatrix(ref cipherKey, 24);
            Common.XORPassIntoMessage(cipherKey, ref allHistoryUInt);
            Cryptographie.CreateMatrix(ref cipherKey, 23);
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
