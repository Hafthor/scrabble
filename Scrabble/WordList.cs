namespace ScrabbleHelper;

public class WordList {
    // public readonly string[] Words = File.ReadAllLines("../../../words.txt");
    // public readonly string[] Dorsw;
    // public WordList() => Dorsw = Words.Select(w => string.Concat(w.Order())).ToArray();

    public readonly string[] Words, Dorsw;
    public readonly string[][] WordsByLen, DorwsByLen;
    public readonly byte[][][] LetterCountsForWordsByLen;

    public WordList() {
        var words = File.ReadAllLines("../../../words.txt");
        Words = words;
        Dorsw = words.Select(w => string.Concat(w.Order())).ToArray();
        
        int maxLen = words.Max(w => w.Length);
        WordsByLen = new string[maxLen + 1][];
        DorwsByLen = new string[maxLen + 1][];
        LetterCountsForWordsByLen = new byte[maxLen + 1][][];
        for (int len = 1; len <= maxLen; len++) {
            WordsByLen[len] = words.Where(w => w.Length == len).ToArray();
            DorwsByLen[len] = WordsByLen[len].Select(w => string.Concat(w.Order())).ToArray();
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