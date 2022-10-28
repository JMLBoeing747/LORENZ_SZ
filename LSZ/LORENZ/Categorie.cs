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

        public static void ConsulterCategorie(int indexCat)
        {
            if (indexCat < ListeCategories.Count && indexCat >= 0)
            {
                Console.Clear();
                Console.WriteLine("Catégorie " + ListeCategories[indexCat].Nom);
                Console.WriteLine();

                // afficher les messages de la catégorie...
            }
            else
            {
                Console.CursorLeft = 0;
                Display.PrintMessage("Index invalide ! ", MessageState.Failure);
                Console.ReadKey(true);
            }
        }

        public static void MenuGeneral()
        {
            Console.Clear();
            if (ListeCategories.Count > 0)
            {
                Console.WriteLine("Sélectionnez une catégorie en inscrivant son index");
                Console.WriteLine("Appuyez sur ESC pour retourner...\n");

                for (int cat = 0; cat < ListeCategories.Count; cat++)
                {
                    Console.WriteLine("[" + (cat + 1) + "]: " + ListeCategories[cat].Nom);
                }

                int curTopInitial = Console.CursorTop;
                int curLeftInitial = Console.CursorLeft;
                string indexCatStr = default;
                while (true)
                {
                    ConsoleKeyInfo numero = Console.ReadKey();
                    if (numero.Key == ConsoleKey.Escape)
                    {
                        return;
                    }

                    if (numero.Key is >= ConsoleKey.D0 and <= ConsoleKey.D9)
                    {
                        indexCatStr += ((int)numero.Key - 48).ToString();
                        Console.Write((char)numero.Key);
                    }
                    else if (numero.Key is >= ConsoleKey.NumPad0 and <= ConsoleKey.NumPad9)
                    {
                        indexCatStr += ((int)numero.Key - 96).ToString();
                        Console.Write((int)(numero.Key - 96));
                    }
                    else if (numero.Key == ConsoleKey.Backspace && curLeftInitial > 3)
                    {
                        Console.SetCursorPosition(curLeftInitial - 1, curTopInitial);
                        Console.Write(' ');
                        Console.SetCursorPosition(curLeftInitial - 1, curTopInitial);

                        indexCatStr = indexCatStr[..(indexCatStr.Length - 1)];
                    }
                    else if (numero.Key == ConsoleKey.Enter)
                    {
                        if (indexCatStr.Length != 0)
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
            }
            else
            {
                Display.PrintMessage("Il n'y a aucune catégorie existante.", MessageState.Warning);
                Display.PrintMessage("Créez-en une nouvelle avant de continuer.", MessageState.Warning);
                Console.WriteLine("\nAppuyez sur n'importe quelle touche pour retourner...");
                Console.ReadKey(true);
            }
        }

        public static void NouvelleCategorie(int msgIndex = -1)
        {
            Console.Clear();
            Console.WriteLine("Création d'une nouvelle catégorie\n");
            Console.Write("Nom de la catégorie : ");
            string newCatName = Console.ReadLine();
            ListeCategories.Add(new(newCatName));
            Display.PrintMessage("Catégorie " + newCatName + " créée avec succès !", MessageState.Success);

            if (msgIndex == -1)
            {
                Console.WriteLine("\nPour ajouter des entrées, accédez à l'un d'eux dans l'historique principal,");
                Console.WriteLine("appuyez sur C puis choisissez la catégorie correspondante.");
                Console.WriteLine("\nAppuyez sur n'importe quelle touche pour terminer...");
                Console.ReadKey(true);
            }
            else
            {

            }
        }

        public static void MenuAjoutMsg(int histIndex)
        {
            Console.WriteLine("Sélectionnez la catégorie dans laquelle vous désirez placer le message :\n");
            for (int i = 0; i < ListeCategories.Count; i++)
            {
                Console.WriteLine("[" + (i + 1) + "]: " + ListeCategories[i].Nom);
            }

            Console.WriteLine("Appuyez sur ESC pour annuler...");
            // add special print digit
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

        private static void LireFichierCategories()
        {

        }

        private static void EcrireFichierCategories()
        {

        }
    }
}
