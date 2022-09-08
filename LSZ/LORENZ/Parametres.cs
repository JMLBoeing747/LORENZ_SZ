using System;
using System.IO;
using Cryptography;

namespace LORENZ
{
    public static class Parametres
    {
        public static string LORENZPATH { get => Environment.CurrentDirectory; }
        private static string ParamsDirectory { get => @"LZPARAMS"; }
        public static string UserlogFile { get => $@"{ParamsDirectory}/USERLOG.LZI"; }
        public static string LastAccessFile { get => $@"{ParamsDirectory}/LASTACSS.LZI"; }
        public static string CoinsRecordFile { get => $@"{ParamsDirectory}/COINSREC.LZI"; }
        public static string HelpFilePath { get => @"LZHELP.CHM"; }
        public static string ProductKeyFile { get => @"PRDCTKEY.LKI"; }
        public static string GeneralParamsFile { get => $@"{ParamsDirectory}/PARAMS.INI"; }
        public static string FichierEnAnalyse { get; set; }

        public static bool ShowPseudoNameSender { get; set; }
        public static string PseudoName { get; set; } = Environment.UserName;

        public static string LID { get; set; }

        public static void VerifierParametres()
        {
            while (true)
                try
                {
                    Display.PrintMessage("Identification...", MessageState.Warning);
                    //Verify integrity of USERLOG.LZI when reading LASTACCS.LZI...
                    Display.PrintMessage("CHECKING 1.", MessageState.Info, false);
                    LireLastAccessFile(new FileInfo(UserlogFile).LastAccessTimeUtc);
                    Display.PrintMessage("OK", MessageState.Info);
                    //Read USERLOG.LZI...
                    Display.PrintMessage("CHECKING 2.", MessageState.Info, false);
                    Cryptographie.DechiffrerUserinfo();
                    Display.PrintMessage("OK", MessageState.Info);
                    //Update LASTACSS.LZI...
                    EcrireLastAccessFile(new FileInfo(UserlogFile).LastAccessTimeUtc);
                    if (Directory.Exists("CRYPTO"))
                        Directory.Delete("CRYPTO", true);
                    if (File.Exists(ProductKeyFile))
                        File.Delete(ProductKeyFile);
                    Display.PrintMessage("Identification successful!", MessageState.Success);
                    return;
                }
                catch (LORENZException le)
                {
                    if (File.Exists(ProductKeyFile) || !Directory.Exists(ParamsDirectory))
                    {
                        string LIDRetrieved = LireCleProduit().Item3;
                        Display.PrintMessage("SUCCESS: KEY IS VALID.", MessageState.Success);
                        File.Delete(ProductKeyFile);
                        //Write userinfos into USERLOGI.LZI...
                        Display.PrintMessage("Writing parameters...", MessageState.Info);
                        Directory.CreateDirectory(ParamsDirectory);
                        EcrireParametres(LIDRetrieved);
                        //Writing for first time LASTACSS.LZI...
                        EcrireLastAccessFile(new FileInfo(UserlogFile).LastAccessTimeUtc);
                    }
                    else throw new LORENZException(le.Err);
                }
        }

        private static void CreerPseudo()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("CRÉATION DU PSEUDONYME :");
            Console.WriteLine("Le pseudo est le nom qui sera affiché à tous les récepteurs pouvant déchiffrer le message");
            Console.WriteLine("et qui ont activé l'affichage de l'expéditeur. Par défaut, il est initialisé à votre nom");
            Console.WriteLine("d'utilisateur système mais vous pouvez le personnaliser. Pour cela, tapez ci-dessous le");
            Console.WriteLine("nouveau pseudo à utiliser pour les chiffrements futurs. Sinon, appuyer sur ENTRÉE sans");
            Console.WriteLine("rien écrire et la valeur par défaut sera considérée.");
            Console.Write("Nouveau >>> ");
            string newPseudo = Console.ReadLine();
            if (newPseudo == "")
            {
                PseudoName = Environment.UserName;
                WriteGeneralParamsFile();
                Display.PrintMessage("Valeur initialisée à : " + PseudoName);
            }
            else if (newPseudo == Environment.UserName)
            {
                PseudoName = newPseudo;
                WriteGeneralParamsFile();
                Display.PrintMessage("Valeur par défaut choisie", MessageState.Warning);
            }
            else
            {
                PseudoName = newPseudo;
                WriteGeneralParamsFile();
                Display.PrintMessage("Nouveau pseudo enregistré!", MessageState.Success);
            }
            Display.PrintMessage("Appuyez sur une touche pour continuer...", MessageState.Warning);
            Console.ReadKey(true);
            Display.PrintMessage("Finalisation...", MessageState.Info);
        }

        public static (string, string, string) LireCleProduit()
        {
            int essais = 0;
            while (true)
            {
                try
                {
                    return DecoderCleProduit();
                }
                catch (LORENZException)
                {
                    essais++;
                    if (essais == 6)
                        break;
                    Console.Clear();
                    if (essais == 1 && !File.Exists(ProductKeyFile))
                    {
                        Display.PrintMessage("Bienvenue chez LORENZ ! En tant que nouvel utilisateur, voici quatre étapes", MessageState.Info);
                        Display.PrintMessage("à suivre avant de pouvoir profiter de l'application :\n", MessageState.Info);
                        Display.PrintMessage("1. Repérez le dossier de l'application \"LORENZSZ\". Pour ce faire, enfoncez", MessageState.Info);
                        Display.PrintMessage("   les touches Windows + R de votre clavier et entrez %lorenzpath%.", MessageState.Info);
                        Display.PrintMessage("2. Si vous avez installé \"CRYPTO\", vous trouverez un dossier du même nom", MessageState.Info);
                        Display.PrintMessage("   dans ce répertoire. Entrez-y et exécutez le programme \"CRYPTO.exe\",", MessageState.Info);
                        Display.PrintMessage("   puis, suivez les instructions qui s'afficheront.", MessageState.Info);
                        Display.PrintMessage($"3. Insérez le fichier \"{ProductKeyFile}\" fourni par votre distributeur LORENZ", MessageState.Info);
                        Display.PrintMessage("   dans le dossier du programme (c.-à-d. \"LORENZSZ\").", MessageState.Info);
                        Display.PrintMessage("4. Appuyez sur n'importe quelle touche et nous vérifierons le reste.\n", MessageState.Info);
                        Display.PrintMessage("NOTA : Si %lorenzpath% ne fonctionne pas, redémarrez votre ordinateur, puis réessayez.\n", MessageState.Warning);
                        Display.PrintMessage("Appuyez sur ESC pour quitter.", MessageState.Info);
                    }
                    else if (!File.Exists(ProductKeyFile))
                    {
                        Display.PrintMessage($"Désolé! Nous n'avons pas détecté une clé \"{ProductKeyFile}\". Veuillez réessayer", MessageState.Warning);
                        Display.PrintMessage("en vous assurant de l'avoir bien mis dans le dossier \"LORENZSZ\".", MessageState.Warning);
                        Display.PrintMessage("Enfoncez les touches Windows + R et entrez %lorenzpath% pour le trouver.\n", MessageState.Warning);
                        Display.PrintMessage("Appuyez sur ESC pour quitter.", MessageState.Warning);
                    }
                    else
                    {
                        Display.PrintMessage("Désolé! La clé que vous avez insérée est invalide ! Veuillez réessayer en", MessageState.Warning);
                        Display.PrintMessage("demandant à votre fournisseur LORENZ de vous en fournir une nouvelle.", MessageState.Warning);
                        Display.PrintMessage("N'oubliez pas ensuite de l'insérer dans \"LORENZSZ\" avant de continuer.\n", MessageState.Warning);
                        Display.PrintMessage("Appuyez sur ESC pour quitter.", MessageState.Warning);
                    }
                    ConsoleKeyInfo saisie = Console.ReadKey(true);
                    while (saisie.Key == ConsoleKey.LeftWindows || saisie.Key == ConsoleKey.RightWindows)
                        saisie = Console.ReadKey(true);
                    if (saisie.Key == ConsoleKey.Escape)
                        throw new LORENZException(ErrorCode.E0xFFF, false);
                }
            }
            throw new LORENZException(ErrorCode.E0x20);
        }

        public static (string, string, string) DecoderCleProduit()
        {
            FichierEnAnalyse = ProductKeyFile;
            try
            {
                //Decyphering...
                Decyphering.OpeningDecyphering(ProductKeyFile, out uint[] keyQBytes, out uint[] cypheredMessageOnly);
                Display.PrintMessage("Déchiffrement de la clé de produit...", MessageState.Info);
                Common.XORPassIntoMessage(keyQBytes, ref cypheredMessageOnly);
                Common.ReverseKey(ref keyQBytes);
                Common.NotOperationToKey(ref keyQBytes);

                Cryptographie.CreateMatrix(ref keyQBytes, -14);
                Common.XORPassIntoMessage(keyQBytes, ref cypheredMessageOnly);
                Common.ReverseKey(ref keyQBytes);
                Cryptographie.CreateMatrix(ref keyQBytes, -13);
                Common.NotOperationToKey(ref keyQBytes);
                Common.XORPassIntoMessage(keyQBytes, ref cypheredMessageOnly);
                Cryptographie.CreateMatrix(ref keyQBytes, -12);
                Common.XORPassIntoMessage(keyQBytes, ref cypheredMessageOnly);

                //Strip out unknown characters, associate and verifying infos...
                (string, string, DateTime, string) userInfos = Decyphering.ShortingUserInfos(Decyphering.StripOutAndSplit(cypheredMessageOnly));
                DateTime dtLimit = userInfos.Item3.AddMinutes(5.0);
                if (userInfos.Item3 < DateTime.UtcNow && dtLimit > DateTime.UtcNow)
                    return (userInfos.Item1, userInfos.Item2, userInfos.Item4);
                else
                    throw new LORENZException(ErrorCode.E0x20, false);
            }
            catch (CryptographyException)
            {
                throw new LORENZException(ErrorCode.E0x20, false);
            }

        }

        private static void EcrireParametres(string LID)
        {
            Cryptographie.ChiffrerFichier(Encryption.CreateScrambledMessage(Environment.UserName, Environment.MachineName, LID), UserlogFile);
        }

        private static void EcrireLastAccessFile(DateTime lastAccessTime)
        {
            Cryptographie.ChiffrerFichier(Encryption.CreateScrambledMessage(lastAccessTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff")), LastAccessFile);
        }

        private static void LireLastAccessFile(DateTime lastAccessTimeToCompare)
        {
            if (!File.Exists(UserlogFile))
                throw new LORENZException(ErrorCode.E0x12, false);
            try
            {
                string[] dateTimeStripped = Decyphering.StripOutAndSplit(Cryptographie.DechiffrerFichier(LastAccessFile));
                Display.PrintMessage(".", MessageState.Info, false);
                DateTime dtLast = DateTime.Parse(dateTimeStripped[0]);
                Display.PrintMessage(".", MessageState.Info, false);
                if (dtLast != lastAccessTimeToCompare)
                    throw new LORENZException(ErrorCode.E0x11, false);
            }
            catch
            {
                throw new LORENZException(ErrorCode.E0x11, false);
            }
        }

        public static byte[] StringToByte(string s)
        {
            byte[] byteArray = new byte[s.Length];
            for (int c = 0; c < s.Length; c++)
                byteArray[c] = Convert.ToByte(s[c]);
            return byteArray;
        }

        public static void LireGeneralParamsFile()
        {
            if (!File.Exists(GeneralParamsFile))
                CreerPseudo();
            string[] AllLinesArray = File.ReadAllLines(GeneralParamsFile, System.Text.Encoding.UTF8);
            foreach (string line in AllLinesArray)
            {
                string[] parameter = line.Split('=');
                switch (parameter[0])
                {
                    case "SHOWSENDER":
                        ShowPseudoNameSender = parameter[1] == "True";
                        break;
                    case "PSEUDONAME":
                        PseudoName = parameter[1];
                        break;
                    default:
                        break;
                }
            }
        }

        public static void WriteGeneralParamsFile()
        {
            string[] parameters;
            if (PseudoName == Environment.UserName)
                parameters = new string[1];
            else
            {
                parameters = new string[2];
                parameters[1] = "PSEUDONAME=" + PseudoName;
            }
            parameters[0] = "SHOWSENDER=" + ShowPseudoNameSender;
            File.WriteAllLines(GeneralParamsFile, parameters);
        }
    }
}
