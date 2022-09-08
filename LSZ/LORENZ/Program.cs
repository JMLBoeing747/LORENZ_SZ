using Cryptography;
using System;

namespace LORENZ
{
    class Program
    {
        public static string VersionNumber { get => "3.0.0-alpha"; }
        private static bool ShowSenderBeginState { get; set; }
        private static bool CancelOperation { get; set; }

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

        static void Demarrage()
        {
            Display.PrintMessage("Initialisation des composants...", MessageState.Info);
            Parametres.LireGeneralParamsFile();
            ShowSenderBeginState = Parametres.ShowPseudoNameSender;
            double Argent = Jeux.ReadCoinsInfoFile();
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("DER LORENZ SCHLÜSSELZUSATZ " + VersionNumber);
            while (true)
            {
                Extensions.Configuration();
                Console.ForegroundColor = ConsoleColor.Cyan;
                if (CancelOperation)
                    CancelOperation = false;
                Console.WriteLine("Choisir une option :");
                Console.WriteLine("1 : Chiffrement");
                Console.WriteLine("2 : Déchiffrement");
                Console.WriteLine("L : LID");
                Console.WriteLine("J : Jeu");
                if (!Parametres.ShowPseudoNameSender)
                    Console.WriteLine("S : Afficher l'expéditeur");
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("S : Masquer l'expéditeur");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                Console.WriteLine("P : Modifier le pseudo");
                Console.WriteLine("H : AIDE");
                Console.WriteLine("\nO : Options\n");
                Console.WriteLine("Pour quitter, appuyez sur ESC");
                if (Argent > 0.00)
                    Console.WriteLine(Environment.NewLine + "Votre solde : " + Argent + " Coins");
                ConsoleKeyInfo saisie = Console.ReadKey(true);

                if ((int)saisie.Key == 49 || (int)saisie.Key == 97)
                    ChiffrerLeMessage();
                else if ((int)saisie.Key == 50 || (int)saisie.Key == 98)
                    DechiffrerLeMessage();
                else if (saisie.Key == ConsoleKey.L)
                    AfficherLID();
                else if (saisie.Key == ConsoleKey.J)
                {
                    double NewArgent = Jeux.TheGame(Argent);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Clear();
                    Argent = NewArgent;
                    continue;
                }
                else if (saisie.Key == ConsoleKey.S)
                {
                    Parametres.ShowPseudoNameSender = !Parametres.ShowPseudoNameSender;
                    Console.Clear();
                    if (Parametres.ShowPseudoNameSender)
                        Display.PrintMessage("EXPÉDITEUR AFFICHÉ", MessageState.Warning);
                    else
                        Display.PrintMessage("EXPÉDITEUR MASQUÉ", MessageState.Warning);
                    continue;
                }
                else if (saisie.Key == ConsoleKey.P)
                {
                    ChangerLePseudo();
                    Console.Clear();
                    continue;
                }
                else if (saisie.Key == ConsoleKey.H)
                {
                    AfficherAide();
                    continue;
                }
                else if (saisie.Key == ConsoleKey.O)
                {
                    MenuOptions();
                    continue;
                }
                else if (saisie.Key == ConsoleKey.Escape)
                {
                    break;
                }
                else if (saisie.Key == ConsoleKey.Enter)
                {
                    /* La gestion de la touche ENTRÉE est gardée pour permettre une meilleure transition
                     * vers la touche ESC auprès des anciens utilisateurs */
                    Console.Clear();
                    Display.PrintMessage("Pour quitter, appuyez sur ESC.", MessageState.Warning, resetColors: true);
                    continue;
                }
                else
                {
                    Console.Clear();
                    Display.PrintMessage("Ceci n'est pas une option valide.", MessageState.Failure);
                    continue;
                }

                if (!CancelOperation)
                {
                    Console.WriteLine(Environment.NewLine + "Press any key to continue...");
                    Console.ReadKey(true);
                }
                Console.Clear();
            }
            Display.PrintMessage("Fermeture en cours...", MessageState.Info);
            if (ShowSenderBeginState != Parametres.ShowPseudoNameSender)
                Parametres.WriteGeneralParamsFile();
        }

        private static void AfficherLID()
        {
            Console.Clear();
            Console.WriteLine("Le LID vous permet de vous identifier de façon unique envers tous les utilisateurs LORENZ.");
            Console.WriteLine("Vous avez besoin de connaître le LID de votre correspondant si vous désirez lui envoyer un");
            Console.WriteLine("message en privé. Le LID ne peut pas être modifié car il est propre à chaque clé de produit.");
            Console.WriteLine("Voici votre LID de six caractères :\n");
            Console.WriteLine(">>> " + Parametres.LID + " <<<");
        }

        static void ChangerLePseudo()
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
                return;
            else if (newPseudo == Parametres.PseudoName)
                Display.PrintMessage("Pseudo identique au précédent. Aucun changement à faire.", MessageState.Warning);
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

        static void AfficherAide()
        {
            try
            {
                if (System.IO.File.Exists(Parametres.HelpFilePath))
                    System.Diagnostics.Process.Start("hh.exe", Parametres.HelpFilePath);
                else throw new System.ComponentModel.Win32Exception();
                Console.Clear();
                Display.PrintMessage("OUVERTURE DU FICHIER D'AIDE", MessageState.Success);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Console.Clear();
                Display.PrintMessage("OUVERTURE ÉCHOUÉE !", MessageState.Failure);
            }
        }

        static void ChiffrerLeMessage()
        {
            Console.Clear();
            Console.WriteLine("Preparation...");
            //Génération du GK et de la TableCode correspondante
            string StrGeneralKey = Algorithmes.GeneratorGK();
            string[,] TheTableCode = Algorithmes.GenerateTableCode(StrGeneralKey);
            while (TheTableCode == null)
            {
                StrGeneralKey = Algorithmes.GeneratorGK();
                TheTableCode = Algorithmes.GenerateTableCode(StrGeneralKey);
            }
            //Demande d'écriture du message
            Console.Clear();
            string MessageOriginal = "";
            string LigneMessage = "";

            while (true)
            {
                Console.WriteLine("Écrivez le texte à chiffrer : ");
                Display.PrintMessage("AVIS : Vous pouvez écrire sur plusieurs lignes !", MessageState.Warning);
                Console.WriteLine("Pour annuler, appuyez sur ENTRÉE sans rien écrire.");
                Console.WriteLine("Pour terminer le message, enfoncez CTRL + D et appuyez sur ENTRÉE.");
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
                        CancelOperation = true;
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
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Le message chiffré :");
                Console.WriteLine(VraiMessageChiffre);
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            else
                CancelOperation = true;
        }

        static bool IfOnlySpaces(string Message)
        {
            int Spaces = 0;
            for (int c = 0; c < Message.Length; c++)
                if (Message[c] == ' ')
                    Spaces++;
            return Spaces == Message.Length;
        }

        static void DechiffrerLeMessage()
        {
            //Demande d'écriture du message chiffré
            Console.Clear();
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
                    string MessageADechiffrer = ConcatSegementsMessage();
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
                    //Vérification si GK mauvais car répétitions de trans
                    if (TheTableCode == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Environment.NewLine + "ERREUR FATALE CODE 55 : Le chiffrement n'est pas authentique.");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        RewriteCypherWarnMsg();
                        continue;
                    }
                    else if (!Algorithmes.IsGoodCheckSum)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(Environment.NewLine + "ATTENTION LE MESSAGE PEUT CONTENIR DES DÉFORMATIONS");
                        Console.WriteLine("DUE À UNE MODIFICATION ACCIDENTELLE OU MALINTENTIONNÉE DE VALEURS DANS LE CHIFFREMENT");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    }
                    //Déchiffrement second
                    string MessageDechiffreComplet = Algorithmes.DechiffrementSecond(TheTableCode, StrGeneralKey, MessageDechiffre1);
                    Console.ForegroundColor = ConsoleColor.White;
                    if (Algorithmes.ToUIDPrivateMeg != Parametres.LID)
                    {
                        Display.PrintMessage("LORENZ WARNING MSG: THIS MESSAGE IS PRIVATELY DESTINATED TO ANOTHER USER", MessageState.Warning);
                        Algorithmes.ToUIDPrivateMeg = Parametres.LID;
                        Algorithmes.IsPrivateMessage = false;
                    }
                    else
                    {
                        if (!Algorithmes.IsPrivateMessage)
                            Console.WriteLine(Environment.NewLine + "Message déchiffré :");
                        else
                        {
                            Console.WriteLine(Environment.NewLine + "Message déchiffré (EN PRIVÉ):");
                            Algorithmes.IsPrivateMessage = false;
                        }
                        if (Parametres.ShowPseudoNameSender)
                            Console.WriteLine(Algorithmes.SenderPseudoName + " : " + MessageDechiffreComplet);
                        else
                            Console.WriteLine(MessageDechiffreComplet);
                    }
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    return;
                }
            }
            catch (LORENZException le)
            {
                if (le.Err == ErrorCode.E0x00)
                    throw new LORENZException(ErrorCode.E0x00, false);
                CancelOperation = true;
                return;
            }
        }

        public static string ConcatSegementsMessage()
        {
            string messagePart = Console.ReadLine();
            string messageConcat = messagePart;
            while (messagePart != "")
            {
                messagePart = Console.ReadLine();
                messageConcat += messagePart;
            }
            if (messageConcat == "")
                throw new LORENZException(ErrorCode.E0xFFF, false);
            return messageConcat;
        }

        public static bool HaveSpaces(string Message)
        {
            for (int c = 0; c < Message.Length; c++)
                if (Convert.ToString(Message[c]) == " ")
                    return true;
            return false;
        }

        static void RewriteCypherWarnMsg()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Environment.NewLine + "Ce message n'est pas valide." + Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Entrez un texte valide à déchiffrer.");
            Console.WriteLine("Validez la dernière ligne en cliquant ENTRÉE sans rien écrire dans cette dernière.");
            Console.WriteLine("Pour annuler, appuyez sur ENTRÉE sans rien écrire :");
        }

        static string TestCipher(string MessageToTest)
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

        static void MenuOptions()
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("===== Options de chiffrement =====\n");
                Console.WriteLine("T : Disposition de la table de transcription");
                Console.WriteLine("S : Disposition de la table secrète");
                Console.WriteLine("\nAppuyez sur ESC pour retourner");
                ConsoleKeyInfo saisie = Console.ReadKey(true);
                if (saisie.Key == ConsoleKey.Escape)
                {
                    Console.Clear();
                    break;
                }
                else if (saisie.Key == ConsoleKey.T)
                {
                    // SetTransTable();
                }
                else if (saisie.Key == ConsoleKey.S)
                {
                    SetSecretTable();
                    Console.Clear();
                }
                else
                {
                    Console.Clear();
                    Display.PrintMessage("Ceci n'est pas une touche valide.", MessageState.Failure);
                }
            }
        }

        static void SetSecretTable()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("ATTENTION ! Modifier la disposition de la table secrète sans avoir aucune connaissance");
            Console.WriteLine("approfondie du principe de chiffrement peut causer de sérieux problèmes auprès de vos");
            Console.WriteLine("correspondants, notamment au moment de la transmission.");
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Il est primordial d'informer ces derniers de toute modification de la disposition des tables");
            Console.WriteLine("de chiffrement avant de transmettre tout nouveau message.\n");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Inscrivez la nouvelle disposition sour la forme d'une chaine de 10 caractères uniques.");
            Console.WriteLine("Pour annuler, appuyez sur ENTRÉE sans rien écrire.\n");
            Console.WriteLine("Disposition actuelle de la TS : " + Algorithmes.BaseSecretCode);
            while (true)
            {
                Console.Write("Nouvelle disposition : ");
                string newSTSet = Console.ReadLine();
                if (newSTSet != "")
                {
                    if (newSTSet.Length != 10)
                    {
                        Display.PrintMessage("La chaîne doit faire 10 caractères de long.", MessageState.Failure);
                    }
                    else
                    {
                        bool sameChars = false;
                        for (int c = 0; c < newSTSet.Length; c++)
                        {
                            for (int d = c + 1; d < newSTSet.Length; d++)
                            {
                                if (newSTSet[c] == newSTSet[d])
                                {
                                    sameChars = true;
                                    break;
                                }
                            }
                            if (sameChars)
                            {
                                break;
                            }
                        }

                        if (!sameChars)
                        {
                            Algorithmes.BaseSecretCode = newSTSet.ToUpper();
                            Display.PrintMessage("Nouvelle disposition : " + Algorithmes.BaseSecretCode, MessageState.Success);
                            Console.WriteLine("Appuyez sur n'importe quelle touche pour continuer...");
                            Console.ReadKey(true);
                            break;
                        }
                        else
                        {
                            Display.PrintMessage("La chaîne doit être composée de 10 caractères uniques.", MessageState.Failure);
                        }
                    }
                }
                else
                {
                    Display.PrintMessage("Aucune nouvelle disposition assignée !", MessageState.Warning);
                    Console.WriteLine("Appuyez sur n'importe quelle touche pour continuer...");
                    Console.ReadKey(true);
                    break;
                }
            }
        }
    }
}
