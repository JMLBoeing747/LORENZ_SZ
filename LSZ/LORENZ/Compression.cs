using System.Collections.Generic;

namespace LORENZ
{
    public static class Compression
    {
        struct SubWord
        {
            public string Word { get; }
            public int Count { get; private set; }

            public SubWord(string word)
            {
                Word = word;
                Count = 1;
            }

            public void Repeat(string newWord)
            {
                if (newWord.ToLower() == Word)
                {
                    Count++;
                }
            }
        }
        struct WordEntry
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

            public void Add(string entry)
            {
                if (entry.ToLower() == MainWord)
                {
                    foreach (SubWord item in SubWordsList)
                    {
                        if (item.Word == entry)
                        {
                            item.Repeat(entry);
                            return;
                        }
                    }

                    string entryCode = EncodeWord(entry);
                    SubWord sw = new(entryCode);
                    SubWordsList.Add(sw);
                }
            }

            private string EncodeWord(string word)
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

                return entryCode;
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

            List<WordEntry> preCompress = new();
            foreach (string newWord in words)
            {
                if (preCompress.Count > 0)
                {
                    foreach (WordEntry we in preCompress)
                    {
                        if (newWord.ToLower() == we.MainWord)
                        {
                            we.Add(newWord);
                            break;
                        }
                        else if (preCompress.IndexOf(we) == preCompress.Count - 1)
                        {
                            preCompress.Add(new(newWord));
                            break;
                        }
                    }
                }
                else
                {
                    preCompress.Add(new(newWord));
                }
            }
        }
    }
}
