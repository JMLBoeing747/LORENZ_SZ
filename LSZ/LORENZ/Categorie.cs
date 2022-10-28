using Cryptography;
using System;
using System.Collections.Generic;

namespace LORENZ
{
    public class Categorie
    {
        public static List<Categorie> ListeCategories { get; set; } = new();
        public static int CategoriesCount => ListeCategories.Count;
        public string Nom { get; set; }
        private List<uint> ListeMsg { get; set; }

        public Categorie(string nom)
        {
            Nom = nom;
            ListeMsg = new();
        }

        private void AddMsg(uint id)
        {
            ListeMsg.Add(id);
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

        private bool AjoutMsg(int histIndex)
        {
            if (histIndex >= 0 && histIndex < Historique.Count)
            {
                uint idRetriv = Historique.ListeHistorique[histIndex].ID;
                AddMsg(idRetriv);
                return true;
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
                Console.Clear();
                Console.WriteLine("Catégorie " + ListeCategories[indexCat].Nom);
                Console.WriteLine();

                // Afficher les messages de la catégorie...
                // Afficher les options de modification...

                Console.ReadKey(true);
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
            Display.PrintMessage("Catégorie " + newCatName + " créée avec succès !", MessageState.Success);

            if (msgIndex == -1)
            {
                Console.WriteLine("\nPour ajouter des entrées :");
                Console.WriteLine("1. Accédez à l'un d'eux dans l'historique principal;");
                Console.WriteLine("2. Appuyez sur C puis choisissez la catégorie correspondante.");
            }
            else
            {
                newCategory.AjoutMsg(msgIndex);
                Display.PrintMessage("Message ajouté avec succès !", MessageState.Success);
            }

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
            int indexCat = -1;
            // add special print digit

            if (indexCat >= 0 && indexCat < ListeCategories.Count)
            {
                ListeCategories[indexCat].AjoutMsg(histIndex);
            }
        }

        private static void LireFichierCategories()
        {

        }

        private static void EcrireFichierCategories()
        {

        }
    }
}
