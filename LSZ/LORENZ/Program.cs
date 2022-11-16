using Cryptography;
using System;
using System.IO;

namespace LORENZ
{
    class Program
    {
        public static string VersionNumber => "3.0.0-alpha";
        private static bool OverridePress { get; set; }

        public static void Main()
        {
            Console.Clear();
            Console.Title = "LORENZ SCHLÜSSELZUSATZ " + VersionNumber;
            try
            {
                Parametres.VerifierParametres();
                Demarrage();
            }
            catch (LORENZException) { }
            Console.ResetColor();
            Console.Clear();
        }

        private static void Demarrage()
        {
            Display.PrintMessage("Initialisation des composants...", MessageState.Info);
            Parametres.LireGeneralParamsFile();
            double argent = Jeux.ReadCoinsInfoFile();
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("DER LORENZ SCHLÜSSELZUSATZ " + VersionNumber);
            while (true)
            {
                Extensions.Configuration();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Choisir une option :");
                Console.WriteLine("1 : Chiffrement");
                Console.WriteLine("2 : Déchiffrement");
                Console.WriteLine("L : LID");
                Console.WriteLine("J : Jeu");
                Console.WriteLine("H : Historique");

                Console.WriteLine();
                if (!Parametres.ShowPseudoNameSender)
                {
                    Console.WriteLine("[ ] Afficher l'expéditeur (S)");
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("[X] Afficher l'expéditeur (S)");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                if (!Compression.CompressionActive)
                {
                    Console.WriteLine("[ ] Activer la compression (C)");
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("[X] Activer la compression (C)");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }

                Console.WriteLine("\nP : Modifier le pseudo");
                if (Parametres.CipherFileDirectory != null)
                {
                    Console.WriteLine("R : Modifier le répertoire des chiffrements");
                }
                else
                {
                    Display.PrintMessage("R : Spécifier le répertoire des chiffrements", MessageState.Warning);
                }
                Console.WriteLine("O : Options de chiffrement");
                Console.WriteLine("\nF1 : AIDE\n");
                Console.WriteLine("Pour quitter, appuyez sur ESC");
                if (argent > 0.00)
                {
                    Console.WriteLine(Environment.NewLine + "Votre solde : " + argent + " Coins");
                }

                ConsoleKeyInfo saisie = Console.ReadKey(true);

                switch (saisie.Key)
                {
                    case ConsoleKey.D1:
                        ChiffrerLeMessage();
                        break;
                    case ConsoleKey.NumPad1:
                        ChiffrerLeMessage();
                        break;
                    case ConsoleKey.D2:
                        DechiffrerLeMessage();
                        break;
                    case ConsoleKey.NumPad2:
                        DechiffrerLeMessage();
                        break;
                    case ConsoleKey.L:
                        AfficherLID();
                        break;
                    case ConsoleKey.J:
                        Jeux.TheGame(ref argent);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Clear();
                        continue;
                    case ConsoleKey.H:
                        MenuHistorique();
                        continue;
                    case ConsoleKey.S:
                        Parametres.ShowPseudoNameSender = !Parametres.ShowPseudoNameSender;
                        Parametres.EcrireGeneralParamsFile();
                        Console.Clear();
                        if (Parametres.ShowPseudoNameSender)
                        {
                            Display.PrintMessage("Expéditeur AFFICHÉ", MessageState.Warning);
                        }
                        else
                        {
                            Display.PrintMessage("Expéditeur MASQUÉ", MessageState.Warning);
                        }
                        continue;
                    case ConsoleKey.C:
                        Compression.CompressionActive = !Compression.CompressionActive;
                        Parametres.EcrireGeneralParamsFile();
                        Console.Clear();
                        if (Compression.CompressionActive)
                        {
                            Display.PrintMessage("Compression ACTIVÉE", MessageState.Warning);
                        }
                        else
                        {
                            Display.PrintMessage("Compression DÉSACTIVÉE", MessageState.Warning);
                        }
                        continue;
                    case ConsoleKey.P:
                        ChangerLePseudo();
                        Console.Clear();
                        continue;
                    case ConsoleKey.R:
                        Console.Clear();
                        OverridePress = !Extensions.SetCipherFileDirectory();
                        break;
                    case ConsoleKey.O:
                        MenuOptions();
                        continue;
                    case ConsoleKey.F1:
                        AfficherAide();
                        continue;
                    case ConsoleKey.Escape:
                        break;
                    case ConsoleKey.Enter:
                        /* La gestion de la touche ENTRÉE est gardée pour permettre une meilleure transition
                         * vers la touche ESC auprès des anciens utilisateurs */
                        Console.Clear();
                        Display.PrintMessage("Pour quitter, appuyez sur ESC.", MessageState.Warning, resetColors: true);
                        continue;
                    case ConsoleKey.A:
                        Extensions.Music();
                        OverridePress = true;
                        break;
                    default:
                        Console.Clear();
                        Display.PrintMessage("Ceci n'est pas une option valide.", MessageState.Failure);
                        continue;
                }

                if (saisie.Key == ConsoleKey.Escape)
                {
                    break;
                }

                if (!OverridePress)
                {
                    Console.WriteLine(Environment.NewLine + "Press any key to continue...");
                    Console.ReadKey(true);
                }

                OverridePress = false;
                Console.Clear();
            }

            Display.PrintMessage("Fermeture en cours...", MessageState.Info);
        }

        private static void AfficherLID()
        {
            Console.Clear();
            Console.WriteLine("Le LID permet de vous identifier de façon unique envers tous les utilisateurs LORENZ.");
            Console.WriteLine("Vous avez besoin de connaître le LID de votre correspondant si vous désirez lui envoyer un");
            Console.WriteLine("message en privé. Le LID ne peut pas être modifié car il est propre à chaque clé de produit.");
            Console.WriteLine("Voici votre LID de six caractères :\n");
            Console.WriteLine(">>> " + Parametres.LID + " <<<");
        }

        private static void ChangerLePseudo()
        {
            Console.Clear();
            Console.WriteLine("Le pseudo est le nom qui sera affiché à tous les récepteurs pouvant déchiffrer le message");
            Console.WriteLine("et qui ont activé l'affichage de l'expéditeur. Si vous désirez le modifier, tapez");
            Console.WriteLine("ci-dessous le nouveau pseudo à utiliser pour les chiffrements futurs. Si vous désirez le");
            Console.WriteLine("réinitialiser, taper tel quel votre nom d'utilisateur système ou $DEFAULT (la casse");
            Console.WriteLine("n'importe pas).");
            Console.WriteLine("\nPour annuler, appuyer sur ESC ou sur ENTRÉE sans rien écrire.");

            Console.WriteLine("\nActuel : " + Parametres.PseudoName);
            Console.Write("Nouveau >>> ");
            string newPseudo = Extensions.SpecialInput();
            if (newPseudo is null or "")
            {
                return;
            }

            if (newPseudo == Parametres.PseudoName)
            {
                Display.PrintMessage("Pseudo identique au précédent. Aucun changement à faire.", MessageState.Warning);
            }
            else if (newPseudo == Environment.UserName || newPseudo.ToUpper() == "$DEFAULT")
            {
                Parametres.PseudoName = Environment.UserName;
                Parametres.EcrireGeneralParamsFile();
                Display.PrintMessage("Valeur par défaut réinitialisée", MessageState.Warning);
            }
            else
            {
                Parametres.PseudoName = newPseudo;
                Parametres.EcrireGeneralParamsFile();
                Display.PrintMessage("Nouveau pseudo enregistré !", MessageState.Success);
            }
            Display.PrintMessage("Appuyez sur une touche pour continuer...", MessageState.Warning);
            Console.ReadKey(true);
        }

        private static void AfficherAide()
        {
            try
            {
                if (System.IO.File.Exists(Parametres.HelpFilePath))
                {
                    System.Diagnostics.Process.Start("hh.exe", Parametres.HelpFilePath);
                }
                else
                {
                    throw new System.ComponentModel.Win32Exception();
                }

                Console.Clear();
                Display.PrintMessage("OUVERTURE DU FICHIER D'AIDE", MessageState.Success);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Console.Clear();
                Display.PrintMessage("OUVERTURE ÉCHOUÉE !", MessageState.Failure);
            }
        }

        private static void ChiffrerLeMessage()
        {
            Console.Clear();
            Console.WriteLine("Preparation...");

            // Génération du GK et de la TableCode correspondante
            string strGeneralKey;
            string[,] theTableCode;
            do
            {
                strGeneralKey = Algorithmes.GeneratorGK();
                theTableCode = Algorithmes.GenerateTableCode(strGeneralKey);
            } while (theTableCode == null);

            // Demande d'écriture du message
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n           CHIFFREMENT           \n");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Cyan;
            string messageOriginal;
            while (true)
            {
                Display.PrintMessage("AVIS : Vous pouvez écrire sur plusieurs paragraphes !", MessageState.Warning);
                Console.WriteLine("Pour annuler, appuyez sur ESC.");
                Console.WriteLine("Pour terminer le message, appuyez sur CTRL + D.");
                Console.WriteLine("Entrez le texte à chiffrer :");
                messageOriginal = Extensions.SpecialInput('\x04');

                if (messageOriginal != null)
                {
                    if (string.IsNullOrWhiteSpace(messageOriginal))
                    {
                        Display.PrintMessage("Aucun texte détecté.", MessageState.Failure);
                    }
                    else
                    {
                        messageOriginal += '\n';
                        break;
                    }
                }
                else
                {
                    OverridePress = true;
                    return;
                }
            }

            string messageChiffre = Algorithmes.Chiffrement(messageOriginal, strGeneralKey, theTableCode);
            string vraiMessageChiffre = Algorithmes.SecondChiffrement(messageChiffre);
            if (vraiMessageChiffre.Length > 4094)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Ce chiffrement contient plus de 4094 caractères.");
                Console.WriteLine("Vous devrez utiliser le fichier de chiffrement nouvellement généré pour transmettre\n" +
                    "votre message, faute de quoi votre correspondant ne pourra pas le déchiffrer.\n");
                Console.Write("Donnez un nom au fichier de chiffrement : ");

                string cipherFileName;
                do
                {
                    cipherFileName = Console.ReadLine();
                    if (cipherFileName == "")
                    {
                        Display.PrintMessage("Aucun nom spécifié. Opération annulée.", MessageState.Failure);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        return;
                    }
                } while (!Extensions.EcrireChiffrementLong(vraiMessageChiffre, cipherFileName));

                Console.WriteLine("Nom du fichier : " + Extensions.GetNomFichierChiffrement());
                Console.ForegroundColor = ConsoleColor.Cyan;
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Le message chiffré :");
            Console.WriteLine(vraiMessageChiffre);
            Console.ForegroundColor = ConsoleColor.Cyan;
        }

        private static void DechiffrerLeMessage()
        {
            //Demande d'écriture du message chiffré
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n          DÉCHIFFREMENT          \n");
            Console.ResetColor();
            Display.PrintMessage("AVIS : Le texte chiffré peut s'étendre sur plusieurs lignes !", MessageState.Warning);
            Console.ForegroundColor = ConsoleColor.Cyan;

            try
            {
                while (true)
                {
                    Console.WriteLine("Pour annuler, appuyez sur ESC.");
                    Console.WriteLine("Pour terminer, appuyez sur CTRL + D.");
                    Console.WriteLine("Entrez le texte à déchiffrer :");
                    string messageADechiffrer = Extensions.SpecialInput('\x04');

                    if (messageADechiffrer == null)
                    {
                        OverridePress = true;
                        return;
                    }

                    // Retrait des retours à la ligne
                    string[] multiLines = messageADechiffrer.Split('\n');
                    messageADechiffrer = default;
                    for (int i = 0; i < multiLines.Length; i++)
                    {
                        messageADechiffrer += multiLines[i];
                    }

                    if (messageADechiffrer.StartsWith("FILE:", StringComparison.OrdinalIgnoreCase))
                    {
                        string cipherFileName = messageADechiffrer["FILE:".Length..].Trim();
                        string cipherFilePath = Parametres.CipherFileDirectory + cipherFileName;
                        if (File.Exists(cipherFilePath))
                        {
                            messageADechiffrer = File.ReadAllText(cipherFilePath);
                        }
                        else
                        {
                            string localCipherFilePath = cipherFileName;
                            if (File.Exists(localCipherFilePath))
                            {
                                messageADechiffrer = File.ReadAllText(localCipherFilePath);
                            }
                            else
                            {
                                Display.PrintMessage("Le fichier \""
                                                     + cipherFileName
                                                     + "\" n'existe pas dans le répertoire de chiffrement.", MessageState.Failure);
                                Display.PrintMessage("Le répertoire de chiffrement spécifié est : " + Parametres.CipherFileDirectory,
                                                     MessageState.Warning);
                                Console.WriteLine("Entrez un nouveau chemin d'accès ou un chiffrement valide,");
                                Console.WriteLine("ou appuyez sur ESC pour quitter.");
                                continue;
                            }
                        }
                    }

                    //Validité du chiffrement complet
                    if (!TestCipher(messageADechiffrer))
                    {
                        Display.PrintMessage("\nCe message n'est pas valide.\n", MessageState.Failure);
                        continue;
                    }
                    //Déchiffrement premier
                    string messageDechiffre1 = Algorithmes.DechiffrementPremier(messageADechiffrer, out bool isGoodCS);
                    //Validité déchiffrement premier total
                    if (messageDechiffre1 == null)
                    {
                        Display.PrintMessage("\nCe message n'est pas valide.\n", MessageState.Failure);
                        continue;
                    }

                    //Extraction du GK
                    string strGeneralKey = Algorithmes.DeveloppGK(messageDechiffre1);

                    //Génération de la Table Code
                    string[,] theTableCode = Algorithmes.GenerateTableCode(strGeneralKey);
                    // Vérification si GK mauvais car répétitions de trans
                    if (theTableCode == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Environment.NewLine + "ERREUR FATALE CODE 55 : Le chiffrement n'est pas authentique.");
                        Display.PrintMessage("\nCe message n'est pas valide.\n", MessageState.Failure);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        continue;
                    }

                    // Déchiffrement second
                    string messageDechiffreComplet = Algorithmes.DechiffrementSecond(theTableCode, strGeneralKey, messageDechiffre1);
                    Console.ForegroundColor = ConsoleColor.White;
                    if (!Algorithmes.IsThePrivateReceiver && !Algorithmes.IsThePrivateSender)
                    {
                        // Si le message n'est pas destiné au bon utilisateur et que ce n'est pas son auteur
                        Display.PrintMessage("LORENZ WARNING MSG: CE MESSAGE NE VOUS EST PAS DESTINÉ.", MessageState.Warning);
                        Algorithmes.IsPrivateMessage = false;
                    }
                    else
                    {
                        PrivacyState msgPrivState;
                        if (!Algorithmes.IsPrivateMessage)
                        {
                            msgPrivState = PrivacyState.Public;
                            Console.WriteLine(Environment.NewLine + "Message déchiffré :");
                        }
                        else
                        {
                            msgPrivState = PrivacyState.Private;

                            if (Algorithmes.IsThePrivateReceiver)
                            {
                                Console.WriteLine(Environment.NewLine + "Message déchiffré (EN PRIVÉ):");
                            }
                            else
                            {
                                Console.WriteLine(Environment.NewLine + "Message déchiffré (PRIVÉ PAR VOUS POUR " + Algorithmes.ThePrivateReceiverLID + "):");
                            }

                            Algorithmes.IsPrivateMessage = false;
                        }

                        if (Parametres.ShowPseudoNameSender)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkGreen;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(Algorithmes.SenderPseudoName + " :");
                            Console.ResetColor();
                            Console.Write(" " + messageDechiffreComplet);
                        }
                        else
                        {
                            Console.Write(messageDechiffreComplet);
                        }

                        Extensions.AfficherMarqueurFin();

                        if (!isGoodCS)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(Environment.NewLine + "ATTENTION ! LE MESSAGE PEUT CONTENIR DES DÉFORMATIONS");
                            Console.WriteLine("DUE À UNE MODIFICATION ACCIDENTELLE OU MALINTENTIONNÉE DE VALEURS DANS LE CHIFFREMENT.");
                            Console.ForegroundColor = ConsoleColor.Cyan;
                        }

                        DateTime dateTimeDechiff = DateTime.Now;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("\n[S]: Sauvegarder le présent message\n");
                        Console.WriteLine("Appuyez sur toute autre touche pour retourner au menu principal...");
                        ConsoleKeyInfo saisie = Console.ReadKey(true);
                        if (saisie.Key == ConsoleKey.S)
                        {
                            Historique.AjouterHistorique(messageDechiffreComplet,
                                                         dateTimeDechiff,
                                                         Algorithmes.SenderPseudoName,
                                                         msgPrivState);
                            Display.PrintMessage("Message sauvegardé !", MessageState.Success);
                        }
                        else
                        {
                            OverridePress = true;
                        }
                    }

                    return;
                }
            }
            catch (LORENZException le)
            {
                if (le.Err == ErrorCode.E0x00)
                {
                    throw new LORENZException(ErrorCode.E0x00, false);
                }

                OverridePress = true;
                return;
            }
        }

        private static bool TestCipher(string messageToTest)
        {
            bool spacesInMessage = messageToTest.Contains(' ');
            if (messageToTest.Length <= 38 || messageToTest.Length % 4 != 0 || spacesInMessage)
            {
                if (spacesInMessage)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Environment.NewLine + "TYPING BREAK ERROR : SPACES DETECTED.");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }

                return false;
            }

            return true;
        }

        private static void MenuHistorique()
        {
            Console.Clear();
            if (Categorie.CategoriesCount == 0)
            {
                Display.PrintMessage("Lecture de CATEGORY.LZI...", MessageState.Info);
                Categorie.LireFichierCategories();
            }

            Console.Clear();
            if (Historique.Count == 0)
            {
                Display.PrintMessage("Lecture de HISTORY.LZI...", MessageState.Info);
                if (!Historique.LireFichierHistorique())
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
                else
                {
                    
                    Console.Clear();
                }
            }

            while (true)
            {
                Console.WriteLine("===== Historique =====\n");
                Console.WriteLine("[H]: Consulter l'historique");
                if (Categorie.CategoriesCount > 0)
                {
                    Console.WriteLine("[C]: Consulter les catégories");
                    Console.WriteLine("[S]: Supprimer une catégorie");
                }
                Console.WriteLine("[N]: Nouvelle catégorie");
                Console.WriteLine("\nAppuyez sur ESC pour retourner");

                ConsoleKeyInfo saisie = Console.ReadKey(true);
                switch (saisie.Key)
                {
                    case ConsoleKey.H:
                        Historique.AfficherHistorique();
                        Console.Clear();
                        break;
                    case ConsoleKey.C:
                        if (Categorie.CategoriesCount > 0)
                        {
                            Categorie.MenuGeneral();
                            Console.Clear();
                        }
                        else
                        {
                            Console.Clear();
                            Display.PrintMessage("Ceci n'est pas une touche valide.", MessageState.Failure);
                        }
                        break;
                    case ConsoleKey.S:
                        if (Categorie.CategoriesCount > 0)
                        {
                            Categorie.MenuSuppression();
                            Console.Clear();
                        }
                        else
                        {
                            Console.Clear();
                            Display.PrintMessage("Ceci n'est pas une touche valide.", MessageState.Failure);
                        }
                        break;
                    case ConsoleKey.N:
                        Categorie.NouvelleCategorie();
                        Console.Clear();
                        break;
                    case ConsoleKey.Escape:
                        Console.Clear();
                        return;
                    default:
                        Console.Clear();
                        Display.PrintMessage("Ceci n'est pas une touche valide.", MessageState.Failure);
                        break;
                }
            }
        }

        private static void MenuOptions()
        {
            Console.Clear();
            while (true)
            {
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine("\n      Options de chiffrement     \n");
                Console.ResetColor();
                Console.WriteLine("C : Modifier le taux de compression");
                Console.WriteLine("T : Disposition de la table de transcription");
                Console.WriteLine("S : Disposition de la table secrète");
                Console.WriteLine("\nAppuyez sur ESC pour retourner");

                ConsoleKeyInfo saisie = Console.ReadKey(true);
                switch (saisie.Key)
                {
                    case ConsoleKey.C:
                        Compression.ModifierTaux();
                        Console.Clear();
                        break;
                    case ConsoleKey.T:
                        Algorithmes.SetTransTable();
                        Console.Clear();
                        break;
                    case ConsoleKey.S:
                        Algorithmes.SetSecretTable();
                        Console.Clear();
                        break;
                    case ConsoleKey.Escape:
                        Console.Clear();
                        return;
                    default:
                        Console.Clear();
                        Display.PrintMessage("Ceci n'est pas une touche valide.", MessageState.Failure);
                        break;
                }
            }
        }
    }
}
