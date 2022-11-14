using Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace LORENZ
{
    public class Categorie
    {
        public static string FichierCategories => $@"{Parametres.ParamsDirectory}/CATEGORY.LZI";
        public static List<Categorie> ListeCategories { get; set; } = new();
        public static int CategoriesCount => ListeCategories.Count;
        public string Nom { get; set; }
        private List<uint> ListeMsg { get; set; }

        public Categorie(string nom)
        {
            Nom = nom;
            ListeMsg = new();
        }

        private Categorie(string nom, List<uint> listeMsg)
        {
            Nom = nom;
            ListeMsg = listeMsg;
        }

        private bool AddMsg(uint id)
        {
            if (!ListeMsg.Contains(id))
            {
                ListeMsg.Add(id);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RemoveMsg(uint id)
        {
            for (int msg = 0; msg < ListeMsg.Count; msg++)
            {
                if (id == ListeMsg[msg])
                {
                    ListeMsg.RemoveAt(msg);
                    return true;
                }
            }

            return false;
        }

        private bool AjouterMsg(int histIndex)
        {
            if (histIndex >= 0 && histIndex < Historique.Count)
            {
                uint idRetriv = Historique.ListeHistorique[histIndex].ID;
                return AddMsg(idRetriv);
            }

            return false;
        }

        public static void MenuGeneral()
        {
            if (ListeCategories.Count > 0)
            {
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Sélectionnez une catégorie en inscrivant son index");
                    Console.WriteLine("Appuyez sur ESC pour retourner...\n");

                    for (int cat = 0; cat < ListeCategories.Count; cat++)
                    {
                        Console.WriteLine("[" + (cat + 1) + "]: " + ListeCategories[cat].Nom);
                    }

                    int indexCat = Extensions.SpecialInputDigits(addNewLine: false);
                    if (indexCat == -1)
                    {
                        return;
                    }
                    ConsulterCategorie(indexCat - 1);
                }
            }
            else
            {
                Console.Clear();
                Display.PrintMessage("Il n'y a aucune catégorie existante.", MessageState.Warning);
                Display.PrintMessage("Créez-en une nouvelle avant de continuer.", MessageState.Warning);
                Console.WriteLine("\nAppuyez sur n'importe quelle touche pour retourner...");
                Console.ReadKey(true);
            }
        }

        public static void ConsulterCategorie(int indexCat)
        {
            if (indexCat < ListeCategories.Count && indexCat >= 0)
            {
                Categorie categorieChoisie = ListeCategories[indexCat];
                if (categorieChoisie.ListeMsg.Count == 0)
                {
                    Console.Clear();
                    Display.PrintMessage("La catégorie " + categorieChoisie.Nom + " ne contient aucun message.", MessageState.Warning);
                    Console.ReadKey(true);
                    return;
                }

                // Afficher les messages de la catégorie
                List<uint> sel = categorieChoisie.ListeMsg;
                string titre = "Catégorie " + categorieChoisie.Nom;
                Historique.AfficherHistorique(titre, sel);
                // Afficher les options de modification...
            }
            else
            {
                Console.CursorLeft = 0;
                Display.PrintMessage("Index invalide ! ", MessageState.Failure);
                Console.ReadKey(true);
            }
        }

        public static void NouvelleCategorie(int msgIndex = -1)
        {
            Console.Clear();
            Console.WriteLine("Création d'une nouvelle catégorie");
            Display.PrintMessage("Pour annuler, appuyez sur ESC, ou sur ENTRÉE sans rien écrire.\n", MessageState.Warning);
            Console.Write("Nom de la nouvelle catégorie : ");
            string newCatName = Extensions.SpecialInput();
            if (newCatName == null || newCatName == "")
            {
                return;
            }

            Categorie newCategory = new(newCatName);
            ListeCategories.Add(newCategory);
            EcrireFichierCategories();
            Display.PrintMessage("Catégorie " + newCatName + " créée avec succès !", MessageState.Success);

            if (msgIndex == -1)
            {
                Console.WriteLine("\nPour ajouter des entrées :");
                Console.WriteLine("1. Accédez à l'un d'eux dans l'historique principal;");
                Console.WriteLine("2. Appuyez sur C puis choisissez la catégorie correspondante.");
            }
            /*else
            {
                newCategory.AjoutMsg(msgIndex);
                Display.PrintMessage("Message ajouté avec succès !", MessageState.Success);
            }*/

            Console.WriteLine("\nAppuyez sur n'importe quelle touche pour terminer...");
            Console.ReadKey(true);
        }

        public static void AjoutCategorieMsg(int histIndex)
        {
            Console.WriteLine("Sélectionnez la catégorie dans laquelle vous désirez placer le message :\n");
            for (int i = 0; i < ListeCategories.Count; i++)
            {
                Console.WriteLine("[" + (i + 1) + "]: " + ListeCategories[i].Nom);
            }

            Console.WriteLine("Appuyez sur ESC pour annuler...");
            int indexTyped;
            do
            {
                indexTyped = Extensions.SpecialInputDigits();
                int indexCat = indexTyped - 1;
                if (indexCat >= 0 && indexCat < ListeCategories.Count)
                {
                    if (!ListeCategories[indexCat].AjouterMsg(histIndex))
                    {
                        Display.PrintMessage("Le message que vous essayez d'ajouter", MessageState.Warning);
                        Display.PrintMessage("a déjà été ajouté dans la catégorie sélectionnée.", MessageState.Warning);
                        Display.PrintMessage("Choisissez une autre catégorie pour poursuivre l'opération.", MessageState.Warning);
                    }
                    else
                    {
                        EcrireFichierCategories();
                        Display.PrintMessage("Message ajouté avec succès !", MessageState.Success);
                        Display.PrintMessage("Appuyez sur n'importe quelle touche pour terminer...", MessageState.Warning);
                        Console.ReadKey(true);
                        break;
                    }
                    
                }
                else if (indexTyped != -1)
                {
                    Display.PrintMessage("Index invalide ! ", MessageState.Failure);
                }

            } while (indexTyped != -1);
        }

        public static bool LireFichierCategories()
        {
            try
            {
                if (!File.Exists(FichierCategories))
                {
                    return false;
                }

                Decyphering.OpeningDecyphering(FichierCategories, out uint[] cipherKey, out uint[] value);
                Cryptographie.CreateMatrix(ref cipherKey, -22);
                Common.XORPassIntoMessage(cipherKey, ref value);
                Cryptographie.CreateMatrix(ref cipherKey, 21);
                Common.XORPassIntoMessage(cipherKey, ref value);

                string categoriesStr = "";
                foreach (uint bItem in value)
                {
                    categoriesStr += (char)(bItem & 0xFF);
                }

                ListeCategories.Clear();
                string[] categoriesLines = categoriesStr.Split(Historique.RECD_SEP, StringSplitOptions.RemoveEmptyEntries);
                foreach (string catLine in categoriesLines)
                {
                    string[] catEntries = catLine.Split(Historique.UNIT_SEP);
                    string catName = catEntries[0];
                    List<uint> msgList = new();
                    if (catEntries.Length > 1)
                    {
                        string[] ids = catEntries[1].Split(';', StringSplitOptions.RemoveEmptyEntries);
                        foreach (string id in ids)
                        {
                            if (uint.TryParse(id, out uint val))
                            {
                                msgList.Add(val);
                            }
                        }
                    }
                    
                    ListeCategories.Add(new(catName, msgList));
                }
            }
            catch (CryptographyException)
            {
                Console.WriteLine("Un problème est survenu lors de la lecture du fichier des catégories.");
                Console.WriteLine("Il est possible que le fichier ait été supprimé, déplacé ou corrompu.");
            }

            return true;
        }

        private static void EcrireFichierCategories()
        {
            Common.CphrMode = CypherMode.x1;
            string allCategoriesStr = "";
            foreach (Categorie catItem in ListeCategories)
            {
                allCategoriesStr += catItem.Nom.ToString() + Historique.UNIT_SEP;
                foreach (uint IDs in catItem.ListeMsg)
                {
                    allCategoriesStr += IDs.ToString() + ';';
                }
                allCategoriesStr += Historique.RECD_SEP;
            }

            uint[] allCategoriesUInt = Encryption.ToUIntArray(allCategoriesStr);
            for (int i = 0; i < allCategoriesUInt.Length; i++)
            {
                byte[] filling = new byte[3];
                RandomNumberGenerator.Create().GetBytes(filling);
                uint filling3Bytes = 0;
                for (int b = 0; b < filling.Length; b++)
                {
                    filling3Bytes += (uint)filling[b] << (8 * b);
                }

                allCategoriesUInt[i] += filling3Bytes << 8;
            }

            uint[] cipherKey = new uint[Common.KeyNbrUInt];
            Cryptography.Random.RandomGeneratedNumberQb(ref cipherKey);
            Common.XORPassIntoMessage(cipherKey, ref allCategoriesUInt);
            Cryptographie.CreateMatrix(ref cipherKey, 21);
            Common.XORPassIntoMessage(cipherKey, ref allCategoriesUInt);
            Cryptographie.CreateMatrix(ref cipherKey, 22);
            Encryption.ClosingCyphering(cipherKey, ref allCategoriesUInt);
            Encryption.WriteCypherIntoFile(allCategoriesUInt, FichierCategories);
        }
    }
}
