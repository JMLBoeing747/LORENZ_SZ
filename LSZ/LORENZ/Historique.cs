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
            if (ListeHistorique.Count == 0 && !LireFichierHistorique())
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("L'historique est vide.");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Display.PrintMessage("\nSauvegardez des messages déchiffrés pour le remplir et revenez y jeter un coup d'oeil.",
                                     MessageState.Warning);
                return;
            }

            for (int hEntry = 0; hEntry < ListeHistorique.Count; hEntry++)
            {
                string dtStr = ListeHistorique[hEntry].Item1.ToString("G");
                string excerpt = ListeHistorique[hEntry].Item2.Replace('\n', ' ');

                int lineLenMax = Console.WindowWidth - 13 - "[x]:dd-MM-yyyy HH:mm:ss : ".Length;
                if (excerpt.Length > lineLenMax)
                {
                    excerpt = excerpt.Substring(0, lineLenMax) + "...";
                }

                Console.WriteLine($"[{hEntry + 1}]:" + dtStr + " : " + excerpt);
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
