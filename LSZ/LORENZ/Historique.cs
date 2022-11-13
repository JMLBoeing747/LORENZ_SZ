﻿using Cryptography;
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
        private const char RECD_SEP = '\x1E';
        private const char UNIT_SEP = '\x1F';

        public static string FichierHistorique => $@"{Parametres.ParamsDirectory}/HISTORY.LZI";
        public static List<(uint ID, DateTime cipherDate, string msg, string author, PrivacyState pState)> ListeHistorique { get; set; } = new();
        public static int Count => ListeHistorique.Count;
        
        public static void AfficherHistorique(List<uint> selection = null)
        {
            List<(uint ID, DateTime cipherDate, string msg, string author, PrivacyState pState)> tempHist = new();
            if (selection != null)
            {
                for (int e = 0; e < Count; e++)
                {
                    if (selection.Contains(ListeHistorique[e].ID))
                    {
                        tempHist.Add(ListeHistorique[e]);
                    }
                }
            }
            else
            {
                tempHist = ListeHistorique;
            }

            int msgHistoryMaxLen = 0;
            for (int m = 0; m < tempHist.Count; m++)
            {
                if (tempHist[m].msg.Length > msgHistoryMaxLen)
                {
                    msgHistoryMaxLen = tempHist[m].msg.Length;
                }
            }

            int page = 0;
            int lastEntry = tempHist.Count - 1;
            Stack<int> stackLastEntry = new();
            while (true)
            {
                Console.Clear();
                int headerSwitch = 0;
                int entryMaxHeight = Console.WindowHeight - 12;
                int headerMaxHeight = entryMaxHeight - 1;
                bool testHeader = false;
                bool failHeader = false;
                for (int hEntry = lastEntry; hEntry >= 0; hEntry--)
                {   
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                    Console.ForegroundColor = ConsoleColor.White;

                    DateTime dateEntry = tempHist[hEntry].cipherDate;
                    if (headerSwitch < 1000)
                    {
                        if (dateEntry.Year == DateTime.Now.Year && headerSwitch < 1000)
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
                                        if (!testHeader)
                                        {
                                            Console.WriteLine("              Hier               ");
                                            headerSwitch = 2;
                                        }

                                        failHeader = testHeader;
                                    }
                                    else if (dateEntry.Day == DateTime.Now.Day - 2 && headerSwitch < 3)
                                    {
                                        if (!testHeader)
                                        {
                                            Console.WriteLine("           Avant-hier            ");
                                            headerSwitch = 3;
                                        }

                                        failHeader = testHeader;
                                    }
                                    else if (dateEntry.Day < DateTime.Now.Day - 2)
                                    {
                                        if (!testHeader)
                                        {
                                            Console.WriteLine("          Cette semaine          ");
                                            headerSwitch = 10;
                                        }

                                        failHeader = testHeader;
                                    }
                                }
                                else if (dateEntry.Day < DateTime.Now.Day - 7)
                                {
                                    if (!testHeader)
                                    {
                                        Console.WriteLine("           Ce mois-ci            ");
                                        headerSwitch = 100;
                                    }

                                    failHeader = testHeader;
                                }
                            }
                            else if (dateEntry.Month == DateTime.Now.Month - 1 && headerSwitch < 200)
                            {
                                if (!testHeader)
                                {
                                    Console.WriteLine("         Le mois dernier         ");
                                    headerSwitch = 200;
                                }

                                failHeader = testHeader;
                            }
                            else if (dateEntry.Month < DateTime.Now.Month - 1)
                            {
                                if (!testHeader)
                                {
                                    Console.WriteLine("           Cette année           ");
                                    headerSwitch = 1000;
                                }

                                failHeader = testHeader;
                            }
                        }
                        else if (dateEntry.Year == DateTime.Now.Year - 1 && headerSwitch < 2000)
                        {
                            if (!testHeader)
                            {
                                Console.WriteLine("         L'année dernière        ");
                                headerSwitch = 2000;
                            }

                            failHeader = testHeader;
                        }
                        else
                        {
                            if (!testHeader)
                            {
                                Console.WriteLine("         Il y a longtemps        ");
                                headerSwitch = 10000;
                            }

                            failHeader = testHeader;
                        }
                    }

                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Cyan;

                    // realEntry : Index réel de l'entrée à afficher sur l'écran
                    int realEntry = tempHist.Count - hEntry;
                    if (!failHeader)
                    {
                        string dtStr = tempHist[hEntry].cipherDate.ToString("G");
                        string excerpt = tempHist[hEntry].msg.Replace('\n', ' ');
                        int excerptLen = tempHist[hEntry].msg.Length;

                        /* indexPaddingMax :  Nombre d'espaces minimum pour aligner les entrées en synchronisation avec
                         *                   l'augmentation de l'index
                         * lenPaddingMax :    Nombre d'espaces minimum pour aligner les entrées selon la plus grande longueur
                         *                   de message sauvegardé dans l'historique
                         * indexPadStr :      String contenant les espaces ' ' qui alignent les entrées de l'historique
                         *                   selon l'index du message
                         * lenPadStr :        String contenant les espaces ' ' qui alignent les entrées de l'historique
                         *                   selon la longueur du message
                         */
                        int indexPaddingMax = tempHist.Count.ToString().Length;
                        int lenPaddingMax = msgHistoryMaxLen.ToString().Length;
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

                    testHeader = Console.CursorTop > headerMaxHeight;
                    string leastEntries = $"{realEntry} / {tempHist.Count} entrées";
                    if ((Console.CursorTop > entryMaxHeight && hEntry > 0) || failHeader)
                    {
                        if (failHeader)
                        {
                            Console.WriteLine();
                            hEntry += 1;
                        }

                        string fowBackStr;
                        if (page > 0)
                        {
                            fowBackStr = "\n<< Précédent | Suivant >>" + leastEntries.PadLeft(40);
                        }
                        else
                        {
                            fowBackStr = "\nSuivant >>" + leastEntries.PadLeft(40);
                        }

                        Console.WriteLine(fowBackStr);
                        stackLastEntry.Push(lastEntry);
                        lastEntry = hEntry - 1;
                        break;
                    }
                    else if (page > 0 && hEntry == 0)
                    {
                        Console.WriteLine("\n<< Précédent" + leastEntries.PadLeft(40));
                        stackLastEntry.Push(lastEntry);
                        lastEntry = -1;
                    }
                    else if (page == 0 && hEntry == 0)
                    {
                        stackLastEntry.Push(lastEntry);
                        lastEntry = -1;
                    }
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                string separator = new('-', Console.WindowWidth - 10);
                Console.WriteLine(separator);
                Console.WriteLine("Pour accéder au contenu complet d'un de ces éléments, " +
                                  "inscrivez le numéro d'index qui les identifie à gauche");
                Console.WriteLine("et appuyez sur ENTRÉE.");
                Console.WriteLine("Pour supprimer une entrée : appuyez sur DELETE et inscrivez son index.");
                Console.WriteLine("Utilisez BACKSPACE pour corriger et les FLÈCHES GAUCHE / DROITE pour changer de page.");
                Console.WriteLine("\nPour retourner, appuyer sur ESC.");
                Console.Write(">> ");
                Console.ForegroundColor = ConsoleColor.Cyan;

                string numeroStr = "";
                bool changedPage = false;
                while (true)
                {
                    int curTopInitial = Console.CursorTop;
                    int curLeftInitial = Console.CursorLeft;

                    ConsoleKeyInfo numero = Console.ReadKey(true);
                    if (numero.Key == ConsoleKey.Escape)
                    {
                        return;
                    }
                    else if (numero.Key == ConsoleKey.RightArrow && lastEntry >= 0)
                    {
                        changedPage = true;
                        page++;
                        break;
                    }
                    else if (numero.Key == ConsoleKey.LeftArrow && page > 0)
                    {
                        changedPage = true;
                        _ = stackLastEntry.Pop();

                        if (stackLastEntry.Count > 0)
                        {
                            lastEntry = stackLastEntry.Pop();
                            page--;
                        }
                        else
                        {
                            lastEntry = tempHist.Count - 1;
                            page = 0;
                        }

                        break;
                    }

                    if (numero.Key is >= ConsoleKey.D0 and <= ConsoleKey.D9)
                    {
                        numeroStr += ((int)numero.Key - 48).ToString();
                        Console.Write((char)numero.Key);
                    }
                    else if (numero.Key is >= ConsoleKey.NumPad0 and <= ConsoleKey.NumPad9)
                    {
                        numeroStr += ((int)numero.Key - 96).ToString();
                        Console.Write((int)(numero.Key - 96));
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

                if (!changedPage)
                {
                    // Afficher entrée
                    if (numeroStr[0] == 'D' && numeroStr.Length > 1)
                    {
                        if (!int.TryParse(numeroStr[1..], out int numeroDel))
                        {
                            numeroDel = -1;
                        }
                        int realNumero = tempHist.Count - numeroDel;
                        if (RetirerHistorique(realNumero))
                        {
                            stackReview(realNumero);
                        }

                    }
                    else if (numeroStr[0] != 'D')
                    {
                        if (!int.TryParse(numeroStr[0..], out int numeroInt))
                        {
                            numeroInt = -1;
                        }
                        int realNumero = tempHist.Count - numeroInt;
                        if (!AfficherEntree(getMainIndexByIndex(realNumero)))
                        {
                            stackReview(realNumero);
                        }
                    }
                    else
                    {
                        Display.PrintMessage("Aucune entrée à supprimer !", MessageState.Failure);
                        Console.ReadKey(true);
                    }

                    if (stackLastEntry.Count > 0)
                    {
                        lastEntry = stackLastEntry.Pop();
                    }
                }
            }

            void stackReview(int deleted)
            {
                Stack<int> tempNew = new();
                while (stackLastEntry.Count > 0)
                {
                    int old = stackLastEntry.Pop();
                    if (old >= deleted)
                    {
                        old--;
                    }

                    tempNew.Push(old);
                }
                while (tempNew.Count > 0)
                {
                    int rev = tempNew.Pop();
                    stackLastEntry.Push(rev);
                }
            }

            int getMainIndexByIndex(int index)
            {
                if (index >= 0 && index < tempHist.Count)
                {
                    uint idRetrieved = tempHist[index].ID;
                    for (int mainIndex = 0; mainIndex < Count; mainIndex++)
                    {
                        if (ListeHistorique[mainIndex].ID == idRetrieved)
                        {
                            return mainIndex;
                        }
                    }
                }
                return -1;
            }
        }

        public static bool AfficherEntree(int index)
        {
            if (index < ListeHistorique.Count && index >= 0)
            {
                Console.Clear();
                string dateOfDeciphering = ListeHistorique[index].cipherDate.ToString("dddd d MMMM yyyy");
                string hourOfDeciphering = ListeHistorique[index].cipherDate.ToString("H' h 'mm");
                string historicMsg = ListeHistorique[index].msg;
                string msgAuthor = ListeHistorique[index].author == "" ? "Inconnu" : ListeHistorique[index].author;
                PrivacyState privSta = ListeHistorique[index].pState;

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
                        Categorie.AjoutCategorieMsg(index);
                        break;
                    case ConsoleKey.Delete:
                        RetirerHistorique(index);
                        Display.PrintMessage("Message supprimé", MessageState.Warning);
                        Console.ReadKey(true);
                        return false;
                    default:
                        break;
                }
            }
            else
            {
                Console.CursorLeft = 0;
                Display.PrintMessage("Index invalide ! ", MessageState.Failure);
                Console.ReadKey(true);
            }

            return true;
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
                string[] historyLines = historyStr.Split(RECD_SEP, StringSplitOptions.RemoveEmptyEntries);
                foreach (string itemLine in historyLines)
                {
                    string[] itemEntries = itemLine.Split(UNIT_SEP);
                    string itemIDStr = "", itemDT = "", itemMsg = "", itemAuthor = "", itemPState = "";

                    if (itemEntries.Length >= 3)
                    {
                        itemIDStr = itemEntries[0];
                        itemDT = itemEntries[1];
                        itemMsg = itemEntries[2];

                        if (itemEntries.Length >= 4)
                        {
                            itemAuthor = itemEntries[3];

                            if (itemEntries.Length >= 5)
                            {
                                itemPState = itemEntries[4];
                            }
                        }
                    }

                    (uint, DateTime, string, string, PrivacyState) tupleEntry = new();
                    if (!uint.TryParse(itemIDStr, out uint itemID))
                    {
                        itemID = AssignNewId();
                    }
                    DateTime dtEntry = DateTime.Parse(itemDT);
                    PrivacyState privSta = itemPState switch
                    {
                        "1" => PrivacyState.Public,
                        "-1" => PrivacyState.Private,
                        _ => PrivacyState.NotDefined,
                    };
                    tupleEntry.Item1 = itemID;
                    tupleEntry.Item2 = dtEntry;
                    tupleEntry.Item3 = itemMsg;
                    tupleEntry.Item4 = itemAuthor;
                    tupleEntry.Item5 = privSta;
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
            foreach ((uint, DateTime, string, string, PrivacyState) item in ListeHistorique)
            {
                allHistoryStr += item.Item1.ToString() +
                    UNIT_SEP + item.Item2.ToUniversalTime().ToString("u") +
                    UNIT_SEP + item.Item3 +
                    UNIT_SEP + item.Item4 +
                    UNIT_SEP + (int)item.Item5 +
                    RECD_SEP;
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
            uint newID = AssignNewId();
            (uint, DateTime, string, string, PrivacyState) nouvEntreeTuple = (newID, dateEntree, nouvEntree, auteur, pState);
            ListeHistorique.Add(nouvEntreeTuple);
            EcrireHistorique();
        }

        public static bool RetirerHistorique(int indexEntree)
        {
            if (indexEntree < ListeHistorique.Count && indexEntree >= 0)
            {
                ListeHistorique.RemoveAt(indexEntree);
                EcrireHistorique();
                return true;
            }
            else
            {
                Console.CursorLeft = 0;
                Display.PrintMessage("Index invalide ! ", MessageState.Failure);
                Console.ReadKey(true);
                return false;
            }
        }

        private static uint AssignNewId()
        {
            List<uint> potentials = new();
            uint normal = 0;
            for (int i = 0; i < ListeHistorique.Count; i++, normal++)
            {
                uint id = ListeHistorique[i].ID;
                if (id != normal)
                {
                    if (potentials.Count > 0 && potentials[0] == id)
                    {
                        potentials.RemoveAt(0);
                        potentials.Add(normal);
                    }

                    if (id > normal)
                    {
                        for (uint j = normal; j < id; j++)
                        {
                            potentials.Add(normal);
                        }
                        normal = id;
                    }
                }
            }

            return potentials.Count > 0 ? potentials[0] : (uint)ListeHistorique.Count;
        }
    }
}
