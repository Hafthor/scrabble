namespace Scrabble;

public class WordList {
    public readonly string[][] WordsByLen;
    public readonly byte[][][] LetterCountsForWordsByLen;

    public WordList() {
        var words = File.ReadAllLines("../../../words.txt");

        int maxLen = words.Max(w => w.Length);
        WordsByLen = new string[maxLen + 1][];
        LetterCountsForWordsByLen = new byte[maxLen + 1][][];
        for (int len = 1; len <= maxLen; len++) {
            WordsByLen[len] = words.Where(w => w.Length == len).ToArray();
            var letterCountsForWords = LetterCountsForWordsByLen[len] = new byte[words.Length][];
            int i = 0;
            foreach (var w in WordsByLen[len]) {
                byte[] letterCounts = letterCountsForWords[i++] = new byte[26];
                foreach (var c in w)
                    letterCounts[c - 'A']++;
            }
        }
    }
}