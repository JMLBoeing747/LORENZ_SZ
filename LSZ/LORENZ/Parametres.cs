using Cryptography;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LORENZ
{
    public static class Parametres
    {
        public static string LORENZPATH => Environment.CurrentDirectory;
        public static string ParamsDirectory => @"LZPARAMS";
        public static string UserlogFile => $@"{ParamsDirectory}/USERLOG.LZI";
        public static string LastAccessFile => $@"{ParamsDirectory}/LASTACSS.LZI";
        public static string CoinsRecordFile => $@"{ParamsDirectory}/COINSREC.LZI";
        public static string HelpFilePath => @"LZHELP.CHM";
        public static string ProductKeyFile => @"PRDCTKEY.LKI";
        public static string OldParamsFile => $@"{ParamsDirectory}/PARAMS.INI";
        public static string LorenzParamsFile => $@"{ParamsDirectory}/LORENZ.INI";
        public static string LzCipherFileExt => ".lc2";
        public static string FichierEnAnalyse { get; set; }

        public static bool ShowPseudoNameSender { get; set; }
        public static string PseudoName { get; set; } = Environment.UserName;

        public static string LID { get; set; }
        public static string CipherFileDirectory { get; set; }

        /// <summary>
        /// The <c>WritePrivateProfileString</c> function copies a string into the specified section of the specified initialization
        /// file. This function is provided for compatibility with 16-bit Windows-based applications. WIn32-based applications
        /// should store initialization information in the registry. (From WIN32 API docs)
        /// </summary>
        /// <param name="lpAppName">Points to a null-terminated string containing the name of the section to which the string
        /// will be copied. If the section does not exist, it is created. The name of the section is case-independent; the
        /// string can be any combination of uppercase and lowercase letters.</param>
        /// <param name="lpKeyName">Points to the null-terminated string containing the name of the key to be associated with a
        /// string. If the key does not exist in the specified section, it is created. If this parameter is <c>NULL</c>, the entire
        /// section, including all entries within the section, is deleted.</param>
        /// <param name="lpString">Points to a null-terminated string to be written to the file. If this parameter is <c>NULL</c>,
        /// the key pointed to by the <c>lpKeyName</c> parameter is deleted.</param>
        /// <param name="lpFileName">Points to a null-terminated string that names the initialization file.</param>
        /// <returns>If the function successfully copies the string to the initialization file, the return value is nonzero.
        /// If the function fails, or if it flushes the cached version of the most recently accessed initialization file, the
        /// return value is zero. To get extended error information, call <c>GetLastError</c>.</returns>
        [DllImport("Kernel32", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(string lpAppName,
                                                             string lpKeyName,
                                                             string lpString,
                                                             string lpFileName);

        /// <summary>
        /// The <c>GetPrivateProfileString</c> function retrieves a string from the specified section in an initialization file.
        /// This function is provided for compatibility with 16-bit Windows-based applications. Win32-based applications
        /// should store initialization information in the registry. (From WIN32 API docs)
        /// </summary>
        /// <param name="lpAppName">Points to a null-terminated string that specifies the section containing the key name. If
        /// this parameter is <c>NULL</c>, the <c>GetPrivateProfileString</c> function copies all section names in the file to
        /// the supplied buffer.</param>
        /// <param name="lpKeyName">Pointer to the null-terminated string containing the key name whose associated string is to
        /// be retrieved. If this parameter is <c>NULL</c>, all key names in the section specified by the <c>lpAppName</c>
        /// parameter are copied to the buffer specified by the <c>lpReturnedString</c> parameter.</param>
        /// <param name="lpDefault">Pointer to a null-terminated default string. If the <c>lpKeyName</c> key cannot be found in
        /// the initialization file, <c>GetPrivateProfileString</c> copies the default string to the <c>lpReturnedString</c>
        /// buffer. This parameter cannot be <c>NULL</c>. Avoid specifying a default string with trailing blank characters. The
        /// function inserts a null character in the <c>lpReturnedString</c> buffer to strip any trailing blanks.</param>
        /// <param name="lpReturnedString">Pointer to the buffer that receives the retrieved string.</param>
        /// <param name="nSize">Specifies the size, in characters, of the buffer pointed to by the <c>lpReturnedString</c>
        /// parameter.</param>
        /// <param name="lpFileName">Pointer to a null-terminated string that names the initialization file. If this parameter
        /// does not contain a full path to the file, Windows searches for the file in the Windows directory.</param>
        /// <returns>If the function succeeds, the return value is the number of characters copied to the buffer, not including
        /// the terminating null character. If neither <c>lpAppName</c> nor <c>lpKeyName</c> is <c>NULL</c> and the supplied
        /// destination buffer is too small to hold the requested string, the string is truncated and followed by a null
        /// character, and the return value is equal to <c>nSize</c> minus one. If either <c>lpAppName</c> or <c>lpKeyName</c>
        /// is <c>NULL</c> and the supplied destination buffer is too small to hold all the strings, the last string is
        /// truncated and followed by two null characters. In this case, the return value is equal to nSize minus two.</returns>
        [DllImport("Kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string lpAppName,
                                                          string lpKeyName,
                                                          string lpDefault,
                                                          StringBuilder lpReturnedString,
                                                          int nSize,
                                                          string lpFileName);

        public static void VerifierParametres()
        {
            while (true)
            {
                try
                {
                    Display.PrintMessage("Identification...", MessageState.Warning);
                    // Verify integrity of USERLOG.LZI when reading LASTACCS.LZI...
                    Display.PrintMessage("VÉRIFICATION 1.", MessageState.Info, false);
                    LireLastAccessFile(new FileInfo(UserlogFile).LastAccessTimeUtc);
                    Display.PrintMessage("OK", MessageState.Info);
                    // Read USERLOG.LZI...
                    Display.PrintMessage("VÉRIFICATION 2.", MessageState.Info, false);
                    Cryptographie.DechiffrerUserinfo();
                    Display.PrintMessage("OK", MessageState.Info);
                    // Update LASTACSS.LZI...
                    EcrireLastAccessFile(new FileInfo(UserlogFile).LastAccessTimeUtc);
                    if (Directory.Exists("CRYPTO"))
                    {
                        Directory.Delete("CRYPTO", true);
                    }

                    if (File.Exists(ProductKeyFile))
                    {
                        File.Delete(ProductKeyFile);
                    }

                    Display.PrintMessage("Identification réussie !", MessageState.Success);
                    return;
                }
                catch (LORENZException le)
                {
                    if (!File.Exists(LastAccessFile))
                    {
                        string LIDRetrieved = LireCleProduit().Item3;
                        Display.PrintMessage("SUCCÈS: CLÉ DE PRODUIT VALIDE.", MessageState.Success);
                        File.Delete(ProductKeyFile);
                        // Write userinfos into USERLOG.LZI...
                        Display.PrintMessage("Écriture des paramètres...", MessageState.Info);
                        Directory.CreateDirectory(ParamsDirectory);
                        EcrireParametres(LIDRetrieved);
                        // Writing for first time LASTACSS.LZI...
                        EcrireLastAccessFile(new FileInfo(UserlogFile).LastAccessTimeUtc);
                    }
                    else
                    {
                        throw new LORENZException(le.Err);
                    }
                }
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
                EcrireFichierParams();
                Display.PrintMessage("Valeur initialisée à : " + PseudoName);
            }
            else if (newPseudo == Environment.UserName)
            {
                PseudoName = newPseudo;
                EcrireFichierParams();
                Display.PrintMessage("Valeur par défaut choisie", MessageState.Warning);
            }
            else
            {
                PseudoName = newPseudo;
                EcrireFichierParams();
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
                    {
                        break;
                    }

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
                    {
                        saisie = Console.ReadKey(true);
                    }

                    if (saisie.Key == ConsoleKey.Escape)
                    {
                        throw new LORENZException(ErrorCode.E0xFFF, false);
                    }
                }
            }
            throw new LORENZException(ErrorCode.E0x20);
        }

        public static (string, string, string) DecoderCleProduit()
        {
            FichierEnAnalyse = ProductKeyFile;
            try
            {
                // Decyphering...
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

                // Strip out unknown characters, associate and verifying infos...
                (string, string, DateTime, string) userInfos = Decyphering.ShortingUserInfos(Decyphering.StripOutAndSplit(cypheredMessageOnly));
                DateTime dtLimit = userInfos.Item3.AddMinutes(5.0);
                if (userInfos.Item3 < DateTime.UtcNow && dtLimit > DateTime.UtcNow)
                {
                    return (userInfos.Item1, userInfos.Item2, userInfos.Item4);
                }
                else
                {
                    throw new LORENZException(ErrorCode.E0x20, false);
                }
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
            {
                throw new LORENZException(ErrorCode.E0x12, false);
            }

            try
            {
                string[] dateTimeStripped = Decyphering.StripOutAndSplit(Cryptographie.DechiffrerFichier(LastAccessFile));
                Display.PrintMessage(".", MessageState.Info, false);
                DateTime dtLast = DateTime.Parse(dateTimeStripped[0]);
                Display.PrintMessage(".", MessageState.Info, false);
                if (dtLast != lastAccessTimeToCompare)
                {
                    throw new LORENZException(ErrorCode.E0x11, false);
                }
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
            {
                byteArray[c] = Convert.ToByte(s[c]);
            }

            return byteArray;
        }

        public static void LireFichierParams()
        {
            if (!File.Exists(LorenzParamsFile) && !File.Exists(OldParamsFile))
            {
                CreerPseudo();
            }
            else if (File.Exists(OldParamsFile))
            {
                LireOldParams();
                return;
            }

            StringBuilder sb = new(255);

            string profile = "Profile";
            int ini11 = GetPrivateProfileString(profile, "SHOWSENDER", "", sb, sb.Capacity, LorenzParamsFile);
            ShowPseudoNameSender = sb.ToString().ToLower() == "true";
            sb.Clear();
            int ini12 = GetPrivateProfileString(profile, "PSEUDONAME", "", sb, sb.Capacity, LorenzParamsFile);
            PseudoName = sb.ToString();
            sb.Clear();

            string settings = "Settings";
            int ini21 = GetPrivateProfileString(settings, "CIPHFILEDR", "", sb, sb.Capacity, LorenzParamsFile);
            CipherFileDirectory = sb.ToString() == "" ? null : sb.ToString();
            sb.Clear();
            int ini22 = GetPrivateProfileString(settings, "TRANSTABLE", "", sb, sb.Capacity, LorenzParamsFile);
            Algorithmes.TransTableRoot = sb.ToString();
            sb.Clear();
            int ini23 = GetPrivateProfileString(settings, "SECRETABLE", "", sb, sb.Capacity, LorenzParamsFile);
            Algorithmes.BaseSecretCode = sb.ToString();
            sb.Clear();

            string compression = "Compression";
            int ini31 = GetPrivateProfileString(compression, "ACTIVCMPRS", "", sb, sb.Capacity, LorenzParamsFile);
            Compression.CompressionActive = sb.ToString().ToLower() == "true";
            sb.Clear();
            int ini32 = GetPrivateProfileString(compression, "CMPRSRATIO", "", sb, sb.Capacity, LorenzParamsFile);
            Compression.TauxCompressionMin = double.TryParse(sb.ToString(), out double ratio) ? ratio : 0.15;
        }

        public static void EcrireFichierParams()
        {
            string profile = "Profile";
            WritePrivateProfileString(profile, "SHOWSENDER", ShowPseudoNameSender.ToString(), LorenzParamsFile);
            WritePrivateProfileString(profile, "PSEUDONAME", PseudoName, LorenzParamsFile);
            string settings = "Settings";
            WritePrivateProfileString(settings, "CIPHFILEDR", CipherFileDirectory, LorenzParamsFile);
            WritePrivateProfileString(settings, "TRANSTABLE", Algorithmes.TransTableRoot, LorenzParamsFile);
            WritePrivateProfileString(settings, "SECRETABLE", Algorithmes.BaseSecretCode, LorenzParamsFile);
            string compression = "Compression";
            WritePrivateProfileString(compression, "ACTIVCMPRS", Compression.CompressionActive.ToString(), LorenzParamsFile);
            WritePrivateProfileString(compression, "CMPRSRATIO", Compression.TauxCompressionMin.ToString(), LorenzParamsFile);
        }

        private static void LireOldParams()
        {
            // Pour la rétrocompatibilité avec l'ancien fichier PARAMS.INI
            // On importe les anciens paramètres aux nouveaux de LORENZ.INI
            string[] lines = File.ReadAllLines(OldParamsFile);
            foreach (string ln in lines)
            {
                if (ln.StartsWith("SHOWSENDER"))
                {
                    string[] param = ln.Split('=');
                    if (param.Length == 2)
                    {
                        ShowPseudoNameSender = param[1].ToLower() == "true";
                    }
                }
                else if (ln.StartsWith("PSEUDONAME"))
                {
                    string[] param = ln.Split('=');
                    if (param.Length == 2)
                    {
                        PseudoName = param[1];
                    }
                }
            }
            EcrireFichierParams();

            // Suppression pour oublier l'ancienne version
            File.Delete(OldParamsFile);
        }
    }
}
