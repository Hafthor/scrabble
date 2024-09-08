namespace Scrabble;

public static class Program {
    public static void Main(string[] args) {
        int players = args.Length > 0 ? int.Parse(args[0]) : 2;
        int seed = args.Length > 1 ? int.Parse(args[1]) : new Random().Next();
        Random random = new(seed);
        Game game = new(players, random);
        WordList wordList = new();
        int passes = 0;
        while (passes < players) {
            game.Board.Print();
            var curPlayer = game.Players[game.Turn];
            int currentPlayerTurn = game.Turn, currentScore = curPlayer.Score;
            Console.Write($"Player {currentPlayerTurn + 1} ({currentScore}) tiles: ");
            Console.WriteLine(string.Join(" ", curPlayer.Tiles));
            string cmd;
            for (;;) {
                Console.Write("Enter command or help: ");
                cmd = Console.ReadLine();
                if (cmd is "" or "plays") {
                    if (curPlayer.Tiles.Count(t => t.Letter == ' ') > 1)
                        Console.WriteLine("Warning: this might be slow with multiple blank tiles");
                    var plays = game.Board.GetPossiblePlays(wordList.WordsByLen, wordList.LetterCountsForWordsByLen,
                            curPlayer.Tiles)
                        .OrderBy(p => p.score + p.extras.Sum(e => e.score)).ThenBy(p => p.word);
                    foreach (var (y, x, h, w, score, extras, underlinePositions) in plays) {
                        string uw = Underline(w, underlinePositions);
                        Console.Write($"{y + 1},{x + 1},{(h ? 'H' : 'V')},{uw}={score}");
                        foreach (var (ww, ss) in extras)
                            Console.Write($", {ww}={ss}");
                        Console.WriteLine(extras.Count > 0 ? $", total={score + extras.Sum(p => p.score)}" : "");
                    }
                    continue;
                } else if (cmd == "seed") {
                    Console.WriteLine($"Seed={seed}");
                    continue;
                } else if (cmd == "pass") {
                    if (++passes < players) game.NextPlayer();
                    break;
                } else if (cmd is "help" or "?") {
                    Console.WriteLine("Commands: pass | rack | plays | seed | exit");
                    Console.WriteLine("or enter row,col,h/v,word to play a word");
                    continue;
                }
                passes = 0;
                if (cmd == "rack") {
                    if (game.Bag.Count == 0) {
                        Console.WriteLine("Bag is empty - did you want to pass?");
                        continue;
                    }
                    curPlayer.Rack(game.Bag, random);
                    game.NextPlayer();
                    break;
                }
                if (cmd is null or "exit" or "quit") break;
                string[] cmdParts = cmd.Split(',');
                if (cmdParts.Length == 1 && cmd.Length > 1 && cmd.Length < wordList.WordsByLen.Length) {
                    string w = cmd.ToUpper();
                    // try to play word - only works if there is a single position/direction that works
                    if (curPlayer.Tiles.Count(t => t.Letter == ' ') > 1)
                        Console.WriteLine("Warning: this might be slow with multiple blank tiles");
                    var plays = game.Board.GetPossiblePlays(wordList.WordsByLen, wordList.LetterCountsForWordsByLen,
                        curPlayer.Tiles).Where(p => p.word == w).ToList();
                    if (plays.Count < 1) {
                        Console.WriteLine("No plays found");
                        continue;
                    } else if (plays.Count > 1) {
                        Console.WriteLine("Must specify row,col,h/v");
                        foreach (var p in plays.OrderBy(p => p.score + p.extras.Sum(e => e.score)))
                            Console.WriteLine(
                                $"  {p.y + 1},{p.x + 1},{(p.horizontal ? 'H' : 'V')},{w} = {p.score + p.extras.Sum(e => e.score)}");
                        continue;
                    }
                    cmdParts = [plays[0].y + 1 + "", plays[0].x + 1 + "", plays[0].horizontal ? "H" : "V", w];
                }
                if (cmdParts.Length != 4) {
                    Console.WriteLine("invalid command");
                    continue;
                }
                if (!int.TryParse(cmdParts[0], out int row) || row < 1 || row > Board.BoardHeight) {
                    Console.WriteLine("invalid row");
                    continue;
                }
                row--;
                if (!int.TryParse(cmdParts[1], out int col) || col < 1 || col > Board.BoardWidth) {
                    Console.WriteLine("invalid col");
                    continue;
                }
                col--;
                if (cmdParts[2].ToUpper() is not "H" and not "V") {
                    Console.WriteLine("invalid direction");
                    continue;
                }
                bool horizontal = cmdParts[2].ToUpper() == "H";
                string word = cmdParts[3].ToUpper();
                if (!wordList.WordsByLen[word.Length].Contains(word)) {
                    Console.WriteLine("Invalid word");
                    continue;
                }
                string error = game.Play(word, row, col, horizontal, wordList.WordsByLen);
                if (error == null) {
                    int newScore = curPlayer.Score;
                    Console.WriteLine(
                        $"Player {currentPlayerTurn + 1} adds {newScore - currentScore} to score, new score = {newScore}");
                    break;
                }
                Console.WriteLine(error);
            }
            if (cmd is "exit" or "quit") break;
        }
        game.Board.Print();
        Console.WriteLine("Game over");
        int maxScore = game.Players.Max(p => p.Score);
        for (int i = 0; i < players; i++) {
            var p = game.Players[i];
            Console.WriteLine($"Player {i + 1}: {p.Score}" + (p.Score == maxScore ? " (winner)" : ""));
        }
    }

    private static string Underline(string word, int positions) {
        for (int i = word.Length; --i >= 0;)
            if ((positions & (1 << i)) != 0)
                word = word.Insert(i + 1, Ansi.Reset).Insert(i, Ansi.Underline);
        return word;
    }
}