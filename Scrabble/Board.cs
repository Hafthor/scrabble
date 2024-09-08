namespace Scrabble;

public class Board {
    private static readonly (int dy, int dx)[] Directions = [(0, 1), (1, 0)];

    // * = center, 2 = double word, 3 = triple word, d = double letter, t = triple letter
    private static readonly string[] Bonuses = """
                                               3..d...3...d..3
                                               .2...t...t...2.
                                               ..2...d.d...2..
                                               d..2...d...2..d
                                               ....2.....2....
                                               .t...t...t...t.
                                               ..d...d.d...d..
                                               3..d...*...d..3
                                               ..d...d.d...d..
                                               .t...t...t...t.
                                               ....2.....2....
                                               d..2...d...2..d
                                               ..2...d.d...2..
                                               .2...t...t...2.
                                               3..d...3...d..3
                                               """.Split('\n'); // *=d

    public static readonly int BoardHeight = Bonuses.Length, BoardWidth = Bonuses[0].Length;

    private static int WordMultiplier(char c) => c switch {
        '*' or '2' => 2,
        '3' => 3,
        _ => 1
    };

    private static int LetterMultiplier(char c) => c switch {
        't' => 3,
        'd' => 2,
        _ => 1
    };

    private static bool IsCenter(char c) => c == '*';

    public readonly Tile[,] Tiles = new Tile[BoardHeight, BoardWidth];

    public void Print() {
        // Prints color legend
        Console.WriteLine(Colors.DoubleLetterScore + "  " + Ansi.Reset + "=2xLetter, " +
                          Colors.TripleLetterScore + "  " + Ansi.Reset + "=3xLetter, " +
                          Colors.DoubleWordScore + "  " + Ansi.Reset + "=2xWord, " +
                          Colors.TripleWordScore + "  " + Ansi.Reset + "=3xWord");
        // Prints the column numbers
        Console.Write("  ");
        for (int c = 0; c < BoardWidth; c++)
            Console.Write($" {c + 1:00}");
        Console.WriteLine();
        // Prints the board
        for (int r = 0; r < BoardHeight; r++) {
            // Row numbers
            Console.Write($"{r + 1:00} ");
            for (int c = 0; c < BoardWidth; c++) {
                Tile t = Tiles[r, c];
                char b = Bonuses[r][c];
                // Colors the tile based on the bonus
                Console.Write(b switch {
                    '*' or '2' => Colors.DoubleWordScore,
                    '3' => Colors.TripleWordScore,
                    'd' => Colors.DoubleLetterScore,
                    't' => Colors.TripleLetterScore,
                    _ => Ansi.BrownBg
                });
                if (t == null) {
                    // if (b == '*') Console.Write(Ansi.Red + "><");
                    // else if (b == '2') Console.Write(Ansi.Red + "dw");
                    // else if (b == '3') Console.Write(Ansi.Pink + "tw");
                    // else if (b == 'd') Console.Write(Ansi.BGreen + "dl");
                    // else if (b == 't') Console.Write(Ansi.BBlue + "tl");
                    // else Console.Write("  ");
                    Console.Write("  ");
                } else {
                    Console.Write(t.OrgLetter == ' ' ? Colors.BlankTile : Colors.RegularTile);
                    Console.Write($"{t}");
                }
                Console.Write(Ansi.Reset);
                Console.Write(' ');
            }
            Console.WriteLine();
        }
    }

    public IEnumerable<(int y, int x, bool horizontal, string word, int score, List<(string word, int score)> extras,
        int blankPositions)> PossiblePlays(string[][] wordsByLen, byte[][][] letterCountsByWordsByLen,
        List<Tile> tiles) {
        // Count the number of blank tiles and the number of each tile in the rack
        int blankCount = 0;
        byte[] letterCountsFromTiles = new byte[26];
        foreach (var t in tiles) {
            char c = t.Letter;
            if (c != ' ')
                letterCountsFromTiles[c - 'A']++;
            else
                blankCount++;
        }

        // Iterate over all possible words of each length
        for (int len = 1; len < wordsByLen.Length; len++) {
            string[] words = wordsByLen[len];
            byte[][] letterCountsForWords = letterCountsByWordsByLen[len];
            byte[] letterCounts = new byte[26];
            foreach (var dir in Directions) {
                int xl = BoardWidth - (len - 1) * dir.dx, yl = BoardHeight - (len - 1) * dir.dy;
                for (int y = 0; y < yl; y++) {
                    for (int x = 0; x < xl; x++) {
                        // ensure is not extending an existing word
                        (int y, int x) before = (y - dir.dy, x - dir.dx);
                        if (before is { y: >= 0, x: >= 0 } && Tiles[before.y, before.x] != null) continue;
                        (int y, int x) after = (y + len * dir.dy, x + len * dir.dx);
                        if (after.y < BoardHeight && after.x < BoardWidth && Tiles[after.y, after.x] != null) continue;

                        // count the tiles on the board and check to see if we are connected
                        Array.Copy(letterCountsFromTiles, letterCounts, 26);
                        bool connected = false;
                        for (int i = 0, tx = x, ty = y; i < len; i++, tx += dir.dx, ty += dir.dy) {
                            Tile b = Tiles[ty, tx];
                            if (b != null) {
                                letterCounts[b.Letter - 'A']++;
                                connected = true;
                            }
                            connected |= IsCenter(Bonuses[ty][tx]);
                        }
                        if (!connected) continue;

                        // check all words of the given length
                        for (int wordIndex = 0; wordIndex < words.Length; wordIndex++) {
                            var wlc = letterCountsForWords[wordIndex];

                            // do we have enough letters (including blanks)
                            bool isValid = true;
                            int remainingBlanks = blankCount;
                            string blankLetters = "";
                            for (int letter = 0; letter < 26; letter++)
                                if (letterCounts[letter] < wlc[letter]) {
                                    if (--remainingBlanks < 0) {
                                        isValid = false;
                                        break;
                                    }
                                    blankLetters += (char)('A' + letter);
                                }
                            if (!isValid) continue;

                            // compute where the blanks would be used
                            string word = words[wordIndex];
                            int blankPositions = 0;
                            if (blankLetters.Length > 0)
                                for (int charIndex = 0, charIndexMask = 1;
                                     charIndex < word.Length;
                                     charIndex++, charIndexMask <<= 1) {
                                    int blankIndex = blankLetters.IndexOf(word[charIndex]);
                                    if (blankIndex >= 0) {
                                        blankPositions |= charIndexMask;
                                        blankLetters = blankLetters.Remove(blankIndex, 1);
                                    }
                                }

                            // compute the score - might be rejected if perpendicular words are not valid
                            (string error, int score, _, List<(string word, int score)> extras) =
                                Score(y, x, dir.dx > 0, word, tiles, wordsByLen);
                            if (error != null) continue;

                            // return the result
                            yield return (y, x, dir.dx > 0, word, score, extras, blankPositions);
                        }
                    }
                }
            }
        }
    }

    public (string error, int score, int usedTiles, List<(string word, int score)> extras) Score(int y, int x,
        bool horizontal, string word, List<Tile> rack, string[][] wordListByLen) {
        bool connected = false;
        int score = 0, wordMultiplier = 1, tileUsed = 0;
        List<(string word, int score)> extras = new();

        // check if the word can be placed and score it
        foreach (var c in word) {
            char b = Bonuses[y][x];
            wordMultiplier *= WordMultiplier(b);
            Tile t = Tiles[y, x];
            if (t != null && t.Letter != c) return ($"cannot change tile at {y + 1},{x + 1}", 0, 0, null);
            connected |= t != null || IsCenter(Bonuses[y][x]);
            bool hadTileAlready = t != null;
            if (t == null)
                for (int j = 0, m = 1; j < rack.Count; j++, m <<= 1)
                    if ((tileUsed & m) == 0 && rack[j].Letter == c) {
                        t = rack[j];
                        tileUsed |= m;
                        break;
                    }
            if (t == null)
                for (int j = 0, m = 1; j < rack.Count; j++, m <<= 1)
                    if ((tileUsed & m) == 0 && rack[j].Letter == ' ') {
                        t = rack[j];
                        tileUsed |= m;
                        break;
                    }
            if (t == null) return ($"missing tile for {y + 1},{x + 1}", 0, 0, null);
            score += t.Points * LetterMultiplier(b);
            if (!hadTileAlready) {
                // if we are placing a tile on the board, we need to check if it is connected to other tiles
                // and that they form valid words as well as score the word we are forming
                int subScore = 0, subWordMultiplier = 1;
                int subWordStart = horizontal ? x : y, subWordEnd = subWordStart, subWordOrg = subWordStart;
                if (horizontal) {
                    for (; subWordStart > 0 && Tiles[subWordStart - 1, x] != null;) subWordStart--; // look up
                    for (; subWordEnd < BoardHeight - 1 && Tiles[subWordEnd + 1, x] != null;) subWordEnd++; // look down
                } else {
                    for (; subWordStart > 0 && Tiles[y, subWordStart - 1] != null;) subWordStart--; // look left
                    for (; subWordEnd < BoardWidth - 1 && Tiles[y, subWordEnd + 1] != null;) subWordEnd++; // look right
                }
                if (subWordStart < subWordEnd) { // if we are connecting perpendicularly
                    // read the word and score it
                    var subWord = new char[subWordEnd - subWordStart + 1];
                    for (int wi = subWordStart; wi <= subWordEnd; wi++) {
                        Tile tt = wi == subWordOrg ? t : horizontal ? Tiles[wi, x] : Tiles[y, wi];
                        char bb = Bonuses[horizontal ? wi : y][horizontal ? x : wi];
                        subScore += tt.Points * LetterMultiplier(bb);
                        subWordMultiplier *= WordMultiplier(bb);
                        subWord[wi - subWordStart] = tt.Letter;
                    }
                    // check if the perpendicular word is valid
                    string sw = new(subWord);
                    if (!wordListByLen[sw.Length].Contains(sw))
                        return ($"invalid perpendicular word {sw} at {y + 1},{x + 1}", 0, 0, null);
                    // record the perpendicular word and its score
                    extras.Add((sw, subScore * subWordMultiplier));
                }
            }
            if (horizontal) x++;
            else y++;
        }
        if (!connected) return ($"word is not connected", 0, 0, null);
        if (tileUsed == 0) return ($"no tiles used", 0, 0, null);
        // bonus for using all tiles
        if (tileUsed == (1 << rack.Count) - 1)
            extras.Add(("all tiles used", 50));
        return (null, score * wordMultiplier, tileUsed, extras);
    }
}