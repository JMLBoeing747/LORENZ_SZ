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
                    string littleWord = divideWord[0] == bigWord ? "\x90" + divideWord[1] : divideWord[0] + "\x90";
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
                    entryCode += "C";
                }
                else
                {
                    entryCode += "L";
                }
            }

            if (entryCode == "C" + new string('L', entryCode.Length - 1))
            {
                entryCode = "T";
            }
            else if (entryCode == new string('L', entryCode.Length))
            {
                entryCode = "";
            }
            else if (entryCode == new string('C', entryCode.Length))
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

        public void Clean()
        {
            for (int we = 0; we < WordsList.Count; we++)
            {
                if (WordsList[we].CountAll == 1)
                {
                    WordsList.RemoveAt(we);
                    we--;
                }
            }
        }

        public string GetCompressCode(string entryWord)
        {
            if (entryWord.Length > 2)
            {
                foreach (WordEntry we in WordsList)
                {
                    string entryCode = we.GetEntryCode(entryWord);
                    if (entryCode != entryWord)
                    {
                        return "\x8F" + WordsList.IndexOf(we) + entryCode;
                    }
                }
            }

            return entryWord;
        }
    }

    public static class Compression
    {
        public static void TryCompression(string msgToCompress, ref string[] attributes)
        {
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

            CompressTable preCompress = new();
            foreach (string newWord in words)
            {
                preCompress.Check(newWord);
            }

            preCompress.Clean();
            preCompress.Sort();

            string newCompressStr = default;
            foreach (string w in words)
            {
                newCompressStr += preCompress.GetCompressCode(w);
            }
        }
    }
}
