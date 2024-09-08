using System.Diagnostics;

namespace Scrabble;

public class Board {
    public static readonly string[] Bonuses = """
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
                                              """.Split('\n'); // *=D

    public static readonly int BoardHeight = Bonuses.Length, BoardWidth = Bonuses[0].Length;

    public static int WordMultiplier(int row, int col) => WordMultiplier(Bonuses[row][col]);

    public static int WordMultiplier(char c) => c is '*' or '2' ? 2 : c is '3' ? 3 : 1;

    public static int LetterMultiplier(int row, int col) => LetterMultiplier(Bonuses[row][col]);

    public static int LetterMultiplier(char c) => c is 't' ? 3 : c is 'd' ? 2 : 1;

    public static bool IsCenter(char c) => c == '*';
    public static bool IsCenter(int row, int col) => IsCenter(Bonuses[row][col]);

    public readonly Tile[,] board = new Tile[BoardHeight, BoardWidth];

    public void Print() {
        Console.WriteLine(Colors.DoubleLetterScore + "  " + Ansi.Reset + "=2xLetter, " +
                          Colors.TripleLetterScore + "  " + Ansi.Reset + "=3xLetter, " +
                          Colors.DoubleWordScore + "  " + Ansi.Reset + "=2xWord, " +
                          Colors.TripleWordScore + "  " + Ansi.Reset + "=3xWord");
        Console.Write("  ");
        for (int c = 0; c < BoardWidth; c++)
            Console.Write($" {(c + 1):00}");
        Console.WriteLine();
        for (int r = 0; r < BoardHeight; r++) {
            Console.Write($"{(r + 1):00} ");
            for (int c = 0; c < BoardWidth; c++) {
                Tile t = board[r, c];
                char b = Bonuses[r][c];
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
                    Console.Write(t.orgLetter == ' ' ? Colors.BlankTile : Colors.RegularTile);
                    Console.Write($"{t}");
                }
                Console.Write(Ansi.Reset);
                Console.Write(' ');
            }
            Console.WriteLine();
        }
    }

    public IEnumerable<(int y, int x, bool horizontal, string word, int score, List<(string word, int score)> extras)>
        GetPossiblePlays(string[][] wordsByLen, byte[][][] letterCountsByWordsByLen, List<Tile> tiles) {
        int blankCount = 0;
        byte[] letterCountsFromTiles = new byte[26];
        foreach (var t in tiles) {
            char c = t.Letter;
            if (c != ' ')
                letterCountsFromTiles[c - 'A']++;
            else
                blankCount++;
        }

        int centerX = BoardWidth / 2, centerY = BoardHeight / 2;
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
                        bool connected;
                        if (horizontal) {
                            if (x > 0 && board[y, x - 1] != null) continue;
                            if (x < xl - 1 && board[y, x + len] != null) continue;
                            connected = y == centerY && x <= centerX && x + len > centerX;
                        } else {
                            if (y > 0 && board[y - 1, x] != null) continue;
                            if (y < yl - 1 && board[y + len, x] != null) continue;
                            connected = x == centerX && y <= centerY && y + len > centerY;
                        }
                        Array.Copy(letterCountsFromTiles, letterCounts, 26);
                        for (int i = 0; i < len; i++) {
                            Tile b = horizontal ? board[y, x + i] : board[y + i, x];
                            if (b != null) {
                                letterCounts[b.Letter - 'A']++;
                                connected = true;
                            }
                        }
                        if (!connected) continue;
                        for (int wi = 0; wi < words.Length; wi++) {
                            var wlc = letterCountsForWords[wi];
                            bool isValid = true;
                            int remainingBlanks = blankCount;
                            for (int i = 0; i < 26; i++) {
                                int diff = letterCounts[i] - wlc[i];
                                if (diff < 0 && --remainingBlanks < 0) {
                                    isValid = false;
                                    break;
                                }
                            }
                            if (isValid) {
                                string word = words[wi];
                                (int score, int usedTiles, List<(string word, int score)> extras) =
                                    GetScore(y, x, horizontal, word, tiles, wordsByLen);
                                if (score > 0) yield return (y, x, horizontal, word, score, extras);
                            }
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
        // check if the word can be placed
        for (int i = 0; i < word.Length; i++) {
            char c = word[i], b = Bonuses[y][x];
            wordMultiplier *= WordMultiplier(b);
            Tile t = board[y, x];
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
            score += t.points * LetterMultiplier(b);
            if (!hadTileAlready) {
                // if we are placing a tile on the board, we need to check if it is connected to other tiles
                // and that they form valid words as well as score the word we are forming
                int subScore = 0, subWordMultiplier = 1;
                char[] subWord = null;
                if (horizontal) {
                    int ys = y, ye = y;
                    for (; ys > 0 && board[ys - 1, x] != null; ys--) ; // look up
                    for (; ye < BoardHeight - 1 && board[ye + 1, x] != null; ye++) ; // look down
                    if (ys < y || ye > y) {
                        subWord = new char[ye - ys + 1];
                        for (int yy = ys; yy <= ye; yy++) {
                            Tile tt = yy == y ? t : board[yy, x];
                            char bb = Bonuses[yy][x];
                            subScore += tt.points * LetterMultiplier(bb);
                            subWordMultiplier *= WordMultiplier(bb);
                            subWord[yy - ys] = tt.Letter;
                        }
                    }
                } else {
                    int xs = x, xe = x;
                    for (; xs > 0 && board[y, xs - 1] != null; xs--) ; // look left
                    for (; xe < BoardWidth - 1 && board[y, xe + 1] != null; xe++) ; // look right
                    if (xs < x || xe > x) {
                        subWord = new char[xe - xs + 1];
                        for (int xx = xs; xx <= xe; xx++) {
                            Tile tt = xx == x ? t : board[y, xx];
                            char bb = Bonuses[y][xx];
                            subScore += tt.points * LetterMultiplier(bb);
                            subWordMultiplier *= WordMultiplier(bb);
                            subWord[xx - xs] = tt.Letter;
                        }
                    }
                }
                if (subWord != null) {
                    string s = new(subWord);
                    if (!wordListByLen[s.Length].Contains(s)) return (0, 0, null);
                    subScore *= subWordMultiplier;
                    extras.Add((s, subScore));
                }
            }
            if (horizontal) x++;
            else y++;
        }
        if (!connected || tileUsed == 0) return (0, 0, null);
        if (tileUsed == (1 << rack.Count) - 1)
            extras.Add(("all tiles used", 50));
        return (score * wordMultiplier + extras.Sum(p => p.score), tileUsed, extras);
    }
}