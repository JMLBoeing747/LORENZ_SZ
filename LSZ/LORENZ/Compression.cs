using System.Collections.Generic;

namespace LORENZ
{
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

    public class WordEntry
    {
        public string MainWord { get; private set; }
        private readonly List<SubWord> SubWordsList;
        private const char WORD_MARKUP = '\x90';

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
                if (MainWord[cWord] != word[cWord])
                {
                    entryCode += "1";
                }
                else
                {
                    entryCode += "0";
                }
            }

            if (entryCode == new string('0', entryCode.Length))
            {
                entryCode = "";
            }
            else if (entryCode == "1" + new string('0', entryCode.Length - 1))
            {
                entryCode = "T";
            }
            else if (entryCode == new string('1', entryCode.Length))
            {
                entryCode = "U";
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

    public class CompressTable
    {
        private readonly List<WordEntry> WordsList;
        public int EntriesCount => WordsList.Count;

        private const char COMPRESS_MARKUP = '\x8F';

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

    public static class Compression
    {
        public static double MinCompressRatio { get; set; } = 0.15;

        public static double TryCompression(ref string msgToCompress, ref string attrStr)
        {
            // Création du message complet sans compression
            string fullInitialMsg = attrStr + Algorithmes.ATTRIB_SEP + msgToCompress;

            /* Découpage du message en mots et en ponctuations
             * pour faciliter la recherche de similitudes
             */
            List<string> words = new();
            string tempWord = default;
            bool wasSeparator = false;
            for (int i = 0; i < msgToCompress.Length; i++)
            {
                char strC = msgToCompress[i];
                if (i == 0)
                {
                    tempWord += strC;
                    continue;
                }

                if (strC is (< '0' or > '9') and
                    (< 'A' or > 'Z') and
                    (< 'a' or > 'z') and
                    (< '\xC0') and not '\'')
                {
                    words.Add(tempWord);
                    tempWord = default;
                    wasSeparator = true;
                }
                else
                {
                    if (wasSeparator)
                    {
                        words.Add(tempWord);
                        tempWord = default;
                        wasSeparator = false;
                    }
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
            int pass = 0;
            while (true)
            {
                pass++;
                if (!compressTable.Clean(pass))
                {
                    continue;
                }
                else if (compressTable.EntriesCount == 0)
                {
                    return 0.0;
                }

                // Construction du nouveau message avec balises de compression
                string newCompressStr = default;
                foreach (string w in words)
                {
                    newCompressStr += compressTable.GetMarkupCompress(w);
                }

                string CTStr = compressTable.GetString();

                string fullCompressMsg = CTStr + attrStr + Algorithmes.ATTRIB_SEP + newCompressStr;
                int diffCount = fullInitialMsg.Length - fullCompressMsg.Length;
                // Test de compression
                double ratio = diffCount / (double)fullInitialMsg.Length;

                if (ratio >= MinCompressRatio)
                {
                    attrStr = CTStr + attrStr;
                    msgToCompress = newCompressStr;
                    return ratio;
                }
            }
        }
    }
}
