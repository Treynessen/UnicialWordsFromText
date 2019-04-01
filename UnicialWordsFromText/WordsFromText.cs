using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Trane.Text.Parsing
{
    public class WordsFromText
    {
        private int hashCellSize;
        private LinkedList<string>[] hashTable;
        private string availableSymbols;
        private Func<string, string> getParentWord;

        public IEnumerable<string> Words
        {
            get
            {
                return (from cell in hashTable
                        where cell != null && cell.Count > 0
                        from word in cell
                        select word).ToList();
            }
        }

        public WordsFromText(string path, int hashCellSize, string availableSymbols, Func<string, string> getParentWord = null)
        {
            if (hashCellSize == 0) throw new ArgumentException();
            if (string.IsNullOrEmpty(path)) throw new ArgumentException();
            if (!File.Exists(path)) throw new FileNotFoundException();

            this.hashCellSize = hashCellSize;
            hashTable = new LinkedList<string>[GetHTSize(path)];
            this.availableSymbols = availableSymbols;
            this.getParentWord = getParentWord;
            ParseText(path);
        }

        private void ParseText(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                StringBuilder builder = new StringBuilder();
                while (!reader.EndOfStream)
                {
                    char symbol = char.ToLower((char)reader.Read());
                    if (!availableSymbols.Contains(symbol))
                    {
                        if (builder.Length > 0)
                            SetWord(builder.ToString());
                        builder.Clear();
                    }
                    else builder.Append(symbol);
                }
                if (builder.Length > 0)
                    SetWord(builder.ToString());
            }
        }

        private void SetWord(string word)
        {
            if (getParentWord != null)
                word = getParentWord(word);
            int hashCellNumber = GetHashCellNumber(word);
            if (hashTable[hashCellNumber] == null)
            {
                hashTable[hashCellNumber] = new LinkedList<string>();
                hashTable[hashCellNumber].AddLast(word);
            }
            else
            {
                bool has = false;
                foreach (var w in hashTable[hashCellNumber])
                    if (w == word)
                        has = true;
                if (!has)
                    hashTable[hashCellNumber].AddLast(word);
            }
        }

        private int GetHashCellNumber(string word)
        {
            long hashCode = word.GetHashCode();
            if (hashCode < 0)
                hashCode = Math.Abs(hashCode) * 2;
            return (int)(hashCode % hashTable.Length);
        }

        private int GetHTSize(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            long size = fileInfo.Length;
            // size делим на средний размер слова (10 * sizeof(char))
            // полученное значение делим на количество слов в 1 ячейке хэш-таблицы
            size /= hashCellSize * 10 * sizeof(char);
            size = size == 0 ? 1 : size;
            return size > int.MaxValue ? int.MaxValue : (int)size;
        }
    }
}