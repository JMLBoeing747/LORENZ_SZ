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
            double Argent = Jeux.ReadCoinsInfoFile();
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
                if (!Parametres.ShowPseudoNameSender)
                {
                    Console.WriteLine("S : Afficher l'expéditeur");
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("S : Masquer l'expéditeur");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                Console.WriteLine("H : Historique");

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
                if (Argent > 0.00)
                {
                    Console.WriteLine(Environment.NewLine + "Votre solde : " + Argent + " Coins");
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
                        double NewArgent = Jeux.TheGame(Argent);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Clear();
                        Argent = NewArgent;
                        continue;
                    case ConsoleKey.S:
                        Parametres.ShowPseudoNameSender = !Parametres.ShowPseudoNameSender;
                        Parametres.WriteGeneralParamsFile();
                        Console.Clear();
                        if (Parametres.ShowPseudoNameSender)
                        {
                            Display.PrintMessage("EXPÉDITEUR AFFICHÉ", MessageState.Warning);
                        }
                        else
                        {
                            Display.PrintMessage("EXPÉDITEUR MASQUÉ", MessageState.Warning);
                        }
                        continue;
                    case ConsoleKey.H:
                        MenuHistorique();
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
            Console.WriteLine("n'importe pas). Sinon, appuyer sur ENTRÉE sans rien écrire.");

            Console.WriteLine("Actuel : " + Parametres.PseudoName);
            Console.Write("Nouveau >>> ");
            string newPseudo = Console.ReadLine();
            if (newPseudo == "")
            {
                return;
            }
            else if (newPseudo == Parametres.PseudoName)
            {
                Display.PrintMessage("Pseudo identique au précédent. Aucun changement à faire.", MessageState.Warning);
            }
            else if (newPseudo == Environment.UserName || newPseudo.ToUpper() == "$DEFAULT")
            {
                Parametres.PseudoName = Environment.UserName;
                Parametres.WriteGeneralParamsFile();
                Display.PrintMessage("Valeur par défaut réinitialisée", MessageState.Warning);
            }
            else
            {
                Parametres.PseudoName = newPseudo;
                Parametres.WriteGeneralParamsFile();
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
            string StrGeneralKey = Algorithmes.GeneratorGK();
            string[,] TheTableCode = Algorithmes.GenerateTableCode(StrGeneralKey);
            while (TheTableCode == null)
            {
                StrGeneralKey = Algorithmes.GeneratorGK();
                TheTableCode = Algorithmes.GenerateTableCode(StrGeneralKey);
            }

            // Demande d'écriture du message
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n           CHIFFREMENT           \n");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Cyan;
            string MessageOriginal = "";
            string LigneMessage = "";

            while (true)
            {
                Console.WriteLine("Écrivez le texte à chiffrer : ");
                Display.PrintMessage("AVIS : Vous pouvez écrire plusieurs paragraphes !", MessageState.Warning);
                Console.WriteLine("Pour annuler, appuyez sur ENTRÉE sans rien écrire.");
                Console.WriteLine("Pour terminer le message, appuyez sur CTRL + D et sur ENTRÉE.");
                while (LigneMessage.Length == 0 || !LigneMessage.EndsWith('\x04'))
                {
                    LigneMessage = Console.ReadLine();
                    MessageOriginal += LigneMessage + '\n';
                    if (MessageOriginal == "\n")
                    {
                        break;
                    }
                }

                if (MessageOriginal.Length > 2)
                {
                    MessageOriginal = MessageOriginal[..^2] + '\n';
                }

                if (IfOnlySpaces(MessageOriginal))
                {
                    if (MessageOriginal == "\n")
                    {
                        OverridePress = true;
                        return;
                    }
                    Console.WriteLine("Aucun texte détecté.");
                }
                else
                {
                    break;
                }
            }

            if (MessageOriginal != "\n")
            {
                string MessageChiffre = Algorithmes.Chiffrement(MessageOriginal, StrGeneralKey, TheTableCode);
                string VraiMessageChiffre = Algorithmes.SecondChiffrement(MessageChiffre);
                if (VraiMessageChiffre.Length > 4094)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
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
                    } while (!Extensions.EcrireChiffrementLong(VraiMessageChiffre, cipherFileName));

                    Console.WriteLine("Nom du fichier : " + Extensions.GetNomFichierChiffrement());
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    return;
                }


                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Le message chiffré :");
                Console.WriteLine(VraiMessageChiffre);
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            else
            {
                OverridePress = true;
            }
        }

        private static bool IfOnlySpaces(string Message)
        {
            int Spaces = 0;
            for (int c = 0; c < Message.Length; c++)
            {
                if (Message[c] == ' ')
                {
                    Spaces++;
                }
            }

            return Spaces == Message.Length;
        }

        private static void DechiffrerLeMessage()
        {
            //Demande d'écriture du message chiffré
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n          DÉCHIFFREMENT          \n");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("AVIS : Le texte chiffré peut s'étendre sur plusieurs lignes !");
            Console.WriteLine("Validez la dernière ligne en cliquant ENTRÉE sans rien écrire dans cette dernière.");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Entrez le texte à déchiffrer :");
            Console.WriteLine("Pour annuler, appuyez sur ENTRÉE sans rien écrire.");
            try
            {
                while (true)
                {
                    bool isFile = ConcatSegementsMessage(out string MessageADechiffrer);

                    if (isFile)
                    {
                        string cipherFilePath = Parametres.CipherFileDirectory + MessageADechiffrer["FILE:".Length..];
                        if (File.Exists(cipherFilePath))
                        {
                            MessageADechiffrer = File.ReadAllText(cipherFilePath);
                        }
                        else
                        {
                            cipherFilePath = MessageADechiffrer["FILE:".Length..];
                            if (File.Exists(cipherFilePath))
                            {
                                MessageADechiffrer = File.ReadAllText(cipherFilePath);
                            }
                            else
                            {
                                Display.PrintMessage("Le fichier \""
                                                 + cipherFilePath
                                                 + "\" n'existe pas dans le répertoire de chiffrement.", MessageState.Failure);
                                Display.PrintMessage("Le répertoire de chiffrement spécifié est : " + Parametres.CipherFileDirectory,
                                                     MessageState.Warning);
                                Console.WriteLine("Entrez un nouveau chemin d'accès ou un chiffrement valide,");
                                Console.WriteLine("ou appuyez sur ENTRÉE sans rien écrire pour quitter.");
                                continue;
                            }
                        }
                    }

                    //Validité du chiffrement complet
                    string MessageTeste = TestCipher(MessageADechiffrer);
                    if (MessageTeste == null)
                    {
                        RewriteCypherWarnMsg();
                        continue;
                    }
                    //Déchiffrement premier
                    string MessageDechiffre1 = Algorithmes.DechiffrementPremier(MessageTeste);
                    //Validité déchiffrement premier total
                    if (MessageDechiffre1 == null)
                    {
                        RewriteCypherWarnMsg();
                        continue;
                    }

                    //Extraction du GK
                    string StrGeneralKey = Algorithmes.DeveloppGK(MessageDechiffre1);

                    //Génération de la Table Code
                    string[,] TheTableCode = Algorithmes.GenerateTableCode(StrGeneralKey);
                    // Vérification si GK mauvais car répétitions de trans
                    if (TheTableCode == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Environment.NewLine + "ERREUR FATALE CODE 55 : Le chiffrement n'est pas authentique.");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        RewriteCypherWarnMsg();
                        continue;
                    }

                    // Déchiffrement second
                    string MessageDechiffreComplet = Algorithmes.DechiffrementSecond(TheTableCode, StrGeneralKey, MessageDechiffre1);
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
                            Console.Write(" " + MessageDechiffreComplet);
                        }
                        else
                        {
                            Console.Write(MessageDechiffreComplet);
                        }

                        Extensions.AfficherMarqueurFin();

                        if (!Algorithmes.IsGoodCheckSum)
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
                            Historique.AjouterHistorique(MessageDechiffreComplet,
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

        public static bool ConcatSegementsMessage(out string messageConcat)
        {
            messageConcat = "";
            string messagePart;
            do
            {
                messagePart = Console.ReadLine();
                messageConcat += messagePart;
                if (messageConcat.StartsWith("FILE:", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            } while (messagePart != "");


            if (messageConcat == "")
            {
                throw new LORENZException(ErrorCode.E0xFFF, false);
            }

            return false;
        }

        public static bool HaveSpaces(string Message)
        {
            for (int c = 0; c < Message.Length; c++)
            {
                if (Convert.ToString(Message[c]) == " ")
                {
                    return true;
                }
            }

            return false;
        }

        private static void RewriteCypherWarnMsg()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Environment.NewLine + "Ce message n'est pas valide." + Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Entrez un texte valide à déchiffrer.");
            Console.WriteLine("Validez la dernière ligne en cliquant ENTRÉE sans rien écrire dans cette dernière.");
            Console.WriteLine("Pour annuler, appuyez sur ENTRÉE sans rien écrire :");
        }

        private static string TestCipher(string MessageToTest)
        {
            bool spacesInMessage = HaveSpaces(MessageToTest);
            if (MessageToTest.Length <= 38 || MessageToTest.Length % 4 != 0 || spacesInMessage)
            {
                if (spacesInMessage)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Environment.NewLine + "TYPING BREAK ERROR : SPACES DETECTED.");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                return null;
            }
            return MessageToTest;
        }

        private static void MenuHistorique()
        {
            Console.Clear();
            if (Historique.ListeHistorique.Count == 0 && !Historique.LireFichierHistorique())
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

            while (true)
            {
                Console.WriteLine("===== Historique =====\n");
                Console.WriteLine("[H]: Consulter l'historique");
                Console.WriteLine("[C]: Nouvelle catégorie");
                Console.WriteLine("\nAppuyez sur ESC pour retourner");

                ConsoleKeyInfo saisie = Console.ReadKey(true);
                switch (saisie.Key)
                {
                    case ConsoleKey.H:
                        Historique.AfficherHistorique();
                        Console.Clear();
                        break;
                    case ConsoleKey.C:
                        // Méthode nouvelle catégorie
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
                Console.WriteLine("===== Options de chiffrement =====\n");
                Console.WriteLine("T : Disposition de la table de transcription");
                Console.WriteLine("S : Disposition de la table secrète");
                Console.WriteLine("\nAppuyez sur ESC pour retourner");

                ConsoleKeyInfo saisie = Console.ReadKey(true);
                switch (saisie.Key)
                {
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
