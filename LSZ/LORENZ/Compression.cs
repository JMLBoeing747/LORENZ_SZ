using System.Collections.Generic;

namespace LORENZ
{
    public static class Compression
    {
        private struct SubWord
        {
            public string WordCode { get; }
            public int Count { get; private set; }

            public SubWord(string wordCode)
            {
                WordCode = wordCode;
                Count = 1;
            }

            public void Repeat(string newWord)
            {
                if (newWord == WordCode)
                {
                    Count++;
                }
            }
        }
        private struct WordEntry
        {
            public string MainWord { get; private set; }
            private readonly List<SubWord> SubWordsList;

            public WordEntry(string entry)
            {
                SubWordsList = new();
                MainWord = entry.ToLower();
                string entryCode = EncodeWord(entry);
                SubWord sw = new(entryCode);
                SubWordsList.Add(sw);
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
                    if (bigWord.ToLower() != MainWord)
                    {
                        return false;
                    }
                    else
                    {
                        string littleWord = divideWord[0] == bigWord ? "\x90" + divideWord[1] : divideWord[0] + "\x90";
                        string entryCode = EncodeWord(bigWord, littleWord);
                        foreach (SubWord item in SubWordsList)
                        {
                            if (item.WordCode == entryCode)
                            {
                                item.Repeat(entryCode);
                                return true;
                            }
                        }

                        SubWord sw = new(entryCode);
                        SubWordsList.Add(sw);
                    }
                }
                else if (entry.ToLower() == MainWord)
                {
                    string entryCode = EncodeWord(entry);
                    foreach (SubWord item in SubWordsList)
                    {
                        if (item.WordCode == entryCode)
                        {
                            item.Repeat(entryCode);
                            return true;
                        }
                    }

                    SubWord sw = new(entryCode);
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
        }

        private struct CompressTable
        {
            private readonly List<WordEntry> WordsList;

            public CompressTable()
            {
                WordsList = new List<WordEntry>();
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
        }

        public static void TryCompression(string strToCompress)
        {
            List<string> words = new();
            string tempWord = default;
            bool wasSeparator = false;
            for (int i = 0; i < strToCompress.Length; i++)
            {
                char strC = strToCompress[i];
                if (strC is (< '0' or > '9') and
                    (< 'A' or > 'Z') and
                    (< 'a' or > 'z') and
                    (< '\xC0') and not '\'')
                {
                    words.Add(tempWord);
                    tempWord = default;
                    wasSeparator = true;
                    tempWord += strC;
                }
                else
                {
                    if (wasSeparator)
                    {
                        words.Add(tempWord);
                        tempWord = default;
                        wasSeparator = false;
                    }

                    tempWord += strC;
                }
            }

            CompressTable preCompress = new();
            foreach (string newWord in words)
            {
                preCompress.Check(newWord);
            }
        }
    }
}
