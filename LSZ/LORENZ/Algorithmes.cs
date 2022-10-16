using Cryptography;
using System;
using System.Collections.Generic;
using System.IO;

namespace LORENZ
{
    public static class Algorithmes
    {
        public static bool IsPrivateMessage { get; set; }
        public static string ThePrivateReceiverLID { get; set; } = "";
        public static bool IsThePrivateReceiver => ThePrivateReceiverLID == Parametres.LID || !IsPrivateMessage;
        public static string ThePrivateSenderLID { get; set; } = "";
        public static bool IsThePrivateSender => ThePrivateSenderLID == Parametres.LID || !IsPrivateMessage;
        public static string SenderPseudoName { get; set; }
        public static string CmdSeperator => "/*/";
        public static string TransTableRoot { get; set; } = "1234";
        public static string BaseSecretCode { get; set; } = "S8H2ALDVFP";

        public const char ATTRIB_SEP = '\xAD';
        private const int MIN_CHAR_TABLE = 32;
        private const int MAX_CHAR_TABLE = 256;

        public static string GeneratorGK()
        {
            string strGeneralKey = null;
            for (int i = 0; i < 32; i++)
            {
                int value = Cryptography.Random.RandomGeneratedNumber(0, 10);
                strGeneralKey += Convert.ToString(value);
            }
            return strGeneralKey;
        }

        public static string DeveloppGK(string messageDecrypted1)
        {
            return messageDecrypted1[..32];
        }

        public static string[,] GenerateTableCode(string theGK)
        {
            //Génération de la table vide avec caractères
            string[,] tableCode = new string[2, MAX_CHAR_TABLE - MIN_CHAR_TABLE];
            for (int charac = MIN_CHAR_TABLE; charac < MAX_CHAR_TABLE; charac++)
            {
                if (charac == '\x81')
                {
                    // Pour insérer le caractère CR (carriage return) dans une entrée vide du CHCP 1252
                    // Voir https://fr.wikipedia.org/wiki/Windows-1252
                    tableCode[0, charac - MIN_CHAR_TABLE] = Convert.ToString('\x0D');
                }
                else if (charac == '\x9D')
                {
                    // Pour insérer le caractère LF (Line feed) dans une entrée vide du CHCP 1252
                    // Voir https://fr.wikipedia.org/wiki/Windows-1252
                    tableCode[0, charac - MIN_CHAR_TABLE] = Convert.ToString('\x0A');
                }
                else
                {
                    tableCode[0, charac - MIN_CHAR_TABLE] = Convert.ToString((char)charac);
                }
            }
            //Génération des Trans
            string[] transChar = GenerateTranscriptionTable(theGK);
            //Vérification des Trans identiques
            bool identical = HaveIdenticalTransChar(transChar);
            if (identical)
            {
                tableCode = null;
            }
            else
            {
                for (int i = 0; i < MAX_CHAR_TABLE - MIN_CHAR_TABLE; i++)
                {
                    tableCode[1, i] = transChar[i];
                }
            }

            return tableCode;
        }

        private static string[] GenerateTranscriptionTable(string theGK)
        {
            // Développement de la racine
            bool chainBool1 = int.TryParse(TransTableRoot[0].ToString(), out int chain1);
            bool chainBool2 = int.TryParse(TransTableRoot[1].ToString(), out int chain2);
            bool chainBool3 = int.TryParse(TransTableRoot[2].ToString(), out int chain3);
            bool chainBool4 = int.TryParse(TransTableRoot[3].ToString(), out int chain4);

            if (!chainBool1 || !chainBool2 || !chainBool3 || !chainBool4)
            {
                throw new LORENZException("Parsing int failed.");
            }

            // Génération des string divisant le GK en 4
            string GK1 = theGK[..8];
            string GK2 = theGK.Substring(8, 8);
            string GK3 = theGK.Substring(16, 8);
            string GK4 = theGK.Substring(24, 8);

            // Création des colonnes de TRANS
            // Colonne 1
            string[] colonne1 = new string[MAX_CHAR_TABLE - MIN_CHAR_TABLE];
            GeneratorOfColumns(ref colonne1, false, chain1, 0, 64, GK1);
            GeneratorOfColumns(ref colonne1, false, Convert.ToInt32(colonne1[63]), 64, 96, GK2);
            GeneratorOfColumns(ref colonne1, true, Convert.ToInt32(colonne1[95]), 96, 160, GK4);
            GeneratorOfColumns(ref colonne1, true, Convert.ToInt32(colonne1[159]), 160, 224, GK3);

            // Colonne 2
            string[] colonne2 = new string[MAX_CHAR_TABLE - MIN_CHAR_TABLE];
            GeneratorOfColumns(ref colonne2, true, chain2, 0, 64, GK2);
            GeneratorOfColumns(ref colonne2, false, Convert.ToInt32(colonne2[63]), 64, 96, GK1);
            GeneratorOfColumns(ref colonne2, false, Convert.ToInt32(colonne2[95]), 96, 160, GK3);
            GeneratorOfColumns(ref colonne2, false, Convert.ToInt32(colonne2[159]), 160, 224, GK4);

            // Colonne 3
            string[] colonne3 = new string[MAX_CHAR_TABLE - MIN_CHAR_TABLE];
            GeneratorOfColumns(ref colonne3, false, chain3, 0, 64, GK3);
            GeneratorOfColumns(ref colonne3, true, Convert.ToInt32(colonne3[63]), 64, 96, GK4);
            GeneratorOfColumns(ref colonne3, true, Convert.ToInt32(colonne3[95]), 96, 160, GK2);
            GeneratorOfColumns(ref colonne3, false, Convert.ToInt32(colonne3[159]), 160, 224, GK1);

            // Colonne 4
            string[] colonne4 = new string[MAX_CHAR_TABLE - MIN_CHAR_TABLE];
            GeneratorOfColumns(ref colonne4, true, chain4, 0, 64, GK4);
            GeneratorOfColumns(ref colonne4, true, Convert.ToInt32(colonne4[63]), 64, 96, GK3);
            GeneratorOfColumns(ref colonne4, true, Convert.ToInt32(colonne4[95]), 96, 160, GK1);
            GeneratorOfColumns(ref colonne4, true, Convert.ToInt32(colonne4[159]), 160, 224, GK2);

            string[] transChar = new string[MAX_CHAR_TABLE - MIN_CHAR_TABLE];
            for (int i = 0; i < MAX_CHAR_TABLE - MIN_CHAR_TABLE; i++)
            {
                transChar[i] = colonne1[i] + colonne2[i] + colonne3[i] + colonne4[i];
            }

            return transChar;
        }

        private static void GeneratorOfColumns(ref string[] aTableColumn, bool order, int chain, int minLimit, int maxLimit, string GKPart)
        {
            int j = -1;
            for (int i = minLimit; i < maxLimit; i++)
            {
                j = order ? (j + 1) % 8 : (j + 7) % 8;
                chain = (chain + int.Parse(GKPart[j].ToString())) % 10;
                aTableColumn[i] = Convert.ToString(chain);
            }
        }

        private static bool HaveIdenticalTransChar(string[] aTable)
        {
            for (int c = 0; c < aTable.Length; c++)
            {
                for (int i = c + 1; i < aTable.Length; i++)
                {
                    if (aTable[c] == aTable[i])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static string AddAttributes(string s, ref double ratio)
        {
            string[] strBufferTb = s.Split(CmdSeperator, StringSplitOptions.RemoveEmptyEntries);
            string[] attributes = new string[5];
            /* Tableau des attributs :
             * [0] : P + Pseudo
             * [1] : R + LID du récepteur (lorsque privé)
             * [2] : S + LID de l'expéditeur (lorsque privé)
             * [3] : A + LID to AFDA
             * [4] : CT (Table de compression, si applicable)
             */
            attributes[0] = "P" + Parametres.PseudoName;

            string msgWithoutAttrib = default;
            for (int strInt = 0; strInt < strBufferTb.Length; strInt++)
            {
                if (strBufferTb[strInt].ToUpper().StartsWith("PRIV:"))
                {
                    string receivLID = strBufferTb[strInt]["PRIV:".Length..];
                    attributes[1] = "R" + receivLID;
                    attributes[2] = "S" + Parametres.LID;

                    RemoveArrayItem(ref strBufferTb, strInt);
                    strInt--;
                    continue;
                }
                else if (strBufferTb[strInt].ToUpper().StartsWith("AFDA:"))
                {
                    string afdaLID = strBufferTb[strInt]["AFDA:".Length..];
                    RemoveArrayItem(ref strBufferTb, strInt);
                    attributes[3] = "A" + afdaLID;
                }

                if (strInt == strBufferTb.Length - 1)
                {
                    msgWithoutAttrib += strBufferTb[strInt];
                    break;
                }

                msgWithoutAttrib += strBufferTb[strInt];
            }

            string attributeStr = default;
            foreach (string attr in attributes)
            {
                if (attr != null)
                {
                    attributeStr = attr + attributeStr;
                }
            }

            ratio = Compression.EssaiCompression(ref msgWithoutAttrib, ref attributeStr);
            return attributeStr + ATTRIB_SEP + msgWithoutAttrib;
        }

        private static void ModuloOperation(int opType, string generalKey, ref string messageWithoutGK, bool isCiphering)
        {
            string resultKey = default;
            switch (opType)
            {
                //--Inversion du GK
                case 1:
                    for (int c = generalKey.Length - 1; c >= 0; c--)
                    {
                        resultKey += generalKey[c];
                    }

                    break;
                //--Division du GK en deux parties inversées
                case 2:
                    resultKey = generalKey[(generalKey.Length / 2)..generalKey.Length] + generalKey[0..(generalKey.Length / 2)];
                    break;
                default:
                    break;
            }

            //--Modulo sur le chiffrement
            string newMessage = default;
            for (int count = 0; count < messageWithoutGK.Length; count++)
            {
                for (int j = 0; j < resultKey.Length; j++)
                {
                    if (count == messageWithoutGK.Length)
                    {
                        break;
                    }

                    int valueGK = int.Parse(resultKey[j].ToString());
                    int valueMsg = int.Parse(messageWithoutGK[count].ToString());
                    if (isCiphering)
                    {
                        newMessage += (valueMsg + valueGK) % 10;
                    }
                    else
                    {
                        newMessage += (valueMsg - valueGK + 10) % 10;
                    }

                    if (j + 1 != resultKey.Length)
                    {
                        count++;
                    }
                }
            }
            messageWithoutGK = newMessage;
        }

        private static string ModuloCipher(string generalKey, string messageWithoutGK, bool isCiphering)
        {
            ModuloOperation(1, generalKey, ref messageWithoutGK, isCiphering);
            ModuloOperation(2, generalKey, ref messageWithoutGK, isCiphering);
            return messageWithoutGK;
        }

        public static string Chiffrement(string theMessage, string generalKey, string[,] aTableCode)
        {
            //-----Partie 1 du premier chiffrement
            double cRatio = 0.0;
            theMessage = AddAttributes(theMessage, ref cRatio);
            if (cRatio != 0.0)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                if (cRatio > 0)
                {
                    Console.Write("Compression : ");
                }
                else
                {
                    Console.Write("Compression par répétitions : ");
                    cRatio = -cRatio;
                }
                Console.Write((cRatio * 100).ToString("0.0") + " %");
                Console.ResetColor();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            string theEncryptedMessage = null;
            for (int c = 0; c < theMessage.Length; c++)
            {
                string charToEvaluate = theMessage[c].ToString();
                for (int i = 0; i < MAX_CHAR_TABLE - MIN_CHAR_TABLE; i++)
                {
                    if (charToEvaluate == aTableCode[0, i])
                    {
                        theEncryptedMessage += aTableCode[1, i];
                        break;
                    }
                    if (charToEvaluate != aTableCode[0, i] && i == MAX_CHAR_TABLE - 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Le caractère " + theMessage[c] + " n'est pas supporté. Néanmoins, il a été remplacé par \"?\" pour compléter le message.");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        theEncryptedMessage += aTableCode[1, 31];
                    }
                }
            }

            //-----Retour en incluant la partie 2 du chiffrement
            return generalKey + ModuloCipher(generalKey, theEncryptedMessage, true);
        }

        public static string SecondChiffrement(string firstEncryptMsg)
        {
            char[,] secretTableCode = GenerateSTC();
            //Calcul de la somme de contrôle et positionnement dans le chiffrement
            string sommeControle = CheckSum(firstEncryptMsg);
            string complFirEncMsg = sommeControle + firstEncryptMsg;
            //Le second chiffrement
            string secondEncryptMsg = null;
            for (int c = 0; c < complFirEncMsg.Length; c++)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (complFirEncMsg[c] == secretTableCode[0, i])
                    {
                        secondEncryptMsg += secretTableCode[1, i];
                        break;
                    }
                }
            }

            return secondEncryptMsg;
        }

        private static string CheckSum(string messageToSum)
        {
            int msgSum = 0;
            for (int a = 0; a < messageToSum.Length; a++)
            {
                msgSum += messageToSum[a];
                msgSum %= 10000;
            }

            return msgSum.ToString("D4");
        }

        public static string DechiffrementPremier(string messageEncrypted, out bool isGoodCS)
        {
            isGoodCS = false;
            char[,] secretTableCode = GenerateSTC();
            string messageDecipheredFirst = null;
            int charErrorCount = 0;
            for (int c = 0; c < messageEncrypted.Length; c++)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (messageEncrypted[c] == secretTableCode[1, i])
                    {
                        messageDecipheredFirst += secretTableCode[0, i];
                        break;
                    }
                    if (messageEncrypted[c] != secretTableCode[0, i] && i == 9)
                    {
                        charErrorCount++;
                    }
                }
                if (charErrorCount > 0 && c == messageEncrypted.Length - 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Environment.NewLine + "ERREUR : Un ou des caractères sont erronés");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    return null;
                }
            }
            //Extraction de la somme de contrôle
            string CSStr = messageDecipheredFirst[0..4];
            string msgWithoutSC = messageDecipheredFirst[4..];
            int checkSumFound = int.Parse(CSStr);

            //Calcul de la somme des caractères pour valider la somme de contrôle
            for (int i = 0; i < msgWithoutSC.Length; i++)
            {
                checkSumFound -= msgWithoutSC[i];
                checkSumFound = checkSumFound < 0 ? checkSumFound + 10000 : checkSumFound;
            }
            isGoodCS = checkSumFound == 0;
            return msgWithoutSC;
        }

        private static void RemoveArrayItem(ref string[] array, params int[] indexes)
        {
            static bool containsIndex(int indexToFind, int[] array)
            {
                foreach (int value in array)
                {
                    if (indexToFind == value)
                    {
                        return true;
                    }
                }

                return false;
            }

            int shift = 0;
            string[] bufferArray = new string[array.Length - indexes.Length];
            for (int i = 0; i < bufferArray.Length; i++)
            {
                if (containsIndex(i, indexes))
                {
                    shift++;
                }

                bufferArray[i] = array[i + shift];
            }
            array = bufferArray;
        }

        private static string CheckAttributes(string s, ref double ratio)
        {
            string[] strSplited = s.Split(ATTRIB_SEP);
            if (strSplited.Length < 2)
            {
                throw new LORENZException("Erreur dans le formatage des attributs");
            }

            string msgAttributes = strSplited[0];
            string msgWithouAttrib = strSplited[1];

            /* Pour combler les morceaux qui pourraient avoir été divisés de trop
             * par un ajout intentionnel du caractère 0xAD dans le message
             */
            for (int i = 2; i < strSplited.Length; i++)
            {
                msgWithouAttrib += ATTRIB_SEP + strSplited[i];
            }

            ratio = Compression.EssaiDecompression(ref msgWithouAttrib, ref msgAttributes);

            for (int c = 0; c < msgAttributes.Length - 1; c++)
            {
                if (msgAttributes[c] == 'A')
                {
                    c++;
                    string afdaLID = msgAttributes.Substring(c, 6);
                    if (afdaLID == Parametres.LID)
                    {
                        File.Delete(Parametres.UserlogFile);
                        throw new LORENZException(ErrorCode.E0x00);
                    }
                }

                if (msgAttributes[c] == 'S')
                {
                    IsPrivateMessage = true;
                    c++;
                    ThePrivateSenderLID = msgAttributes.Substring(c, 6);
                    c += 6;

                    if (msgAttributes[c] == 'R')
                    {
                        c++;
                        ThePrivateReceiverLID = msgAttributes.Substring(c, 6);
                        c += 6;
                    }
                }

                if (msgAttributes[c] == 'P')
                {
                    c++;
                    SenderPseudoName = msgAttributes[c..];
                    c += SenderPseudoName.Length;
                }
            }

            return msgWithouAttrib;
        }

        public static string DechiffrementSecond(string[,] tableCode, string generalKey, string messageToDecrypt)
        {
            string messageWithoutGK = messageToDecrypt[generalKey.Length..];
            string NewMessageWithoutGK = ModuloCipher(generalKey, messageWithoutGK, false);

            //Extraction des trans
            List<string> ExtractedTransList = new();
            for (int i = 0; i < NewMessageWithoutGK.Length / 4; i++)
            {
                string ElementsOfOneTrans = default;
                for (int c = 0; c < 4; c++)
                {
                    ElementsOfOneTrans += NewMessageWithoutGK[4 * i + c];
                }
                ExtractedTransList.Add(ElementsOfOneTrans);
            }
            //Traitement des trans
            string decipheredMessageComplete = null;
            for (int i = 0; i < ExtractedTransList.Count; i++)
            {
                for (int c = 0; c < MAX_CHAR_TABLE - MIN_CHAR_TABLE; c++)
                {
                    if (ExtractedTransList[i] == tableCode[1, c])
                    {
                        decipheredMessageComplete += tableCode[0, c];
                        break;
                    }
                    if (ExtractedTransList[i] != tableCode[1, c] && c == MAX_CHAR_TABLE - MIN_CHAR_TABLE - 1)
                    {
                        decipheredMessageComplete += tableCode[0, 31];
                    }
                }
            }

            //Vérifier présence de commandes de contrôle
            double dRatio = 0.0;
            decipheredMessageComplete = CheckAttributes(decipheredMessageComplete, ref dRatio);
            if (dRatio != 0.0)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                if (dRatio > 0.0)
                {

                    Console.Write("Décompression : ");
                }
                else
                {
                    Console.Write("Décompression par répétitions : ");
                    dRatio = -dRatio;
                }
                Console.Write((dRatio * 100).ToString("0.0") + " %");
                Console.ResetColor();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
            }

            return decipheredMessageComplete;
        }

        private static char[,] GenerateSTC()
        {
            char[,] secretTC = new char[2, 10];
            for (int i = 0; i < 10; i++)
            {
                secretTC[0, i] = Convert.ToChar(Convert.ToString(i));
            }

            for (int i = 0; i < BaseSecretCode.Length; i++)
            {
                secretTC[1, i] = BaseSecretCode[i];
            }
            return secretTC;
        }

        public static void SetTransTable()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("ATTENTION ! Modifier la racine de la table de transcriptions sans avoir aucune");
            Console.WriteLine("connaissance approfondie du principe de chiffrement peut causer de sérieux problèmes auprès");
            Console.WriteLine("de vos correspondants, notamment au moment de la transmission.");
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Il est primordial d'informer ces derniers de toute modification sur la disposition des");
            Console.WriteLine("tables de chiffrement avant de transmettre tout nouveau message.\n");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Display.PrintMessage("Pour poursuivre, appuyez sur F10.\nAppuyez sur n'importe quelle autre touche pour annuler.",
                                 MessageState.Warning);
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key != ConsoleKey.F10)
            {
                return;
            }

            Console.CursorTop = 5;      // Pour effacer les lignes indiquant d'appuyer sur F12.
            Console.WriteLine("\nInscrivez la nouvelle racine composée de 4 chiffres décimaux (0-9).");
            Console.WriteLine("Pour annuler l'opération, appuyez sur ESC.             \n");
            Console.WriteLine("Racine actuelle de la TT : " + TransTableRoot);
            Console.Write("Nouvelle racine : ");

            string rootTemp = "";
            while (true)
            {
                int curTopInitial = Console.CursorTop;
                int curLeftInitial = Console.CursorLeft;

                ConsoleKeyInfo digit = Console.ReadKey();
                if (digit.Key == ConsoleKey.Escape)
                {
                    return;
                }
                else if (digit.Key == ConsoleKey.Enter)
                {
                    Console.SetCursorPosition(curLeftInitial, curTopInitial);
                    continue;
                }

                if (digit.Key is >= ConsoleKey.D0 and <= ConsoleKey.D9)        // chiffres du pavé standard
                {
                    rootTemp += ((int)digit.Key - 48).ToString();
                    if (rootTemp.Length < 4)
                    {
                        Console.Write("-");
                    }
                    else
                    {
                        break;
                    }
                }
                else if (digit.Key is >= ConsoleKey.NumPad0 and <= ConsoleKey.NumPad9)  // chiffres du pavé numérique
                {
                    rootTemp += ((int)digit.Key - 96).ToString();
                    if (rootTemp.Length < 4)
                    {
                        Console.Write("-");
                    }
                    else
                    {
                        break;
                    }
                }
                else if (digit.Key == ConsoleKey.Backspace && rootTemp.Length > 0)
                {
                    Console.SetCursorPosition(curLeftInitial - 2, curTopInitial);
                    Console.Write("  ");
                    Console.SetCursorPosition(curLeftInitial - 2, curTopInitial);
                    rootTemp = rootTemp[..(rootTemp.Length - 1)];
                }
                else
                {
                    Console.SetCursorPosition(curLeftInitial, curTopInitial);
                    Console.Write(' ');
                    Console.SetCursorPosition(curLeftInitial, curTopInitial);
                }
            }

            TransTableRoot = rootTemp;
            Parametres.EcrireGeneralParamsFile();
            Console.WriteLine("\nNouvelle racine : " + TransTableRoot);
            Console.WriteLine("Appuyez sur n'importe quelle touche pour continuer...");
            Console.ReadKey(true);
        }

        public static void SetSecretTable()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("ATTENTION ! Modifier la disposition de la table secrète sans avoir aucune connaissance");
            Console.WriteLine("approfondie du principe de chiffrement peut causer de sérieux problèmes auprès de vos");
            Console.WriteLine("correspondants, notamment au moment de la transmission.");
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Il est primordial d'informer ces derniers de toute modification sur la disposition des");
            Console.WriteLine("tables de chiffrement avant de transmettre tout nouveau message.\n");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Display.PrintMessage("Pour poursuivre, appuyez sur F12.\nAppuyez sur n'importe quelle autre touche pour annuler.",
                                 MessageState.Warning);

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key != ConsoleKey.F12)
            {
                return;
            }

            Console.CursorTop = 5;      // Pour effacer les lignes indiquant d'appuyer sur F12.
            Console.WriteLine("\nInscrivez la nouvelle disposition sous la forme d'une chaine de 10 caractères uniques.");
            Console.WriteLine("Pour annuler l'opération, appuyez sur ESC sans rien écrire.\n");
            Console.WriteLine("Disposition actuelle de la TS : " + BaseSecretCode);
            while (true)
            {
                Console.Write("Nouvelle disposition : ");
                string newSTSet = Extensions.SpecialPrint(maxLength: 10);
                if (newSTSet == null)
                {
                    return;
                }
                else if (newSTSet != "\r")
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
                            BaseSecretCode = newSTSet.ToUpper();
                            Parametres.EcrireGeneralParamsFile();
                            Display.PrintMessage("Nouvelle disposition : " + BaseSecretCode, MessageState.Success);
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
