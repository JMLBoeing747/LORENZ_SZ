using Cryptography;
using System;
using System.Collections.Generic;

namespace LORENZ
{
    public static class Compression
    {
        public static bool CompressionActive { get; set; }
        public static double TauxCompressionMin { get; set; }

        public static double EssaiCompression(ref string msgACompress, ref string attrStr)
        {
            if (!CompressionActive)
            {
                return 0.0;
            }
            
            // Création du message complet sans compression
            string fullInitialMsg = attrStr + Algorithmes.ATTRIB_SEP + msgACompress;

            // Pré-compression pour vérifier les répétitions de caractères
            string tempMsgCompress = msgACompress;
            char repeatMarker = '\x8D';
            int repeatCount = 1;
            for (int c = 0; c < tempMsgCompress.Length; c++)
            {
                for (int d = c + 1; d < tempMsgCompress.Length; d++)
                {
                    if (tempMsgCompress[d] == tempMsgCompress[c])
                    {
                        repeatCount++;
                    }
                    else if (repeatCount > 1)
                    {
                        string compressRep = repeatMarker.ToString()
                                             + repeatCount.ToString()
                                             + "\'"
                                             + tempMsgCompress[c].ToString();
                        if (compressRep.Length < repeatCount)
                        {
                            string partBefore = tempMsgCompress[..c];
                            string partAfter = tempMsgCompress[d..];
                            tempMsgCompress = partBefore + compressRep + partAfter;
                            c += compressRep.Length - 1;
                        }

                        repeatCount = 1;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            msgACompress = tempMsgCompress;

            /* Découpage du message en mots et en ponctuations
             * pour faciliter la recherche de similitudes
             */
            List<string> words = new();
            string tempWord = default;
            bool wasSeparator = false;
            for (int i = 0; i < msgACompress.Length; i++)
            {
                char strC = msgACompress[i];
                if (i == 0)
                {
                    tempWord += strC;
                    continue;
                }

                if (strC != '\'' && IsWordSeparator(strC) || wasSeparator)
                {

                    words.Add(tempWord);
                    tempWord = default;
                    wasSeparator = IsWordSeparator(strC);
                }
                else if (strC == '\'' && IsWordSeparator(msgACompress[i + 1]))
                {
                    words.Add(tempWord);
                    tempWord = default;
                    wasSeparator = true;
                }

                tempWord += strC;
            }
            words.Add(tempWord);

            // Remplissage + triage de la table de compression
            CompressTable compressTable = new();
            foreach (string newWord in words)
            {
                compressTable.Check(newWord);
            }

            compressTable.Sort();

            // Test de compression par passes multiples
            int pass = 0;
            List<(string msg, string CT, double ratio)> passesList = new();
            while (true)
            {
                pass++;
                if (!compressTable.Clean(pass) && pass > 1)
                {
                    continue;
                }
                if (compressTable.EntriesCount == 0)
                {
                    if (passesList.Count == 0)
                    {
                        // Calcul du ratio de compression par répétitions
                        string repCompressMsg = attrStr + Algorithmes.ATTRIB_SEP + msgACompress;
                        int repDiffCount = fullInitialMsg.Length - repCompressMsg.Length;
                        double repRatio = repDiffCount / (double)fullInitialMsg.Length;
                        return -repRatio;
                    }
                    else
                    {
                        double bestRatio = 0.0;
                        int bestIndex = 0;
                        for (int compEntry = 0; compEntry < passesList.Count; compEntry++)
                        {
                            if (passesList[compEntry].ratio > bestRatio)
                            {
                                bestRatio = passesList[compEntry].ratio;
                                bestIndex = compEntry;
                            }
                        }

                        attrStr = passesList[bestIndex].CT + attrStr;
                        msgACompress = passesList[bestIndex].msg;
                        return passesList[bestIndex].ratio;
                    }
                }

                // Construction du nouveau message avec balises de compression
                string newCompressStr = default;
                foreach (string w in words)
                {
                    newCompressStr += compressTable.GetMarkupCompress(w);
                }

                string CTStr = compressTable.GetString();

                // Test de compression
                string fullCompressMsg = CTStr + attrStr + Algorithmes.ATTRIB_SEP + newCompressStr;
                int diffCount = fullInitialMsg.Length - fullCompressMsg.Length;
                double ratio = diffCount / (double)fullInitialMsg.Length;

                if (ratio >= TauxCompressionMin)
                {
                    passesList.Add((newCompressStr, CTStr, ratio));
                }
            }
        }

        public static double EssaiDecompression(ref string msgADecompress, ref string attrStr)
        {
            // Création du message complet sans compression
            string fullInitialMsg = attrStr + Algorithmes.ATTRIB_SEP + msgADecompress;

            // Pré-compression pour vérifier les répétitions de caractères
            string tempMsgCompress = msgADecompress;
            char repeatMarker = '\x8D';
            string repeatCountStr = default;
            for (int c = 0; c < tempMsgCompress.Length; c++)
            {
                if (tempMsgCompress[c] == repeatMarker)
                {
                    for (int d = c + 1; d < tempMsgCompress.Length; d++)
                    {
                        if (tempMsgCompress[d] == '\'')
                        {
                            int repeatCount = int.Parse(repeatCountStr);
                            char repeatChar = tempMsgCompress[d + 1];
                            string repeatStr = new(repeatChar, repeatCount);
                            string partBefore = tempMsgCompress[..c];
                            string partAfter = tempMsgCompress[(d + 2)..];
                            tempMsgCompress = partBefore + repeatStr + partAfter;

                            c += repeatCount - 1;
                            repeatCountStr = default;
                            break;
                        }

                        repeatCountStr += tempMsgCompress[d];
                    }
                }
            }

            msgADecompress = tempMsgCompress;

            if (attrStr[0] == 'C')
            {
                string CTStr = default;
                for (int c = 1; c < attrStr.Length; c++)
                {
                    if (attrStr[c] == ';')
                    {
                        attrStr = attrStr[(c + 1)..];
                        break;
                    }

                    CTStr += attrStr[c];
                }

                string[] CTWords = CTStr.Split(',');

                for (int c = 0; c < msgADecompress.Length; c++)
                {
                    if (msgADecompress[c] == CompressTable.COMPRESS_MARKUP)
                    {
                        string indexStr = default;
                        for (int d = c + 1; d < msgADecompress.Length; d++)
                        {
                            if (msgADecompress[d] is < '0' or > '9')
                            {
                                int indexCT = int.Parse(indexStr);
                                string wordSelect = CTWords[indexCT];
                                switch (msgADecompress[d])
                                {
                                    case 'T':
                                        wordSelect = (char)(wordSelect[0] - 0x20) + wordSelect[1..];
                                        break;
                                    case 'U':
                                        wordSelect = wordSelect.ToUpper();
                                        break;
                                    case 'I':
                                        wordSelect = wordSelect[0] + wordSelect[1..].ToUpper();
                                        break;
                                    case 'X':
                                        break;
                                    default:
                                        d--;
                                        break;
                                }

                                if (msgADecompress[d] is 'T' or 'U' or 'I' or 'X')
                                {
                                    string wordApostrophe = "";
                                    bool markupFound = false;
                                    int e;
                                    for (e = d + 1; e < msgADecompress.Length; e++)
                                    {
                                        if (msgADecompress[e] == WordEntry.WORD_MARKUP)
                                        {
                                            if (wordApostrophe.Length > 0)
                                            {
                                                e++;
                                                wordSelect = wordApostrophe + "'" + wordSelect;
                                                break;
                                            }

                                            markupFound = true;
                                        }
                                        else if (msgADecompress[e] is (>= '0' and <= '9') or
                                                                      (>= 'A' and <= 'Z') or
                                                                      (>= 'a' and <= 'z') or
                                                                      (>= '\xC0'))
                                        {
                                            wordApostrophe += msgADecompress[e];
                                        }
                                        else if (markupFound)
                                        {
                                            wordSelect += "'" + wordApostrophe;
                                            break;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    d = e - 1;
                                }

                                d++;
                                string partBefore = msgADecompress[..c];
                                string partAfter = msgADecompress[d..];
                                msgADecompress = partBefore + wordSelect + partAfter;

                                c += wordSelect.Length - 1;
                                break;
                            }

                            indexStr += msgADecompress[d];
                        }
                    }
                }

                // Calcul du ratio de décompression
                string fullDecompressMsg = attrStr + Algorithmes.ATTRIB_SEP + msgADecompress;
                int diffCount = fullDecompressMsg.Length - fullInitialMsg.Length;
                double ratio = diffCount / (double)fullDecompressMsg.Length;
                return ratio;
            }
            else
            {
                // Calcul du ratio de décompression par répétitions
                string repDecompressMsg = attrStr + Algorithmes.ATTRIB_SEP + msgADecompress;
                int repDiffCount = repDecompressMsg.Length - fullInitialMsg.Length;
                double repRatio = repDiffCount / (double)repDecompressMsg.Length;
                return -repRatio;
            }
        }

        private static bool IsWordSeparator(char toEval)
        {
            if (toEval == '-')
            {
                return false;
            }
            else if (toEval is (< '0' or > '9') and
                               (< 'A' or > 'Z') and
                               (< 'a' or > 'z') and
                               (< '\xC0'))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void ModifierTaux()
        {
            Console.Clear();
            Console.WriteLine("Modification du taux de compression");
            Console.WriteLine("\nInscrivez le taux de compression minimum que vous désirez obtenir");
            Console.WriteLine("lors de vos futurs chiffrements.");
            Display.PrintMessage("Taux de compression actuel : " + (TauxCompressionMin * 100).ToString("0.0") + "%",
                                 MessageState.Warning);

            string newRatioStr = default;
            double newRatio;
            do
            {
                if (newRatioStr != default)
                {
                    Display.PrintMessage("Ceci n'est pas un pourcentage valide.", MessageState.Failure);
                }

                Console.Write("Nouveau taux : ");
                newRatioStr = Extensions.SpecialPrint();
                if (newRatioStr is null or "")
                {
                    return;
                }
            } while (!double.TryParse(newRatioStr, out newRatio));

            TauxCompressionMin = newRatio / 100;
            Parametres.EcrireGeneralParamsFile();
            Display.PrintMessage("Nouveau taux enregistré : " + (TauxCompressionMin * 100).ToString("0.0") + "%",
                                 MessageState.Success);
            Console.WriteLine("Appuyez sur une touche pour terminer...");
            Console.ReadKey(true);
        }
    }

    public class CompressTable
    {
        private readonly List<WordEntry> WordsList;
        public int EntriesCount => WordsList.Count;

        public const char COMPRESS_MARKUP = '\x8F';

        public CompressTable()
        {
            WordsList = new();
        }

        public void Check(string newWord)
        {
            if (newWord.Length > 2)
            {
                if (WordsList.Count > 0)
                {
                    foreach (WordEntry we in WordsList)
                    {
                        if (we.Add(newWord))
                        {
                            break;
                        }
                        else if (WordsList.IndexOf(we) == WordsList.Count - 1)
                        {
                            WordsList.Add(new(newWord));
                            break;
                        }
                    }
                }
                else
                {
                    WordsList.Add(new(newWord));
                }
            }
        }

        public void Sort()
        {
            for (int we1 = 0; we1 < WordsList.Count; we1++)
            {
                WordsList[we1].Sort();
                for (int we2 = we1 - 1; we2 >= 0; we2--)
                {
                    if (WordsList[we2 + 1].CountAll > WordsList[we2].CountAll)
                    {
                        (WordsList[we2], WordsList[we2 + 1]) = (WordsList[we2 + 1], WordsList[we2]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public bool Clean(int level = 1)
        {
            if (level < 1)
            {
                return false;
            }

            bool isModified = false;
            for (int we = 0; we < WordsList.Count; we++)
            {
                if (WordsList[we].CountAll <= level)
                {
                    WordsList.RemoveAt(we);
                    we--;
                    isModified = true;
                }
                else
                {
                    bool isRebased = WordsList[we].Rebase();
                    isModified = isRebased || isModified;
                }
            }

            return isModified;
        }

        public string GetMarkupCompress(string entryWord)
        {
            if (entryWord.Length > 2)
            {
                foreach (WordEntry we in WordsList)
                {
                    string entryCode = we.GetEntryCode(entryWord);
                    if (entryCode != entryWord)
                    {
                        return COMPRESS_MARKUP.ToString() + WordsList.IndexOf(we) + entryCode;
                    }
                }
            }

            return entryWord;
        }

        public string GetString()
        {
            string temp = "C";
            for (int we = 0; we < WordsList.Count; we++)
            {
                temp += WordsList[we].MainWord;
                if (we == WordsList.Count - 1)
                {
                    temp += ";";
                }
                else
                {
                    temp += ",";
                }
            }

            return temp;
        }
    }

    public class WordEntry
    {
        public string MainWord { get; private set; }
        private readonly List<SubWord> SubWordsList;
        public const char WORD_MARKUP = '\x90';

        public WordEntry(string entry)
        {
            SubWordsList = new();
            MainWord = default;
            Add(entry);
        }

        public int CountAll
        {
            get
            {
                int temp = 0;
                foreach (SubWord item in SubWordsList)
                {
                    temp += item.Count;
                }

                return temp;
            }
        }

        public bool Add(string entry)
        {
            string[] divideWord = entry.Split('\'');
            if (divideWord.Length == 2)
            {
                string bigWord = divideWord[0].Length > divideWord[1].Length ? divideWord[0] : divideWord[1];
                if (SubWordsList.Count == 0)
                {
                    MainWord = bigWord.ToLower();
                }

                if (bigWord.ToLower() != MainWord)
                {
                    return false;
                }
                else
                {
                    string littleWord = divideWord[0] == bigWord ? WORD_MARKUP + divideWord[1] : divideWord[0] + WORD_MARKUP;
                    foreach (SubWord item in SubWordsList)
                    {
                        if (item.WordName == entry)
                        {
                            item.Repeat(entry);
                            return true;
                        }
                    }

                    string entryCode = EncodeWord(bigWord, littleWord);
                    SubWord sw = new(entry, entryCode);
                    SubWordsList.Add(sw);
                }
            }
            else if ((entry.ToLower() == MainWord) || (SubWordsList.Count == 0))
            {
                if (SubWordsList.Count == 0)
                {
                    MainWord = entry.ToLower();
                }

                foreach (SubWord item in SubWordsList)
                {
                    if (item.WordName == entry)
                    {
                        item.Repeat(entry);
                        return true;
                    }
                }

                string entryCode = EncodeWord(entry);
                SubWord sw = new(entry, entryCode);
                SubWordsList.Add(sw);
            }
            else
            {
                return false;
            }

            return true;
        }

        private string EncodeWord(string word, string part = null)
        {
            string entryCode = default;
            for (int cWord = 0; cWord < word.Length; cWord++)
            {
                if (MainWord[cWord] == (word[cWord] + 0x20))
                {
                    entryCode += "U";
                }
                else
                {
                    entryCode += "L";
                }
            }

            if (entryCode == new string('L', entryCode.Length))
            {
                entryCode = "";
            }
            else if (entryCode == "U" + new string('L', entryCode.Length - 1))
            {
                entryCode = "T";
            }
            else if (entryCode == new string('U', entryCode.Length))
            {
                entryCode = "U";
            }
            else if (entryCode == "L" + new string('U', entryCode.Length - 1))
            {
                entryCode = "I";
            }

            if (part != null)
            {
                if (entryCode == "")
                {
                    entryCode += "X" + part;
                }
                else
                {
                    entryCode += part;
                }
            }

            return entryCode;
        }

        public void Sort()
        {
            for (int sw1 = 1; sw1 < SubWordsList.Count; sw1++)
            {
                for (int sw2 = sw1 - 1; sw2 >= 0; sw2--)
                {
                    if (SubWordsList[sw2 + 1].Count > SubWordsList[sw2].Count)
                    {
                        (SubWordsList[sw2], SubWordsList[sw2 + 1]) = (SubWordsList[sw2 + 1], SubWordsList[sw2]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public bool Rebase()
        {
            if (SubWordsList.Count == 1)
            {
                if (SubWordsList[0].WordName == MainWord)
                {
                    return false;
                }

                string uniqueWord = SubWordsList[0].WordName;
                int uniqueWordCount = SubWordsList[0].Count;
                SubWordsList.Clear();

                MainWord = uniqueWord;
                string uniqueWordCode = EncodeWord(uniqueWord);
                SubWord uniqueSubWord = new(uniqueWord, uniqueWordCode);
                for (int i = 1; i < uniqueWordCount; i++)
                {
                    uniqueSubWord.Repeat(uniqueWord);
                }

                SubWordsList.Add(uniqueSubWord);
                return true;
            }
            else if (SubWordsList.Count > 1)
            {
                // Vérifie si tous les sous-mots ont le même mot principal
                string commonWord = default;
                foreach (SubWord sw in SubWordsList)
                {
                    if (commonWord == default)
                    {
                        commonWord = sw.WordName.ToLower();
                    }
                    else if (commonWord != sw.WordName.ToLower())
                    {
                        return false;
                    }
                }

                List<string> tempSW = new();
                List<int> tempCounts = new();
                foreach (SubWord oldSW in SubWordsList)
                {
                    tempSW.Add(oldSW.WordName);
                    tempCounts.Add(oldSW.Count);
                }
                SubWordsList.Clear();

                MainWord = commonWord;
                for (int s = 0; s < tempSW.Count; s++)
                {
                    string commonWordCode = EncodeWord(tempSW[s]);
                    SubWord commonSubWord = new(tempSW[s], commonWordCode);
                    for (int i = 1; i < tempCounts[s]; i++)
                    {
                        commonSubWord.Repeat(tempSW[s]);
                    }

                    SubWordsList.Add(commonSubWord);
                }

                return true;
            }

            return false;
        }

        public string GetEntryCode(string entryWord)
        {
            if (entryWord.Length > 2)
            {
                foreach (SubWord sw in SubWordsList)
                {
                    if (sw.WordName == entryWord)
                    {
                        return sw.WordCode;
                    }
                }
            }

            return entryWord;
        }
    }

    public class SubWord
    {
        public string WordName { get; }
        public string WordCode { get; }
        public int Count { get; private set; }

        public SubWord(string wordName, string wordCode)
        {
            WordName = wordName;
            WordCode = wordCode;
            Count = 1;
        }

        public void Repeat(string newWord)
        {
            if (newWord == WordName)
            {
                Count++;
            }
        }
    }
}
