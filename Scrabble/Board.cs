namespace Scrabble;

public class Board {
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

    private static readonly int BoardHeight = Bonuses.Length, BoardWidth = Bonuses[0].Length;

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
            Console.Write($" {(c + 1):00}");
        Console.WriteLine();
        // Prints the board
        for (int r = 0; r < BoardHeight; r++) {
            // Row numbers
            Console.Write($"{(r + 1):00} ");
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
        int blankPositions)> GetPossiblePlays(string[][] wordsByLen, byte[][][] letterCountsByWordsByLen,
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
            for (int h = 0; h < 2; h++) {
                bool horizontal = h != 0;
                int xl = horizontal ? BoardWidth - len + 1 : BoardWidth,
                    yl = horizontal ? BoardHeight : BoardHeight - len + 1;
                for (int y = 0; y < yl; y++) {
                    for (int x = 0; x < xl; x++) {
                        // ensure it is connected and is not extending an existing word
                        if (horizontal) {
                            if (x > 0 && Tiles[y, x - 1] != null || x < xl - 1 && Tiles[y, x + len] != null) continue;
                        } else {
                            if (y > 0 && Tiles[y - 1, x] != null || y < yl - 1 && Tiles[y + len, x] != null) continue;
                        }

                        // add the tiles on the board and check to see if we are connected
                        Array.Copy(letterCountsFromTiles, letterCounts, 26);
                        bool connected = false;
                        for (int i = 0; i < len; i++) {
                            Tile b = horizontal ? Tiles[y, x + i] : Tiles[y + i, x];
                            if (b != null) {
                                letterCounts[b.Letter - 'A']++;
                                connected = true;
                            }
                            connected |= IsCenter(horizontal ? Bonuses[y][x + i] : Bonuses[y + i][x]);
                        }
                        if (!connected) continue;

                        // check all words of the given length
                        for (int wi = 0; wi < words.Length; wi++) {
                            var wlc = letterCountsForWords[wi];

                            // do we have enough letters (including blanks)
                            bool isValid = true;
                            int remainingBlanks = blankCount;
                            string blankLetters = "";
                            for (int i = 0; i < 26; i++)
                                if (letterCounts[i] < wlc[i]) {
                                    if (--remainingBlanks < 0) {
                                        isValid = false;
                                        break;
                                    }
                                    blankLetters += (char)('A' + i);
                                }
                            if (!isValid) continue;

                            // compute where the blanks would be used
                            string word = words[wi];
                            int blankPositions = 0;
                            if (blankLetters.Length > 0) {
                                for (int i = 0, m = 1; i < word.Length; i++, m <<= 1) {
                                    int j = blankLetters.IndexOf(word[i]);
                                    if (j >= 0) {
                                        blankPositions |= m;
                                        blankLetters = blankLetters.Remove(j, 1);
                                    }
                                }
                            }

                            // compute the score - might be rejected if perpendicular words are not valid
                            (int score, _, List<(string word, int score)> extras) =
                                GetScore(y, x, horizontal, word, tiles, wordsByLen);

                            // return the result
                            if (score + extras?.Sum(e => e.score) > 0)
                                yield return (y, x, horizontal, word, score, extras, blankPositions);
                        }
                    }
                }
            }
        }
    }

    public (int score, int usedTiles, List<(string word, int score)> extras) GetScore(int y, int x,
        bool horizontal, string word, List<Tile> rack, string[][] wordListByLen) {
        bool connected = false;
        int score = 0, wordMultiplier = 1, tileUsed = 0;
        List<(string word, int score)> extras = new();

        // check if the word can be placed and score it
        foreach (var c in word) {
            char b = Bonuses[y][x];
            wordMultiplier *= WordMultiplier(b);
            Tile t = Tiles[y, x];
            if (t != null && t.Letter != c) return (0, 0, null);
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
            if (t == null) return (0, 0, null);
            score += t.Points * LetterMultiplier(b);
            if (!hadTileAlready) {
                // if we are placing a tile on the board, we need to check if it is connected to other tiles
                // and that they form valid words as well as score the word we are forming
                int subScore = 0, subWordMultiplier = 1;
                char[] subWord = null;
                if (horizontal) {
                    int ys = y, ye = y;
                    for (; ys > 0 && Tiles[ys - 1, x] != null;) ys--; // look up
                    for (; ye < BoardHeight - 1 && Tiles[ye + 1, x] != null;) ye++; // look down
                    if (ys < y || ye > y) { // if we are connecting perpendicularly
                        // read the word and score it
                        subWord = new char[ye - ys + 1];
                        for (int yy = ys; yy <= ye; yy++) {
                            Tile tt = yy == y ? t : Tiles[yy, x];
                            char bb = Bonuses[yy][x];
                            subScore += tt.Points * LetterMultiplier(bb);
                            subWordMultiplier *= WordMultiplier(bb);
                            subWord[yy - ys] = tt.Letter;
                        }
                    }
                } else {
                    int xs = x, xe = x;
                    for (; xs > 0 && Tiles[y, xs - 1] != null;) xs--; // look left
                    for (; xe < BoardWidth - 1 && Tiles[y, xe + 1] != null;) xe++; // look right
                    if (xs < x || xe > x) { // if we are connecting perpendicularly
                        // read the word and score it
                        subWord = new char[xe - xs + 1];
                        for (int xx = xs; xx <= xe; xx++) {
                            Tile tt = xx == x ? t : Tiles[y, xx];
                            char bb = Bonuses[y][xx];
                            subScore += tt.Points * LetterMultiplier(bb);
                            subWordMultiplier *= WordMultiplier(bb);
                            subWord[xx - xs] = tt.Letter;
                        }
                    }
                }
                if (subWord != null) {
                    // check if the perpendicular word is valid
                    string sw = new(subWord);
                    if (!wordListByLen[sw.Length].Contains(sw)) return (0, 0, null);
                    // record the perpendicular word and its score
                    extras.Add((sw, subScore * subWordMultiplier));
                }
            }
            if (horizontal) x++;
            else y++;
        }
        if (!connected || tileUsed == 0) return (0, 0, null);
        // bonus for using all tiles
        if (tileUsed == (1 << rack.Count) - 1)
            extras.Add(("all tiles used", 50));
        return (score * wordMultiplier, tileUsed, extras);
    }
}