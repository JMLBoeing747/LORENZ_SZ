using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                Count = 0;
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
            private List<SubWord> SubWordsList;
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
            public WordEntry(string entry)
            {
                SubWordsList = new();
                MainWord = entry.ToLower();
                SubWordsList.Add(new(MainWord));
            }

            public void Add(string subEntry)
            {
                if (subEntry.ToLower() == MainWord)
                {
                    foreach (SubWord item in SubWordsList)
                    {
                        if (item.Word == subEntry)
                        {
                            item.Repeat(subEntry);
                            return;
                        }
                    }

                    SubWordsList.Add(new(subEntry));
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
                    (< 'a' or > 'z') and not '\'')
                {
                    if (tempWord.Length > 2)
                    {
                        words.Add(tempWord);
                    }
                    tempWord = default;
                    wasSeparator = true;
                    tempWord += strC;
                }
                else
                {
                    if (wasSeparator)
                    {
                        if (tempWord is not " " and not "\n")
                        {
                            words.Add(tempWord);
                        }
                        tempWord = default;
                        wasSeparator = false;
                    }
                    
                    tempWord += strC;
                }
            }

            List<string> factoStr = new();


        }
    }
}
