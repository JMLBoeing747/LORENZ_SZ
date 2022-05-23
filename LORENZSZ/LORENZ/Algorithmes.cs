using System;
using System.Collections.Generic;
using System.IO;

namespace LORENZ
{
    public static class Algorithmes
    {
        public static string ToUIDPrivateMeg { get; set; } = Parametres.LID;
        public static bool IsPrivateMessage { get; set; } = false;
        public static string SenderPseudoName { get; set; }
        public static string CmdSeperator { get => "/*/"; }
        public static bool IsGoodCheckSum { get; set; }
        
        private const int MAX_CHAR_TABLE = 224;

        public static string GeneratorGK()
        {
            string StrGeneralKey = null;
            for (int i = 0; i < 32; i++)
            {
                int Value = Cryptography.Random.RandomGeneratedNumber(0, 10);
                StrGeneralKey += Convert.ToString(Value);
            }
            return StrGeneralKey;
        }

        public static string DeveloppGK(string MessageDecrypted1)
        {
            string StrGeneralKey = null;
            for (int i = 0; i < 32; i++)
                StrGeneralKey += Convert.ToString(MessageDecrypted1[i]);
            return StrGeneralKey;
        }

        public static string[,] GenerateTableCode(string TheGK)
        {
            //Génération de la table vide avec caractères
            string[,] TableCode = new string[2, MAX_CHAR_TABLE];
            for (int charac = 32; charac < MAX_CHAR_TABLE; charac++)
            {
                if (charac == '\x81')
                {
                    // Pour insérer le caractère CR (carriage return) dans une entrée vide du CHCP 1252
                    // Voir https://fr.wikipedia.org/wiki/Windows-1252
                    TableCode[0, charac - 32] = Convert.ToString('\x0D');
                }
                else if (charac == '\x9D')
                {
                    // Pour insérer le caractère LF (Line feed) dans une entrée vide du CHCP 1252
                    // Voir https://fr.wikipedia.org/wiki/Windows-1252
                    TableCode[0, charac - 32] = Convert.ToString('\x0A');
                }
                else
                {
                    TableCode[0, charac - 32] = Convert.ToString((char)charac);
                }
            }
            //Génération des Trans
            string[] TransChar = GenerateTranscriptionTable(TheGK);
            //Vérification des Trans identiques
            bool Identical = HaveIdenticalTransChar(TransChar);
            if (Identical)
                TableCode = null;
            else
                for (int i = 0; i < MAX_CHAR_TABLE; i++)
                    TableCode[1, i] = TransChar[i];
            return TableCode;
        }

        private static string[] GenerateTranscriptionTable(string TheGK)
        {
            //Génération des string divisant le GK en 4
            string GK1 = TheGK.Substring(0, 8);
            string GK2 = TheGK.Substring(8, 8);
            string GK3 = TheGK.Substring(16, 8);
            string GK4 = TheGK.Substring(24, 8);

            //Création des colonnes de TRANS
            //Colonne 1
            string[] Colonne1 = new string[MAX_CHAR_TABLE];
            GeneratorOfColumns(ref Colonne1, false, 1, 0, 64, GK1);
            GeneratorOfColumns(ref Colonne1, false, Convert.ToInt32(Colonne1[63]), 64, 96, GK2);
            GeneratorOfColumns(ref Colonne1, true, Convert.ToInt32(Colonne1[95]), 96, 160, GK4);
            GeneratorOfColumns(ref Colonne1, true, Convert.ToInt32(Colonne1[159]), 160, 224, GK3);

            //Colonne 2
            string[] Colonne2 = new string[MAX_CHAR_TABLE];
            GeneratorOfColumns(ref Colonne2, true, 2, 0, 64, GK2);
            GeneratorOfColumns(ref Colonne2, false, Convert.ToInt32(Colonne2[63]), 64, 96, GK1);
            GeneratorOfColumns(ref Colonne2, false, Convert.ToInt32(Colonne2[95]), 96, 160, GK3);
            GeneratorOfColumns(ref Colonne2, false, Convert.ToInt32(Colonne2[159]), 160, 224, GK4);

            //Colonne 3
            string[] Colonne3 = new string[MAX_CHAR_TABLE];
            GeneratorOfColumns(ref Colonne3, false, 3, 0, 64, GK3);
            GeneratorOfColumns(ref Colonne3, true, Convert.ToInt32(Colonne3[63]), 64, 96, GK4);
            GeneratorOfColumns(ref Colonne3, true, Convert.ToInt32(Colonne3[95]), 96, 160, GK2);
            GeneratorOfColumns(ref Colonne3, false, Convert.ToInt32(Colonne3[159]), 160, 224, GK1);

            //Colonne 4
            string[] Colonne4 = new string[MAX_CHAR_TABLE];
            GeneratorOfColumns(ref Colonne4, true, 4, 0, 64, GK4);
            GeneratorOfColumns(ref Colonne4, true, Convert.ToInt32(Colonne4[63]), 64, 96, GK3);
            GeneratorOfColumns(ref Colonne4, true, Convert.ToInt32(Colonne4[95]), 96, 160, GK1);
            GeneratorOfColumns(ref Colonne4, true, Convert.ToInt32(Colonne4[159]), 160, 224, GK2);

            string[] TransChar = new string[MAX_CHAR_TABLE];
            for (int i = 0; i < MAX_CHAR_TABLE; i++)
            {
                TransChar[i] = Colonne1[i] + Colonne2[i] + Colonne3[i] + Colonne4[i];
            }

            return TransChar;
        }

        private static void GeneratorOfColumns(ref string[] ATableColumn, bool order, int chain, int MinLimit, int MaxLimit, string GKPart)
        {
            int j = -1;
            for (int i = MinLimit; i < MaxLimit; i++)
            {
                j = order ? (j + 1) % 8 : (j + 7) % 8;
                chain = (chain + int.Parse(GKPart[j].ToString())) % 10;
                ATableColumn[i] = Convert.ToString(chain);
            }
        }

        private static bool HaveIdenticalTransChar(string[] ATable)
        {
            for (int c = 0; c < ATable.Length; c++)
                for (int i = c + 1; i < ATable.Length; i++)
                    if (ATable[c] == ATable[i])
                        return true;
            return false;
        }

        static string CheckControlSender(string s)
        {
            string[] strBufferTb = s.Split(CmdSeperator, StringSplitOptions.RemoveEmptyEntries);
            string strConcat = default;
            for (int str = 0; str < strBufferTb.Length; str++)
            {
                if (strBufferTb[str].ToUpper().StartsWith("SENDER:"))
                    RemoveArrayItem(ref strBufferTb, str);
                if (str >= strBufferTb.Length)
                    break;
                strConcat += strBufferTb[str] + CmdSeperator;
            }
            return strConcat;
        }

        static void ModuloOperation(int opType, string generalKey, ref string messageWithoutGK, bool isCiphering)
        {
            string resultKey = default;
            switch (opType)
            {
                //--Inversion du GK
                case 1:
                    for (int c = generalKey.Length - 1; c >= 0; c--)
                        resultKey += generalKey[c];
                    break;
                //--Division du GK en deux parties inversées
                case 2:
                    resultKey = generalKey[(generalKey.Length / 2)..generalKey.Length] + generalKey[0..(generalKey.Length / 2)];
                    break;
                default:
                    break;
            }

            //--Modulo sur le chiffrement
            string NewMessage = default;
            for (int count = 0; count < messageWithoutGK.Length; count++)
            {
                for (int j = 0; j < resultKey.Length; j++)
                {
                    if (count == messageWithoutGK.Length)
                        break;
                    int valueGK = int.Parse(resultKey[j].ToString());
                    int valueMsg = int.Parse(messageWithoutGK[count].ToString());
                    if (isCiphering)
                        NewMessage += (valueMsg + valueGK) % 10;
                    else
                        NewMessage += (valueMsg - valueGK + 10) % 10;
                    if (j + 1 != resultKey.Length)
                        count++;
                }
            }
            messageWithoutGK = NewMessage;
        }

        static string ModuloCipher(string generalKey, string messageWithoutGK, bool isCiphering)
        {
            ModuloOperation(1, generalKey, ref messageWithoutGK, isCiphering);
            ModuloOperation(2, generalKey, ref messageWithoutGK, isCiphering);
            return messageWithoutGK;
        }

        public static string Chiffrement(string TheMessage, string generalKey, string[,] ATableCode)
        {
            //-----Partie 1 du premier chiffrement
            TheMessage = CheckControlSender(TheMessage);
            TheMessage = CmdSeperator + "SENDER:" + Parametres.PseudoName + CmdSeperator + TheMessage;
            string TheEncryptedMessage = null;
            for (int c = 0; c < TheMessage.Length; c++)
            {
                string CharToEvaluate = Convert.ToString(TheMessage[c]);
                for (int i = 0; i < MAX_CHAR_TABLE; i++)
                {
                    if (CharToEvaluate == ATableCode[0, i])
                    {
                        TheEncryptedMessage += ATableCode[1, i];
                        break;
                    }
                    if (CharToEvaluate != ATableCode[0, i] && i == MAX_CHAR_TABLE - 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Le caractère " + TheMessage[c] + " n'est pas supporté. Néanmoins, il a été replacé par \"?\" pour compléter le message.");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        TheEncryptedMessage += ATableCode[1, 31];
                    }
                }
            }

            //-----Retour en incluant la partie 2 du chiffrement
            return generalKey + ModuloCipher(generalKey, TheEncryptedMessage, true);
        }

        public static string SecondChiffrement(string FirstEncryptedMessage)
        {
            char[,] SecretTableCode = GenerateSTC();
            //Calcul de la somme de contrôle et positionnement dans le chiffrement
            string SommeControle = CheckSum(FirstEncryptedMessage);
            string CompleteFirstEncryptedMessage = SommeControle + FirstEncryptedMessage;
            //Le second chiffrement
            string TheSecondEncryptedMessage = null;
            for (int c = 0; c < CompleteFirstEncryptedMessage.Length; c++)
                for (int i = 0; i < 10; i++)
                    if (CompleteFirstEncryptedMessage[c] == SecretTableCode[0, i])
                    {
                        TheSecondEncryptedMessage += SecretTableCode[1, i];
                        break;
                    }
            return TheSecondEncryptedMessage;
        }

        private static string CheckSum(string MessageToSum)
        {
            int MessageSum = 0;
            for (int a = 0; a < MessageToSum.Length; a++)
            {
                MessageSum += Convert.ToInt32(MessageToSum[a]);
                if (MessageSum > 9999)
                    MessageSum %= 10000;
            }
            string TheMSumStr = null;
            if (MessageSum >= 0 && MessageSum < 10)
            {
                TheMSumStr = "0" + "0" + "0" + Convert.ToString(MessageSum);
            }
            if (MessageSum >= 10 && MessageSum < 100)
            {
                TheMSumStr = "0" + "0" + Convert.ToString(MessageSum);
            }
            if (MessageSum >= 100 && MessageSum < 1000)
            {
                TheMSumStr = "0" + Convert.ToString(MessageSum);
            }
            if (MessageSum >= 1000 && MessageSum < 10000)
            {
                TheMSumStr = Convert.ToString(MessageSum);
            }
            return TheMSumStr;
        }

        public static string DechiffrementPremier(string MessageEncrypted2)
        {
            char[,] SecretTableCode = GenerateSTC();
            string MessageDecipheredFirst = null;
            string MessageDecryptedFirstSCout = null;
            int CharError = 0;
            for (int c = 0; c < MessageEncrypted2.Length; c++)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (MessageEncrypted2[c] == SecretTableCode[1, i])
                    {
                        MessageDecipheredFirst += SecretTableCode[0, i];
                        break;
                    }
                    if (MessageEncrypted2[c] != SecretTableCode[0, i] && i == 9)
                        CharError++;
                }
                if (CharError > 0 && c == MessageEncrypted2.Length - 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Environment.NewLine + "ERREUR : Un ou des caractères sont erronés");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    return null;
                }
            }
            //Extraction de la somme de contrôle
            string StrOfCS = null;
            for (int a = 0; a < 4; a++)
            {
                StrOfCS += MessageDecipheredFirst[a];
            }
            int CheckSumFound = Convert.ToInt32(StrOfCS);
            //Calcul de la somme des caractères pour valider la somme de contrôle
            int CalculatedCS = 0;
            for (int i = 4; i < MessageDecipheredFirst.Length; i++)
            {
                CalculatedCS += Convert.ToInt32(MessageDecipheredFirst[i]);
                MessageDecryptedFirstSCout += MessageDecipheredFirst[i];
                CalculatedCS %= 10000;
            }
            IsGoodCheckSum = CalculatedCS == CheckSumFound;
            return MessageDecryptedFirstSCout;
        }

        static void RemoveArrayItem(ref string[] array, params int[] indexes)
        {
            static bool containsIndex(int indexToFind, int[] array)
            {
                foreach (int value in array)
                    if (indexToFind == value)
                        return true;
                return false;
            }

            int shift = 0;
            string[] bufferArray = new string[array.Length - indexes.Length];
            for (int i = 0; i < bufferArray.Length; i++)
            {
                if (containsIndex(i, indexes))
                    shift++;
                bufferArray[i] = array[i + shift];
            }
            array = bufferArray;
        }

        static bool IsControlCmd(string s)
        {
            if (s.ToUpper().Contains("SENDER:"))
            {
                SenderPseudoName = s["SENDER:".Length..];
                return true;
            }
            else if (s.ToUpper().Contains("SHOW:"))
                return true;
            else if (s.ToUpper().Contains("PRIV:"))
            {
                IsPrivateMessage = true;
                ToUIDPrivateMeg = s["PRIV:".Length..];
                return true;
            }
            else if (s.ToUpper().Contains("AFDA:"))
            {
                if (s["AFDA:".Length..] == Parametres.LID)
                {
                    File.Delete(Parametres.UserlogFile);
                    throw new LORENZException(ErrorCode.E0x00);
                }
                return true;
            }
            else return false;
        }

        static string CheckControlAndFinalize(string s)
        {
            string[] strBufferTb = s.Split(CmdSeperator, StringSplitOptions.RemoveEmptyEntries);
            string strConcat = default;
            for (int str = 0; str < strBufferTb.Length; str++)
            {
                while (str < strBufferTb.Length && IsControlCmd(strBufferTb[str]))
                    RemoveArrayItem(ref strBufferTb, str);
                if (str >= strBufferTb.Length)
                    break;
                strConcat += strBufferTb[str];
            }
            return strConcat;
        }

        public static string DechiffrementSecond(string[,] TableCode, string generalKey, string MessageToDecrypt)
        {
            string MessageWithoutGK = MessageToDecrypt[generalKey.Length..];
            string NewMessageWithoutGK = ModuloCipher(generalKey, MessageWithoutGK, false);

            //Extraction des trans
            List<string> ExtractedTransList = new List<string>();
            for (int i = 0; i < NewMessageWithoutGK.Length / 4; i++)
            {
                string ElementsOfOneTrans = default;
                //List<string> ElementsOfOneTrans = new List<string>();
                for (int c = 0; c < 4; c++)
                {
                    ElementsOfOneTrans += NewMessageWithoutGK[4 * i + c];
                    //ElementsOfOneTrans.Add(Convert.ToString(MessageWithoutGK[4 * i + c]));
                }
                ExtractedTransList.Add(ElementsOfOneTrans);
            }
            //Traitement des trans
            string DecipheredMessageComplete = null;
            for (int i = 0; i < ExtractedTransList.Count; i++)
            {
                for (int c = 0; c < MAX_CHAR_TABLE; c++)
                {
                    if (ExtractedTransList[i] == TableCode[1, c])
                    {
                        DecipheredMessageComplete += TableCode[0, c];
                        break;
                    }
                    if (ExtractedTransList[i] != TableCode[1, c] && c == MAX_CHAR_TABLE - 1)
                    {
                        DecipheredMessageComplete += TableCode[0, 31];
                    }
                }
            }

            //Vérifier présence de commandes de contrôle
            DecipheredMessageComplete = CheckControlAndFinalize(DecipheredMessageComplete);
            return DecipheredMessageComplete;
        }

        private static char[,] GenerateSTC()
        {
            char[,] SecretTC = new char[2, 10];
            for (int i = 0; i < 10; i++)
            {
                SecretTC[0, i] = Convert.ToChar(Convert.ToString(i));
            }
            string BaseSecretCode = "S8H2ALDVFP";
            for (int i = 0; i < BaseSecretCode.Length; i++)
            {
                SecretTC[1, i] = BaseSecretCode[i];
            }
            return SecretTC;
        }
    }
}
