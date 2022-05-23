using System;
using Cryptography;

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
                    Console.WriteLine("S : Masquer l'expéditeur");
                Console.WriteLine("P : Modifier le pseudo");
                Console.WriteLine("H : AIDE");
                Console.WriteLine("Pour quitter, cliquez sur ENTER ou ESC");
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
                else if (saisie.Key == ConsoleKey.Enter || saisie.Key == ConsoleKey.Escape)
                    break;
                else
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ceci n'est pas une option valide.");
                    Console.ForegroundColor = ConsoleColor.Cyan;
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
            Console.WriteLine("n'importe pas). Sinon, appuyer sur ENTER sans rien écrire.");

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
                Console.WriteLine("Pour annuler, cliquez ENTRÉE sans rien écrire.");
                Console.WriteLine("Pour terminer le message, enfoncez CTRL + D et cliquez sur ENTRÉE.");
                while (LigneMessage.Length == 0 || !LigneMessage.EndsWith('\x04'))
                {
                    LigneMessage = Console.ReadLine();
                    MessageOriginal += LigneMessage + '\n';
                }
                MessageOriginal = MessageOriginal.Substring(0, MessageOriginal.Length - 2) + '\n';

                if (IfOnlySpaces(MessageOriginal))
                {
                    if (MessageOriginal == "")
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

            if (MessageOriginal != "")
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
            Console.WriteLine("Validez la dernière ligne en cliquant ENTER sans rien écrire dans cette dernière.");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Entrez le texte à déchiffrer :");
            Console.WriteLine("Pour annuler, cliquez ENTER sans rien écrire.");
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
            Console.WriteLine("Validez la dernière ligne en cliquant ENTER sans rien écrire dans cette dernière.");
            Console.WriteLine("Pour annuler, cliquez ENTER sans rien écrire :");
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
    }
}
