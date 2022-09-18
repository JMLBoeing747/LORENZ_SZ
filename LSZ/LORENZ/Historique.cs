using Cryptography;
using System;
using System.Collections.Generic;
using System.IO;

namespace LORENZ
{
    public static class Historique
    {
        public static string FichierHistorique => $@"{Parametres.ParamsDirectory}/HISTORY.LZI";
        public static List<(DateTime, string)> TableauHistorique { get; private set; }

        public static void AfficherHistorique()
        {
            if (!LireHistorique())
            {
                Console.WriteLine("L'historique est vide. Sauvegardez des messages déchiffrés pour le remplir et");
                Console.WriteLine("revenez voir.");
                return;
            }


        }

        public static bool LireHistorique()
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
            foreach ((DateTime, string) item in TableauHistorique)
            {
                allHistoryStr += item.Item1.ToString("dd-MM-yyyy hh:mm:ss") + ": " + item.Item2 + "\n";
            }

            uint[] allHistoryUInt = Encryption.ToUIntArray(allHistoryStr);
            uint[] cipherKey = new uint[Common.KeyNbrUInt];
            Cryptography.Random.RandomGeneratedNumberQb(ref cipherKey);
            Common.XORPassIntoMessage(cipherKey, ref allHistoryUInt);
            Common.NotOperationToKey(ref cipherKey);
            Common.ReverseKey(ref cipherKey);
            Common.XORPassIntoMessage(cipherKey, ref allHistoryUInt);
            Encryption.FinalizeCypherMessage(cipherKey, ref allHistoryUInt);
            Encryption.WriteCypherIntoFile(allHistoryUInt, FichierHistorique);
        }

        public static void AjouterHistorique(string nouvEntree)
        {
            (DateTime, string) nouvEntreeTuple = (DateTime.Now, nouvEntree);
            TableauHistorique.Add(nouvEntreeTuple);
        }
    }
}
